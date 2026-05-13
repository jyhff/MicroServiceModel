# 搜索模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | SearchService（搜索服务） |
| 服务地址 | http://search-service:5006 |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. API接口列表

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 搜索视频 | GET | /api/search/videos | 公开 |
| 2 | 搜索建议 | GET | /api/search/suggestions | 公开 |
| 3 | 热搜榜单 | GET | /api/search/hot-search | 公开 |

**总计：3个API接口**

---

## 3. 搜索视频接口

### 基本信息

```
GET /api/search/videos?keyword={string}&categoryId={uuid}&maxResultCount={int}&skipCount={int}
权限：公开
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| keyword | string | ✅ | 搜索关键词 |
| categoryId | UUID | ❌ | 分区ID |
| maxResultCount | int | ❌ | 返回数量（默认20） |
| skipCount | int | ❌ | 跳过数量（默认0） |

#### 成功响应（200 OK）

```json
{
  "keyword": "游戏",
  "totalCount": 1000,
  "items": [
    {
      "id": "...",
      "title": "游戏教程视频",
      "description": "...",
      "coverImageUrl": "...",
      "durationSeconds": 120,
      "categoryId": "...",
      "userName": "UP主",
      "totalViews": 10000,
      "relevanceScore": 0.95
    }
  ],
  "searchTime": "...",
  "took": 50
}
```

---

## 4. 搜索建议接口

### 基本信息

```
GET /api/search/suggestions?keyword={string}
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "keyword": "游",
  "suggestions": ["游戏", "游戏教程", "游戏直播", "游戏解说"]
}
```

---

## 5. 热搜榜单接口

### 基本信息

```
GET /api/search/hot-search?maxResultCount={int}
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "keyword": "热门游戏",
      "searchCount": 100000,
      "trend": "up",
      "rank": 1
    }
  ],
  "updateTime": "..."
}
```

---

**文档版本**: v1.0  
**状态**: ✅ 搜索模块API文档完成