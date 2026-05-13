using LCH.Abp.Video.Events;
using LCH.Abp.Video.Storage;
using LCH.Abp.Video.Videos;
using LCH.MicroService.VideoService.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus;
using Volo.Abp.Threading;

namespace LCH.MicroService.VideoService.BackgroundWorkers;

public class UploadSessionCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
    protected IAbpDistributedLock DistributedLock { get; }
    protected IOptionsMonitor<VideoSettings> VideoSettings { get; }

    public UploadSessionCleanupWorker(
        AbpAsyncTimer timer,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<VideoSettings> videoSettings,
        IAbpDistributedLock distributedLock)
        : base(timer, serviceScopeFactory)
    {
        DistributedLock = distributedLock;
        VideoSettings = videoSettings;
        timer.Period = videoSettings.CurrentValue.SessionCleanupPeriod;
    }

    protected async override Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        await using var handle = await DistributedLock.TryAcquireAsync(nameof(UploadSessionCleanupWorker));

        if (handle == null)
        {
            Logger.LogInformation("Lock not acquired for {WorkerName}", nameof(UploadSessionCleanupWorker));
            return;
        }

        Logger.LogInformation("Lock acquired for {WorkerName}", nameof(UploadSessionCleanupWorker));

        try
        {
            var uploadSessionRepository = workerContext.ServiceProvider.GetRequiredService<IUploadSessionRepository>();
            var videoStorageService = workerContext.ServiceProvider.GetRequiredService<IVideoStorageService>();
            var eventBus = workerContext.ServiceProvider.GetRequiredService<IEventBus>();
            var settings = VideoSettings.CurrentValue;

            var expirationThreshold = DateTime.UtcNow.AddHours(-settings.SessionExpirationHours);

            var expiredSessions = await uploadSessionRepository.GetByStatusAsync(UploadSessionStatus.Expired);

            foreach (var session in expiredSessions)
            {
                await CleanupSessionAsync(session, uploadSessionRepository, videoStorageService, eventBus);
            }

            var activeSessions = await uploadSessionRepository.GetListAsync(
                status: UploadSessionStatus.InProgress);

            foreach (var session in activeSessions)
            {
                if (session.ExpiresAt < DateTime.UtcNow)
                {
                    await CleanupSessionAsync(session, uploadSessionRepository, videoStorageService, eventBus);
                }
            }

            var tempFilesCleaned = await videoStorageService.CleanupTempFilesAsync(expirationThreshold);
            Logger.LogInformation("Cleaned {Count} temp files older than {Threshold}", tempFilesCleaned, expirationThreshold);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cleaning up expired upload sessions");
        }
        finally
        {
            Logger.LogInformation("Lock released for {WorkerName}", nameof(UploadSessionCleanupWorker));
        }
    }

    private async Task CleanupSessionAsync(
        UploadSession session,
        IUploadSessionRepository uploadSessionRepository,
        IVideoStorageService videoStorageService,
        IEventBus eventBus)
    {
        Logger.LogInformation(
            "Cleaning up expired upload session {SessionId} for file {FileName}",
            session.Id, session.FileName);

        try
        {
            await videoStorageService.CancelUploadAsync(session.Id);

            await uploadSessionRepository.DeleteAsync(session.Id);

            await eventBus.PublishAsync(new UploadSessionExpiredEvent(
                session.Id,
                session.UserId,
                session.FileName));

            Logger.LogInformation(
                "Successfully cleaned up upload session {SessionId}",
                session.Id);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error cleaning up upload session {SessionId}",
                session.Id);
        }
    }
}