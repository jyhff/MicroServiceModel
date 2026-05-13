# 用户模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | UserService（用户服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 领域模型概览

### 2.1 领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    用户领域模型                              │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐         ┌──────────────┐                │
│  │  BilibiliUser│◄────────│ UserLevel    │                │
│  │ (聚合根)     │         │ (值对象)     │                │
│  └──────────────┘         └──────────────┘                │
│         │                                                  │
│         │                                                  │
│         ├──────────► ┌──────────────┐                     │
│         │            │ UserCoins     │                     │
│         │            │ (值对象)      │                     │
│         │            └──────────────┘                     │
│         │                                                  │
│         ├──────────► ┌──────────────┐                     │
│         │            │ UserProfile   │                     │
│         │            │ (值对象)      │                     │
│         │            └──────────────┘                     │
│         │                                                  │
│         ├──────────► ┌──────────────┐                     │
│         │            │ Verification  │                     │
│         │            │ (值对象)      │                     │
│         │            └──────────────┘                     │
│                                                             │
│  ┌──────────────┐                                          │
│  │UserExperience│                                          │
│  │Record        │                                          │
│  │ (实体)       │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │UserCoinRecord│                                          │
│  │ (实体)       │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ UserFollow   │                                          │
│  │ (实体)       │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ UserMessage  │                                          │
│  │ (实体)       │                                          │
│  └──────────────┘                                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 BilibiliUser（用户聚合根）

#### 实体定义

```csharp
/// <summary>
/// 用户聚合根（继承ABP FullAuditedAggregateRoot）
/// 聚合边界：用户及其相关的等级、B币、认证信息
/// </summary>
public class BilibiliUser : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    // ========== 核心标识 ==========
    public Guid? TenantId { get; set; }
    
    // ========== 值对象 ==========
    private UserLevel _level;              // 用户等级值对象
    private UserCoins _coins;              // B币值对象
    private UserProfile _profile;          // 用户信息值对象
    private Verification _verification;    // 认证信息值对象
    
    // ========== 实体集合 ==========
    private List<UserExperienceRecord> _experienceRecords;  // 经验记录
    private List<UserCoinRecord> _coinRecords;              // B币记录
    
    // ========== 导航属性 ==========
    public Guid UserId { get; private set; }  // 关联AbpUsers
    
    // ========== 构造函数 ==========
    public BilibiliUser(Guid id, Guid userId) : base(id)
    {
        UserId = userId;
        _level = UserLevel.NewUser();         // 初始化LV0
        _coins = UserCoins.Empty();            // 初始化B币0
        _profile = UserProfile.Default();      // 初始化默认信息
        _verification = Verification.None();   // 初始化未认证
        
        // 初始化集合
        _experienceRecords = new List<UserExperienceRecord>();
        _coinRecords = new List<UserCoinRecord>();
    }
    
    // ========== 业务方法（领域行为） ==========
    
    /// <summary>
    /// 增加经验值
    /// 业务规则：每日上限100经验
    /// </summary>
    public void AddExperience(int amount, ExperienceSource source, DateTime date)
    {
        // 业务规则验证
        if (amount <= 0)
            throw new BusinessException("经验值必须大于0");
        
        // 检查每日上限
        var dailyTotal = GetDailyExperience(date);
        if (dailyTotal + amount > 100)
            throw new BusinessException("今日经验已达上限（100）");
        
        // 增加经验值
        _level.AddExperience(amount);
        
        // 记录经验来源
        var record = new UserExperienceRecord(
            GuidGenerator.Create(),
            Id,
            source,
            amount,
            date
        );
        
        _experienceRecords.Add(record);
        
        // 检查升级
        if (_level.ShouldUpgrade())
        {
            _level.Upgrade();
            AddDomainEvent(new UserLevelUpEvent(Id, _level.CurrentLevel));
        }
    }
    
    /// <summary>
    /// 投币（消耗B币）
    /// 业务规则：无限制投币
    /// </summary>
    public void SpendCoins(int amount, Guid targetUserId, Guid videoId)
    {
        // 业务规则验证
        if (amount <= 0)
            throw new BusinessException("投币数量必须大于0");
        
        if (_coins.Balance < amount)
            throw new BusinessException("B币余额不足");
        
        // 消耗B币
        _coins.Spend(amount);
        
        // 记录交易
        var record = new UserCoinRecord(
            GuidGenerator.Create(),
            Id,
            -amount,  // 消耗为负数
            CoinTransactionType.Spend,
            targetUserId,
            videoId
        );
        
        _coinRecords.Add(record);
        
        AddDomainEvent(new UserCoinSpentEvent(Id, amount, targetUserId, videoId));
    }
    
    /// <summary>
    /// 收到投币（获得B币）
    /// </summary>
    public void ReceiveCoins(int amount, Guid senderId, Guid videoId)
    {
        if (amount <= 0)
            throw new BusinessException("收到B币数量必须大于0");
        
        _coins.Receive(amount);
        
        var record = new UserCoinRecord(
            GuidGenerator.Create(),
            Id,
            amount,  // 获得为正数
            CoinTransactionType.Receive,
            senderId,
            videoId
        );
        
        _coinRecords.Add(record);
        
        AddDomainEvent(new UserCoinReceivedEvent(Id, amount, senderId, videoId));
    }
    
    /// <summary>
    /// 实名认证
    /// </summary>
    public void VerifyRealName(string realName, string idCardNumber)
    {
        // 业务规则：只能认证一次
        if (_verification.IsRealNameVerified)
            throw new BusinessException("已实名认证，不能重复认证");
        
        _verification.SetRealName(realName, idCardNumber);
        
        AddDomainEvent(new UserRealNameVerifiedEvent(Id));
    }
    
    /// <summary>
    /// 更新用户信息
    /// </summary>
    public void UpdateProfile(string nickName, string signature, int? gender)
    {
        _profile.Update(nickName, signature, gender);
    }
    
    // ========== 查询方法 ==========
    
    public int GetDailyExperience(DateTime date)
    {
        return _experienceRecords
            .Where(r => r.Date == date)
            .Sum(r => r.Amount);
    }
    
    public UserLevel GetLevel() => _level;
    public UserCoins GetCoins() => _coins;
    public UserProfile GetProfile() => _profile;
    public Verification GetVerification() => _verification;
}
```

