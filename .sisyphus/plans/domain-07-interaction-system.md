# 互动模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | InteractionService（互动服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 题领域模型概览

### 2.1 题领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    互动领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │  Like        │◄───── LikeTarget (值对象)                │
│  │ (聚合根)     │◄───── LikeStatus (值对象)                │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │  Favorite    │◄───── FavoriteFolder (值对象)            │
│  │ (聚合根)     │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │  Coin        │◄───── CoinAmount (值对象)                │
│  │ (聚合根)     │◄───── CoinTransfer (值对象)              │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │  Comment     │◄───── CommentStatus (值对象)             │
│  │ (聚合根)     │◄───── CommentContent (值对象)            │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │  Share       │                                          │
│  │ (聚合根)     │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ WatchHistory │                                          │
│  │ (聚合根)     │                                          │
│  └──────────────┘                                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 Like（点赞聚合根）

```csharp
/// <summary>
/// 点赞聚合根
/// </summary>
public class Like : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 用户ID
    public Guid TargetId { get; private set; }   // 目标ID（视频/评论）
    
    // ========== 值对象 ==========
    private LikeTarget _target;                  // 点赞目标
    private LikeStatus _status;                  // 点赞状态
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    
    // ========== 构造函数 ==========
    private Like() { }
    
    private Like(
        Guid id,
        Guid userId,
        Guid targetId,
        LikeTargetType targetType) : base(id)
    {
        UserId = userId;
        TargetId = targetId;
        _target = LikeTarget.Create(targetId, targetType);
        _status = LikeStatus.Active;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new LikeCreatedEvent(Id, UserId, TargetId, targetType));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 点赞视频
    /// </summary>
    public static Like LikeVideo(Guid userId, Guid videoId)
    {
        return new Like(GuidGenerator.Create(), userId, videoId, LikeTargetType.Video);
    }
    
    /// <summary>
    /// 点赞评论
    /// </summary>
    public static Like LikeComment(Guid userId, Guid commentId)
    {
        return new Like(GuidGenerator.Create(), userId, commentId, LikeTargetType.Comment);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 取消点赞
    /// </summary>
    public void Cancel()
    {
        if (_status == LikeStatus.Cancelled)
            throw new BusinessException("已经取消点赞");
        
        _status = LikeStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        
        AddDomainEvent(new LikeCancelledEvent(Id, UserId, TargetId, _target.Type));
    }
    
    /// <summary>
    /// 恢复点赞
    /// </summary>
    public void Restore()
    {
        if (_status != LikeStatus.Cancelled)
            throw new BusinessException("点赞未取消");
        
        _status = LikeStatus.Active;
        CancelledAt = null;
        
        AddDomainEvent(new LikeRestoredEvent(Id, UserId, TargetId, _target.Type));
    }
    
    // ========== 查询方法 ==========
    
    public LikeTarget GetTarget() => _target;
    public LikeStatus GetStatus() => _status;
    public LikeTargetType GetTargetType() => _target.Type;
    
    public bool IsActive => _status == LikeStatus.Active;
    public bool IsCancelled => _status == LikeStatus.Cancelled;
}

/// <summary>
/// 点赞目标类型
/// </summary>
public enum LikeTargetType
{
    Video = 1,      // 视频
    Comment = 2     // 评论
}
```

---

### 3.2 Favorite（收藏聚合根）

