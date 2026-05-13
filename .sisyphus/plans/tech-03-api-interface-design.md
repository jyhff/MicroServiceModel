# API接口设计文档（结合现有项目）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | API接口设计 |
| 目标项目 | Bilibili视频平台 |
| API框架 | ABP Framework v10.0.2 + ASP.NET Core |
| 创建时间 | 2026-05-12 |

---

## 2. API设计规范

### 2.1 基于ABP Framework的API约定

继承ABP Framework的API设计规范：

| 规范项 | 说明 |
|--------|------|
| **路由前缀** | `/api/{module}/{service}` |
| **DTO命名** | `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto` |
| **返回格式** | 统一使用 `ResultDto<T>` 包装 |
| **错误处理** | ABP异常处理中间件 |
| **版本控制** | 通过路由 `/api/v1/...` 实现 |
| **认证方式** | JWT Bearer Token |
| **权限控制** | ABP Permission System |

---

### 2.2 统一响应格式

```json
{
  "success": true,
  "data": {},
  "error": null,
  "extraProperties": {}
}
```

**错误响应格式**（ABP标准）：

```json
{
  "error": {
    "code": "Video:001",
    "message": "视频不存在",
    "details": "VideoId: xxx not found",
    "validationErrors": []
  }
}
```

---

### 2.3 分页查询格式

继承ABP分页规范：

```json
{
  "items": [],
  "totalCount": 100,
  "skipCount": 0,
  "maxResultCount": 20
}
```

**请求参数**（ABP标准）：

```
?SkipCount=0&MaxResultCount=20&Sorting=CreationTime DESC
```

---

## 3. VideoService API设计

### 3.1 视频管理接口

#### 3.1.1 创建视频（上传初始化）

**接口**: `POST /api/video/videos`

**权限**: `Video.Create`

**请求头**:

```
Authorization: Bearer {token}
Content-Type: application/json
```

**请求体**:

```json
{
  "title": "我的第一个视频",
  "description": "视频描述内容",
  "categoryId": "uuid",
  "tags": ["游戏", "教程"],
  "isOriginal": true,
  "sourceUrl": null
}
```

**响应**:

```json
{
  "id": "uuid",
  "title": "我的第一个视频",
  "uploadSessionId": "uuid",
  "chunkSize": 5242880,
  "totalChunks": 10,
  "uploadUrl": "/api/video/upload/{sessionId}/chunk/{index}",
  "status": "Draft"
}
```

**代码示例**（继承ABP ICrudAppService）:

```csharp
// VideoAppService.cs (继承ABP ApplicationService)
using Volo.Abp.Application.Services;

namespace LCH.MicroService.Video.Application.Services
{
    public class VideoAppService : ApplicationService, IVideoAppService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IUploadSessionManager _uploadSessionManager;
        
        public VideoAppService(
            IVideoRepository videoRepository,
            IUploadSessionManager uploadSessionManager)
        {
            _videoRepository = videoRepository;
            _uploadSessionManager = uploadSessionManager;
        }
        
        [Authorize(VideoPermissions.Create)]
        public async Task<CreateVideoResultDto> CreateAsync(CreateVideoDto input)
        {
            // 1. 创建视频实体
            var video = new Video(
                GuidGenerator.Create(),
                input.Title,
                CurrentUser.Id.Value
            );
            
            video.Description = input.Description;
            video.CategoryId = input.CategoryId;
            video.Tags = input.Tags;
            video.IsOriginal = input.IsOriginal;
            video.SourceUrl = input.SourceUrl;
            
            await _videoRepository.InsertAsync(video);
            
            // 2. 创建上传会话
            var uploadSession = await _uploadSessionManager.CreateSessionAsync(
                video.Id,
                CurrentUser.Id.Value,
                input.TotalFileSize
            );
            
            // 3. 返回上传信息
            return new CreateVideoResultDto
            {
                Id = video.Id,
                Title = video.Title,
                UploadSessionId = uploadSession.Id,
                ChunkSize = uploadSession.ChunkSize,
                TotalChunks = uploadSession.TotalChunks,
                UploadUrl = $"/api/video/upload/{uploadSession.Id}/chunk/{{index}}",
                Status = video.Status.ToString()
            };
        }
    }
}
```

---

#### 3.1.2 上传视频分片

**接口**: `POST /api/video/upload/{sessionId}/chunk/{chunkIndex}`

**权限**: `Video.Upload`

**请求头**:

```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**请求体**:

```
FormData:
  file: binary data (分片文件)
  md5: string (分片MD5校验)
```

**响应**:

```json
{
  "sessionId": "uuid",
  "chunkIndex": 0,
  "uploadedChunks": 1,
  "totalChunks": 10,
  "progress": 10,
  "status": "InProgress"
}
```

**代码示例**:

```csharp
// UploadController.cs
using Volo.Abp.AspNetCore.Mvc;

namespace LCH.MicroService.Video.HttpApi.Controllers
{
    [ApiController]
    [Route("api/video/upload")]
    public class UploadController : AbpControllerBase
    {
        private readonly IUploadSessionManager _uploadSessionManager;
        
        public UploadController(IUploadSessionManager uploadSessionManager)
        {
            _uploadSessionManager = uploadSessionManager;
        }
        
        [HttpPost("{sessionId}/chunk/{chunkIndex}")]
        [Authorize(VideoPermissions.Upload)]
        public async Task<UploadChunkResultDto> UploadChunk(
            Guid sessionId,
            int chunkIndex,
            [FromForm] UploadChunkInput input)
        {
            // 使用现有MinIO BlobContainer
            var blobContainer = await GetBlobContainerAsync();
            
            // 保存分片到临时目录
            var chunkPath = $"temp/{sessionId}/{chunkIndex}";
            await blobContainer.SaveAsync(chunkPath, input.File.OpenReadStream());
            
            // 更新上传会话进度
            var session = await _uploadSessionManager.UpdateProgressAsync(
                sessionId,
                chunkIndex,
                input.Md5
            );
            
            return ObjectMapper.Map<UploadSession, UploadChunkResultDto>(session);
        }
    }
}
```

---

#### 3.1.3 完成上传

**接口**: `POST /api/video/upload/{sessionId}/complete`

**权限**: `Video.Upload`

**请求体**:

```json
{
  "totalMd5": "string",
  "fileName": "my-video.mp4"
}
```

**响应**:

```json
{
  "videoId": "uuid",
  "filePath": "videos/{videoId}/original.mp4",
  "status": "PendingReview",
  "message": "上传完成，等待审核"
}
```

---

#### 3.1.4 获取视频列表

**接口**: `GET /api/video/videos`

**权限**: 无需认证（公开接口）

**查询参数**（继承ABP PagedAndSortedResultRequestDto）:

```
?SkipCount=0
&MaxResultCount=20
&Sorting=PublishTime DESC
&CategoryId=uuid
&UserId=uuid
&Status=Published
&Keyword=搜索关键词
```

**响应**:

```json
{
  "items": [
    {
      "id": "uuid",
      "title": "视频标题",
      "description": "视频描述",
      "coverPath": "covers/{id}/cover.jpg",
      "duration": 120,
      "viewCount": 10000,
      "likeCount": 500,
      "coinCount": 100,
      "publishTime": "2026-05-12T10:00:00Z",
      "author": {
        "id": "uuid",
        "name": "UP主名称",
        "avatar": "avatars/{id}.jpg",
        "level": 3
      },
      "category": {
        "id": "uuid",
        "name": "游戏"
      }
    }
  ],
  "totalCount": 100
}
```

**代码示例**:

```csharp
// VideoAppService.cs
public async Task<PagedResultDto<VideoDto>> GetListAsync(GetVideoListDto input)
{
    // 使用ABP Repository扩展查询
    var query = await _videoRepository.GetQueryableAsync();
    
    query = query
        .Where(v => v.Status == VideoStatus.Published)
        .WhereIf(input.CategoryId.HasValue, v => v.CategoryId == input.CategoryId)
        .WhereIf(input.UserId.HasValue, v => v.UserId == input.UserId)
        .WhereIf(!string.IsNullOrEmpty(input.Keyword), 
            v => v.Title.Contains(input.Keyword) || v.Description.Contains(input.Keyword));
    
    // 排序
    query = query.OrderByDescending(v => v.PublishTime);
    
    // 分页
    var totalCount = await AsyncExecuter.CountAsync(query);
    var videos = await AsyncExecuter.ToListAsync(
        query.Skip(input.SkipCount).Take(input.MaxResultCount)
    );
    
    // 映射DTO（使用ABP ObjectMapper）
    var items = ObjectMapper.Map<List<Video>, List<VideoDto>>(videos);
    
    return new PagedResultDto<VideoDto>(totalCount, items);
}
```

---

#### 3.1.5 获取视频详情

**接口**: `GET /api/video/videos/{id}`

**权限**: 无需认证（公开接口）

**响应**:

```json
{
  "id": "uuid",
  "title": "视频标题",
  "description": "完整描述",
  "duration": 120,
  "masterPlaylist": "/videos/{id}/master.m3u8",
  "resolutions": {
    "1080P": "/videos/{id}/1080p.m3u8",
    "720P": "/videos/{id}/720p.m3u8",
    "480P": "/videos/{id}/480p.m3u8",
    "360P": "/videos/{id}/360p.m3u8"
  },
  "coverPath": "covers/{id}/cover.jpg",
  "viewCount": 10000,
  "likeCount": 500,
  "coinCount": 100,
  "favoriteCount": 200,
  "shareCount": 50,
  "commentCount": 300,
  "danmakuCount": 1000,
  "publishTime": "2026-05-12T10:00:00Z",
  "isOriginal": true,
  "tags": ["游戏", "教程"],
  "author": {
    "id": "uuid",
    "name": "UP主名称",
    "avatar": "avatars/{id}.jpg",
    "level": 3,
    "isFollowed": false
  },
  "category": {
    "id": "uuid",
    "name": "游戏"
  },
  "userInteraction": {
    "isLiked": false,
    "isCoined": false,
    "isFavorite": false
  }
}
```

---

#### 3.1.6 更新视频信息

**接口**: `PUT /api/video/videos/{id}`

**权限**: `Video.Update`（仅UP主本人）

**请求体**:

```json
{
  "title": "新标题",
  "description": "新描述",
  "categoryId": "uuid",
  "tags": ["新标签"]
}
```

---

#### 3.1.7 删除视频

**接口**: `DELETE /api/video/videos/{id}`

**权限**: `Video.Delete`（仅UP主本人）

**响应**:

```json
{
  "success": true,
  "message": "视频已删除"
}
```

---

### 3.2 视频播放接口

#### 3.2.1 记录播放进度

**接口**: `POST /api/video/videos/{id}/play`

**权限**: 需认证

**请求体**:

```json
{
  "playDuration": 60,
  "videoDuration": 120,
  "playedResolution": "720P",
  "deviceType": "Web"
}
```

---

#### 3.2.2 获取播放历史

**接口**: `GET /api/video/history`

**权限**: 需认证

**响应**:

```json
{
  "items": [
    {
      "videoId": "uuid",
      "videoTitle": "视频标题",
      "coverPath": "...",
      "playProgress": 50,
      "lastPlayTime": "2026-05-12T10:00:00Z"
    }
  ],
  "totalCount": 50
}
```

---

## 4. DanmakuService API设计

### 4.1 WebSocket实时接口

#### 4.1.1 WebSocket连接

**接口**: `WebSocket /ws/danmaku`

**连接URL**: 

```
ws://domain/ws/danmaku?videoId={uuid}&token={jwt_token}
```

**认证流程**（继承现有SignalR认证）:

```javascript
// 客户端连接示例
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/danmaku", {
        accessTokenFactory: () => localStorage.getItem('token')
    })
    .withAutomaticReconnect()
    .build();