---

## 4. 值对象设计

### 4.1 UserLevel（用户等级值对象）

```csharp
/// <summary>
/// 用户等级值对象
/// 业务规则：LV0-LV6，每个等级有固定经验区间
/// </summary>
public class UserLevel : ValueObject
{
    // ========== 属性 ==========
    public int CurrentLevel { get; private set; }      // 当前等级
    public int CurrentExperience { get; private set; }  // 当前经验值
    public int TotalExperience { get; private set; }    // 总经验值
    
    // ========== 领域常量（等级要求） ==========
    private static readonly Dictionary<int, int> LevelRequirements = new()
    {
        { 0, 0 },      // LV0: 0-100
        { 1, 101 },    // LV1: 101-200
        { 2, 201 },    // LV2: 201-400
        { 3, 401 },    // LV3: 401-600
        { 4, 601 },    // LV4: 601-800
        { 5, 801 },    // LV5: 801-1000
        { 6, 1001 }    // LV6: 1001+
    };
    
    // ========== 构造函数 ==========
    private UserLevel(int level, int experience)
    {
        CurrentLevel = level;
        CurrentExperience = experience;
        TotalExperience = experience;
    }
    
    // ========== 工厂方法 ==========
    public static UserLevel NewUser()
    {
        return new UserLevel(0, 0);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 增加经验值
    /// </summary>
    public void AddExperience(int amount)
    {
        CurrentExperience += amount;
        TotalExperience += amount;
    }
    
    /// <summary>
    /// 判断是否应该升级
    /// </summary>
    public bool ShouldUpgrade()
    {
        if (CurrentLevel >= 6)
            return false;  // 最高等级
        
        var nextLevelRequirement = LevelRequirements[CurrentLevel + 1];
        return CurrentExperience >= nextLevelRequirement;
    }
    
    /// <summary>
    /// 升级
    /// </summary>
    public void Upgrade()
    {
        if (CurrentLevel < 6)
        {
            CurrentLevel++;
            CurrentExperience = 0;  // 重置当前经验
        }
    }
    
    /// <summary>
    /// 获取升级所需经验
    /// </summary>
    public int GetNextLevelRequirement()
    {
        if (CurrentLevel >= 6)
            return int.MaxValue;  // 已最高级
        
        return LevelRequirements[CurrentLevel + 1] - LevelRequirements[CurrentLevel];
    }
    
    /// <summary>
    /// 获取等级进度百分比
    /// </summary>
    public int GetProgressPercentage()
    {
        if (CurrentLevel >= 6)
            return 100;
        
        var required = GetNextLevelRequirement();
        return (int)((CurrentExperience / required) * 100);
    }
    
    /// <summary>
    /// 获取等级权限
    /// </summary>
    public List<string> GetPrivileges()
    {
        return CurrentLevel switch
        {
            0 => new List<string> { "基础弹幕" },
            1 => new List<string> { "基础弹幕" },
            2 => new List<string> { "基础弹幕", "高级弹幕" },
            3 => new List<string> { "基础弹幕", "高级弹幕", "高级弹幕样式" },
            4 => new List<string> { "基础弹幕", "高级弹幕", "高级弹幕样式", "彩色弹幕" },
            5 => new List<string> { "全部弹幕功能" },
            6 => new List<string> { "全部弹幕功能", "专属标识" },
            _ => new List<string>()
        };
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        return new object[] { CurrentLevel, CurrentExperience, TotalExperience };
    }
}
```

