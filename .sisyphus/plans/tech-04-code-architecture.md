# 代码架构设计文档（结合现有项目）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 代码架构设计 |
| 目标项目 | Bilibili视频平台 |
| 框架 | ABP Framework v10.0.2 + .NET 10.0 |
| 创建时间 | 2026-05-12 |

---

## 2. ABP框架代码架构分析

### 2.1 现有项目模块结构

基于探索结果，现有项目采用ABP标准的模块化架构：

```
aspnet-core/modules/{module-name}/
├── Domain/                          # 领域层
│   ├── Entities/                    # 实体类
│   ├── Repositories/                # Repository接口
│   ├── Services/                    # 领域服务
│   └── {Module}DomainModule.cs      # 领域模块定义
│
├── Domain.Shared/                   # 领域共享层
│   ├── Enums/                       # 枚举定义
│   ├── Constants/                   # 常量定义
│   └─ {Module}DomainSharedModule.cs
│
├── Application.Contracts/           # 应用服务契约层
│   ├── Dtos/                        # DTO定义
│   ├── Services/                    # 应用服务接口
│   └─ {Module}ApplicationContractsModule.cs
│
├── Application/                     # 应用服务层
│   ├── Services/                    # 应用服务实现
│   ├── AutoMapper/                  # 对象映射配置
│   └─ {Module}ApplicationModule.cs
│
├── HttpApi/                         # HTTP API层
│   ├── Controllers/                 # API控制器
│   └─ {Module}HttpApiModule.cs
│
├── HttpApi.Client/                  # HTTP API客户端层
│   ├── ClientProxies/               # 动态客户端代理
│   └─ {Module}HttpApiClientModule.cs
│
└── EntityFrameworkCore/             # EF Core数据访问层
│   ├── DbContexts/                  # DbContext定义
│   ├── Entities/                    # 实体配置
│   ├── Repositories/                # Repository实现
│   └─ {Module}EntityFrameworkCoreModule.cs
```

---

## 3. Bilibili新模块代码架构设计

### 3.1 VideoService代码结构

#### 3.1.1 目录结构

