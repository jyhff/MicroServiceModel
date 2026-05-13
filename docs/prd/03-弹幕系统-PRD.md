# 弹幕系统产品需求文档 (PRD)

## 1. 模块概述

### 1.1 模块定位
弹幕系统是Bilibili最具特色的功能之一，支持用户在观看视频时发送实时评论，并以滚动文字形式显示在视频画面上，营造独特的社区氛围。

### 1.2 核心功能
- **实时弹幕**：WebSocket实时发送和接收
- **弹幕池管理**：按视频时间轴组织和存储
- **弹幕样式**：多种样式（颜色、大小、位置）
- **高级弹幕**：会员专属特效弹幕
- **弹幕屏蔽**：关键词、用户屏蔽
- **弹幕密度**：可调节显示密度

---

## 2. 功能详细设计

### 2.1 弹幕发送

#### 2.1.1 弹幕属性
| 属性 | 类型 | 限制 | 说明 |
|------|------|------|------|
| 内容 | string | 2-100字符 | 弹幕文本内容 |
| 颜色 | string | HEX格式 | 文字颜色（#FFFFFF） |
| 位置 | enum | 滚动/顶部/底部 | 弹幕显示位置 |
| 字号 | enum | 小/中/大 | 文字大小 |
| 时间 | float | 0-视频时长 | 弹幕出现时间（秒） |
| 类型 | enum | 普通/高级 | 普通用户/会员 |

#### 2.1.2 发送API
```http
POST /api/danmaku/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "videoId": "uuid",
  "content": "弹幕内容",
  "color": "#FFFFFF",
  "position": "scroll|top|bottom",
  "fontSize": "small|medium|large",
  "time": 45.5,           // 弹幕出现时间（秒）
  "mode": "normal|advanced"
}

Response:
{
  "id": "uuid",
  "content": "弹幕内容",
  "color": "#FFFFFF",
  "position": "scroll",
  "fontSize": "medium",
  "time": 45.5,
  "mode": "normal",
  "userId": "uuid",
  "username": "用户名",
  "createdAt": "2024-01-01T12:00:00Z"
}
```

#### 2.1.3 发送频率限制
```csharp
public class DanmakuRateLimiter
{
    // 同一用户每分钟最多发送20条
    public async Task<bool> CanSendAsync(Guid userId, Guid videoId)
    {
        var key = $"danmaku:limit:{userId}";
        var count = await _redisDb.StringIncrementAsync(key);
        
        if (count == 1)
        {
            // 设置1分钟过期
            await _redisDb.KeyExpireAsync(key, TimeSpan.FromMinutes(1));
        }
        
        return count <= 20;  // 每分钟最多20条
    }
}
```

### 2.2 弹幕接收

#### 2.2.1 WebSocket连接
```javascript
// 客户端连接
const socket = new WebSocket('wss://api.example.com/ws/danmaku');

// 连接后发送认证
socket.onopen = () => {
    socket.send(JSON.stringify({
        type: 'auth',
        token: 'jwt_token'
    }));
};

// 进入视频房间
socket.send(JSON.stringify({
    type: 'join',
    videoId: 'video_uuid'
}));

// 发送弹幕
socket.send(JSON.stringify({
    type: 'danmaku',
    videoId: 'video_uuid',
    content: '弹幕内容',
    time: 45.5,
    color: '#FFFFFF',
    position: 'scroll'
}));

// 接收弹幕
socket.onmessage = (event) => {
    const data = JSON.parse(event.data);
    // data: { type: 'danmaku', content: '...', time: 45.5, ... }
    renderDanmaku(data);
};
```

#### 2.2.2 WebSocket消息协议
```typescript
// 消息类型定义
interface DanmakuMessage {
    type: 'auth' | 'join' | 'leave' | 'danmaku' | 'heartbeat' | 'system';
    
    // auth消息
    token?: string;
    
    // join/leave消息
    videoId?: string;
    
    // danmaku消息
    content?: string;
    time?: number;
    color?: string;
    position?: 'scroll' | 'top' | 'bottom';
    fontSize?: 'small' | 'medium' | 'large';
    mode?: 'normal' | 'advanced';
}

// 服务端广播消息
interface DanmakuBroadcast {
    type: 'danmaku';
    id: string;
    userId: string;
    username: string;
    content: string;
    time: number;
    color: string;
    position: string;
    fontSize: string;
    mode: string;
    timestamp: string;
}
```

