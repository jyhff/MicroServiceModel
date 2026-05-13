# 直播模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | LiveService（直播服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 题领域模型概览

### 2.1 题领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    直播领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │ LiveStream   │◄───── LiveStatus (值对象)                │
│  │ (聚合根)     │◄───── LiveStreamKey (值对象)             │
│  └──────────────┘◄───── LiveStatistics (值对象)            │
│                                                             │
│  ┌──────────────┐                                          │
│  │ LiveRoom     │                                          │
│  │ (聚合根)     │                                          │
│  │ (直播间)     │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ LiveGift     │◄───── GiftAmount (值对象)                │
│  │ (聚合根)     │◄───── GiftShare (值对象)                 │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ LiveViewer   │                                          │
│  │ (实体)       │                                          │
│  └──────────────┘                                          │
│                                                             │
│  值对象：                                                    │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │LiveStatus    │ │LiveStreamKey │ │LiveStatistics│       │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐                        │
│  │GiftAmount    │ │GiftShare     │                        │
│  └──────────────┘ └──────────────┘                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 LiveStream（直播流聚合根）

**聚合边界定义**：
- 聚合根：LiveStream
- 外部引用：UserId（主播ID）、CategoryId（分类ID）
- 值对象：LiveStatus、LiveStreamKey、LiveStatistics

**职责**：
- 管理直播流生命周期（开播→直播中→结束）
- 生成和管理推流密钥
- 维护直播统计数据（观众数、礼物收入）

```csharp
/// <summary>
/// 直播流聚合根
/// 业务规则：
/// 1. HLS延迟<10秒（用户需求）
/// 2. 礼物100%给UP主（用户需求）
/// 3. 所有用户可开播（用户需求）
/// </summary>
public class LiveStream : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 主播ID
    public Guid? CategoryId { get; private set; } // 分类ID
    
    // ========== 值对象 ==========
    private LiveStatus _status;                  // 直播状态
    private LiveStreamKey _streamKey;            // 推流密钥
    private LiveStatistics _statistics;          // 统计数据
    
    // ========== 属性 ==========
    public string Title { get; private set; }    // 直播标题
    public string? CoverUrl { get; private set; } // 封面图
    public string? Description { get; private set; } // 直播描述
    
    // ========== 流信息 ==========
    public string RtmpUrl { get; private set; }  // RTMP推流地址
    public string HlsUrl { get; private set; }   // HLS播放地址
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    
    // ========== 构造函数 ==========
    private LiveStream() { }
    
    private LiveStream(
        Guid id,
        Guid userId,
        string title,
        Guid? categoryId,
        string? coverUrl,
        string? description) : base(id)
    {
        UserId = userId;
        Title = title;
        CategoryId = categoryId;
        CoverUrl = coverUrl;
        Description = description;
        _status = LiveStatus.NotStarted;
        _streamKey = LiveStreamKey.Generate(id);
        _statistics = LiveStatistics.Empty();
        
        RtmpUrl = $"rtmp://live.bilibili.com/app/{_streamKey.Key}";
        HlsUrl = $"http://live.bilibili.com/hls/{id}.m3u8";
        
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new LiveStreamCreatedEvent(Id, UserId, Title));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建直播流（所有用户可开播）
    /// </summary>
    public static LiveStream Create(
        Guid userId,
        string title,
        Guid? categoryId = null,
        string? coverUrl = null,
        string? description = null)
    {
        // 业务规则：标题长度限制
        if (title.Length > 200)
            throw new BusinessException("直播标题不能超过200字符");
        
        return new LiveStream(
            GuidGenerator.Create(),
            userId,
            title,
            categoryId,
            coverUrl,
            description);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 开始直播（开播）
    /// </summary>
    public void Start()
    {
        // 业务规则：只有未开播的直播可以开始
        if (_status != LiveStatus.NotStarted && _status != LiveStatus.Preparing)
            throw new BusinessException("直播已经开始或已结束");
        
        _status = LiveStatus.Live;
        StartTime = DateTime.UtcNow;
        
        AddDomainEvent(new LiveStartedEvent(Id, UserId, Title));
    }
    
    /// <summary>
    /// 结束直播
    /// </summary>
    public void End()
    {
        // 业务规则：只有直播中的直播可以结束
        if (_status != LiveStatus.Live)
            throw new BusinessException("直播未开始或已结束");
        
        _status = LiveStatus.Ended;
        EndTime = DateTime.UtcNow;
        
        AddDomainEvent(new LiveEndedEvent(Id, UserId, _statistics.TotalViewerCount, _statistics.TotalGiftAmount));
    }
    
    /// <summary>
    /// 更新直播信息
    /// </summary>
    public void UpdateInfo(string title, Guid? categoryId, string? coverUrl, string? description)
    {
        Title = title;
        CategoryId = categoryId;
        CoverUrl = coverUrl;
        Description = description;
    }
    
    /// <summary>
    /// 更新推流密钥（更换密钥）
    /// </summary>
    public void RefreshStreamKey()
    {
        _streamKey = LiveStreamKey.Generate(Id);
        RtmpUrl = $"rtmp://live.bilibili.com/app/{_streamKey.Key}";
    }
    
    /// <summary>
    /// 观众进入
    /// </summary>
    public void ViewerJoin()
    {
        _statistics = _statistics.AddViewer();
    }
    
    /// <summary>
    /// 观众离开
    /// </summary>
    public void ViewerLeave()
    {
        _statistics = _statistics.RemoveViewer();
    }
    
    /// <summary>
    /// 收到礼物
    /// </summary>
    public void ReceiveGift(decimal amount)
    {
        _statistics = _statistics.AddGiftAmount(amount);
    }
    
    /// <summary>
    /// 更新实时观众数（从Redis同步）
    /// </summary>
    public void UpdateViewerCount(int currentCount)
    {
        _statistics = _statistics.UpdateCurrentViewerCount(currentCount);
    }
    
    /// <summary>
    /// 标记准备状态
    /// </summary>
    public void MarkPreparing()
    {
        _status = LiveStatus.Preparing;
    }
    
    // ========== 查询方法 ==========
    
    public LiveStatus GetStatus() => _status;
    public LiveStreamKey GetStreamKey() => _streamKey;
    public LiveStatistics GetStatistics() => _statistics;
    
    public string GetStreamKeyString() => _streamKey.Key;
    public int GetCurrentViewerCount() => _statistics.CurrentViewerCount;
    public int GetTotalViewerCount() => _statistics.TotalViewerCount;
    public decimal GetTotalGiftAmount() => _statistics.TotalGiftAmount;
    
    /// <summary>
    /// 是否直播中
    /// </summary>
    public bool IsLive => _status == LiveStatus.Live;
    
    /// <summary>
    /// 是否已结束
    /// </summary>
    public bool IsEnded => _status == LiveStatus.Ended;
    
    /// <summary>
    /// 是否可开播
    /// </summary>
    public bool CanStart => _status == LiveStatus.NotStarted || _status == LiveStatus.Preparing;
    
    /// <summary>
    /// 获取直播时长
    /// </summary>
    public TimeSpan? GetDuration()
    {
        if (StartTime == null) return null;
        var endTime = EndTime ?? DateTime.UtcNow;
        return endTime - StartTime;
    }
    
    /// <summary>
    /// 格式化直播时长显示
    /// </summary>
    public string FormatDuration()
    {
        var duration = GetDuration();
        if (duration == null) return "00:00";
        
        return $"{(int)duration.Value.TotalHours:D2}:{duration.Value.Minutes:D2}:{duration.Value.Seconds:D2}";
    }
}
```