```csharp
/// <summary>
/// 收藏聚合根
/// </summary>
public class Favorite : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 用户ID
    public Guid VideoId { get; private set; }    // 视频ID
    public Guid? FolderId { get; private set; }  // 收藏夹ID
    
    // ========== 属性 ==========
    public string? Note { get; private set; }    // 收藏备注
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? RemovedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private Favorite() { }
    
    private Favorite(
        Guid id,
        Guid userId,
        Guid videoId,
        Guid? folderId,
        string? note) : base(id)
    {
        UserId = userId;
        VideoId = videoId;
        FolderId = folderId;
        Note = note;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new FavoriteCreatedEvent(Id, UserId, VideoId));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 收藏视频到默认收藏夹
    /// </summary>
    public static Favorite AddToFavorites(Guid userId, Guid videoId, string? note = null)
    {
        return new Favorite(GuidGenerator.Create(), userId, videoId, null, note);
    }
    
    /// <summary>
    /// 收藏视频到指定收藏夹
    /// </summary>
    public static Favorite AddToFolder(Guid userId, Guid videoId, Guid folderId, string? note = null)
    {
        return new Favorite(GuidGenerator.Create(), userId, videoId, folderId, note);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 取消收藏
    /// </summary>
    public void Remove()
    {
        IsDeleted = true;
        RemovedAt = DateTime.UtcNow;
        
        AddDomainEvent(new FavoriteRemovedEvent(Id, UserId, VideoId));
    }
    
    /// <summary>
    /// 更新收藏备注
    /// </summary>
    public void UpdateNote(string newNote)
    {
        Note = newNote;
    }
    
    /// <summary>
    /// 移动到其他收藏夹
    /// </summary>
    public void MoveToFolder(Guid newFolderId)
    {
        FolderId = newFolderId;
    }
    
    public bool IsRemoved => IsDeleted;
}
```

---

### 3.3 Coin（投币聚合根）

```csharp
/// <summary>
/// 投币聚合根
/// 业务规则：投币数量无限制（用户需求）
/// </summary>
public class Coin : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 投币用户ID
    public Guid VideoId { get; private set; }    // 视频ID
    public Guid UpOwnerId { get; private set; }  // UP主ID
    
    // ========== 值对象 ==========
    private CoinAmount _amount;                  // 投币金额
    private CoinTransfer _transfer;              // 转账信息
    
    // ========== 属性 ==========
    public int CoinCount { get; private set; }   // 投币数量
    public string? Message { get; private set; } // 投币留言
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private Coin() { }
    
    private Coin(
        Guid id,
        Guid userId,
        Guid videoId,
        Guid upOwnerId,
        int coinCount,
        string? message) : base(id)
    {
        UserId = userId;
        VideoId = videoId;
        UpOwnerId = upOwnerId;
        CoinCount = coinCount;
        Message = message;
        
        _amount = CoinAmount.Create(coinCount);
        _transfer = CoinTransfer.CreateForBilibili(userId, upOwnerId, coinCount); // 100%给UP主
        
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CoinSentEvent(Id, UserId, VideoId, UpOwnerId, coinCount));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 投币（无限制）
    /// </summary>
    public static Coin SendCoin(
        Guid userId,
        Guid videoId,
        Guid upOwnerId,
        int coinCount,
        string? message = null)
    {
        // 业务规则：投币数量无限制（用户需求）
        if (coinCount < 1)
            throw new BusinessException("投币数量至少为1");
        
        // 业务规则：单次投币最多2个（业务约定）
        if (coinCount > 2)
            throw new BusinessException("单次投币最多2个");
        
        return new Coin(GuidGenerator.Create(), userId, videoId, upOwnerId, coinCount, message);
    }
    
    // ========== 查询方法 ==========
    
    public CoinAmount GetAmount() => _amount;
    public CoinTransfer GetTransfer() => _transfer;
    
    public decimal GetUpOwnerAmount() => _transfer.UpOwnerAmount;
    
    /// <summary>
    /// 是否大额投币（>1）
    /// </summary>
    public bool IsLargeCoin => CoinCount > 1;
}
```

---

### 3.4 Comment（评论聚合根）

