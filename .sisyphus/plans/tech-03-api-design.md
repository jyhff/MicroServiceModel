# API接口设计文档（结合现有ABP架构）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | HTTP API接口设计 |
| 目标项目 | Bilibili视频平台 |
| 框架 | ABP Framework v10.0.2 + ASP.NET Core |
| 创建时间 | 2026-05-12 |

---

## 2. API设计规范

### 2.1 基于ABP框架的API规范

继承ABP Framework内置API规范：

#### 2.1.1 RESTful规范

| HTTP方法 | 用途 | 示例 |
|---------|------|------|
| `GET` | 查询资源 | `GET /api/video/videos/{id}` |
| `POST` | 创建资源 | `POST /api/video/videos` |
| `PUT` | 更新资源（全量） | `PUT /api/video/videos/{id}` |
| `PATCH` | 更新资源（部分） | `PATCH /api/video/videos/{id}` |
| `DELETE` | 删除资源 | `DELETE /api/video/videos/{id}` |

#### 2.1.2 URL命名规范

继承ABP命名约定：

```
基础格式：/api/{service}/{resource}

示例：
- /api/video/videos                 视频服务
- /api/danmaku/danmakus             弹幕服务
- /api/interaction/comments         互动服务
- /api/live/rooms                   直播服务
- /api/search/videos                搜索服务
- /api/category/categories          分区服务
- /api/admin/videos                 管理后台
```

#### 2.1.3 ABP标准响应格式

继承ABP统一响应格式：

```json
// 成功响应（单对象）
{
  "id": "guid",
  "title": "string",
  "creationTime": "datetime",
  // ... 其他字段
}

// 成功响应（列表）
{
  "items": [
    { "id": "guid", "title": "string", ... }
  ],
  "totalCount": 100
}

// ABP分页响应（继承PagedResultDto）
{
  "items": [...],
  "totalCount": 100
}

// ABP错误响应（继承AbpExceptionDto）
{
  "error": {
    "code": "string",
    "message": "string",
    "details": "string",
    "validationErrors": [
      {
        "message": "string",
        "members": ["field1", "field2"]
      }
    ]
  }
}
```

#### 2.1.4 ABP标准HTTP Header

继承ABP请求头：

| Header | 说明 | 示例 |
|--------|------|------|
| `Authorization` | JWT Token | `Bearer {token}` |
| `X-Requested-With` | AJAX标识 | `XMLHttpRequest` |
| `Accept-Language` | 语言（多语言支持） | `zh-CN`, `en-US` |
| `Content-Type` | 内容类型 | `application/json` |
| `_AbpAuditLogCorrelationId` | 审计日志关联ID | `guid` |

---

## 3. 用户模块API（UserService）

### 3.1 用户认证API（继承IdentityServer）

#### 3.1.1 用户注册

```
POST /api/identity/register
Content-Type: application/json

请求体：
{
  "userName": "string",           // 用户名（必填，4-20字符）
  "email": "string",              // 邮箱（必填）
  "password": "string",           // 密码（必填，6-20字符）
  "captchaCode": "string",        // 验证码（必填）
  "captchaKey": "string"          // 验证码Key（必填）
}

成功响应（200 OK）：
{
  "userId": "guid",
  "userName": "string",
  "email": "string",
  "accessToken": "string",        // JWT Token
  "refreshToken": "string",       // Refresh Token
  "expiresIn": 3600               // 过期时间（秒）
}

错误响应（400 Bad Request）：
{
  "error": {
    "code": "Identity:RegistrationFailed",
    "message": "注册失败",
    "validationErrors": [
      {
        "message": "用户名已存在",
        "members": ["userName"]
      }
    ]
  }
}
```

#### 3.1.2 用户登录

```
POST /api/identity/login
Content-Type: application/json

请求体：
{
  "userNameOrEmail": "string",    // 用户名或邮箱（必填）
  "password": "string",           // 密码（必填）
  "rememberMe": boolean           // 记住我（可选，默认false）
}

成功响应（200 OK）：
{
  "userId": "guid",
  "userName": "string",
  "accessToken": "string",
  "refreshToken": "string",
  "expiresIn": 3600
}

错误响应（401 Unauthorized）：
{
  "error": {
    "code": "Identity:InvalidCredentials",
    "message": "用户名或密码错误"
  }
}
```

#### 3.1.3 Token刷新

```
POST /api/identity/refresh-token
Content-Type: application/json

请求体：
{
  "refreshToken": "string"        // Refresh Token（必填）
}

成功响应（200 OK）：
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresIn": 3600
}
```

---

### 3.2 用户信息API

#### 3.2.1 获取用户信息

```
GET /api/identity/users/{id}
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "id": "guid",
  "userName": "string",
  "email": "string",
  "phoneNumber": "string",
  "roles": ["string"],
  "isActive": true,
  "creationTime": "datetime"
}
```

#### 3.2.2 获取用户扩展信息（Bilibili用户）

```
GET /api/bilibili/users/profile
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "userId": "guid",
  "level": 3,                     // LV0-LV6
  "levelProgress": 50,            // 当前等级进度（0-100）
  "totalExperience": 250,         // 总经验值
  "dailyExperience": 50,          // 今日获得经验
  "coinsBalance": 1000,           // B币余额
  "avatarUrl": "string",
  "signature": "string",
  "gender": 1,                    // 0未知 1男 2女
  "isRealNameVerified": true,
  "isVip": false
}
```

#### 3.2.3 更新用户信息

```
PUT /api/bilibili/users/profile
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "avatarUrl": "string",          // 头像URL（可选）
  "signature": "string",          // 个人签名（可选）
  "gender": integer               // 性别（可选）
}

成功响应（200 OK）：
{
  "userId": "guid",
  "avatarUrl": "string",
  "signature": "string",
  "gender": integer,
  "lastModificationTime": "datetime"
}
```

---

### 3.3 用户等级API

