# 推荐模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | RecommendService（推荐服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 领域模型概览

### 2.1 领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    推荐领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │ UserBehavior │◄───── BehaviorType (值对象)              │
│  │ (聚合根)     │◄───── BehaviorWeight (值对象)            │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │Recommendation│◄───── RecommendScore (值对象)           │
│  │ (聚合根)     │◄───── RecommendReason (值对象)          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ UserInterest │                                          │
│  │ (聚合根)     │                                          │
│  │ (用户画像)   │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ SimilarUser  │                                          │
│  │ (实体)       │                                          │
│  └──────────────┘                                          │
│                                                             │
│  值对象：                                                    │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │BehaviorType  │ │BehaviorWeight│ │RecommendScore │      │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
│  ┌──────────────┐                                          │
│  │RecommendReason│                                         │
│  └──────────────┘                                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 UserBehavior（用户行为聚合根）

**聚合边界定义**：
- 聚合根：UserBehavior
- 外部引用：UserId（用户ID）、VideoId（视频ID）
- 值对象：BehaviorType、BehaviorWeight

**职责**：
- 记录用户对视频的行为（观看、点赞、收藏、分享、投币）
- 计算行为权重（用于推荐算法）
- 提供行为数据用于协同过滤

```csharp
/// <summary>
/// 用户行为聚合根
/// 业务规则：
/// 1. 记录用户观看、点赞、收藏、分享、投币行为
/// 2. 行为权重用于推荐算法
/// 3. 实时同步到Elasticsearch（用户需求）
/// </summary>
public class UserBehavior : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 用户ID
    public Guid VideoId { get; private set; }    // 视频ID
    public Guid? CategoryId { get; private set; } // 分类ID（用于分类推荐）
    
    // ========== 值对象 ==========
    private BehaviorType _type;                  // 行为类型
    private BehaviorWeight _weight;              // 行为权重
    
    // ========== 属性 ==========
    public double? WatchProgress { get; private set; } // 观看进度（百分比）
    public double? WatchDuration { get; private set; } // 观看时长（秒）
    public int? CoinAmount { get; private set; }       // 投币数量
    public string? Comment { get; private set; }       // 评论内容（评论行为）
    
    // ========== 时间戳 ==========
    public DateTime OccurredAt { get; private set; }   // 行为发生时间
    public DateTime? UpdatedAt { get; private set; }   // 更新时间（观看进度更新）
    
    // ========== 构造函数 ==========
    private UserBehavior() { }
    
    private UserBehavior(
        Guid id,
        Guid userId,
        Guid videoId,
        Guid? categoryId,
        BehaviorType type,
        BehaviorWeight weight) : base(id)
    {
        UserId = userId;
        VideoId = videoId;
        CategoryId = categoryId;
        _type = type;
        _weight = weight;
        OccurredAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserBehaviorRecordedEvent(Id, UserId, VideoId, type));
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 记录观看行为
    /// </summary>
    public static UserBehavior CreateWatch(
        Guid userId,
        Guid videoId,
        Guid? categoryId,
        double watchProgress,
        double watchDuration)
    {
        var type = BehaviorType.Watch;
        var weight = BehaviorWeight.CalculateForWatch(watchProgress);
        
        var behavior = new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
        
        behavior.WatchProgress = watchProgress;
        behavior.WatchDuration = watchDuration;
        
        return behavior;
    }
    
    /// <summary>
    /// 记录点赞行为
    /// </summary>
    public static UserBehavior CreateLike(
        Guid userId,
        Guid videoId,
        Guid? categoryId)
    {
        var type = BehaviorType.Like;
        var weight = BehaviorWeight.ForLike();
        
        return new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
    }
    
    /// <summary>
    /// 记录收藏行为
    /// </summary>
    public static UserBehavior CreateFavorite(
        Guid userId,
        Guid videoId,
        Guid? categoryId)
    {
        var type = BehaviorType.Favorite;
        var weight = BehaviorWeight.ForFavorite();
        
        return new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
    }
    
    /// <summary>
    /// 记录分享行为
    /// </summary>
    public static UserBehavior CreateShare(
        Guid userId,
        Guid videoId,
        Guid? categoryId,
        string? sharePlatform = null)
    {
        var type = BehaviorType.Share;
        var weight = BehaviorWeight.ForShare();
        
        return new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
    }
    
    /// <summary>
    /// 记录投币行为
    /// </summary>
    public static UserBehavior CreateCoin(
        Guid userId,
        Guid videoId,
        Guid? categoryId,
        int coinAmount)
    {
        var type = BehaviorType.Coin;
        var weight = BehaviorWeight.CalculateForCoin(coinAmount);
        
        var behavior = new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
        
        behavior.CoinAmount = coinAmount;
        
        return behavior;
    }
    
    /// <summary>
    /// 记录评论行为
    /// </summary>
    public static UserBehavior CreateComment(
        Guid userId,
        Guid videoId,
        Guid? categoryId,
        string comment)
    {
        var type = BehaviorType.Comment;
        var weight = BehaviorWeight.ForComment();
        
        var behavior = new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
        
        behavior.Comment = comment;
        
        return behavior;
    }
    
    /// <summary>
    /// 记录取消点赞行为
    /// </summary>
    public static UserBehavior CreateUnlike(
        Guid userId,
        Guid videoId,
        Guid? categoryId)
    {
        var type = BehaviorType.Unlike;
        var weight = BehaviorWeight.ForUnlike();
        
        return new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
    }
    
    /// <summary>
    /// 记录取消收藏行为
    /// </summary>
    public static UserBehavior CreateUnfavorite(
        Guid userId,
        Guid videoId,
        Guid? categoryId)
    {
        var type = BehaviorType.Unfavorite;
        var weight = BehaviorWeight.ForUnfavorite();
        
        return new UserBehavior(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            type,
            weight);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 更新观看进度（观看行为）
    /// </summary>
    public void UpdateWatchProgress(double progress, double duration)
    {
        if (_type != BehaviorType.Watch)
            throw new BusinessException("只能更新观看行为的进度");
        
        WatchProgress = progress;
        WatchDuration = duration;
        UpdatedAt = DateTime.UtcNow;
        
        // 根据观看进度重新计算权重
        _weight = BehaviorWeight.CalculateForWatch(progress);
    }
    
    // ========== 查询方法 ==========
    
    public BehaviorType GetBehaviorType() => _type;
    public BehaviorWeight GetBehaviorWeight() => _weight;
    public double GetWeightValue() => _weight.Value;
    
    /// <summary>
    /// 是否正面行为（用于推荐）
    /// </summary>
    public bool IsPositiveBehavior()
    {
        return _type == BehaviorType.Watch ||
               _type == BehaviorType.Like ||
               _type == BehaviorType.Favorite ||
               _type == BehaviorType.Share ||
               _type == BehaviorType.Coin ||
               _type == BehaviorType.Comment;
    }
    
    /// <summary>
    /// 是否完成观看（进度>80%）
    /// </summary>
    public bool IsCompletedWatch()
    {
        return _type == BehaviorType.Watch && 
               WatchProgress.HasValue && 
               WatchProgress.Value > 0.8;
    }
}
```

