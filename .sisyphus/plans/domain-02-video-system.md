# 视频模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | VideoService（视频服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 领域模型概览

### 2.1 领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    视频领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │   Video      │◄───── VideoStatus (值对象)               │
│  │ (聚合根)     │◄───── VideoMetadata (值对象)             │
│  └──────────────┘◄───── VideoStorageInfo (值对象)          │
│         │         ◄───── VideoStatistics (值对象)          │
│         │                                                  │
│         │                                                  │
│         ├────► VideoPart (实体，多分P视频)                  │
│         │                                                  │
│         ├────► VideoTranscodingTask (实体)                 │
│         │                                                  │
│         ├────► VideoAuditRecord (实体)                     │
│         │                                                  │
│                                                             │
│  ┌──────────────┐                                          │
│  │ UploadSession│                                          │
│  │ (聚合根)     │                                          │
│  │ (分片上传)   │                                          │
│  └──────────────┘                                          │
│                                                             │
│  值对象：                                                    │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │VideoStatus   │ │VideoMetadata │ │VideoStatistics│      │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │VideoStorage  │ │TranscodingInfo│ │AuditStatus   │       │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 Video（视频聚合根）

**聚合边界定义**：
- 聚合根：Video
- 内部实体：VideoPart（多分P视频）、VideoTranscodingTask（转码任务）、VideoAuditRecord（审核记录）
- 外部引用：UserId（用户ID，不直接引用用户聚合）

**职责**：
- 管理视频生命周期（上传→转码→审核→发布→删除）
- 管理多分P视频的顺序和状态
- 触发转码任务并管理转码进度
- 维护视频统计数据（播放量、点赞、收藏）