#### 3.3.1 获取用户等级信息

```
GET /api/bilibili/users/level
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "level": 3,
  "levelProgress": 50,
  "totalExperience": 250,
  "dailyExperience": 50,
  "maxDailyExperience": 100,
  "levelRequirements": [
    {
      "level": 0,
      "minExperience": 0,
      "maxExperience": 100,
      "privileges": ["基础弹幕"]
    },
    {
      "level": 1,
      "minExperience": 101,
      "maxExperience": 200,
      "privileges": ["基础弹幕"]
    },
    {
      "level": 2,
      "minExperience": 201,
      "maxExperience": 400,
      "privileges": ["基础弹幕", "高级弹幕"]
    },
    // ... LV6
  ]
}
```

#### 3.3.2 获取经验记录

```
GET /api/bilibili/users/experience-records?startDate={date}&endDate={date}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}

参数说明：
- startDate: 开始日期（可选，默认今日）
- endDate: 结束日期（可选，默认今日）
- maxResultCount: 返回数量（可选，默认10）
- skipCount: 跳过数量（可选，默认0）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "experienceType": "LOGIN",
      "experienceAmount": 5,
      "description": "每日登录",
      "creationTime": "datetime"
    },
    {
      "id": "guid",
      "userId": "guid",
      "experienceType": "WATCH_VIDEO",
      "experienceAmount": 1,
      "sourceId": "video-guid",
      "description": "观看视频获得经验",
      "creationTime": "datetime"
    }
  ],
  "totalCount": 5
}
```

---

### 3.4 用户B币API

#### 3.4.1 获取B币余额

```
GET /api/bilibili/users/coins/balance
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "coinsBalance": 1000,
  "coinsTotalEarned": 2000,
  "coinsTotalSpent": 1000
}
```

#### 3.4.2 获取B币记录

```
GET /api/bilibili/users/coins/records?maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "transactionType": "COIN_VIDEO",
      "coinAmount": -2,
      "balanceAfter": 998,
      "sourceId": "video-guid",
      "targetUserId": "up-owner-guid",
      "description": "投币给视频xxx",
      "creationTime": "datetime"
    },
    {
      "id": "guid",
      "userId": "guid",
      "transactionType": "RECEIVE_COIN",
      "coinAmount": 5,
      "balanceAfter": 1005,
      "sourceId": "video-guid",
      "description": "收到投币",
      "creationTime": "datetime"
    }
  ],
  "totalCount": 20
}
```

---

## 4. 视频模块API（VideoService）

### 4.1 视频上传API

#### 4.1.1 初始化上传会话（分片上传）

```
POST /api/video/upload/init
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "fileName": "string",           // 文件名（必填）
  "fileSize": long,               // 文件大小（字节，必填）
  "fileMd5": "string",            // 文件MD5（必填）
  "chunkSize": integer            // 分片大小（可选，默认5MB）
}

成功响应（200 OK）：
{
  "uploadId": "guid",             // 上传会话ID
  "chunkCount": 10,               // 总分片数
  "chunkSize": 5242880,           // 每片大小（字节）
  "uploadUrls": [                 // 各分片上传URL
    {
      "chunkIndex": 0,
      "uploadUrl": "string"       // MinIO上传URL
    },
    {
      "chunkIndex": 1,
      "uploadUrl": "string"
    }
    // ... 所有分片
  ],
  "expireTime": "datetime"        // 上传会话过期时间（2小时）
}
```

#### 4.1.2 上传分片（MinIO直传）

```
PUT {uploadUrl}                   // 使用返回的uploadUrl
Content-Type: application/octet-stream

请求体：二进制文件数据

成功响应（200 OK）：
{
  "etag": "string",               // MinIO ETag
  "chunkIndex": integer
}
```

#### 4.1.3 完成上传（合并分片）

```
POST /api/video/upload/complete/{uploadId}
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "fileName": "string",
  "fileMd5": "string",
  "chunkCount": integer,
  "chunks": [                     // 分片信息
    {
      "chunkIndex": 0,
      "etag": "string"
    },
    {
      "chunkIndex": 1,
      "etag": "string"
    }
    // ... 所有分片
  ]
}

成功响应（200 OK）：
{
  "videoId": "guid",              // 创建的视频ID
  "fileUrl": "string",            // 合并后文件URL
  "status": "UPLOADING",
  "creationTime": "datetime"
}
```

#### 4.1.4 上传视频封面

```
POST /api/video/upload/cover/{videoId}
Authorization: Bearer {token}
Content-Type: multipart/form-data

请求体：
{
  "coverImage": file              // 尘面图片文件
}

成功响应（200 OK）：
{
  "coverUrl": "string",
  "thumbnailUrl": "string"
}
```

---

### 4.2 视频管理API

#### 4.2.1 创建视频信息（上传完成后）

```
POST /api/video/videos
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "uploadId": "guid",             // 上传会话ID
  "title": "string",              // 视频标题（必填，最大200字符）
  "description": "string",        // 视频简介（可选，最大5000字符）
  "categoryId": "guid",           // 所属分区（必填）
  "tags": ["string"],             // 标签（可选，最多10个）
  "isOriginal": boolean           // 是否原创（可选，默认true）
}

成功响应（200 OK）：
{
  "id": "guid",
  "userId": "guid",
  "title": "string",
  "description": "string",
  "categoryId": "guid",
  "status": "TRANSCODING",        // 转码中
  "auditStatus": "PENDING",
  "creationTime": "datetime"
}
```

#### 4.2.2 获取视频详情

