# 代码架构设计文档（结合现有ABP项目）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 代码架构设计 |
| 目标项目 | Bilibili视频平台 |
| 架构模式 | ABP DDD分层架构 |
| 创建时间 | 2026-05-12 |

---

## 2. ABP Framework分层架构（继承现有）

### 2.1 现有项目分层结构

基于对现有项目的探索，项目采用标准的ABP DDD分层架构：

```
现有项目结构（aspnet-core/modules/）：
├── identity/                     # 身份认证模块
│   ├── Domain/                   # 领域层
│   ├── Domain.Shared/            # 共享领域层
│   ├── Application.Contracts/    # 应用服务契约
│   ├── Application/              # 应用服务层
│   ├── HttpApi/                  # HTTP API层
│   ├── HttpApi.Client/           # HTTP客户端
│   └── EntityFrameworkCore/      # 数据持久化
│
├── oss-management/               # 对象存储模块
├── realtime-message/             # 实时消息模块
├── platform/                     # 平台模块
└── ...（其他模块）
```

**结论**: 我们将继承此架构模式，为Bilibili新增模块创建相同结构。

---

## 3. VideoService代码架构设计

### 3.1 项目结构

```
aspnet-core/modules/video/
├── LCH.MicroService.Video.Domain.Shared/
│   ├── VideoModuleConstants.cs              # 模块常量
│   ├── Enums/
│   │   ├── VideoStatus.cs                   # 视频状态枚举
│   │   ├── AuditStatus.cs                   # 审核状态枚举
│   │   └── TranscodeStatus.cs               # 转码状态枚举
│   └── Localization/
│       └ VideoLocalizationKeys.cs           # 本地化键
│
├── LCH.MicroService.Video.Domain/
│   ├── VideoDomainModule.cs                 # 领域模块定义
│   ├── Entities/
│   │   ├── Video.cs                         # 视频实体
│   │   ├── VideoUploadSession.cs            # 上传会话实体
│   │   ├── VideoTranscodeTask.cs            # 转码任务实体
│   │   ├── VideoTag.cs                      # 标签实体
│   │   ├── VideoPlayRecord.cs               # 播放记录实体
│   │   └── Category.cs                      # 分类实体
│   ├── Repositories/
│   │   ├── IVideoRepository.cs              # 视频仓储接口
│   │   ├── ICategoryRepository.cs           # 分类仓储接口
│   │   └── IUploadSessionRepository.cs      # 上传会话仓储
│   ├── Services/
│   │   ├── IVideoManager.cs                 # 视频管理领域服务
│   │   ├── IUploadSessionManager.cs         # 上传会话管理
│   │   └── ITranscodeManager.cs             # 转码管理（调用FFmpeg）
│   ├── Events/
│   │   ├── VideoCreatedEvent.cs             # 视频创建事件
│   │   ├── VideoPublishedEvent.cs           # 视频发布事件
│   │   ├── VideoTranscodeCompletedEvent.cs  # 转码完成事件
│   │   └ VideoUploadedEvent.cs              # 上传完成事件
│   └─ Settings/
│       └ VideoSettingDefinitionProvider.cs  # 视频设置定义
│
├── LCH.MicroService.Video.Application.Contracts/
│   ├── VideoApplicationContractsModule.cs   # 契约模块定义
│   ├── Dtos/
│   │   ├── VideoDto.cs                      # 视频DTO
│   │   ├── CreateVideoDto.cs                # 创建视频DTO
│   │   ├── UpdateVideoDto.cs                # 更新视频DTO
│   │   ├── VideoUploadResultDto.cs          # 上传结果DTO
│   │   ├── VideoDetailDto.cs                # 视频详情DTO
│   │   ├── VideoListDto.cs                  # 视频列表DTO
│   │   ├── CategoryDto.cs                   # 分类DTO
│   │   └ CreateCategoryDto.cs               # 创建分类DTO
│   │   └ GetVideoListInput.cs               # 查询输入DTO
│   │   └ UploadChunkInput.cs                # 上传分片输入
│   │   └ UploadChunkResultDto.cs            # 上传分片结果
│   │   └ CompleteUploadInput.cs             # 完成上传输入
│   │   └ VideoPlayRecordDto.cs              # 播放记录DTO
│   ├── Services/
│   │   ├── IVideoAppService.cs              # 视频应用服务接口
│   │   ├── IUploadAppService.cs             # 上传应用服务接口
│   │   ├── ICategoryAppService.cs           # 分类应用服务接口
│   │   └ IVideoPlayAppService.cs            # 播放应用服务接口
│   ├── Permissions/
│   │   └ VideoPermissions.cs                # 权限定义
│   └─ Features/
│       └ VideoFeatureDefinitionProvider.cs  # 功能定义
│
├── LCH.MicroService.Video.Application/
│   ├── VideoApplicationModule.cs            # 应用模块定义
│   ├── Services/
│   │   ├── VideoAppService.cs               # 视频应用服务实现
│   │   ├── UploadAppService.cs              # 上传应用服务实现
│   │   ├── CategoryAppService.cs            # 分类应用服务实现
│   │   ├── VideoPlayAppService.cs           # 播放应用服务实现
│   ├── EventBus/
│   │   ├── VideoCreatedEventHandler.cs      # 视频创建事件处理
│   │   ├── VideoPublishedEventHandler.cs    # 视频发布事件处理
│   │   ├── TranscodeCompletedEventHandler.cs # 转码完成事件处理
│   │   ├── VideoUploadedEventHandler.cs     # 上传完成事件处理
│   ├── ObjectMapping/
│   │   └ VideoAutoMapperProfile.cs          # AutoMapper配置
│   └─ Jobs/
│        CleanExpiredUploadSessionsJob.cs    # 清理过期上传会话后台任务
│
├── LCH.MicroService.Video.HttpApi/
│   ├── VideoHttpApiModule.cs                # HTTP API模块定义
│   ├── Controllers/
│   │   ├── VideoController.cs               # 视频控制器
│   │   ├── UploadController.cs              # 上传控制器
│   │   ├── CategoryController.cs            # 分类控制器
│   │   ├── VideoPlayController.cs           # 播放控制器
│   └─ AbpAntiForgeryOptions.cs              # CSRF配置
│
├── LCH.MicroService.Video.HttpApi.Client/
│   ├── VideoHttpApiClientModule.cs          # HTTP客户端模块定义
│   ├── ClientProxies/
│   │   ├── VideoAppServiceClientProxy.cs    # 视频服务客户端代理
│   │   ├── UploadAppServiceClientProxy.cs   # 上传服务客户端代理
│   │   ├── CategoryAppServiceClientProxy.cs # 分类服务客户端代理
│   └─ Caching/
│        VideoClientCacheOptions.cs          # 客户端缓存配置
│
└── LCH.MicroService.Video.EntityFrameworkCore/
    ├── VideoEntityFrameworkCoreModule.cs    # EF Core模块定义
    ├── DbContexts/
    │   ├── VideoDbContext.cs                # 视频DbContext
    │   ├── IVideoDbContext.cs               # DbContext接口
    ├── EntityConfigurations/
    │   ├── VideoConfiguration.cs            # 视频实体配置
    │   ├── VideoUploadSessionConfiguration.cs # 上传会话配置
    │   ├── VideoTranscodeTaskConfiguration.cs # 转码任务配置
    │   ├── VideoTagConfiguration.cs         # 标签配置
    │   ├── VideoPlayRecordConfiguration.cs  # 播放记录配置
    │   ├── CategoryConfiguration.cs         # 分类配置
    ├── Repositories/
    │   ├── VideoRepository.cs                # 视频仓储实现
    │   ├── CategoryRepository.cs             # 分类仓储实现
    │   ├── UploadSessionRepository.cs        # 上传会话仓储实现
    │   ├── EfCoreVideoRepository.cs          # EF Core视频仓储
    ├── Migrations/
    │   ├── 20260512000000_Initial_Video_Schema.cs # 初始迁移
    │   ├── VideoDbContextModelSnapshot.cs    # 模型快照
    └─ Seeds/
         DefaultCategoryDataSeeder.cs         # 默认分类数据种子
```

