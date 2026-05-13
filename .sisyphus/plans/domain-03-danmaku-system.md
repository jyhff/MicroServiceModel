# 弹幕模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | DanmakuService（弹幕服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 频域模型概览

### 2.1 领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    弹幕领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │   Danmaku    │◄───── DanmakuContent (值对象)            │
│  │ (聚合根)     │◄───── DanmakuStyle (值对象)              │
│  └──────────────┘◄───── DanmakuPosition (值对象)           │
│         │         ◄───── DanmakuStatus (值对象)            │
│         │                                                  │
│         │                                                  │
│         ├────► DanmakuSegment (实体，时间段片段)           │
│         │                                                  │
│  ┌──────────────┐                                          │
│  │ DanmakuRoom  │                                          │
│  │ (聚合根)     │                                          │
│  │ (WebSocket)  │                                          │
│  └──────────────┘                                          │
│                                                             │
│  值对象：                                                    │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │DanmakuContent│ │DanmakuStyle  │ │DanmakuPosition│      │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐                        │
│  │DanmakuStatus │ │DanmakuFilter │                        │
│  └──────────────┘ └──────────────┘                        │
│                                                             │
│  实体：                                                      │
│  ┌──────────────┐                                          │
│  │DanmakuSegment│ (分区表存储)                             │
│  └──────────────┘                                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 Danmaku（弹幕聚合根）

**聚合边界定义**：
- 聚合根：Danmaku
- 外部引用：VideoId（视频ID）、UserId（用户ID）
- 内部结构：值对象组合（Content、Style、Position、Status）

**职责**：
- 管理弹幕内容和样式
- 控制弹幕位置和显示时间
- 维护弹幕状态和审核信息
- 支持高级弹幕权限控制