---

### 4.2 UserCoins（B币值对象）

```csharp
/// <summary>
/// 用户B币值对象
/// </summary>
public class UserCoins : ValueObject
{
    // ========== 属性 ==========
    public int Balance { get; private set; }           // 当前余额
    public int TotalEarned { get; private set; }        // 累计获得
    public int TotalSpent { get; private set; }         // 累计消耗
    
    // ========== 构造函数 ==========
    private UserCoins(int balance, int earned, int spent)
    {
        Balance = balance;
        TotalEarned = earned;
        TotalSpent = spent;
    }
    
    // ========== 工厂方法 ==========
    public static UserCoins Empty()
    {
        return new UserCoins(0, 0, 0);
    }
    
    public static UserCoins WithInitialBalance(int amount)
    {
        return new UserCoins(amount, amount, 0);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 消耗B币
    /// </summary>
    public void Spend(int amount)
    {
        if (amount > Balance)
            throw new BusinessException("B币余额不足");
        
        Balance -= amount;
        TotalSpent += amount;
    }
    
    /// <summary>
    /// 获得B币
    /// </summary>
    public void Receive(int amount)
    {
        Balance += amount;
        TotalEarned += amount;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        return new object[] { Balance, TotalEarned, TotalSpent };
    }
}
```

---

### 4.3 UserProfile（用户信息值对象）

```csharp
/// <summary>
/// 用户信息值对象
/// </summary>
public class UserProfile : ValueObject
{
    // ========== 属性 ==========
    public string NickName { get; private set; }
    public string AvatarUrl { get; private set; }
    public string Signature { get; private set; }
    public int Gender { get; private set; }  // 0未知 1男 2女
    public DateTime? Birthday { get; private set; }
    
    // ========== 构造函数 ==========
    private UserProfile(string nickName, string avatar, string signature, int gender, DateTime? birthday)
    {
        NickName = nickName;
        AvatarUrl = avatar;
        Signature = signature;
        Gender = gender;
        Birthday = birthday;
    }
    
    // ========== 工厂方法 ==========
    public static UserProfile Default()
    {
        return new UserProfile(string.Empty, string.Empty, string.Empty, 0, null);
    }
    
    // ========== 业务方法 ==========
    
    public void Update(string nickName, string signature, int? gender)
    {
        if (!string.IsNullOrEmpty(nickName))
            NickName = nickName;
        
        if (!string.IsNullOrEmpty(signature))
            Signature = signature;
        
        if (gender.HasValue)
            Gender = gender.Value;
    }
    
    public void SetAvatar(string avatarUrl)
    {
        AvatarUrl = avatarUrl;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        return new object[] { NickName, AvatarUrl, Signature, Gender, Birthday };
    }
}
```