---

### 3.2 核心代码示例

#### 3.2.1 Domain层 - Video实体

```csharp
// Video.cs (继承ABP FullAuditedAggregateRoot)
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace LCH.MicroService.Video.Domain.Entities
{
    /// <summary>
    /// 视频实体 - 继承ABP审计聚合根
    /// </summary>
    public class Video : FullAuditedAggregateRoot<Guid>, IMultiTenant
    {
        #region 基础属性
        
        public Guid? TenantId { get; set; }
        public Guid UserId { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverPath { get; set; }
        public Guid CategoryId { get; set; }
        
        #endregion
        
        #region 文件信息
        
        public string OriginalFilePath { get; set; }
        public long? OriginalFileSize { get; set; }
        public string OriginalFileMd5 { get; set; }
        public int? Duration { get; set; }
        
        #endregion
        
        #region HLS转码信息
        
        public bool HasTranscoded { get; set; }
        public string MasterPlaylistPath { get; set; }
        public string Resolution1080PPath { get; set; }
        public string Resolution720PPath { get; set; }
        public string Resolution480PPath { get; set; }
        public string Resolution360PPath { get; set; }
        
        #endregion
        
        #region 状态
        
        public VideoStatus Status { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishTime { get; set; }
        
        #endregion
        
        #region 审核
        
        public AuditStatus AuditStatus { get; set; }
        public DateTime? AuditTime { get; set; }
        public Guid? AuditorId { get; set; }
        public string AuditReason { get; set; }
        
        #endregion
        
        #region 统计
        
        public long ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CoinCount { get; set; }
        public int FavoriteCount { get; set; }
        public int ShareCount { get; set; }
        public int CommentCount { get; set; }
        public int DanmakuCount { get; set; }
        
        #endregion
        
        #region 其他
        
        public bool IsOriginal { get; set; }
        public string SourceUrl { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        
        #endregion
        
        #region 导航属性
        
        public virtual ICollection<VideoTranscodeTask> TranscodeTasks { get; set; }
        public virtual ICollection<VideoPlayRecord> PlayRecords { get; set; }
        
        #endregion
        
        #region 构造函数
        
        protected Video() { } // ORM构造
        
        public Video(
            Guid id,
            Guid userId,
            string title,
            Guid categoryId
        ) : base(id)
        {
            UserId = userId;
            Title = title;
            CategoryId = categoryId;
            Status = VideoStatus.Draft;
            AuditStatus = AuditStatus.Pending;
            IsPublished = false;
            IsOriginal = true;
        }
        
        #endregion
        
        #region 业务方法
        
        /// <summary>
        /// 更新视频信息
        /// </summary>
        public void Update(string title, string description, Guid categoryId, List<string> tags)
        {
            Title = title;
            Description = description;
            CategoryId = categoryId;
            Tags = tags;
        }
        
        /// <summary>
        /// 发布视频
        /// </summary>
        public void Publish()
        {
            if (Status != VideoStatus.Auditing && AuditStatus != AuditStatus.Approved)
            {
                throw new BusinessException(VideoDomainErrorCodes.VideoNotApproved);
            }
            
            Status = VideoStatus.Published;
            IsPublished = true;
            PublishTime = DateTime.UtcNow;
            
            // 发布领域事件
            AddLocalEvent(new VideoPublishedEvent(Id));
        }
        
        /// <summary>
        /// 审核通过
        /// </summary>
        public void Approve(Guid auditorId, string reason)
        {
            AuditStatus = AuditStatus.Approved;
            AuditTime = DateTime.UtcNow;
            AuditorId = auditorId;
            AuditReason = reason;
            Status = VideoStatus.Auditing;
        }
        
        /// <summary>
        /// 审核拒绝
        /// </summary>
        public void Reject(Guid auditorId, string reason)
        {
            AuditStatus = AuditStatus.Rejected;
            AuditTime = DateTime.UtcNow;
            AuditorId = auditorId;
            AuditReason = reason;
            Status = VideoStatus.Rejected;
        }
        
        /// <summary>
        /// 开始转码
        /// </summary>
        public void StartTranscoding()
        {
            if (Status != VideoStatus.UploadCompleted)
            {
                throw new BusinessException(VideoDomainErrorCodes.VideoNotUploaded);
            }
            
            Status = VideoStatus.Transcoding;
        }
        
        /// <summary>
        /// 转码完成
        /// </summary>
        public void CompleteTranscoding(
            string masterPlaylistPath,
            string resolution1080PPath,
            string resolution720PPath,
            string resolution480PPath,
            string resolution360PPath)
        {
            HasTranscoded = true;
            MasterPlaylistPath = masterPlaylistPath;
            Resolution1080PPath = resolution1080PPath;
            Resolution720PPath = resolution720PPath;
            Resolution480PPath = resolution480PPath;
            Resolution360PPath = resolution360PPath;
            Status = VideoStatus.Auditing;
            
            AddLocalEvent(new VideoTranscodeCompletedEvent(Id));
        }
        
        /// <summary>
        /// 增加播放量（原子操作）
        /// </summary>
        public void IncrementViewCount()
        {
            ViewCount++;
        }
        
        /// <summary>
        /// 增加点赞数
        /// </summary>
        public void IncrementLikeCount()
        {
            LikeCount++;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 视频状态枚举
    /// </summary>
    public enum VideoStatus
    {
        Draft = 0,
        Uploading = 1,
        UploadCompleted = 2,
        Transcoding = 3,
        Auditing = 4,
        Published = 5,
        Rejected = 6,
        Deleted = 7
    }
    
    /// <summary>
    /// 审核状态枚举
    /// </summary>
    public enum AuditStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        AutoApproved = 3 // AI自动审核通过
    }
}
```