#### 2.2.3 SignalR实现
```csharp
// SignalR Hub
public class DanmakuHub : Hub<IDanmakuClient>
{
    private readonly IDanmakuService _danmakuService;
    private readonly IVideoService _videoService;
    
    // 用户加入视频房间
    public async Task JoinVideo(string videoId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"video:{videoId}");
        
        // 发送最近100条弹幕
        var recentDanmaku = await _danmakuService.GetRecentDanmakuAsync(videoId, 100);
        await Clients.Caller.SendAsync("RecentDanmaku", recentDanmaku);
    }
    
    // 用户离开视频
    public async Task LeaveVideo(string videoId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"video:{videoId}");
    }
    
    // 发送弹幕
    public async Task SendDanmaku(SendDanmakuInput input)
    {
        // 1. 检查频率限制
        var userId = Context.UserIdentifier.ToGuid();
        if (!await _danmakuService.CanSendAsync(userId, input.VideoId))
        {
            await Clients.Caller.SendAsync("Error", "发送过于频繁，请稍后再试");
            return;
        }
        
        // 2. 内容审核
        if (await _contentFilter.ContainsSensitiveWord(input.Content))
        {
            await Clients.Caller.SendAsync("Error", "弹幕内容包含敏感词");
            return;
        }
        
        // 3. 保存弹幕
        var danmaku = await _danmakuService.CreateAsync(input, userId);
        
        // 4. 广播到视频房间
        await Clients.Group($"video:{input.VideoId}").ReceiveDanmaku(danmaku);
        
        // 5. 更新弹幕数
        await _videoService.IncrementDanmakuCount(input.VideoId);
    }
}

// 客户端接口
public interface IDanmakuClient
{
    Task ReceiveDanmaku(DanmakuDto danmaku);
    Task RecentDanmaku(List<DanmakuDto> danmakuList);
    Task Error(string message);
}
```

### 2.3 弹幕存储

#### 2.3.1 存储方案
**方案选择**: PostgreSQL + Redis

- **PostgreSQL**: 持久化存储，按videoId分表
- **Redis**: 热点视频弹幕缓存，最近1小时弹幕

#### 2.3.2 数据库设计
```sql
-- 弹幕表
CREATE TABLE "Danmakus" (
    "Id" uuid PRIMARY KEY,
    "VideoId" uuid NOT NULL REFERENCES "Videos"("Id"),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Content" varchar(200) NOT NULL,
    "Color" varchar(7) DEFAULT '#FFFFFF',      -- 颜色，默认白色
    "Position" integer DEFAULT 0,              -- 0=滚动,1=顶部,2=底部
    "FontSize" integer DEFAULT 1,              -- 0=小,1=中,2=大
    "Time" decimal(10,3) NOT NULL,             -- 出现时间（秒，支持小数）
    "Mode" integer DEFAULT 0,                  -- 0=普通,1=高级
    "Status" integer DEFAULT 0,                -- 0=正常,1=被删除,2=被屏蔽
    "CreatedAt" timestamp with time zone NOT NULL,
    "ClientIp" varchar(50),                    -- 客户端IP
    "IsDeleted" boolean DEFAULT false
);

-- 按videoId分区索引
CREATE INDEX "IX_Danmakus_VideoId_Time" ON "Danmakus"("VideoId", "Time") 
    WHERE "IsDeleted" = false AND "Status" = 0;
CREATE INDEX "IX_Danmakus_VideoId_CreatedAt" ON "Danmakus"("VideoId", "CreatedAt" DESC) 
    WHERE "IsDeleted" = false;
CREATE INDEX "IX_Danmakus_UserId" ON "Danmakus"("UserId") 
    WHERE "IsDeleted" = false;
```