---

## 4. 值对象设计

### 4.1 LiveStatus（直播状态值对象）

```csharp
/// <summary>
/// 直播状态值对象
/// </summary>
public class LiveStatus : ValueObject
{
    // ========== 状态常量 ==========
    public static readonly LiveStatus NotStarted = new("not_started", "未开播");
    public static readonly LiveStatus Preparing = new("preparing", "准备中");
    public static readonly LiveStatus Live = new("live", "直播中");
    public static readonly LiveStatus Paused = new("paused", "暂停");
    public static readonly LiveStatus Ended = new("ended", "已结束");
    public static readonly LiveStatus Banned = new("banned", "已封禁");
    
    // ========== 属性 ==========
    public string Code { get; }
    public string DisplayName { get; }
    
    // ========== 构造函数 ==========
    private LiveStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否可以开播
    /// </summary>
    public bool CanStart()
    {
        return this == NotStarted || this == Preparing;
    }
    
    /// <summary>
    /// 是否直播中
    /// </summary>
    public bool IsLive()
    {
        return this == Live || this == Paused;
    }
    
    /// <summary>
    /// 是否已结束
    /// </summary>
    public bool IsEnded()
    {
        return this == Ended || this == Banned;
    }
    
    /// <summary>
    /// 是否可以观看
    /// </summary>
    public bool CanWatch()
    {
        return this == Live;
    }
    
    /// <summary>
    /// 是否可以编辑
    /// </summary>
    public bool CanEdit()
    {
        return this == NotStarted || this == Preparing || this == Live;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
    
    // ========== 所有状态列表 ==========
    public static readonly IReadOnlyList<LiveStatus> AllStatuses = new List<LiveStatus>
    {
        NotStarted, Preparing, Live, Paused, Ended, Banned
    };
}
```

---

### 4.2 LiveStreamKey（推流密钥值对象）

