using LCH.Abp.Video.Events;
using LCH.Abp.Video.Transcoding;
using LCH.Abp.Video.Videos;
using LCH.MicroService.VideoService.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus;
using Volo.Abp.Threading;

namespace LCH.MicroService.VideoService.BackgroundWorkers;

public class TranscodingWorker : AsyncPeriodicBackgroundWorkerBase
{
    protected IAbpDistributedLock DistributedLock { get; }
    protected IOptionsMonitor<VideoSettings> VideoSettings { get; }

    public TranscodingWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<VideoSettings> videoSettings,
        IAbpDistributedLock distributedLock)
        : base(timer, serviceScopeFactory)
    {
        DistributedLock = distributedLock;
        VideoSettings = videoSettings;
        timer.Period = videoSettings.CurrentValue.TranscodingCheckPeriod;
    }

    protected async override Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await using var handle = await DistributedLock.TryAcquireAsync(nameof(TranscodingWorker));

        if (handle == null)
        {
            Logger.LogInformation("Lock not acquired for {WorkerName}", nameof(TranscodingWorker));
            return;
        }

        Logger.LogInformation("Lock acquired for {WorkerName}", nameof(TranscodingWorker));

        try
        {
            var videoRepository = workerContext.ServiceProvider.GetRequiredService<IVideoRepository>();
            var transcodingService = workerContext.ServiceProvider.GetRequiredService<ITranscodingService>();
            var eventBus = workerContext.ServiceProvider.GetRequiredService<IEventBus>();
            var settings = VideoSettings.CurrentValue;

            var pendingVideos = await GetPendingTranscodingVideos(videoRepository);

            if (pendingVideos.IsEmpty())
            {
                Logger.LogInformation("No pending transcoding videos found");
                return;
            }

            var inProgressCount = await GetInProgressTranscodingCount(videoRepository);
            var availableSlots = settings.MaxConcurrentTranscodingTasks - inProgressCount;

            if (availableSlots <= 0)
            {
                Logger.LogInformation("Max concurrent transcoding tasks reached ({MaxTasks})", settings.MaxConcurrentTranscodingTasks);
                return;
            }

            var videosToProcess = pendingVideos.Take(availableSlots).ToList();

            foreach (var video in videosToProcess)
            {
                await ProcessTranscodingAsync(video, transcodingService, videoRepository, eventBus, settings);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing transcoding tasks");
        }
        finally
        {
            Logger.LogInformation("Lock released for {WorkerName}", nameof(TranscodingWorker));
        }
    }

    private async Task<List<Video>> GetPendingTranscodingVideos(IVideoRepository videoRepository)
    {
        var transcodingVideos = await videoRepository.GetByStatusAsync(VideoStatus.Transcoding, 0, 100);
        var pendingTasks = transcodingVideos
            .Where(v => v.TranscodingTasks.Any(t => t.Status == TranscodingStatus.Pending))
            .ToList();
        return pendingTasks;
    }

    private async Task<int> GetInProgressTranscodingCount(IVideoRepository videoRepository)
    {
        var transcodingVideos = await videoRepository.GetByStatusAsync(VideoStatus.Transcoding, 0, 100);
        return transcodingVideos
            .SelectMany(v => v.TranscodingTasks)
            .Count(t => t.Status == TranscodingStatus.InProgress);
    }

    private async Task ProcessTranscodingAsync(
        Video video,
        ITranscodingService transcodingService,
        IVideoRepository videoRepository,
        IEventBus eventBus,
        VideoSettings settings)
    {
        var pendingTask = video.TranscodingTasks.FirstOrDefault(t => t.Status == TranscodingStatus.Pending);

        if (pendingTask == null)
        {
            return;
        }

        Logger.LogInformation(
            "Starting transcoding for video {VideoId}, task {TaskId}, resolution {Resolution}",
            video.Id, pendingTask.Id, pendingTask.Resolution.DisplayName);

        try
        {
            pendingTask.Start();
            await videoRepository.UpdateAsync(video);

            var started = await transcodingService.StartTranscodingAsync(pendingTask.Id);

            if (!started)
            {
                pendingTask.MarkFailed("Failed to start transcoding task");
                await videoRepository.UpdateAsync(video);
                return;
            }

            var progress = await transcodingService.GetTranscodingProgressAsync(pendingTask.Id);
            pendingTask.UpdateProgress(progress.Progress, progress.Status);

            if (progress.Status == TranscodingStatus.Completed)
            {
                pendingTask.MarkCompleted(progress.ErrorMessage ?? string.Empty);
                await CheckVideoTranscodingCompletion(video, videoRepository, eventBus);
            }
            else if (progress.Status == TranscodingStatus.Failed)
            {
                pendingTask.MarkFailed(progress.ErrorMessage ?? "Transcoding failed");
                await videoRepository.UpdateAsync(video);
            }
            else
            {
                await videoRepository.UpdateAsync(video);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error transcoding video {VideoId}, task {TaskId}",
                video.Id, pendingTask.Id);

            pendingTask.MarkFailed(ex.Message);
            await videoRepository.UpdateAsync(video);
        }
    }

    private async Task CheckVideoTranscodingCompletion(
        Video video,
        IVideoRepository videoRepository,
        IEventBus eventBus)
    {
        var allCompleted = video.TranscodingTasks.All(t => t.IsCompleted || t.IsFailed);

        if (allCompleted)
        {
            var anySuccess = video.TranscodingTasks.Any(t => t.IsCompleted);

            if (anySuccess)
            {
                Logger.LogInformation("All transcoding tasks completed for video {VideoId}", video.Id);
                await eventBus.PublishAsync(new TranscodingCompletedEvent(video.Id));
            }
            else
            {
                Logger.LogWarning("All transcoding tasks failed for video {VideoId}", video.Id);
            }

            await videoRepository.UpdateAsync(video);
        }
    }
}