connection.start()
    .then(() => connection.invoke("JoinRoom", videoId))
    .catch(err => console.error(err));
```

**消息格式**（继承ABP SignalR Hub）:

```csharp
// DanmakuHub.cs (继承AbpHub)
using Volo.Abp.SignalR;

namespace LCH.MicroService.Danmaku.SignalR.Hubs
{
    public class DanmakuHub : AbpHub
    {
        private readonly IDanmakuService _danmakuService;
        
        public DanmakuHub(IDanmakuService danmakuService)
        {
            _danmakuService = danmakuService;
        }
        
        // 加入房间（继承现有Group管理）
        public async Task JoinRoom(Guid videoId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"video_{videoId}");
            
            // 发送最近弹幕
            var recentDanmakus = await _danmakuService.GetRecentAsync(videoId, 100);
            await Clients.Caller.SendAsync("RecentDanmakus", recentDanmakus);
        }
        
        // 发送弹幕
        public async Task SendDanmaku(SendDanmakuInput input)
        {
            // 1. 验证用户权限
            var userId = CurrentUser.Id.Value;
            var userLevel = await GetUserLevelAsync(userId);
            
            // 2. 创建弹幕实体
            var danmaku = await _danmakuService.CreateAsync(
                input.VideoId,
                userId,
                input.Content,
                input.VideoTime,
                input.Color,
                input.Position
            );
            
            // 3. 广播到房间（使用Redis Scaleout）
            await Clients.Group($"video_{input.VideoId}")
                .SendAsync("NewDanmaku", danmaku);
        }
        