---

## 4. 值对象设计

### 4.1 BehaviorType（行为类型值对象）

```csharp
/// <summary>
/// 行为类型值对象
/// </summary>
public class BehaviorType : ValueObject
{
    // ========== 类型常量 ==========
    public static readonly BehaviorType Watch = new("watch", "观看", 1);
    public static readonly BehaviorType Like = new("like", "点赞", 2);
    public static readonly BehaviorType Favorite = new("favorite", "收藏", 3);
    public static readonly BehaviorType Share = new("share", "分享", 4);
    public static readonly BehaviorType Coin = new("coin", "投币", 5);
    public static readonly BehaviorType Comment = new("comment", "评论", 6);
    public static readonly BehaviorType Unlike = new("unlike", "取消点赞", -1);
    public static readonly BehaviorType Unfavorite = new("unfavorite", "取消收藏", -2);
    
    // ========== 属性 ==========
    public string Code { get; }
    public string DisplayName { get; }
    public int SortOrder { get; }               // 排序顺序
    
    // ========== 构造函数 ==========
    private BehaviorType(string code, string displayName, int sortOrder)
    {
        Code = code;
        DisplayName = displayName;
        SortOrder = sortOrder;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否正向行为
    /// </summary>
    public bool IsPositive()
    {
        return this == Watch || this == Like || this == Favorite || 
               this == Share || this == Coin || this == Comment;
    }
    
    /// <summary>
    /// 是否负向行为
    /// </summary>
    public bool IsNegative()
    {
        return this == Unlike || this == Unfavorite;
    }
    
    /// <summary>
    /// 获取反向行为类型
    /// </summary>
    public BehaviorType? GetReverse()
    {
        return this switch
        {
            _ when this == Like => Unlike,
            _ when this == Favorite => Unfavorite,
            _ when this == Unlike => Like,
            _ when this == Unfavorite => Favorite,
            _ => null
        };
    }
    
    /// <summary>
    /// 是否高价值行为（对推荐贡献大）
    /// </summary>
    public bool IsHighValue()
    {
        return this == Coin || this == Favorite || this == Share;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
    
    // ========== 所有类型列表 ==========
    public static readonly IReadOnlyList<BehaviorType> AllTypes = new List<BehaviorType>
    {
        Watch, Like, Favorite, Share, Coin, Comment, Unlike, Unfavorite
    };
}
```

---

### 4.2 BehaviorWeight（行为权重值对象）

```csharp
/// <summary>
/// 行为权重值对象
/// 业务规则：不同行为对推荐算法贡献不同
/// </summary>
public class BehaviorWeight : ValueObject
{
    // ========== 属性 ==========
    public double Value { get; }                // 权重值（0-10）
    public WeightLevel Level { get; }           // 权重等级
    
    // ========== 权重等级常量 ==========
    public static readonly IReadOnlyDictionary<WeightLevel, double> WeightLevels = new Dictionary<WeightLevel, double>
    {
        { WeightLevel.Low, 1.0 },       // 低权重（观看<50%）
        { WeightLevel.Medium, 3.0 },    // 中权重（点赞、观看50-80%）
        { WeightLevel.High, 5.0 },      // 高权重（收藏、分享、观看>80%）
        { WeightLevel.VeryHigh, 10.0 }  // 极高权重（投币）
    };
    
    // ========== 构造函数 ==========
    private BehaviorWeight(double value, WeightLevel level)
    {
        Value = value;
        Level = level;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 计算观看行为权重（根据观看进度）
    /// </summary>
    public static BehaviorWeight CalculateForWatch(double watchProgress)
    {
        if (watchProgress < 0.3)
            return new BehaviorWeight(1.0, WeightLevel.Low);
        else if (watchProgress < 0.8)
            return new BehaviorWeight(3.0, WeightLevel.Medium);
        else
            return new BehaviorWeight(5.0, WeightLevel.High);
    }
    
    /// <summary>
    /// 点赞权重
    /// </summary>
    public static BehaviorWeight ForLike()
    {
        return new BehaviorWeight(3.0, WeightLevel.Medium);
    }
    
    /// <summary>
    /// 收藏权重
    /// </summary>
    public static BehaviorWeight ForFavorite()
    {
        return new BehaviorWeight(5.0, WeightLevel.High);
    }
    
    /// <summary>
    /// 分享权重
    /// </summary>
    public static BehaviorWeight ForShare()
    {
        return new BehaviorWeight(5.0, WeightLevel.High);
    }
    
    /// <summary>
    /// 投币权重（根据投币数量）
    /// </summary>
    public static BehaviorWeight CalculateForCoin(int coinAmount)
    {
        // 投币权重：基础10分，每多投1币+5分
        var weight = 10.0 + Math.Min(coinAmount - 1, 2) * 5.0;
        return new BehaviorWeight(weight, WeightLevel.VeryHigh);
    }
    
    /// <summary>
    /// 评论权重
    /// </summary>
    public static BehaviorWeight ForComment()
    {
        return new BehaviorWeight(4.0, WeightLevel.Medium);
    }
    
    /// <summary>
    /// 取消点赞权重（负向）
    /// </summary>
    public static BehaviorWeight ForUnlike()
    {
        return new BehaviorWeight(-3.0, WeightLevel.Low);
    }
    
    /// <summary>
    /// 取消收藏权重（负向）
    /// </summary>
    public static BehaviorWeight ForUnfavorite()
    {
        return new BehaviorWeight(-5.0, WeightLevel.Low);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否正向权重
    /// </summary>
    public bool IsPositive()
    {
        return Value > 0;
    }
    
    /// <summary>
    /// 是否高权重
    /// </summary>
    public bool IsHighWeight()
    {
        return Level == WeightLevel.High || Level == WeightLevel.VeryHigh;
    }
    
    /// <summary>
    /// 合并权重（累加）
    /// </summary>
    public BehaviorWeight Merge(BehaviorWeight other)
    {
        var newValue = Value + other.Value;
        var newLevel = newValue >= 10 ? WeightLevel.VeryHigh :
                       newValue >= 5 ? WeightLevel.High :
                       newValue >= 3 ? WeightLevel.Medium :
                       WeightLevel.Low;
        
        return new BehaviorWeight(newValue, newLevel);
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
        yield return Level;
    }
}

/// <summary>
/// 权重等级枚举
/// </summary>
public enum WeightLevel
{
    Low = 1,        // 低权重
    Medium = 2,     // 中权重
    High = 3,       // 高权重
    VeryHigh = 4    // 极高权重
}
```