---

### 4.4 Verification（认证信息值对象）

```csharp
/// <summary>
/// 认证信息值对象
/// </summary>
public class Verification : ValueObject
{
    // ========== 属性 ==========
    public bool IsRealNameVerified { get; private set; }
    public DateTime? RealNameVerifyTime { get; private set; }
    public string RealName { get; private set; }        // 加密存储
    public string IdCardNumber { get; private set; }    // 加密存储
    
    public bool IsUpVerified { get; private set; }
    public DateTime? UpVerifyTime { get; private set; }
    public UpVerificationType UpType { get; private set; }
    
    // ========== 构造函数 ==========
    private Verification(
        bool realNameVerified,
        DateTime? realNameTime,
        string realName,
        string idCard,
        bool upVerified,
        DateTime? upTime,
        UpVerificationType upType)
    {
        IsRealNameVerified = realNameVerified;
        RealNameVerifyTime = realNameTime;
        RealName = realName;
        IdCardNumber = idCard;
        IsUpVerified = upVerified;
        UpVerifyTime = upTime;
        UpType = upType;
    }
    
    // ========== 工厂方法 ==========
    public static Verification None()
    {
        return new Verification(false, null, null, null, false, null, UpVerificationType.None);
    }
    
    // ========== 业务方法 ==========
    
    public void SetRealName(string realName, string idCardNumber)
    {
        // 加密存储（实际项目中应使用加密服务）
        RealName = Encrypt(realName);
        IdCardNumber = Encrypt(idCardNumber);
        IsRealNameVerified = true;
        RealNameVerifyTime = DateTime.UtcNow;
    }
    
    public void SetUpVerification(UpVerificationType type)
    {
        IsUpVerified = true;
        UpType = type;
        UpVerifyTime = DateTime.UtcNow;
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        return new object[] 
        {
            IsRealNameVerified, 
            RealNameVerifyTime, 
            IsUpVerified, 
            UpVerifyTime, 
            UpType 
        };
    }
    
    // ========== 私有方法 ==========
    private string Encrypt(string value)
    {
        // TODO: 实际加密逻辑
        return value;  // 简化示例
    }
}

public enum UpVerificationType
{
    None,
    Individual,
    Organization,
    Media
}
```

---

## 5. 实体设计

### 5.1 UserExperienceRecord（经验记录实体）