```csharp
/// <summary>
/// 视频聚合根
/// 业务规则：
/// 1. 无时长限制（用户需求）
/// 2. 无大小限制（用户需求）
/// 3. 所有清晰度免费（用户需求）
/// 4. 必须审核后才能发布
/// </summary>
public class Video : AggregateRoot<Guid>
{
    // ========== 外部引用（ID引用，不聚合引用） ==========
    public Guid UserId { get; private set; }               // UP主ID
    public Guid? CategoryId { get; private set; }          // 所属分类ID
    
    // ========== 值对象 ==========
    private VideoStatus _status;                           // 视频状态
    private VideoMetadata _metadata;                       // 视频元数据
    private VideoStorageInfo _storageInfo;                 // 存储信息
    private VideoStatistics _statistics;                   // 统计数据
    private VideoAuditStatus _auditStatus;                 // 审核状态
    
    // ========== 内部实体集合 ==========
    private readonly List<VideoPart> _parts = new();       // 多分P视频
    private readonly List<VideoTranscodingTask> _transcodingTasks = new(); // 转码任务
    private readonly List<VideoAuditRecord> _auditRecords = new();         // 审核记录
    
    // ========== 标题和描述 ==========
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string CoverUrl { get; private set; }           // 封面图
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    // ========== 导航属性（只读） ==========
    public IReadOnlyCollection<VideoPart> Parts => _parts.AsReadOnly();
    public IReadOnlyCollection<VideoTranscodingTask> TranscodingTasks => _transcodingTasks.AsReadOnly();
    public IReadOnlyCollection<VideoAuditRecord> AuditRecords => _auditRecords.AsReadOnly();
    
    // ========== 构造函数 ==========
    private Video() { } // EF Core
    
    private Video(
        Guid id,
        Guid userId,
        string title,
        string description,
        string coverUrl,
        VideoMetadata metadata,
        VideoStorageInfo storageInfo,
        Guid? categoryId = null) : base(id)
    {
        UserId = userId;
        Title = title;
        Description = description;
        CoverUrl = coverUrl;
        CategoryId = categoryId;
        _metadata = metadata;
        _storageInfo = storageInfo;
        _status = VideoStatus.Uploaded;            // 初始状态：已上传
        _auditStatus = VideoAuditStatus.Pending;   // 待审核
        _statistics = VideoStatistics.Empty();     // 初始统计为空
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new VideoUploadedEvent(Id, UserId, Title));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建新视频（单分P）
    /// </summary>
    public static Video CreateSinglePart(
        Guid userId,
        string title,
        string description,
        string coverUrl,
        VideoMetadata metadata,
        VideoStorageInfo storageInfo,
        Guid? categoryId = null)
    {
        var video = new Video(
            GuidGenerator.Create(),
            userId,
            title,
            description,
            coverUrl,
            metadata,
            storageInfo,
            categoryId);
        
        // 创建默认分P（单P视频）
        video._parts.Add(VideoPart.CreateFirstPart(
            video.Id,
            metadata.FileName,
            metadata.Duration));
        
        return video;
    }
    
    /// <summary>
    /// 创建新视频（多分P）
    /// </summary>
    public static Video CreateMultiPart(
        Guid userId,
        string title,
        string description,
        string coverUrl,
        IReadOnlyList<(string fileName, long duration)> partInfos,
        Guid? categoryId = null)
    {
        var video = new Video(
            GuidGenerator.Create(),
            userId,
            title,
            description,
            coverUrl,
            VideoMetadata.CreateMultiPart(partInfos),
            VideoStorageInfo.Empty(),
            categoryId);
        
        // 创建多个分P
        for (int i = 0; i < partInfos.Count; i++)
        {
            var part = VideoPart.Create(
                video.Id,
                i + 1, // 分P序号从1开始
                partInfos[i].fileName,
                partInfos[i].duration);
            video._parts.Add(part);
        }
        
        return video;
    }
    
    // ========== 业务方法（状态转换） ==========
    
    /// <summary>
    /// 添加转码任务
    /// 业务规则：为每个分P创建所有清晰度的转码任务
    /// </summary>
    public void AddTranscodingTasks(Guid partId, IReadOnlyList<VideoResolution> resolutions)
    {
        // 业务规则：只能为已上传的视频添加转码任务
        if (_status != VideoStatus.Uploaded)
            throw new BusinessException("只有已上传的视频可以添加转码任务");
        
        // 业务规则：必须验证分P存在
        var part = _parts.FirstOrDefault(p => p.Id == partId);
        if (part == null)
            throw new BusinessException($"分P不存在: {partId}");
        
        // 为每个清晰度创建转码任务
        foreach (var resolution in resolutions)
        {
            var task = VideoTranscodingTask.Create(
                Id,
                partId,
                resolution);
            _transcodingTasks.Add(task);
        }
        
        _status = VideoStatus.Transcoding; // 状态转换：进入转码中
    }
    
    /// <summary>
    /// 更新转码进度
    /// </summary>
    public void UpdateTranscodingProgress(Guid taskId, int progress, TranscodingStatus status)
    {
        var task = _transcodingTasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
            throw new BusinessException($"转码任务不存在: {taskId}");
        
        task.UpdateProgress(progress, status);
        
        // 业务规则：所有任务完成时，视频状态变为转码完成
        if (_transcodingTasks.All(t => t.Status == TranscodingStatus.Completed))
        {
            _status = VideoStatus.Transcoded;
            AddDomainEvent(new VideoTranscodingCompletedEvent(Id));
        }
    }
    
    /// <summary>
    /// 提交审核
    /// </summary>
    public void SubmitForAudit()
    {
        // 业务规则：只有转码完成的视频可以提交审核
        if (_status != VideoStatus.Transcoded)
            throw new BusinessException("只有转码完成的视频可以提交审核");
        
        _auditStatus = VideoAuditStatus.Pending;
        _status = VideoStatus.Auditing;
        
        var auditRecord = VideoAuditRecord.Create(Id, "提交审核", AuditAction.Submit);
        _auditRecords.Add(auditRecord);
        
        AddDomainEvent(new VideoSubmittedForAuditEvent(Id, UserId));
    }
    
    /// <summary>
    /// 审核通过
    /// </summary>
    public void ApproveAudit(string auditorId, string comment)
    {
        // 业务规则：只有待审核的视频可以审核通过
        if (_auditStatus != VideoAuditStatus.Pending)
            throw new BusinessException("只有待审核的视频可以审核");
        
        _auditStatus = VideoAuditStatus.Approved;
        _status = VideoStatus.PendingPublish; // 待发布
        
        var auditRecord = VideoAuditRecord.CreateApproved(Id, auditorId, comment);
        _auditRecords.Add(auditRecord);
        
        AddDomainEvent(new VideoAuditApprovedEvent(Id, UserId));
    }
    
    /// <summary>
    /// 审核拒绝
    /// </summary>
    public void RejectAudit(string auditorId, string reason)
    {
        if (_auditStatus != VideoAuditStatus.Pending)
            throw new BusinessException("只有待审核的视频可以审核");
        
        _auditStatus = VideoAuditStatus.Rejected;
        _status = VideoStatus.AuditFailed;
        
        var auditRecord = VideoAuditRecord.CreateRejected(Id, auditorId, reason);
        _auditRecords.Add(auditRecord);
        
        AddDomainEvent(new VideoAuditRejectedEvent(Id, UserId, reason));
    }
    
    /// <summary>
    /// 发布视频
    /// </summary>
    public void Publish()
    {
        // 业务规则：只有审核通过的视频可以发布
        if (_auditStatus != VideoAuditStatus.Approved)
            throw new BusinessException("只有审核通过的视频可以发布");
        
        _status = VideoStatus.Published;
        PublishedAt = DateTime.UtcNow;
        
        AddDomainEvent(new VideoPublishedEvent(Id, UserId, Title));
    }
    
    /// <summary>
    /// 下架视频
    /// </summary>
    public void Unpublish(string reason)
    {
        // 业务规则：只有已发布的视频可以下架
        if (_status != VideoStatus.Published)
            throw new BusinessException("只有已发布的视频可以下架");
        
        _status = VideoStatus.Unpublished;
        PublishedAt = null;
        
        AddDomainEvent(new VideoUnpublishedEvent(Id, UserId, reason));
    }
    
    /// <summary>
    /// 删除视频（软删除）
    /// </summary>
    public void Delete()
    {
        // 业务规则：任何状态都可以删除
        _status = VideoStatus.Deleted;
        IsDeleted = true;
        
        AddDomainEvent(new VideoDeletedEvent(Id, UserId));
    }
    
    // ========== 业务方法（数据更新） ==========
    
    /// <summary>
    /// 更新视频信息
    /// </summary>
    public void UpdateInfo(string title, string description, string coverUrl, Guid? categoryId)
    {
        Title = title;
        Description = description;
        CoverUrl = coverUrl;
        CategoryId = categoryId;
    }
    
    /// <summary>
    /// 更新存储信息（秒传场景）
    /// </summary>
    public void UpdateStorageInfo(VideoStorageInfo storageInfo)
    {
        _storageInfo = storageInfo;
    }
    
    /// <summary>
    /// 增加播放量
    /// </summary>
    public void IncrementPlayCount()
    {
        _statistics = _statistics.AddPlay();
    }
    
    /// <summary>
    /// 增加点赞数
    /// </summary>
    public void IncrementLikeCount()
    {
        _statistics = _statistics.AddLike();
    }
    
    /// <summary>
    /// 减少点赞数
    /// </summary>
    public void DecrementLikeCount()
    {
        _statistics = _statistics.RemoveLike();
    }
    
    /// <summary>
    /// 增加收藏数
    /// </summary>
    public void IncrementFavoriteCount()
    {
        _statistics = _statistics.AddFavorite();
    }
    
    /// <summary>
    /// 减少收藏数
    /// </summary>
    public void DecrementFavoriteCount()
    {
        _statistics = _statistics.RemoveFavorite();
    }
    
    /// <summary>
    /// 增加分享数
    /// </summary>
    public void IncrementShareCount()
    {
        _statistics = _statistics.AddShare();
    }
    
    /// <summary>
    /// 增加评论数
    /// </summary>
    public void IncrementCommentCount()
    {
        _statistics = _statistics.AddComment();
    }
    
    /// <summary>
    /// 减少评论数
    /// </summary>
    public void DecrementCommentCount()
    {
        _statistics = _statistics.RemoveComment();
    }
    
    // ========== 分P管理方法 ==========
    
    /// <summary>
    /// 添加分P
    /// </summary>
    public void AddPart(string fileName, long duration)
    {
        // 业务规则：只有未发布的视频可以添加分P
        if (_status == VideoStatus.Published)
            throw new BusinessException("已发布的视频不能添加分P");
        
        var nextIndex = _parts.Count + 1;
        var part = VideoPart.Create(Id, nextIndex, fileName, duration);
        _parts.Add(part);
    }
    
    /// <summary>
    /// 删除分P
    /// </summary>
    public void RemovePart(Guid partId)
    {
        if (_status == VideoStatus.Published)
            throw new BusinessException("已发布的视频不能删除分P");
        
        var part = _parts.FirstOrDefault(p => p.Id == partId);
        if (part == null)
            throw new BusinessException($"分P不存在: {partId}");
        
        _parts.Remove(part);
        
        // 重新排序分P序号
        for (int i = 0; i < _parts.Count; i++)
        {
            _parts[i].UpdateIndex(i + 1);
        }
    }
    
    /// <summary>
    /// 调整分P顺序
    /// </summary>
    public void ReorderParts(IReadOnlyList<Guid> newOrder)
    {
        if (_status == VideoStatus.Published)
            throw new BusinessException("已发布的视频不能调整分P顺序");
        
        // 验证所有分P都在新顺序中
        if (newOrder.Count != _parts.Count)
            throw new BusinessException("分P数量不匹配");
        
        if (!_parts.Select(p => p.Id).All(newOrder.Contains))
            throw new BusinessException("分PID不匹配");
        
        // 重新设置序号
        for (int i = 0; i < newOrder.Count; i++)
        {
            var part = _parts.First(p => p.Id == newOrder[i]);
            part.UpdateIndex(i + 1);
        }
    }
    
    // ========== 查询方法 ==========
    
    public VideoStatus GetStatus() => _status;
    public VideoMetadata GetMetadata() => _metadata;
    public VideoStorageInfo GetStorageInfo() => _storageInfo;
    public VideoStatistics GetStatistics() => _statistics;
    public VideoAuditStatus GetAuditStatus() => _auditStatus;
    
    public VideoPart GetPart(Guid partId) => _parts.FirstOrDefault(p => p.Id == partId);
    public VideoPart GetPartByIndex(int index) => _parts.FirstOrDefault(p => p.Index == index);
    
    public VideoTranscodingTask GetTranscodingTask(Guid taskId) => 
        _transcodingTasks.FirstOrDefault(t => t.Id == taskId);
    
    public List<VideoTranscodingTask> GetTranscodingTasksByPart(Guid partId) => 
        _transcodingTasks.Where(t => t.PartId == partId).ToList();
    
    public bool IsPublished => _status == VideoStatus.Published;
    public bool CanPlay => _status == VideoStatus.Published;
}
```

