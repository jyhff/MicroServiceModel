# 视频模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | VideoService（视频服务） |
| 服务地址 | http://video-service:5002 |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. 模块概述

### 2.1 服务职责

- 视频上传管理（分片上传）
- 视频信息管理（创建、更新、删除）
- 视频转码管理（CPU转码，多清晰度）
- 视频播放管理（HLS播放URL）
- 视频查询管理（列表、详情、搜索）
- 视频审核管理（人工审核）
- 视频封面管理
- 视频统计管理（播放量、点赞数等）

### 2.2 继承关系

- **继承ABP CrudAppService**：标准CRUD操作
- **继承现有OssManagement**：MinIO对象存储
- **继承现有CAP EventBus**：RabbitMQ事件发布

---

## 3. API接口列表

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 初始化上传会话 | POST | /api/video/upload/init | 需认证 |
| 2 | 上传分片 | PUT | /api/video/upload/{uploadId}/chunk/{chunkIndex} | 需认证 |
| 3 | 完成上传 | POST | /api/video/upload/{uploadId}/complete | 需认证 |
| 4 | 上传封面 | POST | /api/video/upload/cover/{videoId} | 需认证 |
| 5 | 创建视频信息 | POST | /api/video/videos | 需认证 |
| 6 | 获取视频详情 | GET | /api/video/videos/{id} | 公开 |
| 7 | 获取视频列表 | GET | /api/video/videos | 公开 |
| 8 | 获取用户视频 | GET | /api/video/videos/user/{userId} | 公开 |
| 9 | 更新视频信息 | PUT | /api/video/videos/{id} | 需认证 |
| 10 | 删除视频 | DELETE | /api/video/videos/{id} | 需认证 |
| 11 | 获取播放URL | GET | /api/video/videos/{id}/play-url | 公开 |
| 12 | 记录播放行为 | POST | /api/video/videos/{id}/play-record | 需认证 |
| 13 | 获取转码进度 | GET | /api/video/videos/{id}/transcode-progress | 需认证 |
| 14 | 发布视频 | POST | /api/video/videos/{id}/publish | 需认证 |
| 15 | 获取播放历史 | GET | /api/video/history | 需认证 |

**总计：15个API接口**

---

## 4. 视频上传接口

### 4.1 初始化上传会话

#### 基本信息

```
POST /api/video/upload/init
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| fileName | string | ✅ | 文件名 |
| fileSize | long | ✅ | 文件大小（字节，无限制） |
| fileMd5 | string | ✅ | 文件MD5校验 |
| chunkSize | int | ❌ | 分片大小（默认5MB） |

#### 请求示例

```json
{
  "fileName": "my_video.mp4",
  "fileSize": 104857600,
  "fileMd5": "abc123def456",
  "chunkSize": 5242880
}
```

#### 成功响应（200 OK）

```json
{
  "uploadId": "00000000-0000-0000-0000-000000000001",
  "chunkCount": 20,
  "chunkSize": 5242880,
  "uploadUrls": [
    {
      "chunkIndex": 0,
      "uploadUrl": "http://minio:9000/bilibili-videos/temp/upload-id/chunk-0"
    },
    {
      "chunkIndex": 1,
      "uploadUrl": "http://minio:9000/bilibili-videos/temp/upload-id/chunk-1"
    }
    // ... 所有分片
  ],
  "expireTime": "2026-05-12T12:00:00Z"
}
```

#### 错误码

| 错误码 | 说明 |
|--------|------|
| Video:UploadSessionExpired | 上传会话已过期 |
| Video:InvalidFileSize | 文件大小无效 |

---

### 4.2 上传分片

#### 基本信息

```
PUT {uploadUrl}  // 使用返回的MinIO直传URL
Content-Type: application/octet-stream
权限：需认证
```

#### 请求体

二进制文件分片数据

#### 成功响应（200 OK）

```json
{
  "etag": "etag-string",
  "chunkIndex": 0
}
```

---

### 4.3 完成上传

#### 基本信息

```
POST /api/video/upload/{uploadId}/complete
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| uploadId | UUID | ✅ | 上传会话ID |

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| fileName | string | ✅ | 文件名 |
| fileMd5 | string | ✅ | 文件MD5 |
| chunkCount | int | ✅ | 分片总数 |
| chunks | array | ✅ | 分片信息列表 |

