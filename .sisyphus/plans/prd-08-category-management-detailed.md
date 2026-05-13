# 08-分区与内容管理详细PRD

## 1. 模块概述

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 分区数量 | 10个主分区 |
| 视频审核 | AI自动通过 |

---

## 2. 功能详细设计

### 2.1 分区列表API

```http
GET /api/v1/categories

Response:
{
  "code": 0,
  "data": [
    {
      "id": "uuid",
      "name": "动画",
      "icon": "http://...",
      "children": [
        { "id": "uuid", "name": "番剧" },
        { "id": "uuid", "name": "国产动画" }
      ]
    }
  ]
}
```

---

### 2.2 视频审核API

```http
POST /api/v1/content/audit/{videoId}
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "status": "approved",      // approved/rejected
  "reason": "审核通过"
}
```

---

## 3. 数据库设计

```sql
CREATE TABLE "Categories" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(100) NOT NULL,
    "ParentId" uuid REFERENCES "Categories"("Id"),
    "Icon" varchar(500),
    "SortOrder" integer DEFAULT 0,
    "IsEnabled" boolean DEFAULT true,
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE TABLE "ContentAudits" (
    "Id" uuid PRIMARY KEY,
    "VideoId" uuid NOT NULL,
    "Status" integer DEFAULT 0,
    "Reason" text,
    "AIResult" jsonb,
    "AuditorId" uuid,
    "CreationTime" timestamp with time zone NOT NULL
);
```

---

**文档版本**: v2.0  
**状态**: ✅ 详细设计完成