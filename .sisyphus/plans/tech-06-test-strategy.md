# 测试策略文档（结合现有项目）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 测试策略文档 |
| 目标项目 | Bilibili视频平台 |
| 测试框架 | xUnit + ABP TestBase |
| 创建时间 | 2026-05-12 |

---

## 2. 现有项目测试架构分析

### 2.1 现有测试框架

基于项目探索，现有项目已具备ABP标准测试架构：

```
现有测试项目（aspnet-core/test/）：
├── LCH.MicroService.Identity.Tests/
│   ├── IdentityTestBase.cs
│   ├── IdentityDomainTests.cs
│   └── IdentityApplicationTests.cs
│
├── LCH.MicroService.Platform.Tests/
│   ├── PlatformTestBase.cs
│   └─ ...
│
└── LCH.MicroService.*.Tests/（各模块测试）
```

**结论**: 我们将继承此测试架构，为Bilibili新增模块创建测试项目。

---

## 3. 测试策略设计

### 3.1 测试金字塔

```
┌─────────────────────────────────────────────────────────────┐
│                    测试金字塔                                │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │                E2E测试（端到端测试）                     │ │
│  │              最少（5%），测试关键业务流程                │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │              集成测试（服务间集成）                     │ │
│  │           中等（15%），测试模块间通信                   │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │              单元测试（模块内部逻辑）                   │ │
│  │          最多（80%），测试每个类和方法                  │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### 3.2 测试覆盖率目标

| 测试类型 | 目标覆盖率 | 说明 |
|---------|-----------|------|
| **单元测试** | 80%+ | Domain层、Application层核心逻辑 |
| **集成测试** | 60%+ | 模块间通信、数据库操作 |
| **E2E测试** | 关键流程100% | 用户注册登录、视频上传播放、弹幕发送 |

---

## 4. 单元测试设计

### 4.1 测试项目结构

```
aspnet-core/test/
├── LCH.MicroService.Video.Tests/
│   ├── VideoTestBase.cs                   # 测试基类（继承ABP TestBase）
│   ├── Domain/
│   │   ├── VideoTests.cs                  # Video实体测试
│   │   ├── VideoManagerTests.cs           # 领域服务测试
│   │   ├── CategoryTests.cs               # Category实体测试
│   │   └─ VideoStatusTests.cs             # 状态流转测试
│   ├── Application/
│   │   ├── VideoAppServiceTests.cs        # 应用服务测试
│   │   ├── UploadAppServiceTests.cs       # 上传服务测试
│   │   ├── TranscodeAppServiceTests.cs    # 转码服务测试
│   │   └ VideoQueryAppServiceTests.cs    # 查询服务测试
│   ├── EntityFrameworkCore/
│   │   ├── VideoRepositoryTests.cs        # Repository测试
│   │   ├── VideoDbContextTests.cs         # DbContext测试
│   │   └─ VideoMigrationTests.cs          # 迁移测试
│   └─ VideoTestsModule.cs                 # 测试模块定义
│
├── LCH.MicroService.Danmaku.Tests/
│   ├── DanmakuTestBase.cs
│   ├── Domain/
│   │   ├── DanmakuTests.cs                # Danmaku实体测试
│   │   ├── DanmakuManagerTests.cs         # 领域服务测试
│   ├── Application/
│   │   ├── DanmakuAppServiceTests.cs      # 应用服务测试
│   │   ├── DanmakuHubTests.cs             # SignalR Hub测试
│   └─ DanmakuTestsModule.cs
│
├── LCH.MicroService.Interaction.Tests/
│   ├── InteractionTestBase.cs
│   ├── Domain/
│   │   ├── LikeTests.cs                   # 点赞实体测试
│   │   ├── CoinTests.cs                   # 投币实体测试
│   │   ├── CommentTests.cs                # 评论实体测试
│   ├── Application/
│   │   ├── LikeAppServiceTests.cs         # 点赞服务测试
│   │   ├── CoinAppServiceTests.cs         # 投币服务测试
│   │   ├── CommentAppServiceTests.cs      # 评论服务测试
│   └─ InteractionTestsModule.cs
│
├── LCH.MicroService.User.Tests/
│   ├── UserTestBase.cs
│   ├── Domain/
│   │   ├── BilibiliUserTests.cs           # 用户扩展实体测试
│   │   ├── UserLevelTests.cs              # 用户等级测试
│   │   ├── UserCoinTests.cs               # B币测试
│   ├── Application/
│   │   ├── UserProfileAppServiceTests.cs  # 用户信息服务测试
│   │   ├── UserLevelAppServiceTests.cs    # 等级服务测试
│   │   ├── UserCoinAppServiceTests.cs     # B币服务测试
│   └─ UserTestsModule.cs
│
└── LCH.MicroService.*.Tests/（其他模块测试）
```

---

### 4.2 测试基类设计（继承ABP TestBase）

```csharp
// VideoTestBase.cs
using Volo.Abp;
using Volo.Abp.Testing;
using Volo.Abp.Modularity;
using Volo.Abp.Data;
using Xunit;