```
aspnet-core/services/LCH.MicroService.Video.HttpApi.Host/
├── Controllers/                     # API控制器（继承ABP Controller）
│   ├── VideoController.cs
│   ├── UploadController.cs
│   └─ TranscodeController.cs
│
├── EventBus/                        # 事件总线（继承CAP）
│   ├── VideoCreatedEventHandler.cs
│   ├── TranscodeCompletedEventHandler.cs
│   └ VideoViewCountIncrementEventHandler.cs
│
├── Program.cs                       # 应用入口（继承ABP Program）
├── VideoServiceModule.cs            # 模块定义（继承ABP Module）
├── appsettings.json                 # 配置文件
├── Dockerfile                       # Docker配置
│
aspnet-core/modules/video/
├── Domain/                          # 领域层
│   ├── Entities/
│   │   ├── Video.cs                 # 视频实体（继承FullAuditedAggregateRoot）
│   │   ├── VideoUploadSession.cs    # 上传会话实体
│   │   ├── VideoTranscodeTask.cs    # 转码任务实体
│   │   ├── VideoTag.cs              # 视频标签实体
│   │   └ VideoPlayRecord.cs         # 播放记录实体
│   │
│   ├── Repositories/
│   │   ├── IVideoRepository.cs      # 视频Repository接口（继承IRepository）
│   │   ├── IUploadSessionRepository.cs
│   │   └─ ITranscodeTaskRepository.cs
│   │
│   ├── Services/
│   │   ├── IUploadSessionManager.cs # 上传会话管理领域服务
│   │   ├── ITranscodeTaskManager.cs # 转码任务管理领域服务
│   │   └ VideoDomainService.cs      # 视频领域服务
│   │
│   ├── Enums/
│   │   ├── VideoStatus.cs           # 视频状态枚举
│   │   ├── AuditStatus.cs           # 审核状态枚举
│   │   └ ResolutionType.cs          # 清晰度枚举
│   │
│   ├── Constants/
│   │   ├── VideoConstants.cs        # 视频常量
│   │   └ VideoPermissions.cs        # 视频权限定义
│   │
│   └─ VideoDomainModule.cs          # 领域模块定义
│
├── Domain.Shared/
│   ├── Enums/
│   │   ├── VideoStatus.cs           # 枚举定义（共享）
│   │   └─ ResolutionType.cs
│   │
│   ├── Constants/
│   │   └─ VideoDomainSharedConstants.cs
│   │
│   └─ VideoDomainSharedModule.cs
│
├── Application.Contracts/
│   ├── Dtos/
│   │   ├── VideoDto.cs              # 视频DTO（继承EntityDto）
│   │   ├── VideoDetailDto.cs        # 视频详情DTO
│   │   ├── CreateVideoDto.cs        # 创建视频DTO
│   │   ├── UpdateVideoDto.cs        # 更新视频DTO
│   │   ├── GetVideoListDto.cs       # 查询列表DTO（继承PagedAndSortedResultRequestDto）
│   │   ├── UploadSessionDto.cs
│   │   ├── TranscodeTaskDto.cs
│   │   └─ PlayUrlDto.cs
│   │
│   ├── Services/
│   │   ├── IVideoAppService.cs      # 视频应用服务接口（继承IApplicationService）
│   │   ├── IUploadAppService.cs
│   │   └─ ITranscodeAppService.cs
│   │
│   └─ VideoApplicationContractsModule.cs
│
├── Application/
│   ├── Services/
│   │   ├── VideoAppService.cs       # 视频应用服务（继承ApplicationService）
│   │   ├── UploadAppService.cs
│   │   ├── TranscodeAppService.cs
│   │   └─ VideoQueryAppService.cs   # 视频查询服务
│   │
│   ├── AutoMapper/
│   │   ├── VideoAutoMapperProfile.cs # AutoMapper配置
│   │
│   ├── EventBus/
│   │   ├── VideoCreatedEvent.cs     # 视频创建事件
│   │   ├── TranscodeCompletedEvent.cs
│   │
│   └─ VideoApplicationModule.cs     # 应用模块定义
│
├── HttpApi/
│   ├── Controllers/
│   │   ├── VideoController.cs       # 视频控制器（继承AbpController）
│   │   ├── UploadController.cs
│   │   ├── TranscodeController.cs
│   │
│   ├── Models/
│   │   ├── UploadChunkRequest.cs    # 上传分片请求模型
│   │
│   └─ VideoHttpApiModule.cs         # HTTP API模块定义
│
├── HttpApi.Client/
│   ├── ClientProxies/
│   │   ├── VideoAppServiceClientProxy.cs # 动态客户端代理
│   │
│   └─ VideoHttpApiClientModule.cs
│
└─ EntityFrameworkCore/
    ├── DbContexts/
    │   ├── IVideoDbContext.cs       # DbContext接口
    │   └─ VideoDbContext.cs         # DbContext实现（继承AbpDbContext）
    │
    ├── Entities/
    │   ├── VideoEntityTypeConfiguration.cs # 实体配置
    │   ├── VideoUploadSessionEntityTypeConfiguration.cs
    │
    ├── Repositories/
    │   ├── VideoRepository.cs       # 视频Repository实现（继承EfCoreRepository）
    │   ├── UploadSessionRepository.cs
    │   ├── TranscodeTaskRepository.cs
    │
    ├── Migrations/
    │   ├── 20260512_InitialVideoSchema.cs
    │   ├── VideoDbContextModelSnapshot.cs
    │
    └─ VideoEntityFrameworkCoreModule.cs
```

---

#### 3.1.2 核心代码示例

**Video实体类**（继承ABP Entity）:

```csharp
// Video.cs
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace LCH.MicroService.Video.Domain.Entities
{
    /// <summary>
    /// 视频实体（继承ABP FullAuditedAggregateRoot）
    /// 支持审计、软删除、多租户
    /// </summary>
    public class Video : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        public Guid? TenantId { get; set; } // 多租户支持
        
        // 基础信息
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid? CategoryId { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        
        // 视频文件信息
        public string OriginalFileUrl { get; set; }
        public int? DurationSeconds { get; set; }
        public string HlsMasterUrl { get; set; }
        
        // 封面信息
        public string CoverImageUrl { get; set; }
        
        // 状态
        public VideoStatus Status { get; set; }
        public AuditStatus AuditStatus { get; set; }
        public Guid? AuditorId { get; set; }
        public string AuditReason { get; set; }
        
        // 发布信息
        public DateTime? PublishTime { get; set; }
        public bool IsPublished { get; set; }
        public bool IsOriginal { get; set; }
        
        // 统计信息
        public long TotalViews { get; set; }
        public int TotalLikes { get; set; }
        public int TotalCoins { get; set; }
        public int TotalComments { get; set; }
        public long DanmakuCount { get; set; }
        
        // 导航属性
        public ICollection<VideoTranscodeTask> TranscodeTasks { get; set; }
        
        // 构造函数
        protected Video() { } // ORM构造函数
        
        public Video(Guid id, string title, Guid userId) : base(id)
        {
            Title = title;
            UserId = userId;
            Status = VideoStatus.Draft;
            AuditStatus = AuditStatus.Pending;
            IsOriginal = true;
        }
        
        // 业务方法
        public void Publish()
        {
            Status = VideoStatus.Published;
            PublishTime = DateTime.UtcNow;
            IsPublished = true;
        }
        
        public void IncrementViewCount()
        {
            TotalViews++;
        }
    }
}
```

**VideoRepository**（继承ABP Repository）:

```csharp
// VideoRepository.cs
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LCH.MicroService.Video.Domain.Entities;
using LCH.MicroService.Video.Domain.Repositories;

namespace LCH.MicroService.Video.EntityFrameworkCore.Repositories
{
    /// <summary>
    /// 视频Repository实现（继承ABP EfCoreRepository）
    /// </summary>
    public class VideoRepository : EfCoreRepository<VideoDbContext, Video, Guid>, IVideoRepository
    {
        public VideoRepository(IDbContextProvider<VideoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
        
        /// <summary>
        /// 获取用户视频列表
        /// </summary>
        public async Task<List<Video>> GetByUserIdAsync(
            Guid userId,
            VideoStatus? status = null,
            int maxResultCount = 10,
            int skipCount = 0)
        {
            var dbSet = await GetDbSetAsync();
            
            var query = dbSet
                .Where(v => v.UserId == userId);
            
            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }
            
            return await query
                .OrderByDescending(v => v.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }
        
        /// <summary>
        /// 增加播放量（原子操作）
        /// </summary>
        public async Task IncrementViewCountAsync(Guid videoId)
        {
            var dbContext = await GetDbContextAsync();
            
            // 使用EF Core ExecuteSqlRaw进行原子更新
            await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE \"Videos\" SET \"TotalViews\" = \"TotalViews\" + 1 WHERE \"Id\" = {0}",
                videoId);
        }
        
        /// <summary>
        /// 获取热门视频
        /// </summary>
        public async Task<List<Video>> GetHotVideosAsync(int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(v => v.Status == VideoStatus.Published)
                .OrderByDescending(v => v.TotalViews)
                .Take(maxResultCount)
                .ToListAsync();
        }
    }
}
```

**VideoAppService**（继承ABP ApplicationService）:

```csharp
// VideoAppService.cs
using Volo.Abp.Application.Services;
using Volo.Abp.ObjectMapping;
using LCH.MicroService.Video.Domain.Entities;
using LCH.MicroService.Video.Domain.Repositories;
using LCH.MicroService.Video.Application.Contracts.Dtos;
using LCH.MicroService.Video.Application.Contracts.Services;

namespace LCH.MicroService.Video.Application.Services
{
    /// <summary>
    /// 视频应用服务（继承ABP ApplicationService）
    /// </summary>
    public class VideoAppService : ApplicationService, IVideoAppService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IBlobContainer _blobContainer; // 使用现有MinIO BlobContainer
        private readonly IDistributedEventBus _eventBus; // 使用现有CAP EventBus
        
        public VideoAppService(
            IVideoRepository videoRepository,
            IBlobContainer blobContainer,
            IDistributedEventBus eventBus)
        {
            _videoRepository = videoRepository;
            _blobContainer = blobContainer;
            _eventBus = eventBus;
        }
        
        /// <summary>
        /// 获取视频详情
        /// </summary>
        public async Task<VideoDetailDto> GetAsync(Guid id)
        {
            var video = await _videoRepository.GetAsync(id);
            
            // 使用ABP ObjectMapper进行DTO映射
            var dto = ObjectMapper.Map<Video, VideoDetailDto>(video);
            
            return dto;
        }
        
        /// <summary>
        /// 创建视频
        /// </summary>
        [Authorize(VideoPermissions.Create)]
        public async Task<VideoDto> CreateAsync(CreateVideoDto input)
        {
            // 使用ABP GuidGenerator生成ID
            var video = new Video(
                GuidGenerator.Create(),
                input.Title,
                CurrentUser.Id.Value
            );
            
            video.Description = input.Description;
            video.CategoryId = input.CategoryId;
            video.Tags = input.Tags;
            video.IsOriginal = input.IsOriginal;
            
            // 保存到数据库
            video = await _videoRepository.InsertAsync(video);
            
            // 发布视频创建事件到RabbitMQ（使用CAP）
            await _eventBus.PublishAsync(new VideoCreatedEto
            {
                VideoId = video.Id,
                UserId = video.UserId,
                Title = video.Title,
                CreationTime = video.CreationTime
            });
            
            return ObjectMapper.Map<Video, VideoDto>(video);
        }
        
        /// <summary>
        /// 记录播放行为
        /// </summary>
        [Authorize]
        public async Task<PlayRecordDto> RecordPlayAsync(Guid videoId, RecordPlayDto input)
        {
            // 增加播放量（原子操作）
            await _videoRepository.IncrementViewCountAsync(videoId);
            
            // 完整观看（90%+），给用户+1经验
            if (input.PlayProgress >= 90)
            {
                // 发布经验事件
                await _eventBus.PublishAsync(new UserExperienceEto
                {
                    UserId = CurrentUser.Id.Value,
                    ExperienceType = "WATCH_VIDEO",
                    ExperienceAmount = 1,
                    SourceId = videoId,
                    Description = "完整观看视频"
                });
            }
            
            return new PlayRecordDto
            {
                VideoId = videoId,
                UserId = CurrentUser.Id.Value,
                PlayDuration = input.PlayDuration,
                ExperienceEarned = input.PlayProgress >= 90 ? 1 : 0
            };
        }
    }
}
```

**VideoController**（继承ABP Controller）:

```csharp
// VideoController.cs
using Volo.Abp.AspNetCore.Mvc;
using LCH.MicroService.Video.Application.Contracts.Dtos;
using LCH.MicroService.Video.Application.Contracts.Services;

namespace LCH.MicroService.Video.HttpApi.Controllers
{
    /// <summary>
    /// 视频API控制器（继承ABP AbpController）
    /// </summary>
    [ApiController]
    [Route("api/video/videos")]
    public class VideoController : AbpController
    {
        private readonly IVideoAppService _videoAppService;
        
        public VideoController(IVideoAppService videoAppService)
        {
            _videoAppService = videoAppService;
        }
        
        /// <summary>
        /// 获取视频详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<VideoDetailDto> GetAsync(Guid id)
        {
            return await _videoAppService.GetAsync(id);
        }
        
        /// <summary>
        /// 创建视频
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<VideoDto> CreateAsync(CreateVideoDto input)
        {
            return await _videoAppService.CreateAsync(input);
        }
        
        /// <summary>
        /// 记录播放行为
        /// </summary>
        [HttpPost("{id}/play-record")]
        [Authorize]
        public async Task<PlayRecordDto> RecordPlayAsync(Guid id, RecordPlayDto input)
        {
            return await _videoAppService.RecordPlayAsync(id, input);
        }
    }
}
```

---

### 3.2 DanmakuService代码结构