---

#### 3.2.2 Domain层 - VideoManager领域服务

```csharp
// VideoManager.cs
using Volo.Abp.Domain.Services;
using Volo.Abp.BlobStoring; // 继承现有BlobStoring

namespace LCH.MicroService.Video.Domain.Services
{
    /// <summary>
    /// 视频管理领域服务
    /// </summary>
    public class VideoManager : DomainService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IBlobContainer _blobContainer; // 继承现有MinIO容器
        private readonly IDistributedCache<VideoCacheItem> _cache;
        private readonly ICurrentTenant _currentTenant;
        
        public VideoManager(
            IVideoRepository videoRepository,
            IBlobContainer blobContainer,
            IDistributedCache<VideoCacheItem> cache,
            ICurrentTenant currentTenant)
        {
            _videoRepository = videoRepository;
            _blobContainer = blobContainer; // 使用现有MinIO BlobContainer
            _cache = cache;
            _currentTenant = currentTenant;
        }
        
        /// <summary>
        /// 创建视频实体
        /// </summary>
        public async Task<Video> CreateAsync(
            Guid userId,
            string title,
            Guid categoryId,
            string description = null,
            List<string> tags = null)
        {
            // 验证分类是否存在
            var category = await _videoRepository.GetCategoryAsync(categoryId);
            if (category == null)
            {
                throw new BusinessException(VideoDomainErrorCodes.CategoryNotFound);
            }
            
            // 创建视频实体
            var video = new Video(
                GuidGenerator.Create(),
                userId,
                title,
                categoryId
            );
            
            video.Description = description;
            video.Tags = tags ?? new List<string>();
            
            await _videoRepository.InsertAsync(video);
            
            return video;
        }
        
        /// <summary>
        /// 获取视频（带缓存）
        /// </summary>
        public async Task<Video> GetAsync(Guid id)
        {
            var cacheKey = $"video:{id}";
            
            return await _cache.GetOrAddAsync(
                cacheKey,
                async () => await _videoRepository.GetAsync(id),
                () => new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                }
            );
        }
        
        /// <summary>
        /// 保存视频文件（使用现有BlobContainer）
        /// </summary>
        public async Task<string> SaveVideoFileAsync(
            Guid videoId,
            Stream fileStream,
            string fileName)
        {
            // 使用现有MinIO BlobContainer（继承OssManagement配置）
            var blobName = $"videos/{videoId}/original/{fileName}";
            await _blobContainer.SaveAsync(blobName, fileStream);
            
            return blobName;
        }
        
        /// <summary>
        /// 生成封面（从视频提取）
        /// </summary>
        public async Task<string> GenerateCoverAsync(Guid videoId, int second)
        {
            var videoPath = await GetVideoPathAsync(videoId);
            
            // 使用FFmpeg提取封面
            var coverPath = $"covers/{videoId}/cover_{second}.jpg";
            
            // FFmpeg命令（继承现有FFmpeg配置）
            var ffmpegCommand = $"ffmpeg -i {videoPath} -ss {second} -frames:v 1 -f image2 {coverPath}";
            
            // 执行FFmpeg命令
            await ExecuteFFmpegAsync(ffmpegCommand);
            
            return coverPath;
        }
        
        private async Task ExecuteFFmpegAsync(string command)
        {
            // 执行FFmpeg命令（继承现有TaskManagement）
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            await process.WaitForExitAsync();
        }
    }
    
    /// <summary>
    /// 视频缓存项
    /// </summary>
    public class VideoCacheItem
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string CoverPath { get; set; }
        public long ViewCount { get; set; }
    }
}
```

---

#### 3.2.3 Application层 - VideoAppService

