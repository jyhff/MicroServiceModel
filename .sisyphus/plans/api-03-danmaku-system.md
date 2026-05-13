# 弹幕模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | DanmakuService（弹幕服务） |
| 服务地址 | http://danmaku-service:5003 |
| WebSocket地址 | ws://danmaku-service:5003/hubs/danmaku |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. 模块概述

### 2.1 服务职责

- 弹幕实时推送（WebSocket/SignalR）
- 弹幕HTTP查询
- 弹幕发送管理
- 弹幕举报管理
- 弹幕权限控制（LV2+高级弹幕）

### 2.2 继承关系

- **继承ABP SignalR Hub**：WebSocket实时通信
- **继承现有Redis Scaleout**：跨服务器弹幕广播
- **继承现有RealtimeMessage模块**：SignalR架构

---

## 3. API接口列表

### 3.1 HTTP接口

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 获取弹幕列表 | GET | /api/danmaku/videos/{videoId}/danmakus | 公开 |
| 2 | 发送弹幕（HTTP） | POST | /api/danmaku/videos/{videoId}/danmakus | 需认证 |
| 3 | 获取用户弹幕 | GET | /api/danmaku/user/danmakus | 需认证 |
| 4 | 举报弹幕 | POST | /api/danmaku/danmakus/{id}/report | 需认证 |

### 3.2 WebSocket接口

| 序号 | 接口名称 | WebSocket方法 | Hub路径 | 权限 |
|------|---------|--------------|---------|------|
| 1 | 连接弹幕Hub | WebSocket | /hubs/danmaku | 需认证 |
| 2 | 加入视频房间 | Invoke | JoinVideoRoom | 需认证 |
| 3 | 发送弹幕 | Invoke | SendDanmaku | 需认证 |
| 4 | 退出视频房间 | Invoke | LeaveVideoRoom | 需认证 |

**总计：8个接口（4 HTTP + 4 WebSocket）**

---

## 4. HTTP接口定义

### 4.1 获取弹幕列表

#### 基本信息

```
GET /api/danmaku/videos/{videoId}/danmakus?startTime={decimal}&endTime={decimal}&limit={int}
权限：公开
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| startTime | decimal | ❌ | 开始时间（秒，默认0） |
| endTime | decimal | ❌ | 结束时间（秒，默认视频总时长） |
| limit | int | ❌ | 返回数量（默认500） |

#### 成功响应（200 OK）

```json
{
  "videoId": "...",
  "totalCount": 1000,
  "timeRange": {
    "startTime": 0,
    "endTime": 1200
  },
  "danmakus": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "videoId": "...",
      "userId": "00000000-0000-0000-0000-000000000001",
      "userName": "用户A",
      "userLevel": 3,
      "content": "哈哈哈",
      "positionTime": 10.5,
      "danmakuType": 0,
      "fontSize": 25,
      "fontColor": "#FFFFFF",
      "position": "Scroll",
      "isSendByVip": false,
      "sendTime": "2026-05-12T10:00:00Z"
    },
    {
      "id": "...",
      "positionTime": 12.3,
      "content": "太精彩了",
      "danmakuType": 0,
      "fontColor": "#FF0000"
    }
    // ... 弹幕列表
  ]
}
```

#### 弹幕类型枚举

| 类型值 | 说明 |
|--------|------|
| 0 | 滚动弹幕（Normal） |
| 1 | 顶部弹幕（Top） |
| 2 | 底部弹幕（Bottom） |
| 3 | 高级弹幕（Advanced，LV2+可用） |

---

### 4.2 发送弹幕（HTTP方式）

#### 基本信息

```
POST /api/danmaku/videos/{videoId}/danmakus
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| content | string | ✅ | 弹幕内容（最大500字符） |
| positionTime | decimal | ✅ | 出现时间（秒） |
| danmakuType | int | ❌ | 弹幕类型（默认0） |
| fontSize | int | ❌ | 字体大小（默认25） |
| fontColor | string | ❌ | 字体颜色（默认#FFFFFF） |
| position | string | ❌ | 弹幕位置（默认Scroll） |

#### 请求示例

