# 分类管理模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | CategoryService（分类服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 频域模型概览

### 2.1 题领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    分类领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │  Category    │◄───── CategoryStatus (值对象)            │
│  │ (聚合根)     │◄───── CategoryHierarchy (值对象)         │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ContentAudit  │◄───── AuditResult (值对象)              │
│  │ (聚合根)     │◄───── AuditStatus (值对象)              │
│  └──────────────┘                                          │
│                                                             │
│  值对象：                                                    │
│  ┌──────────────┐ ┌──────────────┐                        │
│  │CategoryStatus│ │CategoryHierarchy│                     │
│  └──────────────┘ └──────────────┘                        │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐                        │
│  │ AuditResult  │ │ AuditStatus │                        │
│  └──────────────┘ └──────────────┘                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 Category（分类聚合根）

```csharp
/// <summary>
/// 分类聚合根
/// 业务规则：10个主分区（用户需求）
/// </summary>
public class Category : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid? ParentId { get; private set; } // 父分类ID（null表示主分区）
    
    // ========== 属性 ==========
    public string Name { get; private set; }    // 分类名称
    public string? Icon { get; private set; }   // 分类图标
    public string? Description { get; private set; } // 分类描述
    public int SortOrder { get; private set; }  // 排序顺序
    public int Level { get; private set; }      // 分类层级（0=主分区，1=子分区）
    public bool IsEnabled { get; private set; } // 是否启用
    
    // ========== 子分类集合 ==========
    private readonly List<Category> _children = new();
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private Category() { }
    
    private Category(
        Guid id,
        string name,
        Guid? parentId,
        int level,
        string? icon,
        string? description,
        int sortOrder) : base(id)
    {
        Name = name;
        ParentId = parentId;
        Level = level;
        Icon = icon;
        Description = description;
        SortOrder = sortOrder;
        IsEnabled = true;
        CreatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建主分区（Level 0）
    /// </summary>
    public static Category CreateMain(string name, string? icon = null, string? description = null, int sortOrder = 0)
    {
        // 业务规则：主分区最多10个（用户需求）
        if (name.Length > 100)
            throw new BusinessException("分类名称不能超过100字符");
        
        return new Category(GuidGenerator.Create(), name, null, 0, icon, description, sortOrder);
    }
    
    /// <summary>
    /// 创建子分区（Level 1）
    /// </summary>
    public static Category CreateSub(Guid parentId, string name, int sortOrder = 0)
    {
        return new Category(GuidGenerator.Create(), name, parentId, 1, null, null, sortOrder);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 添加子分类
    /// </summary>
    public void AddChild(Category child)
    {
        if (Level != 0)
            throw new BusinessException("只有主分区可以添加子分类");
        
        _children.Add(child);
    }
    
    /// <summary>
    /// 更新分类信息
    /// </summary>
    public void UpdateInfo(string name, string? icon, string? description)
    {
        Name = name;
        Icon = icon;
        Description = description;
    }
    
    /// <summary>
    /// 更新排序
    /// </summary>
    public void UpdateSortOrder(int newOrder)
    {
        SortOrder = newOrder;
    }
    
    /// <summary>
    /// 启用分类
    /// </summary>
    public void Enable()
    {
        IsEnabled = true;
    }
    
    /// <summary>
    /// 禁用分类
    /// </summary>
    public void Disable()
    {
        IsEnabled = false;
    }
    
    // ========== 查询方法 ==========
    
    public bool IsMainCategory => Level == 0;
    public bool IsSubCategory => Level == 1;
    public bool HasChildren => _children.Count > 0;
    public int ChildrenCount => _children.Count;
}
```

---

### 3.2 ContentAudit（内容审核聚合根）

```csharp
/// <summary>
/// 内容审核聚合根
/// 业务规则：AI自动审核通过（用户需求）
/// </summary>
public class ContentAudit : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid VideoId { get; private set; }   // 视频ID
    public Guid? AuditorId { get; private set; } // 审核员ID（人工审核）
    
    // ========== 值对象 ==========
    private AuditStatus _status;                // 审核状态
    private AuditResult _aiResult;              // AI审核结果
    
    // ========== 属性 ==========
    public string? Reason { get; private set; } // 审核理由
    public string? AIAuditJson { get; private set; } // AI审核JSON
    
    // ========== 时间戳 ==========
    public DateTime CreatedAt { get; private set; }
    public DateTime? AuditedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private ContentAudit() { }
    
    private ContentAudit(Guid videoId) : base(GuidGenerator.Create())
    {
        VideoId = videoId;
        _status = AuditStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建审核记录
    /// </summary>
    public static ContentAudit Create(Guid videoId)
    {
        return new ContentAudit(videoId);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// AI自动审核（自动通过）
    /// </summary>
    public void AutoApproveByAI(string aiAuditJson)
    {
        // 业务规则：AI自动审核通过（用户需求）
        _status = AuditStatus.AutoApproved;
        AIAuditJson = aiAuditJson;
        AuditedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ContentAutoApprovedEvent(Id, VideoId));
    }
    
    /// <summary>
    /// AI审核标记需要人工审核
    /// </summary>
    public void MarkNeedManualAudit(string aiAuditJson, string reason)
    {
        _status = AuditStatus.NeedManual;
        AIAuditJson = aiAuditJson;
        Reason = reason;
    }
    
    /// <summary>
    /// 人工审核通过
    /// </summary>
    public void ManualApprove(Guid auditorId, string reason)
    {
        if (_status != AuditStatus.NeedManual)
            throw new BusinessException("只有待人工审核的内容可以审核");
        
        _status = AuditStatus.ManualApproved;
        AuditorId = auditorId;
        Reason = reason;
        AuditedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ContentManualApprovedEvent(Id, VideoId, auditorId));
    }
    
    /// <summary>
    /// 人工审核拒绝
    /// </summary>
    public void ManualReject(Guid auditorId, string reason)
    {
        if (_status != AuditStatus.NeedManual)
            throw new BusinessException("只有待人工审核的内容可以审核");
        
        _status = AuditStatus.Rejected;
        AuditorId = auditorId;
        Reason = reason;
        AuditedAt = DateTime.UtcNow;
        
        AddDomainEvent(new ContentRejectedEvent(Id, VideoId, auditorId, reason));
    }
    
    // ========== 查询方法 ==========
    
    public AuditStatus GetStatus() => _status;
    
    public bool IsApproved => _status == AuditStatus.AutoApproved || _status == AuditStatus.ManualApproved;
    public bool IsRejected => _status == AuditStatus.Rejected;
    public bool NeedManualAudit => _status == AuditStatus.NeedManual;
}
```