```csharp
/// <summary>
/// 推流密钥值对象
/// 业务规则：密钥安全生成，定期更换
/// </summary>
public class LiveStreamKey : ValueObject
{
    // ========== 属性 ==========
    public string Key { get; }                  // 推流密钥
    public DateTime GeneratedAt { get; }        // 生成时间
    public DateTime? ExpiresAt { get; }         // 过期时间
    public bool IsActive { get; }               // 是否激活
    
    // ========== 构造函数 ==========
    private LiveStreamKey(string key, DateTime generatedAt, DateTime? expiresAt, bool isActive)
    {
        Key = key;
        GeneratedAt = generatedAt;
        ExpiresAt = expiresAt;
        IsActive = isActive;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 生成推流密钥
    /// </summary>
    public static LiveStreamKey Generate(Guid liveId)
    {
        // 安全密钥生成：liveId + 随机字符串 + 时间戳
        var randomPart = Guid.NewGuid().ToString("N").Substring(0, 16);
        var timestamp = DateTime.UtcNow.Ticks;
        var key = $"{liveId:N}_{randomPart}_{timestamp}";
        
        // 密钥24小时过期
        var expiresAt = DateTime.UtcNow.AddHours(24);
        
        return new LiveStreamKey(key, DateTime.UtcNow, expiresAt, true);
    }
    
    /// <summary>
    /// 从数据库重建
    /// </summary>
    public static LiveStreamKey FromData(string key, DateTime generatedAt, DateTime? expiresAt, bool isActive)
    {
        return new LiveStreamKey(key, generatedAt, expiresAt, isActive);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
    
    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid()
    {
        return IsActive && !IsExpired();
    }
    
    /// <summary>
    /// 验证密钥
    /// </summary>
    public bool Validate(string inputKey)
    {
        return IsValid() && Key == inputKey;
    }
    
    /// <summary>
    /// 失效密钥
    /// </summary>
    public LiveStreamKey Deactivate()
    {
        return new LiveStreamKey(Key, GeneratedAt, ExpiresAt, false);
    }
    
    /// <summary>
    /// 延长过期时间
    /// </summary>
    public LiveStreamKey ExtendExpiration(int hours)
    {
        var newExpiresAt = DateTime.UtcNow.AddHours(hours);
        return new LiveStreamKey(Key, GeneratedAt, newExpiresAt, IsActive);
    }
    
    /// <summary>
    /// 获取RTMP推流URL
    /// </summary>
    public string GetRtmpUrl(string server = "rtmp://live.bilibili.com")
    {
        return $"{server}/app/{Key}";
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Key;
    }
}
```

---

### 4.3 LiveStatistics（直播统计数据值对象）

```csharp
/// <summary>
/// 直播统计数据值对象
/// </summary>
public class LiveStatistics : ValueObject
{
    // ========== 属性 ==========
    public int CurrentViewerCount { get; }      // 当前观众数
    public int TotalViewerCount { get; }        // 总观众数（累计）
    public int PeakViewerCount { get; }         // 最高观众数
    public decimal TotalGiftAmount { get; }     // 礼物总收入（B币）
    public int TotalGiftCount { get; }          // 礼物总数
    public int TotalDanmakuCount { get; }       // 弹幕总数
    
    // ========== 构造函数 ==========
    private LiveStatistics(
        int currentViewerCount,
        int totalViewerCount,
        int peakViewerCount,
        decimal totalGiftAmount,
        int totalGiftCount,
        int totalDanmakuCount)
    {
        CurrentViewerCount = currentViewerCount;
        TotalViewerCount = totalViewerCount;
        PeakViewerCount = peakViewerCount;
        TotalGiftAmount = totalGiftAmount;
        TotalGiftCount = totalGiftCount;
        TotalDanmakuCount = totalDanmakuCount;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建空统计（新直播）
    /// </summary>
    public static LiveStatistics Empty()
    {
        return new LiveStatistics(0, 0, 0, 0, 0, 0);
    }
    
    /// <summary>
    /// 从数据库重建
    /// </summary>
    public static LiveStatistics FromData(
        int currentViewerCount,
        int totalViewerCount,
        int peakViewerCount,
        decimal totalGiftAmount,
        int totalGiftCount,
        int totalDanmakuCount)
    {
        return new LiveStatistics(
            currentViewerCount,
            totalViewerCount,
            peakViewerCount,
            totalGiftAmount,
            totalGiftCount,
            totalDanmakuCount);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 观众进入
    /// </summary>
    public LiveStatistics AddViewer()
    {
        var newCurrent = CurrentViewerCount + 1;
        var newTotal = TotalViewerCount + 1;
        var newPeak = Math.Max(PeakViewerCount, newCurrent);
        
        return new LiveStatistics(
            newCurrent,
            newTotal,
            newPeak,
            TotalGiftAmount,
            TotalGiftCount,
            TotalDanmakuCount);
    }
    
    /// <summary>
    /// 观众离开
    /// </summary>
    public LiveStatistics RemoveViewer()
    {
        var newCurrent = Math.Max(0, CurrentViewerCount - 1);
        
        return new LiveStatistics(
            newCurrent,
            TotalViewerCount,
            PeakViewerCount,
            TotalGiftAmount,
            TotalGiftCount,
            TotalDanmakuCount);
    }
    
    /// <summary>
    /// 更新当前观众数（从Redis同步）
    /// </summary>
    public LiveStatistics UpdateCurrentViewerCount(int count)
    {
        var newPeak = Math.Max(PeakViewerCount, count);
        
        return new LiveStatistics(
            count,
            TotalViewerCount,
            newPeak,
            TotalGiftAmount,
            TotalGiftCount,
            TotalDanmakuCount);
    }
    
    /// <summary>
    /// 收到礼物
    /// </summary>
    public LiveStatistics AddGiftAmount(decimal amount)
    {
        return new LiveStatistics(
            CurrentViewerCount,
            TotalViewerCount,
            PeakViewerCount,
            TotalGiftAmount + amount,
            TotalGiftCount + 1,
            TotalDanmakuCount);
    }
    
    /// <summary>
    /// 发送弹幕
    /// </summary>
    public LiveStatistics AddDanmaku()
    {
        return new LiveStatistics(
            CurrentViewerCount,
            TotalViewerCount,
            PeakViewerCount,
            TotalGiftAmount,
            TotalGiftCount,
            TotalDanmakuCount + 1);
    }
    
    /// <summary>
    /// 是否热门直播（观众数高）
    /// </summary>
    public bool IsHotLive()
    {
        return CurrentViewerCount > 1000;
    }
    
    /// <summary>
    /// 计算热度值
    /// </summary>
    public double CalculateHotScore()
    {
        // 热度公式：观众数权重 + 礼物权重 + 弹幕权重
        var viewerScore = Math.Log10(CurrentViewerCount + 1) * 0.5;
        var giftScore = Math.Log10((double)(TotalGiftAmount + 1)) * 0.3;
        var danmakuScore = Math.Log10(TotalDanmakuCount + 1) * 0.2;
        
        return viewerScore + giftScore + danmakuScore;
    }
    
    /// <summary>
    /// 格式化观众数显示
    /// </summary>
    public string FormatViewerCount()
    {
        if (CurrentViewerCount < 1000)
            return CurrentViewerCount.ToString();
        else if (CurrentViewerCount < 10000)
            return $"{CurrentViewerCount / 1000}K";
        else
            return $"{CurrentViewerCount / 10000}W";
    }
    
    /// <summary>
    /// 格式化礼物金额显示
    /// </summary>
    public string FormatGiftAmount()
    {
        return $"{TotalGiftAmount:F0}B币";
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CurrentViewerCount;
        yield return TotalViewerCount;
        yield return PeakViewerCount;
        yield return TotalGiftAmount;
    }
}
```

