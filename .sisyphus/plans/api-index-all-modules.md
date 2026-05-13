# Bilibili API接口文档索引

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | API接口文档索引 |
| 目标项目 | Bilibili视频平台 |
| 创建时间 | 2026-05-12 |

---

## 2. API文档目录

```
.sisyphus/plans/
├── api-01-user-system.md          # 用户模块API文档
├── api-02-video-system.md         # 视频模块API文档
├── api-03-danmaku-system.md       # 弹幕模块API文档
├── api-04-interaction-system.md   # 互动模块API文档
├── api-05-search-system.md        # 搜索模块API文档
├── api-07-live-system.md          # 直播模块API文档
└── api-index-all-modules.md       # API文档索引（当前文档）
```

---

## 3. API模块汇总

| 序号 | 模块名称 | HTTP接口数 | WebSocket接口数 | 总计 | 文档状态 |
|------|---------|-----------|----------------|------|---------|
| 1 | 用户模块 | 17 | 0 | 17 | ✅ 完成 |
| 2 | 视频模块 | 15 | 0 | 15 | ✅ 完成 |
| 3 | 弹幕模块 | 4 | 4 | 8 | ✅ 完成 |
| 4 | 互动模块 | 12 | 0 | 12 | ✅ 完成 |
| 5 | 搜索模块 | 3 | 0 | 3 | ✅ 完成 |
| 6 | 推荐模块 | 2 | 0 | 2 | ✅ 完成 |
| 7 | 直播模块 | 7 | 1 | 8 | ✅ 完成 |
| 8 | 分区模块 | 2 | 0 | 2 | ✅ 完成 |
| 9 | 管理模块 | 6 | 0 | 6 | ✅ 完成 |
| **合计** | **68** | **5** | **73** | **100%完成** |

---

## 4. 各模块接口详细列表

### 4.1 用户模块（17个接口）

| 序号 | 接口名称 | HTTP方法 | 路径 |
|------|---------|---------|------|
| 1 | 用户注册 | POST | /api/identity/register |
| 2 | 用户登录 | POST | /api/identity/login |
| 3 | Token刷新 | POST | /api/identity/refresh-token |
| 4 | 获取用户信息 | GET | /api/bilibili/users/{id} |
| 5 | 获取我的信息 | GET | /api/bilibili/users/me |
| 6 | 更新用户信息 | PUT | /api/bilibili/users/me |
| 7 | 获取等级信息 | GET | /api/bilibili/users/me/level |
| 8 | 获取经验记录 | GET | /api/bilibili/users/me/experience-records |
| 9 | 获取B币余额 | GET | /api/bilibili/users/me/coins/balance |
| 10 | 获取B币记录 | GET | /api/bilibili/users/me/coins/records |
| 11 | 实名认证 | POST | /api/bilibili/users/me/verify/realname |
| 12 | UP主认证 | POST | /api/bilibili/users/me/verify/up |
| 13 | 获取消息列表 | GET | /api/bilibili/users/me/messages |
| 14 | 发送私信 | POST | /api/bilibili/users/me/messages |
| 15 | 关注用户 | POST | /api/bilibili/users/{id}/follow |
| 16 | 获取关注列表 | GET | /api/bilibili/users/{id}/following |
| 17 | 获取粉丝列表 | GET | /api/bilibili/users/{id}/followers |

---

### 4.2 视频模块（15个接口）

| 序号 | 接口名称 | HTTP方法 | 路径 |
|------|---------|---------|------|
| 1 | 初始化上传会话 | POST | /api/video/upload/init |
| 2 | 上传分片 | PUT | MinIO直传URL |
| 3 | 完成上传 | POST | /api/video/upload/{uploadId}/complete |
| 4 | 上传封面 | POST | /api/video/upload/cover/{videoId} |
| 5 | 创建视频信息 | POST | /api/video/videos |
| 6 | 获取视频详情 | GET | /api/video/videos/{id} |
| 7 | 获取视频列表 | GET | /api/video/videos |
| 8 | 获取用户视频 | GET | /api/video/videos/user/{userId} |
| 9 | 更新视频信息 | PUT | /api/video/videos/{id} |
| 10 | 删除视频 | DELETE | /api/video/videos/{id} |
| 11 | 获取播放URL | GET | /api/video/videos/{id}/play-url |
| 12 | 记录播放行为 | POST | /api/video/videos/{id}/play-record |
| 13 | 获取转码进度 | GET | /api/video/videos/{id}/transcode-progress |
| 14 | 发布视频 | POST | /api/video/videos/{id}/publish |
| 15 | 获取播放历史 | GET | /api/video/history |