        // 退出房间
        public async Task LeaveRoom(Guid videoId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video_{videoId}");
        }
    }
}
```

---

#### 4.1.2 接收弹幕消息

**客户端接收事件**:

```json
{
  "event": "NewDanmaku",
  "data": {
    "id": "uuid",
    "content": "弹幕内容",
    "videoTime": 30.5,
    "color": "#FFFFFF",
    "position": "Scroll",
    "fontSize": 25,
    "userId": "uuid",
    "userName": "用户名",
    "userLevel": 2,
    "createTime": "2026-05-12T10:00:00Z"
  }
}
```

---

### 4.2 HTTP查询接口

#### 4.2.1 获取弹幕列表

**接口**: `GET /api/danmaku/videos/{videoId}/danmakus`

**权限**: 无需认证

**查询参数**:

```
?StartTime=0
&EndTime=120
&Limit=500
```

**响应**:

```json
{
  "items": [
    {
      "id": "uuid",
      "content": "弹幕内容",
      "videoTime": 30.5,
      "color": "#FFFFFF",
      "position": "Scroll",
      "fontSize": 25
    }
  ],
  "totalCount": 1000
}
```

---

#### 4.2.2 发送弹幕（HTTP方式）

**接口**: `POST /api/danmaku/videos/{videoId}/danmakus`

**权限**: 需认证

**请求体**:

```json
{
  "content": "弹幕内容",
  "videoTime": 30.5,
  "color": "#FFFFFF",
  "position": "Scroll",
  "fontSize": 25,
  "danmakuType": "Normal"
}
```

---

#### 4.2.3 举报弹幕

**接口**: `POST /api/danmaku/danmakus/{id}/report`

**权限**: 需认证

**请求体**:

```json
{
  "reason": "Spam",
  "description": "垃圾广告弹幕"
}
```

---

## 5. InteractionService API设计

### 5.1 视频互动接口

#### 5.1.1 点赞视频

**接口**: `POST /api/interaction/videos/{videoId}/like`

**权限**: 需认证

**请求体**:

```json
{
  "isLike": true
}
```

**响应**:

```json
{
  "success": true,
  "likeCount": 501,
  "isLiked": true
}
```

**代码示例**:

```csharp
// InteractionAppService.cs
public async Task<LikeResultDto> LikeVideoAsync(Guid videoId, LikeInput input)
{
    var userId = CurrentUser.Id.Value;
    
    // 1. 检查是否已点赞
    var existingLike = await _likeRepository.FindAsync(
        l => l.VideoId == videoId && l.UserId == userId
    );
    
    if (input.IsLike)
    {
        if (existingLike == null)
        {
            // 新点赞
            var like = new VideoLike(
                GuidGenerator.Create(),
                videoId,
                userId
            );
            await _likeRepository.InsertAsync(like);
            
            // 更新视频点赞数（通过RabbitMQ事件）
            await _eventBus.PublishAsync(new VideoLikeEvent
            {
                VideoId = videoId,
                Delta = 1
            });
        }
    }
    else
    {
        // 取消点赞
        if (existingLike != null)
        {
            await _likeRepository.DeleteAsync(existingLike);
            
            await _eventBus.PublishAsync(new VideoLikeEvent
            {
                VideoId = videoId,
                Delta = -1
            });
        }
    }
    
    // 返回最新点赞数（从Redis获取）
    var likeCount = await _cache.GetAsync<int>($"video:count:{videoId}:like");
    
    return new LikeResultDto
    {
        Success = true,
        LikeCount = likeCount,
        IsLiked = input.IsLike
    };
}
```

---

#### 5.1.2 投币视频

**接口**: `POST /api/interaction/videos/{videoId}/coin`

**权限**: 需认证

**请求体**:

```json
{
  "coinCount": 1
}
```

**响应**:

```json
{
  "success": true,
  "coinCount": 101,
  "userCoinBalance": 999,
  "message": "投币成功"
}
```

---

#### 5.1.3 收藏视频

**接口**: `POST /api/interaction/videos/{videoId}/favorite`

**权限**: 需认证

**请求体**:

```json
{
  "favoriteGroupId": "uuid",
  "isFavorite": true
}
```

---

#### 5.1.4 分享视频

**接口**: `POST /api/interaction/videos/{videoId}/share`

**权限**: 需认证

**请求体**:

```json
{
  "platform": "WeChat"
}
```

---

### 5.2 评论接口

#### 5.2.1 获取评论列表

**接口**: `GET /api/interaction/videos/{videoId}/comments`

**权限**: 无需认证

**查询参数**:

```
?SkipCount=0
&MaxResultCount=20
&Sorting=LikeCount DESC
&RootCommentId=null
```

**响应**:

```json
{
  "items": [
    {
      "id": "uuid",
      "content": "评论内容",
      "userId": "uuid",
      "userName": "用户名",
      "userAvatar": "...",
      "likeCount": 50,
      "replyCount": 10,
      "createTime": "2026-05-12T10:00:00Z",
      "replies": [
        {
          "id": "uuid",
          "content": "回复内容",
          "userId": "uuid",
          "userName": "用户名",
          "replyToUserName": "原用户名",
          "createTime": "..."
        }
      ]
    }
  ],
  "totalCount": 100
}
```

---

#### 5.2.2 发送评论

**接口**: `POST /api/interaction/videos/{videoId}/comments`

**权限**: 需认证

**请求体**:

```json
{
  "content": "评论内容",
  "parentCommentId": "uuid",
  "replyToUserId": "uuid"
}
```

**响应**:

```json
{
  "id": "uuid",
  "status": "PendingReview",
  "message": "评论已提交，等待人工审核"
}
```

---

#### 5.2.3 点赞评论

**接口**: `POST /api/interaction/comments/{commentId}/like`

**权限**: 需认证

---

### 5.3 用户关注接口

#### 5.3.1 关注用户

**接口**: `POST /api/interaction/users/{userId}/follow`

**权限**: 需认证

**请求体**:

```json
{
  "isFollow": true
}
```

---

#### 5.3.2 获取关注列表

**接口**: `GET /api/interaction/users/{userId}/following`

**权限**: 无需认证

---

#### 5.3.3 获取粉丝列表

**接口**: `GET /api/interaction/users/{userId}/followers`

**权限**: 无需认证

---

## 6. UserService API设计（继承现有IdentityService）

### 6.1 用户信息接口

#### 6.1.1 获取用户信息

**接口**: `GET /api/user/profiles/{userId}`

**权限**: 无需认证

**响应**:

```json
{
  "id": "uuid",
  "userName": "用户名",
  "nickName": "昵称",
  "avatar": "avatars/{id}.jpg",
  "signature": "个性签名",
  "level": 3,
  "experience": 500,
  "nextLevelExperience": 1000,
  "isVerified": false,
  "isUpVerified": true,
  "videoCount": 50,
  "followerCount": 1000,
  "followingCount": 200,
  "isFollowed": false
}
```

---

#### 6.1.2 更新用户信息

**接口**: `PUT /api/user/profiles/{userId}`

**权限**: 需认证（仅本人）

**请求体**:

```json
{
  "nickName": "新昵称",
  "avatar": "base64_image",
  "signature": "新签名"
}
```

---

#### 6.1.3 获取我的信息

**接口**: `GET /api/user/me`

**权限**: 需认证

**响应**:

```json
{
  "id": "uuid",
  "userName": "用户名",
  "email": "user@example.com",
  "level": 3,
  "coinBalance": 1000,
  "experience": 500,
  "dailyExperienceEarned": 50,
  "dailyExperienceLimit": 100
}
```

---

### 6.2 用户等级接口

#### 6.2.1 获取等级经验信息

**接口**: `GET /api/user/me/experience`

**权限**: 需认证

**响应**:

```json
{
  "currentLevel": 3,
  "currentExperience": 500,
  "nextLevelExperience": 1000,
  "levelProgress": 50,
  "dailyEarned": 50,
  "dailyLimit": 100,
  "sources": {
    "dailyLogin": 5,
    "watchVideo": 15,
    "likeVideo": 10,
    "coinVideo": 20
  }
}
```

---

#### 6.2.2 获取经验记录

**接口**: `GET /api/user/me/experience/records`

**权限**: 需认证

**查询参数**:

```
?Date=2026-05-12
```

---

### 6.3 用户认证接口

#### 6.3.1 实名认证

**接口**: `POST /api/user/me/verify/realname`

**权限**: 需认证

**请求体**:

```json
{
  "realName": "真实姓名",
  "idCardNumber": "身份证号"
}
```

---

#### 6.3.2 UP主认证

**接口**: `POST /api/user/me/verify/up`

**权限**: 需认证

---

### 6.4 消息接口

#### 6.4.1 获取消息列表

**接口**: `GET /api/user/me/messages`

**权限**: 需认证

**查询参数**:

```
?MessageType=Private
&IsRead=false
&SkipCount=0
&MaxResultCount=20
```

---

#### 6.4.2 发送私信

**接口**: `POST /api/user/me/messages`

**权限**: 需认证

**请求体**:

```json
{
  "receiverId": "uuid",
  "content": "消息内容"
}
```

---

## 7. LiveService API设计

### 7.1 直播房间接口

#### 7.1.1 创建直播房间

**接口**: `POST /api/live/rooms`

**权限**: 需认证 + 实名认证

**请求体**:

```json
{
  "title": "直播标题",
  "categoryId": "uuid"
}
```

**响应**:

```json
{
  "id": "uuid",
  "roomNumber": "100001",
  "streamUrl": "rtmp://server/live/{roomNumber}",
  "streamKey": "secret_key",
  "status": "Offline"
}
```

---

#### 7.1.2 获取直播房间信息

**接口**: `GET /api/live/rooms/{roomId}`

**权限**: 无需认证

**响应**:

```json
{
  "id": "uuid",
  "roomNumber": "100001",
  "title": "直播标题",
  "anchor": {
    "id": "uuid",
    "name": "主播名称",
    "avatar": "...",
    "level": 5
  },
  "status": "Live",
  "currentViewers": 500,
  "hlsUrl": "http://server/live/{roomNumber}/index.m3u8",
  "categoryId": "uuid",
  "categoryName": "游戏"
}
```

---

#### 7.1.3 开始直播

**接口**: `POST /api/live/rooms/{roomId}/start`

**权限**: 需认证（仅主播）

---

#### 7.1.4 结束直播

**接口**: `POST /api/live/rooms/{roomId}/end`

**权限**: 需认证（仅主播）

---

### 7.2 直播礼物接口

#### 7.2.1 获取礼物列表

**接口**: `GET /api/live/gifts`

**权限**: 无需认证

**响应**:

```json
{
  "items": [
    {
      "id": "uuid",
      "name": "小电视",
      "imagePath": "gifts/small_tv.png",
      "coinPrice": 100,
      "effectDuration": 3
    }
  ]
}
```

---

#### 7.2.2 发送礼物

**接口**: `POST /api/live/rooms/{roomId}/gifts`

**权限**: 需认证

**请求体**:

```json
{
  "giftId": "uuid",
  "giftCount": 1,
  "isCombo": false
}
```

**响应**:

```json
{
  "success": true,
  "totalCoin": 100,
  "userCoinBalance": 900,
  "message": "礼物发送成功，UP主获得100%收益"
}
```

---

### 7.3 直播WebSocket接口

#### 7.3.1 直播弹幕WebSocket

**接口**: `WebSocket /ws/live/{roomId}`

**连接URL**:

```
ws://domain/ws/live/{roomId}?token={jwt_token}
```

**消息格式**:

```json
{
  "event": "Gift",
  "data": {
    "giftId": "uuid",
    "giftName": "小电视",
    "giftCount": 1,
    "userId": "uuid",
    "userName": "用户名",
    "totalCoin": 100
  }
}
```

---

## 8. SearchService API设计

### 8.1 搜索接口

#### 8.1.1 综合搜索

**接口**: `GET /api/search`

**权限**: 无需认证

**查询参数**:

```
?Keyword=关键词
&Type=Video
&SkipCount=0
&MaxResultCount=20
&CategoryId=uuid
&DurationMin=0
&DurationMax=3600
```

**响应**:

```json
{
  "keyword": "关键词",
  "items": [
    {
      "type": "Video",
      "data": {
        "id": "uuid",
        "title": "视频标题",
        "cover": "...",
        "duration": 120,
        "viewCount": 10000
      }
    },
    {
      "type": "User",
      "data": {
        "id": "uuid",
        "name": "用户名",
        "avatar": "...",
        "followerCount": 1000
      }
    }
  ],
  "totalCount": 100,
  "suggestions": ["关键词1", "关键词2"]
}
```

---

#### 8.1.2 视频搜索

**接口**: `GET /api/search/videos`

**权限**: 无需认证

---

#### 8.1.3 用户搜索

**接口**: `GET /api/search/users`

**权限**: 无需认证

---

#### 8.1.4 搜索建议

**接口**: `GET /api/search/suggestions`

**权限**: 无需认证

**查询参数**:

```
?Keyword=关键词前缀
```

**响应**:

```json
{
  "suggestions": [
    "关键词1",
    "关键词2",
    "关键词3"
  ]
}
```

---

## 9. RecommendService API设计

### 9.1 推荐接口

#### 9.1.1 获取首页推荐

**接口**: `GET /api/recommend/home`

**权限**: 无需认证（基于用户历史）

**响应**:

```json
{
  "items": [
    {
      "id": "uuid",
      "title": "推荐视频",
      "cover": "...",
      "viewCount": 10000,
      "recommendReason": "相似兴趣"
    }
  ],
  "totalCount": 20
}
```

---

#### 9.1.2 获取热门视频

**接口**: `GET /api/recommend/hot`

**权限**: 无需认证

---

#### 9.1.3 获取相关视频推荐

**接口**: `GET /api/recommend/videos/{videoId}/related`

**权限**: 无需认证

---

## 10. CategoryService API设计

### 10.1 分类接口

#### 10.1.1 获取分类列表

**接口**: `GET /api/category/categories`

**权限**: 无需认证

**响应**:

```json
{
  "items": [
    {
      "id": "uuid",
      "name": "animation",
      "displayName": "动画",
      "icon": "icons/animation.png",
      "videoCount": 50000,
      "children": [
        {
          "id": "uuid",
          "name": "anime",
          "displayName": "动漫",
          "videoCount": 30000
        }
      ]
    }
  ]
}
```

---

#### 10.1.2 获取分类视频

**接口**: `GET /api/category/categories/{categoryId}/videos`

**权限**: 无需认证

---

## 11. AdminService API设计（继承现有BackendAdmin）

### 11.1 视频审核接口

#### 11.1.1 获取待审核视频列表

**接口**: `GET /api/admin/videos/pending`

**权限**: 需超级管理员权限

---

#### 11.1.2 审核视频

**接口**: `POST /api/admin/videos/{videoId}/audit`

**权限**: 需超级管理员权限

**请求体**:

```json
{
  "status": "Approved",
  "reason": "审核通过"
}
```

---

### 11.2 评论审核接口

#### 11.2.1 获取待审核评论

**接口**: `GET /api/admin/comments/pending`

**权限**: 需超级管理员权限

---

#### 11.2.2 审核评论

**接口**: `POST /api/admin/comments/{commentId}/audit`

**权限**: 需超级管理员权限

---

### 11.3 弹幕审核接口

#### 11.3.1 处理弹幕举报

**接口**: `POST /api/admin/danmaku/reports/{reportId}/handle`

**权限**: 需超级管理员权限

---

## 12. API网关配置（继承现有Ocelot）

### 12.1 路由配置示例

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/video/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "video-service",
          "Port": 5001
        }
      ],
      "UpstreamPathTemplate": "/api/video/{everything}",
      "UpstreamHttpMethod": ["Get", "Post", "Put", "Delete"],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "IdentityServer",
        "AllowedScopes": ["video-service"]
      },
      "RateLimitOptions": {
        "ClientWhitelist": [],
        "EnableRateLimiting": true,
        "Period": "1s",
        "PeriodTimespan": 1,
        "Limit": 100
      }
    },
    
    {
      "DownstreamPathTemplate": "/ws/danmaku",
      "DownstreamScheme": "ws",
      "DownstreamHostAndPorts": [
        {
          "Host": "danmaku-service",
          "Port": 5002
        }
      ],
      "UpstreamPathTemplate": "/ws/danmaku"
    }
  ]
}
```

