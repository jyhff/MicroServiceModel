# 管理模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | AdminService（管理服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 题领域模型概览

```
┌─────────────────────────────────────────────────────────────┐
│                    管理领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │ AdminUser    │◄───── AdminRole (值对象)                 │
│  │ (聚合根)     │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ BanRecord    │◄───── BanDuration (值对象)               │
│  │ (聚合根)     │◄───── BanReason (值对象)                 │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ AuditRecord  │◄───── AuditStatus (值对象)               │
│  │ (聚合根)     │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ SystemConfig │                                          │
│  │ (聚合根)     │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ DashboardStat│                                          │
│  │ (值对象)     │                                          │
│  └──────────────┘                                          │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 AdminUser（管理员聚合根）

```csharp
/// <summary>
/// 管理员聚合根
/// 业务规则：仅超级管理员（用户需求）
/// </summary>
public class AdminUser : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }    // 关联用户ID
    
    // ========== 属性 ==========
    public string Username { get; private set; } // 管理员账号
    public AdminRole Role { get; private set; }  // 角色（仅超级管理员）
    public bool IsActive { get; private set; }   // 是否激活
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    
    // ========== 构造函数 ==========
    private AdminUser() { }
    
    private AdminUser(Guid userId, string username) : base(GuidGenerator.Create())
    {
        UserId = userId;
        Username = username;
        Role = AdminRole.SuperAdmin; // 仅超级管理员
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建超级管理员
    /// </summary>
    public static AdminUser CreateSuperAdmin(Guid userId, string username)
    {
        return new AdminUser(userId, username);
    }
    
    // ========== 业务方法 ==========
    
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
    
    public void Activate()
    {
        IsActive = true;
    }
    
    public bool IsSuperAdmin => Role == AdminRole.SuperAdmin;
}

/// <summary>
/// 管理员角色（仅超级管理员）
/// </summary>
public enum AdminRole
{
    SuperAdmin = 1  // 超级管理员（用户需求：仅此角色）
}
```

---

### 3.2 BanRecord（封禁记录聚合根）

```csharp
/// <summary>
/// 封禁记录聚合根
/// </summary>
public class BanRecord : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 被封禁用户ID
    public Guid AdminId { get; private set; }    // 执行封禁的管理员ID
    
    // ========== 属性 ==========
    public BanType Type { get; private set; }    // 封禁类型
    public string Reason { get; private set; }   // 封禁原因
    public int DurationDays { get; private set; } // 封禁时长（天）
    public BanStatus Status { get; private set; } // 封禁状态
    
    // ========== 时间戳 ==========
    public DateTime BannedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? UnbannedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private BanRecord() { }
    
    private BanRecord(
        Guid userId,
        Guid adminId,
        BanType type,
        string reason,
        int durationDays) : base(GuidGenerator.Create())
    {
        UserId = userId;
        AdminId = adminId;
        Type = type;
        Reason = reason;
        DurationDays = durationDays;
        Status = BanStatus.Active;
        BannedAt = DateTime.UtcNow;
        
        if (durationDays > 0)
            ExpiresAt = DateTime.UtcNow.AddDays(durationDays);
        else
            ExpiresAt = null; // 永久封禁
        
        AddDomainEvent(new UserBannedEvent(userId, type, reason, durationDays));
    }
    
    // ========== 工厂方法 ==========
    
    public static BanRecord BanUser(Guid userId, Guid adminId, BanType type, string reason, int durationDays)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException("封禁原因不能为空");
        
        return new BanRecord(userId, adminId, type, reason, durationDays);
    }
    
    public static BanRecord PermanentBan(Guid userId, Guid adminId, BanType type, string reason)
    {
        return new BanRecord(userId, adminId, type, reason, 0);
    }
    
    // ========== 业务方法 ==========
    
    public void Unban(Guid adminId, string reason)
    {
        if (Status != BanStatus.Active)
            throw new BusinessException("用户未被封禁");
        
        Status = BanStatus.Unbanned;
        UnbannedAt = DateTime.UtcNow;
        
        AddDomainEvent(new UserUnbannedEvent(UserId, reason));
    }
    
    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }
    
    public bool IsPermanent => DurationDays == 0;
    public bool IsActive => Status == BanStatus.Active && !IsExpired();
}

