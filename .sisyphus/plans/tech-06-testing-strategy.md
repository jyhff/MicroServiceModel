# 测试策略文档（结合现有ABP测试框架）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 测试策略设计 |
| 目标项目 | Bilibili视频平台 |
| 测试框架 | xUnit + ABP TestBase |
| 创建时间 | 2026-05-12 |

---

## 2. 测试框架架构（继承ABP）

### 2.1 ABP测试基础设施

继承ABP Framework内置测试基础设施：

| 测试类型 | ABP框架支持 | 说明 |
|---------|-----------|------|
| **单元测试** | xUnit + Shouldly | 测试单个类、方法 |
| **集成测试** | AbpIntegratedTest | 测试服务集成（带数据库） |
| **Repository测试** | EfCoreRepositoryTestBase | 测试数据访问 |
| **ApplicationService测试** | ApplicationTestBase | 测试应用服务 |
| **API测试** | WebApplicationFactory | 测试HTTP API |
| **性能测试** | BenchmarkDotNet | 性能基准测试 |

---

### 2.2 测试项目结构

```
aspnet-core/tests/
├── LCH.MicroService.Video.Tests/
│   ├── Domain.Tests/                     # 领域层测试
│   │   ├── VideoTests.cs                 # Video实体测试
│   │   ├── VideoManagerTests.cs          # 领域服务测试
│   │   └─ VideoDomainTestBase.cs         # 领域测试基类
│   │
│   ├── Application.Tests/                # 应用层测试
│   │   ├── VideoAppServiceTests.cs       # 应用服务测试
│   │   ├── UploadAppServiceTests.cs
│   │   └ VideoApplicationTestBase.cs     # 应用测试基类
│   │
│   ├── EntityFrameworkCore.Tests/        # 数据访问测试
│   │   ├── VideoRepositoryTests.cs       # Repository测试
│   │   ├── VideoDbContextTests.cs        # DbContext测试
│   │   └ VideoEfCoreTestBase.cs          # EF Core测试基类
│   │
│   └─ HttpApi.Tests/                     # HTTP API测试
│       ├── VideoControllerTests.cs       # 控制器测试
│       └ VideoHttpApiTestBase.cs         # HTTP API测试基类
│
├── LCH.MicroService.Danmaku.Tests/
│   ├── Domain.Tests/
│   ├── Application.Tests/
│   ├── SignalR.Tests/                    # SignalR Hub测试
│   │   ├── DanmakuHubTests.cs            # 弹幕Hub测试
│   │   └─ SignalRTestBase.cs             # SignalR测试基类
│   └
│   └─ EntityFrameworkCore.Tests/
│
├── LCH.MicroService.Interaction.Tests/
│   ├── Domain.Tests/
│   ├── Application.Tests/
│   ├── EventBus.Tests/                   # 事件总线测试
│   │   ├── VideoLikedEventHandlerTests.cs
│   │   ├── VideoCoinedEventHandlerTests.cs
│   │   └ EventBusTestBase.cs             # 事件总线测试基类
│   └
│   └─ EntityFrameworkCore.Tests/
│
├── LCH.MicroService.TranscodeWorker.Tests/
│   ├── Workers.Tests/                    # Worker测试
│   │   ├── TranscodeWorkerTests.cs       # 转码Worker测试
│   │   ├── FFmpegServiceTests.cs         # FFmpeg服务测试
│   │   ├── HLSGeneratorTests.cs          # HLS生成测试
│   │   └ WorkerTestBase.cs               # Worker测试基类
│   └
│   └ Integration.Tests/                  # 集成测试
│   │   ├── TranscodeIntegrationTests.cs  # 转码完整流程测试
│   │   └ IntegrationTestBase.cs          # 集成测试基类
│   └
│   └─ Benchmarks/                        # 性能测试
│       ├── TranscodeBenchmark.cs         # 转码性能测试
│       └ BenchmarkDotNetConfig.cs        # 性能测试配置
│
└── LCH.MicroService.All.Tests/           # 全局集成测试
│   ├── VideoUploadTranscodeFlowTests.cs  # 上传转码完整流程测试
│   ├── DanmakuInteractionFlowTests.cs    # 弹幕互动完整流程测试
│   └ AllTestsTestBase.cs                 # 全局测试基类
```

---

## 3. 测试基类设计

### 3.1 领域层测试基类