---

## 13. 权限定义示例（继承ABP Permission）

### 13.1 视频服务权限

```csharp
// VideoPermissions.cs
using Volo.Abp.Authorization.Permissions;

namespace LCH.MicroService.Video.Authorization
{
    public class VideoPermissions : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var videoGroup = context.AddGroup(VideoPermissions.GroupName, "视频管理");
            
            var videos = videoGroup.AddPermission(VideoPermissions.Videos.Default, "视频管理");
            videos.AddChild(VideoPermissions.Videos.Create, "创建视频");
            videos.AddChild(VideoPermissions.Videos.Upload, "上传视频");
            videos.AddChild(VideoPermissions.Videos.Update, "更新视频");
            videos.AddChild(VideoPermissions.Videos.Delete, "删除视频");
            videos.AddChild(VideoPermissions.Videos.Publish, "发布视频");
            
            var audit = videoGroup.AddPermission(VideoPermissions.Audit.Default, "视频审核");
            audit.AddChild(VideoPermissions.Audit.Approve, "审核通过");
            audit.AddChild(VideoPermissions.Audit.Reject, "审核拒绝");
        }
        
        public static class Videos
        {
            public const string Default = "Video.Videos";
            public const string Create = "Video.Videos.Create";
            public const string Upload = "Video.Videos.Upload";
            public const string Update = "Video.Videos.Update";
            public const string Delete = "Video.Videos.Delete";
            public const string Publish = "Video.Videos.Publish";
        }
        
        public static class Audit
        {
            public const string Default = "Video.Audit";
            public const string Approve = "Video.Audit.Approve";
            public const string Reject = "Video.Audit.Reject";
        }
    }
}
```