```csharp
/// <summary>
/// 评论聚合根
/// 业务规则：评论先审后发（人工审核）（用户需求）
/// </summary>
public class Comment : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 评论用户ID
    public Guid VideoId { get; private set; }    // 视频ID
    public Guid? ParentId { get; private set; }  // 父评论ID（回复）
    public Guid? RootId { get; private set; }    // 根评论ID（多层回复）
    
    // ========== 值对象 ==========
    private CommentContent _content;             // 评论内容
    private CommentStatus _status;               // 评论状态
    
    // ========== 属性 ==========
    public int LikeCount { get; private set; }   // 点赞数
    public int ReplyCount { get; private set; }  // 回复数
    public int Level { get; private set; }       // 评论层级（0=根评论，1=一级回复...）
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? AuditedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private Comment() { }
    
    private Comment(
        Guid id,
        Guid userId,
        Guid videoId,
        Guid? parentId,
        Guid? rootId,
        string content,
        int level) : base(id)
    {
        UserId = userId;
        VideoId = videoId;
        ParentId = parentId;
        RootId = rootId;
        Level = level;
        
        _content = CommentContent.Create(content);
        _status = CommentStatus.Pending; // 先审后发：初始状态为待审核
        
        LikeCount = 0;
        ReplyCount = 0;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CommentCreatedEvent(Id, UserId, VideoId, level));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建根评论
    /// </summary>
    public static Comment CreateRoot(Guid userId, Guid videoId, string content)
    {
        return new Comment(GuidGenerator.Create(), userId, videoId, null, null, content, 0);
    }
    
    /// <summary>
    /// 创建回复评论
    /// </summary>
    public static Comment CreateReply(
        Guid userId,
        Guid videoId,
        Guid parentId,
        Guid rootId,
        string content,
        int level)
    {
        if (level > 3)
            throw new BusinessException("评论层级不能超过3层");
        
        return new Comment(GuidGenerator.Create(), userId, videoId, parentId, rootId, content, level);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 审核通过
    /// </summary>
    public void Approve(string auditorId)
    {
        if (_status != CommentStatus.Pending)
            throw new BusinessException("评论不在待审核状态");
        
        _status = CommentStatus.Published;
        AuditedAt = DateTime.UtcNow;
        PublishedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CommentApprovedEvent(Id, VideoId, UserId));
    }
    
    /// <summary>
    /// 审核拒绝
    /// </summary>
    public void Reject(string auditorId, string reason)
    {
        if (_status != CommentStatus.Pending)
            throw new BusinessException("评论不在待审核状态");
        
        _status = CommentStatus.Rejected;
        AuditedAt = DateTime.UtcNow;
        
        AddDomainEvent(new CommentRejectedEvent(Id, VideoId, UserId, reason));
    }
    
    /// <summary>
    /// 删除评论
    /// </summary>
    public void Delete(string reason)
    {
        _status = CommentStatus.Deleted;
        IsDeleted = true;
        
        AddDomainEvent(new CommentDeletedEvent(Id, VideoId, UserId, reason));
    }
    
    /// <summary>
    /// 增加点赞数
    /// </summary>
    public void IncrementLike()
    {
        LikeCount++;
    }
    
    /// <summary>
    /// 减少点赞数
    /// </summary>
    public void DecrementLike()
    {
        LikeCount = Math.Max(0, LikeCount - 1);
    }
    
    /// <summary>
    /// 增加回复数
    /// </summary>
    public void IncrementReply()
    {
        ReplyCount++;
    }
    
    /// <summary>
    /// 减少回复数
    /// </summary>
    public void DecrementReply()
    {
        ReplyCount = Math.Max(0, ReplyCount - 1);
    }
    
    // ========== 查询方法 ==========
    
    public CommentContent GetContent() => _content;
    public CommentStatus GetStatus() => _status;
    
    public string GetText() => _content.Text;
    
    public bool IsPublished => _status == CommentStatus.Published;
    public bool IsPending => _status == CommentStatus.Pending;
    public bool IsRootComment => ParentId == null;
    public bool IsReply => ParentId != null;
    
    /// <summary>
    /// 是否热门评论（点赞>100）
    /// </summary>
    public bool IsHotComment => LikeCount > 100;
}
```