namespace LCH.MicroService.Video.Tests
{
    /// <summary>
    /// 视频模块测试基类（继承ABP TestBase）
    /// </summary>
    public abstract class VideoTestBase : AbpIntegratedTest<VideoTestModule>
    {
        protected VideoTestBase()
        {
        }
        
        /// <summary>
        /// 获取测试用户ID
        /// </summary>
        protected Guid GetTestUserId()
        {
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
        
        /// <summary>
        /// 创建测试视频实体
        /// </summary>
        protected Video CreateTestVideo(string title = "测试视频")
        {
            return new Video(
                GuidGenerator.Create(),
                title,
                GetTestUserId()
            );
        }
    }
    
    /// <summary>
    /// 视频测试模块定义
    /// </summary>
    [DependsOn(
        typeof(VideoDomainModule),
        typeof(VideoApplicationModule),
        typeof(AbpTestBaseModule)
    )]
    public class VideoTestModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // 配置测试数据库（使用InMemory数据库）
            context.Services.AddEntityFrameworkInMemoryDatabase();
            
            // 配置测试Redis（使用Fake Redis）
            context.Services.AddSingleton<IDistributedCache, FakeDistributedCache>();
            
            // 配置测试BlobContainer（使用InMemory存储）
            context.Services.Replace(ServiceDescriptor.Singleton<IBlobContainer, FakeBlobContainer>());
        }
        
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            // 种子数据
            SeedTestData(context);
        }
        
        private void SeedTestData(ApplicationInitializationContext context)
        {
            using (var scope = context.ServiceProvider.CreateScope())
            {
                var videoRepository = scope.ServiceProvider.GetRequiredService<IVideoRepository>();
                
                // 创建测试数据
                var video = new Video(
                    Guid.Parse("00000000-0000-0000-0000-000000000001"),
                    "测试视频1",
                    Guid.Parse("00000000-0000-0000-0000-000000000001")
                );
                
                videoRepository.InsertAsync(video);
            }
        }
    }
}
```

---

### 4.3 Domain层单元测试示例

```csharp
// VideoTests.cs
using Xunit;
using Shouldly;
using Volo.Abp;

namespace LCH.MicroService.Video.Tests.Domain
{
    /// <summary>
    /// Video实体单元测试
    /// </summary>
    public class VideoTests : VideoTestBase
    {
        [Fact]
        public void Should_Create_Video_With_Valid_Data()
        {
            // Arrange
            var title = "测试视频标题";
            var userId = GetTestUserId();
            
            // Act
            var video = new Video(GuidGenerator.Create(), title, userId);
            
            // Assert
            video.ShouldNotBeNull();
            video.Title.ShouldBe(title);
            video.UserId.ShouldBe(userId);
            video.Status.ShouldBe(VideoStatus.Draft);
            video.AuditStatus.ShouldBe(AuditStatus.Pending);
            video.IsPublished.ShouldBeFalse();
        }
        
        [Fact]
        public void Should_Not_Create_Video_With_Empty_Title()
        {
            // Arrange
            var title = "";
            var userId = GetTestUserId();
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => 
                new Video(GuidGenerator.Create(), title, userId));
        }
        
        [Fact]
        public void Should_Publish_Video_When_Approved()
        {
            // Arrange
            var video = CreateTestVideo();
            video.AuditStatus = AuditStatus.Approved;
            video.Status = VideoStatus.Auditing;
            
            // Act
            video.Publish();
            
            // Assert
            video.Status.ShouldBe(VideoStatus.Published);
            video.IsPublished.ShouldBeTrue();
            video.PublishTime.ShouldNotBeNull();
        }
        