```csharp
// VideoDomainTestBase.cs
using Volo.Abp.Testing;
using Volo.Abp.Modularity;
using LCH.MicroService.Video.Domain;
using Xunit;

namespace LCH.MicroService.Video.Tests.Domain
{
    /// <summary>
    /// 视频领域测试基类（继承ABP TestBase）
    /// </summary>
    public abstract class VideoDomainTestBase : AbpIntegratedTest<VideoDomainTestModule>
    {
        protected readonly IVideoRepository VideoRepository;
        protected readonly VideoManager VideoManager;
        
        protected VideoDomainTestBase()
        {
            VideoRepository = GetRequiredService<IVideoRepository>();
            VideoManager = GetRequiredService<VideoManager>();
        }
    }
    
    /// <summary>
    /// 测试模块定义
    /// </summary>
    [DependsOn(
        typeof(VideoDomainModule)
    )]
    public class VideoDomainTestModule : AbpModule
    {
    }
}
```

---

### 3.2 应用层测试基类

```csharp
// VideoApplicationTestBase.cs
using Volo.Abp.Testing;
using LCH.MicroService.Video.Application;
using LCH.MicroService.Video.Application.Contracts.Services;

namespace LCH.MicroService.Video.Tests.Application
{
    /// <summary>
    /// 视频应用服务测试基类（继承ABP TestBase）
    /// </summary>
    public abstract class VideoApplicationTestBase : AbpIntegratedTest<VideoApplicationTestModule>
    {
        protected readonly IVideoAppService VideoAppService;
        protected readonly IUploadAppService UploadAppService;
        
        protected VideoApplicationTestBase()
        {
            VideoAppService = GetRequiredService<IVideoAppService>();
            UploadAppService = GetRequiredService<IUploadAppService>();
        }
        
        /// <summary>
        /// 创建测试用户（模拟登录）
        /// </summary>
        protected void LoginAsTestUser()
        {
            // 使用ABP FakeCurrentUser模拟登录
            var currentUser = GetRequiredService<ICurrentUser>();
            (currentUser as FakeCurrentUser).Id = Guid.NewGuid();
            (currentUser as FakeCurrentUser).UserName = "test-user";
        }
    }
    
    [DependsOn(
        typeof(VideoApplicationModule),
        typeof(VideoDomainModule)
    )]
    public class VideoApplicationTestModule : AbpModule
    {
    }
}
```

---

## 4. 单元测试示例

### 4.1 Video实体测试

```csharp
// VideoTests.cs
using Xunit;
using Shouldly;
using LCH.MicroService.Video.Domain.Entities;

namespace LCH.MicroService.Video.Tests.Domain
{
    public class VideoTests : VideoDomainTestBase
    {
        [Fact]
        public void Should_Create_Video_With_Valid_Parameters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var title = "测试视频";
            var categoryId = Guid.NewGuid();
            
            // Act
            var video = new Video(
                Guid.NewGuid(),
                userId,
                title,
                categoryId
            );
            
            // Assert
            video.ShouldNotBeNull();
            video.UserId.ShouldBe(userId);
            video.Title.ShouldBe(title);
            video.CategoryId.ShouldBe(categoryId);
            video.Status.ShouldBe(VideoStatus.Draft);
            video.IsOriginal.ShouldBeTrue();
        }
        
        [Fact]
        public void Should_Publish_Video_When_Audit_Approved()
        {
            // Arrange
            var video = CreateTestVideo();
            video.AuditStatus = AuditStatus.Approved;
            
            // Act
            video.Publish();
            
            // Assert
            video.Status.ShouldBe(VideoStatus.Published);
            video.IsPublished.ShouldBeTrue();
            video.PublishTime.ShouldNotBeNull();
        }
        
        [Fact]
        public void Should_Throw_Exception_When_Publish_Without_Audit()
        {
            // Arrange
            var video = CreateTestVideo();
            video.AuditStatus = AuditStatus.Pending;
            
            // Act & Assert
            Assert.Throws<BusinessException>(() => video.Publish());
        }
        
        [Fact]
        public void Should_Increment_View_Count()
        {
            // Arrange
            var video = CreateTestVideo();
            var initialCount = video.TotalViews;
            
            // Act
            video.IncrementViewCount();
            
            // Assert
            video.TotalViews.ShouldBe(initialCount + 1);
        }
        
        private Video CreateTestVideo()
        {
            return new Video(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "测试视频",
                Guid.NewGuid()
            );
        }
    }
}
```