---

## 4. 值对象设计

### 4.1 VideoStatus（视频状态值对象）

```csharp
/// <summary>
/// 视频状态值对象
/// 状态流转：Uploaded → Transcoding → Transcoded → Auditing → PendingPublish → Published → Unpublished/Deleted
/// </summary>
public class VideoStatus : ValueObject
{
    // ========== 状态常量 ==========
    public static readonly VideoStatus Uploaded = new("uploaded", "已上传");
    public static readonly VideoStatus Transcoding = new("transcoding", "转码中");
    public static readonly VideoStatus Transcoded = new("transcoded", "转码完成");
    public static readonly VideoStatus Auditing = new("auditing", "审核中");
    public static readonly VideoStatus PendingPublish = new("pending_publish", "待发布");
    public static readonly VideoStatus Published = new("published", "已发布");
    public static readonly VideoStatus Unpublished = new("unpublished", "已下架");
    public static readonly VideoStatus AuditFailed = new("audit_failed", "审核失败");
    public static readonly VideoStatus Deleted = new("deleted", "已删除");
    
    // ========== 属性 ==========
    public string Code { get; }
    public string DisplayName { get; }
    
    // ========== 构造函数 ==========
    private VideoStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否可以播放
    /// </summary>
    public bool CanPlay() => this == Published;
    
    /// <summary>
    /// 是否可以编辑
    /// </summary>
    public bool CanEdit() => this != Published && this != Deleted;
    
    /// <summary>
    /// 是否可以删除
    /// </summary>
    public bool CanDelete() => this != Deleted;
    
    /// <summary>
    /// 是否需要审核
    /// </summary>
    public bool NeedAudit() => this == Transcoded;
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
    
    // ========== 所有状态列表 ==========
    public static readonly IReadOnlyList<VideoStatus> AllStatuses = new List<VideoStatus>
    {
        Uploaded, Transcoding, Transcoded, Auditing,
        PendingPublish, Published, Unpublished, AuditFailed, Deleted
    };
}
```

---

### 4.2 VideoMetadata（视频元数据值对象）

```csharp
/// <summary>
/// 视频元数据值对象
/// 业务规则：无时长限制、无大小限制（用户需求）
/// </summary>
public class VideoMetadata : ValueObject
{
    // ========== 属性 ==========
    public string FileName { get; }              // 原始文件名
    public string FileHash { get; }              // 文件MD5哈希（用于秒传）
    public long FileSize { get; }                // 文件大小（字节，无限制）
    public long Duration { get; }                // 视频时长（秒，无限制）
    public string MimeType { get; }              // MIME类型
    public string? Codec { get; }                // 编码格式
    public string? Resolution { get; }           // 原始分辨率
    public int? Bitrate { get; }                 // 码率
    public double? FrameRate { get; }            // 帧率
    
    // ========== 构造函数 ==========
    private VideoMetadata(
        string fileName,
        string fileHash,
        long fileSize,
        long duration,
        string mimeType,
        string? codec = null,
        string? resolution = null,
        int? bitrate = null,
        double? frameRate = null)
    {
        FileName = fileName;
        FileHash = fileHash;
        FileSize = fileSize;
        Duration = duration;
        MimeType = mimeType;
        Codec = codec;
        Resolution = resolution;
        Bitrate = bitrate;
        FrameRate = frameRate;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建单分P视频元数据
    /// </summary>
    public static VideoMetadata CreateSingle(
        string fileName,
        string fileHash,
        long fileSize,
        long duration,
        string mimeType)
    {
        return new VideoMetadata(fileName, fileHash, fileSize, duration, mimeType);
    }
    
    /// <summary>
    /// 创建多分P视频元数据（汇总）
    /// </summary>
    public static VideoMetadata CreateMultiPart(IReadOnlyList<(string fileName, long duration)> partInfos)
    {
        var totalDuration = partInfos.Sum(p => p.duration);
        return new VideoMetadata(
            "multi_part",
            "",
            0, // 多分P的总大小需计算
            totalDuration,
            "video/mp4");
    }
    
    /// <summary>
    /// 从转码结果更新
    /// </summary>
    public VideoMetadata WithCodecInfo(string codec, string resolution, int bitrate, double frameRate)
    {
        return new VideoMetadata(
            FileName,
            FileHash,
            FileSize,
            Duration,
            MimeType,
            codec,
            resolution,
            bitrate,
            frameRate);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 格式化时长显示
    /// </summary>
    public string FormatDuration()
    {
        if (Duration < 60)
            return $"{Duration}秒";
        else if (Duration < 3600)
            return $"{Duration / 60}分{Duration % 60}秒";
        else
            return $"{Duration / 3600}小时{(Duration % 3600) / 60}分{(Duration % 3600) % 60}秒";
    }
    
    /// <summary>
    /// 格式化文件大小显示
    /// </summary>
    public string FormatFileSize()
    {
        if (FileSize < 1024)
            return $"{FileSize}B";
        else if (FileSize < 1024 * 1024)
            return $"{FileSize / 1024}KB";
        else if (FileSize < 1024 * 1024 * 1024)
            return $"{FileSize / (1024 * 1024):F2}MB";
        else
            return $"{FileSize / (1024 * 1024 * 1024):F2}GB";
    }
    
    /// <summary>
    /// 是否可以秒传
    /// </summary>
    public bool CanInstantUpload() => !string.IsNullOrEmpty(FileHash);
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FileName;
        yield return FileHash;
        yield return FileSize;
        yield return Duration;
        yield return MimeType;
    }
}
```

---

### 4.3 VideoStorageInfo（视频存储信息值对象）

