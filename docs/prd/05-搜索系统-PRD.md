# 搜索系统产品需求文档 (PRD)

## 1. 模块概述

### 1.2 核心功能
- 全文搜索（标题、描述、标签）
- 搜索建议（自动补全）
- 搜索结果排序
- 多维度筛选（分区、时长、发布时间）
- 搜索历史与热门

---

## 2. 功能详细设计

### 2.1 搜索功能

#### 2.1.1 搜索范围
| 范围 | 字段 | 权重 |
|------|------|------|
| 标题 | title | 10 |
| 描述 | description | 5 |
| 标签 | tags | 3 |
| UP主 | user.username | 2 |

#### 2.1.2 Elasticsearch索引
```json
{
  "mappings": {
    "properties": {
      "videoId": { "type": "keyword" },
      "title": { 
        "type": "text",
        "analyzer": "ik_max_word",
        "search_analyzer": "ik_smart",
        "fields": {
          "keyword": { "type": "keyword" }
        }
      },
      "description": { 
        "type": "text",
        "analyzer": "ik_max_word"
      },
      "tags": { 
        "type": "keyword"
      },
      "categoryId": { "type": "keyword" },
      "userId": { "type": "keyword" },
      "username": { 
        "type": "text",
        "analyzer": "ik_max_word"
      },
      "duration": { "type": "integer" },
      "viewCount": { "type": "long" },
      "publishTime": { "type": "date" },
      "status": { "type": "integer" }
    }
  },
  "settings": {
    "analysis": {
      "analyzer": {
        "ik_analyzer": {
          "type": "custom",
          "tokenizer": "ik_max_word"
        }
      }
    }
  }
}
```

### 2.2 搜索建议
```csharp
// 搜索建议实现
public class SearchSuggestionService
{
    public async Task<List<string>> GetSuggestionsAsync(string keyword, int limit = 10)
    {
        // 1. 从Redis获取热门搜索建议
        var hotSuggestions = await _redis.SortedSetRangeByRankAsync(
            $"search:suggestions:{keyword}", 
            0, limit);
        
        if (hotSuggestions.Any())
            return hotSuggestions.Select(s => s.ToString()).ToList();
        
        // 2. Elasticsearch自动补全
        var response = await _esClient.SearchAsync<VideoSearchDoc>(s => s
            .Suggest(su => su
                .Completion("suggestions", c => c
                    .Field("title.suggest")
                    .Prefix(keyword)
                    .Size(limit)
                )
            )
        );
        
        return response.Suggest["suggestions"]
            .SelectMany(s => s.Options)
            .Select(o => o.Text)
            .ToList();
    }
}
```

### 2.3 搜索API
```http
GET /api/search?q={keyword}&category={id}&duration=short&sort=view&page=1

Response:
{
  "query": "搜索关键词",
  "totalCount": 1580,
  "took": 45,  // 搜索耗时（ms）
  "items": [
    {
      "videoId": "uuid",
      "title": "视频标题",
      "highlight": "视频<em>标题</em>",  // 高亮
      "description": "...",
      "duration": 300,
      "viewCount": 10000,
      "user": { "id": "uuid", "username": "UP主" }
    }
  ]
}
```

---

## 3. 数据库设计

```sql
CREATE TABLE "SearchHistories" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid REFERENCES "Users"("Id"),
    "Keyword" varchar(100) NOT NULL,
    "ResultCount" integer,
    "IsClicked" boolean DEFAULT false,
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE INDEX "IX_SearchHistories_UserId" ON "SearchHistories"("UserId");
CREATE INDEX "IX_SearchHistories_Keyword" ON "SearchHistories"("Keyword");
```

---

## 4. 技术架构

- **搜索引擎**: Elasticsearch 8.x
- **中文分词**: IK Analyzer
- **实时同步**: Canal + Kafka
- **缓存**: Redis（搜索结果缓存）

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0 + Elasticsearch