```
GET /api/video/videos/{id}
Authorization: Bearer {token} (可选，未登录也能查看)

成功响应（200 OK）：
{
  "id": "guid",
  "userId": "guid",
  "userName": "string",
  "userAvatar": "string",
  "userLevel": 3,
  "title": "string",
  "description": "string",
  "coverImageUrl": "string",
  "categoryId": "guid",
  "categoryName": "string",
  "durationSeconds": 1200,
  "hlsMasterUrl": "string",
  "resolutions": [
    {
      "resolution": "1080p",
      "hlsPlaylistUrl": "string",
      "width": 1920,
      "height": 1080,
      "bitrate": 5000
    },
    {
      "resolution": "720p",
      "hlsPlaylistUrl": "string",
      "width": 1280,
      "height": 720,
      "bitrate": 3000
    },
    {
      "resolution": "480p",
      "hlsPlaylistUrl": "string",
      "width": 854,
      "height": 480,
      "bitrate": 1500
    },
    {
      "resolution": "360p",
      "hlsPlaylistUrl": "string",
      "width": 640,
      "height": 360,
      "bitrate": 800
    }
  ],
  "status": "PUBLISHED",
  "publishTime": "datetime",
  "isOriginal": true,
  "totalViews": 100000,
  "totalLikes": 5000,
  "totalCoins": 2000,
  "totalComments": 1000,
  "danmakuCount": 5000,
  "tags": ["tag1", "tag2"],
  "creationTime": "datetime",
  
  // 用户互动状态（需登录）
  "userInteraction": {
    "isLiked": false,
    "coinAmount": 0,
    "isFavorite": false
  }
}
```

#### 4.2.3 获取视频列表（UP主视频）

```
GET /api/video/videos/user/{userId}?status={status}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token} (可选)

参数说明：
- userId: 用户ID（必填）
- status: 视频状态（可选，0=DRAFT, 1=UPLOADING, 2=TRANSCODING, 3=AUDITING, 4=PUBLISHED, 5=FAILED）
- maxResultCount: 返回数量（可选，默认10）
- skipCount: 跳过数量（可选，默认0）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "title": "string",
      "coverImageUrl": "string",
      "durationSeconds": integer,
      "status": "PUBLISHED",
      "totalViews": long,
      "publishTime": "datetime",
      "creationTime": "datetime"
    }
  ],
  "totalCount": 50
}
```

#### 4.2.4 更新视频信息

```
PUT /api/video/videos/{id}
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "title": "string",              // 视频标题（可选）
  "description": "string",        // 视频简介（可选）
  "categoryId": "guid",           // 所属分区（可选）
  "tags": ["string"]              // 标签（可选）
}

成功响应（200 OK）：
{
  "id": "guid",
  "title": "string",
  "description": "string",
  "categoryId": "guid",
  "lastModificationTime": "datetime"
}
```

#### 4.2.5 删除视频

```
DELETE /api/video/videos/{id}
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "id": "guid",
  "isDeleted": true,
  "deletionTime": "datetime"
}
```

---

### 4.3 视频转码API

#### 4.3.1 获取转码进度

```
GET /api/video/videos/{id}/transcode-progress
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "videoId": "guid",
  "status": "TRANSCODING",
  "progressPercent": 60,
  "completedResolutions": ["360p", "480p"],
  "pendingResolutions": ["720p", "1080p"],
  "startTime": "datetime",
  "estimatedEndTime": "datetime"
}
```

---

### 4.4 视频播放API

#### 4.4.1 获取播放URL

```
GET /api/video/videos/{id}/play-url?resolution={resolution}
Authorization: Bearer {token} (可选)

参数说明：
- id: 视频ID（必填）
- resolution: 清晰度（可选，默认720p，可选值：360p/480p/720p/1080p）

成功响应（200 OK）：
{
  "videoId": "guid",
  "resolution": "720p",
  "hlsPlaylistUrl": "string",
  "durationSeconds": integer,
  "expireTime": "datetime"        // URL过期时间（24小时）
}
```

#### 4.4.2 记录播放进度（用户行为）

```
POST /api/video/videos/{id}/play-record
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "playDuration": integer,        // 实际播放时长（秒，必填）
  "videoDuration": integer,       // 视频总时长（秒，必填）
  "playProgress": decimal,        // 播放进度百分比（0-100）
  "playedResolution": "string",   // 播放清晰度
  "deviceType": "string",         // 设备类型（Web/Mobile/Desktop）
  "startTime": "datetime",        // 开始播放时间
  "endTime": "datetime"           // 结束播放时间
}

成功响应（200 OK）：
{
  "videoId": "guid",
  "userId": "guid",
  "playDuration": integer,
  "experienceEarned": 1,           // 获得经验（完整观看+1）
  "creationTime": "datetime"
}
```

---

## 5. 弹幕模块API（DanmakuService）

### 5.1 弹幕查询API

#### 5.1.1 获取视频弹幕列表

```
GET /api/danmaku/danmakus/video/{videoId}?startTime={decimal}&endTime={decimal}
Authorization: Bearer {token} (可选)

参数说明：
- videoId: 视频ID（必填）
- startTime: 开始时间（秒，可选，默认0）
- endTime: 结束时间（秒，可选，默认视频总时长）

成功响应（200 OK）：
{
  "videoId": "guid",
  "totalCount": 500,
  "timeRange": {
    "startTime": 0,
    "endTime": 1200
  },
  "danmakus": [
    {
      "id": "guid",
      "videoId": "guid",
      "userId": "guid",
      "userName": "string",
      "userLevel": 3,
      "content": "string",
      "positionTime": 10.5,
      "danmakuType": 0,            // 0=滚动 1=顶部 2=底部 3=高级
      "fontSize": 25,
      "fontColor": "#FFFFFF",
      "isSendByVip": false,
      "sendTime": "datetime"
    },
    {
      "id": "guid",
      "positionTime": 12.3,
      "content": "哈哈哈",
      "danmakuType": 0,
      "fontColor": "#FF0000"
    }
    // ... 弹幕列表
  ]
}
```

#### 5.1.2 获取用户发送的弹幕