```csharp
/// <summary>
/// 视频存储信息值对象
/// 业务规则：仅MinIO本地存储，无CDN（用户需求）
/// </summary>
public class VideoStorageInfo : ValueObject
{
    // ========== 属性 ==========
    public string OriginalFileUrl { get; }       // 原始文件URL
    public IReadOnlyDictionary<VideoResolution, string> TranscodedUrls { get; } // 转码文件URL（各清晰度）
    public string? CoverUrl { get; }             // 封面图URL
    public StorageType StorageType { get; }      // 存储类型（MinIO）
    public string BucketName { get; }            // MinIO桶名
    
    // ========== 构造函数 ==========
    private VideoStorageInfo(
        string originalFileUrl,
        IReadOnlyDictionary<VideoResolution, string> transcodedUrls,
        string? coverUrl,
        StorageType storageType,
        string bucketName)
    {
        OriginalFileUrl = originalFileUrl;
        TranscodedUrls = transcodedUrls;
        CoverUrl = coverUrl;
        StorageType = storageType;
        BucketName = bucketName;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建空存储信息（上传初始化时）
    /// </summary>
    public static VideoStorageInfo Empty()
    {
        return new VideoStorageInfo(
            "",
            new Dictionary<VideoResolution, string>(),
            null,
            StorageType.MinIO,
            "videos");
    }
    
    /// <summary>
    /// 创建原始文件存储信息
    /// </summary>
    public static VideoStorageInfo ForOriginal(
        string originalFileUrl,
        string bucketName)
    {
        return new VideoStorageInfo(
            originalFileUrl,
            new Dictionary<VideoResolution, string>(),
            null,
            StorageType.MinIO,
            bucketName);
    }
    
    /// <summary>
    /// 添加转码文件URL
    /// </summary>
    public VideoStorageInfo AddTranscodedUrl(VideoResolution resolution, string url)
    {
        var newUrls = new Dictionary<VideoResolution, string>(TranscodedUrls);
        newUrls[resolution] = url;
        
        return new VideoStorageInfo(
            OriginalFileUrl,
            newUrls,
            CoverUrl,
            StorageType,
            BucketName);
    }
    
    /// <summary>
    /// 设置封面图URL
    /// </summary>
    public VideoStorageInfo WithCover(string coverUrl)
    {
        return new VideoStorageInfo(
            OriginalFileUrl,
            TranscodedUrls,
            coverUrl,
            StorageType,
            BucketName);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 获取指定清晰度的播放URL
    /// 业务规则：所有清晰度免费（用户需求）
    /// </summary>
    public string? GetPlayUrl(VideoResolution resolution)
    {
        return TranscodedUrls.TryGetValue(resolution, out var url) ? url : null;
    }
    
    /// <summary>
    /// 获取最高清晰度
    /// </summary>
    public VideoResolution GetHighestResolution()
    {
        if (TranscodedUrls.Count == 0)
            return VideoResolution.Original;
        
        return TranscodedUrls.Keys.MaxBy(r => r.PixelCount);
    }
    
    /// <summary>
    /// 获取所有可用清晰度
    /// </summary>
    public IReadOnlyList<VideoResolution> GetAvailableResolutions()
    {
        return TranscodedUrls.Keys.ToList();
    }
    
    /// <summary>
    /// 是否已完成转码
    /// </summary>
    public bool IsTranscoded() => TranscodedUrls.Count > 0;
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return OriginalFileUrl;
        yield return StorageType;
        yield return BucketName;
    }
}

/// <summary>
/// 存储类型枚举
/// </summary>
public enum StorageType
{
    MinIO = 1,      // 本地MinIO存储（用户需求：无CDN）
    CDN = 2         // CDN（暂不使用）
}
```

---

### 4.4 VideoStatistics（视频统计数据值对象）

```csharp
/// <summary>
/// 视频统计数据值对象
/// </summary>
public class VideoStatistics : ValueObject
{
    // ========== 属性 ==========
    public long PlayCount { get; }               // 播放量
    public long LikeCount { get; }               // 点赞数
    public long FavoriteCount { get; }           // 收藏数
    public long ShareCount { get; }              // 分享数
    public long CommentCount { get; }            // 评论数
    public long CoinCount { get; }               // 投币数
    public double AverageRating { get; }         // 平均评分
    
    // ========== 构造函数 ==========
    private VideoStatistics(
        long playCount,
        long likeCount,
        long favoriteCount,
        long shareCount,
        long commentCount,
        long coinCount,
        double averageRating)
    {
        PlayCount = playCount;
        LikeCount = likeCount;
        FavoriteCount = favoriteCount;
        ShareCount = shareCount;
        CommentCount = commentCount;
        CoinCount = coinCount;
        AverageRating = averageRating;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建空统计（新视频）
    /// </summary>
    public static VideoStatistics Empty()
    {
        return new VideoStatistics(0, 0, 0, 0, 0, 0, 0.0);
    }
    
    /// <summary>
    /// 从数据库重建
    /// </summary>
    public static VideoStatistics FromData(
        long playCount,
        long likeCount,
        long favoriteCount,
        long shareCount,
        long commentCount,
        long coinCount,
        double averageRating)
    {
        return new VideoStatistics(
            playCount,
            likeCount,
            favoriteCount,
            shareCount,
            commentCount,
            coinCount,
            averageRating);
    }
    
    // ========== 业务方法（增量操作） ==========
    
    public VideoStatistics AddPlay() => 
        new VideoStatistics(PlayCount + 1, LikeCount, FavoriteCount, ShareCount, CommentCount, CoinCount, AverageRating);
    
    public VideoStatistics AddLike() => 
        new VideoStatistics(PlayCount, LikeCount + 1, FavoriteCount, ShareCount, CommentCount, CoinCount, AverageRating);
    
    public VideoStatistics RemoveLike() => 
        new VideoStatistics(PlayCount, Math.Max(0, LikeCount - 1), FavoriteCount, ShareCount, CommentCount, CoinCount, AverageRating);
    
    public VideoStatistics AddFavorite() => 
        new VideoStatistics(PlayCount, LikeCount, FavoriteCount + 1, ShareCount, CommentCount, CoinCount, AverageRating);
    
    public VideoStatistics RemoveFavorite() => 
        new VideoStatistics(PlayCount, LikeCount, Math.Max(0, FavoriteCount - 1), ShareCount, CommentCount, CoinCount, AverageRating);
    
    public VideoStatistics AddShare() => 
        new VideoStatistics(PlayCount, LikeCount, FavoriteCount, ShareCount + 1, CommentCount, CoinCount, AverageRating);
    
    public VideoStatistics AddComment() => 
        new VideoStatistics(PlayCount, LikeCount, FavoriteCount, ShareCount, CommentCount + 1, CoinCount, AverageRating);
    
    public VideoStatistics RemoveComment() => 
        new VideoStatistics(PlayCount, LikeCount, FavoriteCount, ShareCount, Math.Max(0, CommentCount - 1), CoinCount, AverageRating);
    
    public VideoStatistics AddCoin(int amount) => 
        new VideoStatistics(PlayCount, LikeCount, FavoriteCount, ShareCount, CommentCount, CoinCount + amount, AverageRating);
    
    // ========== 业务方法（计算） ==========
    
    /// <summary>
    /// 计算热度值（用于推荐排序）
    /// </summary>
    public double CalculateHotScore()
    {
        // 热度公式：播放量权重 + 互动权重
        // 互动权重 = 点赞 + 2*收藏 + 2*投币 + 分享 + 评论
        var interactionScore = LikeCount + 2 * FavoriteCount + 2 * CoinCount + ShareCount + CommentCount;
        var hotScore = PlayCount * 0.3 + interactionScore * 0.7;
        return hotScore;
    }
    
    /// <summary>
    /// 计算互动率
    /// </summary>
    public double CalculateInteractionRate()
    {
        if (PlayCount == 0) return 0;
        var totalInteraction = LikeCount + FavoriteCount + ShareCount + CommentCount + CoinCount;
        return (double)totalInteraction / PlayCount;
    }
    
    /// <summary>
    /// 是否热门视频
    /// </summary>
    public bool IsHotVideo()
    {
        return CalculateHotScore() > 10000;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return PlayCount;
        yield return LikeCount;
        yield return FavoriteCount;
        yield return ShareCount;
        yield return CommentCount;
        yield return CoinCount;
        yield return AverageRating;
    }
}
```