---

### 3.5 Share（分享聚合根）

```csharp
/// <summary>
/// 分享聚合根
/// </summary>
public class Share : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 分享用户ID
    public Guid VideoId { get; private set; }    // 视频ID
    
    // ========== 属性 ==========
    public SharePlatform Platform { get; private set; } // 分享平台
    public string? ShareUrl { get; private set; }       // 分享链接
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private Share() { }
    
    private Share(
        Guid id,
        Guid userId,
        Guid videoId,
        SharePlatform platform,
        string? shareUrl) : base(id)
    {
        UserId = userId;
        VideoId = videoId;
        Platform = platform;
        ShareUrl = shareUrl;
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ShareCreatedEvent(Id, UserId, VideoId, platform));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建分享记录
    /// </summary>
    public static Share Create(Guid userId, Guid videoId, SharePlatform platform, string? shareUrl = null)
    {
        return new Share(GuidGenerator.Create(), userId, videoId, platform, shareUrl);
    }
    
    /// <summary>
    /// 内部分享（站内分享）
    /// </summary>
    public static Share CreateInternal(Guid userId, Guid videoId)
    {
        return new Share(GuidGenerator.Create(), userId, videoId, SharePlatform.Internal, null);
    }
}

/// <summary>
/// 分享平台枚举
/// </summary>
public enum SharePlatform
{
    Internal = 0,   // 站内分享
    WeChat = 1,     // 微信
    QQ = 2,         // QQ
    Weibo = 3,      // 微博
    Twitter = 4,    // Twitter
    CopyLink = 5    // 复制链接
}
```

---

### 3.6 WatchHistory（观看历史聚合根）

```csharp
/// <summary>
/// 观看历史聚合根
/// </summary>
public class WatchHistory : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 用户ID
    public Guid VideoId { get; private set; }    // 视频ID
    
    // ========== 属性 ==========
    public double WatchProgress { get; private set; } // 观看进度（百分比）
    public double WatchDuration { get; private set; } // 观看时长（秒）
    public int WatchCount { get; private set; }       // 观看次数
    
    // ========== 时间戳 ==========
    public DateTime FirstWatchAt { get; private set; } // 首次观看时间
    public DateTime LastWatchAt { get; private set; }  // 最后观看时间
    
    // ========== 构造函数 ==========
    private WatchHistory() { }
    
    private WatchHistory(Guid userId, Guid videoId) : base(GuidGenerator.Create())
    {
        UserId = userId;
        VideoId = videoId;
        WatchProgress = 0;
        WatchDuration = 0;
        WatchCount = 0;
        FirstWatchAt = DateTime.UtcNow;
        LastWatchAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建观看历史
    /// </summary>
    public static WatchHistory Create(Guid userId, Guid videoId)
    {
        return new WatchHistory(userId, videoId);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 更新观看进度
    /// </summary>
    public void UpdateProgress(double progress, double duration)
    {
        WatchProgress = Math.Clamp(progress, 0, 1);
        WatchDuration = duration;
        WatchCount++;
        LastWatchAt = DateTime.UtcNow;
        
        AddDomainEvent(new WatchProgressUpdatedEvent(Id, UserId, VideoId, progress));
    }
    
    /// <summary>
    /// 标记为已看完
    /// </summary>
    public void MarkAsCompleted()
    {
        WatchProgress = 1.0;
    }
    
    /// <summary>
    /// 是否已看完
    /// </summary>
    public bool IsCompleted => WatchProgress >= 0.9;
    
    /// <summary>
    /// 是否最近观看（24小时内）
    /// </summary>
    public bool IsRecent => DateTime.UtcNow - LastWatchAt < TimeSpan.FromHours(24);
    
    /// <summary>
    /// 清除历史
    /// </summary>
    public void Clear()
    {
        IsDeleted = true;
    }
}
```