---

### 4.2 VideoRepository测试

```csharp
// VideoRepositoryTests.cs
using Xunit;
using Shouldly;
using LCH.MicroService.Video.Domain.Entities;

namespace LCH.MicroService.Video.Tests.EntityFrameworkCore
{
    public class VideoRepositoryTests : VideoEfCoreTestBase
    {
        private readonly IVideoRepository _videoRepository;
        
        public VideoRepositoryTests()
        {
            _videoRepository = GetRequiredService<IVideoRepository>();
        }
        
        [Fact]
        public async Task Should_Insert_Video_To_Database()
        {
            // Arrange
            var video = new Video(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Repository测试视频",
                Guid.NewGuid()
            );
            
            // Act
            await _videoRepository.InsertAsync(video);
            
            // Assert
            var savedVideo = await _videoRepository.FindAsync(v => v.Id == video.Id);
            savedVideo.ShouldNotBeNull();
            savedVideo.Title.ShouldBe("Repository测试视频");
        }
        
        [Fact]
        public async Task Should_Get_User_Videos()
        {
            // Arrange
            var userId = Guid.NewGuid();
            await CreateTestVideos(userId, 5);
            
            // Act
            var videos = await _videoRepository.GetByUserIdAsync(userId);
            
            // Assert
            videos.Count.ShouldBe(5);
        }
        
        [Fact]
        public async Task Should_Increment_View_Count_Atomic()
        {
            // Arrange
            var video = await CreateAndSaveTestVideo();
            var initialCount = video.TotalViews;
            
            // Act
            await _videoRepository.IncrementViewCountAsync(video.Id);
            
            // Assert
            var updatedVideo = await _videoRepository.GetAsync(video.Id);
            updatedVideo.TotalViews.ShouldBe(initialCount + 1);
        }
        
        private async Task CreateTestVideos(Guid userId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var video = new Video(
                    Guid.NewGuid(),
                    userId,
                    $"测试视频{i}",
                    Guid.NewGuid()
                );
                await _videoRepository.InsertAsync(video);
            }
        }
        
        private async Task<Video> CreateAndSaveTestVideo()
        {
            var video = new Video(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "原子操作测试视频",
                Guid.NewGuid()
            );
            await _videoRepository.InsertAsync(video);
            return video;
        }
    }
}
```

---

### 4.3 VideoAppService测试

```csharp
// VideoAppServiceTests.cs
using Xunit;
using Shouldly;
using LCH.MicroService.Video.Application.Contracts.Dtos;
using LCH.MicroService.Video.Domain.Entities;

namespace LCH.MicroService.Video.Tests.Application
{
    public class VideoAppServiceTests : VideoApplicationTestBase
    {
        private readonly IVideoAppService _videoAppService;
        
        public VideoAppServiceTests()
        {
            _videoAppService = GetRequiredService<IVideoAppService>();
        }
        
        [Fact]
        public async Task Should_Create_Video()
        {
            // Arrange
            LoginAsTestUser();
            var input = new CreateVideoDto
            {
                Title = "应用服务测试视频",
                Description = "测试描述",
                CategoryId = Guid.NewGuid(),
                IsOriginal = true
            };
            
            // Act
            var result = await _videoAppService.CreateAsync(input);
            
            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe("应用服务测试视频");
            result.Status.ShouldBe("Draft");
        }
        
        [Fact]
        public async Task Should_Get_Video_List()
        {
            // Arrange
            LoginAsTestUser();
            var userId = GetCurrentUserId();
            await CreateTestVideos(userId, 10);
            
            var input = new GetVideoListDto
            {
                UserId = userId,
                MaxResultCount = 5,
                SkipCount = 0
            };
            
            // Act
            var result = await _videoAppService.GetListAsync(input);
            
            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(5);
            result.TotalCount.ShouldBe(10);
        }
        
        [Fact]
        public async Task Should_Throw_Exception_When_Unauthorized_Create()
        {
            // Arrange（未登录）
            var input = new CreateVideoDto
            {
                Title = "未授权测试",
                CategoryId = Guid.NewGuid()
            };
            
            // Act & Assert
            await Assert.ThrowsAsync<AbpAuthorizationException>(
                async () => await _videoAppService.CreateAsync(input)
            );
        }
        
        private Guid GetCurrentUserId()
        {
            var currentUser = GetRequiredService<ICurrentUser>();
            return currentUser.Id.Value;
        }
        
        private async Task CreateTestVideos(Guid userId, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var input = new CreateVideoDto
                {
                    Title = $"测试视频{i}",
                    CategoryId = Guid.NewGuid()
                };
                await _videoAppService.CreateAsync(input);
            }
        }
    }
}
```