```
aspnet-core/services/LCH.MicroService.Danmaku.HttpApi.Host/
├── Controllers/
│   └── DanmakuController.cs
│
├── SignalR/
│   ├── Hubs/
│   │   └── DanmakuHub.cs           # 弹幕Hub（继承AbpHub）
│   ├── Models/
│   │   ├── SendDanmakuMessage.cs
│   │   └── DanmakuMessage.cs
│   └─ DanmakuSignalRModule.cs
│
├── DanmakuServiceModule.cs
│
aspnet-core/modules/danmaku/
├── Domain/
│   ├── Entities/
│   │   ├── Danmaku.cs              # 弹幕实体（分区表）
│   │   ├── DanmakuReport.cs        # 弹幕举报实体
│   │
│   ├── Repositories/
│   │   ├── IDanmakuRepository.cs
│   │   └─ IDanmakuReportRepository.cs
│   │
│   ├── Services/
│   │   ├── IDanmakuManager.cs      # 弹幕管理领域服务
│   │
│   └─ DanmakuDomainModule.cs
│
├── Application.Contracts/
│   ├── Dtos/
│   │   ├── DanmakuDto.cs
│   │   ├── CreateDanmakuDto.cs
│   │   ├── GetDanmakuListDto.cs
│   │
│   ├── Services/
│   │   ├── IDanmakuAppService.cs
│   │
│   └─ DanmakuApplicationContractsModule.cs
│
├── Application/
│   ├── Services/
│   │   ├── DanmakuAppService.cs    # 弹幕应用服务
│   │
│   └─ DanmakuApplicationModule.cs
│
├── HttpApi/
│   ├── Controllers/
│   │   ├── DanmakuController.cs
│   │
│   └─ DanmakuHttpApiModule.cs
│
└─ EntityFrameworkCore/
    ├── DbContexts/
    │   ├── DanmakuDbContext.cs     # 弹幕DbContext（分区表配置）
    │
    ├── Repositories/
    │   ├── DanmakuRepository.cs
    │
    └─ DanmakuEntityFrameworkCoreModule.cs
```

**DanmakuHub**（继承ABP SignalR Hub）:

```csharp
// DanmakuHub.cs
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;
using LCH.MicroService.Danmaku.Application.Services;

namespace LCH.MicroService.Danmaku.SignalR.Hubs
{
    /// <summary>
    /// 弹幕SignalR Hub（继承ABP AbpHub）
    /// 使用Redis Scaleout实现跨服务器广播
    /// </summary>
    public class DanmakuHub : AbpHub
    {
        private readonly IDanmakuAppService _danmakuService;
        
        public DanmakuHub(IDanmakuAppService danmakuService)
        {
            _danmakuService = danmakuService;
        }
        
        /// <summary>
        /// 加入视频房间（使用SignalR Group）
        /// </summary>
        public async Task JoinVideoRoom(Guid videoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"video-{videoId}");
            
            // 发送最近100条弹幕
            var recentDanmakus = await _danmakuService.GetRecentAsync(videoId, 100);
            await Clients.Caller.SendAsync("RecentDanmakus", recentDanmakus);
        }
        
        /// <summary>
        /// 发送弹幕（需登录）
        /// </summary>
        public async Task SendDanmaku(SendDanmakuMessage message)
        {
            // 验证用户等级（高级弹幕需LV2+）
            if (message.DanmakuType == 3 && CurrentUser.Level < 2)
            {
                throw new UserFriendlyException("高级弹幕需要LV2以上等级");
            }
            
            // 创建弹幕
            var danmaku = await _danmakuService.CreateAsync(new CreateDanmakuDto
            {
                VideoId = message.VideoId,
                Content = message.Content,
                PositionTime = message.PositionTime,
                DanmakuType = message.DanmakuType,
                FontSize = message.FontSize,
                FontColor = message.FontColor
            });
            
            // 广播弹幕到所有观众（Redis Scaleout）
            await Clients.Group($"video-{message.VideoId}")
                .SendAsync("NewDanmaku", ObjectMapper.Map<Danmaku, DanmakuDto>(danmaku));
        }
        
        /// <summary>
        /// 退出视频房间
        /// </summary>
        public async Task LeaveVideoRoom(Guid videoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video-{videoId}");
        }
    }
}
```

---

### 3.3 InteractionService代码结构