        [Fact]
        public void Should_Not_Publish_Video_When_Not_Approved()
        {
            // Arrange
            var video = CreateTestVideo();
            video.AuditStatus = AuditStatus.Pending;
            
            // Act & Assert
            Assert.Throws<BusinessException>(() => video.Publish());
        }
        
        [Fact]
        public void Should_Approve_Video_By_Auditor()
        {
            // Arrange
            var video = CreateTestVideo();
            var auditorId = Guid.NewGuid();
            var reason = "内容符合规范";
            
            // Act
            video.Approve(auditorId, reason);
            
            // Assert
            video.AuditStatus.ShouldBe(AuditStatus.Approved);
            video.AuditorId.ShouldBe(auditorId);
            video.AuditReason.ShouldBe(reason);
            video.AuditTime.ShouldNotBeNull();
        }
        
        [Fact]
        public void Should_Reject_Video_By_Auditor()
        {
            // Arrange
            var video = CreateTestVideo();
            var auditorId = Guid.NewGuid();
            var reason = "内容违规";
            
            // Act
            video.Reject(auditorId, reason);
            
            // Assert
            video.AuditStatus.ShouldBe(AuditStatus.Rejected);
            video.Status.ShouldBe(VideoStatus.Rejected);
            video.AuditReason.ShouldBe(reason);
        }
        
        [Fact]
        public void Should_Complete_Transcoding()
        {
            // Arrange
            var video = CreateTestVideo();
            video.Status = VideoStatus.UploadCompleted;
            
            var masterPath = "videos/test/master.m3u8";
            var resolution1080PPath = "videos/test/1080p.m3u8";
            var resolution720PPath = "videos/test/720p.m3u8";
            var resolution480PPath = "videos/test/480p.m3u8";
            var resolution360PPath = "videos/test/360p.m3u8";
            
            // Act
            video.CompleteTranscoding(
                masterPath,
                resolution1080PPath,
                resolution720PPath,
                resolution480PPath,
                resolution360PPath
            );
            
            // Assert
            video.HasTranscoded.ShouldBeTrue();
            video.MasterPlaylistPath.ShouldBe(masterPath);
            video.Status.ShouldBe(VideoStatus.Auditing);
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
    }
}
```

---

### 4.4 Application层单元测试示例

```csharp
// VideoAppServiceTests.cs
using Xunit;
using Shouldly;
using Moq;
using Volo.Abp.ObjectMapping;

namespace LCH.MicroService.Video.Tests.Application
{
    /// <summary>
    /// VideoAppService单元测试
    /// </summary>
    public class VideoAppServiceTests : VideoTestBase
    {
        private readonly IVideoAppService _videoAppService;
        private readonly IVideoRepository _videoRepository;
        private readonly Mock<IBlobContainer> _blobContainerMock;
        private readonly Mock<IDistributedEventBus> _eventBusMock;
        
        public VideoAppServiceTests()
        {
            _videoRepository = GetRequiredService<IVideoRepository>();
            _blobContainerMock = new Mock<IBlobContainer>();
            _eventBusMock = new Mock<IDistributedEventBus>();
            
            _videoAppService = new VideoAppService(
                _videoRepository,
                _blobContainerMock.Object,
                _eventBusMock.Object
            );
        }
        
        [Fact]
        public async Task Should_Create_Video_Successfully()
        {
            // Arrange
            var input = new CreateVideoDto
            {
                Title = "测试视频",
                Description = "测试描述",
                CategoryId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Tags = new List<string> { "测试" },
                IsOriginal = true,
                TotalFileSize = 1024000
            };
            
            // Act
            var result = await _videoAppService.CreateAsync(input);
            
            // Assert
            result.ShouldNotBeNull();
            result.Title.ShouldBe(input.Title);
            result.Status.ShouldBe(VideoStatus.Draft.ToString());
            result.UploadSessionId.ShouldNotBeNull();
            
            // 验证事件发布
            _eventBusMock.Verify(
                x => x.PublishAsync(It.IsAny<VideoCreatedEto>()),
                Times.Once
            );
        }
        
        [Fact]
        public async Task Should_Get_Video_Detail()
        {
            // Arrange
            var video = await CreateAndSaveTestVideo("测试视频详情");
            
            // Act
            var result = await _videoAppService.GetAsync(video.Id);
            
            // Assert
            result.ShouldNotBeNull();
            result.Id.ShouldBe(video.Id);
            result.Title.ShouldBe(video.Title);
        }
        
