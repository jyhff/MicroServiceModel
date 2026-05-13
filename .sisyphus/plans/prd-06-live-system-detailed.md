# 06-直播系统详细PRD

## 1. 模块概述

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 直播延迟 | <10秒（HLS标准） |
| 礼物分成 | 100%给UP主 |
| 开播权限 | 所有用户可开播 |

---

## 2. 功能详细设计

### 2.1 直播API

#### 2.1.1 开播

```http
POST /api/v1/live/start
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "title": "直播标题",
  "categoryId": "uuid",
  "cover": "http://..."
}

Response:
{
  "code": 0,
  "data": {
    "liveId": "uuid",
    "streamKey": "secure_stream_key",
    "rtmpUrl": "rtmp://live.example.com/app/{streamKey}",
    "hlsUrl": "http://live.example.com/hls/{liveId}.m3u8"
  }
}
```

---

#### 2.1.2 礼物打赏

```http
POST /api/v1/live/gift
Authorization: Bearer {accessToken}
Content-Type: application/json

Request:
{
  "liveId": "uuid",
  "giftId": "gift_uuid",
  "count": 10,
  "message": "主播加油！"
}

Response:
{
  "code": 0,
  "data": {
    "success": true,
    "totalAmount": 100,        // 总金额（B币）
    "upAmount": 100,           // UP主获得（100%）
    "balance": 500             // 用户余额
  }
}
```

---

## 3. 数据库设计

```sql
CREATE TABLE "LiveStreams" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Title" varchar(200) NOT NULL,
    "CategoryId" uuid,
    "Cover" varchar(500),
    "StreamKey" varchar(100) NOT NULL,
    "Status" integer DEFAULT 0,  -- 0=未开播,1=直播中,2=已结束
    "ViewerCount" integer DEFAULT 0,
    "StartTime" timestamp with time zone,
    "EndTime" timestamp with time zone,
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE TABLE "LiveGifts" (
    "Id" uuid PRIMARY KEY,
    "LiveId" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "GiftId" uuid NOT NULL,
    "Count" integer DEFAULT 1,
    "Amount" decimal(10,2),
    "UpAmount" decimal(10,2),       -- UP主获得
    "Message" varchar(100),
    "CreationTime" timestamp with time zone NOT NULL
);
```

---

**文档版本**: v2.0  
**状态**: ✅ 详细设计完成