---

### 4.3 RecommendScore（推荐得分值对象）

```csharp
/// <summary>
/// 推荐得分值对象
/// </summary>
public class RecommendScore : ValueObject
{
    // ========== 属性 ==========
    public double Value { get; }                // 推荐得分（0-1）
    public double CollaborativeScore { get; }   // 协同过滤得分
    public double HotScore { get; }             // 热门得分
    public double PersonalScore { get; }        // 个性化得分
    public double CategoryScore { get; }        // 分类匹配得分
    
    // ========== 权重配置 ==========
    private static readonly double CollaborativeWeight = 0.4; // 协同过滤权重
    private static readonly double HotWeight = 0.2;           // 热门权重
    private static readonly double PersonalWeight = 0.3;      // 个性化权重
    private static readonly double CategoryWeight = 0.1;      // 分类权重
    
    // ========== 构造函数 ==========
    private RecommendScore(
        double collaborativeScore,
        double hotScore,
        double personalScore,
        double categoryScore)
    {
        CollaborativeScore = collaborativeScore;
        HotScore = hotScore;
        PersonalScore = personalScore;
        CategoryScore = categoryScore;
        
        // 综合得分计算
        Value = CollaborativeScore * CollaborativeWeight +
                HotScore * HotWeight +
                PersonalScore * PersonalWeight +
                CategoryScore * CategoryWeight;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建推荐得分
    /// </summary>
    public static RecommendScore Create(
        double collaborativeScore,
        double hotScore,
        double personalScore,
        double categoryScore)
    {
        // 验证得分范围（0-1）
        collaborativeScore = Math.Clamp(collaborativeScore, 0, 1);
        hotScore = Math.Clamp(hotScore, 0, 1);
        personalScore = Math.Clamp(personalScore, 0, 1);
        categoryScore = Math.Clamp(categoryScore, 0, 1);
        
        return new RecommendScore(collaborativeScore, hotScore, personalScore, categoryScore);
    }
    
    /// <summary>
    /// 创建仅热门得分（无用户数据时）
    /// </summary>
    public static RecommendScore CreateHotOnly(double hotScore)
    {
        return new RecommendScore(0, hotScore, 0, 0);
    }
    
    /// <summary>
    /// 创建最低得分
    /// </summary>
    public static RecommendScore Min()
    {
        return new RecommendScore(0, 0, 0, 0);
    }
    
    /// <summary>
    /// 创建最高得分
    /// </summary>
    public static RecommendScore Max()
    {
        return new RecommendScore(1, 1, 1, 1);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否高推荐得分
    /// </summary>
    public bool IsHighRecommendation()
    {
        return Value > 0.7;
    }
    
    /// <summary>
    /// 是否低推荐得分
    /// </summary>
    public bool IsLowRecommendation()
    {
        return Value < 0.3;
    }
    
    /// <summary>
    /// 主要推荐来源
    /// </summary>
    public RecommendSource GetPrimarySource()
    {
        var maxComponent = Math.Max(
            CollaborativeScore * CollaborativeWeight,
            Math.Max(
                HotScore * HotWeight,
                Math.Max(
                    PersonalScore * PersonalWeight,
                    CategoryScore * CategoryWeight)));
        
        if (maxComponent == CollaborativeScore * CollaborativeWeight)
            return RecommendSource.Collaborative;
        if (maxComponent == HotScore * HotWeight)
            return RecommendSource.Hot;
        if (maxComponent == PersonalScore * PersonalWeight)
            return RecommendSource.Personal;
        return RecommendSource.Category;
    }
    
    /// <summary>
    /// 格式化得分显示
    /// </summary>
    public string FormatValue()
    {
        return $"{Value:P0}"; // 百分比显示
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

/// <summary>
/// 推荐来源枚举
/// </summary>
public enum RecommendSource
{
    Collaborative = 1,  // 协同过滤
    Hot = 2,            // 热门推荐
    Personal = 3,       // 个性化
    Category = 4        // 分类匹配
}
```