```csharp
/// <summary>
/// 弹幕聚合根
/// 业务规则：
/// 1. 弹幕并发无限制（用户需求）
/// 2. PostgreSQL分区表存储（用户需求）
/// 3. LV2+可使用高级弹幕（用户需求）
/// </summary>
public class Danmaku : AggregateRoot<Guid>
{
    // ========== 外部引用（ID引用，不聚合引用） ==========
    public Guid VideoId { get; private set; }     // 视频ID
    public Guid UserId { get; private set; }      // 发送用户ID
    
    // ========== 值对象 ==========
    private DanmakuContent _content;              // 弹幕内容
    private DanmakuStyle _style;                  // 弹幕样式（颜色、字号）
    private DanmakuPosition _position;            // 弹幕位置（时间、类型）
    private DanmakuStatus _status;                // 弹幕状态
    
    // ========== 用户信息快照（避免跨聚合查询） ==========
    public string UserNickname { get; private set; } // 用户昵称
    public int UserLevel { get; private set; }       // 用户等级（用于权限判断）
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? AuditedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private Danmaku() { } // EF Core
    
    private Danmaku(
        Guid id,
        Guid videoId,
        Guid userId,
        string userNickname,
        int userLevel,
        DanmakuContent content,
        DanmakuStyle style,
        DanmakuPosition position) : base(id)
    {
        VideoId = videoId;
        UserId = userId;
        UserNickname = userNickname;
        UserLevel = userLevel;
        _content = content;
        _style = style;
        _position = position;
        _status = DanmakuStatus.Normal; // 默认状态：正常
        CreatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new DanmakuSentEvent(Id, VideoId, UserId, Content));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建普通弹幕
    /// </summary>
    public static Danmaku CreateNormal(
        Guid videoId,
        Guid userId,
        string userNickname,
        int userLevel,
        string content,
        double time,
        DanmakuType type,
        string color,
        int fontSize)
    {
        // 业务规则：验证内容长度
        if (content.Length > 100)
            throw new BusinessException("弹幕内容不能超过100字符");
        
        // 业务规则：验证时间有效性
        if (time < 0)
            throw new BusinessException("弹幕时间不能为负数");
        
        var danmakuContent = DanmakuContent.Create(content);
        var danmakuStyle = DanmakuStyle.Create(color, fontSize);
        var danmakuPosition = DanmakuPosition.Create(time, type);
        
        return new Danmaku(
            GuidGenerator.Create(),
            videoId,
            userId,
            userNickname,
            userLevel,
            danmakuContent,
            danmakuStyle,
            danmakuPosition);
    }
    
    /// <summary>
    /// 创建高级弹幕
    /// 业务规则：LV2+用户才能发送高级弹幕
    /// </summary>
    public static Danmaku CreateAdvanced(
        Guid videoId,
        Guid userId,
        string userNickname,
        int userLevel,
        string content,
        double time,
        DanmakuType type,
        string color,
        int fontSize,
        string? motion = null,
        string? effect = null)
    {
        // 业务规则：LV2+才能使用高级弹幕
        if (userLevel < 2)
            throw new BusinessException("LV2及以上用户才能使用高级弹幕");
        
        // 业务规则：高级弹幕内容长度放宽到150字符
        if (content.Length > 150)
            throw new BusinessException("高级弹幕内容不能超过150字符");
        
        var danmakuContent = DanmakuContent.CreateAdvanced(content, motion, effect);
        var danmakuStyle = DanmakuStyle.CreateAdvanced(color, fontSize);
        var danmakuPosition = DanmakuPosition.Create(time, type);
        
        var danmaku = new Danmaku(
            GuidGenerator.Create(),
            videoId,
            userId,
            userNickname,
            userLevel,
            danmakuContent,
            danmakuStyle,
            danmakuPosition);
        
        danmaku._status = DanmakuStatus.Advanced;
        return danmaku;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 审核弹幕
    /// </summary>
    public void Audit(bool approved, string? reason = null)
    {
        if (approved)
        {
            _status = _status == DanmakuStatus.Advanced ? 
                DanmakuStatus.AdvancedApproved : DanmakuStatus.Approved;
        }
        else
        {
            _status = DanmakuStatus.Rejected;
        }
        
        AuditedAt = DateTime.UtcNow;
        
        AddDomainEvent(new DanmakuAuditedEvent(Id, VideoId, approved, reason));
    }
    
    /// <summary>
    /// 删除弹幕
    /// </summary>
    public void Delete(string reason)
    {
        _status = DanmakuStatus.Deleted;
        IsDeleted = true;
        
        AddDomainEvent(new DanmakuDeletedEvent(Id, VideoId, UserId, reason));
    }
    
    /// <summary>
    /// 屏蔽弹幕（用户屏蔽）
    /// </summary>
    public void Block()
    {
        _status = DanmakuStatus.Blocked;
    }
    
    /// <summary>
    /// 更新弹幕内容（仅限未审核状态）
    /// </summary>
    public void UpdateContent(string newContent)
    {
        // 业务规则：只能修改未审核的弹幕
        if (_status != DanmakuStatus.Normal && _status != DanmakuStatus.Advanced)
            throw new BusinessException("只能修改未审核的弹幕");
        
        if (newContent.Length > 100)
            throw new BusinessException("弹幕内容不能超过100字符");
        
        _content = DanmakuContent.Create(newContent);
    }
    
    // ========== 查询方法 ==========
    
    public string Content => _content.Text;
    public DanmakuType Type => _position.Type;
    public double Time => _position.VideoTime;
    public string Color => _style.Color;
    public int FontSize => _style.FontSize;
    public DanmakuStatus GetStatus() => _status;
    public bool IsAdvanced => _status == DanmakuStatus.Advanced || _status == DanmakuStatus.AdvancedApproved;
    public bool CanDisplay => _status == DanmakuStatus.Normal || 
                              _status == DanmakuStatus.Approved ||
                              _status == DanmakuStatus.AdvancedApproved;
    
    /// <summary>
    /// 获取弹幕显示信息（用于前端渲染）
    /// </summary>
    public DanmakuDisplayInfo GetDisplayInfo()
    {
        return new DanmakuDisplayInfo
        {
            Id = Id,
            Content = Content,
            Time = Time,
            Type = Type,
            Color = Color,
            FontSize = FontSize,
            UserId = UserId,
            UserNickname = UserNickname,
            UserLevel = UserLevel,
            IsAdvanced = IsAdvanced,
            CreatedAt = CreatedAt
        };
    }
}

/// <summary>
/// 弹幕显示信息DTO
/// </summary>
public class DanmakuDisplayInfo
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public double Time { get; set; }
    public DanmakuType Type { get; set; }
    public string Color { get; set; }
    public int FontSize { get; set; }
    public Guid UserId { get; set; }
    public string UserNickname { get; set; }
    public int UserLevel { get; set; }
    public bool IsAdvanced { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## 4. 值对象设计

### 4.1 DanmakuContent（弹幕内容值对象）

```csharp
/// <summary>
/// 弹幕内容值对象
/// </summary>
public class DanmakuContent : ValueObject
{
    // ========== 属性 ==========
    public string Text { get; }                  // 弹幕文本
    public string? Motion { get; }               // 运动轨迹（高级弹幕）
    public string? Effect { get; }               // 特效代码（高级弹幕）
    public bool IsAdvanced { get; }              // 是否高级弹幕
    