#### 请求示例

```json
{
  "fileName": "my_video.mp4",
  "fileMd5": "abc123def456",
  "chunkCount": 20,
  "chunks": [
    { "chunkIndex": 0, "etag": "etag-0" },
    { "chunkIndex": 1, "etag": "etag-1" }
    // ... 所有分片
  ]
}
```

#### 成功响应（200 OK）

```json
{
  "videoId": "00000000-0000-0000-0000-000000000002",
  "fileUrl": "videos/video-id/original/my_video.mp4",
  "status": "Transcoding",
  "message": "上传完成，开始转码"
}
```

---

### 4.4 上传封面

#### 基本信息

```
POST /api/video/upload/cover/{videoId}
Authorization: Bearer {token}
Content-Type: multipart/form-data
权限：需认证
```

#### 请求参数（FormData）

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| coverImage | file | ✅ | 尘面图片文件 |

#### 成功响应（200 OK）

```json
{
  "coverUrl": "covers/video-id/cover_1.jpg",
  "thumbnailUrl": "covers/video-id/thumb_1.jpg"
}
```

---

## 5. 视频管理接口

### 5.1 创建视频信息

#### 基本信息

```
POST /api/video/videos
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| uploadId | UUID | ✅ | 上传会话ID |
| title | string | ✅ | 视频标题（最大200字符） |
| description | string | ❌ | 视频简介（最大5000字符） |
| categoryId | UUID | ✅ | 所属分区ID |
| tags | array | ❌ | 标签（最多10个） |
| isOriginal | boolean | ❌ | 是否原创（默认true） |

#### 请求示例

```json
{
  "uploadId": "00000000-0000-0000-0000-000000000001",
  "title": "我的第一个视频",
  "description": "这是一个测试视频",
  "categoryId": "00000000-0000-0000-0000-000000000001",
  "tags": ["游戏", "教程", "测试"],
  "isOriginal": true
}
```

#### 成功响应（200 OK）

```json
{
  "id": "00000000-0000-0000-0000-000000000002",
  "userId": "00000000-0000-0000-0000-000000000001",
  "title": "我的第一个视频",
  "description": "这是一个测试视频",
  "categoryId": "00000000-0000-0000-0000-000000000001",
  "status": "Transcoding",
  "auditStatus": "Pending",
  "creationTime": "2026-05-12T10:00:00Z"
}
```

---

### 5.2 获取视频详情

#### 基本信息

```
GET /api/video/videos/{id}
Authorization: Bearer {token} (可选)
权限：公开
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| id | UUID | ✅ | 视频ID |

#### 成功响应（200 OK）

```json
{
  "id": "00000000-0000-0000-0000-000000000002",
  "userId": "00000000-0000-0000-0000-000000000001",
  "userName": "UP主A",
  "userAvatar": "avatars/user_001.jpg",
  "userLevel": 3,
  "title": "我的第一个视频",
  "description": "这是一个测试视频",
  "coverImageUrl": "covers/video-id/cover.jpg",
  "categoryId": "00000000-0000-0000-0000-000000000001",
  "categoryName": "游戏",
  "durationSeconds": 1200,
  "hlsMasterUrl": "/videos/video-id/master.m3u8",
  "resolutions": [
    {
      "resolution": "1080P",
      "hlsPlaylistUrl": "/videos/video-id/1080p.m3u8",
      "width": 1920,
      "height": 1080,
      "bitrate": 5000
    },
    {
      "resolution": "720P",
      "hlsPlaylistUrl": "/videos/video-id/720p.m3u8",
      "width": 1280,
      "height": 720,
      "bitrate": 3000
    },
    {
      "resolution": "480P",
      "hlsPlaylistUrl": "/videos/video-id/480p.m3u8",
      "width": 854,
      "height": 480,
      "bitrate": 1500
    },
    {
      "resolution": "360P",
      "hlsPlaylistUrl": "/videos/video-id/360p.m3u8",
      "width": 640,
      "height": 360,
      "bitrate": 800
    }
  ],
  "status": "Published",
  "publishTime": "2026-05-12T11:00:00Z",
  "isOriginal": true,
  "totalViews": 10000,
  "totalLikes": 500,
  "totalCoins": 200,
  "totalComments": 100,
  "danmakuCount": 5000,
  "tags": ["游戏", "教程"],
  "creationTime": "2026-05-12T10:00:00Z",
  
  // 用户交互信息（需登录）
  "userInteraction": {
    "isLiked": false,
    "isCoined": false,
    "isFavorite": false
  }
}
```