---

## 4. 值对象设计

### 4.1 LikeTarget（点赞目标值对象）

```csharp
/// <summary>
/// 点赞目标值对象
/// </summary>
public class LikeTarget : ValueObject
{
    public Guid Id { get; }
    public LikeTargetType Type { get; }
    
    private LikeTarget(Guid id, LikeTargetType type)
    {
        Id = id;
        Type = type;
    }
    
    public static LikeTarget Create(Guid id, LikeTargetType type)
    {
        return new LikeTarget(id, type);
    }
    
    public bool IsVideo => Type == LikeTargetType.Video;
    public bool IsComment => Type == LikeTargetType.Comment;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Id;
        yield return Type;
    }
}
```

---

### 4.2 LikeStatus（点赞状态值对象）

```csharp
/// <summary>
/// 点赞状态值对象
/// </summary>
public class LikeStatus : ValueObject
{
    public static readonly LikeStatus Active = new("active", "已点赞");
    public static readonly LikeStatus Cancelled = new("cancelled", "已取消");
    
    public string Code { get; }
    public string DisplayName { get; }
    
    private LikeStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
}
```

---

### 4.3 CoinAmount（投币金额值对象）

```csharp
/// <summary>
/// 投币金额值对象
/// </summary>
public class CoinAmount : ValueObject
{
    public int Count { get; }                   // 投币数量
    public decimal TotalValue { get; }          // 总价值（1币=1B币）
    
    private CoinAmount(int count)
    {
        Count = count;
        TotalValue = count; // 1币=1B币
    }
    
    public static CoinAmount Create(int count)
    {
        if (count < 1 || count > 2)
            throw new BusinessException("投币数量必须在1-2之间");
        
        return new CoinAmount(count);
    }
    
    public bool IsLargeAmount => Count > 1;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Count;
    }
}
```

---

### 4.4 CoinTransfer（投币转账值对象）

```csharp
/// <summary>
/// 投币转账值对象
/// 业务规则：投币100%给UP主（用户需求）
/// </summary>
public class CoinTransfer : ValueObject
{
    public Guid FromUserId { get; }             // 发送者ID
    public Guid ToUserId { get; }               // 接收者ID
    public decimal UpOwnerAmount { get; }       // UP主获得金额
    public decimal PlatformAmount { get; }      // 平台获得金额
    
    private CoinTransfer(Guid fromUserId, Guid toUserId, decimal upOwnerAmount, decimal platformAmount)
    {
        FromUserId = fromUserId;
        ToUserId = toUserId;
        UpOwnerAmount = upOwnerAmount;
        PlatformAmount = platformAmount;
    }
    
    /// <summary>
    /// 创建B站投币转账（100%给UP主）
    /// </summary>
    public static CoinTransfer CreateForBilibili(Guid fromUserId, Guid toUserId, int coinCount)
    {
        var totalAmount = coinCount;
        var upOwnerAmount = totalAmount; // 100%给UP主
        var platformAmount = 0;          // 平台0%
        
        return new CoinTransfer(fromUserId, toUserId, upOwnerAmount, platformAmount);
    }
    
    public bool IsFullToUpOwner => PlatformAmount == 0;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FromUserId;
        yield return ToUserId;
        yield return UpOwnerAmount;
    }
}
```

---

### 4.5 CommentContent（评论内容值对象）

```csharp
/// <summary>
/// 评论内容值对象
/// </summary>
public class CommentContent : ValueObject
{
    public string Text { get; }
    public int Length { get; }
    
    private CommentContent(string text)
    {
        Text = text;
        Length = text.Length;
    }
    
    public static CommentContent Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new BusinessException("评论内容不能为空");
        
        if (text.Length > 1000)
            throw new BusinessException("评论内容不能超过1000字符");
        
        return new CommentContent(text.Trim());
    }
    
    public bool IsLongComment => Length > 200;
    public bool IsShortComment => Length < 50;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Text;
    }
}
```