```
GET /api/danmaku/danmakus/user?maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "videoId": "guid",
      "videoTitle": "string",
      "content": "string",
      "positionTime": decimal,
      "danmakuType": integer,
      "sendTime": "datetime"
    }
  ],
  "totalCount": 100
}
```

---

### 5.2 弹幕发送API

#### 5.2.1 发送弹幕（HTTP）

```
POST /api/danmaku/danmakus
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "videoId": "guid",              // 视频ID（必填）
  "content": "string",            // 弹幕内容（必填，最大500字符）
  "positionTime": decimal,        // 出现时间（秒，必填）
  "danmakuType": integer,         // 弹幕类型（可选，默认0）
  "fontSize": integer,            // 字体大小（可选，默认25）
  "fontColor": "string"           // 字体颜色（可选，默认#FFFFFF）
}

成功响应（200 OK）：
{
  "id": "guid",
  "videoId": "guid",
  "userId": "guid",
  "content": "string",
  "positionTime": decimal,
  "danmakuType": integer,
  "fontSize": integer,
  "fontColor": "string",
  "sendTime": "datetime"
}

错误响应（403 Forbidden）：
{
  "error": {
    "code": "Danmaku:AdvancedLevelRequired",
    "message": "高级弹幕需要LV2以上"
  }
}
```

---

### 5.3 弹幕WebSocket接口（实时推送）

#### 5.3.1 WebSocket连接

```
WebSocket URL: ws://{domain}/hubs/danmaku?videoId={videoId}&token={jwt_token}

连接参数：
- videoId: 视频ID（必填）
- token: JWT Token（可选，未登录也能观看弹幕）

连接流程：
1. 客户端发起WebSocket连接
2. 服务端验证Token（可选）
3. 服务端加入SignalR Group（video-{videoId})
4. 开始接收弹幕推送

消息格式（服务端推送）：
{
  "type": "danmaku",
  "data": {
    "id": "guid",
    "userId": "guid",
    "userName": "string",
    "userLevel": integer,
    "content": "string",
    "positionTime": decimal,
    "danmakuType": integer,
    "fontSize": integer,
    "fontColor": "string",
    "sendTime": "datetime"
  }
}

消息格式（客户端发送）：
{
  "type": "sendDanmaku",
  "data": {
    "content": "string",
    "positionTime": decimal,
    "danmakuType": integer,
    "fontSize": integer,
    "fontColor": "string"
  }
}
```

#### 5.3.2 SignalR Hub代码示例

```csharp
// DanmakuHub.cs（继承ABP Hub）
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;

namespace LCH.MicroService.Danmaku.Hubs
{
    public class DanmakuHub : AbpHub
    {
        private readonly IDanmakuService _danmakuService;
        
        public DanmakuHub(IDanmakuService danmakuService)
        {
            _danmakuService = danmakuService;
        }
        
        /// <summary>
        /// 客户端连接时加入视频房间
        /// </summary>
        public async Task JoinVideoRoom(Guid videoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"video-{videoId}");
            
            // 发送欢迎消息
            await Clients.Client(Context.ConnectionId).SendAsync("joined", new
            {
                videoId = videoId,
                connectionId = Context.ConnectionId,
                viewerCount = 1  // 观众数（Redis实时统计）
            });
        }
        
        /// <summary>
        /// 客户端发送弹幕
        /// </summary>
        public async Task SendDanmaku(SendDanmakuDto input)
        {
            // 验证用户等级（高级弹幕需要LV2+）
            if (input.DanmakuType == 3 && CurrentUser.Level < 2)
            {
                throw new UserFriendlyException("高级弹幕需要LV2以上");
            }
            
            // 保存弹幕到数据库
            var danmaku = await _danmakuService.CreateAsync(new CreateDanmakuDto
            {
                VideoId = input.VideoId,
                Content = input.Content,
                PositionTime = input.PositionTime,
                DanmakuType = input.DanmakuType,
                FontSize = input.FontSize,
                FontColor = input.FontColor
            });
            
            // 广播弹幕到所有观众
            await Clients.Group($"video-{input.VideoId}").SendAsync("danmaku", new
            {
                id = danmaku.Id,
                userId = CurrentUser.Id,
                userName = CurrentUser.UserName,
                userLevel = CurrentUser.Level,
                content = danmaku.Content,
                positionTime = danmaku.PositionTime,
                danmakuType = danmaku.DanmakuType,
                fontSize = danmaku.FontSize,
                fontColor = danmaku.FontColor,
                sendTime = danmaku.CreationTime
            });
        }
        
        /// <summary>
        /// 客户端断开连接时离开视频房间
        /// </summary>
        public async Task LeaveVideoRoom(Guid videoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video-{videoId}");
        }
        
        /// <summary>
        /// 获取当前观众数
        /// </summary>
        public async Task<int> GetViewerCount(Guid videoId)
        {
            // 从Redis获取观众数
            return await _danmakuService.GetViewerCountAsync(videoId);
        }
    }
    
    // DTO定义
    public class SendDanmakuDto
    {
        public Guid VideoId { get; set; }
        public string Content { get; set; }
        public decimal PositionTime { get; set; }
        public int DanmakuType { get; set; }
        public int FontSize { get; set; }
        public string FontColor { get; set; }
    }
}
```

---

## 6. 互动模块API（InteractionService）

### 6.1 点赞API

#### 6.1.1 点赞视频

```
POST /api/interaction/likes
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "videoId": "guid"               // 视频ID（必填）
}

成功响应（200 OK）：
{
  "id": "guid",
  "userId": "guid",
  "videoId": "guid",
  "isLiked": true,
  "creationTime": "datetime"
}
```

#### 6.1.2 取消点赞

```
DELETE /api/interaction/likes/{videoId}
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "id": "guid",
  "videoId": "guid",
  "isLiked": false,
  "lastModificationTime": "datetime"
}
```

#### 6.1.3 查询点赞状态