```json
{
  "content": "精彩",
  "positionTime": 30.5,
  "danmakuType": 0,
  "fontSize": 25,
  "fontColor": "#FFFFFF",
  "position": "Scroll"
}
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "videoId": "...",
  "userId": "...",
  "content": "精彩",
  "positionTime": 30.5,
  "danmakuType": 0,
  "fontSize": 25,
  "fontColor": "#FFFFFF",
  "sendTime": "2026-05-12T10:00:00Z"
}
```

#### 错误响应（403 Forbidden）

```json
{
  "error": {
    "code": "Danmaku:AdvancedLevelRequired",
    "message": "高级弹幕需要LV2以上等级"
  }
}
```

---

### 4.3 获取用户发送的弹幕

#### 基本信息

```
GET /api/danmaku/user/danmakus?maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "videoId": "...",
      "videoTitle": "视频标题",
      "content": "弹幕内容",
      "positionTime": 30.5,
      "danmakuType": 0,
      "sendTime": "..."
    }
  ],
  "totalCount": 100
}
```

---

### 4.4 举报弹幕

#### 基本信息

```
POST /api/danmaku/danmakus/{id}/report
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| id | UUID | ✅ | 弹幕ID |

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| reason | string | ✅ | 举报原因（Spam/Abuse/Illegal/Other） |
| description | string | ❌ | 详细描述 |

#### 请求示例

```json
{
  "reason": "Spam",
  "description": "垃圾广告弹幕"
}
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "danmakuId": "...",
  "reporterId": "...",
  "reason": "Spam",
  "status": "Pending",
  "creationTime": "..."
}
```

---

## 5. WebSocket接口定义

### 5.1 WebSocket连接

#### 基本信息

```
WebSocket ws://{domain}/hubs/danmaku?videoId={videoId}&token={jwt_token}
```

#### 连接参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |
| token | string | ❌ | JWT Token（未登录也能观看弹幕） |

#### 连接示例（JavaScript）

```javascript
// SignalR客户端连接示例
import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/danmaku', {
    accessTokenFactory: () => localStorage.getItem('token')
  })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

// 启动连接
await connection.start();

// 加入视频房间
await connection.invoke('JoinVideoRoom', videoId);

// 监听弹幕消息
connection.on('NewDanmaku', (danmaku) => {
  console.log('收到弹幕:', danmaku);
  // 渲染弹幕到页面
});

// 监听最近弹幕
connection.on('RecentDanmakus', (danmakus) => {
  console.log('最近弹幕:', danmakus);
});

// 断开连接
connection.onclose((error) => {
  console.log('连接断开:', error);
});
```

---

### 5.2 加入视频房间（JoinVideoRoom）

#### 基本信息

```
方法：JoinVideoRoom
Hub：DanmakuHub
权限：需认证（可选）
```

#### 参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |

#### 调用示例

```javascript
await connection.invoke('JoinVideoRoom', 'video-id-guid');
```

#### 服务端响应

- 发送最近100条弹幕（`RecentDanmakus`事件）
- 加入SignalR Group（`video-{videoId}`）

---

### 5.3 发送弹幕（SendDanmaku）

#### 基本信息

```
方法：SendDanmaku
Hub：DanmakuHub
权限：需认证
```

#### 参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |
| content | string | ✅ | 弹幕内容 |
| positionTime | decimal | ✅ | 出现时间（秒） |
| danmakuType | int | ❌ | 弹幕类型 |
| fontSize | int | ❌ | 字体大小 |
| fontColor | string | ❌ | 字体颜色 |

#### 调用示例

```javascript
await connection.invoke('SendDanmaku', {
  videoId: 'video-id-guid',
  content: '精彩！',
  positionTime: 30.5,
  danmakuType: 0,
  fontSize: 25,
  fontColor: '#FFFFFF'
});
```

#### 服务端处理流程

1. **验证用户等级**：高级弹幕需LV2+
2. **创建弹幕实体**：保存到数据库
3. **广播弹幕**：发送`NewDanmaku`事件到所有观众

---

### 5.4 退出视频房间（LeaveVideoRoom）

#### 基本信息

```
方法：LeaveVideoRoom
Hub：DanmakuHub
权限：需认证
```

#### 参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |

#### 调用示例

```javascript
await connection.invoke('LeaveVideoRoom', 'video-id-guid');
```

---

## 6. WebSocket事件定义

### 6.1 服务端推送事件

#### NewDanmaku事件（新弹幕）

```json
{
  "type": "NewDanmaku",
  "data": {
    "id": "...",
    "userId": "...",
    "userName": "用户名",
    "userLevel": 3,
    "content": "弹幕内容",
    "positionTime": 30.5,
    "danmakuType": 0,
    "fontSize": 25,
    "fontColor": "#FFFFFF",
    "position": "Scroll",
    "sendTime": "..."
  }
}
```

#### RecentDanmakus事件（最近弹幕）

```json
{
  "type": "RecentDanmakus",
  "data": [
    { "id": "...", "content": "...", ... },
    { "id": "...", "content": "...", ... }
    // ... 最近100条弹幕
  ]
}
```

#### Error事件（错误）

```json
{
  "type": "Error",
  "data": {
    "code": "Danmaku:AdvancedLevelRequired",
    "message": "高级弹幕需要LV2以上等级"
  }
}
```

---

### 6.2 客户端发送事件

#### SendDanmaku事件

```json
{
  "type": "SendDanmaku",
  "data": {
    "content": "弹幕内容",
    "positionTime": 30.5,
    "danmakuType": 0,
    "fontSize": 25,
    "fontColor": "#FFFFFF"
  }
}
```

---

## 7. SignalR Hub代码示例

### 7.1 DanmakuHub实现（继承ABP Hub）

```csharp
// DanmakuHub.cs
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;