---

### 4.4 RecommendReason（推荐理由值对象）

```csharp
/// <summary>
/// 推荐理由值对象
/// </summary>
public class RecommendReason : ValueObject
{
    // ========== 属性 ==========
    public string Text { get; }                 // 推荐理由文本
    public RecommendSource Source { get; }      // 推荐来源
    public IReadOnlyList<string>? Tags { get; } // 推荐标签
    
    // ========== 预定义理由模板 ==========
    private static readonly IReadOnlyDictionary<RecommendSource, string> ReasonTemplates = new Dictionary<RecommendSource, string>
    {
        { RecommendSource.Collaborative, "和你兴趣相似的用户喜欢" },
        { RecommendSource.Hot, "热门视频" },
        { RecommendSource.Personal, "根据你的观看历史推荐" },
        { RecommendSource.Category, "你关注的分类" }
    };
    
    // ========== 构造函数 ==========
    private RecommendReason(string text, RecommendSource source, IReadOnlyList<string>? tags = null)
    {
        Text = text;
        Source = source;
        Tags = tags;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建推荐理由
    /// </summary>
    public static RecommendReason Create(RecommendSource source, IReadOnlyList<string>? tags = null)
    {
        var template = ReasonTemplates[source];
        return new RecommendReason(template, source, tags);
    }
    
    /// <summary>
    /// 创建协同过滤理由
    /// </summary>
    public static RecommendReason ForCollaborative(int similarUserCount)
    {
        var text = $"有{similarUserCount}位和你兴趣相似的用户喜欢";
        return new RecommendReason(text, RecommendSource.Collaborative);
    }
    
    /// <summary>
    /// 创建热门理由
    /// </summary>
    public static RecommendReason ForHot(long viewCount)
    {
        var text = $"热门视频，{viewCount}人正在观看";
        return new RecommendReason(text, RecommendSource.Hot);
    }
    
    /// <summary>
    /// 创建个性化理由
    /// </summary>
    public static RecommendReason ForPersonal(string videoTitle)
    {
        var text = $"根据你对「{videoTitle}」的兴趣推荐";
        return new RecommendReason(text, RecommendSource.Personal);
    }
    
    /// <summary>
    /// 创建分类理由
    /// </summary>
    public static RecommendReason ForCategory(string categoryName)
    {
        var text = $"来自你关注的「{categoryName}」分类";
        return new RecommendReason(text, RecommendSource.Category, new List<string> { categoryName });
    }
    
    /// <summary>
    /// 创建自定义理由
    /// </summary>
    public static RecommendReason Custom(string text, RecommendSource source)
    {
        return new RecommendReason(text, source);
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Text;
        yield return Source;
    }
}
```

---

## 5. 聚合根设计

### 5.1 Recommendation（推荐聚合根）

```csharp
/// <summary>
/// 推荐聚合根
/// </summary>
public class Recommendation : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 推荐目标用户ID
    public Guid VideoId { get; private set; }    // 推荐视频ID
    public Guid? CategoryId { get; private set; } // 视频分类ID
    
    // ========== 值对象 ==========
    private RecommendScore _score;               // 推荐得分
    private RecommendReason _reason;             // 推荐理由
    
    // ========== 属性 ==========
    public int Position { get; private set; }    // 推荐位置（排序）
    public RecommendStatus Status { get; private set; } // 推荐状态
    
    // ========== 时间戳 ==========
    public DateTime GeneratedAt { get; private set; }    // 生成时间
    public DateTime? DisplayedAt { get; private set; }   // 展示时间
    public DateTime? ClickedAt { get; private set; }     // 点击时间
    
    // ========== 构造函数 ==========
    private Recommendation() { }
    
    private Recommendation(
        Guid id,
        Guid userId,
        Guid videoId,
        Guid? categoryId,
        RecommendScore score,
        RecommendReason reason,
        int position) : base(id)
    {
        UserId = userId;
        VideoId = videoId;
        CategoryId = categoryId;
        _score = score;
        _reason = reason;
        Position = position;
        Status = RecommendStatus.Generated;
        GeneratedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建推荐
    /// </summary>
    public static Recommendation Create(
        Guid userId,
        Guid videoId,
        Guid? categoryId,
        RecommendScore score,
        RecommendReason reason,
        int position)
    {
        return new Recommendation(
            GuidGenerator.Create(),
            userId,
            videoId,
            categoryId,
            score,
            reason,
            position);
    }
    
    /// <summary>
    /// 批量创建推荐列表
    /// </summary>
    public static List<Recommendation> CreateBatch(
        Guid userId,
        IReadOnlyList<(Guid videoId, Guid? categoryId, RecommendScore score, RecommendReason reason)> items)
    {
        var recommendations = new List<Recommendation>();
        
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            recommendations.Add(Create(
                userId,
                item.videoId,
                item.categoryId,
                item.score,
                item.reason,
                i + 1));
        }
        
        return recommendations;
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 标记已展示
    /// </summary>
    public void MarkDisplayed()
    {
        Status = RecommendStatus.Displayed;
        DisplayedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 标记已点击
    /// </summary>
    public void MarkClicked()
    {
        Status = RecommendStatus.Clicked;
        ClickedAt = DateTime.UtcNow;
        
        AddDomainEvent(new RecommendationClickedEvent(Id, UserId, VideoId));
    }
    
    /// <summary>
    /// 标记已忽略
    /// </summary>
    public void MarkIgnored()
    {
        Status = RecommendStatus.Ignored;
    }
    
    /// <summary>
    /// 更新推荐得分
    /// </summary>
    public void UpdateScore(RecommendScore newScore)
    {
        _score = newScore;
    }
    
    /// <summary>
    /// 更新推荐理由
    /// </summary>
    public void UpdateReason(RecommendReason newReason)
    {
        _reason = newReason;
    }
    
    /// <summary>
    /// 更新推荐位置
    /// </summary>
    public void UpdatePosition(int newPosition)
    {
        Position = newPosition;
    }
    
    // ========== 查询方法 ==========
    
    public RecommendScore GetScore() => _score;
    public RecommendReason GetReason() => _reason;
    public double GetScoreValue() => _score.Value;
    public string GetReasonText() => _reason.Text;
    
    /// <summary>
    /// 是否已点击
    /// </summary>
    public bool IsClicked => Status == RecommendStatus.Clicked;
    
    /// <summary>
    /// 是否已展示
    /// </summary>
    public bool IsDisplayed => Status == RecommendStatus.Displayed ||
                               Status == RecommendStatus.Clicked ||
                               Status == RecommendStatus.Ignored;
}

/// <summary>
/// 推荐状态枚举
/// </summary>
public enum RecommendStatus
{
    Generated = 1,      // 已生成
    Displayed = 2,      // 已展示
    Clicked = 3,        // 已点击
    Ignored = 4         // 已忽略
}
```

