# 09-后台管理系统详细PRD

## 1. 模块概述

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 权限分级 | 仅超级管理员 |

---

## 2. 功能详细设计

### 2.1 数据统计API

```http
GET /api/v1/admin/dashboard/stats
Authorization: Bearer {adminToken}

Response:
{
  "code": 0,
  "data": {
    "dau": 125000,
    "mau": 2500000,
    "newUsersToday": 5800,
    "retention": {
      "d1": 45.5,
      "d7": 25.3,
      "d30": 18.2
    },
    "videoStats": {
      "dailyViews": 2500000,
      "dailyUploads": 8500,
      "avgWatchTime": 420
    }
  }
}
```

---

### 2.2 用户管理API

```http
GET /api/v1/admin/users?page=1&pageSize=20
Authorization: Bearer {adminToken}

POST /api/v1/admin/users/{userId}/ban
Authorization: Bearer {adminToken}

Request:
{
  "reason": "违规操作",
  "duration": 30             // 封禁天数
}
```

---

### 2.3 视频管理API

```http
GET /api/v1/admin/videos?status=pending&page=1
Authorization: Bearer {adminToken}

POST /api/v1/admin/videos/{videoId}/approve
Authorization: Bearer {adminToken}

POST /api/v1/admin/videos/{videoId}/reject
Authorization: Bearer {adminToken}

Request:
{
  "reason": "内容违规"
}
```

---

## 3. 数据库设计

```sql
CREATE TABLE "AdminActions" (
    "Id" uuid PRIMARY KEY,
    "AdminId" uuid NOT NULL,
    "ActionType" integer NOT NULL,
    "TargetId" uuid,
    "TargetType" integer,
    "Details" jsonb,
    "CreationTime" timestamp with time zone NOT NULL
);
```

---

**文档版本**: v2.0  
**状态**: ✅ 详细设计完成