---

### 4.4 GiftAmount（礼物金额值对象）

```csharp
/// <summary>
/// 礼物金额值对象
/// </summary>
public class GiftAmount : ValueObject
{
    // ========== 属性 ==========
    public decimal TotalAmount { get; }         // 总金额
    public int GiftCount { get; }               // 礼物数量
    public decimal SinglePrice { get; }         // 单个礼物价格
    
    // ========== 构造函数 ==========
    private GiftAmount(decimal totalAmount, int giftCount, decimal singlePrice)
    {
        TotalAmount = totalAmount;
        GiftCount = giftCount;
        SinglePrice = singlePrice;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建礼物金额
    /// </summary>
    public static GiftAmount Create(decimal singlePrice, int count)
    {
        var totalAmount = singlePrice * count;
        return new GiftAmount(totalAmount, count, singlePrice);
    }
    
    /// <summary>
    /// 从数据库重建
    /// </summary>
    public static GiftAmount FromData(decimal totalAmount, int giftCount, decimal singlePrice)
    {
        return new GiftAmount(totalAmount, giftCount, singlePrice);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否大额礼物
    /// </summary>
    public bool IsLargeGift()
    {
        return TotalAmount > 100;
    }
    
    /// <summary>
    /// 格式化金额显示
    /// </summary>
    public string FormatAmount()
    {
        return $"{TotalAmount:F0}B币";
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return TotalAmount;
        yield return GiftCount;
    }
}
```

---

### 4.5 GiftShare（礼物分成值对象）

```csharp
/// <summary>
/// 礼物分成值对象
/// 业务规则：礼物100%给UP主（用户需求）
/// </summary>
public class GiftShare : ValueObject
{
    // ========== 属性 ==========
    public decimal UpOwnerAmount { get; }       // UP主获得金额
    public decimal PlatformAmount { get; }      // 平台获得金额
    public decimal UpOwnerShareRate { get; }    // UP主分成比例
    
    // ========== 构造函数 ==========
    private GiftShare(decimal upOwnerAmount, decimal platformAmount, decimal upOwnerShareRate)
    {
        UpOwnerAmount = upOwnerAmount;
        PlatformAmount = platformAmount;
        UpOwnerShareRate = upOwnerShareRate;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建礼物分成（100%给UP主）
    /// </summary>
    public static GiftShare CreateForBilibili(decimal totalAmount)
    {
        // 业务规则：礼物100%给UP主（用户需求）
        var upOwnerAmount = totalAmount;
        var platformAmount = 0;
        var shareRate = 1.0; // 100%
        
        return new GiftShare(upOwnerAmount, platformAmount, shareRate);
    }
    
    /// <summary>
    /// 创建自定义分成
    /// </summary>
    public static GiftShare CreateCustom(decimal totalAmount, decimal shareRate)
    {
        var upOwnerAmount = totalAmount * shareRate;
        var platformAmount = totalAmount - upOwnerAmount;
        
        return new GiftShare(upOwnerAmount, platformAmount, shareRate);
    }
    
    // ========== 业务方法 =///
    
    /// <summary>
    /// 是否全额给UP主
    /// </summary>
    public bool IsFullToUpOwner()
    {
        return UpOwnerShareRate == 1.0;
    }
    
    /// <summary>
    /// 格式化分成显示
    /// </summary>
    public string FormatShare()
    {
        return $"UP主获得{UpOwnerAmount:F0}B币（{UpOwnerShareRate:P0})";
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return UpOwnerAmount;
        yield return PlatformAmount;
        yield return UpOwnerShareRate;
    }
}
```

