# 07-互动系统详细PRD

## 1. 模块概述

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 投币上限 | 无限制 |
| 评论审核 | 先审后发（人工审核） |

---

## 2. 功能详细设计

### 2.1 点赞API

```http
POST /api/v1/interaction/like
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "targetType": "video",    // video/comment
  "targetId": "uuid",
  "isLike": true            // true=点赞, false=取消
}

Response:
{
  "code": 0,
  "data": {
    "isLiked": true,
    "likeCount": 1001
  }
}
```

---

### 2.2 投币API

```http
POST /api/v1/interaction/coin
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "videoId": "uuid",
  "count": 1                // 1或2
}

Response:
{
  "code": 0,
  "data": {
    "success": true,
    "coinCount": 100,           // UP主投币数
    "userBalance": 1000          // 用户B币余额
  }
}
```

---

### 2.3 评论API（先审后发）

```http
POST /api/v1/interaction/comment
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "videoId": "uuid",
  "content": "评论内容",
  "parentId": null
}

Response:
{
  "code": 0,
  "data": {
    "commentId": "uuid",
    "status": "pending",         // pending=待审核, approved=已发布
    "message": "评论已提交，等待审核"
  }
}
```

---

## 3. 数据库设计

```sql
CREATE TABLE "Likes" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "TargetId" uuid NOT NULL,
    "TargetType" integer NOT NULL,
    "CreationTime" timestamp with time zone NOT NULL,
    UNIQUE("UserId", "TargetId", "TargetType")
);

CREATE TABLE "Comments" (
    "Id" uuid PRIMARY KEY,
    "VideoId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "ParentId" uuid,
    "Content" varchar(1000) NOT NULL,
    "Status" integer DEFAULT 0,    -- 0=待审核,1=已发布,2=已拒绝
    "LikeCount" integer DEFAULT 0,
    "CreationTime" timestamp with time zone NOT NULL
);
```

---

**文档版本**: v2.0  
**状态**: ✅ 详细设计完成