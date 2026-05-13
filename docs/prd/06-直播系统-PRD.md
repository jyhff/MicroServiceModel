# 直播系统产品需求文档 (PRD)

## 1. 模块概述

### 1.2 核心功能
- 直播推流（RTMP/WebRTC）
- 直播播放（HLS/FLV/WebRTC）
- 直播弹幕
- 礼物打赏
- 直播间管理

---

## 2. 功能详细设计

### 2.1 直播推流

#### 2.1.1 推流地址
```
RTMP: rtmp://live.example.com/app/{streamKey}
WebRTC: wss://live.example.com/ws/publish/{streamKey}
```

#### 2.1.2 推流鉴权
```csharp
public class StreamKeyService
{
    public string GenerateStreamKey(Guid userId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = $"{userId}:{timestamp}";
        var signature = HMACSHA256(data, _secretKey);
        return $"{userId}-{timestamp}-{signature}";
    }
    
    public bool ValidateStreamKey(string streamKey)
    {
        var parts = streamKey.Split('-');
        if (parts.Length != 3) return false;
        
        var userId = parts[0];
        var timestamp = long.Parse(parts[1]);
        var signature = parts[2];
        
        // 检查是否过期（30分钟有效）
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp > 1800)
            return false;
        
        var expectedData = $"{userId}:{timestamp}";
        var expectedSignature = HMACSHA256(expectedData, _secretKey);
        
        return signature == expectedSignature;
    }
}
```

### 2.2 礼物系统

#### 2.2.1 礼物类型
| 礼物 | 价格（B币） | 特效 |
|------|-----------|------|
| 辣条 | 0.1 | 无 |
| 硬币 | 1 | 小动画 |
| 小花花 | 5 | 中动画 |
| 大火箭 | 100 | 全屏特效 |
| 超级战舰 | 1000 | 顶级特效 |

#### 2.2.2 礼物API
```http
POST /api/live/gift
Authorization: Bearer {token}
Content-Type: application/json

{
  "liveId": "uuid",
  "giftId": "gift_uuid",
  "count": 10,
  "message": "主播加油！"
}
```

### 2.3 直播间管理
- **禁言**: 禁止某用户发言
- **踢人**: 将用户移出直播间
- **设置房管**: 赋予管理权限
- **屏蔽词**: 自动过滤敏感词

---

## 3. 数据库设计

```sql
CREATE TABLE "LiveStreams" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Title" varchar(200) NOT NULL,
    "Description" text,
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
    "LiveId" uuid NOT NULL REFERENCES "LiveStreams"("Id"),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "GiftId" uuid NOT NULL,
    "Count" integer DEFAULT 1,
    "Message" varchar(100),
    "Amount" decimal(10,2),  -- 金额
    "CreationTime" timestamp with time zone NOT NULL
);
```

---

## 4. 技术架构

- **推流协议**: RTMP / WebRTC
- **播放协议**: HLS / HTTP-FLV / WebRTC
- **流媒体服务器**: SRS / Nginx-RTMP
- **实时通信**: SignalR
- **CDN**: 阿里云直播CDN

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0 + SRS + WebRTC