---

## 6. 聚合根设计：UserInterest（用户兴趣聚合）

### 6.1 UserInterest（用户兴趣聚合根）

```csharp
/// <summary>
/// 用户兴趣聚合根（用户画像）
/// </summary>
public class UserInterest : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 用户ID
    
    // ========== 属性 ==========
    private readonly Dictionary<Guid, double> _categoryPreferences; // 分类偏好
    private readonly Dictionary<string, double> _tagPreferences;    // 标签偏好
    private readonly Dictionary<Guid, double> _uploaderPreferences; // UP主偏好
    
    public int TotalWatchCount { get; private set; }     // 总观看数
    public int TotalLikeCount { get; private set; }      // 总点赞数
    public int TotalFavoriteCount { get; private set; }  // 总收藏数
    public double AverageWatchProgress { get; private set; } // 平均观看进度
    
    public DateTime UpdatedAt { get; private set; }      // 更新时间
    
    // ========== 构造函数 ==========
    private UserInterest() 
    {
        _categoryPreferences = new Dictionary<Guid, double>();
        _tagPreferences = new Dictionary<string, double>();
        _uploaderPreferences = new Dictionary<Guid, double>();
    }
    
    private UserInterest(Guid userId) : base(GuidGenerator.Create())
    {
        UserId = userId;
        _categoryPreferences = new Dictionary<Guid, double>();
        _tagPreferences = new Dictionary<string, double>();
        _uploaderPreferences = new Dictionary<Guid, double>();
        TotalWatchCount = 0;
        TotalLikeCount = 0;
        TotalFavoriteCount = 0;
        AverageWatchProgress = 0;
        UpdatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    public static UserInterest Create(Guid userId)
    {
        return new UserInterest(userId);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 根据行为更新兴趣
    /// </summary>
    public void UpdateFromBehavior(UserBehavior behavior)
    {
        // 更新分类偏好
        if (behavior.CategoryId.HasValue)
        {
            var categoryId = behavior.CategoryId.Value;
            var weight = behavior.GetWeightValue();
            
            if (!_categoryPreferences.ContainsKey(categoryId))
                _categoryPreferences[categoryId] = 0;
            
            _categoryPreferences[categoryId] += weight;
        }
        
        // 更新统计
        UpdateStatistics(behavior);
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserInterestUpdatedEvent(UserId, behavior.GetBehaviorType()));
    }
    
    /// <summary>
    /// 更新统计
    /// </summary>
    private void UpdateStatistics(UserBehavior behavior)
    {
        switch (behavior.GetBehaviorType())
        {
            case BehaviorType b when b == BehaviorType.Watch:
                TotalWatchCount++;
                if (behavior.WatchProgress.HasValue)
                {
                    AverageWatchProgress = (AverageWatchProgress * (TotalWatchCount - 1) + behavior.WatchProgress.Value) / TotalWatchCount;
                }
                break;
            case BehaviorType b when b == BehaviorType.Like:
                TotalLikeCount++;
                break;
            case BehaviorType b when b == BehaviorType.Favorite:
                TotalFavoriteCount++;
                break;
            case BehaviorType b when b == BehaviorType.Unlike:
                TotalLikeCount = Math.Max(0, TotalLikeCount - 1);
                break;
            case BehaviorType b when b == BehaviorType.Unfavorite:
                TotalFavoriteCount = Math.Max(0, TotalFavoriteCount - 1);
                break;
        }
    }
    
    /// <summary>
    /// 获取分类偏好得分
    /// </summary>
    public double GetCategoryPreference(Guid categoryId)
    {
        return _categoryPreferences.TryGetValue(categoryId, out var score) ? score : 0;
    }
    
    /// <summary>
    /// 获取偏好分类列表
    /// </summary>
    public IReadOnlyList<(Guid CategoryId, double Score)> GetPreferredCategories(int topN = 10)
    {
        return _categoryPreferences
            .OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }
    
    /// <summary>
    /// 获取UP主偏好列表
    /// </summary>
    public IReadOnlyList<(Guid UploaderId, double Score)> GetPreferredUploaders(int topN = 10)
    {
        return _uploaderPreferences
            .OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }
    
    /// <summary>
    /// 清除过期数据（定期清理）
    /// </summary>
    public void DecayPreferences(double decayFactor = 0.9)
    {
        // 衰减所有偏好得分
        foreach (var key in _categoryPreferences.Keys.ToList())
        {
            _categoryPreferences[key] *= decayFactor;
            if (_categoryPreferences[key] < 0.1)
                _categoryPreferences.Remove(key);
        }
        
        foreach (var key in _tagPreferences.Keys.ToList())
        {
            _tagPreferences[key] *= decayFactor;
            if (_tagPreferences[key] < 0.1)
                _tagPreferences.Remove(key);
        }
        
        foreach (var key in _uploaderPreferences.Keys.ToList())
        {
            _uploaderPreferences[key] *= decayFactor;
            if (_uploaderPreferences[key] < 0.1)
                _uploaderPreferences.Remove(key);
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
}
```