---

### 4.5 VideoAuditStatus（审核状态值对象）

```csharp
/// <summary>
/// 视频审核状态值对象
/// </summary>
public class VideoAuditStatus : ValueObject
{
    // ========== 状态常量 ==========
    public static readonly VideoAuditStatus Pending = new("pending", "待审核");
    public static readonly VideoAuditStatus Approved = new("approved", "审核通过");
    public static readonly VideoAuditStatus Rejected = new("rejected", "审核拒绝");
    
    // ========== 属性 ==========
    public string Code { get; }
    public string DisplayName { get; }
    
    // ========== 构造函数 ==========
    private VideoAuditStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
}
```

---

### 4.6 VideoResolution（视频清晰度值对象）

```csharp
/// <summary>
/// 视频清晰度值对象
/// 业务规则：所有清晰度免费（用户需求）
/// </summary>
public class VideoResolution : ValueObject
{
    // ========== 清晰度常量 ==========
    public static readonly VideoResolution Original = new("original", "原始", 0, 0);
    public static readonly VideoResolution P360 = new("360p", "流畅", 360, 640);
    public static readonly VideoResolution P480 = new("480p", "清晰", 480, 854);
    public static readonly VideoResolution P720 = new("720p", "高清", 720, 1280);
    public static readonly VideoResolution P1080 = new("1080p", "超高清", 1080, 1920);
    public static readonly VideoResolution P4K = new("4k", "4K超清", 2160, 3840);
    
    // ========== 属性 ==========
    public string Code { get; }
    public string DisplayName { get; }
    public int Height { get; }                  // 垂直像素数
    public int Width { get; }                   // 水平像素数（默认）
    public int PixelCount => Height * Width;    // 总像素数
    
    // ========== 构造函数 ==========
    private VideoResolution(string code, string displayName, int height, int width)
    {
        Code = code;
        DisplayName = displayName;
        Height = height;
        Width = width;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否会员专享
    /// 业务规则：全部免费，返回false
    /// </summary>
    public bool IsMemberOnly() => false; // 用户需求：全部免费
    
    /// <summary>
    /// 获取码率建议（基于分辨率）
    /// </summary>
    public int GetRecommendedBitrate()
    {
        return this switch
        {
            _ when this == P360 => 800_000,      // 800kbps
            _ when this == P480 => 1_500_000,    // 1.5Mbps
            _ when this == P720 => 3_000_000,    // 3Mbps
            _ when this == P1080 => 5_000_000,   // 5Mbps
            _ when this == P4K => 15_000_000,    // 15Mbps
            _ => 0
        };
    }
    
    /// <summary>
    /// 比较清晰度高低
    /// </summary>
    public bool IsHigherThan(VideoResolution other)
    {
        return PixelCount > other.PixelCount;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
        yield return Height;
        yield return Width;
    }
    
    // ========== 所有清晰度列表 ==========
    public static readonly IReadOnlyList<VideoResolution> AllResolutions = new List<VideoResolution>
    {
        P360, P480, P720, P1080, P4K
    };
}
```

---

## 5. 实体设计

### 5.1 VideoPart（视频分P实体）

```csharp
/// <summary>
/// 视频分P实体
/// 业务规则：多分P视频，每个分P独立转码
/// </summary>
public class VideoPart : Entity<Guid>
{
    // ========== 外部引用 ==========
    public Guid VideoId { get; private set; }    // 所属视频ID
    
    // ========== 属性 ==========
    public int Index { get; private set; }       // 分P序号（从1开始）
    public string Title { get; private set; }    // 分P标题（可选）
    public string FileName { get; private set; } // 文件名
    public long Duration { get; private set; }   // 分P时长（秒）
    public string StorageUrl { get; private set; } // 存储URL
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private VideoPart() { } // EF Core
    
    private VideoPart(
        Guid id,
        Guid videoId,
        int index,
        string fileName,
        long duration) : base(id)
    {
        VideoId = videoId;
        Index = index;
        Title = fileName; // 默认使用文件名作为标题
        FileName = fileName;
        Duration = duration;
        StorageUrl = "";
        CreatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建第一个分P（单P视频默认）
    /// </summary>
    public static VideoPart CreateFirstPart(Guid videoId, string fileName, long duration)
    {
        return new VideoPart(GuidGenerator.Create(), videoId, 1, fileName, duration);
    }
    
    /// <summary>
    /// 创建分P
    /// </summary>
    public static VideoPart Create(Guid videoId, int index, string fileName, long duration)
    {
        if (index < 1)
            throw new BusinessException("分P序号必须从1开始");
        
        return new VideoPart(GuidGenerator.Create(), videoId, index, fileName, duration);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 更新分P标题
    /// </summary>
    public void UpdateTitle(string title)
    {
        Title = title;
    }
    
    /// <summary>
    /// 更新分P序号
    /// </summary>
    public void UpdateIndex(int newIndex)
    {
        if (newIndex < 1)
            throw new BusinessException("分P序号必须从1开始");
        
        Index = newIndex;
    }
    
    /// <summary>
    /// 更新存储URL
    /// </summary>
    public void UpdateStorageUrl(string url)
    {
        StorageUrl = url;
    }
    
    /// <summary>
    /// 格式化时长显示
    /// </summary>
    public string FormatDuration()
    {
        if (Duration < 60)
            return $"{Duration}秒";
        else if (Duration < 3600)
            return $"{Duration / 60}分{Duration % 60}秒";
        else
            return $"{Duration / 3600}小时{(Duration % 3600) / 60}分";
    }
}
```

---

### 5.2 VideoTranscodingTask（视频转码任务实体）

