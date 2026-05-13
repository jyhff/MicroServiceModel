# 直播模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | LiveService（直播服务） |
| 服务地址 | http://live-service:5008 |
| WebSocket地址 | ws://live-service:5008/hubs/live |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. API接口列表

### 2.1 HTTP接口

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 创建直播房间 | POST | /api/live/rooms | 需认证+实名认证 |
| 2 | 获取房间信息 | GET | /api/live/rooms/{roomId} | 公开 |
| 3 | 开始直播 | POST | /api/live/rooms/{roomId}/start | 需认证（主播） |
| 4 | 结束直播 | POST | /api/live/rooms/{roomId}/end | 需认证（主播） |
| 5 | 获取房间列表 | GET | /api/live/rooms | 公开 |
| 6 | 获取礼物列表 | GET | /api/live/gifts | 公开 |
| 7 | 发送礼物 | POST | /api/live/rooms/{roomId}/gifts | 需认证 |

### 2.2 WebSocket接口

| 序号 | 接口名称 | WebSocket方法 | Hub路径 | 权限 |
|------|---------|--------------|---------|------|
| 1 | 连接直播Hub | WebSocket | /hubs/live | 需认证 |

**总计：8个接口（7 HTTP + 1 WebSocket）**

---

## 3. HTTP接口定义

### 3.1 创建直播房间

#### 基本信息

```
POST /api/live/rooms
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证+实名认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| title | string | ✅ | 直播标题 |
| categoryId | UUID | ✅ | 直播分区 |
| description | string | ❌ | 直播简介 |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "userId": "...",
  "title": "游戏直播",
  "categoryId": "...",
  "status": 0,
  "streamUrl": "rtmp://server/live/100001",
  "playUrl": "http://server/live/100001/index.m3u8",
  "creationTime": "..."
}
```

---

### 3.2 获取房间信息

#### 基本信息

```
GET /api/live/rooms/{roomId}
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "roomNumber": "100001",
  "title": "游戏直播",
  "anchor": {
    "id": "...",
    "name": "主播",
    "avatar": "...",
    "level": 5
  },
  "status": 1,
  "currentViewers": 500,
  "hlsUrl": "http://server/live/100001/index.m3u8",
  "categoryId": "...",
  "categoryName": "游戏"
}
```

---

### 3.3 开始直播

#### 基本信息

```
POST /api/live/rooms/{roomId}/start
Authorization: Bearer {token}
权限：需认证（主播）
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "status": 1,
  "startTime": "...",
  "playUrl": "...",
  "viewerCount": 0
}
```

---

### 3.4 结束直播

#### 基本信息

```
POST /api/live/rooms/{roomId}/end
Authorization: Bearer {token}
权限：需认证（主播）
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "status": 0,
  "endTime": "...",
  "durationSeconds": 3600,
  "totalViewerCount": 1000
}
```

---

### 3.5 获取房间列表

#### 基本信息

```
GET /api/live/rooms?categoryId={uuid}&status={int}&maxResultCount={int}&skipCount={int}
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "userName": "主播",
      "title": "直播标题",
      "status": 1,
      "playUrl": "...",
      "viewerCount": 100
    }
  ],
  "totalCount": 50
}
```

---

### 3.6 获取礼物列表

#### 基本信息

```
GET /api/live/gifts
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "name": "小电视",
      "imageUrl": "...",
      "animationUrl": "...",
      "price": 10.00,
      "description": "..."
    }
  ]
}
```

---

### 3.7 发送礼物

#### 基本信息

```
POST /api/live/rooms/{roomId}/gifts
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| giftId | UUID | ✅ | 礼物ID |
| giftCount | int | ✅ | 礼物数量（默认1） |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "liveRoomId": "...",
  "senderId": "...",
  "receiverId": "...",
  "giftId": "...",
  "giftName": "小电视",
  "giftCount": 1,
  "totalPrice": 10.00,
  "receiverRevenue": 10.00,
  "sendTime": "...",
  "message": "礼物发送成功，UP主获得100%收益"
}
```

---

## 4. WebSocket接口

### 连接直播Hub

```
WebSocket ws://{domain}/hubs/live?roomId={roomId}&token={jwt_token}
```

#### 推送事件示例

```json
{
  "type": "Gift",
  "data": {
    "giftId": "...",
    "giftName": "小电视",
    "senderName": "用户",
    "giftCount": 1,
    "totalPrice": 10.00
  }
}
```

```json
{
  "type": "viewer_count_update",
  "data": {
    "roomId": "...",
    "viewerCount": 100
  }
}
```

---

**文档版本**: v1.0  
**状态**: ✅ 直播模块API文档完成