---

## 7. 实体设计

### 7.1 SimilarUser（相似用户实体）

```csharp
/// <summary>
/// 相似用户实体
/// 用于协同过滤算法
/// </summary>
public class SimilarUser : Entity<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }         // 目标用户ID
    public Guid SimilarUserId { get; private set; }  // 相似用户ID
    
    // ========== 属性 ==========
    public double SimilarityScore { get; private set; } // 相似度得分（0-1）
    public SimilarityMethod Method { get; private set; } // 相似度计算方法
    public int CommonVideoCount { get; private set; }    // 共同观看视频数
    public IReadOnlyList<Guid> CommonVideos { get; private set; } // 共同视频列表
    
    public DateTime CalculatedAt { get; private set; }  // 计算时间
    
    // ========== 构造函数 ==========
    private SimilarUser() { }
    
    private SimilarUser(
        Guid userId,
        Guid similarUserId,
        double similarityScore,
        SimilarityMethod method,
        int commonVideoCount,
        IReadOnlyList<Guid> commonVideos) : base(GuidGenerator.Create())
    {
        UserId = userId;
        SimilarUserId = similarUserId;
        SimilarityScore = similarityScore;
        Method = method;
        CommonVideoCount = commonVideoCount;
        CommonVideos = commonVideos;
        CalculatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建相似用户（Jaccard相似度）
    /// </summary>
    public static SimilarUser CreateByJaccard(
        Guid userId,
        Guid similarUserId,
        IReadOnlyList<Guid> userVideos,
        IReadOnlyList<Guid> similarUserVideos)
    {
        // 计算Jaccard相似度：交集/并集
        var intersection = userVideos.Intersect(similarUserVideos).ToList();
        var union = userVideos.Union(similarUserVideos).ToList();
        
        var similarityScore = union.Count > 0 ? 
            (double)intersection.Count / union.Count : 0;
        
        return new SimilarUser(
            userId,
            similarUserId,
            similarityScore,
            SimilarityMethod.Jaccard,
            intersection.Count,
            intersection);
    }
    
    /// <summary>
    /// 创建相似用户（余弦相似度）
    /// </summary>
    public static SimilarUser CreateByCosine(
        Guid userId,
        Guid similarUserId,
        IReadOnlyDictionary<Guid, double> userWeights,
        IReadOnlyDictionary<Guid, double> similarUserWeights)
    {
        // 计算余弦相似度
        var commonVideos = userWeights.Keys.Intersect(similarUserWeights.Keys).ToList();
        
        double dotProduct = 0;
        double userNorm = 0;
        double similarNorm = 0;
        
        foreach (var videoId in commonVideos)
        {
            var userWeight = userWeights[videoId];
            var similarWeight = similarUserWeights[videoId];
            dotProduct += userWeight * similarWeight;
        }
        
        foreach (var weight in userWeights.Values)
            userNorm += weight * weight;
        foreach (var weight in similarUserWeights.Values)
            similarNorm += weight * weight;
        
        userNorm = Math.Sqrt(userNorm);
        similarNorm = Math.Sqrt(similarNorm);
        
        var similarityScore = (userNorm > 0 && similarNorm > 0) ?
            dotProduct / (userNorm * similarNorm) : 0;
        
        return new SimilarUser(
            userId,
            similarUserId,
            similarityScore,
            SimilarityMethod.Cosine,
            commonVideos.Count,
            commonVideos);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否高相似度
    /// </summary>
    public bool IsHighSimilarity()
    {
        return SimilarityScore > 0.7;
    }
    
    /// <summary>
    /// 是否有足够共同视频
    /// </summary>
    public bool HasEnoughCommonVideos()
    {
        return CommonVideoCount >= 5;
    }
}

/// <summary>
/// 相似度计算方法枚举
/// </summary>
public enum SimilarityMethod
{
    Jaccard = 1,    // Jaccard相似度
    Cosine = 2      // 余弦相似度
}
```

---

## 8. 领域服务设计

### 8.1 ICollaborativeFilteringService（协同过滤领域服务）

```csharp
/// <summary>
/// 协同过滤领域服务接口
/// 业务规则：协同过滤+热门（用户需求）
/// </summary>
public interface ICollaborativeFilteringService
{
    /// <summary>
    /// 查找相似用户
    /// </summary>
    Task<IReadOnlyList<SimilarUser>> FindSimilarUsersAsync(Guid userId, int topN = 50);
    
    /// <summary>
    /// 基于用户的协同过滤推荐
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double Score)>> UserBasedRecommendAsync(
        Guid userId,
        IReadOnlyList<SimilarUser> similarUsers,
        int topN = 100);
    
    /// <summary>
    /// 基于物品的协同过滤推荐
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double Score)>> ItemBasedRecommendAsync(
        Guid userId,
        int topN = 100);
    
    /// <summary>
    /// 计算用户相似度矩阵
    /// </summary>
    Task UpdateUserSimilarityMatrixAsync();
    
    /// <summary>
    /// 计算视频相似度矩阵
    /// </summary>
    Task UpdateVideoSimilarityMatrixAsync();
}
```

---

### 8.2 IHotRecommendationService（热门推荐领域服务）