```csharp
// VideoAppService.cs (继承ABP ApplicationService)
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Users;

namespace LCH.MicroService.Video.Application.Services
{
    /// <summary>
    /// 视频应用服务 - 继承ABP CRUD服务
    /// </summary>
    public class VideoAppService :
        CrudAppService<
            Video,
            VideoDto,
            Guid,
            GetVideoListInput,
            CreateVideoDto,
            UpdateVideoDto>,
        IVideoAppService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly VideoManager _videoManager;
        private readonly IUploadSessionManager _uploadSessionManager;
        private readonly ICurrentUser _currentUser;
        private readonly IBlobContainer _blobContainer;
        
        public VideoAppService(
            IVideoRepository videoRepository,
            VideoManager videoManager,
            IUploadSessionManager uploadSessionManager,
            ICurrentUser currentUser,
            IBlobContainer blobContainer)
            : base(videoRepository)
        {
            _videoRepository = videoRepository;
            _videoManager = videoManager;
            _uploadSessionManager = uploadSessionManager;
            _currentUser = currentUser;
            _blobContainer = blobContainer;
            
            // 权限定义（继承ABP Permission）
            GetPolicyName = VideoPermissions.Videos.Default;
            GetListPolicyName = VideoPermissions.Videos.Default;
            CreatePolicyName = VideoPermissions.Videos.Create;
            UpdatePolicyName = VideoPermissions.Videos.Update;
            DeletePolicyName = VideoPermissions.Videos.Delete;
        }
        
        /// <summary>
        /// 创建视频（重写基类方法）
        /// </summary>
        [Authorize(VideoPermissions.Videos.Create)]
        public override async Task<VideoDto> CreateAsync(CreateVideoDto input)
        {
            // 1. 创建视频实体
            var video = await _videoManager.CreateAsync(
                _currentUser.Id.Value,
                input.Title,
                input.CategoryId.Value,
                input.Description,
                input.Tags
            );
            
            // 2. 创建上传会话
            var uploadSession = await _uploadSessionManager.CreateSessionAsync(
                video.Id,
                input.TotalFileSize
            );
            
            // 3. 返回DTO（使用ABP ObjectMapper）
            var videoDto = ObjectMapper.Map<Video, VideoDto>(video);
            
            // 4. 添加上传信息
            videoDto.UploadSessionId = uploadSession.Id;
            videoDto.UploadUrl = $"/api/video/upload/{uploadSession.Id}";
            
            return videoDto;
        }
        
        /// <summary>
        /// 获取视频列表（重写基类方法）
        /// </summary>
        public override async Task<PagedResultDto<VideoDto>> GetListAsync(GetVideoListInput input)
        {
            // 使用ABP Repository扩展查询
            var query = await _videoRepository.GetQueryableAsync();
            
            query = query
                .Where(v => v.Status == VideoStatus.Published)
                .WhereIf(input.CategoryId.HasValue, v => v.CategoryId == input.CategoryId.Value)
                .WhereIf(input.UserId.HasValue, v => v.UserId == input.UserId.Value)
                .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                    v => v.Title.Contains(input.Keyword) || v.Description.Contains(input.Keyword));
            
            // 排序（继承ABP Sorting）
            query = input.Sorting switch
            {
                "ViewCount DESC" => query.OrderByDescending(v => v.ViewCount),
                "PublishTime DESC" => query.OrderByDescending(v => v.PublishTime),
                "LikeCount DESC" => query.OrderByDescending(v => v.LikeCount),
                _ => query.OrderByDescending(v => v.PublishTime)
            };
            
            // 分页（继承ABP Pagination）
            var totalCount = await AsyncExecuter.CountAsync(query);
            var videos = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );
            
            // 映射DTO（继承ABP ObjectMapper）
            var items = ObjectMapper.Map<List<Video>, List<VideoDto>>(videos);
            
            return new PagedResultDto<VideoDto>(totalCount, items);
        }
        
        /// <summary>
        /// 获取我的视频列表
        /// </summary>
        [Authorize(VideoPermissions.Videos.Default)]
        public async Task<PagedResultDto<VideoDto>> GetMyVideosAsync(GetMyVideosInput input)
        {
            var query = await _videoRepository.GetQueryableAsync();
            
            query = query
                .Where(v => v.UserId == _currentUser.Id.Value)
                .WhereIf(input.Status.HasValue, v => v.Status == input.Status.Value);
            
            var totalCount = await AsyncExecuter.CountAsync(query);
            var videos = await AsyncExecuter.ToListAsync(
                query.OrderByDescending(v => v.CreationTime)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
            );
            
            return new PagedResultDto<VideoDto>(
                totalCount,
                ObjectMapper.Map<List<Video>, List<VideoDto>>(videos)
            );
        }
        
        /// <summary>
        /// 获取视频详情（带用户交互信息）
        /// </summary>
        public async Task<VideoDetailDto> GetDetailAsync(Guid id)
        {
            var video = await _videoRepository.GetAsync(id);
            
            var detailDto = ObjectMapper.Map<Video, VideoDetailDto>(video);
            
            // 添加用户交互信息（如果已登录）
            if (_currentUser.IsAuthenticated)
            {
                var userId = _currentUser.Id.Value;
                detailDto.UserInteraction = new UserInteractionDto
                {
                    IsLiked = await _videoRepository.IsLikedAsync(id, userId),
                    IsCoined = await _videoRepository.IsCoinedAsync(id, userId),
                    IsFavorite = await _videoRepository.IsFavoriteAsync(id, userId)
                };
            }
            
            return detailDto;
        }
        
        /// <summary>
        /// 发布视频
        /// </summary>
        [Authorize(VideoPermissions.Videos.Publish)]
        public async Task PublishAsync(Guid id)
        {
            var video = await _videoRepository.GetAsync(id);
            
            // 验证权限（仅UP主本人可发布）
            if (video.UserId != _currentUser.Id.Value)
            {
                throw new BusinessException(VideoDomainErrorCodes.NotVideoOwner);
            }
            
            video.Publish();
            
            await _videoRepository.UpdateAsync(video);
        }
    }
}
```

---

## 4. DanmakuService代码架构设计

### 4.1 项目结构

```
aspnet-core/modules/danmaku/
├── LCH.MicroService.Danmaku.Domain.Shared/
│   ├── Enums/
│   │   ├── DanmakuType.cs                   # 弹幕类型
│   │   ├── DanmakuPosition.cs               # 弹幕位置
│   │   └ ReportStatus.cs                    # 举报状态
│   └─ Localization/
│       └ DanmakuLocalizationKeys.cs
│
├── LCH.MicroService.Danmaku.Domain/
│   ├── Entities/
│   │   ├── Danmaku.cs                       # 弹幕实体（分区表）
│   │   ├── DanmakuReport.cs                 # 弹幕举报
│   ├── Services/
│   │   ├── IDanmakuManager.cs               # 弹幕管理领域服务
│   │   ├── IDanmakuFilterService.cs         # 弹幕过滤服务
│   ├── Events/
│   │   ├── DanmakuSentEvent.cs              # 弹幕发送事件
│   │   ├── DanmakuReportedEvent.cs          # 弹幕举报事件
│   └─ Specifications/
│       ├── DanmakuByVideoSpecification.cs   # 视频弹幕规格
│       └ DanmakuByTimeRangeSpecification.cs # 时间段弹幕规格
│
├── LCH.MicroService.Danmaku.Application.Contracts/
│   ├── Dtos/
│   │   ├── DanmakuDto.cs                    # 弹幕DTO
│   │   ├── SendDanmakuInput.cs              # 发送弹幕输入
│   │   ├── DanmakuListResultDto.cs          # 弹幕列表结果
│   │   ├── ReportDanmakuInput.cs            # 举报弹幕输入
│   ├── Services/
│   │   ├── IDanmakuAppService.cs            # 弹幕应用服务接口
│   │   ├── IDanmakuReportAppService.cs      # 弹幕举报服务接口
│   ├── Permissions/
│   │   └ DanmakuPermissions.cs              # 弹幕权限
│   └─ Settings/
│       └ DanmakuSettings.cs                 # 弹幕设置
│
├── LCH.MicroService.Danmaku.Application/
│   ├── Services/
│   │   ├── DanmakuAppService.cs             # 弹幕应用服务
│   │   ├── DanmakuReportAppService.cs       # 弹幕举报服务
│   ├── SignalR/
│   │   ├── DanmakuHub.cs                    # 弹幕SignalR Hub（继承现有）
│   │   ├── DanmakuHubContext.cs             # Hub上下文
│   ├── EventBus/
│   │   ├── DanmakuSentEventHandler.cs       # 弹幕发送事件处理
│   │   ├── VideoPublishedEventHandler.cs    # 视频发布事件处理（同步弹幕）
│   ├── ObjectMapping/
│   │   └ DanmakuAutoMapperProfile.cs
│   └─ BackgroundServices/
│        DanmakuCacheBackgroundService.cs    # 弹幕缓存后台服务
│
├── LCH.MicroService.Danmaku.HttpApi/
│   ├── Controllers/
│   │   ├── DanmakuController.cs             # 弹幕HTTP控制器
│   │   ├── DanmakuReportController.cs       # 弹幕举报控制器
│   └─ SignalR/
│       └ DanmakuHubRouteProvider.cs         # SignalR路由配置
│
├── LCH.MicroService.Danmaku.HttpApi.Client/
│   ├── ClientProxies/
│   │   ├── DanmakuAppServiceClientProxy.cs
│   │   ├── DanmakuReportAppServiceClientProxy.cs
│   └─ Caching/
│       \ DanmakuClientCacheOptions.cs
│
└── LCH.MicroService.Danmaku.EntityFrameworkCore/
    ├── DbContexts/
    │   ├── DanmakuDbContext.cs              # 弹幕DbContext
    ├── EntityConfigurations/
    │   ├── DanmakuConfiguration.cs          # 弹幕实体配置（分区表）
    │   ├── DanmakuReportConfiguration.cs
    ├── Repositories/
    │   ├── EfCoreDanmakuRepository.cs       # EF Core弹幕仓储
    │   ├── DanmakuRepository.cs             # 弹幕仓储（分区查询）
    └─ Extensions/
         DanmakuDbContextExtensions.cs       # DbContext扩展（分区创建）
```