```csharp
/// <summary>
/// 用户经验记录实体
/// </summary>
public class UserExperienceRecord : Entity<Guid>
{
    // ========== 属性 ==========
    public Guid UserId { get; private set; }
    public ExperienceSource Source { get; private set; }
    public int Amount { get; private set; }
    public Guid? SourceId { get; private set; }  // 来源对象ID（视频/评论）
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public DateTime CreationTime { get; private set; }
    
    // ========== 构造函数 ==========
    public UserExperienceRecord(
        Guid id,
        Guid userId,
        ExperienceSource source,
        int amount,
        DateTime date,
        Guid? sourceId = null,
        string description = null) : base(id)
    {
        UserId = userId;
        Source = source;
        Amount = amount;
        Date = date;
        SourceId = sourceId;
        Description = description ?? source.GetDescription();
        CreationTime = DateTime.UtcNow;
    }
}

/// <summary>
/// 经验来源枚举
/// </summary>
public enum ExperienceSource
{
    DailyLogin,      // 每日登录 +5
    WatchVideo,      // 完整观看视频 +1
    LikeVideo,       // 点赞视频 +1
    CoinVideo,       // 投币视频 +1
    CommentVideo,    // 评论视频 +2
    ShareVideo,      // 分享视频 +2
    PublishVideo     // 发布视频 +10
}

public static class ExperienceSourceExtensions
{
    public static int GetAmount(this ExperienceSource source)
    {
        return source switch
        {
            ExperienceSource.DailyLogin => 5,
            ExperienceSource.WatchVideo => 1,
            ExperienceSource.LikeVideo => 1,
            ExperienceSource.CoinVideo => 1,
            ExperienceSource.CommentVideo => 2,
            ExperienceSource.ShareVideo => 2,
            ExperienceSource.PublishVideo => 10,
            _ => 0
        };
    }
    
    public static string GetDescription(this ExperienceSource source)
    {
        return source switch
        {
            ExperienceSource.DailyLogin => "每日登录",
            ExperienceSource.WatchVideo => "完整观看视频",
            ExperienceSource.LikeVideo => "点赞视频",
            ExperienceSource.CoinVideo => "投币视频",
            ExperienceSource.CommentVideo => "评论视频",
            ExperienceSource.ShareVideo => "分享视频",
            ExperienceSource.PublishVideo => "发布视频",
            _ => "未知来源"
        };
    }
}
```

---

### 5.2 UserCoinRecord（B币记录实体）

```csharp
/// <summary>
/// 用户B币记录实体
/// </summary>
public class UserCoinRecord : Entity<Guid>
{
    // ========== 属性 ==========
    public Guid UserId { get; private set; }
    public CoinTransactionType Type { get; private set; }
    public int Amount { get; private set; }          // 正数获得，负数消耗
    public int BalanceAfter { get; private set; }     // 交易后余额
    public Guid? TargetUserId { get; private set; }   // 目标用户（投币对象）
    public Guid? SourceId { get; private set; }       // 来源对象（视频）
    public string Description { get; private set; }
    public DateTime CreationTime { get; private set; }
    
    // ========== 构造函数 ==========
    public UserCoinRecord(
        Guid id,
        Guid userId,
        int amount,
        CoinTransactionType type,
        Guid? targetUserId = null,
        Guid? sourceId = null,
        string description = null) : base(id)
    {
        UserId = userId;
        Amount = amount;
        Type = type;
        TargetUserId = targetUserId;
        SourceId = sourceId;
        Description = description ?? type.GetDescription();
        CreationTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 设置交易后余额（由领域服务调用）
    /// </summary>
    internal void SetBalanceAfter(int balance)
    {
        BalanceAfter = balance;
    }
}

public enum CoinTransactionType
{
    AdminGrant,      // 管理员发放
    WatchReward,     // 观看奖励
    Receive,         // 收到投币
    Spend            // 投币消耗
}

public static class CoinTransactionTypeExtensions
{
    public static string GetDescription(this CoinTransactionType type)
    {
        return type switch
        {
            CoinTransactionType.AdminGrant => "管理员发放",
            CoinTransactionType.WatchReward => "观看奖励",
            CoinTransactionType.Receive => "收到投币",
            CoinTransactionType.Spend => "投币消耗",
            _ => "未知交易"
        };
    }
}
```

---

### 5.3 UserFollow（用户关注实体）

```csharp
/// <summary>
/// 用户关注实体
/// </summary>
public class UserFollow : Entity<Guid>
{
    // ========== 属性 ==========
    public Guid FollowerId { get; private set; }      // 关注者
    public Guid FollowingId { get; private set; }     // 被关注者
    public bool IsFollowing { get; private set; }
    public DateTime FollowTime { get; private set; }
    public DateTime? UnfollowTime { get; private set; }
    
    // ========== 构造函数 ==========
    public UserFollow(Guid id, Guid followerId, Guid followingId) : base(id)
    {
        FollowerId = followerId;
        FollowingId = followingId;
        IsFollowing = true;
        FollowTime = DateTime.UtcNow;
    }
    
    // ========== 业务方法 ==========
    
    public void Unfollow()
    {
        IsFollowing = false;
        UnfollowTime = DateTime.UtcNow;
    }
    
    public void Refollow()
    {
        IsFollowing = true;
        FollowTime = DateTime.UtcNow;
        UnfollowTime = null;
    }
}
```