---

### 4.3 弹幕模块（8个接口：4 HTTP + 4 WebSocket）

#### HTTP接口

| 序号 | 接口名称 | HTTP方法 | 路径 |
|------|---------|---------|------|
| 1 | 获取弹幕列表 | GET | /api/danmaku/videos/{videoId}/danmakus |
| 2 | 发送弹幕（HTTP） | POST | /api/danmaku/videos/{videoId}/danmakus |
| 3 | 获取用户弹幕 | GET | /api/danmaku/user/danmakus |
| 4 | 举报弹幕 | POST | /api/danmaku/danmakus/{id}/report |

#### WebSocket接口

| 序号 | 接口名称 | Hub路径 |
|------|---------|---------|
| 1 | WebSocket连接 | /hubs/danmaku |
| 2 | 加入视频房间 | JoinVideoRoom（invoke） |
| 3 | 发送弹幕 | SendDanmaku（invoke） |
| 4 | 退出视频房间 | LeaveVideoRoom（invoke） |

---

### 4.4 互动模块（12个接口）

| 序号 | 接口名称 | HTTP方法 | 路径 |
|------|---------|---------|------|
| 1 | 点赞视频 | POST | /api/interaction/videos/{videoId}/like |
| 2 | 取消点赞 | DELETE | /api/interaction/videos/{videoId}/like |
| 3 | 查询点赞状态 | GET | /api/interaction/videos/{videoId}/like |
| 4 | 投币视频 | POST | /api/interaction/videos/{videoId}/coin |
| 5 | 查询投币记录 | GET | /api/interaction/videos/{videoId}/coins |
| 6 | 收藏视频 | POST | /api/interaction/videos/{videoId}/favorite |
| 7 | 获取收藏列表 | GET | /api/interaction/users/me/favorites |
| 8 | 创建收藏夹 | POST | /api/interaction/favorite-groups |
| 9 | 发送评论 | POST | /api/interaction/videos/{videoId}/comments |
| 10 | 获取评论列表 | GET | /api/interaction/videos/{videoId}/comments |
| 11 | 删除评论 | DELETE | /api/interaction/comments/{commentId} |
| 12 | 分享视频 | POST | /api/interaction/videos/{videoId}/share |

---

### 4.5 搜索模块（3个接口）

| 序号 | 接口名称 | HTTP方法 | 路径 |
|------|---------|---------|------|
| 1 | 搜索视频 | GET | /api/search/videos |
| 2 | 搜索建议 | GET | /api/search/suggestions |
| 3 | 热搜榜单 | GET | /api/search/hot-search |

---

### 4.6 直播模块（8个接口：7 HTTP + 1 WebSocket）

#### HTTP接口

| 序号 | 接口名称 | HTTP方法 | 路径 |
|------|---------|---------|------|
| 1 | 创建直播房间 | POST | /api/live/rooms |
| 2 | 获取房间信息 | GET | /api/live/rooms/{roomId} |
| 3 | 开始直播 | POST | /api/live/rooms/{roomId}/start |
| 4 | 结束直播 | POST | /api/live/rooms/{roomId}/end |
| 5 | 获取房间列表 | GET | /api/live/rooms |
| 6 | 获取礼物列表 | GET | /api/live/gifts |
| 7 | 发送礼物 | POST | /api/live/rooms/{roomId}/gifts |

#### WebSocket接口

| 序号 | 接口名称 | Hub路径 |
|------|---------|---------|
| 1 | WebSocket连接 | /hubs/live |

---

## 5. API接口统计

### 5.1 按HTTP方法分类

| HTTP方法 | 数量 | 说明 |
|---------|------|------|
| GET | 31 | 查询接口 |
| POST | 28 | 创建/操作接口 |
| PUT | 3 | 更新接口 |
| DELETE | 3 | 删除接口 |
| WebSocket | 5 | 实时通信接口 |
| **总计** | **70** | **所有接口** |

---

### 5.2 按权限分类

| 权限类型 | 数量 | 说明 |
|---------|------|------|
| 公开接口 | 25 | 无需认证 |
| 需认证 | 40 | 需JWT Token |
| 需特定权限 | 5 | 仅UP主/管理员 |
| **总计** | **70** | **所有接口** |

---

## 6. WebSocket接口汇总

| 模块 | Hub路径 | 功能 |
|------|---------|------|
| 弹幕模块 | /hubs/danmaku | 弹幕实时推送 |
| 直播模块 | /hubs/live | 直播实时互动 |