```csharp
/// <summary>
/// 视频转码任务实体
/// 业务规则：CPU转码（用户需求）
/// </summary>
public class VideoTranscodingTask : Entity<Guid>
{
    // ========== 外部引用 ==========
    public Guid VideoId { get; private set; }    // 所属视频ID
    public Guid PartId { get; private set; }     // 所属分PID
    
    // ========== 属性 ==========
    public VideoResolution Resolution { get; private set; } // 目标清晰度
    public TranscodingStatus Status { get; private set; }   // 转码状态
    public int Progress { get; private set; }               // 进度百分比
    public string OutputUrl { get; private set; }           // 输出文件URL
    public string? ErrorMessage { get; private set; }       // 错误信息
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private VideoTranscodingTask() { } // EF Core
    
    private VideoTranscodingTask(
        Guid id,
        Guid videoId,
        Guid partId,
        VideoResolution resolution) : base(id)
    {
        VideoId = videoId;
        PartId = partId;
        Resolution = resolution;
        Status = TranscodingStatus.Pending;
        Progress = 0;
        OutputUrl = "";
        CreatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    public static VideoTranscodingTask Create(
        Guid videoId,
        Guid partId,
        VideoResolution resolution)
    {
        return new VideoTranscodingTask(
            GuidGenerator.Create(),
            videoId,
            partId,
            resolution);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 开始转码
    /// </summary>
    public void Start()
    {
        Status = TranscodingStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 更新进度
    /// </summary>
    public void UpdateProgress(int progress, TranscodingStatus status)
    {
        if (progress < 0 || progress > 100)
            throw new BusinessException("进度必须在0-100之间");
        
        Progress = progress;
        Status = status;
        
        if (status == TranscodingStatus.Completed)
        {
            CompletedAt = DateTime.UtcNow;
        }
    }
    
    /// <summary>
    /// 标记完成
    /// </summary>
    public void MarkCompleted(string outputUrl)
    {
        Status = TranscodingStatus.Completed;
        Progress = 100;
        OutputUrl = outputUrl;
        CompletedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 标记失败
    /// </summary>
    public void MarkFailed(string errorMessage)
    {
        Status = TranscodingStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 是否完成
    /// </summary>
    public bool IsCompleted => Status == TranscodingStatus.Completed;
    
    /// <summary>
    /// 是否失败
    /// </summary>
    public bool IsFailed => Status == TranscodingStatus.Failed;
}

/// <summary>
/// 转码状态枚举
/// </summary>
public enum TranscodingStatus
{
    Pending = 0,        // 待转码
    InProgress = 1,     // 转码中
    Completed = 2,      // 完成
    Failed = 3          // 失败
}
```

---

### 5.3 VideoAuditRecord（视频审核记录实体）

```csharp
/// <summary>
/// 视频审核记录实体
/// </summary>
public class VideoAuditRecord : Entity<Guid>
{
    // ========== 外部引用 ==========
    public Guid VideoId { get; private set; }    // 所属视频ID
    
    // ========== 属性 ==========
    public AuditAction Action { get; private set; }   // 审核动作
    public string? AuditorId { get; private set; }    // 审核员ID
    public string? Comment { get; private set; }      // 审核意见
    public string? Reason { get; private set; }       // 拒绝原因
    public DateTime OccurredAt { get; private set; }  // 发生时间
    
    // ========== 构造函数 ==========
    private VideoAuditRecord() { } // EF Core
    
    private VideoAuditRecord(
        Guid id,
        Guid videoId,
        AuditAction action,
        string? auditorId = null,
        string? comment = null,
        string? reason = null) : base(id)
    {
        VideoId = videoId;
        Action = action;
        AuditorId = auditorId;
        Comment = comment;
        Reason = reason;
        OccurredAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    public static VideoAuditRecord Create(Guid videoId, string note, AuditAction action)
    {
        return new VideoAuditRecord(
            GuidGenerator.Create(),
            videoId,
            action,
            null,
            note,
            null);
    }
    
    public static VideoAuditRecord CreateApproved(Guid videoId, string auditorId, string comment)
    {
        return new VideoAuditRecord(
            GuidGenerator.Create(),
            videoId,
            AuditAction.Approve,
            auditorId,
            comment,
            null);
    }
    
    public static VideoAuditRecord CreateRejected(Guid videoId, string auditorId, string reason)
    {
        return new VideoAuditRecord(
            GuidGenerator.Create(),
            videoId,
            AuditAction.Reject,
            auditorId,
            null,
            reason);
    }
}

/// <summary>
/// 审核动作枚举
/// </summary>
public enum AuditAction
{
    Submit = 0,         // 提交审核
    Approve = 1,        // 通过
    Reject = 2,         // 拒绝
    Withdraw = 3        // 撤回
}
```

---

## 6. 聚合根设计：UploadSession（分片上传聚合）

### 6.1 UploadSession（上传会话聚合根）

```csharp
/// <summary>
/// 上传会话聚合根（分片上传管理）
/// </summary>
public class UploadSession : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 上传者ID
    
    // ========== 属性 ==========
    public string FileName { get; private set; } // 文件名
    public string FileHash { get; private set; } // 文件哈希（用于秒传）
    public long FileSize { get; private set; }   // 文件大小
    public int TotalChunks { get; private set; } // 总分片数
    public int ChunkSize { get; private set; }   // 分片大小（5MB）
    public UploadStatus Status { get; private set; } // 上传状态
    
    // ========== 内部实体 ==========
    private readonly List<UploadChunk> _chunks = new();
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; } // 过期时间（24小时）
    
    // ========== 导航属性 ==========
    public IReadOnlyCollection<UploadChunk> Chunks => _chunks.AsReadOnly();
    
    // ========== 构造函数 ==========
    private UploadSession() { }
    
    private UploadSession(
        Guid id,
        Guid userId,
        string fileName,
        string fileHash,
        long fileSize,
        int chunkSize) : base(id)
    {
        UserId = userId;
        FileName = fileName;
        FileHash = fileHash;
        FileSize = fileSize;
        ChunkSize = chunkSize;
        TotalChunks = (int)Math.Ceiling(fileSize / (double)chunkSize);
        Status = UploadStatus.Initialized;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddHours(24);
        
        AddDomainEvent(new UploadSessionCreatedEvent(Id, UserId, FileName));
    }
    
    // ========== 工厂方法 ==========
    
    public static UploadSession Create(
        Guid userId,
        string fileName,
        string fileHash,
        long fileSize,
        int chunkSize = 5 * 1024 * 1024) // 默认5MB
    {
        return new UploadSession(
            GuidGenerator.Create(),
            userId,
            fileName,
            fileHash,
            fileSize,
            chunkSize);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 上传分片
    /// </summary>
    public void UploadChunk(int chunkIndex, string chunkHash)
    {
        // 业务规则：验证会话未过期
        if (DateTime.UtcNow > ExpiresAt)
            throw new BusinessException("上传会话已过期");
        
        // 业务规则：验证分片序号有效
        if (chunkIndex < 1 || chunkIndex > TotalChunks)
            throw new BusinessException($"分片序号无效: {chunkIndex}");
        
        // 业务规则：验证分片未上传
        if (_chunks.Any(c => c.Index == chunkIndex))
            throw new BusinessException($"分片已上传: {chunkIndex}");
        
        var chunk = UploadChunk.Create(Id, chunkIndex, chunkHash);
        _chunks.Add(chunk);
        
        // 更新状态
        if (_chunks.Count == TotalChunks)
        {
            Status = UploadStatus.AllChunksUploaded;
        }
        else
        {
            Status = UploadStatus.InProgress;
        }
    }
    
    /// <summary>
    /// 完成上传（合并分片）
    /// </summary>
    public void Complete()
    {
        // 业务规则：必须所有分片已上传
        if (_chunks.Count != TotalChunks)
            throw new BusinessException($"还有{TotalChunks - _chunks.Count}个分片未上传");
        
        Status = UploadStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UploadCompletedEvent(Id, UserId, FileName, FileHash));
    }
    
    /// <summary>
    /// 取消上传
    /// </summary>
    public void Cancel()
    {
        Status = UploadStatus.Cancelled;
        AddDomainEvent(new UploadCancelledEvent(Id, UserId));
    }
    
    /// <summary>
    /// 获取已上传分片列表
    /// </summary>
    public List<int> GetUploadedChunkIndexes()
    {
        return _chunks.Select(c => c.Index).ToList();
    }
    
    /// <summary>
    /// 获取上传进度
    /// </summary>
    public int GetProgress()
    {
        return (int)((double)_chunks.Count / TotalChunks * 100);
    }
    
    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    
    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsCompleted => Status == UploadStatus.Completed;
}

/// <summary>
/// 上传状态枚举
/// </summary>
public enum UploadStatus
{
    Initialized = 0,        // 已初始化
    InProgress = 1,         // 上传中
    AllChunksUploaded = 2,  // 所有分片已上传
    Completed = 3,          // 已完成
    Cancelled = 4           // 已取消
}

/// <summary>
/// 上传分片实体
/// </summary>
public class UploadChunk : Entity<Guid>
{
    public Guid SessionId { get; private set; }
    public int Index { get; private set; }
    public string ChunkHash { get; private set; }
    public DateTime UploadedAt { get; private set; }
    
    private UploadChunk() { }
    
    private UploadChunk(Guid id, Guid sessionId, int index, string chunkHash) : base(id)
    {
        SessionId = sessionId;
        Index = index;
        ChunkHash = chunkHash;
        UploadedAt = DateTime.UtcNow;
    }
    
    public static UploadChunk Create(Guid sessionId, int index, string chunkHash)
    {
        return new UploadChunk(GuidGenerator.Create(), sessionId, index, chunkHash);
    }
}
```