---

### 5.4 UserMessage（用户消息实体）

```csharp
/// <summary>
/// 用户消息实体
/// </summary>
public class UserMessage : Entity<Guid>
{
    // ========== 属性 ==========
    public Guid ReceiverId { get; private set; }
    public Guid? SenderId { get; private set; }
    public MessageType Type { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public MessageRelatedType? RelatedType { get; private set; }
    public Guid? RelatedId { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadTime { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreationTime { get; private set; }
    
    // ========== 构造函数 ==========
    public UserMessage(
        Guid id,
        Guid receiverId,
        Guid? senderId,
        MessageType type,
        string content,
        string title = null,
        MessageRelatedType? relatedType = null,
        Guid? relatedId = null) : base(id)
    {
        ReceiverId = receiverId;
        SenderId = senderId;
        Type = type;
        Title = title;
        Content = content;
        RelatedType = relatedType;
        RelatedId = relatedId;
        IsRead = false;
        IsDeleted = false;
        CreationTime = DateTime.UtcNow;
    }
    
    // ========== 业务方法 ==========
    
    public void MarkAsRead()
    {
        IsRead = true;
        ReadTime = DateTime.UtcNow;
    }
    
    public void Delete()
    {
        IsDeleted = true;
    }
}

public enum MessageType
{
    System,      // 系统消息
    Private,     // 私信
    Reply,       // 评论回复
    At,          // @提及
    Notice       // 通知
}

public enum MessageRelatedType
{
    Video,
    Comment,
    Live,
    None
}
```

---

## 6. 领域服务设计

### 6.1 IUserLevelManager（等级管理领域服务）

```csharp
/// <summary>
/// 用户等级管理领域服务
/// 职责：处理等级相关的复杂业务逻辑
/// </summary>
public interface IUserLevelManager
{
    /// <summary>
    /// 计算每日获得的经验
    /// </summary>
    Task<int> CalculateDailyExperienceAsync(Guid userId, DateTime date);
    
    /// <summary>
    /// 检查每日经验是否达到上限
    /// </summary>
    Task<bool> IsDailyExperienceLimitReachedAsync(Guid userId, DateTime date);
    
    /// <summary>
    /// 获取用户等级详情
    /// </summary>
    Task<UserLevelDetail> GetLevelDetailAsync(Guid userId);
    
    /// <summary>
    /// 获取等级所需经验列表
    /// </summary>
    List<LevelRequirement> GetLevelRequirements();
}

/// <summary>
/// 领域服务实现
/// </summary>
public class UserLevelManager : DomainService, IUserLevelManager
{
    private readonly IUserExperienceRecordRepository _experienceRepository;
    
    public UserLevelManager(IUserExperienceRecordRepository experienceRepository)
    {
        _experienceRepository = experienceRepository;
    }
    
    public async Task<int> CalculateDailyExperienceAsync(Guid userId, DateTime date)
    {
        var records = await _experienceRepository.GetByUserIdAndDateAsync(userId, date);
        return records.Sum(r => r.Amount);
    }
    
    public async Task<bool> IsDailyExperienceLimitReachedAsync(Guid userId, DateTime date)
    {
        var daily = await CalculateDailyExperienceAsync(userId, date);
        return daily >= 100;  // 每日上限100
    }
    
    public Task<UserLevelDetail> GetLevelDetailAsync(Guid userId)
    {
        // 返回等级详情（包含等级、进度、所需经验等）
        // 实现略...
    }
    
    public List<LevelRequirement> GetLevelRequirements()
    {
        return new List<LevelRequirement>
        {
            new LevelRequirement(0, 0, 100, new List<string> { "基础弹幕" }),
            new LevelRequirement(1, 101, 200, new List<string> { "基础弹幕" }),
            new LevelRequirement(2, 201, 400, new List<string> { "基础弹幕", "高级弹幕" }),
            // ...
        };
    }
}
```