```csharp
/// <summary>
/// 热门推荐领域服务接口
/// </summary>
public interface IHotRecommendationService
{
    /// <summary>
    /// 获取热门视频列表
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double HotScore)>> GetHotVideosAsync(
        int topN = 100,
        HotCategory? category = null);
    
    /// <summary>
    /// 获取分类热门视频
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double HotScore)>> GetCategoryHotVideosAsync(
        Guid categoryId,
        int topN = 50);
    
    /// <summary>
    /// 获取最新热门视频（24小时内）
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double HotScore)>> GetRecentHotVideosAsync(
        int topN = 50);
    
    /// <summary>
    /// 计算视频热度得分
    /// </summary>
    Task<double> CalculateHotScoreAsync(Guid videoId);
    
    /// <summary>
    /// 更新热度排行榜
    /// </summary>
    Task UpdateHotRankingAsync();
}

/// <summary>
/// 热门分类枚举
/// </summary>
public enum HotCategory
{
    All = 1,            // 全站热门
    Weekly = 2,         // 周榜
    Daily = 3,          // 日榜
    Rising = 4          // 上升最快
}
```

---

### 8.3 IPersonalRecommendationService（个性化推荐领域服务）

```csharp
/// <summary>
/// 个性化推荐领域服务接口
/// </summary>
public interface IPersonalRecommendationService
{
    /// <summary>
    /// 根据用户画像推荐
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double Score)>> PersonalRecommendAsync(
        Guid userId,
        UserInterest userInterest,
        int topN = 100);
    
    /// <summary>
    /// 根据观看历史推荐相似视频
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double Score)>> RecommendSimilarToHistoryAsync(
        Guid userId,
        IReadOnlyList<Guid> watchedVideos,
        int topN = 50);
    
    /// <summary>
    /// 根据偏好分类推荐
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double Score)>> RecommendByPreferredCategoriesAsync(
        Guid userId,
        IReadOnlyList<Guid> preferredCategories,
        int topN = 50);
    
    /// <summary>
    /// 根据偏好UP主推荐
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double Score)>> RecommendByPreferredUploadersAsync(
        Guid userId,
        IReadOnlyList<Guid> preferredUploaders,
        int topN = 50);
    
    /// <summary>
    /// 更新用户画像
    /// </summary>
    Task UpdateUserInterestAsync(Guid userId);
}
```

---

### 8.4 IRecommendationEngineService（推荐引擎领域服务）

```csharp
/// <summary>
/// 推荐引擎领域服务接口（综合推荐）
/// </summary>
public interface IRecommendationEngineService
{
    /// <summary>
    /// 生成首页推荐feed
    /// </summary>
    Task<List<Recommendation>> GenerateFeedAsync(
        Guid userId,
        int count = 20,
        int page = 1);
    
    /// <summary>
    /// 过滤已观看视频
    /// </summary>
    Task<IReadOnlyList<(Guid VideoId, double Score)>> FilterWatchedVideosAsync(
        Guid userId,
        IReadOnlyList<(Guid VideoId, double Score)> candidates);
    
    /// <summary>
    /// 多样化推荐（避免同类型过多）
    /// </summary>
    Task<List<Recommendation>> DiversifyRecommendationsAsync(
        List<Recommendation> recommendations,
        int diversityFactor = 20);
    
    /// <summary>
    /// 记录推荐反馈（点击/忽略）
    /// </summary>
    Task RecordFeedbackAsync(Guid recommendationId, bool clicked);
    
    /// <summary>
    /// 实时同步到Elasticsearch
    /// </summary>
    Task SyncToElasticsearchAsync(Guid userId);
}
```

---

## 9. 仓储接口设计

### 9.1 IUserBehaviorRepository

```csharp
/// <summary>
/// 用户行为仓储接口
/// </summary>
public interface IUserBehaviorRepository : IRepository<UserBehavior, Guid>
{
    /// <summary>
    /// 获取用户最近行为
    /// </summary>
    Task<List<UserBehavior>> GetRecentAsync(Guid userId, int days = 30);
    
    /// <summary>
    /// 获取用户指定类型行为
    /// </summary>
    Task<List<UserBehavior>> GetByTypeAsync(Guid userId, BehaviorType type);
    
    /// <summary>
    /// 获取用户对视频的行为
    /// </summary>
    Task<UserBehavior?> GetByUserAndVideoAsync(Guid userId, Guid videoId, BehaviorType type);
    
    /// <summary>
    /// 获取看过指定视频的用户列表
    /// </summary>
    Task<List<Guid>> GetUsersByVideoAsync(Guid videoId);
    
    /// <summary>
    /// 获取用户观看的视频列表
    /// </summary>
    Task<List<Guid>> GetUserVideosAsync(Guid userId, BehaviorType? type = null);
    
    /// <summary>
    /// 批量插入行为
    /// </summary>
    Task<int> BulkInsertAsync(IReadOnlyList<UserBehavior> behaviors);
    
    /// <summary>
    /// 获取用户行为统计
    /// </summary>
    Task<UserBehaviorStatistics> GetStatisticsAsync(Guid userId);
}

/// <summary>
/// 用户行为统计
/// </summary>
public class UserBehaviorStatistics
{
    public int WatchCount { get; set; }
    public int LikeCount { get; set; }
    public int FavoriteCount { get; set; }
    public int ShareCount { get; set; }
    public int CoinCount { get; set; }
    public int CommentCount { get; set; }
    public double TotalWeight { get; set; }
}
```

---

### 9.2 IRecommendationRepository

```csharp
/// <summary>
/// 推荐仓储接口
/// </summary>
public interface IRecommendationRepository : IRepository<Recommendation, Guid>
{
    /// <summary>
    /// 获取用户推荐列表
    /// </summary>
    Task<List<Recommendation>> GetUserRecommendationsAsync(Guid userId, int skip, int take);
    
    /// <summary>
    /// 获取未展示的推荐
    /// </summary>
    Task<List<Recommendation>> GetUndisplayedAsync(Guid userId, int limit);
    
    /// <summary>
    /// 获取已点击的推荐
    /// </summary>
    Task<List<Recommendation>> GetClickedAsync(Guid userId, int days = 7);
    
    /// <summary>
    /// 批量插入推荐
    /// </summary>
    Task<int> BulkInsertAsync(IReadOnlyList<Recommendation> recommendations);
    
    /// <summary>
    /// 清除过期推荐
    /// </summary>
    Task<int> ClearExpiredAsync(Guid userId, int days = 7);
    
    /// <summary>
    /// 获取推荐点击率
    /// </summary>
    Task<double> GetClickRateAsync(Guid userId);
}
```