namespace LCH.MicroService.Danmaku.SignalR.Hubs
{
    public class DanmakuHub : AbpHub
    {
        private readonly IDanmakuAppService _danmakuService;
        
        public DanmakuHub(IDanmakuAppService danmakuService)
        {
            _danmakuService = danmakuService;
        }
        
        // 加入视频房间
        public async Task JoinVideoRoom(Guid videoId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"video-{videoId}"
            );
            
            // 发送最近弹幕
            var recentDanmakus = await _danmakuService.GetRecentAsync(videoId, 100);
            await Clients.Caller.SendAsync("RecentDanmakus", recentDanmakus);
        }
        
        // 发送弹幕
        public async Task SendDanmaku(SendDanmakuMessage message)
        {
            // 验证用户等级（高级弹幕需LV2+）
            if (message.DanmakuType == 3 && CurrentUser.Level < 2)
            {
                throw new UserFriendlyException("高级弹幕需要LV2以上等级");
            }
            
            // 创建弹幕
            var danmaku = await _danmakuService.CreateAsync(new CreateDanmakuDto
            {
                VideoId = message.VideoId,
                Content = message.Content,
                PositionTime = message.PositionTime,
                DanmakuType = message.DanmakuType,
                FontSize = message.FontSize,
                FontColor = message.FontColor
            });
            
            // 广播弹幕到所有观众（Redis Scaleout）
            await Clients.Group($"video-{message.VideoId}")
                .SendAsync("NewDanmaku", danmaku);
        }
        
