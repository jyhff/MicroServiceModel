# 03-弹幕系统详细PRD

## 1. 模块概述

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 弹幕并发 | 无限制 |
| 弹幕存储 | PostgreSQL分区表 |
| 高级弹幕 | LV2+可使用 |

---

## 2. 功能详细设计

### 2.1 弹幕发送

#### 2.1.1 WebSocket连接协议

```
WebSocket URL: wss://api.bilibili.com/ws/danmaku?videoId={videoId}

连接流程:
1. 客户端建立WebSocket连接
2. 发送认证消息 {type: "auth", token: "jwt_token"}
3. 服务器验证Token，返回认证成功
4. 客户端发送 {type: "join", videoId: "uuid"}
5. 服务器返回最近100条弹幕
6. 客户端可发送弹幕，接收实时弹幕
```

---

#### 2.1.2 WebSocket消息格式

**认证消息**:
```json
{
  "type": "auth",
  "token": "jwt_access_token"
}

响应:
{
  "type": "auth_success",
  "userId": "uuid",
  "level": 3
}
```

---

**加入房间消息**:
```json
{
  "type": "join",
  "videoId": "uuid"
}

响应:
{
  "type": "joined",
  "videoId": "uuid",
  "userCount": 125,
  "recentDanmaku": [
    {
      "id": "uuid",
      "content": "弹幕内容",
      "time": 45.5,
      "type": 0,
      "color": "#FFFFFF",
      "size": 24,
      "user": {
        "id": "uuid",
        "nickname": "用户",
        "level": 2
      }
    }
  ]
}
```

---

**发送弹幕消息**:
```json
{
  "type": "send",
  "videoId": "uuid",
  "content": "弹幕内容",
  "time": 125.5,
  "type": 0,                    // 0=滚动,1=顶部,2=底部
  "color": "#FFFFFF",
  "size": 24,
  "isAdvanced": false
}

响应（广播到房间所有用户）:
{
  "type": "danmaku",
  "id": "uuid",
  "content": "弹幕内容",
  "time": 125.5,
  "type": 0,
  "color": "#FFFFFF",
  "size": 24,
  "isAdvanced": false,
  "user": {
    "id": "uuid",
    "nickname": "用户",
    "level": 3,
    "isVerified": false
  },
  "timestamp": 1704100800000
}
```

---

**错误消息**:
```json
{
  "type": "error",
  "code": 30001,
  "message": "发送过于频繁"
}
```

---

### 2.2 SignalR实现代码