        [Fact]
        public async Task Should_Record_Play_And_Grant_Experience()
        {
            // Arrange
            var video = await CreateAndSaveTestVideo();
            
            var input = new RecordPlayDto
            {
                PlayDuration = 120,
                VideoDuration = 120,
                PlayProgress = 95,  // 95% > 90%, 完整观看
                PlayedResolution = "720P",
                DeviceType = "Web",
                StartTime = DateTime.UtcNow.AddMinutes(-2),
                EndTime = DateTime.UtcNow
            };
            
            // Act
            var result = await _videoAppService.RecordPlayAsync(video.Id, input);
            
            // Assert
            result.ShouldNotBeNull();
            result.VideoId.ShouldBe(video.Id);
            result.ExperienceEarned.ShouldBe(1);  // 完整观看+1经验
            
            // 验证播放量增加事件
            _eventBusMock.Verify(
                x => x.PublishAsync(It.IsAny<UserExperienceEto>()),
                Times.Once
            );
        }
        
        [Fact]
        public async Task Should_Not_Grant_Experience_When_Not_Complete_Watch()
        {
            // Arrange
            var video = await CreateAndSaveTestVideo();
            
            var input = new RecordPlayDto
            {
                PlayDuration = 60,
                VideoDuration = 120,
                PlayProgress = 50,  // 50% < 90%, 未完整观看
            };
            
            // Act
            var result = await _videoAppService.RecordPlayAsync(video.Id, input);
            
            // Assert
            result.ExperienceEarned.ShouldBe(0);  // 未完整观看+0经验
        }
        
        [Fact]
        public async Task Should_Get_My_Videos_List()
        {
            // Arrange
            var userId = GetTestUserId();
            
            // 创建多个测试视频
            await CreateAndSaveTestVideo("视频1", userId);
            await CreateAndSaveTestVideo("视频2", userId);
            await CreateAndSaveTestVideo("视频3", userId);
            
            var input = new GetMyVideosInput
            {
                MaxResultCount = 10,
                SkipCount = 0
            };
            
            // Act
            var result = await _videoAppService.GetMyVideosAsync(input);
            
            // Assert
            result.ShouldNotBeNull();
            result.Items.Count.ShouldBe(3);
            result.TotalCount.ShouldBe(3);
        }
        