/// <summary>
/// 封禁类型
/// </summary>
public enum BanType
{
    Account = 1,       // 账号封禁
    Upload = 2,        // 上传封禁
    Comment = 3,       // 评论封禁
    Danmaku = 4        // 弹幕封禁
}

/// <summary>
/// 封禁状态
/// </summary>
public enum BanStatus
{
    Active = 1,        // 封禁中
    Unbanned = 2,      // 已解封
    Expired = 3        // 已过期
}
```

---

### 3.3 AuditRecord（审核记录聚合根）

```csharp
/// <summary>
/// 审核记录聚合根（视频审核）
/// </summary>
public class AuditRecord : AggregateRoot<Guid>
{
    public Guid VideoId { get; private set; }
    public Guid AdminId { get; private set; }
    public AuditType Type { get; private set; }
    public AuditDecision Decision { get; private set; }
    public string? Reason { get; private set; }
    public DateTime AuditedAt { get; private set; }
    
    private AuditRecord() { }
    
    private AuditRecord(Guid videoId, Guid adminId, AuditType type, AuditDecision decision, string? reason) : base(GuidGenerator.Create())
    {
        VideoId = videoId;
        AdminId = adminId;
        Type = type;
        Decision = decision;
        Reason = reason;
        AuditedAt = DateTime.UtcNow;
        
        AddDomainEvent(new VideoAuditedEvent(videoId, decision, reason));
    }
    
    public static AuditRecord Approve(Guid videoId, Guid adminId, AuditType type)
    {
        return new AuditRecord(videoId, adminId, type, AuditDecision.Approved, null);
    }
    
    public static AuditRecord Reject(Guid videoId, Guid adminId, AuditType type, string reason)
    {
        return new AuditRecord(videoId, adminId, type, AuditDecision.Rejected, reason);
    }
    
    public bool IsApproved => Decision == AuditDecision.Approved;
}

/// <summary>
/// 审核类型
/// </summary>
public enum AuditType
{
    Video = 1,         // 视频审核
    Comment = 2,       // 评论审核
    Danmaku = 3        // 弹幕审核
}