```csharp
public class DanmakuHub : Hub<IDanmakuClient>
{
    private readonly IUserRepository _userRepo;
    private readonly IDanmakuRepository _danmakuRepo;
    private readonly IExperienceService _expService;
    private readonly IDistributedCache _cache;
    private readonly IContentFilter _contentFilter;
    
    // 用户连接到视频房间
    public async Task JoinVideo(Guid videoId)
    {
        // 1. 获取用户信息
        var userId = Context.UserIdentifier;
        var user = await _userRepo.GetAsync(userId);
        
        // 2. 加入房间组
        await Groups.AddToGroupAsync(Context.ConnectionId, $"video_{videoId}");
        
        // 3. 获取最近100条弹幕
        var recentDanmaku = await _danmakuRepo.GetRecentAsync(videoId, 100);
        
        // 4. 发送给客户端
        await Clients.Caller.Joined(new JoinedResult
        {
            VideoId = videoId,
            UserCount = await GetUserCount(videoId),
            RecentDanmaku = recentDanmaku
        });
        
        // 5. 更新房间人数
        await UpdateUserCount(videoId);
    }
    
    // 用户离开房间
    public async Task LeaveVideo(Guid videoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video_{videoId}");
        await UpdateUserCount(videoId);
    }
    
    // 发送弹幕
    public async Task SendDanmaku(SendDanmakuInput input)
    {
        var userId = Context.UserIdentifier;
        
        // 1. 检查发送频率（每秒最多1条）
        var rateLimitKey = $"danmaku:rate:{userId}";
        var lastSendTime = await _cache.GetAsync(rateLimitKey);
        
        if (lastSendTime.HasValue &&
            DateTime.UtcNow - new DateTime(long.Parse(lastSendTime)) < TimeSpan.FromSeconds(1))
        {
            await Clients.Caller.Error("30001", "发送过于频繁");
            return;
        }
        
        // 2. 检查等级权限（LV1+可发送弹幕）
        var user = await _userRepo.GetAsync(userId);
        if (user.Level < 1)
        {
            await Clients.Caller.Error("30002", "等级不足，LV1+可发送弹幕");
            return;
        }
        
        // 3. 高级弹幕检查（LV2+）
        if (input.IsAdvanced && user.Level < 2)
        {
            await Clients.Caller.Error("30003", "等级不足，LV2+可使用高级弹幕");
            return;
        }
        
        // 4. 内容审核（敏感词过滤）
        var filterResult = await _contentFilter.Filter(input.Content);
        if (!filterResult.IsValid)
        {
            await Clients.Caller.Error("30004", "弹幕内容包含敏感词");
            return;
        }
        
        // 5. 保存弹幕
        var danmaku = new Danmaku
        {
            Id = Guid.NewGuid(),
            VideoId = input.VideoId,
            UserId = userId,
            Content = filterResult.Content,
            Time = input.Time,
            Type = input.Type,
            Color = input.Color,
            Size = input.Size,
            IsAdvanced = input.IsAdvanced,
            IP = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString(),
            CreationTime = DateTime.UtcNow
        };
        
        await _danmakuRepo.InsertAsync(danmaku);
        
        // 6. 更新频率限制
        await _cache.SetAsync(rateLimitKey, DateTime.UtcNow.Ticks.ToString(), TimeSpan.FromSeconds(1));
        
        // 7. 广播到房间所有用户
        await Clients.Group($"video_{input.VideoId}").ReceiveDanmaku(new DanmakuDto
        {
            Id = danmaku.Id,
            Content = danmaku.Content,
            Time = danmaku.Time,
            Type = danmaku.Type,
            Color = danmaku.Color,
            Size = danmaku.Size,
            IsAdvanced = danmaku.IsAdvanced,
            User = new UserDto
            {
                Id = user.Id,
                Nickname = user.Nickname,
                Level = user.Level,
                IsVerified = user.IsVerified
            },
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        
        // 8. 增加经验值
        await _expService.AddExperienceAsync(userId, ExperienceAction.SendDanmaku, input.VideoId);
        
        // 9. 更新弹幕数
        await UpdateDanmakuCount(input.VideoId);
    }
    
    private async Task<int> GetUserCount(Guid videoId)
    {
        var key = $"video:user_count:{videoId}";
        var count = await _cache.GetAsync(key);
        return count.HasValue ? int.Parse(count) : 0;
    }
    
    private async Task UpdateUserCount(Guid videoId)
    {
        var key = $"video:user_count:{videoId}";
        await _cache.IncrementAsync(key);
    }
}
```

---

## 3. 数据库设计

### 3.1 Danmakus表（弹幕表）

```sql
CREATE TABLE "Danmakus" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" uuid NOT NULL REFERENCES "Videos"("Id"),
    "UserId" uuid REFERENCES "Users"("Id"),
    "Content" varchar(200) NOT NULL,
    "Time" decimal(10, 3) NOT NULL,         -- 视频时间点（秒，支持小数）
    "Type" integer DEFAULT 0,                -- 0=滚动,1=顶部,2=底部
    "Color" varchar(7) DEFAULT '#FFFFFF',
    "Size" integer DEFAULT 24,
    "IsAdvanced" boolean DEFAULT false,
    "IP" varchar(50) NOT NULL,
    "IsDeleted" boolean DEFAULT false,
    "DeletionReason" varchar(200),
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW()
) PARTITION BY RANGE ("CreationTime");

-- 按月份分区
CREATE TABLE "Danmakus_2024_06" PARTITION OF "Danmakus"
FOR VALUES FROM ('2024-06-01') TO ('2024-07-01');

CREATE INDEX "IX_Danmakus_VideoId_Time" ON "Danmakus"("VideoId", "Time") 
WHERE "IsDeleted" = false;
CREATE INDEX "IX_Danmakus_UserId" ON "Danmakus"("UserId");
CREATE INDEX "IX_Danmakus_CreationTime" ON "Danmakus"("CreationTime" DESC);
```

---

## 4. 错误码定义

| 错误码 | 说明 |
|--------|------|
| 30001 | 发送过于频繁 |
| 30002 | 等级不足，LV1+可发送弹幕 |
| 30003 | 等级不足，LV2+可使用高级弹幕 |
| 30004 | 弹幕内容包含敏感词 |
| 30005 | 弹幕内容过长（>200字符） |

---

**文档版本**: v2.0  
**状态**: ✅ 详细设计完成