        private async Task<Video> CreateAndSaveTestVideo(string title = "测试视频", Guid? userId = null)
        {
            var video = new Video(
                GuidGenerator.Create(),
                title,
                userId ?? GetTestUserId()
            );
            
            return await _videoRepository.InsertAsync(video);
        }
    }
}
```

---

## 5. 集成测试设计

### 5.1 SignalR集成测试

```csharp
// DanmakuHubTests.cs
using Xunit;
using Shouldly;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace LCH.MicroService.Danmaku.Tests.Application
{
    /// <summary>
    /// 弹幕SignalR Hub集成测试
    /// </summary>
    public class DanmakuHubTests : DanmakuTestBase
    {
        private readonly DanmakuHub _hub;
        private readonly Mock<HubCallerContext> _callerContextMock;
        private readonly Mock<IGroupManager> _groupsMock;
        private readonly Mock<IClientProxy> _clientProxyMock;
        
        public DanmakuHubTests()
        {
            _hub = GetRequiredService<DanmakuHub>();
            _callerContextMock = new Mock<HubCallerContext>();
            _groupsMock = new Mock<IGroupManager>();
            _clientProxyMock = new Mock<IClientProxy>();
            
            _hub.Context = _callerContextMock.Object;
            _hub.Groups = _groupsMock.Object;
        }
        
        [Fact]
        public async Task Should_Join_Video_Room_Successfully()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var connectionId = "test-connection-id";
            
            _callerContextMock.Setup(x => x.ConnectionId).Returns(connectionId);
            _groupsMock.Setup(x => x.AddToGroupAsync(connectionId, $"video-{videoId}", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            await _hub.JoinVideoRoom(videoId);
            
            // Assert
            _groupsMock.Verify(
                x => x.AddToGroupAsync(connectionId, $"video-{videoId}", It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        
        [Fact]
        public async Task Should_Send_Danmaku_To_Room()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var userId = GetTestUserId();
            var userName = "测试用户";
            
            var message = new SendDanmakuMessage
            {
                VideoId = videoId,
                Content = "测试弹幕",
                PositionTime = 10.5,
                DanmakuType = 0,  // 普通弹幕
                FontSize = 25,
                FontColor = "#FFFFFF"
            };
            
            // 模拟当前用户
            _callerContextMock.Setup(x => x.UserIdentifier).Returns(userId.ToString());
            
            // Act
            await _hub.SendDanmaku(message);
            
            // Assert
            // 验证弹幕已保存到数据库
            var danmakuRepository = GetRequiredService<IDanmakuRepository>();
            var danmakus = await danmakuRepository.GetListAsync(d => d.VideoId == videoId);
            danmakus.Count.ShouldBe(1);
            danmakus[0].Content.ShouldBe("测试弹幕");
        }
        
        [Fact]
        public async Task Should_Not_Send_Advanced_Danmaku_When_Level_Below_2()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var userId = Guid.NewGuid();  // 低等级用户
            
            var message = new SendDanmakuMessage
            {
                VideoId = videoId,
                Content = "高级弹幕",
                PositionTime = 10.5,
                DanmakuType = 3,  // 高级弹幕
            };
            
            // 模拟LV1用户
            _callerContextMock.Setup(x => x.UserIdentifier).Returns(userId.ToString());
            
            // Act & Assert
            await Assert.ThrowsAsync<UserFriendlyException>(() => 
                _hub.SendDanmaku(message));
        }
        
        [Fact]
        public async Task Should_Send_Advanced_Danmaku_When_Level_Above_2()
        {
            // Arrange
            var videoId = Guid.NewGuid();
            var userId = Guid.NewGuid();  // LV2+用户
            
            var message = new SendDanmakuMessage
            {
                VideoId = videoId,
                Content = "高级弹幕",
                PositionTime = 10.5,
                DanmakuType = 3,  // 高级弹幕
            };
            
            // 模拟LV2用户
            _callerContextMock.Setup(x => x.UserIdentifier).Returns(userId.ToString());
            
            // Act
            await _hub.SendDanmaku(message);
            
            // Assert
            // 验证高级弹幕已保存
            var danmakuRepository = GetRequiredService<IDanmakuRepository>();
            var danmaku = await danmakuRepository.FirstOrDefaultAsync(d => d.VideoId == videoId);
            danmaku.ShouldNotBeNull();
            danmaku.DanmakuType.ShouldBe(DanmakuType.Advanced);
        }
    }
}
```

---

### 5.2 数据库集成测试

```csharp
// VideoRepositoryTests.cs
using Xunit;
using Shouldly;

namespace LCH.MicroService.Video.Tests.EntityFrameworkCore
{
    /// <summary>
    /// VideoRepository集成测试
    /// </summary>
    public class VideoRepositoryTests : VideoTestBase
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
                GuidGenerator.Create(),
                "数据库测试视频",
                GetTestUserId()
            );
            
            // Act
            var insertedVideo = await _videoRepository.InsertAsync(video);
            
            // Assert
            insertedVideo.ShouldNotBeNull();
            insertedVideo.Id.ShouldBe(video.Id);
            
            // 验证数据库中确实存在
            var retrievedVideo = await _videoRepository.FindAsync(video.Id);
            retrievedVideo.ShouldNotBeNull();
            retrievedVideo.Title.ShouldBe("数据库测试视频");
        }
        
        [Fact]
        public async Task Should_Get_User_Videos_List()
        {
            // Arrange
            var userId = GetTestUserId();
            
            // 创建多个视频
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "视频1", userId));
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "视频2", userId));
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "视频3", userId));
            
            // Act
            var videos = await _videoRepository.GetByUserIdAsync(userId, null, 10, 0);
            
            // Assert
            videos.Count.ShouldBe(3);
        }
        
        [Fact]
        public async Task Should_Get_Videos_By_Status()
        {
            // Arrange
            var userId = GetTestUserId();
            
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "草稿视频", userId)
            {
                Status = VideoStatus.Draft
            });
            
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "已发布视频", userId)
            {
                Status = VideoStatus.Published
            });
            
            // Act
            var draftVideos = await _videoRepository.GetByUserIdAsync(userId, VideoStatus.Draft, 10, 0);
            var publishedVideos = await _videoRepository.GetByUserIdAsync(userId, VideoStatus.Published, 10, 0);
            
            // Assert
            draftVideos.Count.ShouldBe(1);
            draftVideos[0].Status.ShouldBe(VideoStatus.Draft);
            
            publishedVideos.Count.ShouldBe(1);
            publishedVideos[0].Status.ShouldBe(VideoStatus.Published);
        }
        
        [Fact]
        public async Task Should_Get_Hot_Videos()
        {
            // Arrange
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "热门视频1", GetTestUserId())
            {
                Status = VideoStatus.Published,
                TotalViews = 100000
            });
            
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "热门视频2", GetTestUserId())
            {
                Status = VideoStatus.Published,
                TotalViews = 50000
            });
            
            await _videoRepository.InsertAsync(new Video(GuidGenerator.Create(), "冷门视频", GetTestUserId())
            {
                Status = VideoStatus.Published,
                TotalViews = 10
            });
            
            // Act
            var hotVideos = await _videoRepository.GetHotVideosAsync(10);
            
            // Assert
            hotVideos.Count.ShouldBe(3);
            hotVideos[0].TotalViews.ShouldBeGreaterThan(hotVideos[1].TotalViews);  // 按播放量排序
        }
        
        [Fact]
        public async Task Should_Increment_View_Count_Atomic()
        {
            // Arrange
            var video = await _videoRepository.InsertAsync(
                new Video(GuidGenerator.Create(), "原子测试视频", GetTestUserId())
            );
            
            var initialViews = video.TotalViews;
            
            // Act（模拟并发增加播放量）
            await _videoRepository.IncrementViewCountAsync(video.Id);
            await _videoRepository.IncrementViewCountAsync(video.Id);
            await _videoRepository.IncrementViewCountAsync(video.Id);
            
            // Assert
            var updatedVideo = await _videoRepository.GetAsync(video.Id);
            updatedVideo.TotalViews.ShouldBe(initialViews + 3);
        }
    }
}
```

---

## 6. E2E测试设计（关键业务流程）

### 6.1 用户注册登录流程测试

```csharp
// UserRegistrationE2ETests.cs
using Xunit;
using Shouldly;
using System.Net.Http;
using System.Net.Http.Json;