```
GET /api/interaction/likes/{videoId}
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "videoId": "guid",
  "isLiked": true,
  "likeTime": "datetime"
}
```

---

### 6.2 投币API

#### 6.2.1 投币给视频

```
POST /api/interaction/coins
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "videoId": "guid",              // 视频ID（必填）
  "coinAmount": integer           // 投币数量（必填，无限制）
}

成功响应（200 OK）：
{
  "id": "guid",
  "userId": "guid",
  "videoId": "guid",
  "coinAmount": 2,
  "receiverUserId": "guid",
  "creationTime": "datetime",
  
  // 用户B币余额更新
  "coinsBalanceAfter": 998
}

错误响应（400 Bad Request）：
{
  "error": {
    "code": "Interaction:InsufficientCoins",
    "message": "B币余额不足"
  }
}
```

#### 6.2.2 查询投币记录

```
GET /api/interaction/coins/video/{videoId}?maxResultCount={int}&skipCount={int}
Authorization: Bearer {token} (可选)

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "userName": "string",
      "userAvatar": "string",
      "coinAmount": 2,
      "creationTime": "datetime"
    }
  ],
  "totalCount": 100
}
```

---

### 6.3 评论API

#### 6.3.1 发送评论（需审核）

```
POST /api/interaction/comments
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "videoId": "guid",              // 视频ID（必填）
  "content": "string",            // 评论内容（必填，最大1000字符）
  "parentCommentId": "guid"       // 父评论ID（可选，楼中楼）
}

成功响应（200 OK）：
{
  "id": "guid",
  "userId": "guid",
  "videoId": "guid",
  "content": "string",
  "parentCommentId": "guid",
  "auditStatus": 0,               // 0=待审
  "isPublished": false,           // 未发布（需人工审核）
  "creationTime": "datetime"
}
```

#### 6.3.2 获取评论列表（已审核通过）

```
GET /api/interaction/comments/video/{videoId}?maxResultCount={int}&skipCount={int}&sortOrder={string}
Authorization: Bearer {token} (可选)

参数说明：
- videoId: 视频ID（必填）
- maxResultCount: 返回数量（可选，默认20）
- skipCount: 跳过数量（可选，默认0）
- sortOrder: 排序方式（可选，默认hot，可选值：hot/new）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "userName": "string",
      "userAvatar": "string",
      "userLevel": integer,
      "content": "string",
      "likeCount": 50,
      "replyCount": 10,
      "parentCommentId": null,
      "replies": [                  // 回复列表（楼中楼）
        {
          "id": "guid",
          "userId": "guid",
          "userName": "string",
          "content": "string",
          "replyToUserName": "string",
          "likeCount": 5,
          "creationTime": "datetime"
        }
      ],
      "publishTime": "datetime",
      "creationTime": "datetime"
    }
  ],
  "totalCount": 200
}
```

#### 6.3.3 删除评论

```
DELETE /api/interaction/comments/{id}
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "id": "guid",
  "isDeleted": true,
  "deletionTime": "datetime"
}
```

---

### 6.4 分享API

#### 6.4.1 记录分享行为

```
POST /api/interaction/shares
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "videoId": "guid",              // 视频ID（必填）
  "sharePlatform": "string"       // 分享平台（必填：WECHAT/WEIBO/QQ/TWITTER/FACEBOOK/COPY_LINK）
}

成功响应（200 OK）：
{
  "id": "guid",
  "userId": "guid",
  "videoId": "guid",
  "sharePlatform": "WECHAT",
  "shareTime": "datetime",
  "experienceEarned": 2           // 分享获得经验+2
}
```

---

## 7. 搜索模块API（SearchService）

### 7.1 视频搜索API

#### 7.1.1 搜索视频

```
GET /api/search/videos?keyword={keyword}&categoryId={guid}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token} (可选)

参数说明：
- keyword: 搜索关键词（必填）
- categoryId: 分区ID（可选）
- maxResultCount: 返回数量（可选，默认20）
- skipCount: 跳过数量（可选，默认0）

成功响应（200 OK）：
{
  "keyword": "string",
  "totalCount": 1000,
  "items": [
    {
      "id": "guid",
      "title": "string",
      "description": "string",
      "coverImageUrl": "string",
      "durationSeconds": integer,
      "categoryId": "guid",
      "categoryName": "string",
      "userId": "guid",
      "userName": "string",
      "userLevel": integer,
      "totalViews": long,
      "totalLikes": long,
      "publishTime": "datetime",
      "relevanceScore": 0.95       // 相关度得分
    }
  ],
  "searchTime": "datetime",
  "took": 50                       // 搜索耗时（毫秒）
}
```

#### 7.1.2 获取搜索建议（自动补全）

```
GET /api/search/suggestions?keyword={keyword}
Authorization: Bearer {token} (可选)

参数说明：
- keyword: 搜索关键词（必填，至少1字符）

成功响应（200 OK）：
{
  "keyword": "str",
  "suggestions": [
    "string 类型",
    "string 转换",
    "string 方法",
    "string 拼接"
  ]
}
```

#### 7.1.3 获取热搜榜单

```
GET /api/search/hot-search?maxResultCount={int}
Authorization: Bearer {token} (可选)

参数说明：
- maxResultCount: 返回数量（可选，默认10）

成功响应（200 OK）：
{
  "items": [
    {
      "keyword": "string",
      "searchCount": 100000,
      "trend": "up",               // up/down/stable
      "rank": 1
    }
  ],
  "updateTime": "datetime"
}
```

---

## 8. 推荐模块API（RecommendService）

### 8.1 推荐API

#### 8.1.1 获取个性化推荐视频

```
GET /api/recommend/videos/personalized?maxResultCount={int}
Authorization: Bearer {token}

参数说明：
- maxResultCount: 返回数量（可选，默认10）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "title": "string",
      "coverImageUrl": "string",
      "durationSeconds": integer,
      "userId": "guid",
      "userName": "string",
      "totalViews": long,
      "publishTime": "datetime",
      "recommendScore": 0.92,      // 推荐得分
      "recommendReason": "based_on_your_watch_history"  // 推荐理由
    }
  ],
  "totalCount": 100,
  "algorithm": "collaborative_filtering"
}
```