---

### 4.6 CommentStatus（评论状态值对象）

```csharp
/// <summary>
/// 评论状态值对象
/// </summary>
public class CommentStatus : ValueObject
{
    public static readonly CommentStatus Pending = new("pending", "待审核");
    public static readonly CommentStatus Published = new("published", "已发布");
    public static readonly CommentStatus Rejected = new("rejected", "审核拒绝");
    public static readonly CommentStatus Deleted = new("deleted", "已删除");
    public static readonly CommentStatus Hidden = new("hidden", "已隐藏");
    
    public string Code { get; }
    public string DisplayName { get; }
    
    private CommentStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }
    
    public bool CanDisplay => this == Published;
    public bool NeedAudit => this == Pending;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
}
```

---

## 5. 领域服务设计

### 5.1 IInteractionService（互动领域服务）

```csharp
/// <summary>
/// 互动领域服务接口
/// </summary>
public interface IInteractionService
{
    /// <summary>
    /// 点赞
    /// </summary>
    Task<Like> LikeAsync(Guid userId, Guid targetId, LikeTargetType targetType);
    
    /// <summary>
    /// 取消点赞
    /// </summary>
    Task CancelLikeAsync(Guid userId, Guid targetId);
    
    /// <summary>
    /// 收藏
    /// </summary>
    Task<Favorite> FavoriteAsync(Guid userId, Guid videoId, Guid? folderId = null, string? note = null);
    
    /// <summary>
    /// 取消收藏
    /// </summary>
    Task CancelFavoriteAsync(Guid userId, Guid videoId);
    
    /// <summary>
    /// 投币
    /// </summary>
    Task<Coin> CoinAsync(Guid userId, Guid videoId, int coinCount, string? message = null);
    
    /// <summary>
    /// 分享
    /// </summary>
    Task<Share> ShareAsync(Guid userId, Guid videoId, SharePlatform platform);
    
    /// <summary>
    /// 记录观看历史
    /// </summary>
    Task<WatchHistory> RecordWatchHistoryAsync(Guid userId, Guid videoId, double progress, double duration);
    
    /// <summary>
    /// 检查用户是否已点赞
    /// </summary>
    Task<bool> HasLikedAsync(Guid userId, Guid targetId, LikeTargetType targetType);
    
    /// <summary>
    /// 检查用户是否已收藏
    /// </summary>
    Task<bool> HasFavoritedAsync(Guid userId, Guid videoId);
}
```

---

### 5.2 ICommentService（评论领域服务）

```csharp
/// <summary>
/// 评论领域服务接口
/// 业务规则：评论先审后发（人工审核）（用户需求）
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// 发送评论（先审后发）
    /// </summary>
    Task<Comment> SendCommentAsync(Guid userId, Guid videoId, string content, Guid? parentId = null);
    
    /// <summary>
    /// 审核评论
    /// </summary>
    Task AuditCommentAsync(Guid commentId, bool approved, string? reason = null);
    
    /// <summary>
    /// 删除评论
    /// </summary>
    Task DeleteCommentAsync(Guid commentId, string reason);
    
    /// <summary>
    /// 获取视频评论列表
    /// </summary>
    Task<IReadOnlyList<Comment>> GetVideoCommentsAsync(Guid videoId, int skip, int take);
    
    /// <summary>
    /// 获取评论回复列表
    /// </summary>
    Task<IReadOnlyList<Comment>> GetCommentRepliesAsync(Guid commentId, int skip, int take);
    
    /// <summary>
    /// 获取待审核评论列表
    /// </summary>
    Task<IReadOnlyList<Comment>> GetPendingCommentsAsync(int skip, int take);
    
    /// <summary>
    /// 点赞评论
    /// </summary>
    Task LikeCommentAsync(Guid userId, Guid commentId);
}
```