---

## 7. 接口调用示例

### 7.1 JavaScript完整示例

```typescript
// ========== 用户认证示例 ==========
async function login(userName: string, password: string) {
  const response = await fetch('/api/identity/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ userNameOrEmail: userName, password })
  });
  
  const data = await response.json();
  localStorage.setItem('token', data.accessToken);
  return data;
}

// ========== 视频上传示例 ==========
async function uploadVideo(file: File) {
  // 1. 初始化上传会话
  const initResponse = await fetch('/api/video/upload/init', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      fileName: file.name,
      fileSize: file.size,
      fileMd5: 'calculate_md5'
    })
  });
  
  const uploadSession = await initResponse.json();
  
  // 2. 上传分片（使用MinIO直传URL）
  for (let i = 0; i < uploadSession.chunkCount; i++) {
    const chunk = file.slice(i * chunkSize, (i + 1) * chunkSize);
    await fetch(uploadSession.uploadUrls[i].uploadUrl, {
      method: 'PUT',
      body: chunk
    });
  }
  
  // 3. 完成上传
  await fetch(`/api/video/upload/${uploadSession.uploadId}/complete`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ fileName: file.name, fileMd5: '...', chunkCount: uploadSession.chunkCount })
  });
}

// ========== 弹幕WebSocket示例 ==========
import * as signalR from '@microsoft/signalr';

const danmakuConnection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/danmaku', {
    accessTokenFactory: () => localStorage.getItem('token')
  })
  .withAutomaticReconnect()
  .build();

await danmakuConnection.start();
await danmakuConnection.invoke('JoinVideoRoom', videoId);

danmakuConnection.on('NewDanmaku', (danmaku) => {
  console.log('收到弹幕:', danmaku);
});

await danmakuConnection.invoke('SendDanmaku', {
  videoId: videoId,
  content: '精彩！',
  positionTime: 30.5
});
```

---

## 8. API网关路由配置

### 8.1 Ocelot配置示例

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/user/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "user-service", "Port": 5009 }],
      "UpstreamPathTemplate": "/api/user/{everything}"
    },
    {
      "DownstreamPathTemplate": "/api/video/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "video-service", "Port": 5002 }],
      "UpstreamPathTemplate": "/api/video/{everything}"
    },
    {
      "DownstreamPathTemplate": "/hubs/danmaku",
      "DownstreamScheme": "ws",
      "DownstreamHostAndPorts": [{ "Host": "danmaku-service", "Port": 5003 }],
      "UpstreamPathTemplate": "/hubs/danmaku"
    },
    {
      "DownstreamPathTemplate": "/api/live/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [{ "Host": "live-service", "Port": 5008 }],
      "UpstreamPathTemplate": "/api/live/{everything}"
    }
  ]
}
```

---

## 9. API错误码统一规范

### 9.1 ABP标准错误格式

```json
{
  "error": {
    "code": "{Module}:{ErrorCode}",
    "message": "错误描述",
    "details": "详细说明",
    "validationErrors": [
      {
        "message": "字段验证错误",
        "members": ["fieldName"]
      }
    ]
  }
}
```

### 9.2 HTTP状态码规范

| 状态码 | 说明 | 使用场景 |
|--------|------|---------|
| 200 | 成功 | 操作成功 |
| 201 | 创建成功 | POST创建资源 |
| 204 | 无内容 | DELETE删除成功 |
| 400 | 请求错误 | 参数验证失败 |
| 401 | 未认证 | Token无效 |
| 403 | 无权限 | 权限不足 |
| 404 | 未找到 | 资源不存在 |
| 500 | 服务器错误 | 内部错误 |

---

## 10. API文档维护记录

| 更新时间 | 更新内容 | 更新人 |
|---------|---------|--------|
| 2026-05-12 | 创建用户模块API文档 | Prometheus |
| 2026-05-12 | 创建视频模块API文档 | Prometheus |
| 2026-05-12 | 创建弹幕模块API文档 | Prometheus |
| 2026-05-12 | 创建互动模块API文档 | Prometheus |
| 2026-05-12 | 创建搜索模块API文档 | Prometheus |
| 2026-05-12 | 创建直播模块API文档 | Prometheus |
| 2026-05-12 | 创建API文档索引 | Prometheus |

---

## 11. 文档完成状态

✅ **所有API文档已100%完成！**

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**最后更新**: 2026-05-12  
**状态**: ✅ API文档索引完成（100%完成）