namespace LCH.MicroService.Tests.E2E
{
    /// <summary>
    /// 用户注册登录E2E测试（关键流程）
    /// </summary>
    public class UserRegistrationE2ETests : AbpWebApplicationFactory<IdentityServerModule>
    {
        private readonly HttpClient _client;
        
        public UserRegistrationE2ETests()
        {
            _client = CreateClient();
        }
        
        [Fact]
        public async Task Should_Register_And_Login_Successfully()
        {
            // Step 1: 注册用户
            var registerRequest = new
            {
                userName = "testuser",
                email = "test@example.com",
                password = "Test@123456"
            };
            
            var registerResponse = await _client.PostAsJsonAsync(
                "/api/identity/register",
                registerRequest
            );
            
            registerResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var registerResult = await registerResponse.Content.ReadFromJsonAsync<RegistrationResultDto>();
            registerResult.UserId.ShouldNotBeNull();
            
            // Step 2: 登录用户
            var loginRequest = new
            {
                userNameOrEmail = "testuser",
                password = "Test@123456"
            };
            
            var loginResponse = await _client.PostAsJsonAsync(
                "/api/identity/login",
                loginRequest
            );
            
            loginResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResultDto>();
            loginResult.AccessToken.ShouldNotBeNull();
            loginResult.RefreshToken.ShouldNotBeNull();
            
            // Step 3: 使用Token访问用户信息
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);
            
            var profileResponse = await _client.GetAsync("/api/bilibili/users/profile");
            profileResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var profileResult = await profileResponse.Content.ReadFromJsonAsync<UserProfileDto>();
            profileResult.Level.ShouldBe(0);  // 新注册用户LV0
        }
    }
}
```

---

### 6.2 视频上传播放流程测试

```csharp
// VideoUploadPlayE2ETests.cs
using Xunit;
using Shouldly;

namespace LCH.MicroService.Tests.E2E
{
    /// <summary>
    /// 视频上传播放E2E测试（关键流程）
    /// </summary>
    public class VideoUploadPlayE2ETests : AbpWebApplicationFactory<VideoServiceModule>
    {
        private readonly HttpClient _client;
        
        public VideoUploadPlayE2ETests()
        {
            _client = CreateClient();
        }
        