---

### 4.2 SignalR Hub实现（继承现有架构）

```csharp
// DanmakuHub.cs (继承现有AbpHub)
using Volo.Abp.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace LCH.MicroService.Danmaku.Application.SignalR
{
    /// <summary>
    /// 弹幕Hub - 继承ABP SignalR架构
    /// </summary>
    public class DanmakuHub : AbpHub
    {
        private readonly IDanmakuAppService _danmakuAppService;
        private readonly ICurrentUser _currentUser;
        private readonly IDistributedCache<List<DanmakuDto>> _cache;
        private readonly ILogger<DanmakuHub> _logger;
        
        public DanmakuHub(
            IDanmakuAppService danmakuAppService,
            ICurrentUser currentUser,
            IDistributedCache<List<DanmakuDto>> cache,
            ILogger<DanmakuHub> logger)
        {
            _danmakuAppService = danmakuAppService;
            _currentUser = currentUser;
            _cache = cache;
            _logger = logger;
        }
        
        /// <summary>
        /// 连接时验证用户
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            if (!_currentUser.IsAuthenticated)
            {
                _logger.LogWarning($"未认证用户尝试连接: {Context.ConnectionId}");
                Context.Abort();
                return;
            }
            
            _logger.LogInformation($"用户 {_currentUser.UserName} 已连接");
            await base.OnConnectedAsync();
        }
        
        /// <summary>
        /// 加入视频房间（继承现有Group管理）
        /// </summary>
        public async Task JoinRoom(Guid videoId)
        {
            // 加入SignalR Group（继承Redis Scaleout）
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"video_{videoId}"
            );
            
            // 发送最近弹幕（从缓存获取）
            var cacheKey = $"danmaku:recent:{videoId}";
            var recentDanmakus = await _cache.GetOrAddAsync(
                cacheKey,
                async () => await _danmakuAppService.GetRecentAsync(videoId, 100),
                () => new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                }
            );
            
            // 发送给客户端
            await Clients.Caller.SendAsync("RecentDanmakus", recentDanmakus);
            
            _logger.LogInformation($"用户 {_currentUser.UserName} 加入房间 video_{videoId}");
        }
        
        /// <summary>
        /// 发送弹幕
        /// </summary>
        public async Task SendDanmaku(SendDanmakuInput input)
        {
            try
            {
                // 1. 验证用户等级（继承现有用户系统）
                var userLevel = await GetUserLevelAsync(_currentUser.Id.Value);
                
                // 2. 高级弹幕权限检查
                if (input.DanmakuType == DanmakuType.Advanced && userLevel < 2)
                {
                    throw new BusinessException(DanmakuDomainErrorCodes.AdvancedDanmakuRequiresLevel2);
                }
                
                // 3. 创建弹幕（通过Application服务）
                var danmakuDto = await _danmakuAppService.SendAsync(input);
                
                // 4. 广播到房间（使用Redis Scaleout）
                await Clients.Group($"video_{input.VideoId}")
                    .SendAsync("NewDanmaku", danmakuDto);
                
                // 5. 更新缓存（后台服务定时同步）
                await UpdateDanmakuCacheAsync(input.VideoId, danmakuDto);
                
                _logger.LogInformation($"弹幕已发送: Video={input.VideoId}, User={_currentUser.UserName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"弹幕发送失败: {ex.Message}");
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }
        
        /// <summary>
        /// 退出房间
        /// </summary>
        public async Task LeaveRoom(Guid videoId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"video_{videoId}"
            );
            
            _logger.LogInformation($"用户 {_currentUser.UserName} 退出房间 video_{videoId}");
        }
        
        /// <summary>
        /// 断开连接
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"用户 {_currentUser.UserName} 已断开连接");
            await base.OnDisconnectedAsync(exception);
        }
        
        #region 私有方法
        
        private async Task<int> GetUserLevelAsync(Guid userId)
        {
            // 通过HTTP调用UserService（继承现有服务间通信）
            // 或通过Redis缓存获取
            var cacheKey = $"user:level:{userId}";
            return await _cache.GetAsync<int>(cacheKey) ?? 0;
        }
        
        private async Task UpdateDanmakuCacheAsync(Guid videoId, DanmakuDto newDanmaku)
        {
            var cacheKey = $"danmaku:recent:{videoId}";
            var danmakus = await _cache.GetAsync<List<DanmakuDto>>(cacheKey);
            
            if (danmakus != null)
            {
                danmakus.Add(newDanmaku);
                if (danmakus.Count > 100)
                {
                    danmakus.RemoveAt(0); // 保持最新100条
                }
                
                await _cache.SetAsync(cacheKey, danmakus);
            }
        }
        
        #endregion
    }
}
```

---

## 5. TranscodeService.Worker代码架构设计

### 5.1 项目结构