        // 退出视频房间
        public async Task LeaveVideoRoom(Guid videoId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"video-{videoId}"
            );
        }
    }
}
```

---

## 8. 客户端完整示例

### 8.1 React弹幕组件示例

```typescript
// DanmakuPlayer.tsx
import React, { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

interface Danmaku {
  id: string;
  content: string;
  positionTime: number;
  fontSize: number;
  fontColor: string;
  position: string;
}

export const DanmakuPlayer: React.FC<{ videoId: string }> = ({ videoId }) => {
  const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
  const [danmakus, setDanmakus] = useState<Danmaku[]>([]);
  
  useEffect(() => {
    // 创建SignalR连接
    const newConnection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/danmaku', {
        accessTokenFactory: () => localStorage.getItem('token') || ''
      })
      .withAutomaticReconnect()
      .build();
    
    setConnection(newConnection);
    
    // 启动连接
    newConnection.start()
      .then(() => {
        console.log('SignalR连接成功');
        
        // 加入视频房间
        newConnection.invoke('JoinVideoRoom', videoId);
      })
      .catch(err => console.error('SignalR连接失败:', err));
    
    // 监听弹幕消息
    newConnection.on('NewDanmaku', (danmaku: Danmaku) => {
      setDanmakus(prev => [...prev, danmaku]);
      
      // 渲染弹幕到视频播放器
      renderDanmaku(danmaku);
    });
    
    // 监听最近弹幕
    newConnection.on('RecentDanmakus', (recentDanmakus: Danmaku[]) => {
      setDanmakus(recentDanmakus);
    });
    
    // 清理
    return () => {
      newConnection.stop();
    };
  }, [videoId]);
  
  // 发送弹幕
  const sendDanmaku = async (content: string, positionTime: number) => {
    if (connection) {
      await connection.invoke('SendDanmaku', {
        videoId,
        content,
        positionTime,
        danmakuType: 0,
        fontSize: 25,
        fontColor: '#FFFFFF'
      });
    }
  };
  
  // 渲染弹幕（实现滚动弹幕效果）
  const renderDanmaku = (danmaku: Danmaku) => {
    // 创建弹幕DOM元素
    const danmakuElement = document.createElement('div');
    danmakuElement.textContent = danmaku.content;
    danmakuElement.style.fontSize = `${danmaku.fontSize}px`;
    danmakuElement.style.color = danmaku.fontColor;
    danmakuElement.style.position = 'absolute';
    danmakuElement.style.whiteSpace = 'nowrap';
    
    // 添加到视频容器
    const videoContainer = document.getElementById('video-container');
    videoContainer?.appendChild(danmakuElement);
    
    // 计算弹幕滚动动画
    const containerWidth = videoContainer?.clientWidth || 0;
    danmakuElement.style.left = `${containerWidth}px`;
    danmakuElement.style.top = `${Math.random() * 200}px`;
    
    // 添加滚动动画
    danmakuElement.animate([
      { transform: 'translateX(0)' },
      { transform: `translateX(-${containerWidth + danmakuElement.offsetWidth}px)` }
    ], {
      duration: 5000, // 5秒滚动完
      easing: 'linear'
    }).onfinish = () => {
      danmakuElement.remove();
    };
  };
  
  return (
    <div>
      {/* 弹幕发送输入框 */}
      <input 
        type="text" 
        placeholder="发送弹幕..."
        onKeyDown={(e) => {
          if (e.key === 'Enter') {
            const videoCurrentTime = 30; // 当前视频播放时间
            sendDanmaku(e.currentTarget.value, videoCurrentTime);
            e.currentTarget.value = '';
          }
        }}
      />
    </div>
  );
};
```

---

## 9. 权限定义

| 权限名称 | 说明 |
|---------|------|
| Danmaku.View | 查看弹幕（公开） |
| Danmaku.Send | 发送弹幕（需认证） |
| Danmaku.Advanced | 发送高级弹幕（需LV2+） |
| Danmaku.Report | 举报弹幕（需认证） |

---

## 10. 错误码汇总

| 错误码 | HTTP状态码 | 说明 |
|--------|-----------|------|
| Danmaku:NotFound | 404 | 弹幕不存在 |
| Danmaku:AdvancedLevelRequired | 403 | 高级弹幕需LV2+ |
| Danmaku:ContentTooLong | 400 | 弹幕内容过长 |
| Danmaku:VideoNotFound | 404 | 视频不存在 |
| Danmaku:UserBanned | 403 | 用户已封禁，无法发送弹幕 |
| Danmaku:ConnectionFailed | 500 | WebSocket连接失败 |

---

## 11. 接口性能指标

| 接口 | 平均响应时间 | QPS上限 | 说明 |
|------|------------|---------|------|
| 获取弹幕列表 | <50ms | 2000 | 高频接口，需缓存 |
| 发送弹幕（HTTP） | <50ms | 1000 | 中频接口 |
| WebSocket连接 | <20ms | 无限制 | 实时连接 |
| WebSocket发送弹幕 | <10ms | 无限制 | 实时推送 |

---

## 12. 接口依赖关系

### 12.1 依赖其他服务

| 服务 | 依赖接口 | 用途 |
|------|---------|------|
| UserService | /api/bilibili/users/me/level | 获取用户等级 |
| VideoService | /api/video/videos/{id} | 验证视频是否存在 |
| Redis | SignalR Scaleout | 跨服务器弹幕广播 |

---

## 13. 接口版本历史

| 版本 | 更新时间 | 更新内容 |
|------|---------|---------|
| v1.0 | 2026-05-12 | 初始版本，8个接口（4 HTTP + 4 WebSocket） |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 弹幕模块API文档完成（含WebSocket接口）