    // ========== 构造函数 ==========
    private DanmakuContent(string text, string? motion = null, string? effect = null, bool isAdvanced = false)
    {
        Text = text;
        Motion = motion;
        Effect = effect;
        IsAdvanced = isAdvanced;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建普通弹幕内容
    /// </summary>
    public static DanmakuContent Create(string text)
    {
        // 业务规则：普通弹幕最大100字符
        if (text.Length > 100)
            throw new BusinessException("弹幕内容不能超过100字符");
        
        // 业务规则：过滤敏感词（领域服务处理）
        return new DanmakuContent(text);
    }
    
    /// <summary>
    /// 创建高级弹幕内容
    /// </summary>
    public static DanmakuContent CreateAdvanced(string text, string? motion, string? effect)
    {
        // 业务规则：高级弹幕最大150字符
        if (text.Length > 150)
            throw new BusinessException("高级弹幕内容不能超过150字符");
        
        return new DanmakuContent(text, motion, effect, true);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否包含敏感词（需要领域服务配合）
    /// </summary>
    public bool ContainsSensitiveWords(IReadOnlyList<string> sensitiveWords)
    {
        return sensitiveWords.Any(w => Text.Contains(w));
    }
    
    /// <summary>
    /// 是否纯表情
    /// </summary>
    public bool IsOnlyEmoji()
    {
        return Regex.IsMatch(Text, @"^[\p{S}\p{P}\p{Sm}\p{Sc}\p{Sk}\p{So}]+$");
    }
    
    /// <summary>
    /// 是否包含链接
    /// </summary>
    public bool ContainsUrl()
    {
        return Regex.IsMatch(Text, @"https?://\S+");
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Text;
        yield return Motion ?? string.Empty;
        yield return Effect ?? string.Empty;
        yield return IsAdvanced;
    }
}
```

---

### 4.2 DanmakuStyle（弹幕样式值对象）

```csharp
/// <summary>
/// 弹幕样式值对象
/// </summary>
public class DanmakuStyle : ValueObject
{
    // ========== 属性 ==========
    public string Color { get; }                 // 颜色（十六进制）
    public int FontSize { get; }                 // 字号
    public bool IsBold { get; }                  // 是否加粗
    public bool IsItalic { get; }                // 是否斜体
    
    // ========== 预定义颜色常量 ==========
    public static readonly IReadOnlyList<string> DefaultColors = new List<string>
    {
        "#FFFFFF", // 白色（默认）
        "#FE0302", // 红色
        "#00CD00", // 绿色
        "#019899", // 青色
        "#FFAA02", // 黄色
        "#7200F1", // 紫色
        "#CA007F", // 粉色
        "#00D8FF", // 蓝色
        "#9B9B9B", // 灰色
        "#A0A0A0"  // 深灰
    };
    
    // ========== 预定义字号常量 ==========
    public static readonly IReadOnlyList<int> DefaultFontSizes = new List<int>
    {
        18,  // 小号
        24,  // 标准（默认）
        36,  // 大号
        45,  // 超大号
        64   // 巨大号（高级弹幕）
    };
    
    // ========== 构造函数 ==========
    private DanmakuStyle(string color, int fontSize, bool isBold = false, bool isItalic = false)
    {
        Color = color;
        FontSize = fontSize;
        IsBold = isBold;
        IsItalic = isItalic;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建标准样式
    /// </summary>
    public static DanmakuStyle Create(string color, int fontSize)
    {
        // 业务规则：颜色必须是有效的十六进制格式
        if (!IsValidColor(color))
            throw new BusinessException($"无效的颜色格式: {color}");
        
        // 业务规则：字号必须在允许范围内
        if (!DefaultFontSizes.Contains(fontSize))
            fontSize = 24; // 默认标准字号
        
        return new DanmakuStyle(color, fontSize);
    }
    
    /// <summary>
    /// 创建高级样式
    /// </summary>
    public static DanmakuStyle CreateAdvanced(string color, int fontSize)
    {
        // 高级弹幕允许自定义颜色和字号
        if (!IsValidColor(color))
            color = "#FFFFFF"; // 默认白色
        
        // 高级弹幕字号范围更大
        if (fontSize < 12 || fontSize > 100)
            fontSize = 24;
        
        return new DanmakuStyle(color, fontSize);
    }
    
    /// <summary>
    /// 创建默认样式（白色、标准字号）
    /// </summary>
    public static DanmakuStyle Default()
    {
        return new DanmakuStyle("#FFFFFF", 24);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 验证颜色格式
    /// </summary>
    private static bool IsValidColor(string color)
    {
        return Regex.IsMatch(color, @"^#[0-9A-Fa-f]{6}$");
    }
    
    /// <summary>
    /// 是否默认样式
    /// </summary>
    public bool IsDefault()
    {
        return Color == "#FFFFFF" && FontSize == 24 && !IsBold && !IsItalic;
    }
    
    /// <summary>
    /// 是否高对比度颜色（易于阅读）
    /// </summary>
    public bool IsHighContrast()
    {
        var highContrastColors = new[] { "#FFFFFF", "#FE0302", "#00CD00", "#FFAA02" };
        return highContrastColors.Contains(Color);
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Color;
        yield return FontSize;
        yield return IsBold;
        yield return IsItalic;
    }
}
```

---

### 4.3 DanmakuPosition（弹幕位置值对象）

```csharp
/// <summary>
/// 弹幕位置值对象
/// </summary>
public class DanmakuPosition : ValueObject
{
    // ========== 属性 ==========
    public double VideoTime { get; }             // 视频时间（秒）
    public DanmakuType Type { get; }             // 弹幕类型（滚动/顶部/底部）
    public int? PositionY { get; }               // Y轴位置（顶部/底部弹幕）
    
    // ========== 构造函数 ==========
    private DanmakuPosition(double videoTime, DanmakuType type, int? positionY = null)
    {
        VideoTime = videoTime;
        Type = type;
        PositionY = positionY;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建弹幕位置
    /// </summary>
    public static DanmakuPosition Create(double videoTime, DanmakuType type, int? positionY = null)
    {
        // 业务规则：时间不能为负数
        if (videoTime < 0)
            throw new BusinessException("弹幕时间不能为负数");
        
        // 业务规则：顶部/底部弹幕必须有Y位置
        if ((type == DanmakuType.Top || type == DanmakuType.Bottom) && positionY == null)
        {
            // 自动计算Y位置（避免重叠）
            positionY = CalculateDefaultY(type);
        }
        
        return new DanmakuPosition(videoTime, type, positionY);
    }
    
    /// <summary>
    /// 计算默认Y位置
    /// </summary>
    private static int CalculateDefaultY(DanmakuType type)
    {
        // 默认：顶部弹幕在顶部区域，底部弹幕在底部区域
        return type == DanmakuType.Top ? 0 : 100;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否在同一时间窗口（用于碰撞检测）
    /// </summary>
    public bool IsInTimeWindow(DanmakuPosition other, double windowSeconds = 3.0)
    {
        return Math.Abs(VideoTime - other.VideoTime) <= windowSeconds;
    }
    
    /// <summary>
    /// 是否会碰撞（同类型、同时间、同位置）
    /// </summary>
    public bool WillCollide(DanmakuPosition other)
    {
        if (Type != other.Type) return false;
        if (!IsInTimeWindow(other)) return false;
        
        // 滚动弹幕：时间相近即碰撞
        if (Type == DanmakuType.Scroll)
            return true;
        
        // 顶部/底部弹幕：同Y位置碰撞
        return PositionY == other.PositionY;
    }
    
    /// <summary>
    /// 获取显示时长（根据类型）
    /// </summary>
    public double GetDisplayDuration()
    {
        return Type switch
        {
            DanmakuType.Scroll => 5.0,     // 滚动弹幕：5秒横移
            DanmakuType.Top => 3.0,        // 顶部弹幕：3秒停留
            DanmakuType.Bottom => 3.0,     // 底部弹幕：3秒停留
            _ => 5.0
        };
    }
    
    /// <summary>
    /// 格式化时间显示
    /// </summary>
    public string FormatTime()
    {
        var minutes = (int)(VideoTime / 60);
        var seconds = (int)(VideoTime % 60);
        return $"{minutes:D2}:{seconds:D2}";
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return VideoTime;
        yield return Type;
        yield return PositionY ?? 0;
    }
}

/// <summary>
/// 弹幕类型枚举
/// </summary>
public enum DanmakuType
{
    Scroll = 0,     // 滚动弹幕（从右到左）
    Top = 1,        // 顶部弹幕（固定）
    Bottom = 2,     // 底部弹幕（固定）
    Reverse = 3,    // 逆向弹幕（高级弹幕：从左到右）
    Precise = 4,    // 精准定位弹幕（高级弹幕）
    Custom = 5      // 自定义弹幕（高级弹幕）
}
```

---

### 4.4 DanmakuStatus（弹幕状态值对象）

```csharp
/// <summary>
/// 弹幕状态值对象
/// </summary>
public class DanmakuStatus : ValueObject
{
    // ========== 状态常量 ==========
    public static readonly DanmakuStatus Normal = new("normal", "正常");
    public static readonly DanmakuStatus Approved = new("approved", "已审核");
    public static readonly DanmakuStatus Rejected = new("rejected", "已拒绝");
    public static readonly DanmakuStatus Deleted = new("deleted", "已删除");
    public static readonly DanmakuStatus Blocked = new("blocked", "已屏蔽");
    public static readonly DanmakuStatus Advanced = new("advanced", "高级弹幕（待审核）");
    public static readonly DanmakuStatus AdvancedApproved = new("advanced_approved", "高级弹幕（已审核）");
    
    // ========== 属性 ==========
    public string Code { get; }
    public string DisplayName { get; }
    
    // ========== 构造函数 ==========
    private DanmakuStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否可以显示
    /// </summary>
    public bool CanDisplay()
    {
        return this == Normal || this == Approved || this == AdvancedApproved;
    }
    
    /// <summary>
    /// 是否需要审核
    /// </summary>
    public bool NeedAudit()
    {
        return this == Normal || this == Advanced;
    }
    
    /// <summary>
    /// 是否已审核
    /// </summary>
    public bool IsAudited()
    {
        return this == Approved || this == Rejected || this == AdvancedApproved;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
}
```

---

### 4.5 DanmakuFilter（弹幕过滤器值对象）

```csharp
/// <summary>
/// 弹幕过滤器值对象（用于查询过滤）
/// </summary>
public class DanmakuFilter : ValueObject
{
    // ========== 属性 ==========
    public double? StartTime { get; }            // 开始时间
    public double? EndTime { get; }              // 结束时间
    public IReadOnlyList<DanmakuType>? Types { get; } // 弹幕类型过滤
    public IReadOnlyList<string>? Colors { get; }     // 颜色过滤
    public int? MinFontSize { get; }             // 最小字号
    public int? MaxFontSize { get; }             // 最大字号
    public bool? OnlyAdvanced { get; }           // 仅高级弹幕
    public bool? ExcludeAdvanced { get; }        // 排除高级弹幕
    public IReadOnlyList<Guid>? ExcludeUsers { get; } // 排除用户
    
    // ========== 构造函数 ==========
    private DanmakuFilter(
        double? startTime,
        double? endTime,
        IReadOnlyList<DanmakuType>? types,
        IReadOnlyList<string>? colors,
        int? minFontSize,
        int? maxFontSize,
        bool? onlyAdvanced,
        bool? excludeAdvanced,
        IReadOnlyList<Guid>? excludeUsers)
    {
        StartTime = startTime;
        EndTime = endTime;
        Types = types;
        Colors = colors;
        MinFontSize = minFontSize;
        MaxFontSize = maxFontSize;
        OnlyAdvanced = onlyAdvanced;
        ExcludeAdvanced = excludeAdvanced;
        ExcludeUsers = excludeUsers;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建时间范围过滤器
    /// </summary>
    public static DanmakuFilter ByTimeRange(double startTime, double endTime)
    {
        return new DanmakuFilter(startTime, endTime, null, null, null, null, null, null, null);
    }
    
    /// <summary>
    /// 创建类型过滤器
    /// </summary>
    public static DanmakuFilter ByType(IReadOnlyList<DanmakuType> types)
    {
        return new DanmakuFilter(null, null, types, null, null, null, null, null, null);
    }
    
    /// <summary>
    /// 创建空过滤器（获取所有）
    /// </summary>
    public static DanmakuFilter Empty()
    {
        return new DanmakuFilter(null, null, null, null, null, null, null, null, null);
    }
    
    /// <summary>
    /// 创建用户屏蔽过滤器
    /// </summary>
    public static DanmakuFilter ExcludeUsers(IReadOnlyList<Guid> userIds)
    {
        return new DanmakuFilter(null, null, null, null, null, null, null, null, userIds);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否匹配弹幕
    /// </summary>
    public bool Matches(Danmaku danmaku)
    {
        // 时间范围检查
        if (StartTime.HasValue && danmaku.Time < StartTime.Value)
            return false;
        if (EndTime.HasValue && danmaku.Time > EndTime.Value)
            return false;
        
        // 类型检查
        if (Types != null && !Types.Contains(danmaku.Type))
            return false;
        
        // 颜色检查
        if (Colors != null && !Colors.Contains(danmaku.Color))
            return false;
        
        // 字号检查
        if (MinFontSize.HasValue && danmaku.FontSize < MinFontSize.Value)
            return false;
        if (MaxFontSize.HasValue && danmaku.FontSize > MaxFontSize.Value)
            return false;
        
        // 高级弹幕检查
        if (OnlyAdvanced.HasValue && OnlyAdvanced.Value && !danmaku.IsAdvanced)
            return false;
        if (ExcludeAdvanced.HasValue && ExcludeAdvanced.Value && danmaku.IsAdvanced)
            return false;
        
        // 用户屏蔽检查
        if (ExcludeUsers != null && ExcludeUsers.Contains(danmaku.UserId))
            return false;
        
        return true;
    }
    
    /// <summary>
    /// 合并过滤器
    /// </summary>
    public DanmakuFilter Merge(DanmakuFilter other)
    {
        return new DanmakuFilter(
            StartTime ?? other.StartTime,
            EndTime ?? other.EndTime,
            Types ?? other.Types,
            Colors ?? other.Colors,
            MinFontSize ?? other.MinFontSize,
            MaxFontSize ?? other.MaxFontSize,
            OnlyAdvanced ?? other.OnlyAdvanced,
            ExcludeAdvanced ?? other.ExcludeAdvanced,
            ExcludeUsers ?? other.ExcludeUsers);
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return StartTime ?? 0;
        yield return EndTime ?? 0;
    }
}
```

---

## 5. 实体设计

### 5.1 DanmakuSegment（弹幕分段实体）

```csharp
/// <summary>
/// 弹幕分段实体
/// 业务规则：用于PostgreSQL分区表存储（按时间分段）
/// </summary>
public class DanmakuSegment : Entity<Guid>
{
    // ========== 外部引用 ==========
    public Guid VideoId { get; private set; }
    
    // ========== 属性 ==========
    public int SegmentIndex { get; private set; }    // 分段序号（10秒一段）
    public double StartTime { get; private set; }    // 分段开始时间
    public double EndTime { get; private set; }      // 分段结束时间
    public int DanmakuCount { get; private set; }    // 弹幕数量（统计）
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private DanmakuSegment() { }
    
    private DanmakuSegment(Guid videoId, int segmentIndex, double startTime, double endTime) : base(GuidGenerator.Create())
    {
        VideoId = videoId;
        SegmentIndex = segmentIndex;
        StartTime = startTime;
        EndTime = endTime;
        DanmakuCount = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建分段（每10秒一个分段）
    /// </summary>
    public static DanmakuSegment Create(Guid videoId, double videoTime)
    {
        var segmentIndex = (int)(videoTime / 10);
        var startTime = segmentIndex * 10;
        var endTime = startTime + 10;
        
        return new DanmakuSegment(videoId, segmentIndex, startTime, endTime);
    }
    
    /// <summary>
    /// 计算分段序号
    /// </summary>
    public static int CalculateSegmentIndex(double videoTime)
    {
        return (int)(videoTime / 10);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 增加弹幕计数
    /// </summary>
    public void IncrementDanmakuCount()
    {
        DanmakuCount++;
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 减少弹幕计数
    /// </summary>
    public void DecrementDanmakuCount()
    {
        DanmakuCount = Math.Max(0, DanmakuCount - 1);
        UpdatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 是否包含指定时间
    /// </summary>
    public bool ContainsTime(double time)
    {
        return time >= StartTime && time < EndTime;
    }
    
    /// <summary>
    /// 是否热门分段（弹幕密度高）
    /// </summary>
    public bool IsHotSegment()
    {
        return DanmakuCount > 100; // 10秒内超过100条弹幕为热门
    }
}
```

---

## 6. 聚合根设计：DanmakuRoom（弹幕房间聚合）

### 6.1 DanmakuRoom（弹幕房间聚合根）

```csharp
/// <summary>
/// 弹幕房间聚合根（WebSocket连接管理）
/// </summary>
public class DanmakuRoom : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid VideoId { get; private set; }
    
    // ========== 属性 ==========
    public string RoomId { get; private set; }       // 房间ID（字符串，便于WebSocket路由）
    public int ActiveUserCount { get; private set; } // 当前在线用户数
    public int TotalDanmakuCount { get; private set; } // 房间弹幕总数
    public DateTime CreatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public RoomStatus Status { get; private set; }
    
    // ========== 构造函数 ==========
    private DanmakuRoom() { }
    
    private DanmakuRoom(Guid videoId) : base(GuidGenerator.Create())
    {
        VideoId = videoId;
        RoomId = $"room_{videoId:N}";
        ActiveUserCount = 0;
        TotalDanmakuCount = 0;
        Status = RoomStatus.Active;
        CreatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    public static DanmakuRoom Create(Guid videoId)
    {
        return new DanmakuRoom(videoId);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 用户加入房间
    /// </summary>
    public void UserJoin()
    {
        ActiveUserCount++;
    }
    
    /// <summary>
    /// 用户离开房间
    /// </summary>
    public void UserLeave()
    {
        ActiveUserCount = Math.Max(0, ActiveUserCount - 1);
    }
    
    /// <summary>
    /// 弹幕发送
    /// </summary>
    public void DanmakuSent()
    {
        TotalDanmakuCount++;
    }
    
    /// <summary>
    /// 关闭房间
    /// </summary>
    public void Close()
    {
        Status = RoomStatus.Closed;
        ClosedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 是否活跃
    /// </summary>
    public bool IsActive => Status == RoomStatus.Active && ActiveUserCount > 0;
}

/// <summary>
/// 房间状态枚举
/// </summary>
public enum RoomStatus
{
    Active = 1,     // 活跃
    Closed = 2      // 已关闭
}
```

---

## 7. 领域服务设计

### 7.1 IDanmakuCollisionService（弹幕碰撞检测领域服务）

```csharp
/// <summary>
/// 弹幕碰撞检测领域服务
/// 用于避免弹幕重叠显示
/// </summary>
public interface IDanmakuCollisionService
{
    /// <summary>
    /// 检测滚动弹幕碰撞
    /// </summary>
    Task<bool> WillCollideAsync(Guid videoId, double time, string content);
    
    /// <summary>
    /// 计算安全Y位置（顶部/底部弹幕）
    /// </summary>
    Task<int> CalculateSafeYPositionAsync(Guid videoId, double time, DanmakuType type);
    
    /// <summary>
    /// 获取时间窗口内的弹幕列表
    /// </summary>
    Task<IReadOnlyList<Danmaku>> GetDanmakuInTimeWindowAsync(Guid videoId, double time, double window);
    
    /// <summary>
    /// 批量计算弹幕位置（避免碰撞）
    /// </summary>
    Task<IReadOnlyList<DanmakuPosition>> CalculateBatchPositionsAsync(
        Guid videoId, 
        IReadOnlyList<(double time, DanmakuType type, string content)> danmakuInfos);
}
```

---

### 7.2 IDanmakuFilterService（弹幕过滤领域服务）

```csharp
/// <summary>
/// 弹幕过滤领域服务
/// </summary>
public interface IDanmakuFilterService
{
    /// <summary>
    /// 应用过滤器获取弹幕列表
    /// </summary>
    Task<IReadOnlyList<Danmaku>> FilterDanmakuAsync(
        Guid videoId,
        DanmakuFilter filter,
        int skip = 0,
        int take = 100);
    
    /// <summary>
    /// 按用户屏蔽过滤
    /// </summary>
    Task<IReadOnlyList<Danmaku>> ExcludeBlockedUsersAsync(
        Guid videoId,
        Guid requestingUserId,
        IReadOnlyList<Danmaku> danmakuList);
    
    /// <summary>
    /// 按关键词屏蔽过滤
    /// </summary>
    Task<IReadOnlyList<Danmaku>> ExcludeBlockedKeywordsAsync(
        Guid videoId,
        Guid requestingUserId,
        IReadOnlyList<Danmaku> danmakuList);
    
    /// <summary>
    /// 获取用户屏蔽设置
    /// </summary>
    Task<UserBlockSettings> GetUserBlockSettingsAsync(Guid userId);
}

/// <summary>
/// 用户屏蔽设置
/// </summary>
public class UserBlockSettings
{
    public IReadOnlyList<Guid> BlockedUsers { get; set; } = new List<Guid>();
    public IReadOnlyList<string> BlockedKeywords { get; set; } = new List<string>();
    public bool BlockAdvancedDanmaku { get; set; }
    public bool BlockColorDanmaku { get; set; }
}
```

---

### 7.3 IDanmakuStatisticsService（弹幕统计领域服务）

```csharp
/// <summary>
/// 弹幕统计领域服务
/// </summary>
public interface IDanmakuStatisticsService
{
    /// <summary>
    /// 获取视频弹幕总数
    /// </summary>
    Task<long> GetTotalCountAsync(Guid videoId);
    
    /// <summary>
    /// 获取分段弹幕密度（用于热度图）
    /// </summary>
    Task<IReadOnlyList<(int SegmentIndex, int Count)>> GetSegmentDensityAsync(Guid videoId);
    
    /// <summary>
    /// 获取热门时间段（弹幕密度最高）
    /// </summary>
    Task<IReadOnlyList<(double StartTime, double EndTime, int Count)>> GetHotTimeRangesAsync(
        Guid videoId, 
        int topN = 10);
    
    /// <summary>
    /// 获取用户发送弹幕统计
    /// </summary>
    Task<IReadOnlyList<(Guid UserId, int Count)>> GetTopSendersAsync(Guid videoId, int topN = 10);
    
    /// <summary>
    /// 批量更新统计（从Redis同步）
    /// </summary>
    Task UpdateStatisticsBatchAsync(IReadOnlyDictionary<Guid, int> danmakuCounts);
}
```

---

### 7.4 IDanmakuRealtimeService（弹幕实时推送领域服务）

```csharp
/// <summary>
/// 弹幕实时推送领域服务
/// </summary>
public interface IDanmakuRealtimeService
{
    /// <summary>
    /// 推送弹幕到房间所有用户
    /// </summary>
    Task BroadcastDanmakuAsync(Guid videoId, DanmakuDisplayInfo danmaku);
    
    /// <summary>
    /// 推送系统消息
    /// </summary>
    Task BroadcastSystemMessageAsync(Guid videoId, string message);
    
    /// <summary>
    /// 获取房间在线用户数
    /// </summary>
    Task<int> GetActiveUserCountAsync(Guid videoId);
    
    /// <summary>
    /// 缓存弹幕到Redis（实时缓冲）
    /// </summary>
    Task CacheDanmakuToRedisAsync(Guid videoId, DanmakuDisplayInfo danmaku);
    
    /// <summary>
    /// 从Redis获取最近弹幕
    /// </summary>
    Task<IReadOnlyList<DanmakuDisplayInfo>> GetRecentDanmakuFromRedisAsync(
        Guid videoId, 
        int limit = 100);
    
    /// <summary>
    /// 批量同步Redis缓存到数据库
    /// </summary>
    Task<int> SyncRedisToDatabaseAsync(Guid videoId);
}
```

---

## 8. 仓储接口设计

### 8.1 IDanmakuRepository

```csharp
/// <summary>
/// 弹幕仓储接口
/// 业务规则：使用PostgreSQL分区表存储（用户需求）
/// </summary>
public interface IDanmakuRepository : IRepository<Danmaku, Guid>
{
    /// <summary>
    /// 根据视频ID获取弹幕列表（时间范围）
    /// </summary>
    Task<List<Danmaku>> GetByVideoIdAsync(
        Guid videoId,
        double startTime,
        double endTime,
        int skip = 0,
        int take = 100);
    
    /// <summary>
    /// 根据视频ID获取弹幕列表（按分段）
    /// </summary>
    Task<List<Danmaku>> GetBySegmentAsync(
        Guid videoId,
        int segmentIndex);
    
    /// <summary>
    /// 根据用户ID获取弹幕列表
    /// </summary>
    Task<List<Danmaku>> GetByUserIdAsync(Guid userId, int skip, int take);
    
    /// <summary>
    /// 获取视频弹幕总数
    /// </summary>
    Task<long> CountByVideoIdAsync(Guid videoId);
    
    /// <summary>
    /// 获取待审核弹幕列表
    /// </summary>
    Task<List<Danmaku>> GetPendingAuditAsync(int skip, int take);
    
    /// <summary>
    /// 批量插入弹幕（分区表优化）
    /// </summary>
    Task<int> BulkInsertAsync(IReadOnlyList<Danmaku> danmakuList);
    
    /// <summary>
    /// 按过滤器查询
    /// </summary>
    Task<List<Danmaku>> FilterAsync(
        Guid videoId,
        DanmakuFilter filter,
        int skip,
        int take);
    
    /// <summary>
    /// 创建分区（按视频ID）
    /// </summary>
    Task CreatePartitionAsync(Guid videoId);
    
    /// <summary>
    /// 删除视频所有弹幕
    /// </summary>
    Task<int> DeleteByVideoIdAsync(Guid videoId);
}
```

---

### 8.2 IDanmakuSegmentRepository

```csharp
/// <summary>
/// 弹幕分段仓储接口
/// </summary>
public interface IDanmakuSegmentRepository : IRepository<DanmakuSegment, Guid>
{
    /// <summary>
    /// 根据视频ID获取所有分段
    /// </summary>
    Task<List<DanmakuSegment>> GetByVideoIdAsync(Guid videoId);
    
    /// <summary>
    /// 根据视频ID和时间获取分段
    /// </summary>
    Task<DanmakuSegment?> GetByTimeAsync(Guid videoId, double time);
    
    /// <summary>
    /// 获取热门分段（弹幕密度高）
    /// </summary>
    Task<List<DanmakuSegment>> GetHotSegmentsAsync(Guid videoId, int topN = 10);
    
    /// <summary>
    /// 更新分段计数
    /// </summary>
    Task UpdateCountsAsync(IReadOnlyDictionary<Guid, int> counts);
}
```

---

### 8.3 IDanmakuRoomRepository

```csharp
/// <summary>
/// 弹幕房间仓储接口
/// </summary>
public interface IDanmakuRoomRepository : IRepository<DanmakuRoom, Guid>
{
    /// <summary>
    /// 根据视频ID获取房间
    /// </summary>
    Task<DanmakuRoom?> GetByVideoIdAsync(Guid videoId);
    
    /// <summary>
    /// 获取活跃房间列表
    /// </summary>
    Task<List<DanmakuRoom>> GetActiveRoomsAsync(int skip, int take);
    
    /// <summary>
    /// 获取热门房间（在线用户数高）
    /// </summary>
    Task<List<DanmakuRoom>> GetHotRoomsAsync(int topN = 10);
}
```

---

## 9. 频域事件设计

### 9.1 频域事件列表

```csharp
/// <summary>
/// 弹幕发送事件
/// </summary>
public class DanmakuSentEvent
{
    public Guid DanmakuId { get; }
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public string Content { get; }
    public DateTime OccurredOn { get; }
    
    public DanmakuSentEvent(Guid danmakuId, Guid videoId, Guid userId, string content)
    {
        DanmakuId = danmakuId;
        VideoId = videoId;
        UserId = userId;
        Content = content;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 弹幕审核事件
/// </summary>
public class DanmakuAuditedEvent
{
    public Guid DanmakuId { get; }
    public Guid VideoId { get; }
    public bool Approved { get; }
    public string? Reason { get; }
    public DateTime OccurredOn { get; }
    
    public DanmakuAuditedEvent(Guid danmakuId, Guid videoId, bool approved, string? reason)
    {
        DanmakuId = danmakuId;
        VideoId = videoId;
        Approved = approved;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 弹幕删除事件
/// </summary>
public class DanmakuDeletedEvent
{
    public Guid DanmakuId { get; }
    public Guid VideoId { get; }
    public Guid UserId { get; }
    public string Reason { get; }
    public DateTime OccurredOn { get; }
    
    public DanmakuDeletedEvent(Guid danmakuId, Guid videoId, Guid userId, string reason)
    {
        DanmakuId = danmakuId;
        VideoId = videoId;
        UserId = userId;
        Reason = reason;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 弹幕房间创建事件
/// </summary>
public class DanmakuRoomCreatedEvent
{
    public Guid RoomId { get; }
    public Guid VideoId { get; }
    public DateTime OccurredOn { get; }
    
    public DanmakuRoomCreatedEvent(Guid roomId, Guid videoId)
    {
        RoomId = roomId;
        VideoId = videoId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 用户加入房间事件
/// </summary>
public class UserJoinedRoomEvent
{
    public Guid RoomId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public UserJoinedRoomEvent(Guid roomId, Guid userId)
    {
        RoomId = roomId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 用户离开房间事件
/// </summary>
public class UserLeftRoomEvent
{
    public Guid RoomId { get; }
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public UserLeftRoomEvent(Guid roomId, Guid userId)
    {
        RoomId = roomId;
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}
```

---

## 10. 业务规则总结

### 10.1 弹幕发送规则

| 规则 | 说明 |
|------|------|
| 弹幕并发 | 无限制（用户需求） |
| 内容长度 | 普通弹幕≤100字符，高级弹幕≤150字符 |
| 高级弹幕权限 | LV2+用户才能使用 |
| 弹幕类型 | 滚动(0)、顶部(1)、底部(2) |
| 显示时长 | 滚动5秒，顶部/底部3秒 |

---

### 10.2 弹幕存储规则

| 规则 | 说明 |
|------|------|
| 存储方式 | PostgreSQL分区表（用户需求） |
| 分区策略 | 按视频ID分区 |
| 分段策略 | 每10秒一个分段 |
| Redis缓存 | 实时弹幕缓冲（最近100条） |
| 批量同步 | 定时同步Redis到数据库 |

---

### 10.3 弹幕过滤规则

| 规则 | 说明 |
|------|------|
| 时间范围过滤 | 支持指定时间段 |
| 类型过滤 | 支持按弹幕类型筛选 |
| 用户屏蔽 | 用户可屏蔽特定用户弹幕 |
| 关键词屏蔽 | 用户可屏蔽关键词 |
| 高级弹幕屏蔽 | 用户可选择屏蔽高级弹幕 |

---

### 10.4 WebSocket规则

| 规则 | 说明 |
|------|------|
| 连接协议 | WebSocket |
| 认证方式 | JWT Token |
| 房间隔离 | 每个视频独立房间 |
| 推送延迟 | 实时推送（<100ms） |
| 断线重连 | 支持断线重连 |

---

### 10.5 弹幕统计规则

| 规则 | 说明 |
|------|------|
| 弹幕总数 | 每个视频统计总数 |
| 分段密度 | 每10秒统计弹幕数量 |
| 热门时间段 | 弹幕密度最高的时间段 |
| 热门分段 | 弹幕数量>100为热门分段 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 弹幕模块领域设计文档完成