#### 8.1.2 获取热门推荐视频

```
GET /api/recommend/videos/hot?maxResultCount={int}
Authorization: Bearer {token} (可选)

参数说明：
- maxResultCount: 返回数量（可选，默认10）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "title": "string",
      "coverImageUrl": "string",
      "totalViews": long,
      "trend": "hot"
    }
  ]
}
```

---

## 9. 直播模块API（LiveService）

### 9.1 直播房间API

#### 9.1.1 创建直播房间

```
POST /api/live/rooms
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "title": "string",              // 直播标题（必填）
  "categoryId": "guid",           // 直播分区（必填）
  "description": "string"         // 直播简介（可选）
}

成功响应（200 OK）：
{
  "id": "guid",
  "userId": "guid",
  "title": "string",
  "categoryId": "guid",
  "status": 0,                    // 0=未开播
  "streamUrl": "string",          // 推流地址（rtmp://）
  "playUrl": "string",            // 播放地址（HLS）
  "creationTime": "datetime"
}
```

#### 9.1.2 开始直播

```
POST /api/live/rooms/{id}/start
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "id": "guid",
  "status": 1,                    // 1=直播中
  "startTime": "datetime",
  "playUrl": "string",
  "viewerCount": 0
}
```

#### 9.1.3 结束直播

```
POST /api/live/rooms/{id}/end
Authorization: Bearer {token}

成功响应（200 OK）：
{
  "id": "guid",
  "status": 0,
  "endTime": "datetime",
  "durationSeconds": 3600,
  "totalViewerCount": 1000
}
```

#### 9.1.4 获取直播房间列表

```
GET /api/live/rooms?categoryId={guid}&status={int}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token} (可选)

参数说明：
- categoryId: 分区ID（可选）
- status: 房间状态（可选，1=直播中）
- maxResultCount: 返回数量（可选，默认20）
- skipCount: 跳过数量（可选，默认0）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "userName": "string",
      "userAvatar": "string",
      "title": "string",
      "coverImageUrl": "string",
      "categoryId": "guid",
      "status": 1,
      "playUrl": "string",
      "viewerCount": 100,
      "startTime": "datetime"
    }
  ],
  "totalCount": 50
}
```

---

### 9.2 直播礼物API

#### 9.2.1 获取礼物列表

```
GET /api/live/gifts
Authorization: Bearer {token} (可选)

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "name": "string",
      "imageUrl": "string",
      "animationUrl": "string",
      "price": 10.00,
      "description": "string"
    }
  ]
}
```

#### 9.2.2 发送礼物

```
POST /api/live/gift-records
Authorization: Bearer {token}
Content-Type: application/json

请求体：
{
  "liveRoomId": "guid",           // 直播房间ID（必填）
  "giftId": "guid",               // 礼物ID（必填）
  "giftCount": integer            // 礼物数量（必填，默认1）
}

成功响应（200 OK）：
{
  "id": "guid",
  "liveRoomId": "guid",
  "senderId": "guid",
  "receiverId": "guid",           // 主播ID
  "giftId": "guid",
  "giftName": "string",
  "giftCount": 1,
  "totalPrice": 10.00,
  "sendTime": "datetime",
  "receiverRevenue": 10.00        // 主播收益（100%给UP主）
}
```

---

### 9.3 直播WebSocket接口

#### 9.3.1 WebSocket连接（观众）

```
WebSocket URL: ws://{domain}/hubs/live?roomId={roomId}&token={jwt_token}

消息格式（服务端推送）：
{
  "type": "viewer_count_update",
  "data": {
    "roomId": "guid",
    "viewerCount": 100
  }
}

{
  "type": "gift",
  "data": {
    "giftId": "guid",
    "giftName": "string",
    "senderName": "string",
    "giftCount": 1,
    "totalPrice": 10.00,
    "animationUrl": "string"
  }
}
```

---

## 10. 分区管理API（CategoryService）

### 10.1 分区查询API

#### 10.1.1 获取分区列表

```
GET /api/category/categories?parentId={guid}&level={int}
Authorization: Bearer {token} (可选)

参数说明：
- parentId: 父分区ID（可选，获取子分区）
- level: 分区层级（可选，1=主分区，2=子分区）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "name": "动画",
      "code": "animation",
      "iconUrl": "string",
      "description": "string",
      "parentId": null,
      "level": 1,
      "sortOrder": 1,
      "videoCount": 10000,
      "isActive": true
    },
    {
      "id": "guid",
      "name": "游戏",
      "code": "game",
      "level": 1,
      "sortOrder": 2
    }
    // ... 10个主分区
  ],
  "totalCount": 10
}
```

#### 10.1.2 获取分区视频

```
GET /api/category/categories/{id}/videos?maxResultCount={int}&skipCount={int}&sortOrder={string}
Authorization: Bearer {token} (可选)

参数说明：
- id: 分区ID（必填）
- maxResultCount: 返回数量（可选，默认20）
- skipCount: 跳过数量（可选，默认0）
- sortOrder: 排序方式（可选，默认hot，可选值：hot/new）

成功响应（200 OK）：
{
  "categoryId": "guid",
  "categoryName": "动画",
  "items": [
    {
      "id": "guid",
      "title": "string",
      "coverImageUrl": "string",
      "totalViews": long,
      "publishTime": "datetime"
    }
  ],
  "totalCount": 500
}
```

---

## 11. 管理后台API（AdminService）

### 11.1 视频审核API（继承ABP权限）

#### 11.1.1 获取待审核视频列表

