# 推荐模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | RecommendService（推荐服务） |
| 服务地址 | http://recommend-service:5007 |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. 模块概述

### 2.1 服务职责

- 个性化推荐（基于用户观看历史、投币、点赞行为）
- 热门视频推荐（基于播放量、互动数据）
- 相关视频推荐（基于视频标签、分区）

### 2.2 推荐算法

- **协同过滤**：基于用户行为的相似度推荐
- **热门推荐**：基于播放量、点赞数、投币数排序
- **混合推荐**：协同过滤 + 热门推荐

---

## 3. API接口列表

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 获取个性化推荐 | GET | /api/recommend/videos/personalized | 需认证 |
| 2 | 获取热门推荐 | GET | /api/recommend/videos/hot | 公开 |

**总计：2个API接口**

---

## 4. 个性化推荐接口

### 4.1 获取个性化推荐

#### 基本信息

```
GET /api/recommend/videos/personalized?maxResultCount={int}
Authorization: Bearer {token}
权限：需认证（基于用户历史行为）
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| maxResultCount | int | ❌ | 返回数量（默认10） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "title": "推荐视频标题",
      "coverImageUrl": "...",
      "durationSeconds": 120,
      "userId": "...",
      "userName": "UP主",
      "userAvatar": "...",
      "userLevel": 5,
      "totalViews": 10000,
      "publishTime": "...",
      "recommendScore": 0.92,
      "recommendReason": "based_on_your_watch_history"
    }
  ],
  "totalCount": 100,
  "algorithm": "CollaborativeFiltering",
  "basedOn": {
    "watchHistory": true,
    "likeBehavior": true,
    "coinBehavior": true,
    "categoryPreference": true
  }
}
```

#### 说明

- **推荐算法**：基于用户的观看历史、点赞、投币行为计算相似度
- **推荐原因**：
  - `based_on_your_watch_history`: 基于观看历史
  - `based_on_your_likes`: 基于点赞行为
  - `based_on_your_coins`: 基于投币行为
  - `similar_to_category`: 基于分区偏好

---

## 5. 热门推荐接口

### 5.1 获取热门推荐

#### 基本信息

```
GET /api/recommend/videos/hot?categoryId={uuid}&maxResultCount={int}
权限：公开
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| categoryId | UUID | ❌ | 分区ID（可选，按分区筛选） |
| maxResultCount | int | ❌ | 返回数量（默认10） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "title": "热门视频标题",
      "coverImageUrl": "...",
      "durationSeconds": 120,
      "userName": "UP主",
      "totalViews": 100000,
      "totalLikes": 5000,
      "totalCoins": 2000,
      "publishTime": "...",
      "trend": "hot",
      "hotScore": 95
    }
  ],
  "totalCount": 50,
  "algorithm": "HotRanking",
  "rankingCriteria": {
    "viewCount": 0.4,
    "likeCount": 0.3,
    "coinCount": 0.3
  }
}
```

#### 说明

- **热门评分计算**：
  - 播放量权重：40%
  - 点赞数权重：30%
  - 投币数权重：30%

---

## 6. 接口性能指标

| 接口 | 平均响应时间 | QPS上限 | 说明 |
|------|------------|---------|------|
| 个性化推荐 | <100ms | 1000 | 需Redis缓存推荐结果 |
| 热门推荐 | <50ms | 2000 | 需Redis缓存热门列表 |

---

## 7. 接口依赖关系

### 7.1 依赖其他服务

| 服务 | 依赖数据 | 用途 |
|------|---------|------|
| UserService | 用户观看历史 | 计算个性化推荐 |
| InteractionService | 点赞、投币记录 | 计算用户偏好 |
| VideoService | 视频统计数据 | 计算热门评分 |
| Redis | 缓存推荐结果 | 提高响应速度 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 推荐模块API文档完成