---

## 14. DTO设计示例

### 14.1 VideoDto（继承ABP EntityDto）

```csharp
// VideoDto.cs
using Volo.Abp.Application.Dtos;

namespace LCH.MicroService.Video.Application.Contracts.Dtos
{
    public class VideoDto : FullAuditedEntityDto<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        
        public string CoverPath { get; set; }
        public int? Duration { get; set; }
        
        public long ViewCount { get; set; }
        public int LikeCount { get; set; }
        public int CoinCount { get; set; }
        public int FavoriteCount { get; set; }
        public int ShareCount { get; set; }
        public int CommentCount { get; set; }
        public int DanmakuCount { get; set; }
        
        public DateTime? PublishTime { get; set; }
        public bool IsOriginal { get; set; }
        public List<string> Tags { get; set; }
        
        public UserSimpleDto Author { get; set; }
        public CategoryDto Category { get; set; }
    }
    
    public class CreateVideoDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid? CategoryId { get; set; }
        public List<string> Tags { get; set; }
        public bool IsOriginal { get; set; }
        public string SourceUrl { get; set; }
    }
    
    public class UpdateVideoDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid? CategoryId { get; set; }
        public List<string> Tags { get; set; }
    }
    
    // 分页查询DTO（继承ABP标准）
    public class GetVideoListDto : PagedAndSortedResultRequestDto
    {
        public Guid? CategoryId { get; set; }
        public Guid? UserId { get; set; }
        public string Status { get; set; }
        public string Keyword { get; set; }
    }
}
```