```
GET /api/admin/videos/audit?auditStatus={int}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token} (需超级管理员权限)

参数说明：
- auditStatus: 审核状态（可选，0=待审）
- maxResultCount: 返回数量（可选，默认20）
- skipCount: 跳过数量（可选，默认0）

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "userId": "guid",
      "userName": "string",
      "title": "string",
      "description": "string",
      "coverImageUrl": "string",
      "durationSeconds": integer,
      "categoryId": "guid",
      "auditStatus": 0,
      "creationTime": "datetime"
    }
  ],
  "totalCount": 50
}
```

#### 11.1.2 审核视频（通过/拒绝）

```
POST /api/admin/videos/{id}/audit
Authorization: Bearer {token} (需超级管理员权限)
Content-Type: application/json

请求体：
{
  "auditStatus": integer,         // 审核结果（必填，1=通过 2=拒绝）
  "auditReason": "string"         // 审核意见（可选）
}

成功响应（200 OK）：
{
  "id": "guid",
  "auditStatus": 1,
  "auditTime": "datetime",
  "auditorId": "guid",
  "auditReason": "string",
  "isPublished": true,
  "publishTime": "datetime"
}
```

---

### 11.2 评论审核API

#### 11.2.1 获取待审核评论列表

```
GET /api/admin/comments/audit?auditStatus={int}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token} (需超级管理员权限)

成功响应（200 OK）：
{
  "items": [
    {
      "id": "guid",
      "videoId": "guid",
      "videoTitle": "string",
      "userId": "guid",
      "userName": "string",
      "content": "string",
      "auditStatus": 0,
      "creationTime": "datetime"
    }
  ],
  "totalCount": 100
}
```

#### 11.2.2 审核评论

```
POST /api/admin/comments/{id}/audit
Authorization: Bearer {token} (需超级管理员权限)
Content-Type: application/json

请求体：
{
  "auditStatus": integer,
  "auditReason": "string"
}

成功响应（200 OK）：
{
  "id": "guid",
  "auditStatus": 1,
  "auditTime": "datetime",
  "isPublished": true,
  "publishTime": "datetime"
}
```

---

### 11.3 用户管理API

#### 11.3.1 封禁用户

```
POST /api/admin/users/{id}/ban
Authorization: Bearer {token} (需超级管理员权限)
Content-Type: application/json

请求体：
{
  "banExpireDate": "datetime",    // 封禁到期日期（可选，永久封禁不填）
  "banReason": "string"           // 封禁原因（必填）
}

成功响应（200 OK）：
{
  "userId": "guid",
  "isBanned": true,
  "banExpireDate": "datetime",
  "banReason": "string",
  "bannedTime": "datetime"
}
```

#### 11.3.2 解封用户

```
POST /api/admin/users/{id}/unban
Authorization: Bearer {token} (需超级管理员权限)

成功响应（200 OK）：
{
  "userId": "guid",
  "isBanned": false,
  "unbannedTime": "datetime"
}
```

---

## 12. ABP控制器代码示例

### 12.1 VideoController（继承ABP Controller）

```csharp
// VideoController.cs（继承ABP Controller）
using Volo.Abp.AspNetCore.Mvc;
using LCH.MicroService.Video.Application.Services;
using LCH.MicroService.Video.Application.Dtos;

namespace LCH.MicroService.Video.HttpApi.Controllers
{
    /// <summary>
    /// 视频API控制器（继承ABP Controller）
    /// </summary>
    [ApiController]
    [Route("api/video/videos")]
    public class VideoController : AbpController
    {
        private readonly IVideoAppService _videoAppService;
        
        public VideoController(IVideoAppService videoAppService)
        {
            _videoAppService = videoAppService;
        }
        
        /// <summary>
        /// 获取视频详情
        /// </summary>
        [HttpGet("{id}")]
        public async Task<VideoDetailDto> GetAsync(Guid id)
        {
            return await _videoAppService.GetAsync(id);
        }
        
        /// <summary>
        /// 获取用户视频列表
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<PagedResultDto<VideoDto>> GetUserVideosAsync(
            Guid userId,
            VideoStatus? status = null,
            int maxResultCount = 10,
            int skipCount = 0)
        {
            return await _videoAppService.GetUserVideosAsync(
                userId, status, maxResultCount, skipCount);
        }
        
        /// <summary>
        /// 创建视频（需登录）
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<VideoDto> CreateAsync(CreateVideoDto input)
        {
            return await _videoAppService.CreateAsync(input);
        }
        
        /// <summary>
        /// 更新视频（需登录）
        /// </summary>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<VideoDto> UpdateAsync(Guid id, UpdateVideoDto input)
        {
            return await _videoAppService.UpdateAsync(id, input);
        }
        
        /// <summary>
        /// 删除视频（需登录）
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task DeleteAsync(Guid id)
        {
            await _videoAppService.DeleteAsync(id);
        }
        
        /// <summary>
        /// 获取播放URL
        /// </summary>
        [HttpGet("{id}/play-url")]
        public async Task<PlayUrlDto> GetPlayUrlAsync(
            Guid id, 
            string resolution = "720p")
        {
            return await _videoAppService.GetPlayUrlAsync(id, resolution);
        }
        
        /// <summary>
        /// 记录播放行为（需登录）
        /// </summary>
        [HttpPost("{id}/play-record")]
        [Authorize]
        public async Task<PlayRecordDto> RecordPlayAsync(
            Guid id, 
            RecordPlayDto input)
        {
            return await _videoAppService.RecordPlayAsync(id, input);
        }
    }
}
```

---

### 12.2 ApplicationService代码示例

