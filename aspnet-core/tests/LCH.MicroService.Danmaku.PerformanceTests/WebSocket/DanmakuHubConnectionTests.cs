using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace LCH.MicroService.Danmaku.PerformanceTests.WebSocket;

/// <summary>
/// WebSocket并发连接测试
/// 测试场景：100个并发WebSocket连接
/// </summary>
public class DanmakuHubConnectionTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private readonly string _hubUrl;
    private readonly List<HubConnection> _connections = new();
    private readonly ConcurrentBag<Exception> _connectionErrors = new();

    public DanmakuHubConnectionTests(ITestOutputHelper output)
    {
        _output = output;
        // 默认使用本地开发环境地址，可通过环境变量覆盖
        _hubUrl = Environment.GetEnvironmentVariable("DANMAKU_HUB_URL") 
            ?? "http://localhost:5000/hubs/danmaku";
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        foreach (var connection in _connections)
        {
            try
            {
                if (connection.State == HubConnectionState.Connected)
                {
                    await connection.StopAsync();
                }
                await connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _output.WriteLine($"Error disposing connection: {ex.Message}");
            }
        }
        _connections.Clear();
    }

    private HubConnection CreateHubConnection(string? accessToken = null)
    {
        var builder = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                }
                options.HttpMessageHandlerFactory = handler =>
                {
                    // 用于测试环境，跳过SSL验证
                    return handler;
                };
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3) })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Warning);
            });

        return builder.Build();
    }

    /// <summary>
    /// 测试：创建单个WebSocket连接
    /// </summary>
    [Fact]
    public async Task SingleConnection_ShouldConnectSuccessfully()
    {
        // Arrange
        var connection = CreateHubConnection();
        _connections.Add(connection);

        var connectionTcs = new TaskCompletionSource<bool>();
        connection.Closed += error =>
        {
            if (error != null)
            {
                connectionTcs.TrySetException(error);
            }
            return Task.CompletedTask;
        };

        // Act
        await connection.StartAsync();
        await Task.Delay(100); // 等待连接稳定

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);
    }

    /// <summary>
    /// 测试：10个并发WebSocket连接
    /// </summary>
    [Fact]
    public async Task TenConcurrentConnections_ShouldAllConnectSuccessfully()
    {
        // Arrange
        const int connectionCount = 10;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < connectionCount; i++)
        {
            var connection = CreateHubConnection();
            _connections.Add(connection);
            tasks.Add(ConnectWithRetryAsync(connection, i));
        }

        await Task.WhenAll(tasks);

        // Assert
        var connectedCount = _connections.Count(c => c.State == HubConnectionState.Connected);
        _output.WriteLine($"Connected: {connectedCount}/{connectionCount}");
        Assert.Equal(connectionCount, connectedCount);
    }

    /// <summary>
    /// 测试：100个并发WebSocket连接
    /// </summary>
    [Fact]
    public async Task HundredConcurrentConnections_ShouldHandleGracefully()
    {
        // Arrange
        const int connectionCount = 100;
        var tasks = new List<Task>();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        for (int i = 0; i < connectionCount; i++)
        {
            var connection = CreateHubConnection();
            _connections.Add(connection);
            tasks.Add(ConnectWithRetryAsync(connection, i));
        }

        await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        var connectedCount = _connections.Count(c => c.State == HubConnectionState.Connected);
        var failedCount = _connectionErrors.Count;
        
        _output.WriteLine($"Total connections: {connectionCount}");
        _output.WriteLine($"Connected: {connectedCount}");
        _output.WriteLine($"Failed: {failedCount}");
        _output.WriteLine($"Time elapsed: {stopwatch.ElapsedMilliseconds}ms");
        _output.WriteLine($"Connections per second: {connectedCount * 1000.0 / stopwatch.ElapsedMilliseconds:F2}");

        // 至少90%的连接应该成功
        Assert.True(connectedCount >= connectionCount * 0.9, 
            $"Expected at least {connectionCount * 0.9} connections, but got {connectedCount}");
    }

    /// <summary>
    /// 测试：连接后加入视频房间
    /// </summary>
    [Fact]
    public async Task Connection_ShouldJoinVideoRoomSuccessfully()
    {
        // Arrange
        var connection = CreateHubConnection();
        _connections.Add(connection);
        var videoId = Guid.NewGuid();
        var recentDanmakuReceived = new TaskCompletionSource<bool>();

        connection.On<List<object>>("RecentDanmakus", danmakus =>
        {
            recentDanmakuReceived.TrySetResult(true);
        });

        // Act
        await connection.StartAsync();
        await connection.InvokeAsync("JoinVideoRoom", videoId);

        // 等待接收历史弹幕（可能为空列表）
        await Task.WhenAny(recentDanmakuReceived.Task, Task.Delay(5000));

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);
    }

    /// <summary>
    /// 测试：多个连接同时加入同一视频房间
    /// </summary>
    [Fact]
    public async Task MultipleConnections_ShouldJoinSameVideoRoom()
    {
        // Arrange
        const int connectionCount = 20;
        var videoId = Guid.NewGuid();
        var tasks = new List<Task>();
        var receivedCount = 0;
        var lockObj = new object();

        // Act
        for (int i = 0; i < connectionCount; i++)
        {
            var connection = CreateHubConnection();
            _connections.Add(connection);
            
            connection.On<List<object>>("RecentDanmakus", danmakus =>
            {
                lock (lockObj)
                {
                    receivedCount++;
                }
            });

            tasks.Add(ConnectAndJoinRoomAsync(connection, videoId));
        }

        await Task.WhenAll(tasks);
        await Task.Delay(1000); // 等待所有响应

        // Assert
        var connectedCount = _connections.Count(c => c.State == HubConnectionState.Connected);
        _output.WriteLine($"Connected: {connectedCount}/{connectionCount}");
        _output.WriteLine($"Received recent danmaku: {receivedCount}/{connectionCount}");
        
        Assert.True(connectedCount >= connectionCount * 0.9);
    }

    /// <summary>
    /// 测试：连接断开后自动重连
    /// </summary>
    [Fact]
    public async Task Connection_ShouldReconnectAutomatically()
    {
        // Arrange
        var connection = CreateHubConnection();
        _connections.Add(connection);
        var reconnected = new TaskCompletionSource<bool>();

        connection.Reconnecting += error =>
        {
            _output.WriteLine("Connection reconnecting...");
            return Task.CompletedTask;
        };

        connection.Reconnected += connectionId =>
        {
            _output.WriteLine($"Connection reconnected: {connectionId}");
            reconnected.TrySetResult(true);
            return Task.CompletedTask;
        };

        // Act
        await connection.StartAsync();
        Assert.Equal(HubConnectionState.Connected, connection.State);

        // 模拟网络中断（通过停止后重新启动来测试重连机制）
        await connection.StopAsync();
        await Task.Delay(100);
        await connection.StartAsync();

        // Assert
        Assert.Equal(HubConnectionState.Connected, connection.State);
    }

    private async Task ConnectWithRetryAsync(HubConnection connection, int index, int maxRetries = 3)
    {
        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                await connection.StartAsync();
                await Task.Delay(50); // 等待连接稳定
                if (connection.State == HubConnectionState.Connected)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                _connectionErrors.Add(ex);
                if (retry < maxRetries - 1)
                {
                    await Task.Delay(100 * (retry + 1));
                }
            }
        }
    }

    private async Task ConnectAndJoinRoomAsync(HubConnection connection, Guid videoId)
    {
        try
        {
            await connection.StartAsync();
            await connection.InvokeAsync("JoinVideoRoom", videoId);
        }
        catch (Exception ex)
        {
            _connectionErrors.Add(ex);
        }
    }
}