---

## 7. 领域服务设计

### 7.1 IVideoTranscodingService（视频转码领域服务）

```csharp
/// <summary>
/// 视频转码领域服务接口
/// 业务规则：CPU转码（用户需求）
/// </summary>
public interface IVideoTranscodingService
{
    /// <summary>
    /// 触发转码任务
    /// </summary>
    Task<Guid> StartTranscodingAsync(
        Guid videoId,
        Guid partId,
        VideoResolution resolution,
        string inputFilePath);
    
    /// <summary>
    /// 查询转码进度
    /// </summary>
    Task<(TranscodingStatus Status, int Progress, string? OutputPath)> GetProgressAsync(Guid taskId);
    
    /// <summary>
    /// 取消转码任务
    /// </summary>
    Task CancelTranscodingAsync(Guid taskId);
    
    /// <summary>
    /// 获取转码输出文件信息
    /// </summary>
    Task<VideoMetadata> GetTranscodedMetadataAsync(string outputFilePath);
    
    /// <summary>
    /// 提取视频封面图
    /// </summary>
    Task<string> ExtractCoverAsync(string videoFilePath, int timestampSeconds);
}
```

---

### 7.2 IVideoAuditService（视频审核领域服务）

```csharp
/// <summary>
/// 视频审核领域服务接口
/// </summary>
public interface IVideoAuditService
{
    /// <summary>
    /// 审核视频内容（AI预审核）
    /// </summary>
    Task<AuditResult> PreAuditVideoAsync(Guid videoId);
    
    /// <summary>
    /// 提交人工审核
    /// </summary>
    Task SubmitForManualAuditAsync(Guid videoId, string? aiAuditNote);
    
    /// <summary>
    /// 获取待审核视频列表
    /// </summary>
    Task<IReadOnlyList<Guid>> GetPendingAuditVideosAsync(int limit);
}

/// <summary>
/// 审核结果
/// </summary>
public class AuditResult
{
    public bool IsApproved { get; set; }
    public string? Reason { get; set; }
    public List<string> FlaggedContent { get; set; } = new();
}
```

---

### 7.3 IVideoStorageService（视频存储领域服务）

```csharp
/// <summary>
/// 视频存储领域服务接口
/// 业务规则：仅MinIO本地存储（用户需求）
/// </summary>
public interface IVideoStorageService
{
    /// <summary>
    /// 保存原始视频文件
    /// </summary>
    Task<string> SaveOriginalFileAsync(
        Guid videoId,
        string fileName,
        Stream fileStream);
    
    /// <summary>
    /// 保存转码文件
    /// </summary>
    Task<string> SaveTranscodedFileAsync(
        Guid videoId,
        Guid partId,
        VideoResolution resolution,
        Stream fileStream);
    
    /// <summary>
    /// 获取视频文件URL
    /// </summary>
    string GetFileUrl(Guid videoId, string fileName);
    
    /// <summary>
    /// 删除视频文件
    /// </summary>
    Task DeleteVideoFilesAsync(Guid videoId);
    
    /// <summary>
    /// 检查秒传（文件哈希是否存在）
    /// </summary>
    Task<bool> CheckInstantUploadAsync(string fileHash);
    
    /// <summary>
    /// 获取秒传文件URL
    /// </summary>
    Task<string?> GetInstantUploadUrlAsync(string fileHash);
    
    /// <summary>
    /// 记录文件哈希（用于秒传）
    /// </summary>
    Task RecordFileHashAsync(string fileHash, Guid videoId, string fileUrl);
}
```

---

### 7.4 IVideoStatisticsService（视频统计领域服务）

```csharp
/// <summary>
/// 视频统计领域服务接口
/// </summary>
public interface IVideoStatisticsService
{
    /// <summary>
    /// 批量更新播放量（从Redis批量同步）
    /// </summary>
    Task UpdatePlayCountBatchAsync(IReadOnlyDictionary<Guid, long> playCounts);
    
    /// <summary>
    /// 获取视频统计数据
    /// </summary>
    Task<VideoStatistics> GetStatisticsAsync(Guid videoId);
    
    /// <summary>
    /// 获取热门视频列表（基于热度算法）
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double HotScore)>> GetHotVideosAsync(int limit);
    
    /// <summary>
    /// 计算视频热度值
    /// </summary>
    double CalculateHotScore(VideoStatistics statistics);
}
```

---

## 8. 仓储接口设计

### 8.1 IVideoRepository