```
aspnet-core/services/transcode-worker/
├── LCH.MicroService.TranscodeWorker/
│   ├── Program.cs                           # Worker入口
│   ├── TranscodeWorkerModule.cs             # Worker模块定义
│   ├── Workers/
│   │   ├── TranscodeBackgroundWorker.cs     # 转码后台Worker（继承ABP）
│   │   ├── CleanupTempFilesWorker.cs        # 清理临时文件Worker
│   ├── Services/
│   │   ├── FFmpegService.cs                 # FFmpeg调用服务
│   │   ├── HLSGeneratorService.cs           # HLS生成服务
│   │   ├── ThumbnailExtractorService.cs     # 尘面提取服务
│   │   ├── ITranscodeQueueConsumer.cs       # 转码队列消费者
│   ├── EventBus/
│   │   ├── TranscodeTaskCreatedEventHandler.cs # 转码任务事件处理
│   │   ├── VideoUploadedEventHandler.cs     # 视频上传完成事件处理
│   ├── Configuration/
│   │   ├── TranscodeSettings.cs             # 转码配置
│   │   ├── FFmpegPreset.cs                  # FFmpeg预设配置
│   ├── Models/
│   │   ├── TranscodeTask.cs                 # 转码任务模型
│   │   ├── TranscodeResult.cs               # 转码结果
│   │   ├── VideoMediaInfo.cs                # 视频媒体信息
│   ├── appsettings.json                     # 配置文件
│   └── Dockerfile                           # Docker配置
│
└── Tests/
    ├── FFmpegServiceTests.cs                 # FFmpeg服务测试
    ├── HLSGeneratorTests.cs                  # HLS生成测试
    └── TranscodeWorkerTests.cs               # Worker测试
```

---

### 5.2 转码Worker实现（继承ABP BackgroundWorker）

```csharp
// TranscodeBackgroundWorker.cs (继承ABP BackgroundWorker)
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.BlobStoring;

namespace LCH.MicroService.TranscodeWorker.Workers
{
    /// <summary>
    /// 转码后台Worker - 继承ABP BackgroundWorker
    /// 使用RabbitMQ消费转码任务队列（继承现有CAP配置）
    /// </summary>
    public class TranscodeBackgroundWorker : BackgroundWorkerBase
    {
        private readonly ITranscodeQueueConsumer _queueConsumer;
        private readonly FFmpegService _ffmpegService;
        private readonly HLSGeneratorService _hlsGenerator;
        private readonly IBlobContainer _blobContainer;
        private readonly IDistributedEventBus _eventBus;
        private readonly ILogger<TranscodeBackgroundWorker> _logger;
        private readonly TranscodeSettings _settings;
        
        public TranscodeBackgroundWorker(
            ITranscodeQueueConsumer queueConsumer,
            FFmpegService ffmpegService,
            HLSGeneratorService hlsGenerator,
            IBlobContainer blobContainer,
            IDistributedEventBus eventBus,
            ILogger<TranscodeBackgroundWorker> logger,
            TranscodeSettings settings)
        {
            _queueConsumer = queueConsumer;
            _ffmpegService = ffmpegService;
            _hlsGenerator = hlsGenerator;
            _blobContainer = blobContainer;
            _eventBus = eventBus;
            _logger = logger;
            _settings = settings;
        }
        
        /// <summary>
        /// Worker启动（继承ABP BackgroundWorkerBase）
        /// </summary>
        public override async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("转码Worker启动，并发数: {Concurrency}", _settings.MaxConcurrency);
            
            // 启动队列消费者（继承现有CAP/RabbitMQ）
            await _queueConsumer.StartConsumingAsync(
                "transcode_tasks",
                async (task) => await ProcessTranscodeTaskAsync(task),
                cancellationToken
            );
            
            await base.StartAsync(cancellationToken);
        }
        
        /// <summary>
        /// 处理转码任务
        /// </summary>
        private async Task ProcessTranscodeTaskAsync(TranscodeTaskMessage task)
        {
            _logger.LogInformation(
                "开始处理转码任务: VideoId={VideoId}, TaskId={TaskId}",
                task.VideoId,
                task.TaskId
            );
            
            try
            {
                // 1. 获取视频媒体信息（使用FFprobe）
                var mediaInfo = await _ffmpegService.GetMediaInfoAsync(task.OriginalFilePath);
                
                // 2. 确定转码清晰度（继承现有配置）
                var resolutions = DetermineResolutions(mediaInfo, _settings);
                
                // 3. 执行CPU转码（继承现有FFmpeg配置）
                var transcodeResults = new Dictionary<string, string>();
                
                foreach (var resolution in resolutions)
                {
                    _logger.LogInformation("转码 {Resolution}", resolution.Name);
                    
                    var outputPath = await _ffmpegService.TranscodeAsync(
                        task.OriginalFilePath,
                        resolution,
                        cancellationToken: CancellationToken
                    );
                    
                    transcodeResults[resolution.Name] = outputPath;
                }
                
                // 4. 生成HLS Master Playlist（继承现有HLS生成逻辑）
                var masterPlaylistPath = await _hlsGenerator.GenerateMasterPlaylistAsync(
                    task.VideoId,
                    transcodeResults,
                    mediaInfo
                );
                
                // 5. 上传到MinIO（继承现有BlobContainer）
                await UploadToStorageAsync(task.VideoId, transcodeResults, masterPlaylistPath);
                
                // 6. 发布转码完成事件（继承现有EventBus）
                await _eventBus.PublishAsync(new VideoTranscodeCompletedEto
                {
                    VideoId = task.VideoId,
                    TaskId = task.TaskId,
                    MasterPlaylistPath = masterPlaylistPath,
                    ResolutionPaths = transcodeResults,
                    Status = TranscodeStatus.Completed
                });
                
                _logger.LogInformation(
                    "转码完成: VideoId={VideoId}, Resolutions={Count}",
                    task.VideoId,
                    transcodeResults.Count
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "转码失败: VideoId={VideoId}", task.VideoId);
                
                // 发布转码失败事件
                await _eventBus.PublishAsync(new VideoTranscodeFailedEto
                {
                    VideoId = task.VideoId,
                    TaskId = task.TaskId,
                    ErrorMessage = ex.Message,
                    Status = TranscodeStatus.Failed
                });
            }
        }
        
        /// <summary>
        /// 确定转码清晰度
        /// </summary>
        private List<Resolution> DetermineResolutions(VideoMediaInfo mediaInfo, TranscodeSettings settings)
        {
            var resolutions = new List<Resolution>();
            
            // 基于原始视频分辨率确定（继承现有逻辑）
            if (mediaInfo.Height >= 1080 && settings.Enable1080P)
                resolutions.Add(Resolution1080P);
            
            if (mediaInfo.Height >= 720 && settings.Enable720P)
                resolutions.Add(Resolution720P);
            
            if (mediaInfo.Height >= 480 && settings.Enable480P)
                resolutions.Add(Resolution480P);
            
            // 360P始终生成
            resolutions.Add(Resolution360P);
            
            return resolutions;
        }
        
        /// <summary>
        /// 上传到存储（继承现有MinIO）
        /// </summary>
        private async Task UploadToStorageAsync(
            Guid videoId,
            Dictionary<string, string> transcodeResults,
            string masterPlaylistPath)
        {
            // 使用现有BlobContainer（继承OssManagement）
            foreach (var kvp in transcodeResults)
            {
                var blobName = $"videos/{videoId}/hls/{kvp.Key}.m3u8";
                var fileBytes = await File.ReadAllBytesAsync(kvp.Value);
                await _blobContainer.SaveAsync(blobName, new MemoryStream(fileBytes));
                
                // 上传TS分片
                var tsFiles = Directory.GetFiles(Path.GetDirectoryName(kvp.Value), "*.ts");
                foreach (var tsFile in tsFiles)
                {
                    var tsBlobName = $"videos/{videoId}/hls/{Path.GetFileName(tsFile)}";
                    var tsBytes = await File.ReadAllBytesAsync(tsFile);
                    await _blobContainer.SaveAsync(tsBlobName, new MemoryStream(tsBytes));
                }
            }
            
            // 上传Master Playlist
            var masterBytes = await File.ReadAllBytesAsync(masterPlaylistPath);
            await _blobContainer.SaveAsync(
                $"videos/{videoId}/hls/master.m3u8",
                new MemoryStream(masterBytes)
            );
        }
    }
    
    /// <summary>
    /// 转码设置（继承现有配置）
    /// </summary>
    public class TranscodeSettings
    {
        public int MaxConcurrency { get; set; } = 3;
        public bool Enable1080P { get; set; } = true;
        public bool Enable720P { get; set; } = true;
        public bool Enable480P { get; set; } = true;
        public bool Enable360P { get; set; } = true;
        public string FFmpegPath { get; set; } = "/usr/bin/ffmpeg";
        public string TempDirectory { get; set; } = "/tmp/transcode";
    }
}
```