#### 2.3.3 缓存策略
```csharp
public class DanmakuCacheService
{
    // 获取弹幕列表（优先从缓存）
    public async Task<List<DanmakuDto>> GetDanmakuAsync(string videoId, float startTime, float endTime)
    {
        var cacheKey = $"danmaku:{videoId}:{startTime}:{endTime}";
        
        // 1. 尝试从Redis获取
        var cached = await _redisDb.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<List<DanmakuDto>>(cached);
        }
        
        // 2. 从数据库查询
        var danmakuList = await _danmakuRepository.GetListAsync(
            videoId, startTime, endTime, maxResults: 1000);
        
        // 3. 存入缓存（5分钟）
        await _redisDb.StringSetAsync(cacheKey, 
            JsonSerializer.Serialize(danmakuList), 
            TimeSpan.FromMinutes(5));
        
        return danmakuList;
    }
    
    // 新弹幕写入时更新缓存
    public async Task InvalidateCacheAsync(string videoId)
    {
        // 删除该视频的所有弹幕缓存
        var pattern = $"danmaku:{videoId}:*";
        var keys = await _redisDb.ScriptEvaluateAsync(
            "return redis.call('keys', ARGV[1])", 
            null, 
            new RedisValue[] { pattern });
        
        if (keys is RedisResult[] keyArray)
        {
            foreach (var key in keyArray)
            {
                await _redisDb.KeyDeleteAsync((string)key);
            }
        }
    }
}
```

### 2.4 弹幕管理

#### 2.4.1 弹幕屏蔽
```csharp
// 关键词屏蔽
public class DanmakuFilterService
{
    private readonly ISet<string> _blockedWords;
    private readonly ISet<Guid> _blockedUsers;
    
    // 初始化时加载屏蔽词库
    public DanmakuFilterService()
    {
        _blockedWords = new HashSet<string>(
            _config.GetSection("Danmaku:BlockedWords").Get<string[]>(),
            StringComparer.OrdinalIgnoreCase);
    }
    
    // 检查弹幕是否合法
    public DanmakuCheckResult CheckDanmaku(string content, Guid userId)
    {
        // 1. 检查用户是否被屏蔽
        if (_blockedUsers.Contains(userId))
            return DanmakuCheckResult.Failed("用户被禁止发送弹幕");
        
        // 2. 检查内容长度
        if (content.Length < 2 || content.Length > 100)
            return DanmakuCheckResult.Failed("弹幕长度不符合要求");
        
        // 3. 检查敏感词
        foreach (var word in _blockedWords)
        {
            if (content.Contains(word, StringComparison.OrdinalIgnoreCase))
                return DanmakuCheckResult.Failed("弹幕包含敏感词");
        }
        
        return DanmakuCheckResult.Success();
    }
}
```

#### 2.4.2 用户屏蔽列表
```sql
-- 用户屏蔽列表
CREATE TABLE "DanmakuBlocks" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "TargetUserId" uuid REFERENCES "Users"("Id"),      -- 屏蔽的用户
    "Keyword" varchar(100),                             -- 屏蔽的关键词
    "BlockType" integer NOT NULL,                       -- 0=屏蔽用户,1=屏蔽关键词
    "CreatedAt" timestamp with time zone NOT NULL
);

CREATE UNIQUE INDEX "IX_DanmakuBlocks_UserId_TargetUserId" 
    ON "DanmakuBlocks"("UserId", "TargetUserId") 
    WHERE "BlockType" = 0;
```

### 2.5 高级弹幕

#### 2.5.1 会员弹幕特权
| 等级 | 特权 |
|------|------|
| LV0-LV1 | 基础弹幕（白色、滚动） |
| LV2-LV4 | 彩色弹幕、描边效果 |
| LV5 | 顶部/底部弹幕、大字号 |
| LV6 | 高级特效弹幕（会员专属） |
| 大会员 | 全部特效、专属颜色 |

#### 2.5.2 高级弹幕样式
```csharp
public class AdvancedDanmakuStyles
{
    // 描边效果
    public static DanmakuStyle Border(string color) => new DanmakuStyle
    {
        TextShadow = $"1px 1px 2px #000, -1px -1px 2px #000",
        FontWeight = "bold",
        Color = color
    };
    
    // 渐变色
    public static DanmakuStyle Gradient(string fromColor, string toColor) => new DanmakuStyle
    {
        Background = $"linear-gradient(to right, {fromColor}, {toColor})",
        WebkitBackgroundClip = "text",
        WebkitTextFillColor = "transparent",
        FontWeight = "bold"
    };
    
    // 阴影效果
    public static DanmakuStyle Shadow(string color) => new DanmakuStyle
    {
        TextShadow = $"2px 2px 4px rgba(0,0,0,0.5)",
        Color = color
    };
}
```