---

### 6.2 IUserCoinManager（B币管理领域服务）

```csharp
/// <summary>
/// 用户B币管理领域服务
/// 职责：处理B币交易相关的复杂业务逻辑
/// </summary>
public interface IUserCoinManager
{
    /// <summary>
    /// 处理投币交易
    /// </summary>
    Task<CoinTransactionResult> ProcessCoinTransactionAsync(
        Guid senderId,
        Guid receiverId,
        Guid videoId,
        int amount);
    
    /// <summary>
    /// 检查用户B币余额
    /// </summary>
    Task<bool> HasEnoughCoinsAsync(Guid userId, int amount);
    
    /// <summary>
    /// 获取用户B币余额
    /// </summary>
    Task<int> GetBalanceAsync(Guid userId);
}

public class UserCoinManager : DomainService, IUserCoinManager
{
    private readonly IBilibiliUserRepository _userRepository;
    private readonly IUserCoinRecordRepository _coinRecordRepository;
    
    public UserCoinManager(
        IBilibiliUserRepository userRepository,
        IUserCoinRecordRepository coinRecordRepository)
    {
        _userRepository = userRepository;
        _coinRecordRepository = coinRecordRepository;
    }
    
    public async Task<CoinTransactionResult> ProcessCoinTransactionAsync(
        Guid senderId,
        Guid receiverId,
        Guid videoId,
        int amount)
    {
        // 1. 获取发送者
        var sender = await _userRepository.GetAsync(senderId);
        
        // 2. 验证余额
        if (!sender.GetCoins().HasEnough(amount))
            throw new BusinessException("B币余额不足");
        
        // 3. 获取接收者
        var receiver = await _userRepository.GetAsync(receiverId);
        
        // 4. 执行交易
        sender.SpendCoins(amount, receiverId, videoId);
        receiver.ReceiveCoins(amount, senderId, videoId);
        
        // 5. 保存
        await _userRepository.UpdateAsync(sender);
        await _userRepository.UpdateAsync(receiver);
        
        return new CoinTransactionResult(
            sender.GetCoins().Balance,
            receiver.GetCoins().Balance,
            amount
        );
    }
    
    public async Task<bool> HasEnoughCoinsAsync(Guid userId, int amount)
    {
        var user = await _userRepository.GetAsync(userId);
        return user.GetCoins().HasEnough(amount);
    }
    
    public async Task<int> GetBalanceAsync(Guid userId)
    {
        var user = await _userRepository.GetAsync(userId);
        return user.GetCoins().Balance;
    }
}
```

---

## 7. 仓储接口设计

### 7.1 IBilibiliUserRepository

```csharp
/// <summary>
/// 用户仓储接口（继承ABP IRepository）
/// </summary>
public interface IBilibiliUserRepository : IRepository<BilibiliUser, Guid>
{
    /// <summary>
    /// 根据UserId获取BilibiliUser
    /// </summary>
    Task<BilibiliUser> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// 获取用户等级信息
    /// </summary>
    Task<UserLevel> GetUserLevelAsync(Guid userId);
    
    /// <summary>
    /// 获取用户B币余额
    /// </summary>
    Task<int> GetUserCoinsBalanceAsync(Guid userId);
    
    /// <summary>
    /// 检查实名认证状态
    /// </summary>
    Task<bool> IsRealNameVerifiedAsync(Guid userId);
    
    /// <summary>
    /// 获取用户关注列表
    /// </summary>
    Task<List<UserFollow>> GetFollowingAsync(Guid userId, int maxResultCount, int skipCount);
    
    /// <summary>
    /// 获取用户粉丝列表
    /// </summary>
    Task<List<UserFollow>> GetFollowersAsync(Guid userId, int maxResultCount, int skipCount);
    
    /// <summary>
    /// 检查关注关系
    /// </summary>
    Task<bool> IsFollowingAsync(Guid followerId, Guid followingId);
}
```

