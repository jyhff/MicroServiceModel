# 互动系统产品需求文档 (PRD)

## 1. 模块概述

### 1.2 核心功能
- 点赞（视频/评论）
- 投币（虚拟货币打赏）
- 收藏（添加到收藏夹）
- 评论（发表评论、回复）
- 关注（UP主关注）

---

## 2. 功能详细设计

### 2.1 点赞系统

#### 2.1.1 点赞API
```http
POST /api/interaction/like
Authorization: Bearer {token}
Content-Type: application/json

{
  "targetType": "video|comment",
  "targetId": "uuid",
  "isLike": true  // true=点赞, false=取消
}

Response:
{
  "success": true,
  "likeCount": 1001
}
```

#### 2.1.2 实现
```csharp
public class LikeService
{
    public async Task<LikeResult> ToggleLikeAsync(Guid userId, LikeInput input)
    {
        var existing = await _likeRepository
            .FirstOrDefaultAsync(l => l.UserId == userId && l.TargetId == input.TargetId);
        
        if (existing != null)
        {
            // 取消点赞
            await _likeRepository.DeleteAsync(existing);
            await _cache.DecrementAsync($"like:{input.TargetType}:{input.TargetId}");
            return new LikeResult { IsLiked = false };
        }
        else
        {
            // 新增点赞
            await _likeRepository.InsertAsync(new Like
            {
                UserId = userId,
                TargetId = input.TargetId,
                TargetType = input.TargetType
            });
            await _cache.IncrementAsync($"like:{input.TargetType}:{input.TargetId}");
            return new LikeResult { IsLiked = true };
        }
    }
}
```

### 2.2 投币系统

#### 2.2.1 投币规则
- 每日免费投币上限：根据等级
- 充值B币：1元 = 10 B币
- 投币收益：UP主获得70%

#### 2.2.2 投币API
```http
POST /api/interaction/coin
Authorization: Bearer {token}
Content-Type: application/json

{
  "videoId": "uuid",
  "count": 1  // 1或2
}
```

### 2.3 评论系统

#### 2.3.1 评论结构
```sql
CREATE TABLE "Comments" (
    "Id" uuid PRIMARY KEY,
    "VideoId" uuid NOT NULL REFERENCES "Videos"("Id"),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "ParentId" uuid REFERENCES "Comments"("Id"),  -- 回复评论
    "Content" varchar(1000) NOT NULL,
    "LikeCount" integer DEFAULT 0,
    "ReplyCount" integer DEFAULT 0,
    "IsTop" boolean DEFAULT false,  -- 是否置顶
    "IsDeleted" boolean DEFAULT false,
    "CreationTime" timestamp with time zone NOT NULL
);
```

#### 2.3.2 评论API
```http
POST /api/interaction/comment
Authorization: Bearer {token}
Content-Type: application/json

{
  "videoId": "uuid",
  "content": "评论内容",
  "parentId": null  // 回复某条评论
}

GET /api/interaction/comments?videoId={id}&page=1&sort=hot
```

---

## 3. 数据库设计

```sql
CREATE TABLE "Likes" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "TargetId" uuid NOT NULL,
    "TargetType" integer NOT NULL,  -- 1=视频,2=评论
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE UNIQUE INDEX "IX_Likes_UserId_TargetId_TargetType" 
    ON "Likes"("UserId", "TargetId", "TargetType");

CREATE TABLE "Coins" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "VideoId" uuid NOT NULL,
    "Count" integer NOT NULL,
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE TABLE "Collections" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL,
    "VideoId" uuid NOT NULL,
    "CollectionId" uuid,  -- 收藏夹ID
    "CreationTime" timestamp with time zone NOT NULL
);
```

---

## 4. 技术架构

- **缓存**: Redis（计数缓存）
- **消息队列**: 异步处理通知
- **防刷**: 限流中间件

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0 + Redis