---

## 5. 集成测试示例

### 5.1 视频上传转码完整流程测试

```csharp
// VideoUploadTranscodeFlowTests.cs
using Xunit;
using Shouldly;
using LCH.MicroService.Video.Application.Contracts.Dtos;

namespace LCH.MicroService.All.Tests
{
    public class VideoUploadTranscodeFlowTests : AllTestsTestBase
    {
        private readonly IVideoAppService _videoAppService;
        private readonly IUploadAppService _uploadAppService;
        private readonly ITranscodeAppService _transcodeAppService;
        private readonly IDistributedEventBus _eventBus;
        
        public VideoUploadTranscodeFlowTests()
        {
            _videoAppService = GetRequiredService<IVideoAppService>();
            _uploadAppService = GetRequiredService<IUploadAppService>();
            _transcodeAppService = GetRequiredService<ITranscodeAppService>();
            _eventBus = GetRequiredService<IDistributedEventBus>();
        }
        
        [Fact]
        public async Task Should_Complete_Video_Upload_Transcode_Flow()
        {
            // 1. 创建视频
            LoginAsTestUser();
            var videoInput = new CreateVideoDto
            {
                Title = "完整流程测试视频",
                CategoryId = Guid.NewGuid()
            };
            var video = await _videoAppService.CreateAsync(videoInput);
            
            // 2. 初始化上传会话
            var uploadSession = await _uploadAppService.InitAsync(new InitUploadDto
            {
                VideoId = video.Id,
                FileName = "test.mp4",
                FileSize = 10 * 1024 * 1024 // 10MB
            });
            
            // 3. 上传分片（模拟）
            var chunks = 2;
            for (int i = 0; i < chunks; i++)
            {
                await _uploadAppService.UploadChunkAsync(uploadSession.UploadSessionId, i, new UploadChunkDto
                {
                    Md5 = $"chunk{i}_md5"
                });
            }
            
            // 4. 完成上传
            await _uploadAppService.CompleteAsync(uploadSession.UploadSessionId, new CompleteUploadDto
            {
                TotalMd5 = "total_md5"
            });
            
            // 5. 等待转码事件
            var transcodeEventReceived = false;
            _eventBus.Subscribe<TranscodeCompletedEto>(async (eventData) =>
            {
                transcodeEventReceived = true;
            });
            
            // 6. 模拟转码完成（实际环境中Worker会处理）
            await _eventBus.PublishAsync(new TranscodeCompletedEto
            {
                VideoId = video.Id,
                Resolutions = new List<string> { "360p", "480p", "720p" }
            });
            
            // 7. 验证视频状态
            await WaitForCondition(() => transcodeEventReceived, TimeSpan.FromSeconds(5));
            
            var finalVideo = await _videoAppService.GetAsync(video.Id);
            finalVideo.Status.ShouldBe("Auditing");
            finalVideo.HasTranscoded.ShouldBeTrue();
        }
        
        private async Task WaitForCondition(Func<bool> condition, TimeSpan timeout)
        {
            var endTime = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < endTime)
            {
                if (condition())
                    return;
                await Task.Delay(100);
            }
            throw new TimeoutException("条件未满足");
        }
    }
}
```

---

### 5.2 弹幕互动完整流程测试