---

### 7.2 IUserExperienceRecordRepository

```csharp
/// <summary>
/// 经验记录仓储接口
/// </summary>
public interface IUserExperienceRecordRepository : IRepository<UserExperienceRecord, Guid>
{
    /// <summary>
    /// 获取用户指定日期的经验记录
    /// </summary>
    Task<List<UserExperienceRecord>> GetByUserIdAndDateAsync(Guid userId, DateTime date);
    
    /// <summary>
    /// 获取用户经验记录列表
    /// </summary>
    Task<List<UserExperienceRecord>> GetByUserIdAsync(
        Guid userId,
        DateTime? startDate,
        DateTime? endDate,
        int maxResultCount,
        int skipCount);
}
```

---

### 7.3 IUserCoinRecordRepository

```csharp
/// <summary>
/// B币记录仓储接口
/// </summary>
public interface IUserCoinRecordRepository : IRepository<UserCoinRecord, Guid>
{
    /// <summary>
    /// 获取用户B币记录列表
    /// </summary>
    Task<List<UserCoinRecord>> GetByUserIdAsync(
        Guid userId,
        int maxResultCount,
        int skipCount);
    
    /// <summary>
    /// 获取用户B币统计
    /// </summary>
    Task<UserCoinStatistics> GetStatisticsAsync(Guid userId);
}
```

---

## 8. 领域事件设计

### 8.1 领域事件定义

```csharp
/// <summary>
/// 用户升级事件
/// </summary>
public class UserLevelUpEvent
{
    public Guid UserId { get; }
    public int NewLevel { get; }
    public DateTime OccurredOn { get; }
    
    public UserLevelUpEvent(Guid userId, int newLevel)
    {
        UserId = userId;
        NewLevel = newLevel;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 用户投币消耗事件
/// </summary>
public class UserCoinSpentEvent
{
    public Guid UserId { get; }
    public int Amount { get; }
    public Guid TargetUserId { get; }
    public Guid VideoId { get; }
    public DateTime OccurredOn { get; }
    
    public UserCoinSpentEvent(Guid userId, int amount, Guid targetUserId, Guid videoId)
    {
        UserId = userId;
        Amount = amount;
        TargetUserId = targetUserId;
        VideoId = videoId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 用户收到投币事件
/// </summary>
public class UserCoinReceivedEvent
{
    public Guid UserId { get; }
    public int Amount { get; }
    public Guid SenderId { get; }
    public Guid VideoId { get; }
    public DateTime OccurredOn { get; }
    
    public UserCoinReceivedEvent(Guid userId, int amount, Guid senderId, Guid videoId)
    {
        UserId = userId;
        Amount = amount;
        SenderId = senderId;
        VideoId = videoId;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 用户实名认证事件
/// </summary>
public class UserRealNameVerifiedEvent
{
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public UserRealNameVerifiedEvent(Guid userId)
    {
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}
```

---

## 9. 业务规则总结

### 9.1 等级规则

| 规则 | 说明 |
|------|------|
| 等级上限 | LV6（最高等级） |
| 每日经验上限 | 100经验 |
| 等级所需经验 | LV0:0-100, LV1:101-200, LV2:201-400... |
| 升级触发 | 当前经验达到下一级要求 |
| 权限解锁 | LV2解锁高级弹幕 |

---

### 9.2 B币规则

| 规则 | 说明 |
|------|------|
| 投币数量 | 无限制（用户需求） |
| 余额验证 | 投币前验证余额 |
| 交易记录 | 所有交易必须记录 |
| 收益分配 | 投币100%给UP主 |

---

### 9.3 认证规则

| 规则 | 说明 |
|------|------|
| 实名认证 | 只能认证一次 |
| UP主认证 | 需人工审核 |
| 数据加密 | 真实姓名、身份证号加密存储 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 用户模块领域设计文档完成