---

### 5.3 IFavoriteFolderService（收藏夹领域服务）

```csharp
/// <summary>
/// 收藏夹领域服务接口
/// </summary>
public interface IFavoriteFolderService
{
    /// <summary>
    /// 创建收藏夹
    /// </summary>
    Task<FavoriteFolder> CreateFolderAsync(Guid userId, string name, string? description = null);
    
    /// <summary>
    /// 删除收藏夹
    /// </summary>
    Task DeleteFolderAsync(Guid folderId);
    
    /// <summary>
    /// 获取用户收藏夹列表
    /// </summary>
    Task<IReadOnlyList<FavoriteFolder>> GetUserFoldersAsync(Guid userId);
    
    /// <summary>
    /// 获取收藏夹内容
    /// </summary>
    Task<IReadOnlyList<Favorite>> GetFolderContentsAsync(Guid folderId, int skip, int take);
}

/// <summary>
/// 收藏夹聚合根
/// </summary>
public class FavoriteFolder : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int VideoCount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private FavoriteFolder() { }
    
    private FavoriteFolder(Guid userId, string name, string? description) : base(GuidGenerator.Create())
    {
        UserId = userId;
        Name = name;
        Description = description;
        VideoCount = 0;
        CreatedAt = DateTime.UtcNow;
    }
    
    public static FavoriteFolder Create(Guid userId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("收藏夹名称不能为空");
        
        if (name.Length > 50)
            throw new BusinessException("收藏夹名称不能超过50字符");
        
        return new FavoriteFolder(userId, name, description);
    }
    
    public void UpdateInfo(string name, string? description)
    {
        Name = name;
        Description = description;
    }
    
    public void IncrementVideoCount()
    {
        VideoCount++;
    }
    
    public void DecrementVideoCount()
    {
        VideoCount = Math.Max(0, VideoCount - 1);
    }
}
```

---

## 6. 仓储接口设计

### 6.1 ILikeRepository

```csharp
/// <summary>
/// 点赞仓储接口
/// </summary>
public interface ILikeRepository : IRepository<Like, Guid>
{
    Task<Like?> GetByUserAndTargetAsync(Guid userId, Guid targetId, LikeTargetType targetType);
    Task<List<Like>> GetByUserAsync(Guid userId, int skip, int take);
    Task<long> CountByTargetAsync(Guid targetId, LikeTargetType targetType);
}
```

---

### 6.2 IFavoriteRepository

```csharp
/// <summary>
/// 收藏仓储接口
/// </summary>
public interface IFavoriteRepository : IRepository<Favorite, Guid>
{
    Task<Favorite?> GetByUserAndVideoAsync(Guid userId, Guid videoId);
    Task<List<Favorite>> GetByUserAsync(Guid userId, int skip, int take);
    Task<List<Favorite>> GetByFolderAsync(Guid folderId, int skip, int take);
    Task<long> CountByVideoAsync(Guid videoId);
}
```

---

### 6.3 ICoinRepository

```csharp
/// <summary>
/// 投币仓储接口
/// </summary>
public interface ICoinRepository : IRepository<Coin, Guid>
{
    Task<List<Coin>> GetByVideoAsync(Guid videoId, int skip, int take);
    Task<List<Coin>> GetByUserAsync(Guid userId, int skip, int take);
    Task<long> CountByVideoAsync(Guid videoId);
    Task<int> GetUserCoinCountForVideoAsync(Guid userId, Guid videoId);
}
```

---

### 6.4 ICommentRepository

```csharp
/// <summary>
/// 评论仓储接口
/// </summary>
public interface ICommentRepository : IRepository<Comment, Guid>
{
    Task<List<Comment>> GetByVideoAsync(Guid videoId, int skip, int take);
    Task<List<Comment>> GetRepliesAsync(Guid commentId, int skip, int take);
    Task<List<Comment>> GetPendingAsync(int skip, int take);
    Task<long> CountByVideoAsync(Guid videoId);
    Task<long> CountRepliesAsync(Guid commentId);
}
```