```csharp
// DanmakuInteractionFlowTests.cs
using Xunit;
using Shouldly;
using Microsoft.AspNetCore.SignalR.Client;

namespace LCH.MicroService.All.Tests
{
    public class DanmakuInteractionFlowTests : AllTestsTestBase
    {
        private readonly IVideoAppService _videoAppService;
        private readonly IDanmakuAppService _danmakuAppService;
        private readonly IInteractionAppService _interactionAppService;
        private readonly IUserLevelAppService _userLevelAppService;
        
        [Fact]
        public async Task Should_Complete_Watch_Danmaku_Interaction_Flow()
        {
            // 1. 创建视频并发布
            LoginAsTestUser();
            var video = await CreateAndPublishVideo();
            
            // 2. 观看视频（触发经验获得）
            await _videoAppService.RecordPlayAsync(video.Id, new RecordPlayDto
            {
                PlayDuration = 100,
                VideoDuration = 100,
                PlayProgress = 100 // 完整观看
            });
            
            // 3. 验证获得经验（+1）
            var userLevel = await _userLevelAppService.GetAsync();
            userLevel.DailyExperience.ShouldBe(1);
            
            // 4. 发送弹幕
            var danmaku = await _danmakuAppService.SendAsync(new SendDanmakuDto
            {
                VideoId = video.Id,
                Content = "测试弹幕",
                PositionTime = 10.5
            });
            danmaku.ShouldNotBeNull();
            
            // 5. 点赞视频（触发事件）
            await _interactionAppService.LikeAsync(video.Id);
            
            // 6. 投币视频（消耗B币）
            await _interactionAppService.CoinAsync(video.Id, new CoinInputDto
            {
                CoinAmount = 1
            });
            
            // 7. 验证B币扣除
            var userCoins = await _userLevelAppService.GetCoinsAsync();
            userCoins.Balance.ShouldBeLessThan(1000);
            
            // 8. 验证视频统计更新
            var finalVideo = await _videoAppService.GetAsync(video.Id);
            finalVideo.TotalLikes.ShouldBe(1);
            finalVideo.TotalCoins.ShouldBe(1);
        }
        
        private async Task<VideoDto> CreateAndPublishVideo()
        {
            var video = await _videoAppService.CreateAsync(new CreateVideoDto
            {
                Title = "弹幕互动测试视频",
                CategoryId = Guid.NewGuid()
            });
            
            // 模拟审核通过
            await _videoAppService.ApproveAsync(video.Id, new AuditInputDto
            {
                Status = AuditStatus.Approved
            });
            
            // 发布
            await _videoAppService.PublishAsync(video.Id);
            
            return await _videoAppService.GetAsync(video.Id);
        }
    }
}
```

---

## 6. SignalR测试

### 6.1 SignalR Hub测试

```csharp
// DanmakuHubTests.cs
using Xunit;
using Shouldly;
using Microsoft.AspNetCore.SignalR.Client;

namespace LCH.MicroService.Danmaku.Tests.SignalR
{
    public class DanmakuHubTests : SignalRTestBase
    {
        private readonly HubConnection _hubConnection;
        
        public DanmakuHubTests()
        {
            _hubConnection = CreateHubConnection("/hubs/danmaku");
        }
        
        [Fact]
        public async Task Should_Connect_To_DanmakuHub()
        {
            // Act
            await _hubConnection.StartAsync();
            
            // Assert
            _hubConnection.State.ShouldBe(HubConnectionState.Connected);
        }
        
        [Fact]
        public async Task Should_Join_Video_Room()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var recentDanmakusReceived = false;
            
            _hubConnection.On<List<DanmakuDto>>("RecentDanmakus", (danmakus) =>
            {
                recentDanmakusReceived = true;
            });
            
            await _hubConnection.StartAsync();
            
            // Act
            await _hubConnection.InvokeAsync("JoinRoom", videoId);
            
            // Assert
            await WaitForCondition(() => recentDanmakusReceived, TimeSpan.FromSeconds(2));
            recentDanmakusReceived.ShouldBeTrue();
        }
        
        [Fact]
        public async Task Should_Send_And_Receive_Danmaku()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var receivedDanmaku = false;
            var expectedContent = "SignalR测试弹幕";
            
            _hubConnection.On<DanmakuDto>("NewDanmaku", (danmaku) =>
            {
                if (danmaku.Content == expectedContent)
                receivedDanmaku = true;
            });
            
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync("JoinRoom", videoId);
            
            // Act
            await _hubConnection.InvokeAsync("SendDanmaku", new SendDanmakuMessage
            {
                VideoId = videoId,
                Content = expectedContent,
                PositionTime = 10.5,
                DanmakuType = 0,
                FontSize = 25,
                FontColor = "#FFFFFF"
            });
            
            // Assert
            await WaitForCondition(() => receivedDanmaku, TimeSpan.FromSeconds(2));
            receivedDanmaku.ShouldBeTrue();
        }
        
        [Fact]
        public async Task Should_Reject_Advanced_Danmaku_When_Level_Low()
        {
            // Arrange
            LoginAsUserWithLevel(1); // LV1用户
            var videoId = Guid.NewGuid();
            
            await _hubConnection.StartAsync();
            await _hubConnection.InvokeAsync("JoinRoom", videoId);
            
            // Act & Assert
            await Assert.ThrowsAsync<HubException>(
                async () => await _hubConnection.InvokeAsync("SendDanmaku", new SendDanmakuMessage
                {
                    VideoId = videoId,
                    Content = "高级弹幕",
                    DanmakuType = 3 // 高级弹幕
                })
            );
        }
        
        private HubConnection CreateHubConnection(string path)
        {
            return new HubConnectionBuilder()
                .WithUrl($"{TestServer.BaseAddress}{path}", options =>
                {
                    options.HttpMessageHandlerFactory = _ => TestServer.CreateHandler();
                    options.AccessTokenProvider = () => GetTestToken();
                })
                .Build();
        }
        
        private async Task WaitForCondition(Func<bool> condition, TimeSpan timeout)
        {
            var endTime = DateTime.UtcNow.Add(timeout);
            while (DateTime.UtcNow < endTime)
            {
                if (condition())
                    return;
                await Task.Delay(100);
            }
            throw new TimeoutException("条件未满足");
        }
    }
}
```