---

## 5. 聚合根设计：LiveRoom（直播间聚合）

### 5.1 LiveRoom（直播间聚合根）

```csharp
/// <summary>
/// 直播间聚合根（观众管理）
/// </summary>
public class LiveRoom : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid LiveId { get; private set; }    // 直播ID
    
    // ========== 属性 ==========
    private readonly List<LiveViewer> _viewers = new(); // 观众列表
    public int ActiveViewerCount { get; private set; }  // 当前观众数
    public string RoomId { get; private set; }          // 房间ID
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    
    // ========== 导航属性 ==========
    public IReadOnlyCollection<LiveViewer> Viewers => _viewers.AsReadOnly();
    
    // ========== 构造函数 ==========
    private LiveRoom() { }
    
    private LiveRoom(Guid liveId) : base(GuidGenerator.Create())
    {
        LiveId = liveId;
        RoomId = $"room_{liveId:N}";
        ActiveViewerCount = 0;
        CreatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 =///
    
    public static LiveRoom Create(Guid liveId)
    {
        return new LiveRoom(liveId);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 观众加入
    /// </summary>
    public void ViewerJoin(Guid userId, string nickname)
    {
        var viewer = LiveViewer.Create(LiveId, userId, nickname);
        _viewers.Add(viewer);
        ActiveViewerCount++;
        
        AddDomainEvent(new LiveViewerJoinedEvent(LiveId, userId, nickname));
    }
    
    /// <summary>
    /// 观众离开
    /// </summary>
    public void ViewerLeave(Guid userId)
    {
        var viewer = _viewers.FirstOrDefault(v => v.UserId == userId && v.IsActive);
        if (viewer != null)
        {
            viewer.Leave();
            ActiveViewerCount = Math.Max(0, ActiveViewerCount - 1);
            
            AddDomainEvent(new LiveViewerLeftEvent(LiveId, userId));
        }
    }
    
    /// <summary>
    /// 观众发送弹幕
    /// </summary>
    public void ViewerSendDanmaku(Guid userId, string content)
    {
        var viewer = _viewers.FirstOrDefault(v => v.UserId == userId && v.IsActive);
        if (viewer != null)
        {
            viewer.AddDanmaku();
        }
    }
    
    /// <summary>
    /// 观众发送礼物
    /// </summary>
    public void ViewerSendGift(Guid userId, decimal amount)
    {
        var viewer = _viewers.FirstOrDefault(v => v.UserId == userId && v.IsActive);
        if (viewer != null)
        {
            viewer.AddGift(amount);
        }
    }
    
    /// <summary>
    /// 获取活跃观众列表
    /// </summary>
    public List<LiveViewer> GetActiveViewers()
    {
        return _viewers.Where(v => v.IsActive).ToList();
    }
    
    /// <summary>
    /// 清理过期观众（长时间未活跃）
    /// </summary>
    public int CleanupInactiveViewers(int minutes = 30)
    {
        var threshold = DateTime.UtcNow.AddMinutes(-minutes);
        var inactiveViewers = _viewers
            .Where(v => v.IsActive && v.LastActiveAt < threshold)
            .ToList();
        
        foreach (var viewer in inactiveViewers)
        {
            viewer.Leave();
            ActiveViewerCount = Math.Max(0, ActiveViewerCount - 1);
        }
        
        return inactiveViewers.Count;
    }
}
```

---

## 6. 聚合根设计：LiveGift（礼物记录聚合）

### 6.1 LiveGift（礼物记录聚合根）

```csharp
/// <summary>
/// 礼物记录聚合根
/// </summary>
public class LiveGift : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid LiveId { get; private set; }    // 直播ID
    public Guid UserId { get; private set; }    // 发送者ID
    public Guid GiftTemplateId { get; private set; } // 礼物模板ID
    
    // ========== 值对象 ==========
    private GiftAmount _amount;                 // 礼物金额
    private GiftShare _share;                   // 礼物分成
    
    // ========== 属性 ==========
    public int GiftCount { get; private set; }  // 礼物数量
    public string GiftName { get; private set; } // 礼物名称
    public string? Message { get; private set; } // 赠言
    public string? GiftIcon { get; private set; } // 礼物图标
    
    // ========== 时间戳 ==========
    public DateTime SentAt { get; private set; }
    
    // ========== 构造函数 ==========
    private LiveGift() { }
    
    private LiveGift(
        Guid id,
        Guid liveId,
        Guid userId,
        Guid giftTemplateId,
        string giftName,
        decimal singlePrice,
        int count,
        string? message,
        string? giftIcon) : base(id)
    {
        LiveId = liveId;
        UserId = userId;
        GiftTemplateId = giftTemplateId;
        GiftName = giftName;
        GiftCount = count;
        Message = message;
        GiftIcon = giftIcon;
        
        _amount = GiftAmount.Create(singlePrice, count);
        _share = GiftShare.CreateForBilibili(_amount.TotalAmount); // 100%给UP主
        
        SentAt = DateTime.UtcNow;
        
        AddDomainEvent(new LiveGiftSentEvent(LiveId, UserId, giftName, _amount.TotalAmount));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 发送礼物
    /// </summary>
    public static LiveGift Send(
        Guid liveId,
        Guid userId,
        Guid giftTemplateId,
        string giftName,
        decimal singlePrice,
        int count,
        string? message = null,
        string? giftIcon = null)
    {
        return new LiveGift(
            GuidGenerator.Create(),
            liveId,
            userId,
            giftTemplateId,
            giftName,
            singlePrice,
            count,
            message,
            giftIcon);
    }
    
    // ========== 查询方法 ==========
    
    public GiftAmount GetAmount() => _amount;
    public GiftShare GetShare() => _share;
    
    public decimal GetTotalAmount() => _amount.TotalAmount;
    public decimal GetUpOwnerAmount() => _share.UpOwnerAmount;
    
    /// <summary>
    /// 是否大额礼物
    /// </summary>
    public bool IsLargeGift => _amount.IsLargeGift();
    
    /// <summary>
    /// 格式化显示
    /// </summary>
    public string FormatDisplay()
    {
        return $"发送{GiftName}x{GiftCount}（{_amount.FormatAmount()}）";
    }
}
```