---

## 6. InteractionService代码架构设计

### 6.1 项目结构

```
aspnet-core/modules/interaction/
├── LCH.MicroService.Interaction.Domain.Shared/
│   ├── Enums/
│   │   ├── InteractionType.cs               # 互动类型
│   │   ├── CommentAuditStatus.cs            # 评论审核状态
│   └─ Localization/
│       \ InteractionLocalizationKeys.cs
│
├── LCH.MicroService.Interaction.Domain/
│   ├── Entities/
│   │   ├── UserLike.cs                      # 用户点赞
│   │   ├── UserCoin.cs                      # 用户投币
│   │   ├── UserFavorite.cs                  # 用户收藏
│   │   ├── FavoriteGroup.cs                 # 收藏夹
│   │   ├── Comment.cs                       # 评论
│   │   ├── VideoShare.cs                    # 分享记录
│   │   ├── UserFollow.cs                    # 用户关注
│   │   ├── UserBehaviorRecord.cs            # 用户行为记录
│   ├── Services/
│   │   ├── IInteractionManager.cs           # 互动管理领域服务
│   │   ├── ICommentManager.cs               # 评论管理
│   │   ├── IUserCoinManager.cs              # 投币管理
│   ├── Events/
│   │   ├── VideoLikedEvent.cs               # 视频点赞事件
│   │   ├── VideoCoinedEvent.cs              # 视频投币事件
│   │   ├── CommentCreatedEvent.cs           # 评论创建事件
│   │   ├── UserFollowedEvent.cs             # 用户关注事件
│   └─ Specifications/
│       ├── UserLikedSpecification.cs        # 用户点赞规格
│        VideoCommentedSpecification.cs      # 视频评论规格
│
├── LCH.MicroService.Interaction.Application.Contracts/
│   ├── Dtos/
│   │   ├── LikeResultDto.cs                 # 点赞结果DTO
│   │   ├── CoinResultDto.cs                 # 投币结果DTO
│   │   ├── CommentDto.cs                    # 评论DTO
│   │   ├── CreateCommentDto.cs              # 创建评论DTO
│   │   ├── CommentListDto.cs                # 评论列表DTO
│   │   ├── FavoriteDto.cs                   # 收藏DTO
│   │   ├── FollowResultDto.cs               # 关注结果DTO
│   ├── Services/
│   │   ├── ILikeAppService.cs               # 点赞服务接口
│   │   ├── ICoinAppService.cs               # 投币服务接口
│   │   ├── ICommentAppService.cs            # 评论服务接口
│   │   ├── IFavoriteAppService.cs           # 收藏服务接口
│   │   ├── IFollowAppService.cs             # 关注服务接口
│   │   ├── ISearchAppService.cs             # 分享服务接口
│   ├── Permissions/
│   │   \ InteractionPermissions.cs
│   └─ Features/
│       \ InteractionFeatureDefinitionProvider.cs
│
├── LCH.MicroService.Interaction.Application/
│   ├── Services/
│   │   ├── LikeAppService.cs                # 点赞应用服务
│   │   ├── CoinAppService.cs                # 投币应用服务
│   │   ├── CommentAppService.cs             # 评论应用服务
│   │   ├── FavoriteAppService.cs            # 收藏应用服务
│   │   ├── FollowAppService.cs              # 关注应用服务
│   │   ├── SearchAppService.cs              # 分享应用服务
│   ├── EventBus/
│   │   ├── VideoLikedEventHandler.cs        # 视频点赞事件处理
│   │   ├── VideoCoinedEventHandler.cs       # 视频投币事件处理
│   │   ├── CommentCreatedEventHandler.cs    # 评论创建事件处理
│   ├── Validators/
│   │   ├── CommentValidator.cs              # 评论验证器
│   │   ├── CreateCommentValidator.cs        # 创建评论验证器
│   ├── ObjectMapping/
│   │   \ InteractionAutoMapperProfile.cs
│   └─ BackgroundServices/
│       \ UserBehaviorSyncBackgroundService.cs # 用户行为同步服务
│
├── LCH.MicroService.Interaction.HttpApi/
│   ├── Controllers/
│   │   ├── LikeController.cs                # 点赞控制器
│   │   ├── CoinController.cs                # 投币控制器
│   │   ├── CommentController.cs             # 评论控制器
│   │   ├── FavoriteController.cs            # 收藏控制器
│   │   ├── FollowController.cs              # 关注控制器
│   │   ├── ShareController.cs               # 分享控制器
│   └─ AbpAntiForgeryOptions.cs
│
├── LCH.MicroService.Interaction.HttpApi.Client/
│   ├── ClientProxies/
│   │   ├── LikeAppServiceClientProxy.cs
│   │   ├── CoinAppServiceClientProxy.cs
│   │   ├── CommentAppServiceClientProxy.cs
│   │   ├── FavoriteAppServiceClientProxy.cs
│   │   ├── FollowAppServiceClientProxy.cs
│   └─ Caching/
│       \ InteractionClientCacheOptions.cs
│
└── LCH.MicroService.Interaction.EntityFrameworkCore/
    ├── DbContexts/
    │   ├── InteractionDbContext.cs          # 互动DbContext
    ├── EntityConfigurations/
    │   ├── UserLikeConfiguration.cs
    │   ├── UserCoinConfiguration.cs
    │   ├── CommentConfiguration.cs
    │   ├── UserFollowConfiguration.cs
    │   ├── UserBehaviorRecordConfiguration.cs # 行为记录配置（分区表）
    ├── Repositories/
    │   ├── EfCoreUserLikeRepository.cs
    │   ├── EfCoreCommentRepository.cs
    │   ├── EfCoreUserBehaviorRepository.cs    # 用户行为仓储
    └─ Extensions/
        \ InteractionDbContextExtensions.cs
```