```
aspnet-core/modules/interaction/
├── Domain/
│   ├── Entities/
│   │   ├── VideoLike.cs            # 点赞实体
│   │   ├── VideoCoin.cs            # 投币实体
│   │   ├── VideoFavorite.cs        # 收藏实体
│   │   ├── Comment.cs              # 评论实体
│   │   ├── VideoShare.cs           # 分享实体
│   │   ├── UserFollow.cs           # 关注实体
│   │
│   ├── Repositories/
│   │   ├── ILikeRepository.cs
│   │   ├── ICoinRepository.cs
│   │   ├── ICommentRepository.cs
│   │
│   └─ InteractionDomainModule.cs
│
├── Application.Contracts/
│   ├── Dtos/
│   │   ├── LikeDto.cs
│   │   ├── CoinDto.cs
│   │   ├── CommentDto.cs
│   │
│   ├── Services/
│   │   ├── ILikeAppService.cs
│   │   ├── ICoinAppService.cs
│   │   ├── ICommentAppService.cs
│   │
│   └─ InteractionApplicationContractsModule.cs
│
├── Application/
│   ├── Services/
│   │   ├── LikeAppService.cs
│   │   ├── CoinAppService.cs
│   │   ├── CommentAppService.cs   # 评论服务（人工审核）
│   │
│   └─ InteractionApplicationModule.cs
│
├── HttpApi/
│   ├── Controllers/
│   │   ├── LikeController.cs
│   │   ├── CoinController.cs
│   │   ├── CommentController.cs
│   │
│   └─ InteractionHttpApiModule.cs
│
└─ EntityFrameworkCore/
    ├── DbContexts/
    │   └─ InteractionDbContext.cs
    │
    ├── Repositories/
    │   ├── LikeRepository.cs
    │   ├── CoinRepository.cs
    │   ├── CommentRepository.cs
    │
    └─ InteractionEntityFrameworkCoreModule.cs
```

---

### 3.4 其他模块代码结构（简要）

#### UserService代码结构

```
aspnet-core/modules/bilibili-user/
├── Domain/
│   ├── Entities/
│   │   ├── BilibiliUser.cs         # 用户扩展实体（继承AbpUser）
│   │   ├── UserExperienceRecord.cs # 经验记录实体
│   │   ├── UserCoinRecord.cs       # B币记录实体
│   │   ├── UserFollow.cs           # 关注关系实体
│   │   ├── UserMessage.cs          # 消息实体
│   │
│   ├── Repositories/
│   │   ├── IUserExperienceRepository.cs
│   │   ├── IUserCoinRepository.cs
│   │
│   ├── Services/
│   │   ├── IUserLevelManager.cs    # 用户等级管理领域服务
│   │   ├── IUserCoinManager.cs     # 用户B币管理领域服务
│   │
│   └─ BilibiliUserDomainModule.cs
│
├── Application.Contracts/
│   ├── Dtos/
│   │   ├── UserProfileDto.cs
│   │   ├── UserLevelDto.cs
│   │   ├── UserExperienceDto.cs
│   │
│   ├── Services/
│   │   ├── IUserProfileAppService.cs
│   │   ├── IUserLevelAppService.cs
│   │
│   └─ BilibiliUserApplicationContractsModule.cs
│
├── Application/
│   ├── Services/
│   │   ├── UserProfileAppService.cs
│   │   ├── UserLevelAppService.cs
│   │   ├── UserCoinAppService.cs
│   │   ├── UserMessageAppService.cs
│   │
│   ├── EventBus/
│   │   ├── VideoWatchedEventHandler.cs    # 观看视频获得经验
│   │   ├── VideoCoinEventHandler.cs       # 投币消耗B币
│   │   ├── ReceiveCoinEventHandler.cs     # 收到投币获得B币
│   │
│   └─ BilibiliUserApplicationModule.cs
│
└─ EntityFrameworkCore/
    ├── DbContexts/
    │   └─ BilibiliUserDbContext.cs
    │
    └─ BilibiliUserEntityFrameworkCoreModule.cs
```

---

## 4. 跨模块通信架构（基于现有CAP）

### 4.1 事件总线配置

继承现有CAP配置：