---

## 7. 实体设计

### 7.1 LiveViewer（直播观众实体）

```csharp
/// <summary>
/// 直播观众实体
/// </summary>
public class LiveViewer : Entity<Guid>
{
    // ========== 外部引用 ==========
    public Guid LiveId { get; private set; }    // 直播ID
    public Guid UserId { get; private set; }    // 用户ID
    
    // ========== 属性 ==========
    public string Nickname { get; private set; } // 用户昵称
    public bool IsActive { get; private set; }  // 是否活跃
    public decimal TotalGiftAmount { get; private set; } // 礼物总金额
    public int TotalDanmakuCount { get; private set; }    // 弹幕总数
    
    // ========== 时间戳 ==========
    public DateTime JoinedAt { get; private set; }   // 加入时间
    public DateTime? LeftAt { get; private set; }    // 离开时间
    public DateTime LastActiveAt { get; private set; } // 最后活跃时间
    
    // ========== 构造函数 ==========
    private LiveViewer() { }
    
    private LiveViewer(Guid liveId, Guid userId, string nickname) : base(GuidGenerator.Create())
    {
        LiveId = liveId;
        UserId = userId;
        Nickname = nickname;
        IsActive = true;
        TotalGiftAmount = 0;
        TotalDanmakuCount = 0;
        JoinedAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    public static LiveViewer Create(Guid liveId, Guid userId, string nickname)
    {
        return new LiveViewer(liveId, userId, nickname);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 离开直播间
    /// </summary>
    public void Leave()
    {
        IsActive = false;
        LeftAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 发送弹幕
    /// </summary>
    public void AddDanmaku()
    {
        TotalDanmakuCount++;
        LastActiveAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 发送礼物
    /// </summary>
    public void AddGift(decimal amount)
    {
        TotalGiftAmount += amount;
        LastActiveAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 更新活跃时间
    /// </summary>
    public void UpdateActiveTime()
    {
        LastActiveAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 获取观看时长
    /// </summary>
    public TimeSpan GetWatchDuration()
    {
        var endTime = LeftAt ?? DateTime.UtcNow;
        return endTime - JoinedAt;
    }
    
    /// <summary>
    /// 是否大额观众（礼物金额高）
    /// </summary>
    public bool IsLargeGiftViewer()
    {
        return TotalGiftAmount > 100;
    }
}
```

---

## 8. 题领域服务设计

### 8.1 ILiveStreamService（直播流领域服务）

```csharp
/// <summary>
/// 直播流领域服务接口
/// </summary>
public interface ILiveStreamService
{
    /// <summary>
    /// 开始直播
    /// </summary>
    Task<LiveStream> StartLiveAsync(Guid userId, string title, Guid? categoryId);
    
    /// <summary>
    /// 结束直播
    /// </summary>
    Task EndLiveAsync(Guid liveId);
    
    /// <summary>
    /// 获取推流地址
    /// </summary>
    Task<(string RtmpUrl, string StreamKey)> GetPushUrlAsync(Guid liveId);
    
    /// <summary>
    /// 获取播放地址
    /// </summary>
    Task<string> GetPlayUrlAsync(Guid liveId);
    
    /// <summary>
    /// 验证推流密钥
    /// </summary>
    Task<bool> ValidateStreamKeyAsync(Guid liveId, string streamKey);
    
    /// <summary>
    /// 刷新推流密钥
    /// </summary>
    Task<string> RefreshStreamKeyAsync(Guid liveId);
}
```

---

### 8.2 ILiveRoomService（直播间领域服务）