#### 错误码

| 错误码 | 说明 |
|--------|------|
| Video:NotFound | 视频不存在 |
| Video:NotPublished | 视频未发布 |
| Video:Deleted | 视频已删除 |

---

### 5.3 获取视频列表

#### 基本信息

```
GET /api/video/videos?categoryId={id}&userId={id}&status={status}&keyword={string}&maxResultCount={int}&skipCount={int}
权限：公开
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| categoryId | UUID | ❌ | 分区ID |
| userId | UUID | ❌ | 用户ID |
| status | string | ❌ | 视频状态（Published） |
| keyword | string | ❌ | 搜索关键词 |
| maxResultCount | int | ❌ | 返回数量（默认20） |
| skipCount | int | ❌ | 跳过数量（默认0） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000002",
      "title": "视频标题",
      "description": "视频简介",
      "coverImageUrl": "covers/video-id/cover.jpg",
      "durationSeconds": 120,
      "categoryId": "...",
      "userId": "...",
      "userName": "UP主",
      "userAvatar": "...",
      "userLevel": 3,
      "totalViews": 10000,
      "totalLikes": 500,
      "publishTime": "2026-05-12T11:00:00Z"
    }
  ],
  "totalCount": 100
}
```

---

### 5.4 获取用户视频

#### 基本信息

```
GET /api/video/videos/user/{userId}?status={status}&maxResultCount={int}&skipCount={int}
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "title": "视频标题",
      "coverImageUrl": "...",
      "durationSeconds": 120,
      "status": "Published",
      "totalViews": 1000,
      "publishTime": "..."
    }
  ],
  "totalCount": 50
}
```

---

### 5.5 更新视频信息

#### 基本信息

```
PUT /api/video/videos/{id}
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证（仅UP主本人）
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| title | string | ❌ | 新标题 |
| description | string | ❌ | 新简介 |
| categoryId | UUID | ❌ | 新分区 |
| tags | array | ❌ | 新标签 |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "title": "新标题",
  "lastModificationTime": "2026-05-12T12:00:00Z"
}
```

---

### 5.6 删除视频

#### 基本信息

```
DELETE /api/video/videos/{id}
Authorization: Bearer {token}
权限：需认证（仅UP主本人）
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "isDeleted": true,
  "deletionTime": "2026-05-12T12:00:00Z"
}
```

---

## 6. 视频播放接口

### 6.1 获取播放URL

#### 基本信息

```
GET /api/video/videos/{id}/play-url?resolution={resolution}
权限：公开
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| resolution | string | ❌ | 清晰度（默认720P，可选：360P/480P/720P/1080P） |

#### 成功响应（200 OK）

```json
{
  "videoId": "...",
  "resolution": "720P",
  "hlsPlaylistUrl": "/videos/video-id/720p.m3u8",
  "durationSeconds": 1200,
  "expireTime": "2026-05-13T10:00:00Z"
}
```

---

### 6.2 记录播放行为

#### 基本信息

```
POST /api/video/videos/{id}/play-record
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| playDuration | int | ✅ | 实际播放时长（秒） |
| videoDuration | int | ✅ | 视频总时长（秒） |
| playProgress | decimal | ✅ | 播放进度百分比（0-100） |
| playedResolution | string | ❌ | 播放清晰度 |
| deviceType | string | ❌ | 设备类型（Web/Mobile） |

#### 请求示例

```json
{
  "playDuration": 100,
  "videoDuration": 120,
  "playProgress": 83.33,
  "playedResolution": "720P",
  "deviceType": "Web"
}
```