---

## 7. 性能测试

### 7.1 转码性能测试

```csharp
// TranscodeBenchmark.cs
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace LCH.MicroService.TranscodeWorker.Tests.Benchmarks
{
    [MemoryDiagnoser]
    public class TranscodeBenchmark
    {
        private FFmpegService _ffmpegService;
        private string _testVideoPath;
        
        [GlobalSetup]
        public void Setup()
        {
            _ffmpegService = new FFmpegService();
            _testVideoPath = "test_videos/sample_1080p.mp4";
        }
        
        [Benchmark]
        public async Task Transcode_360P()
        {
            await _ffmpegService.TranscodeAsync(_testVideoPath, Resolution360P);
        }
        
        [Benchmark]
        public async Task Transcode_480P()
        {
            await _ffmpegService.TranscodeAsync(_testVideoPath, Resolution480P);
        }
        
        [Benchmark]
        public async Task Transcode_720P()
        {
            await _ffmpegService.TranscodeAsync(_testVideoPath, Resolution720P);
        }
        
        [Benchmark]
        public async Task Transcode_1080P()
        {
            await _ffmpegService.TranscodeAsync(_testVideoPath, Resolution1080P);
        }
        
        [Benchmark]
        public async Task Generate_HLS_Master()
        {
            await _ffmpegService.GenerateHLSAsync(_testVideoPath);
        }
    }
}

// 运行性能测试
public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<TranscodeBenchmark>();
    }
}
```

---

## 8. 测试覆盖率目标

| 模块 | 单元测试覆盖率 | 集成测试覆盖率 | 总覆盖率 |
|------|--------------|--------------|---------|
| VideoService | >80% | >70% | >75% |
| DanmakuService | >85% | >70% | >78% |
| InteractionService | >80% | >75% | >77% |
| UserService | >85% | >80% | >82% |
| TranscodeWorker | >70% | >60% | >65% |
| SearchService | >75% | >60% | >68% |
| RecommendService | >70% | >50% | >60% |
| LiveService | >75% | >60% | >68% |
| AdminService | >85% | >70% | >78% |
| **平均覆盖率** | **>80%** | **>70%** | **>75%** |

---

## 9. 测试命令

### 9.1 运行所有测试

```bash
# 运行所有单元测试
dotnet test

# 运行特定模块测试
dotnet test aspnet-core/tests/LCH.MicroService.Video.Tests

# 运行集成测试
dotnet test aspnet-core/tests/LCH.MicroService.All.Tests

# 运行性能测试
dotnet run --project aspnet-core/tests/LCH.MicroService.TranscodeWorker.Tests/Benchmarks
```

---

## 10. 测试最佳实践

### 10.1 ABP测试最佳实践

| 实践 | 说明 |
|------|------|
| **继承ABP TestBase** | 使用AbpIntegratedTest作为测试基类 |
| **使用Shouldly** | 使用Shouldly库进行断言（ABP推荐） |
| **Mock ICurrentUser** | 使用FakeCurrentUser模拟登录用户 |
| **使用InMemory数据库** | 集成测试使用InMemory SQLite |
| **测试事件总线** | 使用CAP测试支持验证事件传递 |
| **测试并发操作** | 测试原子操作和并发场景 |
| **测试权限** | 测试未授权访问抛出AbpAuthorizationException |
| **清理测试数据** | 测试后清理数据库和缓存 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 测试策略文档完成