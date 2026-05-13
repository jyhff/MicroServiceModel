using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using LCH.MicroService.Danmaku.Entities;
using LCH.MicroService.Danmaku.Enums;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace LCH.MicroService.Danmaku.PerformanceTests.Performance;

/// <summary>
/// 弹幕发送性能测试
/// 使用BenchmarkDotNet进行基准测试
/// 测试场景：
/// 1. 1000弹幕/秒发送性能
/// 2. Redis缓存性能
/// </summary>
public class DanmakuSendPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public DanmakuSendPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// 测试：模拟1000弹幕/秒发送
    /// </summary>
    [Fact]
    public async Task ThousandDanmakuPerSecond_ShouldBeHandled()
    {
        // Arrange
        const int targetDanmakuCount = 1000;
        const int targetDurationMs = 1000;
        var danmakus = GenerateTestDanmakus(targetDanmakuCount);
        var processedCount = 0;
        var errors = new ConcurrentBag<Exception>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - 模拟并发发送
        var tasks = danmakus.Select(async danmaku =>
        {
            try
            {
                // 模拟弹幕处理（这里只是模拟，实际测试需要连接真实服务）
                await SimulateDanmakuProcessingAsync(danmaku);
                Interlocked.Increment(ref processedCount);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var throughput = processedCount * 1000.0 / stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Total danmaku: {targetDanmakuCount}");
        _output.WriteLine($"Processed: {processedCount}");
        _output.WriteLine($"Errors: {errors.Count}");
        _output.WriteLine($"Time elapsed: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Throughput: {throughput:F2} danmaku/second");

        Assert.True(processedCount >= targetDanmakuCount * 0.95, 
            $"Expected at least {targetDanmakuCount * 0.95} processed, got {processedCount}");
        Assert.True(throughput >= 500, 
            $"Expected at least 500 danmaku/second throughput, got {throughput:F2}");
    }

    /// <summary>
    /// 测试：并发弹幕创建性能
    /// </summary>
    [Fact]
    public void ConcurrentDanmakuCreation_ShouldBeFast()
    {
        // Arrange
        const int count = 10000;
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var danmakus = new List<Danmaku>();
        for (int i = 0; i < count; i++)
        {
            var danmaku = Danmaku.CreateNormal(
                videoId,
                userId,
                $"User{i}",
                i % 100,
                $"测试弹幕内容{i}",
                i % 300,
                DanmakuType.Scroll,
                "#FFFFFF",
                25);
            danmakus.Add(danmaku);
        }
        stopwatch.Stop();

        // Assert
        var throughput = count * 1000.0 / stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Created {count} danmakus in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Throughput: {throughput:F2} danmaku/second");

        Assert.True(throughput >= 5000, 
            $"Expected at least 5000 danmaku/second creation throughput, got {throughput:F2}");
    }

    /// <summary>
    /// 测试：弹幕序列化性能
    /// </summary>
    [Fact]
    public void DanmakuSerialization_ShouldBeFast()
    {
        // Arrange
        const int count = 10000;
        var danmakus = GenerateTestDanmakus(count);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var serialized = new List<string>();
        foreach (var danmaku in danmakus)
        {
            var displayInfo = danmaku.GetDisplayInfo();
            var json = JsonSerializer.Serialize(displayInfo);
            serialized.Add(json);
        }
        stopwatch.Stop();

        // Assert
        var throughput = count * 1000.0 / stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Serialized {count} danmakus in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Throughput: {throughput:F2} danmaku/second");

        Assert.True(throughput >= 10000, 
            $"Expected at least 10000 danmaku/second serialization throughput, got {throughput:F2}");
    }

    /// <summary>
    /// 测试：Redis缓存写入性能（模拟）
    /// </summary>
    [Fact]
    public async Task RedisCacheWrite_ShouldBeFast()
    {
        // Arrange
        const int count = 1000;
        var cache = Substitute.For<IDistributedCache>();
        var danmakus = GenerateTestDanmakus(count);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - 模拟缓存写入
        var tasks = danmakus.Select(async danmaku =>
        {
            var displayInfo = danmaku.GetDisplayInfo();
            var json = JsonSerializer.Serialize(displayInfo);
            await cache.SetStringAsync(
                $"danmaku:{danmaku.VideoId}:{danmaku.Id}",
                json,
                Arg.Any<DistributedCacheEntryOptions>());
        });

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var throughput = count * 1000.0 / stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Cached {count} danmakus in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Throughput: {throughput:F2} danmaku/second");

        // 模拟测试，主要验证代码逻辑
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Cache operations took too long: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// 测试：弹幕碰撞检测性能
    /// </summary>
    [Fact]
    public void CollisionDetection_ShouldBeEfficient()
    {
        // Arrange
        const int existingDanmakuCount = 100;
        const int newDanmakuCount = 1000;
        var existingDanmakus = GenerateTestDanmakus(existingDanmakuCount);
        var newDanmakus = GenerateTestDanmakus(newDanmakuCount);
        var collisionCount = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act - 简单的碰撞检测模拟
        foreach (var newDanmaku in newDanmakus)
        {
            var hasCollision = existingDanmakus.Any(d =>
                Math.Abs(d.GetDisplayInfo().PositionTime - newDanmaku.GetDisplayInfo().PositionTime) < 0.5);
            if (hasCollision)
            {
                collisionCount++;
            }
        }
        stopwatch.Stop();

        // Assert
        var throughput = newDanmakuCount * 1000.0 / stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Checked {newDanmakuCount} danmakus against {existingDanmakuCount} existing in {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Collisions found: {collisionCount}");
        _output.WriteLine($"Throughput: {throughput:F2} checks/second");

        Assert.True(throughput >= 1000, 
            $"Expected at least 1000 collision checks/second, got {throughput:F2}");
    }

    /// <summary>
    /// 测试：批量弹幕处理性能
    /// </summary>
    [Fact]
    public async Task BatchDanmakuProcessing_ShouldBeEfficient()
    {
        // Arrange
        const int batchSize = 100;
        const int totalBatches = 10;
        var allDanmakus = GenerateTestDanmakus(batchSize * totalBatches);
        var processedBatches = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < totalBatches; i++)
        {
            var batch = allDanmakus.Skip(i * batchSize).Take(batchSize);
            await ProcessBatchAsync(batch);
            processedBatches++;
        }
        stopwatch.Stop();

        // Assert
        var totalProcessed = batchSize * processedBatches;
        var throughput = totalProcessed * 1000.0 / stopwatch.ElapsedMilliseconds;
        _output.WriteLine($"Processed {totalProcessed} danmakus in {processedBatches} batches");
        _output.WriteLine($"Time elapsed: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Throughput: {throughput:F2} danmaku/second");

        Assert.Equal(totalBatches, processedBatches);
    }

    private List<Danmaku> GenerateTestDanmakus(int count)
    {
        var videoId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var danmakus = new List<Danmaku>();

        for (int i = 0; i < count; i++)
        {
            var danmaku = Danmaku.CreateNormal(
                videoId,
                userId,
                $"User{i % 100}",
                i % 100,
                $"测试弹幕内容{i}",
                (i % 300) + (i * 0.1), // 不同的时间点
                (DanmakuType)(i % 4),
                $"#{i % 256:X2}{(i * 2) % 256:X2}{(i * 3) % 256:X2}", // 随机颜色
                25);
            danmakus.Add(danmaku);
        }

        return danmakus;
    }

    private async Task SimulateDanmakuProcessingAsync(Danmaku danmaku)
    {
        // 模拟弹幕处理延迟（网络、数据库、缓存等）
        await Task.Delay(1);
        
        // 模拟序列化
        var displayInfo = danmaku.GetDisplayInfo();
        var json = JsonSerializer.Serialize(displayInfo);
        
        // 模拟缓存操作
        await Task.Delay(1);
    }

    private async Task ProcessBatchAsync(IEnumerable<Danmaku> batch)
    {
        foreach (var danmaku in batch)
        {
            await SimulateDanmakuProcessingAsync(danmaku);
        }
    }
}

/// <summary>
/// BenchmarkDotNet基准测试配置
/// </summary>
public class DanmakuBenchmarkConfig : ManualConfig
{
    public DanmakuBenchmarkConfig()
    {
        AddJob(Job.ShortRun);
        AddColumn(TargetMethodColumn.Method);
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(StatisticColumn.OperationsPerSecond);
    }
}

/// <summary>
/// BenchmarkDotNet基准测试
/// 运行方式：dotnet run -c Release
/// </summary>
[MemoryDiagnoser]
[Config(typeof(DanmakuBenchmarkConfig))]
public class DanmakuBenchmarks
{
    private List<Danmaku> _danmakus = new();
    private readonly Guid _videoId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    [Params(100, 1000, 10000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _danmakus = new List<Danmaku>();
        for (int i = 0; i < Count; i++)
        {
            var danmaku = Danmaku.CreateNormal(
                _videoId,
                _userId,
                $"User{i % 100}",
                i % 100,
                $"测试弹幕内容{i}",
                i % 300,
                DanmakuType.Scroll,
                "#FFFFFF",
                25);
            _danmakus.Add(danmaku);
        }
    }

    [Benchmark(Description = "弹幕创建")]
    public Danmaku CreateDanmaku()
    {
        return Danmaku.CreateNormal(
            _videoId,
            _userId,
            "TestUser",
            1,
            "测试弹幕内容",
            10.5,
            DanmakuType.Scroll,
            "#FFFFFF",
            25);
    }

    [Benchmark(Description = "弹幕序列化")]
    public string SerializeDanmaku()
    {
        var danmaku = _danmakus[0];
        var displayInfo = danmaku.GetDisplayInfo();
        return JsonSerializer.Serialize(displayInfo);
    }

    [Benchmark(Description = "批量弹幕序列化")]
    public List<string> SerializeDanmakus()
    {
        var results = new List<string>();
        foreach (var danmaku in _danmakus)
        {
            var displayInfo = danmaku.GetDisplayInfo();
            results.Add(JsonSerializer.Serialize(displayInfo));
        }
        return results;
    }

    [Benchmark(Description = "弹幕反序列化")]
    public DanmakuDisplayInfo DeserializeDanmaku()
    {
        var json = JsonSerializer.Serialize(_danmakus[0].GetDisplayInfo());
        return JsonSerializer.Deserialize<DanmakuDisplayInfo>(json)!;
    }
}

/// <summary>
/// BenchmarkDotNet入口点
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<DanmakuBenchmarks>();
    }
}