```csharp
// VideoServiceModule.cs（继承ABP Module）
using Volo.Abp.Modularity;
using DotNetCore.CAP;

namespace LCH.MicroService.Video
{
    [DependsOn(
        typeof(AbpAspNetCoreMvcModule),
        typeof(AbpEntityFrameworkCorePostgreSqlModule),
        typeof(AbpEventBusRabbitMqModule) // 使用现有RabbitMQ模块
    )]
    public class VideoServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            
            // 配置CAP（继承现有RabbitMQ配置）
            context.Services.AddCap(x =>
            {
                x.UseRabbitMQ(options =>
                {
                    options.HostName = configuration["RabbitMQ:HostName"];
                    options.UserName = configuration["RabbitMQ:UserName"];
                    options.Password = configuration["RabbitMQ:Password"];
                });
                
                x.UsePostgreSQL<VideoDbContext>();
                
                x.PublishOptions.DefaultGroupName = "video-service";
            });
        }
        
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            
            app.UseAbpRequestLocalization();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseMvc();
        }
    }
}
```

---

### 4.2 事件定义

```csharp
// VideoCreatedEto.cs（Event Transfer Object）
using Volo.Abp.EventBus;

namespace LCH.MicroService.Video.Application.Contracts.Events
{
    /// <summary>
    /// 视频创建事件（用于跨模块通信）
    /// </summary>
    [EventName("Video.Created")]
    public class VideoCreatedEto
    {
        public Guid VideoId { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public DateTime CreationTime { get; set; }
    }
}

// TranscodeCompletedEto.cs
[EventName("Video.TranscodeCompleted")]
public class TranscodeCompletedEto
{
    public Guid VideoId { get; set; }
    public List<string> Resolutions { get; set; }
    public DateTime CompleteTime { get; set; }
}

// UserExperienceEto.cs（用户经验事件）
[EventName("User.Experience")]
public class UserExperienceEto
{
    public Guid UserId { get; set; }
    public string ExperienceType { get; set; }
    public int ExperienceAmount { get; set; }
    public Guid? SourceId { get; set; }
    public string Description { get; set; }
}
```

---

### 4.3 事件处理器

```csharp
// VideoCreatedEventHandler.cs（继承ABP IEventHandler）
using Volo.Abp.EventBus;
using DotNetCore.CAP;

namespace LCH.MicroService.Video.Application.EventHandlers
{
    /// <summary>
    /// 视频创建事件处理器（订阅事件）
    /// </summary>
    public class VideoCreatedEventHandler : IEventHandler<VideoCreatedEto>
    {
        private readonly ILogger<VideoCreatedEventHandler> _logger;
        
        [CapSubscribe("Video.Created")]
        public async Task HandleEventAsync(VideoCreatedEto eventData)
        {
            _logger.LogInformation($"视频创建事件: VideoId={eventData.VideoId}, UserId={eventData.UserId}");
            
            // 1. 通知搜索服务同步索引
            // 2. 通知推荐服务计算推荐
            // 3. 通知用户服务UP主发布视频
        }
    }
}

// UserExperienceEventHandler.cs（用户模块订阅）
public class UserExperienceEventHandler : IEventHandler<UserExperienceEto>
{
    private readonly IUserExperienceRepository _experienceRepository;
    
    [CapSubscribe("User.Experience")]
    public async Task HandleEventAsync(UserExperienceEto eventData)
    {
        // 用户模块处理经验事件
        // 1. 检查每日经验上限（100）
        // 2. 增加用户经验值
        // 3. 检查是否升级
        // 4. 记录经验来源
    }
}
```

---

## 5. 模块依赖关系图