```csharp
/// <summary>
/// 直播间领域服务接口
/// </summary>
public interface ILiveRoomService
{
    /// <summary>
    /// 观众进入直播间
    /// </summary>
    Task ViewerJoinAsync(Guid liveId, Guid userId, string nickname);
    
    /// <summary>
    /// 观众离开直播间
    /// </summary>
    Task ViewerLeaveAsync(Guid liveId, Guid userId);
    
    /// <summary>
    /// 获取直播间观众列表
    /// </summary>
    Task<IReadOnlyList<LiveViewer>> GetViewersAsync(Guid liveId);
    
    /// <summary>
    /// 获取当前观众数
    /// </summary>
    Task<int> GetActiveViewerCountAsync(Guid liveId);
    
    /// <summary>
    /// 广播弹幕到直播间
    /// </summary>
    Task BroadcastDanmakuAsync(Guid liveId, string content, Guid userId);
    
    /// <summary>
    /// 广播礼物到直播间
    /// </summary>
    Task BroadcastGiftAsync(Guid liveId, LiveGift gift);
}
```

---

### 8.3 ILiveGiftService（礼物领域服务）

```csharp
/// <summary>
/// 礼物领域服务接口
/// 业务规则：礼物100%给UP主（用户需求）
/// </summary>
public interface ILiveGiftService
{
    /// <summary>
    /// 发送礼物
    /// </summary>
    Task<LiveGift> SendGiftAsync(
        Guid liveId,
        Guid userId,
        Guid giftTemplateId,
        int count,
        string? message = null);
    
    /// <summary>
    /// 获取礼物模板列表
    /// </summary>
    Task<IReadOnlyList<GiftTemplate>> GetGiftTemplatesAsync();
    
    /// <summary>
    /// 获取直播礼物记录
    /// </summary>
    Task<IReadOnlyList<LiveGift>> GetLiveGiftsAsync(Guid liveId, int limit = 50);
    
    /// <summary>
    /// 获取主播礼物收入统计
    /// </summary>
    Task<decimal> GetUpOwnerGiftIncomeAsync(Guid userId, DateTime? startTime = null, DateTime? endTime = null);
}

/// <summary>
/// 礼物模板
/// </summary>
public class GiftTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string IconUrl { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
```

---

### 8.4 ILiveStatisticsService（直播统计领域服务）

```csharp
/// <summary>
/// 直播统计领域服务接口
/// </summary>
public interface ILiveStatisticsService
{
    /// <summary>
    /// 更新实时观众数（从Redis同步）
    /// </summary>
    Task UpdateViewerCountAsync(Guid liveId);
    
    /// <summary>
    /// 获取热门直播列表
    /// </summary>
    Task<IReadOnlyList<(Guid LiveId, double HotScore)>> GetHotLivesAsync(int topN = 50);
    
    /// <summary>
    /// 获取主播直播统计
    /// </summary>
    Task<LiveStatistics> GetUserLiveStatisticsAsync(Guid userId);
    
    /// <summary>
    /// 获取直播排行（观众数/礼物收入）
    /// </summary>
    Task<IReadOnlyList<(Guid LiveId, int ViewerCount, decimal GiftAmount)>> GetLiveRankingAsync(RankingType type, int topN = 50);
}

/// <summary>
/// 排行类型枚举
/// </summary>
public enum RankingType
{
    ViewerCount = 1,    // 观众数排行
    GiftAmount = 2      // 礼物收入排行
}
```

---

## 9. 仓储接口设计

### 9.1 ILiveStreamRepository

```csharp
/// <summary>
/// 直播流仓储接口
/// </summary>
public interface ILiveStreamRepository : IRepository<LiveStream, Guid>
{
    /// <summary>
    /// 获取用户直播列表
    /// </summary>
    Task<List<LiveStream>> GetByUserIdAsync(Guid userId, int skip, int take);
    
    /// <summary>
    /// 获取正在直播的列表
    /// </summary>
    Task<List<LiveStream>> GetLiveStreamsAsync(int skip, int take);
    
    /// <summary>
    /// 获取热门直播
    /// </summary>
    Task<List<LiveStream>> GetHotLivesAsync(int topN);
    
    /// <summary>
    /// 根据状态获取
    /// </summary>
    Task<List<LiveStream>> GetByStatusAsync(LiveStatus status, int skip, int take);
    
    /// <summary>
    /// 根据分类获取
    /// </summary>
    Task<List<LiveStream>> GetByCategoryAsync(Guid categoryId, int skip, int take);
    
    /// <summary>
    /// 获取用户正在进行的直播
    /// </summary>
    Task<LiveStream?> GetUserCurrentLiveAsync(Guid userId);
}
```

---

### 9.2 ILiveRoomRepository

```csharp
/// <summary>
/// 直播间仓储接口
/// </summary>
public interface ILiveRoomRepository : IRepository<LiveRoom, Guid>
{
    /// <summary>
    /// 根据直播ID获取房间
    /// </summary>
    Task<LiveRoom?> GetByLiveIdAsync(Guid liveId);
    
    /// <summary>
    /// 获取活跃房间列表
    /// </summary>
    Task<List<LiveRoom>> GetActiveRoomsAsync(int skip, int take);
}
```

---

### 9.3 ILiveGiftRepository