---

## 7. 项目依赖关系图

```
┌─────────────────────────────────────────────────────────────┐
│                    Bilibili模块依赖关系                       │
└─────────────────────────────────────────────────────────────┘

Domain.Shared (基础层)
    │
    ↓ 被所有层引用
Domain (领域层)
    │ 依赖: Domain.Shared, ABP Domain
    │
    ↓ 被Application和EntityFrameworkCore引用
Application.Contracts (契约层)
    │ 依赖: Domain.Shared, ABP Application.Contracts
    │
    ↓ 被Application和HttpApi.Client引用
Application (应用层)
    │ 依赖: Domain, Application.Contracts, ABP Application
    │       EventBus, Redis, BlobStoring
    │
    ↓ 被HttpApi引用
HttpApi (API层)
    │ 依赖: Application.Contracts, ABP HttpApi
    │
    ↓ 被HttpApi.Host引用
HttpApi.Client (客户端层)
    │ 依赖: Application.Contracts, ABP HttpApi.Client
    │
    ↓ 被其他服务引用（服务间调用）
EntityFrameworkCore (持久化层)
    │ 依赖: Domain, ABP EntityFrameworkCore
    │
    ↓ 被HttpApi.Host引用

模块间依赖:
- VideoService → Identity (用户), OssManagement (存储)
- DanmakuService → VideoService (视频), RealtimeMessage (SignalR)
- InteractionService → VideoService, Identity
- TranscodeWorker → VideoService (事件), OssManagement (存储)
- SearchService → VideoService (同步), Elasticsearch
- RecommendService → InteractionService (行为数据), VideoService
- LiveService → VideoService (用户), OssManagement (存储)
- CategoryService → VideoService (视频)
- UserService → Identity (继承), UserBehavior数据
```

---

## 8. 配置文件示例

### 8.1 VideoService模块配置

```csharp
// VideoServiceModule.cs (继承ABp Module)
using Volo.Abp.Modularity;
using Volo.Abp.BlobStoring;
using Volo.Abp.EventBus.RabbitMQ;

namespace LCH.MicroService.Video
{
    [DependsOn(
        typeof(AbpAspNetCoreMvcModule),         // ABP MVC模块
        typeof(AbpEntityFrameworkCoreModule),   // EF Core模块
        typeof(AbpBlobStoringModule),           // Blob存储模块（继承现有）
        typeof(AbpEventBusRabbitMqModule),      // 事件总线（继承现有CAP）
        typeof(IdentityModule),                 // 身份模块（继承现有）
        typeof(OssManagementModule),            // 对象存储模块（继承现有）
        typeof(VideoDomainModule),              // 视频领域模块
        typeof(VideoApplicationModule),         // 视频应用模块
        typeof(VideoEntityFrameworkCoreModule)  // 视频EF Core模块
    )]
    public class VideoServiceModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            
            // 配置BlobContainer（继承现有MinIO）
            context.Services.Configure<AbpBlobStoringOptions>(options =>
            {
                options.Containers.Configure<VideoBlobContainerConfiguration>(container =>
                {
                    container.UseMinIO(minIO =>
                    {
                        minIO.EndPoint = configuration["MinIO:EndPoint"];
                        minIO.AccessKey = configuration["MinIO:AccessKey"];
                        minIO.SecretKey = configuration["MinIO:SecretKey"];
                        minIO.BucketName = "bilibili-videos";
                    });
                });
            });
            
            // 配置CAP（继承现有RabbitMQ）
            context.Services.AddCap(x =>
            {
                x.UseRabbitMQ(options =>
                {
                    options.HostName = configuration["RabbitMQ:HostName"];
                    options.UserName = configuration["RabbitMQ:UserName"];
                    options.Password = configuration["RabbitMQ:Password"];
                });
                
                x.UsePostgreSQL<VideoDbContext>(configuration["ConnectionStrings:Video"]);
            });
            
            // 配置Redis（继承现有）
            context.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:Configuration"];
            });
        }
        
        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            var app = context.GetApplicationBuilder();
            
            // 使用ABP中间件（继承现有）
            app.UseAbpRequestLocalization();
            app.UseAbpSecurityHeaders();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAbpSwagger();
            app.UseMvc();
        }
    }
}
```

---

## 9. 总结

### 9.1 代码架构特点

| 特点 | 说明 |
|------|------|
| **继承ABP架构** | 完全继承现有ABP DDD分层架构 |
| **继承现有模块** | 依赖Identity、OssManagement、RealtimeMessage等现有模块 |
| **继承现有配置** | 使用现有Redis、RabbitMQ、MinIO、SignalR配置 |
| **继承现有服务间通信** | 使用现有CAP事件总线、HTTP Client |
| **继承现有认证授权** | 使用现有IdentityServer、JWT认证 |
| **继承现有缓存策略** | 使用现有Redis分布式缓存 |
| **继承现有后台任务** | 使用现有BackgroundWorkers、CAP队列 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 代码架构设计完成