        [Fact]
        public async Task Should_Upload_And_Play_Video_Successfully()
        {
            // Step 1: 初始化上传会话
            var initUploadRequest = new
            {
                fileName = "test-video.mp4",
                fileSize = 10240000L,
                fileMd5 = "abc123def456"
            };
            
            var initResponse = await _client.PostAsJsonAsync(
                "/api/video/upload/init",
                initUploadRequest
            );
            
            initResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var uploadSession = await initResponse.Content.ReadFromJsonAsync<UploadSessionDto>();
            uploadSession.UploadId.ShouldNotBeNull();
            uploadSession.ChunkCount.ShouldBeGreaterThan(0);
            
            // Step 2: 上传分片（模拟上传3个分片）
            for (int i = 0; i < uploadSession.ChunkCount && i < 3; i++)
            {
                var chunkContent = new ByteArrayContent(new byte[uploadSession.ChunkSize]);
                var chunkResponse = await _client.PutAsync(
                    uploadSession.UploadUrls[i].UploadUrl,
                    chunkContent
                );
                
                chunkResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            }
            
            // Step 3: 完成上传
            var completeRequest = new
            {
                fileName = "test-video.mp4",
                fileMd5 = "abc123def456",
                chunkCount = uploadSession.ChunkCount
            };
            
            var completeResponse = await _client.PostAsJsonAsync(
                $"/api/video/upload/complete/{uploadSession.UploadId}",
                completeRequest
            );
            
            completeResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var completeResult = await completeResponse.Content.ReadFromJsonAsync<CompleteUploadResultDto>();
            completeResult.VideoId.ShouldNotBeNull();
            
            // Step 4: 创建视频信息
            var createVideoRequest = new
            {
                uploadId = uploadSession.UploadId,
                title = "E2E测试视频",
                description = "这是一个E2E测试视频",
                categoryId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                tags = new[] { "测试", "E2E" },
                isOriginal = true
            };
            
            var createResponse = await _client.PostAsJsonAsync(
                "/api/video/videos",
                createVideoRequest
            );
            
            createResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var videoResult = await createResponse.Content.ReadFromJsonAsync<VideoDto>();
            videoResult.Status.ShouldBe("Transcoding");  // 状态：转码中
            
            // Step 5: 模拟转码完成（等待或手动触发）
            await WaitForTranscodingComplete(videoResult.Id, TimeSpan.FromSeconds(30));
            
            // Step 6: 获取视频详情并播放
            var detailResponse = await _client.GetAsync($"/api/video/videos/{videoResult.Id}");
            detailResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var detailResult = await detailResponse.Content.ReadFromJsonAsync<VideoDetailDto>();
            detailResult.HasTranscoded.ShouldBeTrue();
            detailResult.MasterPlaylist.ShouldNotBeNull();
            
            // Step 7: 记录播放行为
            var playRecordRequest = new
            {
                playDuration = 120,
                videoDuration = 120,
                playProgress = 95,
                playedResolution = "720P",
                deviceType = "Web"
            };
            
            var playResponse = await _client.PostAsJsonAsync(
                $"/api/video/videos/{videoResult.Id}/play-record",
                playRecordRequest
            );
            
            playResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            
            var playResult = await playResponse.Content.ReadFromJsonAsync<PlayRecordDto>();
            playResult.ExperienceEarned.ShouldBe(1);  // 完整观看+1经验
        }
        
        private async Task WaitForTranscodingComplete(Guid videoId, TimeSpan timeout)
        {
            var startTime = DateTime.UtcNow;
            
            while (DateTime.UtcNow - startTime < timeout)
            {
                var response = await _client.GetAsync($"/api/video/videos/{videoId}/transcode-progress");
                var progress = await response.Content.ReadFromJsonAsync<TranscodeProgressDto>();
                
                if (progress.Status == "Completed")
                {
                    return;
                }
                
                await Task.Delay(5000);
            }
            
            throw new TimeoutException("转码超时未完成");
        }
    }
}
```

---

## 7. 测试工具配置

### 7.1 xUnit配置

```json
// xunit.runner.json
{
  "$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
  "shadowCopy": false,
  "parallelizeAssembly": true,
  "parallelizeTestCollections": true,
  "maxParallelThreads": 4
}
```

---

### 7.2 Coverlet配置（代码覆盖率）

```xml
// Directory.Build.props
<Project>
  <PropertyGroup>
    <CollectCoverage>true</CollectCoverage>
    <CoverletOutputFormat>opencover</CoverletOutputFormat>
    <CoverletOutputDir>$(SolutionDir)coverage\</CoverletOutputDir>
    <Threshold>80</Threshold>  <!-- 目标覆盖率80% -->
  </PropertyGroup>