---

### 9.3 IUserInterestRepository

```csharp
/// <summary>
/// 用户兴趣仓储接口
/// </summary>
public interface IUserInterestRepository : IRepository<UserInterest, Guid>
{
    /// <summary>
    /// 根据用户ID获取兴趣
    /// </summary>
    Task<UserInterest?> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// 批量获取用户兴趣
    /// </summary>
    Task<List<UserInterest>> GetByUserIdsAsync(IReadOnlyList<Guid> userIds);
    
    /// <summary>
    /// 获取所有需要更新的用户兴趣
    /// </summary>
    Task<List<UserInterest>> GetNeedUpdateAsync(int hours = 24);
}
```

---

### 9.4 ISimilarUserRepository

```csharp
/// <summary>
/// 相似用户仓储接口
/// </summary>
public interface ISimilarUserRepository : IRepository<SimilarUser, Guid>
{
    /// <summary>
    /// 获取用户的相似用户列表
    /// </summary>
    Task<List<SimilarUser>> GetByUserIdAsync(Guid userId, int topN = 50);
    
    /// <summary>
    /// 批量插入相似用户
    /// </summary>
    Task<int> BulkInsertAsync(IReadOnlyList<SimilarUser> similarUsers);
    
    /// <summary>
    /// 清除过期相似关系
    /// </summary>
    Task<int> ClearExpiredAsync(int days = 30);
}
```

---

## 10. 领域事件设计

### 10.1 频域事件列表

```csharp
/// <summary>
/// 用户行为记录事件
/// </summary>
public class UserBehaviorRecordedEvent
{
    public Guid BehaviorId { get; }
    public Guid UserId { get; }
    public Guid VideoId { get; }
    public BehaviorType Type { get; }
    public DateTime OccurredOn { get; }
    
    public UserBehaviorRecordedEvent(Guid behaviorId, Guid userId, Guid videoId, BehaviorType type)
    {
        BehaviorId = behaviorId;
        UserId = userId;
        VideoId = videoId;
        Type = type;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 用户兴趣更新事件
/// </summary>
public class UserInterestUpdatedEvent
{
    public Guid UserId { get; }
    public BehaviorType TriggerBehavior { get; }
    public DateTime OccurredOn { get; }
    
    public UserInterestUpdatedEvent(Guid userId, BehaviorType triggerBehavior)
    {
        UserId = userId;
        TriggerBehavior = triggerBehavior;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 推荐点击事件
/// </summary>
public class RecommendationClickedEvent
{
    public Guid RecommendationId { get; }
    public Guid UserId { get; }
    public Guid VideoId { get; }
    public DateTime OccurredOn { get; }
    
    public RecommendationClickedEvent(Guid recommendationId, Guid userId, Guid videoId)
    {
        RecommendationId = recommendationId;
        UserId = userId;
        VideoId = videoId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 推荐生成事件
/// </summary>
public class RecommendationsGeneratedEvent
{
    public Guid UserId { get; }
    public int Count { get; }
    public DateTime OccurredOn { get; }
    
    public RecommendationsGeneratedEvent(Guid userId, int count)
    {
        UserId = userId;
        Count = count;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 热度排行榜更新事件
/// </summary>
public class HotRankingUpdatedEvent
{
    public DateTime OccurredOn { get; }
    
    public HotRankingUpdatedEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 相似用户矩阵更新事件
/// </summary>
public class SimilarityMatrixUpdatedEvent
{
    public DateTime OccurredOn { get; }
    
    public SimilarityMatrixUpdatedEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }
}
```

---

## 11. 业务规则总结

### 11.1 推荐算法规则

| 规则 | 说明 |
|------|------|
| 推荐算法 | 协同过滤+热门（用户需求） |
| 协同过滤 | 基于用户+基于物品 |
| 相似度计算 | Jaccard/余弦相似度 |
| 热门计算 | 播放量+互动权重 |
| 索引同步 | 实时同步Elasticsearch（用户需求） |

---

### 11.2 行为权重规则

| 行为类型 | 权重值 | 说明 |
|---------|-------|------|
| 观看<30% | 1.0 | 低权重 |
| 观看30-80% | 3.0 | 中权重 |
| 观看>80% | 5.0 | 高权重 |
| 点赞 | 3.0 | 中权重 |
| 收藏 | 5.0 | 高权重 |
| 分享 | 5.0 | 高权重 |
| 投币(基础) | 10.0 | 极高权重 |
| 投币(+1币) | +5.0 | 每币加5分 |
| 评论 | 4.0 | 中权重 |

---

### 11.3 推荐得分规则

| 规则 | 说明 |
|------|------|
| 协同过滤权重 | 40% |
| 热门权重 | 20% |
| 个性化权重 | 30% |
| 分类权重 | 10% |
| 高推荐阈值 | >0.7 |
| 低推荐阈值 | <0.3 |

---

### 11.4 多样化规则

| 规则 | 说明 |
|------|------|
| 同类型限制 | 单类型不超过20% |
| 同UP主限制 | 单UP主不超过15% |
| 已观看过滤 | 过滤已观看视频 |
| 新用户策略 | 热门推荐为主 |
| 衰减因子 | 历史偏好每日衰减0.9 |

---

### 11.5 实时同步规则

| 规则 | 说明 |
|------|------|
| 同步方式 | 实时同步到Elasticsearch（用户需求） |
| 同步时机 | 行为发生后立即同步 |
| 用户画像更新 | 行为触发立即更新 |
| 热度更新 | 每小时更新排行榜 |
| 相似矩阵更新 | 每天更新 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 推荐模块领域设计文档完成