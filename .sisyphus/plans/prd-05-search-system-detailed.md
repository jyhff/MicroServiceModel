# 05-搜索系统详细PRD

## 1. 模块概述

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 索引更新 | 实时同步 |

---

## 2. 功能详细设计

### 2.1 搜索API

```http
GET /api/v1/search?q={keyword}&category={id}&sort=view&page=1&pageSize=20

Response:
{
  "code": 0,
  "data": {
    "query": "搜索关键词",
    "totalCount": 1580,
    "took": 45,
    "items": [
      {
        "id": "uuid",
        "title": "视频标题",
        "highlight": "视频<em>标题</em>",
        "description": "...",
        "cover": "http://...",
        "duration": 300,
        "viewCount": 10000,
        "user": {...}
      }
    ]
  }
}
```

---

### 2.2 Elasticsearch索引设计

```json
{
  "mappings": {
    "properties": {
      "videoId": { "type": "keyword" },
      "title": {
        "type": "text",
        "analyzer": "ik_max_word"
      },
      "description": { "type": "text", "analyzer": "ik_max_word" },
      "tags": { "type": "keyword" },
      "categoryId": { "type": "keyword" },
      "userId": { "type": "keyword" },
      "nickname": { "type": "text", "analyzer": "ik_max_word" },
      "viewCount": { "type": "long" },
      "publishTime": { "type": "date" },
      "status": { "type": "integer" }
    }
  }
}
```

---

### 2.3 实时同步实现

```csharp
public class VideoSearchSyncService : IHostedService
{
    public async Task SyncVideoToElasticsearch(Video video)
    {
        var doc = new VideoSearchDoc
        {
            VideoId = video.Id,
            Title = video.Title,
            Description = video.Description,
            Tags = video.Tags,
            CategoryId = video.CategoryId,
            UserId = video.UserId,
            Nickname = video.User.Nickname,
            ViewCount = video.ViewCount,
            PublishTime = video.PublishTime,
            Status = video.Status
        };
        
        await _elasticClient.IndexAsync(doc, i => i.Index("videos"));
    }
}
```

---

**文档版本**: v2.0  
**状态**: ✅ 详细设计完成