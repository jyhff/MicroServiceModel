# 搜索模块领域设计文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | SearchService（搜索服务） |
| 领域层 | Domain层 |
| 创建时间 | 2026-05-12 |

---

## 2. 题领域模型概览

### 2.1 题领域模型图

```
┌─────────────────────────────────────────────────────────────┐
│                    搜索领域模型                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌──────────────┐                                          │
│  │ SearchQuery  │◄───── SearchKeyword (值对象)             │
│  │ (聚合根)     │◄───── SearchFilter (值对象)              │
│  └──────────────┘◄───── SearchSort (值对象)               │
│                                                             │
│  ┌──────────────┐                                          │
│  │SearchResult  │◄───── SearchHighlight (值对象)          │
│  │ (聚合根)     │◄───── SearchScore (值对象)              │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ SearchIndex  │                                          │
│  │ (聚合根)     │                                          │
│  │ (ES索引管理) │                                          │
│  └──────────────┘                                          │
│                                                             │
│  ┌──────────────┐                                          │
│  │ SearchHistory│                                          │
│  │ (聚合根)     │                                          │
│  │ (搜索历史)   │                                          │
│  └──────────────┘                                          │
│                                                             │
│  值对象：                                                    │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐       │
│  │SearchKeyword │ │SearchFilter  │ │SearchSort    │       │
│  └──────────────┘ └──────────────┘ └──────────────┘       │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐                        │
│  │SearchHighlight│ │SearchScore │                        │
│  └──────────────┘ └──────────────┘                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 聚合根设计

### 3.1 SearchQuery（搜索查询聚合根）

**聚合边界定义**：
- 聚合根：SearchQuery
- 值对象：SearchKeyword、SearchFilter、SearchSort
- 外部引用：UserId（用户ID）

**职责**：
- 管理搜索关键词和过滤器
- 定义搜索排序规则
- 记录搜索查询参数

```csharp
/// <summary>
/// 搜索查询聚合根
/// 业务规则：
/// 1. Elasticsearch索引搜索
/// 2. 实时同步（用户需求）
/// </summary>
public class SearchQuery : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid? UserId { get; private set; }    // 搜索用户ID（可为空，匿名搜索）
    
    // ========== 值对象 ==========
    private SearchKeyword _keyword;              // 搜索关键词
    private SearchFilter _filter;                // 搜索过滤器
    private SearchSort _sort;                    // 搜索排序
    
    // ========== 属性 ==========
    public int PageSize { get; private set; }    // 每页数量
    public int PageNumber { get; private set; }  // 页码
    
    // ========== 时间戳 ==========
    public DateTime QueriedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private SearchQuery() { }
    
    private SearchQuery(
        Guid id,
        Guid? userId,
        SearchKeyword keyword,
        SearchFilter filter,
        SearchSort sort,
        int pageSize,
        int pageNumber) : base(id)
    {
        UserId = userId;
        _keyword = keyword;
        _filter = filter;
        _sort = sort;
        PageSize = pageSize;
        PageNumber = pageNumber;
        QueriedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索查询
    /// </summary>
    public static SearchQuery Create(
        string keyword,
        Guid? userId = null,
        Guid? categoryId = null,
        SearchSortType sortType = SearchSortType.Relevance,
        int pageSize = 20,
        int pageNumber = 1)
    {
        var searchKeyword = SearchKeyword.Create(keyword);
        var searchFilter = SearchFilter.Create(categoryId);
        var searchSort = SearchSort.Create(sortType);
        
        return new SearchQuery(
            GuidGenerator.Create(),
            userId,
            searchKeyword,
            searchFilter,
            searchSort,
            pageSize,
            pageNumber);
    }
    
    /// <summary>
    /// 创建空查询（无关键词）
    /// </summary>
    public static SearchQuery Empty(Guid? userId = null)
    {
        return new SearchQuery(
            GuidGenerator.Create(),
            userId,
            SearchKeyword.Empty(),
            SearchFilter.Empty(),
            SearchSort.Default(),
            20,
            1);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 更新搜索关键词
    /// </summary>
    public void UpdateKeyword(string newKeyword)
    {
        _keyword = SearchKeyword.Create(newKeyword);
        QueriedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 更新搜索过滤器
    /// </summary>
    public void UpdateFilter(Guid? categoryId)
    {
        _filter = SearchFilter.Create(categoryId);
    }
    
    /// <summary>
    /// 更新搜索排序
    /// </summary>
    public void UpdateSort(SearchSortType sortType)
    {
        _sort = SearchSort.Create(sortType);
    }
    
    /// <summary>
    /// 更新页码
    /// </summary>
    public void UpdatePage(int newPage)
    {
        if (newPage < 1)
            throw new BusinessException("页码必须大于0");
        
        PageNumber = newPage;
    }
    
    // ========== 查询方法 ==========
    
    public SearchKeyword GetKeyword() => _keyword;
    public SearchFilter GetFilter() => _filter;
    public SearchSort GetSort() => _sort;
    
    public string GetKeywordText() => _keyword.Text;
    public int GetOffset() => (PageNumber - 1) * PageSize;
    
    public bool HasKeyword() => !_keyword.IsEmpty();
    public bool HasFilter() => !_filter.IsEmpty();
    
    /// <summary>
    /// 是否有效搜索（有关键词）
    /// </summary>
    public bool IsValid() => HasKeyword();
    
    /// <summary>
    /// 转换为Elasticsearch查询DSL（示例）
    /// </summary>
    public object ToElasticsearchQuery()
    {
        var query = new Dictionary<string, object>();
        
        if (HasKeyword())
        {
            query["query"] = new Dictionary<string, object>
            {
                ["multi_match"] = new Dictionary<string, object>
                {
                    ["query"] = _keyword.Text,
                    ["fields"] = new[] { "title^3", "description^1", "nickname^2" },
                    ["type"] = "best_fields"
                }
            };
        }
        else
        {
            query["query"] = new Dictionary<string, object>
            {
                ["match_all"] = new Dictionary<string, object>()
            };
        }
        
        // 排序
        query["sort"] = _sort.ToElasticsearchSort();
        
        // 分页
        query["from"] = GetOffset();
        query["size"] = PageSize;
        
        // 高亮
        query["highlight"] = new Dictionary<string, object>
        {
            ["fields"] = new Dictionary<string, object>
            {
                ["title"] = new Dictionary<string, object>(),
                ["description"] = new Dictionary<string, object>()
            },
            ["pre_tags"] = new[] { "<em>" },
            ["post_tags"] = new[] { "</em>" }
        };
        
        return query;
    }
}
```

---

## 4. 值对象设计

### 4.1 SearchKeyword（搜索关键词值对象）

```csharp
/// <summary>
/// 搜索关键词值对象
/// </summary>
public class SearchKeyword : ValueObject
{
    // ========== 属性 ==========
    public string Text { get; }                 // 关键词文本
    public IReadOnlyList<string> Terms { get; } // 分词后的词条
    public bool IsEmpty { get; }                // 是否空关键词
    
    // ========== 构造函数 ==========
    private SearchKeyword(string text, IReadOnlyList<string> terms, bool isEmpty)
    {
        Text = text;
        Terms = terms;
        IsEmpty = isEmpty;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索关键词
    /// </summary>
    public static SearchKeyword Create(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Empty();
        
        // 清理关键词
        text = text.Trim();
        
        // 分词（简单分词：空格分隔）
        var terms = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        
        return new SearchKeyword(text, terms, false);
    }
    
    /// <summary>
    /// 创建空关键词
    /// </summary>
    public static SearchKeyword Empty()
    {
        return new SearchKeyword("", new List<string>(), true);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否包含指定词条
    /// </summary>
    public bool Contains(string term)
    {
        return Terms.Contains(term);
    }
    
    /// <summary>
    /// 是否多个词条
    /// </summary>
    public bool IsMultiTerm()
    {
        return Terms.Count > 1;
    }
    
    /// <summary>
    /// 词条数量
    /// </summary>
    public int TermCount()
    {
        return Terms.Count;
    }
    
    /// <summary>
    /// 是否可能是视频ID搜索
    /// </summary>
    public bool IsVideoIdSearch()
    {
        // GUID格式检测
        return Guid.TryParse(Text, out _);
    }
    
    /// <summary>
    /// 是否可能是用户ID搜索
    /// </summary>
    public bool IsUserIdSearch()
    {
        return Guid.TryParse(Text, out _);
    }
    
    /// <summary>
    /// 是否短关键词（<3字符）
    /// </summary>
    public bool IsShortKeyword()
    {
        return Text.Length < 3;
    }
    
    /// <summary>
    /// 是否长关键词（>50字符）
    /// </summary>
    public bool IsLongKeyword()
    {
        return Text.Length > 50;
    }
    
    /// <summary>
    /// 转换为搜索建议匹配
    /// </summary>
    public bool MatchesSuggestion(string suggestion)
    {
        return suggestion.ToLowerInvariant().Contains(Text.ToLowerInvariant());
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Text;
    }
}
```

---

### 4.2 SearchFilter（搜索过滤器值对象）

```csharp
/// <summary>
/// 搜索过滤器值对象
/// </summary>
public class SearchFilter : ValueObject
{
    // ========== 属性 ==========
    public Guid? CategoryId { get; }            // 分类ID
    public Guid? UserId { get; }                // UP主ID
    public DateTime? StartTime { get; }         // 开始时间
    public DateTime? EndTime { get; }           // 结束时间
    public int? MinDuration { get; }            // 最小时长（秒）
    public int? MaxDuration { get; }            // 最大时长（秒）
    public IReadOnlyList<string>? Tags { get; } // 标签过滤
    
    // ========== 构造函数 ==========
    private SearchFilter(
        Guid? categoryId,
        Guid? userId,
        DateTime? startTime,
        DateTime? endTime,
        int? minDuration,
        int? maxDuration,
        IReadOnlyList<string>? tags)
    {
        CategoryId = categoryId;
        UserId = userId;
        StartTime = startTime;
        EndTime = endTime;
        MinDuration = minDuration;
        MaxDuration = maxDuration;
        Tags = tags;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索过滤器
    /// </summary>
    public static SearchFilter Create(
        Guid? categoryId = null,
        Guid? userId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int? minDuration = null,
        int? maxDuration = null,
        IReadOnlyList<string>? tags = null)
    {
        return new SearchFilter(
            categoryId,
            userId,
            startTime,
            endTime,
            minDuration,
            maxDuration,
            tags);
    }
    
    /// <summary>
    /// 创建分类过滤器
    /// </summary>
    public static SearchFilter ByCategory(Guid categoryId)
    {
        return new SearchFilter(categoryId, null, null, null, null, null, null);
    }
    
    /// <summary>
    /// 创建UP主过滤器
    /// </summary>
    public static SearchFilter ByUser(Guid userId)
    {
        return new SearchFilter(null, userId, null, null, null, null, null);
    }
    
    /// <summary>
    /// 创建时间范围过滤器
    /// </summary>
    public static SearchFilter ByTimeRange(DateTime startTime, DateTime endTime)
    {
        return new SearchFilter(null, null, startTime, endTime, null, null, null);
    }
    
    /// <summary>
    /// 创建时长范围过滤器
    /// </summary>
    public static SearchFilter ByDurationRange(int minDuration, int maxDuration)
    {
        return new SearchFilter(null, null, null, null, minDuration, maxDuration, null);
    }
    
    /// <summary>
    /// 创建标签过滤器
    /// </summary>
    public static SearchFilter ByTags(IReadOnlyList<string> tags)
    {
        return new SearchFilter(null, null, null, null, null, null, tags);
    }
    
    /// <summary>
    /// 创建空过滤器
    /// </summary>
    public static SearchFilter Empty()
    {
        return new SearchFilter(null, null, null, null, null, null, null);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否空过滤器
    /// </summary>
    public bool IsEmpty()
    {
        return CategoryId == null &&
               UserId == null &&
               StartTime == null &&
               EndTime == null &&
               MinDuration == null &&
               MaxDuration == null &&
               (Tags == null || Tags.Count == 0);
    }
    
    /// <summary>
    /// 是否有分类过滤
    /// </summary>
    public bool HasCategoryFilter()
    {
        return CategoryId.HasValue;
    }
    
    /// <summary>
    /// 是否有用户过滤
    /// </summary>
    public bool HasUserFilter()
    {
        return UserId.HasValue;
    }
    
    /// <summary>
    /// 是否有时间过滤
    /// </summary>
    public bool HasTimeFilter()
    {
        return StartTime.HasValue || EndTime.HasValue;
    }
    
    /// <summary>
    /// 是否有时长过滤
    /// </summary>
    public bool HasDurationFilter()
    {
        return MinDuration.HasValue || MaxDuration.HasValue;
    }
    
    /// <summary>
    /// 是否有标签过滤
    /// </summary>
    public bool HasTagFilter()
    {
        return Tags != null && Tags.Count > 0;
    }
    
    /// <summary>
    /// 合并过滤器
    /// </summary>
    public SearchFilter Merge(SearchFilter other)
    {
        return new SearchFilter(
            CategoryId ?? other.CategoryId,
            UserId ?? other.UserId,
            StartTime ?? other.StartTime,
            EndTime ?? other.EndTime,
            MinDuration ?? other.MinDuration,
            MaxDuration ?? other.MaxDuration,
            Tags ?? other.Tags);
    }
    
    /// <summary>
    /// 转换为Elasticsearch过滤DSL
    /// </summary>
    public object ToElasticsearchFilter()
    {
        var filters = new List<object>();
        
        if (HasCategoryFilter())
        {
            filters.Add(new Dictionary<string, object>
            {
                ["term"] = new Dictionary<string, object>
                {
                    ["categoryId"] = CategoryId.Value.ToString()
                }
            });
        }
        
        if (HasUserFilter())
        {
            filters.Add(new Dictionary<string, object>
            {
                ["term"] = new Dictionary<string, object>
                {
                    ["userId"] = UserId.Value.ToString()
                }
            });
        }
        
        if (HasTimeFilter())
        {
            var rangeFilter = new Dictionary<string, object>
            {
                ["range"] = new Dictionary<string, object>
                {
                    ["publishTime"] = new Dictionary<string, object>()
                }
            };
            
            var timeRange = (Dictionary<string, object>)((Dictionary<string, object>)rangeFilter["range"])["publishTime"];
            
            if (StartTime.HasValue)
                timeRange["gte"] = StartTime.Value.ToString("o");
            if (EndTime.HasValue)
                timeRange["lte"] = EndTime.Value.ToString("o");
            
            filters.Add(rangeFilter);
        }
        
        if (HasDurationFilter())
        {
            var rangeFilter = new Dictionary<string, object>
            {
                ["range"] = new Dictionary<string, object>
                {
                    ["duration"] = new Dictionary<string, object>()
                }
            };
            
            var durationRange = (Dictionary<string, object>)((Dictionary<string, object>)rangeFilter["range"])["duration"];
            
            if (MinDuration.HasValue)
                durationRange["gte"] = MinDuration.Value;
            if (MaxDuration.HasValue)
                durationRange["lte"] = MaxDuration.Value;
            
            filters.Add(rangeFilter);
        }
        
        if (HasTagFilter())
        {
            filters.Add(new Dictionary<string, object>
            {
                ["terms"] = new Dictionary<string, object>
                {
                    ["tags"] = Tags
                }
            });
        }
        
        if (filters.Count == 0)
            return null;
        
        return new Dictionary<string, object>
        {
            ["bool"] = new Dictionary<string, object>
            {
                ["filter"] = filters
            }
        };
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CategoryId ?? Guid.Empty;
        yield return UserId ?? Guid.Empty;
    }
}
```

---

### 4.3 SearchSort（搜索排序值对象）

```csharp
/// <summary>
/// 搜索排序值对象
/// </summary>
public class SearchSort : ValueObject
{
    // ========== 属性 ==========
    public SearchSortType Type { get; }         // 排序类型
    public bool IsDescending { get; }           // 是否降序
    
    // ========== 构造函数 ==========
    private SearchSort(SearchSortType type, bool isDescending = true)
    {
        Type = type;
        IsDescending = isDescending;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索排序
    /// </summary>
    public static SearchSort Create(SearchSortType type, bool isDescending = true)
    {
        return new SearchSort(type, isDescending);
    }
    
    /// <summary>
    /// 创建默认排序（相关性）
    /// </summary>
    public static SearchSort Default()
    {
        return new SearchSort(SearchSortType.Relevance, false);
    }
    
    /// <summary>
    /// 创建播放量排序
    /// </summary>
    public static SearchSort ByViewCount()
    {
        return new SearchSort(SearchSortType.ViewCount, true);
    }
    
    /// <summary>
    /// 创建发布时间排序
    /// </summary>
    public static SearchSort ByPublishTime()
    {
        return new SearchSort(SearchSortType.PublishTime, true);
    }
    
    /// <summary>
    /// 创建点赞数排序
    /// </summary>
    public static SearchSort ByLikeCount()
    {
        return new SearchSort(SearchSortType.LikeCount, true);
    }
    
    /// <summary>
    /// 创建收藏数排序
    /// </summary>
    public static SearchSort ByFavoriteCount()
    {
        return new SearchSort(SearchSortType.FavoriteCount, true);
    }
    
    /// <summary>
    /// 创建时长排序
    /// </summary>
    public static SearchSort ByDuration(bool ascending = false)
    {
        return new SearchSort(SearchSortType.Duration, !ascending);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否相关性排序
    /// </summary>
    public bool IsRelevanceSort()
    {
        return Type == SearchSortType.Relevance;
    }
    
    /// <summary>
    /// 是否播放量排序
    /// </summary>
    public bool IsViewCountSort()
    {
        return Type == SearchSortType.ViewCount;
    }
    
    /// <summary>
    /// 是否时间排序
    /// </summary>
    public bool IsTimeSort()
    {
        return Type == SearchSortType.PublishTime;
    }
    
    /// <summary>
    /// 是否互动排序（点赞/收藏）
    /// </summary>
    public bool IsInteractionSort()
    {
        return Type == SearchSortType.LikeCount || Type == SearchSortType.FavoriteCount;
    }
    
    /// <summary>
    /// 转换为Elasticsearch排序DSL
    /// </summary>
    public object ToElasticsearchSort()
    {
        if (IsRelevanceSort())
            return new List<object>(); // 空排序，使用相关性
        
        var sortField = Type switch
        {
            SearchSortType.ViewCount => "viewCount",
            SearchSortType.PublishTime => "publishTime",
            SearchSortType.LikeCount => "likeCount",
            SearchSortType.FavoriteCount => "favoriteCount",
            SearchSortType.Duration => "duration",
            _ => "_score"
        };
        
        return new List<object>
        {
            new Dictionary<string, object>
            {
                [sortField] = new Dictionary<string, object>
                {
                    ["order"] = IsDescending ? "desc" : "asc"
                }
            }
        };
    }
    
    /// <summary>
    /// 获取排序显示名称
    /// </summary>
    public string GetDisplayName()
    {
        return Type switch
        {
            SearchSortType.Relevance => "综合排序",
            SearchSortType.ViewCount => "最多播放",
            SearchSortType.PublishTime => "最新发布",
            SearchSortType.LikeCount => "最多点赞",
            SearchSortType.FavoriteCount => "最多收藏",
            SearchSortType.Duration => IsDescending ? "最长时长" : "最短时长",
            _ => "默认排序"
        };
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return IsDescending;
    }
}

/// <summary>
/// 搜索排序类型枚举
/// </summary>
public enum SearchSortType
{
    Relevance = 0,      // 相关性（默认）
    ViewCount = 1,      // 播放量
    PublishTime = 2,    // 发布时间
    LikeCount = 3,      // 点赞数
    FavoriteCount = 4,  // 收藏数
    Duration = 5        // 时长
}
```

---

### 4.4 SearchHighlight（搜索高亮值对象）

```csharp
/// <summary>
/// 搜索高亮值对象
/// </summary>
public class SearchHighlight : ValueObject
{
    // ========== 属性 ==========
    public string? TitleHighlight { get; }      // 标题高亮
    public string? DescriptionHighlight { get; } // 描述高亮
    public string? NicknameHighlight { get; }   // 用户昵称高亮
    
    // ========== 构造函数 ==========
    private SearchHighlight(
        string? titleHighlight,
        string? descriptionHighlight,
        string? nicknameHighlight)
    {
        TitleHighlight = titleHighlight;
        DescriptionHighlight = descriptionHighlight;
        NicknameHighlight = nicknameHighlight;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索高亮
    /// </summary>
    public static SearchHighlight Create(
        string? titleHighlight = null,
        string? descriptionHighlight = null,
        string? nicknameHighlight = null)
    {
        return new SearchHighlight(titleHighlight, descriptionHighlight, nicknameHighlight);
    }
    
    /// <summary>
    /// 从Elasticsearch高亮结果创建
    /// </summary>
    public static SearchHighlight FromElasticsearch(Dictionary<string, List<string>> highlights)
    {
        string? title = null;
        string? description = null;
        string? nickname = null;
        
        if (highlights.TryGetValue("title", out var titleHighlights) && titleHighlights.Count > 0)
            title = titleHighlights[0];
        
        if (highlights.TryGetValue("description", out var descHighlights) && descHighlights.Count > 0)
            description = descHighlights[0];
        
        if (highlights.TryGetValue("nickname", out var nickHighlights) && nickHighlights.Count > 0)
            nickname = nickHighlights[0];
        
        return new SearchHighlight(title, description, nickname);
    }
    
    /// <summary>
    /// 创建空高亮
    /// </summary>
    public static SearchHighlight Empty()
    {
        return new SearchHighlight(null, null, null);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 是否有高亮
    /// </summary>
    public bool HasHighlight()
    {
        return TitleHighlight != null ||
               DescriptionHighlight != null ||
               NicknameHighlight != null;
    }
    
    /// <summary>
    /// 是否标题高亮
    /// </summary>
    public bool HasTitleHighlight()
    {
        return TitleHighlight != null;
    }
    
    /// <summary>
    /// 是否描述高亮
    /// </summary>
    public bool HasDescriptionHighlight()
    {
        return DescriptionHighlight != null;
    }
    
    /// <summary>
    /// 获取主要高亮（优先标题）
    /// </summary>
    public string? GetPrimaryHighlight()
    {
        return TitleHighlight ?? DescriptionHighlight ?? NicknameHighlight;
    }
    
    /// <summary>
    /// 获取显示标题（使用高亮或原标题）
    /// </summary>
    public string GetDisplayTitle(string originalTitle)
    {
        return TitleHighlight ?? originalTitle;
    }
    
    /// <summary>
    /// 获取显示描述（使用高亮或截取描述）
    /// </summary>
    public string GetDisplayDescription(string originalDescription, int maxLength = 100)
    {
        if (DescriptionHighlight != null)
            return DescriptionHighlight;
        
        if (originalDescription.Length <= maxLength)
            return originalDescription;
        
        return originalDescription.Substring(0, maxLength) + "...";
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return TitleHighlight ?? string.Empty;
        yield return DescriptionHighlight ?? string.Empty;
        yield return NicknameHighlight ?? string.Empty;
    }
}
```

---

### 4.5 SearchScore（搜索得分值对象）

```csharp
/// <summary>
/// 搜索得分值对象
/// </summary>
public class SearchScore : ValueObject
{
    // ========== 属性 ==========
    public double ElasticsearchScore { get; }   // Elasticsearch得分
    public double RelevanceScore { get; }       // 相关性得分（标准化）
    public double PopularityScore { get; }      // 热度得分
    public double FinalScore { get; }           // 最终得分（综合）
    
    // ========== 构造函数 ==========
    private SearchScore(
        double elasticsearchScore,
        double relevanceScore,
        double popularityScore,
        double finalScore)
    {
        ElasticsearchScore = elasticsearchScore;
        RelevanceScore = relevanceScore;
        PopularityScore = popularityScore;
        FinalScore = finalScore;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索得分
    /// </summary>
    public static SearchScore Create(
        double elasticsearchScore,
        long viewCount,
        long likeCount,
        long favoriteCount)
    {
        // 标准化相关性得分（0-1）
        var relevanceScore = NormalizeScore(elasticsearchScore);
        
        // 计算热度得分（0-1）
        var popularityScore = CalculatePopularityScore(viewCount, likeCount, favoriteCount);
        
        // 综合得分：相关性70% + 热度30%
        var finalScore = relevanceScore * 0.7 + popularityScore * 0.3;
        
        return new SearchScore(elasticsearchScore, relevanceScore, popularityScore, finalScore);
    }
    
    /// <summary>
    /// 创建仅相关性得分（无热度数据）
    /// </summary>
    public static SearchScore CreateRelevanceOnly(double elasticsearchScore)
    {
        var relevanceScore = NormalizeScore(elasticsearchScore);
        return new SearchScore(elasticsearchScore, relevanceScore, 0, relevanceScore);
    }
    
    /// <summary>
    /// 创建最低得分
    /// </summary>
    public static SearchScore Min()
    {
        return new SearchScore(0, 0, 0, 0);
    }
    
    /// <summary>
    /// 创建最高得分
    /// </summary>
    public static SearchScore Max()
    {
        return new SearchScore(100, 1, 1, 1);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 标准化得分（将ES得分转为0-1）
    /// </summary>
    private static double NormalizeScore(double score)
    {
        // 简单标准化：假设ES得分范围0-100
        return Math.Clamp(score / 100, 0, 1);
    }
    
    /// <summary>
    /// 计算热度得分
    /// </summary>
    private static double CalculatePopularityScore(long viewCount, long likeCount, long favoriteCount)
    {
        // 热度公式：播放量权重 + 互动权重
        var viewScore = Math.Log10(viewCount + 1) / 7; // 播放量对数标准化
        var interactionScore = Math.Log10(likeCount + favoriteCount + 1) / 5;
        
        return Math.Clamp(viewScore * 0.4 + interactionScore * 0.6, 0, 1);
    }
    
    /// <summary>
    /// 是否高相关性
    /// </summary>
    public bool IsHighRelevance()
    {
        return RelevanceScore > 0.7;
    }
    
    /// <summary>
    /// 是否低相关性
    /// </summary>
    public bool IsLowRelevance()
    {
        return RelevanceScore < 0.3;
    }
    
    /// <summary>
    /// 是否高热度
    /// </summary>
    public bool IsHighPopularity()
    {
        return PopularityScore > 0.7;
    }
    
    /// <summary>
    /// 是否高得分（综合）
    /// </summary>
    public bool IsHighScore()
    {
        return FinalScore > 0.7;
    }
    
    /// <summary>
    /// 格式化得分显示
    /// </summary>
    public string FormatScore()
    {
        return $"{FinalScore:P0}";
    }
    
    // ========== 值对象相等性 ==========
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return FinalScore;
    }
}
```

---

## 5. 聚合根设计：SearchResult（搜索结果聚合）

### 5.1 SearchResult（搜索结果聚合根）

```csharp
/// <summary>
/// 搜索结果聚合根
/// </summary>
public class SearchResult : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid QueryId { get; private set; }    // 搜索查询ID
    public Guid VideoId { get; private set; }    // 视频ID
    public Guid UserId { get; private set; }     // UP主ID
    public Guid? CategoryId { get; private set; } // 分类ID
    
    // ========== 值对象 ==========
    private SearchHighlight _highlight;          // 高亮信息
    private SearchScore _score;                  // 搜索得分
    
    // ========== 属性 ==========
    public string Title { get; private set; }    // 视频标题
    public string Description { get; private set; } // 视频描述
    public string CoverUrl { get; private set; } // 封面图
    public string UserNickname { get; private set; } // UP主昵称
    public long Duration { get; private set; }   // 视频时长
    public long ViewCount { get; private set; }  // 播放量
    public long LikeCount { get; private set; }  // 点赞数
    public long FavoriteCount { get; private set; } // 收藏数
    public DateTime PublishTime { get; private set; } // 发布时间
    
    public int Position { get; private set; }    // 结果位置（排序）
    
    // ========== 时间戳 ==========
    public DateTime RetrievedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private SearchResult() { }
    
    private SearchResult(
        Guid queryId,
        Guid videoId,
        Guid userId,
        string title,
        string description,
        string coverUrl,
        string userNickname,
        long duration,
        long viewCount,
        long likeCount,
        long favoriteCount,
        DateTime publishTime,
        Guid? categoryId,
        SearchHighlight highlight,
        SearchScore score,
        int position) : base(GuidGenerator.Create())
    {
        QueryId = queryId;
        VideoId = videoId;
        UserId = userId;
        Title = title;
        Description = description;
        CoverUrl = coverUrl;
        UserNickname = userNickname;
        Duration = duration;
        ViewCount = viewCount;
        LikeCount = likeCount;
        FavoriteCount = favoriteCount;
        PublishTime = publishTime;
        CategoryId = categoryId;
        _highlight = highlight;
        _score = score;
        Position = position;
        RetrievedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索结果
    /// </summary>
    public static SearchResult Create(
        Guid queryId,
        Guid videoId,
        Guid userId,
        string title,
        string description,
        string coverUrl,
        string userNickname,
        long duration,
        long viewCount,
        long likeCount,
        long favoriteCount,
        DateTime publishTime,
        Guid? categoryId,
        SearchHighlight highlight,
        SearchScore score,
        int position)
    {
        return new SearchResult(
            queryId,
            videoId,
            userId,
            title,
            description,
            coverUrl,
            userNickname,
            duration,
            viewCount,
            likeCount,
            favoriteCount,
            publishTime,
            categoryId,
            highlight,
            score,
            position);
    }
    
    /// <summary>
    /// 从Elasticsearch文档创建
    /// </summary>
    public static SearchResult FromElasticsearch(
        Guid queryId,
        VideoSearchDoc doc,
        Dictionary<string, List<string>>? highlights,
        double elasticsearchScore,
        int position)
    {
        var highlight = highlights != null ?
            SearchHighlight.FromElasticsearch(highlights) :
            SearchHighlight.Empty();
        
        var score = SearchScore.Create(
            elasticsearchScore,
            doc.ViewCount,
            doc.LikeCount,
            doc.FavoriteCount);
        
        return new SearchResult(
            queryId,
            doc.VideoId,
            doc.UserId,
            doc.Title,
            doc.Description,
            doc.CoverUrl,
            doc.Nickname,
            doc.Duration,
            doc.ViewCount,
            doc.LikeCount,
            doc.FavoriteCount,
            doc.PublishTime,
            doc.CategoryId,
            highlight,
            score,
            position);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 更新高亮信息
    /// </summary>
    public void UpdateHighlight(SearchHighlight newHighlight)
    {
        _highlight = newHighlight;
    }
    
    /// <summary>
    /// 更新得分
    /// </summary>
    public void UpdateScore(SearchScore newScore)
    {
        _score = newScore;
    }
    
    /// <summary>
    /// 更新位置
    /// </summary>
    public void UpdatePosition(int newPosition)
    {
        Position = newPosition;
    }
    
    // ========== 查询方法 ==========
    
    public SearchHighlight GetHighlight() => _highlight;
    public SearchScore GetScore() => _score;
    
    /// <summary>
    /// 获取显示标题
    /// </summary>
    public string GetDisplayTitle()
    {
        return _highlight.GetDisplayTitle(Title);
    }
    
    /// <summary>
    /// 获取显示描述
    /// </summary>
    public string GetDisplayDescription()
    {
        return _highlight.GetDisplayDescription(Description);
    }
    
    /// <summary>
    /// 是否有高亮
    /// </summary>
    public bool HasHighlight => _highlight.HasHighlight();
    
    /// <summary>
    /// 是否高相关性
    /// </summary>
    public bool IsHighRelevance => _score.IsHighRelevance();
}

/// <summary>
/// Elasticsearch视频搜索文档（DTO）
/// </summary>
public class VideoSearchDoc
{
    public Guid VideoId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string CoverUrl { get; set; }
    public Guid UserId { get; set; }
    public string Nickname { get; set; }
    public Guid? CategoryId { get; set; }
    public long Duration { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long FavoriteCount { get; set; }
    public DateTime PublishTime { get; set; }
    public int Status { get; set; }
    public List<string>? Tags { get; set; }
}
```

---

## 6. 聚合根设计：SearchIndex（索引聚合）

### 6.1 SearchIndex（搜索索引聚合根）

```csharp
/// <summary>
/// 搜索索引聚合根（Elasticsearch索引管理）
/// </summary>
public class SearchIndex : AggregateRoot<Guid>
{
    // ========== 属性 ==========
    public string IndexName { get; private set; } // 索引名称
    public IndexType Type { get; private set; }  // 索引类型
    public int DocumentCount { get; private set; } // 文档数量
    public IndexStatus Status { get; private set; } // 索引状态
    
    public DateTime CreatedAt { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    
    // ========== 构造函数 ==========
    private SearchIndex() { }
    
    private SearchIndex(string indexName, IndexType type) : base(GuidGenerator.Create())
    {
        IndexName = indexName;
        Type = type;
        DocumentCount = 0;
        Status = IndexStatus.Created;
        CreatedAt = DateTime.UtcNow;
        LastSyncedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建视频索引
    /// </summary>
    public static SearchIndex CreateVideoIndex()
    {
        return new SearchIndex("videos", IndexType.Video);
    }
    
    /// <summary>
    /// 创建用户索引
    /// </summary>
    public static SearchIndex CreateUserIndex()
    {
        return new SearchIndex("users", IndexType.User);
    }
    
    /// <summary>
    /// 创建弹幕索引
    /// </summary>
    public static SearchIndex CreateDanmakuIndex()
    {
        return new SearchIndex("danmaku", IndexType.Danmaku);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 标记同步完成
    /// </summary>
    public void MarkSynced(int documentCount)
    {
        DocumentCount = documentCount;
        Status = IndexStatus.Synced;
        LastSyncedAt = DateTime.UtcNow;
        
        AddDomainEvent(new SearchIndexSyncedEvent(IndexName, documentCount));
    }
    
    /// <summary>
    /// 标记同步失败
    /// </summary>
    public void MarkSyncFailed()
    {
        Status = IndexStatus.Failed;
    }
    
    /// <summary>
    /// 增加文档计数
    /// </summary>
    public void IncrementDocumentCount()
    {
        DocumentCount++;
        LastSyncedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 减少文档计数
    /// </summary>
    public void DecrementDocumentCount()
    {
        DocumentCount = Math.Max(0, DocumentCount - 1);
    }
    
    /// <summary>
    /// 是否需要同步
    /// </summary>
    public bool NeedSync()
    {
        return DateTime.UtcNow - LastSyncedAt > TimeSpan.FromMinutes(5);
    }
    
    /// <summary>
    /// 是否活跃
    /// </summary>
    public bool IsActive => Status == IndexStatus.Synced;
}

/// <summary>
/// 索引类型枚举
/// </summary>
public enum IndexType
{
    Video = 1,      // 视频索引
    User = 2,       // 用户索引
    Danmaku = 3     // 弹幕索引
}

/// <summary>
/// 索引状态枚举
/// </summary>
public enum IndexStatus
{
    Created = 1,    // 已创建
    Synced = 2,     // 已同步
    Failed = 3      // 同步失败
}
```

---

## 7. 聚合根设计：SearchHistory（搜索历史聚合）

### 7.1 SearchHistory（搜索历史聚合根）

```csharp
/// <summary>
/// 搜索历史聚合根
/// </summary>
public class SearchHistory : AggregateRoot<Guid>
{
    // ========== 外部引用 ==========
    public Guid UserId { get; private set; }     // 用户ID
    
    // ========== 属性 ==========
    public string Keyword { get; private set; }  // 搜索关键词
    public int SearchCount { get; private set; } // 搜索次数
    public DateTime FirstSearchedAt { get; private set; } // 首次搜索时间
    public DateTime LastSearchedAt { get; private set; }  // 最后搜索时间
    
    // ========== 构造函数 ==========
    private SearchHistory() { }
    
    private SearchHistory(Guid userId, string keyword) : base(GuidGenerator.Create())
    {
        UserId = userId;
        Keyword = keyword;
        SearchCount = 1;
        FirstSearchedAt = DateTime.UtcNow;
        LastSearchedAt = DateTime.UtcNow;
    }
    
    // ========== 工厂方法 ==========
    
    /// <summary>
    /// 创建搜索历史
    /// </summary>
    public static SearchHistory Create(Guid userId, string keyword)
    {
        return new SearchHistory(userId, keyword);
    }
    
    // ========== 业务方法 ==========
    
    /// <summary>
    /// 增加搜索次数
    /// </summary>
    public void IncrementSearchCount()
    {
        SearchCount++;
        LastSearchedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// 清除历史
    /// </summary>
    public void Clear()
    {
        IsDeleted = true;
    }
    
    /// <summary>
    /// 是否热门搜索（搜索次数高）
    /// </summary>
    public bool IsHotSearch()
    {
        return SearchCount > 10;
    }
    
    /// <summary>
    /// 是否最近搜索（24小时内）
    /// </summary>
    public bool IsRecentSearch()
    {
        return DateTime.UtcNow - LastSearchedAt < TimeSpan.FromHours(24);
    }
}
```

---

## 8. 领域服务设计

### 8.1 ISearchService（搜索领域服务）

```csharp
/// <summary>
/// 搜索领域服务接口
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// 执行搜索
    /// </summary>
    Task<SearchResponse> SearchAsync(SearchQuery query);
    
    /// <summary>
    /// 获取搜索建议
    /// </summary>
    Task<IReadOnlyList<string>> GetSuggestionsAsync(string keyword, int limit = 10);
    
    /// <summary>
    /// 获取热门搜索关键词
    /// </summary>
    Task<IReadOnlyList<string>> GetHotKeywordsAsync(int limit = 10);
    
    /// <summary>
    /// 获取用户搜索历史
    /// </summary>
    Task<IReadOnlyList<SearchHistory>> GetUserHistoryAsync(Guid userId, int limit = 10);
    
    /// <summary>
    /// 清除用户搜索历史
    /// </summary>
    Task ClearUserHistoryAsync(Guid userId);
}

/// <summary>
/// 搜索响应
/// </summary>
public class SearchResponse
{
    public Guid QueryId { get; set; }
    public string Keyword { get; set; }
    public int TotalCount { get; set; }
    public int TookMilliseconds { get; set; }
    public List<SearchResult> Results { get; set; } = new();
    public bool HasMore { get; set; }
}
```

---

### 8.2 ISearchIndexService（索引管理领域服务）

```csharp
/// <summary>
/// 搜索索引管理领域服务接口
/// 业务规则：实时同步（用户需求）
/// </summary>
public interface ISearchIndexService
{
    /// <summary>
    /// 同步视频到索引
    /// </summary>
    Task SyncVideoAsync(Guid videoId, VideoSearchDoc doc);
    
    /// <summary>
    /// 删除视频索引
    /// </summary>
    Task DeleteVideoAsync(Guid videoId);
    
    /// <summary>
    /// 批量同步视频
    /// </summary>
    Task<int> BulkSyncVideosAsync(IReadOnlyList<VideoSearchDoc> docs);
    
    /// <summary>
    /// 同步用户到索引
    /// </summary>
    Task SyncUserAsync(Guid userId, UserSearchDoc doc);
    
    /// <summary>
    /// 删除用户索引
    /// </summary>
    Task DeleteUserAsync(Guid userId);
    
    /// <summary>
    /// 全量重建索引
    /// </summary>
    Task RebuildIndexAsync(IndexType type);
    
    /// <summary>
    /// 获取索引状态
    /// </summary>
    Task<SearchIndex> GetIndexStatusAsync(IndexType type);
    
    /// <summary>
    /// 实时同步（监听事件触发）
    /// </summary>
    Task RealtimeSyncAsync(Guid entityId, IndexType type, SyncAction action);
}

/// <summary>
/// 用户搜索文档
/// </summary>
public class UserSearchDoc
{
    public Guid UserId { get; set; }
    public string Nickname { get; set; }
    public string? Avatar { get; set; }
    public string? Signature { get; set; }
    public int Level { get; set; }
}

/// <summary>
/// 同步动作枚举
/// </summary>
public enum SyncAction
{
    Create = 1,     // 创建
    Update = 2,     // 更新
    Delete = 3      // 删除
}
```

---

### 8.3 ISearchAnalyzerService（搜索分析领域服务）

```csharp
/// <summary>
/// 搜索分析领域服务接口
/// </summary>
public interface ISearchAnalyzerService
{
    /// <summary>
    /// 分析关键词热度
    /// </summary>
    Task<KeywordAnalysis> AnalyzeKeywordAsync(string keyword);
    
    /// <summary>
    /// 获取搜索趋势
    /// </summary>
    Task<IReadOnlyList<(string Keyword, int Count)>> GetSearchTrendsAsync(int days = 7);
    
    /// <summary>
    /// 获取无结果关键词（需要优化）
    /// </summary>
    Task<IReadOnlyList<string>> GetNoResultKeywordsAsync(int limit = 50);
    
    /// <summary>
    /// 获取搜索点击率
    /// </summary>
    Task<double> GetClickRateAsync(string keyword);
}

/// <summary>
/// 关键词分析结果
/// </summary>
public class KeywordAnalysis
{
    public string Keyword { get; set; }
    public int SearchCount { get; set; }
    public int ResultCount { get; set; }
    public double ClickRate { get; set; }
    public List<string> RelatedKeywords { get; set; } = new();
}
```

---

## 9. 仓储接口设计

### 9.1 ISearchQueryRepository

```csharp
/// <summary>
/// 搜索查询仓储接口
/// </summary>
public interface ISearchQueryRepository : IRepository<SearchQuery, Guid>
{
    /// <summary>
    /// 获取用户最近查询
    /// </summary>
    Task<List<SearchQuery>> GetRecentByUserAsync(Guid userId, int limit = 10);
    
    /// <summary>
    /// 获取热门查询
    /// </summary>
    Task<List<SearchQuery>> GetHotQueriesAsync(int limit = 10);
}
```

---

### 9.2 ISearchResultRepository

```csharp
/// <summary>
/// 搜索结果仓储接口
/// </summary>
public interface ISearchResultRepository : IRepository<SearchResult, Guid>
{
    /// <summary>
    /// 根据查询ID获取结果
    /// </summary>
    Task<List<SearchResult>> GetByQueryIdAsync(Guid queryId);
    
    /// <summary>
    /// 批量插入结果
    /// </summary>
    Task<int> BulkInsertAsync(IReadOnlyList<SearchResult> results);
}
```

---

### 9.3 ISearchHistoryRepository

```csharp
/// <summary>
/// 搜索历史仓储接口
/// </summary>
public interface ISearchHistoryRepository : IRepository<SearchHistory, Guid>
{
    /// <summary>
    /// 获取用户搜索历史
    /// </summary>
    Task<List<SearchHistory>> GetByUserIdAsync(Guid userId, int limit = 10);
    
    /// <summary>
    /// 根据关键词获取历史
    /// </summary>
    Task<SearchHistory?> GetByKeywordAsync(Guid userId, string keyword);
    
    /// <summary>
    /// 获取热门搜索关键词
    /// </summary>
    Task<List<SearchHistory>> GetHotKeywordsAsync(int limit = 10);
    
    /// <summary>
    /// 清除用户历史
    /// </summary>
    Task<int> ClearByUserIdAsync(Guid userId);
}
```

---

### 9.4 ISearchIndexRepository

```csharp
/// <summary>
/// 搜索索引仓储接口
/// </summary>
public interface ISearchIndexRepository : IRepository<SearchIndex, Guid>
{
    /// <summary>
    /// 根据索引名称获取
    /// </summary>
    Task<SearchIndex?> GetByNameAsync(string indexName);
    
    /// <summary>
    /// 根据类型获取
    /// </summary>
    Task<SearchIndex?> GetByTypeAsync(IndexType type);
}
```

---

## 10. 频域事件设计

### 10.1 频域事件列表

```csharp
/// <summary>
/// 搜索执行事件
/// </summary>
public class SearchExecutedEvent
{
    public Guid QueryId { get; }
    public Guid? UserId { get; }
    public string Keyword { get; }
    public int ResultCount { get; }
    public int TookMilliseconds { get; }
    public DateTime OccurredOn { get; }
    
    public SearchExecutedEvent(Guid queryId, Guid? userId, string keyword, int resultCount, int tookMs)
    {
        QueryId = queryId;
        UserId = userId;
        Keyword = keyword;
        ResultCount = resultCount;
        TookMilliseconds = tookMs;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 搜索索引同步事件
/// </summary>
public class SearchIndexSyncedEvent
{
    public string IndexName { get; }
    public int DocumentCount { get; }
    public DateTime OccurredOn { get; }
    
    public SearchIndexSyncedEvent(string indexName, int documentCount)
    {
        IndexName = indexName;
        DocumentCount = documentCount;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 搜索结果点击事件
/// </summary>
public class SearchResultClickedEvent
{
    public Guid ResultId { get; }
    public Guid QueryId { get; }
    public Guid VideoId { get; }
    public int Position { get; }
    public DateTime OccurredOn { get; }
    
    public SearchResultClickedEvent(Guid resultId, Guid queryId, Guid videoId, int position)
    {
        ResultId = resultId;
        QueryId = queryId;
        VideoId = videoId;
        Position = position;
        OccurredOn = DateTime.UtcNow;
    }
}

/// <summary>
/// 搜索历史清除事件
/// </summary>
public class SearchHistoryClearedEvent
{
    public Guid UserId { get; }
    public DateTime OccurredOn { get; }
    
    public SearchHistoryClearedEvent(Guid userId)
    {
        UserId = userId;
        OccurredOn = DateTime.UtcNow;
    }
}
```

---

## 11. 业务规则总结

### 11.1 搜索执行规则

| 规则 | 说明 |
|------|------|
| 索引引擎 | Elasticsearch |
| 分词器 | ik_max_word（中文） |
| 搜索字段 | title^3, description^1, nickname^2 |
| 高亮标签 | <em></em> |
| 实时同步 | 实时同步（用户需求） |

---

### 11.2 搜索排序规则

| 排序类型 | 说明 |
|---------|------|
| Relevance | 综合排序（相关性70% + 热度30%） |
| ViewCount | 播放量降序 |
| PublishTime | 发布时间降序 |
| LikeCount | 点赞数降序 |
| FavoriteCount | 收藏数降序 |
| Duration | 时长排序（可选升序/降序） |

---

### 11.3 搜索过滤规则

| 规则 | 说明 |
|------|------|
| 分类过滤 | term: categoryId |
| 用户过滤 | term: userId |
| 时间过滤 | range: publishTime |
| 时长过滤 | range: duration |
| 标签过滤 | terms: tags |

---

### 11.4 索引同步规则

| 规则 | 说明 |
|------|------|
| 同步方式 | 实时同步（用户需求） |
| 同步时机 | 视频发布/更新/删除后立即同步 |
| 同步延迟 | <100ms |
| 批量同步 | 支持批量同步视频 |
| 全量重建 | 支持手动触发全量重建 |

---

### 11.5 搜索建议规则

| 规则 | 说明 |
|------|------|
| 建议来源 | 用户历史 + 热门关键词 |
| 建议数量 | 最多10条 |
| 历史优先 | 用户历史优先显示 |
| 热门补充 | 热门关键词补充建议 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 搜索模块领域设计文档完成