#### 成功响应（200 OK）

```json
{
  "videoId": "...",
  "userId": "...",
  "playDuration": 100,
  "experienceEarned": 0,  // 未完整观看（<90%）不加经验
  "creationTime": "..."
}
```

#### 说明

- **完整观看**（playProgress ≥ 90%）：用户获得 +1 经验
- **不完整观看**（playProgress < 90%）：不加经验

---

## 7. 视频转码接口

### 7.1 获取转码进度

#### 基本信息

```
GET /api/video/videos/{id}/transcode-progress
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "videoId": "...",
  "status": "Transcoding",
  "progressPercent": 60,
  "completedResolutions": ["360P", "480P"],
  "pendingResolutions": ["720P", "1080P"],
  "startTime": "2026-05-12T10:00:00Z",
  "estimatedEndTime": "2026-05-12T11:00:00Z"
}
```

---

## 8. 视频发布接口

### 8.1 发布视频

#### 基本信息

```
POST /api/video/videos/{id}/publish
Authorization: Bearer {token}
权限：需认证（仅UP主本人）
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "status": "Published",
  "publishTime": "2026-05-12T12:00:00Z",
  "message": "视频发布成功"
}
```

#### 错误码

| 错误码 | 说明 |
|--------|------|
| Video:NotApproved | 视频未审核通过 |
| Video:NotTranscoded | 视频未完成转码 |

---

## 9. 播放历史接口

### 9.1 获取播放历史

#### 基本信息

```
GET /api/video/history?maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "videoId": "...",
      "videoTitle": "视频标题",
      "coverImageUrl": "...",
      "playProgress": 50,
      "lastPlayTime": "2026-05-12T09:00:00Z"
    }
  ],
  "totalCount": 50
}
```

---

## 10. 权限定义

| 权限名称 | 说明 |
|---------|------|
| Video.Videos.View | 查看视频信息（公开） |
| Video.Videos.Create | 创建视频（需认证） |
| Video.Videos.Upload | 上传视频（需认证） |
| Video.Videos.Update | 更新视频（仅UP主） |
| Video.Videos.Delete | 删除视频（仅UP主） |
| Video.Videos.Publish | 发布视频（仅UP主） |

---

## 11. 错误码汇总

| 错误码 | HTTP状态码 | 说明 |
|--------|-----------|------|
| Video:NotFound | 404 | 视频不存在 |
| Video:NotPublished | 403 | 视频未发布 |
| Video:Deleted | 404 | 视频已删除 |
| Video:NotOwner | 403 | 不是视频UP主 |
| Video:UploadSessionExpired | 400 | 上传会话已过期 |
| Video:UploadFailed | 400 | 上传失败 |
| Video:TranscodeFailed | 500 | 转码失败 |
| Video:NotApproved | 400 | 未审核通过 |
| Video:NotTranscoded | 400 | 未完成转码 |

---

## 12. 接口依赖关系

### 12.1 依赖其他服务

| 服务 | 依赖接口 | 用途 |
|------|---------|------|
| UserService | /api/bilibili/users/{id} | 获取UP主信息 |
| CategoryService | /api/category/categories/{id} | 获取分区信息 |
| InteractionService | /api/interaction/* | 获取互动统计 |
| OssManagement | MinIO直传URL | 上传视频文件 |
| RabbitMQ | CAP EventBus | 发布转码任务 |

---

## 13. 接口性能指标

| 接口 | 平均响应时间 | QPS上限 | 说明 |
|------|------------|---------|------|
| 初始化上传 | <100ms | 1000 | 中频接口 |
| 获取视频详情 | <50ms | 5000 | 高频接口，需缓存 |
| 获取视频列表 | <100ms | 2000 | 高频接口 |
| 获取播放URL | <50ms | 10000 | 最高频接口 |
| 记录播放行为 | <50ms | 5000 | 高频接口 |
| 获取转码进度 | <100ms | 500 | 低频接口 |

---

## 14. 接口版本历史

| 版本 | 更新时间 | 更新内容 |
|------|---------|---------|
| v1.0 | 2026-05-12 | 初始版本，15个接口 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 视频模块API文档完成