---

## 15. 客户端调用示例

### 15.1 JavaScript/TypeScript客户端

```typescript
// 使用ABP Dynamic C# Client Proxy（推荐）
import { VideoAppService } from './api/video';

// 或使用标准HTTP客户端
class VideoClient {
    private baseUrl = '/api/video';
    
    async createVideo(data: CreateVideoDto): Promise<CreateVideoResultDto> {
        const response = await fetch(`${this.baseUrl}/videos`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });
        
        return response.json();
    }
    
    async uploadChunk(sessionId: string, chunkIndex: number, file: Blob): Promise<UploadChunkResultDto> {
        const formData = new FormData();
        formData.append('file', file);
        formData.append('md5', await this.calculateMd5(file));
        
        const response = await fetch(`${this.baseUrl}/upload/${sessionId}/chunk/${chunkIndex}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            body: formData
        });
        
        return response.json();
    }
    
    async getVideoList(params: GetVideoListDto): Promise<PagedResultDto<VideoDto>> {
        const query = new URLSearchParams({
            SkipCount: params.skipCount.toString(),
            MaxResultCount: params.maxResultCount.toString(),
            Sorting: params.sorting
        });
        
        const response = await fetch(`${this.baseUrl}/videos?${query}`);
        return response.json();
    }
}
```

---

## 16. API文档工具

### 16.1 Swagger配置（继承ABP）

```csharp
// VideoServiceModule.cs
public override void ConfigureServices(ServiceConfigurationContext context)
{
    context.Services.AddAbpSwaggerGen(options =>
    {
        options.SwaggerDoc("video", new OpenApiInfo
        {
            Title = "Video Service API",
            Version = "v1",
            Description = "Bilibili视频管理服务API"
        });
        
        options.DocInclusionPredicate((docName, description) => true);
        options.CustomSchemaIds(type => type.FullName);
    });
}

public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var app = context.GetApplicationBuilder();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/video/swagger.json", "Video Service API");
    });
}
```

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ API接口设计完成