---

### 6.5 IShareRepository

```csharp
/// <summary>
/// 分享仓储接口
/// </summary>
public interface IShareRepository : IRepository<Share, Guid>
{
    Task<List<Share>> GetByVideoAsync(Guid videoId, int skip, int take);
    Task<long> CountByVideoAsync(Guid videoId);
}
```

---

### 6.6 IWatchHistoryRepository

```csharp
/// <summary>
/// 观看历史仓储接口
/// </summary>
public interface IWatchHistoryRepository : IRepository<WatchHistory, Guid>
{
    Task<WatchHistory?> GetByUserAndVideoAsync(Guid userId, Guid videoId);
    Task<List<WatchHistory>> GetByUserAsync(Guid userId, int skip, int take);
    Task ClearByUserAsync(Guid userId);
}
```

---

## 7. 领域事件设计

```csharp
public class LikeCreatedEvent { public Guid Id, UserId, TargetId; public LikeTargetType Type; }
public class LikeCancelledEvent { public Guid Id, UserId, TargetId; public LikeTargetType Type; }
public class LikeRestoredEvent { public Guid Id, UserId, TargetId; public LikeTargetType Type; }
public class FavoriteCreatedEvent { public Guid Id, UserId, VideoId; }
public class FavoriteRemovedEvent { public Guid Id, UserId, VideoId; }
public class CoinSentEvent { public Guid Id, UserId, VideoId, UpOwnerId; public int CoinCount; }
public class CommentCreatedEvent { public Guid Id, UserId, VideoId; public int Level; }
public class CommentApprovedEvent { public Guid Id, VideoId, UserId; }
public class CommentRejectedEvent { public Guid Id, VideoId, UserId; public string Reason; }
public class CommentDeletedEvent { public Guid Id, VideoId, UserId; public string Reason; }
public class ShareCreatedEvent { public Guid Id, UserId, VideoId; public SharePlatform Platform; }
public class WatchProgressUpdatedEvent { public Guid Id, UserId, VideoId; public double Progress; }
```

---

## 8. 业务规则总结

### 8.1 点赞规则

| 规则 | 说明 |
|------|------|
| 点赞目标 | 视频、评论 |
| 取消点赞 | 支持 |
| 点赞统计 | 实时更新 |
| 重复点赞 | 不允许（已点赞不能再点赞） |

---

### 8.2 收藏规则

| 规则 | 说明 |
|------|------|
| 收藏夹 | 支持多收藏夹 |
| 收藏备注 | 可添加备注 |
| 取消收藏 | 支持 |
| 默认收藏夹 | 系统默认收藏夹 |

---

### 8.3 投币规则

| 规则 | 说明 |
|------|------|
| 投币上限 | 无限制（用户需求） |
| 单次投币 | 最多2个 |
| 投币分成 | 100%给UP主（用户需求） |
| 投币留言 | 可选 |

---

### 8.4 评论规则

| 规则 | 说明 |
|------|------|
| 评论审核 | 先审后发（人工审核）（用户需求） |
| 评论层级 | 最多3层 |
| 评论长度 | 最大1000字符 |
| 评论回复 | 支持回复评论 |
| 评论删除 | 支持删除 |

---

### 8.5 分享规则

| 规则 | 说明 |
|------|------|
| 分享平台 | 微信、QQ、微博、Twitter、复制链接 |
| 站内分享 | 支持 |
| 分享统计 | 记录分享次数 |

---

### 8.6 观看历史规则

| 规则 | 说明 |
|------|------|
| 历史记录 | 自动记录观看进度 |
| 进度保存 | 实时保存 |
| 历史清除 | 用户可清除 |
| 完成标记 | 观看进度≥90%标记完成 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 互动模块领域设计文档完成