```csharp
// VideoAppService.cs（继承ABP ApplicationService）
using Volo.Abp.Application.Services;
using LCH.MicroService.Video.Domain.Entities;
using LCH.MicroService.Video.Domain.Repositories;

namespace LCH.MicroService.Video.Application.Services
{
    /// <summary>
    /// 视频应用服务（继承ABP ApplicationService）
    /// </summary>
    public class VideoAppService : ApplicationService, IVideoAppService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IBlobContainer _blobContainer;
        private readonly ICurrentUser _currentUser;
        
        public VideoAppService(
            IVideoRepository videoRepository,
            IBlobContainer blobContainer,
            ICurrentUser currentUser)
        {
            _videoRepository = videoRepository;
            _blobContainer = blobContainer;
            _currentUser = currentUser;
        }
        
        /// <summary>
        /// 获取视频详情
        /// </summary>
        public async Task<VideoDetailDto> GetAsync(Guid id)
        {
            var video = await _videoRepository.GetAsync(id);
            
            // 转换为DTO
            var dto = ObjectMapper.Map<Video, VideoDetailDto>(video);
            
            // 如果用户已登录，查询用户互动状态
            if (_currentUser.Id.HasValue)
            {
                dto.UserInteraction = await GetUserInteractionAsync(id, _currentUser.Id.Value);
            }
            
            return dto;
        }
        
        /// <summary>
        /// 创建视频
        /// </summary>
        [Authorize]
        public async Task<VideoDto> CreateAsync(CreateVideoDto input)
        {
            // 创建视频实体
            var video = new Video(
                GuidGenerator.Create(),
                input.Title,
                _currentUser.Id.Value
            )
            {
                Description = input.Description,
                CategoryId = input.CategoryId,
                Tags = input.Tags,
                IsOriginal = input.IsOriginal,
                Status = VideoStatus.Transcoding,
                AuditStatus = AuditStatus.Pending
            };
            
            // 保存到数据库
            video = await _videoRepository.InsertAsync(video);
            
            // 发布转码任务到RabbitMQ
            await PublishTranscodeTaskAsync(video.Id);
            
            return ObjectMapper.Map<Video, VideoDto>(video);
        }
        
        /// <summary>
        /// 记录播放行为（原子增加播放量）
        /// </summary>
        [Authorize]
        public async Task<PlayRecordDto> RecordPlayAsync(Guid videoId, RecordPlayDto input)
        {
            // 增加播放量（原子操作）
            await _videoRepository.IncrementViewCountAsync(videoId);
            
            // 记录用户播放行为（用于推荐算法）
            var playRecord = new VideoPlayRecord
            {
                VideoId = videoId,
                UserId = _currentUser.Id.Value,
                PlayDuration = input.PlayDuration,
                VideoDuration = input.VideoDuration,
                PlayProgress = input.PlayProgress,
                PlayedResolution = input.PlayedResolution,
                DeviceType = input.DeviceType,
                StartTime = input.StartTime,
                EndTime = input.EndTime
            };
            
            // 如果完整观看（progress >= 90），给用户+1经验
            if (input.PlayProgress >= 90)
            {
                await GrantUserExperienceAsync(
                    _currentUser.Id.Value,
                    "WATCH_VIDEO",
                    1,
                    videoId,
                    "完整观看视频获得经验"
                );
            }
            
            return new PlayRecordDto
            {
                VideoId = videoId,
                UserId = _currentUser.Id.Value,
                PlayDuration = input.PlayDuration,
                ExperienceEarned = input.PlayProgress >= 90 ? 1 : 0,
                CreationTime = Clock.Now
            };
        }
    }
}
```

---

## 13. API统计汇总

### 13.1 API数量统计

| 模块 | API数量 | 说明 |
|------|--------|------|
| 用户模块 | 10 | 注册、登录、用户信息、等级、B币 |
| 视频模块 | 12 | 上传、管理、转码、播放 |
| 弹幕模块 | 5 | 查询、发送、WebSocket |
| 互动模块 | 9 | 点赞、投币、评论、分享 |
| 搜索模块 | 3 | 搜索、建议、热搜 |
| 推荐模块 | 2 | 个性化推荐、热门推荐 |
| 直播模块 | 7 | 房间管理、礼物、WebSocket |
| 分区管理 | 2 | 分区列表、分区视频 |
| 管理后台 | 6 | 视频审核、评论审核、用户管理 |
| **总计** | **48** | **48个HTTP API接口 + 2个WebSocket接口** |

---

## 14. API Gateway配置示例（继承现有Ocelot）

```json
// app-config.json（Ocelot路由配置）
{
  "Routes": [
    // 视频服务
    {
      "DownstreamPathTemplate": "/api/video/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "video-service", "Port": 5001 }
      ],
      "UpstreamPathTemplate": "/api/video/{everything}",
      "UpstreamHttpMethod": ["Get", "Post", "Put", "Delete"]
    },
    
    // 弹幕服务（HTTP）
    {
      "DownstreamPathTemplate": "/api/danmaku/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "danmaku-service", "Port": 5002 }
      ],
      "UpstreamPathTemplate": "/api/danmaku/{everything}"
    },
    
    // 弹幕WebSocket
    {
      "DownstreamPathTemplate": "/hubs/danmaku",
      "DownstreamScheme": "ws",
      "DownstreamHostAndPorts": [
        { "Host": "danmaku-service", "Port": 5002 }
      ],
      "UpstreamPathTemplate": "/hubs/danmaku"
    },
    
    // 搜索服务
    {
      "DownstreamPathTemplate": "/api/search/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "search-service", "Port": 5003 }
      ],
      "UpstreamPathTemplate": "/api/search/{everything}"
    },
    
    // 直播服务
    {
      "DownstreamPathTemplate": "/api/live/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "live-service", "Port": 5004 }
      ],
      "UpstreamPathTemplate": "/api/live/{everything}"
    },
    
    // 直播WebSocket
    {
      "DownstreamPathTemplate": "/hubs/live",
      "DownstreamScheme": "ws",
      "DownstreamHostAndPorts": [
        { "Host": "live-service", "Port": 5004 }
      ],
      "UpstreamPathTemplate": "/hubs/live"
    }
  ]
}
```

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ API接口设计完成