---

## 3. API设计

### 3.1 获取弹幕列表
```http
GET /api/danmaku/list?videoId={videoId}&startTime=0&endTime=60
Authorization: Bearer {token}

Response:
{
  "items": [
    {
      "id": "uuid",
      "userId": "uuid",
      "username": "弹幕发送者",
      "content": "弹幕内容",
      "color": "#FFFFFF",
      "position": "scroll",
      "fontSize": "medium",
      "time": 45.5,
      "mode": "normal",
      "createdAt": "2024-01-01T12:00:00Z"
    }
  ],
  "totalCount": 15680
}
```

### 3.2 删除弹幕（UP主/自己）
```http
DELETE /api/danmaku/{danmakuId}
Authorization: Bearer {token}

// 只能删除自己发的弹幕或自己视频下的弹幕
```

### 3.3 举报弹幕
```http
POST /api/danmaku/{danmakuId}/report
Authorization: Bearer {token}
Content-Type: application/json

{
  "reason": "spam|abuse|advertisement|other",
  "description": "举报说明"
}
```

---

## 4. 技术实现

### 4.1 技术栈
- **框架**: ABP Framework v10.0.2 + .NET 10.0
- **实时通信**: SignalR
- **缓存**: Redis（分布式缓存、Pub/Sub）
- **数据库**: PostgreSQL
- **WebSocket**: ASP.NET Core SignalR

### 4.2 SignalR配置
```csharp
// Program.cs
builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration.ConnectionString = redisConnectionString;
    })
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// 配置路由
app.MapHub<DanmakuHub>("/hubs/danmaku");
```

### 4.3 弹幕渲染（前端）
```typescript
// DPlayer弹幕配置
const player = new DPlayer({
    container: document.getElementById('player'),
    video: {
        url: 'video_url',
        type: 'hls'
    },
    danmaku: {
        id: 'video_uuid',
        api: '/api/danmaku/',
        user: 'user_id',
        bottom: '15%',
        unlimited: false
    }
});

// 手动添加弹幕
player.danmaku.draw({
    text: '弹幕内容',
    color: '#FFFFFF',
    type: 'right',  // right=滚动, top=顶部, bottom=底部
    size: 1         // 0=小, 1=中, 2=大
});
```

---

## 5. 性能优化

### 5.1 弹幕密度控制
```csharp
// 根据弹幕数量动态调整显示密度
public class DanmakuDensityController
{
    public int CalculateDensity(int totalDanmakuCount, int duration)
    {
        // 弹幕密度 = 弹幕总数 / 视频时长
        var density = totalDanmakuCount / (float)duration;
        
        if (density > 10) return 1;      // 太密集，只显示1%
        if (density > 5) return 5;       // 较密集，显示5%
        if (density > 2) return 20;      // 一般，显示20%
        return 100;                       // 稀疏，显示全部
    }
}
```

### 5.2 弹幕合并
- **相同内容合并**：连续相同弹幕显示为「内容 xN」
- **相似内容折叠**：相似弹幕合并显示

### 5.3 分片加载
- 视频分段加载弹幕（每30秒一段）
- 避免一次性加载全部弹幕

---

## 6. 监控指标

| 指标 | 告警阈值 | 说明 |
|------|---------|------|
| 弹幕发送延迟 | >500ms | WebSocket延迟过高 |
| 并发连接数 | >10万 | 连接数超限 |
| 弹幕丢失率 | >1% | 消息未成功送达 |
| 敏感词命中 | >100/分钟 | 需加强审核 |

---

## 7. 附录

### 7.1 错误码
| 错误码 | 描述 |
|--------|------|
| 30001 | 弹幕内容过长 |
| 30002 | 发送过于频繁 |
| 30003 | 包含敏感词 |
| 30004 | 用户被禁言 |
| 30005 | 视频已关闭弹幕 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0 + SignalR + PostgreSQL + Redis