/// <summary>
/// 审核决定
/// </summary>
public enum AuditDecision
{
    Approved = 1,      // 通过
    Rejected = 2       // 拒绝
}
```

---

### 3.4 SystemConfig（系统配置聚合根）

```csharp
/// <summary>
/// 系统配置聚合根
/// </summary>
public class SystemConfig : AggregateRoot<Guid>
{
    public string Key { get; private set; }
    public string Value { get; private set; }
    public string? Description { get; private set; }
    public string Category { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    private SystemConfig() { }
    
    private SystemConfig(string key, string value, string? description, string category) : base(GuidGenerator.Create())
    {
        Key = key;
        Value = value;
        Description = description;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public static SystemConfig Create(string key, string value, string? description, string category)
    {
        return new SystemConfig(key, value, description, category);
    }
    
    public void UpdateValue(string newValue)
    {
        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

---

## 4. 值对象设计

### 4.1 DashboardStats（仪表盘统计值对象）

```csharp
/// <summary>
/// 仪表盘统计值对象
/// </summary>
public class DashboardStats : ValueObject
{
    public long DAU { get; }                    // 日活用户
    public long MAU { get; }                    // 月活用户
    public long NewUsersToday { get; }          // 今日新增用户
    public double D1Retention { get; }          // 次日留存率
    public double D7Retention { get; }          // 7日留存率
    public double D30Retention { get; }         // 30日留存率
    public long DailyViews { get; }             // 日播放量
    public long DailyUploads { get; }           // 日上传量
    public double AvgWatchTime { get; }         // 平均观看时长
    
    private DashboardStats(
        long dau, long mau, long newUsersToday,
        double d1Retention, double d7Retention, double d30Retention,
        long dailyViews, long dailyUploads, double avgWatchTime)
    {
        DAU = dau;
        MAU = mau;
        NewUsersToday = newUsersToday;
        D1Retention = d1Retention;
        D7Retention = d7Retention;
        D30Retention = d30Retention;
        DailyViews = dailyViews;
        DailyUploads = dailyUploads;
        AvgWatchTime = avgWatchTime;
    }
    
    public static DashboardStats Create(
        long dau, long mau, long newUsersToday,
        double d1Retention, double d7Retention, double d30Retention,
        long dailyViews, long dailyUploads, double avgWatchTime)
    {
        return new DashboardStats(dau, mau, newUsersToday, d1Retention, d7Retention, d30Retention, dailyViews, dailyUploads, avgWatchTime);
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return DAU;
        yield return MAU;
    }
}
```

---

## 5. 领域服务设计

### 5.1 IAdminService

```csharp
public interface IAdminService
{
    Task<DashboardStats> GetDashboardStatsAsync();
    Task BanUserAsync(Guid userId, Guid adminId, BanType type, string reason, int durationDays);
    Task UnbanUserAsync(Guid userId, Guid adminId, string reason);
    Task ApproveVideoAsync(Guid videoId, Guid adminId);
    Task RejectVideoAsync(Guid videoId, Guid adminId, string reason);
    Task<bool> IsUserBannedAsync(Guid userId, BanType type);
    Task<List<BanRecord>> GetActiveBansAsync(Guid userId);
}
```

---

### 5.2 IAdminUserService

```csharp
public interface IAdminUserService
{
    Task<AdminUser?> GetByIdAsync(Guid adminId);
    Task<AdminUser?> GetByUserIdAsync(Guid userId);
    Task<bool> IsAdminAsync(Guid userId);
    Task<bool> IsSuperAdminAsync(Guid userId);
    Task<List<AdminUser>> GetAllAdminsAsync();
}
```

---

## 6. 仓储接口设计

```csharp
public interface IAdminUserRepository : IRepository<AdminUser, Guid>
{
    Task<AdminUser?> GetByUserIdAsync(Guid userId);
    Task<List<AdminUser>> GetAllAsync();
}

public interface IBanRecordRepository : IRepository<BanRecord, Guid>
{
    Task<List<BanRecord>> GetByUserIdAsync(Guid userId);
    Task<BanRecord?> GetActiveBanAsync(Guid userId, BanType type);
    Task<int> CountActiveBansAsync(Guid userId);
}

public interface IAuditRecordRepository : IRepository<AuditRecord, Guid>
{
    Task<List<AuditRecord>> GetByVideoIdAsync(Guid videoId);
    Task<List<AuditRecord>> GetByAdminIdAsync(Guid adminId);
}

public interface ISystemConfigRepository : IRepository<SystemConfig, Guid>
{
    Task<SystemConfig?> GetByKeyAsync(string key);
    Task<List<SystemConfig>> GetByCategoryAsync(string category);
}
```

---

## 7. 题领域事件

```csharp
public class UserBannedEvent { public Guid UserId; public BanType Type; public string Reason; public int DurationDays; }
public class UserUnbannedEvent { public Guid UserId; public string Reason; }
public class VideoAuditedEvent { public Guid VideoId; public AuditDecision Decision; public string? Reason; }
public class AdminLoginEvent { public Guid AdminId; }
```

---

## 8. 业务规则总结

| 规则 | 说明 |
|------|------|
| 管理员角色 | 仅超级管理员（用户需求） |
| 封禁类型 | 账号、上传、评论、弹幕 |
| 封禁时长 | 可永久封禁 |
| 视频审核 | 人工审核 |
| 统计数据 | DAU、MAU、留存率、播放量 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 管理模块领域设计文档完成