```csharp
/// <summary>
/// 礼物记录仓储接口
/// </summary>
public interface ILiveGiftRepository : IRepository<LiveGift, Guid>
{
    /// <summary>
    /// 获取直播礼物列表
    /// </summary>
    Task<List<LiveGift>> GetByLiveIdAsync(Guid liveId, int skip, int take);
    
    /// <summary>
    /// 获取用户发送的礼物
    /// </summary>
    Task<List<LiveGift>> GetByUserIdAsync(Guid userId, int skip, int take);
    
    /// <summary>
    /// 获取主播收到的礼物
    /// </summary>
    Task<List<LiveGift>> GetReceivedByUserIdAsync(Guid userId, int skip, int take);
    
    /// <summary>
    /// 获取礼物统计（主播收入）
    /// </summary>
    Task<decimal> GetTotalReceivedByUserIdAsync(Guid userId, DateTime? startTime = null, DateTime? endTime = null);
}
```

---

### 9.4 ILiveViewerRepository

```csharp
/// <summary>
/// 直播观众仓储接口
/// </summary>
public interface ILiveViewerRepository : IRepository<LiveViewer, Guid>
{
    /// <summary>
    /// 获取直播观众列表
    /// </summary>
    Task<List<LiveViewer>> GetByLiveIdAsync(Guid liveId, int skip, int take);
    
    /// <summary>
    /// 获取活跃观众
    /// </summary>
    Task<List<LiveViewer>> GetActiveViewersAsync(Guid liveId);
    
    /// <summary>
    /// 获取用户观看记录
    /// </summary>
    Task<List<LiveViewer>> GetByUserIdAsync(Guid userId, int skip, int take);
}
```

---

## 10. 题领域事件设计

### 10.1 题领域事件列表

```csharp
/// <summary>
/// 直播创建事件
/// </summary>
public class LiveStreamCreatedEvent
{
    public Guid LiveId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }
    
    public LiveStreamCreatedEvent(Guid liveId, Guid userId, string title)
    {
        LiveId = liveId;
        UserId = userId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 直播开始事件
/// </summary>
public class LiveStartedEvent
{
    public Guid LiveId { get; }
    public Guid UserId { get; }
    public string Title { get; }
    public DateTime OccurredOn { get; }
    
    public LiveStartedEvent(Guid liveId, Guid userId, string title)
    {
        LiveId = liveId;
        UserId = userId;
        Title = title;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 直播结束事件
/// </summary>
public class LiveEndedEvent
{
    public Guid LiveId { get; }
    public Guid UserId { get; }
    public int TotalViewerCount { get; }
    public decimal TotalGiftAmount { get; }
    public DateTime OccurredOn { get; }
    
    public LiveEndedEvent(Guid liveId, Guid userId, int totalViewerCount, decimal totalGiftAmount)
    {
        LiveId = liveId;
        UserId = userId;
        TotalViewerCount = totalViewerCount;
        TotalGiftAmount = totalGiftAmount;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 观众加入事件
/// </summary>
public class LiveViewerJoinedEvent
{
    public Guid LiveId { get; }
    public Guid UserId { get; }
    public string Nickname { get; }
    public DateTime OccurredOn { get; }
    
    public LiveViewerJoinedEvent(Guid liveId, Guid userId, string nickname)
    {
        LiveId = liveId;
        UserId = userId;
        Nickname = nickname;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 观众离开事件
/// </summary>
public class LiveViewerLeftEvent
{
    public Guid LiveId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public LiveViewerLeftEvent(Guid liveId, Guid userId)
    {
        LiveId = liveId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 礼物发送事件
/// </summary>
public class LiveGiftSentEvent
{
    public Guid LiveId { get; }
    public Guid UserId { get; }
    public string GiftName { get; }
    public decimal Amount { get; }
    public DateTime OccurredOn { get; }
    
    public LiveGiftSentEvent(Guid liveId, Guid userId, string giftName, decimal amount)
    {
        LiveId = liveId;
        UserId = userId;
        GiftName = giftName;
        Amount = amount;
        OccurredOn = DateTime.UtcNow;
    }
}
```

---

## 11. 业务规则总结

### 11.1 直播流规则

| 规则 | 说明 |
|------|------|
| 开播权限 | 所有用户可开播（用户需求） |
| 直播延迟 | HLS<10秒（用户需求） |
| 推流协议 | RTMP |
| 播放协议 | HLS |
| 密钥有效期 | 24小时 |
| 密钥更换 | 支持更换密钥 |

---

### 11.2 礼物分成规则

| 规则 | 说明 |
|------|------|
| UP主分成 | 100%（用户需求） |
| 平台分成 | 0% |
| 礼物类型 | 预定义礼物模板 |
| 礼物数量 | 无限制 |
| 赠言 | 可选 |

---

### 11.3 观众管理规则

| 规则 | 说明 |
|------|------|
| 观众数量 | 无限制 |
| 观众活跃检测 | 30分钟无活跃视为离开 |
| 弹幕发送 | 支持实时弹幕 |
| 礼物发送 | 支持实时礼物 |
| 观看记录 | 记录观看时长 |

---

### 11.4 统计数据规则

| 规则 | 说明 |
|------|------|
| 实时观众数 | Redis实时同步 |
| 累计观众数 | 总观众数量 |
| 最高观众数 | 观众峰值 |
| 礼物总收入 | 累计礼物金额 |
| 热度计算 | 观众数权重50% + 礼物权重30% + 弹幕权重20% |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 直播模块领域设计文档完成