```
┌─────────────────────────────────────────────────────────────┐
│                    ABP Framework核心模块                     │
│  AbpAspNetCoreMvc | AbpEntityFrameworkCore | AbpIdentity    │
└─────────────────────────────────────────────────────────────┘
                              ↑
                              │ 继承依赖
                              │
┌─────────────────────────────────────────────────────────────┐
│                   Bilibili业务模块层                         │
│                                                             │
│  ┌──────────────┬──────────────┬──────────────┬────────────┐ │
│  │ UserService  │ VideoService │ DanmakuService│ Interaction│ │
│  │ (用户扩展)   │ (视频管理)   │ (弹幕推送)   │ (互动行为) │ │
│  └──────────────┴──────────────┴──────────────┴────────────┘ │
│                              ↑                              │
│                              │ 事件依赖（RabbitMQ/CAP）      │
│                              │                              │
│  ┌──────────────┬──────────────┬──────────────┬────────────┐ │
│  │ SearchService│ RecommendSvc │ LiveService  │ CategorySvc │ │
│  │ (搜索)       │ (推荐)       │ (直播)       │ (分区)      │ │
│  └──────────────┴──────────────┴──────────────┴────────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
                              ↑
                              │ 基础设施依赖
                              │
┌─────────────────────────────────────────────────────────────┐
│                   基础设施模块层                             │
│                                                             │
│  ┌──────────────┬──────────────┬──────────────┬────────────┐ │
│  │ OssManagement│ RealtimeMsg  │ EventBus     │ Identity   │ │
│  │ (MinIO存储)  │ (SignalR)    │ (CAP/RabbitMQ│ (认证授权) │ │
│  └──────────────┴──────────────┴──────────────┴────────────┘ │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 6. 模块初始化顺序

### 6.1 ABP模块依赖配置

```csharp
// VideoServiceModule.cs
[DependsOn(
    // ABP核心模块
    typeof(AbpAspNetCoreMvcModule),
    typeof(AbpEntityFrameworkCorePostgreSqlModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpEventBusRabbitMqModule),
    
    // Bilibili业务模块
    typeof(BilibiliUserDomainModule),           // 用户模块（Domain层）
    typeof(BilibiliUserApplicationContractsModule), // 用户模块（Contracts层）
    
    // 现有基础设施模块
    typeof(OssManagementDomainModule),          // MinIO对象存储
    typeof(OssManagementApplicationContractsModule),
    typeof(IdentityDomainModule),               // 用户认证（继承ABP Identity）
    
    // 视频模块各层
    typeof(VideoDomainModule),
    typeof(VideoApplicationContractsModule),
    typeof(VideoApplicationModule),
    typeof(VideoEntityFrameworkCoreModule),
    typeof(VideoHttpApiModule)
)]
public class VideoServiceModule : AbpModule
{
    // ...
}
```

---

## 7. 代码规范总结

### 7.1 ABP代码规范

| 规范项 | 要求 | 示例 |
|--------|------|------|
| **实体继承** | 继承`FullAuditedAggregateRoot<Guid>` | `public class Video : FullAuditedAggregateRoot<Guid>` |
| **Repository继承** | 继承`EfCoreRepository<TDbContext, TEntity, TKey>` | `public class VideoRepository : EfCoreRepository<VideoDbContext, Video, Guid>` |
| **AppService继承** | 继承`ApplicationService` | `public class VideoAppService : ApplicationService` |
| **Controller继承** | 继承`AbpController` | `public class VideoController : AbpController` |
| **DTO命名** | `{Entity}Dto`, `Create{Entity}Dto` | `VideoDto`, `CreateVideoDto` |
| **Service命名** | `I{Entity}AppService` | `IVideoAppService` |
| **Module命名** | `{Module}DomainModule` | `VideoDomainModule` |

---

## 8. 开发流程

### 8.1 创建新模块流程

```
步骤1: 创建Domain层
├── 定义实体类（继承ABP Entity）
├── 定义Repository接口（继承IRepository）
├── 定义领域服务（领域逻辑）
└── 创建DomainModule

步骤2: 创建Domain.Shared层
├── 定义枚举（跨层共享）
├── 定义常量
└── 创建DomainSharedModule

步骤3: 创建Application.Contracts层
├── 定义DTO（继承ABP DTO基类）
├── 定义AppService接口（继承IApplicationService）
├── 定义事件（Event Transfer Object）
└── 创建ApplicationContractsModule

步骤4: 创建Application层
├── 实现AppService（继承ApplicationService）
├── 配置AutoMapper
├── 实现事件处理器
├── 创建ApplicationModule

步骤5: 创建EntityFrameworkCore层
├── 创建DbContext（继承AbpDbContext）
├── 实现Repository（继承EfCoreRepository）
├── 配置实体映射
├── 创建迁移
├── 创建EntityFrameworkCoreModule

步骤6: 创建HttpApi层
├── 创建Controller（继承AbpController）
├── 配置路由
├── 创建HttpApiModule

步骤7: 创建HttpApi.Host服务
├── 创建Program.cs（继承ABP Program）
├── 创建Module（依赖所有层模块）
├── 配置appsettings.json
├── 配置Dockerfile
```

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 代码架构设计完成