```csharp
/// <summary>
/// 视频仓储接口
/// </summary>
public interface IVideoRepository : IRepository<Video, Guid>
{
    /// <summary>
    /// 根据用户ID获取视频列表
    /// </summary>
    Task<List<Video>> GetByUserIdAsync(Guid userId, int skip, int take);
    
    /// <summary>
    /// 根据分类ID获取视频列表
    /// </summary>
    Task<List<Video>> GetByCategoryIdAsync(Guid categoryId, int skip, int take);
    
    /// <summary>
    /// 获取已发布视频列表
    /// </summary>
    Task<List<Video>> GetPublishedVideosAsync(int skip, int take);
    
    /// <summary>
    /// 获取待审核视频列表
    /// </summary>
    Task<List<Video>> GetPendingAuditVideosAsync(int skip, int take);
    
    /// <summary>
    /// 根据状态获取视频列表
    /// </summary>
    Task<List<Video>> GetByStatusAsync(VideoStatus status, int skip, int take);
    
    /// <summary>
    /// 搜索视频（标题、描述）
    /// </summary>
    Task<List<Video>> SearchAsync(string keyword, int skip, int take);
    
    /// <summary>
    /// 获取用户视频数量
    /// </summary>
    Task<long> CountByUserIdAsync(Guid userId);
    
    /// <summary>
    /// 包含分P和转码任务的查询
    /// </summary>
    Task<Video?> GetWithDetailsAsync(Guid id);
}
```

---

### 8.2 IUploadSessionRepository

```csharp
/// <summary>
/// 上传会话仓储接口
/// </summary>
public interface IUploadSessionRepository : IRepository<UploadSession, Guid>
{
    /// <summary>
    /// 根据用户ID获取活跃上传会话
    /// </summary>
    Task<List<UploadSession>> GetActiveSessionsByUserIdAsync(Guid userId);
    
    /// <summary>
    /// 根据文件哈希获取已完成的上传会话（秒传）
    /// </summary>
    Task<UploadSession?> GetByFileHashAsync(string fileHash);
    
    /// <summary>
    /// 清理过期会话
    /// </summary>
    Task<int> CleanupExpiredSessionsAsync();
}
```

---

## 9. 领域事件设计

### 9.1 领域事件列表

```csharp
/// <summary>
/// 视频上传完成事件
/// </summary>
public class VideoUploadedEvent
{
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }
    
    public VideoUploadedEvent(Guid videoId, Guid userId, string title)
    {
        VideoId = videoId;
        UserId = userId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 视频转码完成事件
/// </summary>
public class VideoTranscodingCompletedEvent
{
    public Guid VideoId { get; }
    public DateTime OccurredOn { get; }
    
    public VideoTranscodingCompletedEvent(Guid videoId)
    {
        VideoId = videoId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 视频提交审核事件
/// </summary>
public class VideoSubmittedForAuditEvent
{
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public VideoSubmittedForAuditEvent(Guid videoId, Guid userId)
    {
        VideoId = videoId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 视频审核通过事件
/// </summary>
public class VideoAuditApprovedEvent
{
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public VideoAuditApprovedEvent(Guid videoId, Guid userId)
    {
        VideoId = videoId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 视频审核拒绝事件
/// </summary>
public class VideoAuditRejectedEvent
{
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public string Reason { get; }
    public DateTime OccurredOn { get; }
    
    public VideoAuditRejectedEvent(Guid videoId, Guid userId, string reason)
    {
        VideoId = videoId;
        UserId = userId;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 视频发布事件
/// </summary>
public class VideoPublishedEvent
{
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }
    
    public VideoPublishedEvent(Guid videoId, Guid userId, string title)
    {
        VideoId = videoId;
        UserId = userId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 视频下架事件
/// </summary>
public class VideoUnpublishedEvent
{
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public string Reason { get; }
    public DateTime OccurredOn { get; }
    
    public VideoUnpublishedEvent(Guid videoId, Guid userId, string reason)
    {
        VideoId = videoId;
        UserId = userId;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 视频删除事件
/// </summary>
public class VideoDeletedEvent
{
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public VideoDeletedEvent(Guid videoId, Guid userId)
    {
        VideoId = videoId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 上传会话创建事件
/// </summary>
public class UploadSessionCreatedEvent
{
    public Guid SessionId { get; }
    public Guid UserId { get; }
    public string FileName { get; }
    public DateTime OccurredOn { get; }
    
    public UploadSessionCreatedEvent(Guid sessionId, Guid userId, string fileName)
    {
        SessionId = sessionId;
        UserId = userId;
        FileName = fileName;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 上传完成事件
/// </summary>
public class UploadCompletedEvent
{
    public Guid SessionId { get; }
    public Guid UserId { get; }
    public string FileName { get; }
    public string FileHash { get; }
    public DateTime OccurredOn { get; }
    
    public UploadCompletedEvent(Guid sessionId, Guid userId, string fileName, string fileHash)
    {
        SessionId = sessionId;
        UserId = userId;
        FileName = fileName;
        FileHash = fileHash;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 上传取消事件
/// </summary>
public class UploadCancelledEvent
{
    public Guid SessionId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public UploadCancelledEvent(Guid sessionId, Guid userId)
    {
        SessionId = sessionId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}
```

---

## 10. 业务规则总结

### 10.1 视频上传规则

| 规则 | 说明 |
|------|------|
| 视频时长 | 无限制（用户需求） |
| 文件大小 | 无限制（用户需求） |
| 分片大小 | 5MB（默认） |
| 秒传检测 | 文件Hash匹配即秒传 |
| 会话过期 | 24小时 |

---

### 10.2 视频转码规则

| 规则 | 说明 |
|------|------|
| 转码方式 | CPU转码（成本低，用户需求） |
| 清晰度 | 360P, 480P, 720P, 1080P, 4K |
| 清晰度收费 | 全部免费（无会员特权，用户需求） |
| 码率建议 | 360P:800k, 480P:1.5M, 720P:3M, 1080P:5M, 4K:15M |

---

### 10.3 视频审核规则

| 规则 | 说明 |
|------|------|
| 审核时机 | 转码完成后必须审核 |
| 审核流程 | AI预审核 → 人工审核 |
| 发布条件 | 审核通过才能发布 |
| 拒绝处理 | 拒绝后可重新修改提交 |

---

### 10.4 视频存储规则

| 规则 | 说明 |
|------|------|
| 存储方式 | MinIO本地存储（用户需求） |
| CDN | 不使用CDN（用户需求） |
| 桶命名 | videos |
| 文件路径 | videos/{videoId}/{resolution}/{fileName} |

---

### 10.5 多分P视频规则

| 规则 | 说明 |
|------|------|
| 分P数量 | 无限制 |
| 分P序号 | 从1开始 |
| 分P标题 | 可自定义，默认文件名 |
| 分P转码 | 每个分P独立转码所有清晰度 |
| 分P管理 | 已发布视频不能修改分P |

---

### 10.6 统计数据规则

| 规则 | 说明 |
|------|------|
| 播放量 | 每次播放+1 |
| 点赞数 | 点赞+1，取消点赞-1 |
| 收藏数 | 收藏+1，取消收藏-1 |
| 投币数 | 从用户模块同步 |
| 评论数 | 从互动模块同步 |
| 热度计算 | 播放量*0.3 + 互动权重*0.7 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 视频模块领域设计文档完成