---

## 4. 值对象设计

### 4.1 AuditStatus（审核状态值对象）

```csharp
/// <summary>
/// 审核状态值对象
/// </summary>
public class AuditStatus : ValueObject
{
    public static readonly AuditStatus Pending = new("pending", "待审核");
    public static readonly AuditStatus AutoApproved = new("auto_approved", "AI自动通过");
    public static readonly AuditStatus NeedManual = new("need_manual", "需人工审核");
    public static readonly AuditStatus ManualApproved = new("manual_approved", "人工通过");
    public static readonly AuditStatus Rejected = new("rejected", "审核拒绝");
    
    public string Code { get; }
    public string DisplayName { get; }
    
    private AuditStatus(string code, string displayName)
    {
        Code = code;
        DisplayName = displayName;
    }
    
    public bool IsApproved() => this == AutoApproved || this == ManualApproved;
    public bool NeedManual() => this == NeedManual;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Code;
    }
}
```

---

### 4.2 AuditResult（审核结果值对象）

```csharp
/// <summary>
/// AI审核结果值对象
/// </summary>
public class AuditResult : ValueObject
{
    public bool IsSafe { get; }                 // 是否安全
    public double Confidence { get; }           // 置信度（0-1）
    public IReadOnlyList<string> FlaggedContent { get; } // 标记内容
    public string? SuggestedAction { get; }     // 建议动作
    
    private AuditResult(bool isSafe, double confidence, IReadOnlyList<string> flaggedContent, string? suggestedAction)
    {
        IsSafe = isSafe;
        Confidence = confidence;
        FlaggedContent = flaggedContent;
        SuggestedAction = suggestedAction;
    }
    
    public static AuditResult Safe()
    {
        return new AuditResult(true, 0.95, new List<string>(), "approve");
    }
    
    public static AuditResult Flagged(IReadOnlyList<string> flaggedContent)
    {
        return new AuditResult(false, 0.85, flaggedContent, "manual_audit");
    }
    
    public bool NeedManualAudit() => !IsSafe || Confidence < 0.8;
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return IsSafe;
        yield return Confidence;
    }
}
```

---

## 5. 领域服务设计

### 5.1 ICategoryService

```csharp
public interface ICategoryService
{
    Task<Category> CreateMainCategoryAsync(string name, string? icon = null);
    Task<Category> CreateSubCategoryAsync(Guid parentId, string name);
    Task<List<Category>> GetMainCategoriesAsync();
    Task<List<Category>> GetSubCategoriesAsync(Guid parentId);
    Task<Category?> GetByIdAsync(Guid id);
}
```

---

### 5.2 IContentAuditService

```csharp
/// <summary>
/// 内容审核领域服务
/// 业务规则：AI自动审核通过（用户需求）
/// </summary>
public interface IContentAuditService
{
    Task<ContentAudit> SubmitForAuditAsync(Guid videoId);
    Task<AuditResult> PerformAIAsync(Guid videoId, string videoContent);
    Task AutoApproveAsync(Guid auditId, string aiResult);
    Task ManualApproveAsync(Guid auditId, Guid auditorId, string reason);
    Task ManualRejectAsync(Guid auditId, Guid auditorId, string reason);
    Task<List<ContentAudit>> GetPendingManualAuditsAsync(int skip, int take);
}
```

---

## 6. 仓储接口设计

```csharp
public interface ICategoryRepository : IRepository<Category, Guid>
{
    Task<List<Category>> GetMainCategoriesAsync();
    Task<List<Category>> GetSubCategoriesAsync(Guid parentId);
    Task<Category?> GetByNameAsync(string name);
    Task<int> CountMainCategoriesAsync();
}

public interface IContentAuditRepository : IRepository<ContentAudit, Guid>
{
    Task<ContentAudit?> GetByVideoIdAsync(Guid videoId);
    Task<List<ContentAudit>> GetByStatusAsync(AuditStatus status, int skip, int take);
    Task<List<ContentAudit>> GetPendingManualAsync(int skip, int take);
}
```

---

## 7. 频域事件

```csharp
public class CategoryCreatedEvent { public Guid Id, string Name, int Level; }
public class ContentAutoApprovedEvent { public Guid Id, VideoId; }
public class ContentManualApprovedEvent { public Guid Id, VideoId, AuditorId; }
public class ContentRejectedEvent { public Guid Id, VideoId, AuditorId; public string Reason; }
```

---

## 8. 业务规则总结

| 规则 | 说明 |
|------|------|
| 主分区数量 | 10个（用户需求） |
| 子分区层级 | 最多1层（主分区→子分区） |
| 分类启用 | 支持启用/禁用 |
| AI审核 | 自动审核通过（用户需求） |
| 人工审核 | 仅AI标记的内容需人工审核 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 分类管理模块领域设计文档完成