</Project>
```

---

## 8. 测试执行脚本

### 8.1 单元测试执行脚本

```bash
# scripts/run-unit-tests.sh

#!/bin/bash

echo "========== 运行单元测试 =========="

# 运行所有单元测试并生成覆盖率报告
dotnet test aspnet-core/test/ \
    --configuration Release \
    --collect:"XPlat Code Coverage" \
    --results-directory ./coverage \
    --verbosity normal

echo "========== 单元测试完成 =========="
echo "覆盖率报告已生成到 ./coverage/"
```

---

### 8.2 集成测试执行脚本

```bash
# scripts/run-integration-tests.sh

#!/bin/bash

echo "========== 运行集成测试 =========="

# 运行集成测试（需要测试数据库）
dotnet test aspnet-core/test/LCH.MicroService.Video.Tests/ \
    --configuration Release \
    --filter "FullyQualifiedName~IntegrationTests" \
    --verbosity normal

dotnet test aspnet-core/test/LCH.MicroService.Danmaku.Tests/ \
    --configuration Release \
    --filter "FullyQualifiedName~IntegrationTests" \
    --verbosity normal

echo "========== 集成测试完成 =========="
```

---

### 8.3 E2E测试执行脚本

```bash
# scripts/run-e2e-tests.sh

#!/bin/bash

echo "========== 运行E2E测试 =========="

# 启动测试环境（Docker）
docker-compose -f docker-compose.test.yml up -d

# 等待服务启动
sleep 30

# 运行E2E测试
dotnet test aspnet-core/test/LCH.MicroService.Tests.E2E/ \
    --configuration Release \
    --verbosity normal

# 清理测试环境
docker-compose -f docker-compose.test.yml down

echo "========== E2E测试完成 =========="
```

---

## 9. 测试覆盖率目标

| 模块 | 单元测试覆盖率 | 集成测试覆盖率 | E2E测试 |
|------|---------------|---------------|--------|
| VideoService | 80% | 60% | ✅ 上传播放流程 |
| DanmakuService | 80% | 70% | ✅ 弹幕发送流程 |
| InteractionService | 80% | 60% | ✅ 点赞投币流程 |
| UserService | 85% | 70% | ✅ 注册登录流程 |
| SearchService | 70% | 50% | ⚠️ 搜索流程（可选） |
| RecommendService | 60% | 40% | ⚠️ 推荐流程（可选） |
| LiveService | 75% | 60% | ✅ 直播开播流程 |
| CategoryService | 80% | 50% | ⚠️ 分区管理（可选） |
| AdminService | 70% | 50% | ✅ 审核流程 |

---

## 10. 测试最佳实践

### 10.1 ABP测试最佳实践

| 实践项 | 说明 |
|--------|------|
| **继承TestBase** | 所有测试继承`AbpIntegratedTest` |
| **使用InMemory数据库** | 单元测试使用`EntityFrameworkInMemoryDatabase` |
| **模拟外部依赖** | 使用Mock模拟BlobContainer、EventBus等 |
| **种子数据** | 在TestModule中统一注入种子数据 |
| **异步测试** | 所有测试方法使用`async/await` |
| **Shouldly断言** | 使用Shouldly库进行断言（比Assert更清晰） |

---

## 11. 测试报告配置

### 11.1 测试报告生成

```bash
# 生成覆盖率报告（使用ReportGenerator）
dotnet tool install -g dotnet-reportgenerator-globaltool

reportgenerator \
    -reports:./coverage/*.xml \
    -targetdir:./coverage/report \
    -reporttypes:HtmlInline_AzurePipelines;Cobertura

echo "覆盖率报告已生成: ./coverage/report/index.html"
```

---

## 12. 总结

### 12.1 测试策略特点

| 特点 | 说明 |
|------|------|
| **继承ABP TestBase** | 使用现有ABP测试架构 |
| **分层测试策略** | 单元测试(80%) + 集成测试(15%) + E2E测试(5%) |
| **自动化执行** | 提供完整测试执行脚本 |
| **覆盖率监控** | 使用Coverlet生成覆盖率报告 |
| **关键流程E2E测试** | 注册登录、视频上传播放、弹幕发送 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 测试策略文档完成