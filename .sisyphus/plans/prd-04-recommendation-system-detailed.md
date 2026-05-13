# 04-推荐系统详细PRD

## 1. 模块概述

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 推荐算法 | 协同过滤+热门 |
| 索引更新 | 实时同步 |

---

## 2. 功能详细设计

### 2.1 推荐场景

#### 2.1.1 首页推荐API

```http
GET /api/v1/recommendation/feed?page=1&pageSize=20
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "items": [
      {
        "id": "uuid",
        "title": "推荐视频标题",
        "cover": "http://...",
        "duration": 300,
        "viewCount": 10000,
        "user": {
          "id": "uuid",
          "nickname": "UP主",
          "avatar": "..."
        },
        "recommendReason": "和你兴趣相似的用户喜欢",
        "recommendScore": 0.85
      }
    ],
    "hasMore": true,
    "nextPageToken": "base64_token"
  }
}
```

---

### 2.2 协同过滤实现

```csharp
public class CollaborativeFilteringService
{
    private readonly IUserBehaviorRepository _behaviorRepo;
    private readonly IVideoRepository _videoRepo;
    private readonly IDistributedCache _cache;
    
    public async Task<List<RecommendedVideo>> RecommendAsync(Guid userId, int count)
    {
        // 1. 获取用户最近行为
        var userBehaviors = await _behaviorRepo.GetRecentAsync(userId, days: 30);
        
        // 2. 找到相似用户
        var similarUsers = await FindSimilarUsers(userId, userBehaviors);
        
        // 3. 获取相似用户喜欢的视频
        var candidateVideos = await GetCandidateVideos(similarUsers, userId);
        
        // 4. 计算推荐得分
        var scoredVideos = await CalculateScores(candidateVideos, userId, similarUsers);
        
        // 5. 过滤已观看视频
        var filtered = await FilterWatchedVideos(scoredVideos, userId);
        
        // 6. 返回TopN
        return filtered.OrderByDescending(v => v.Score).Take(count).ToList();
    }
    
    private async Task<List<SimilarUser>> FindSimilarUsers(Guid userId, List<UserBehavior> behaviors)
    {
        // 获取用户看过的视频ID集合
        var userVideos = behaviors
            .Where(b => b.BehaviorType == BehaviorType.Watch)
            .Select(b => b.VideoId)
            .ToHashSet();
        
        // 找到看过相同视频的用户
        var otherUsers = await _behaviorRepo.GetUsersByVideosAsync(userVideos);
        
        // 计算相似度（Jaccard相似度）
        var similarities = new List<SimilarUser>();
        
        foreach (var otherUserId in otherUsers.Distinct())
        {
            if (otherUserId == userId) continue;
            
            var otherVideos = await _behaviorRepo.GetUserVideosAsync(otherUserId);
            var intersection = userVideos.Intersect(otherVideos).Count();
            var union = userVideos.Union(otherVideos).Count();
            var similarity = (double)intersection / union;
            
            if (similarity > 0.3)  // 相似度阈值
            {
                similarities.Add(new SimilarUser
                {
                    UserId = otherUserId,
                    Similarity = similarity
                });
            }
        }
        
        return similarities.OrderByDescending(s => s.Similarity).Take(50).ToList();
    }
    
    private async Task<List<RecommendedVideo>> GetCandidateVideos(
        List<SimilarUser> similarUsers,
        Guid excludeUserId)
    {
        var candidates = new Dictionary<Guid, double>();
        
        foreach (var similar in similarUsers)
        {
            var likedVideos = await _behaviorRepo.GetUserLikedVideosAsync(similar.UserId);
            
            foreach (var videoId in likedVideos)
            {
                if (!candidates.ContainsKey(videoId))
                    candidates[videoId] = 0;
                
                candidates[videoId] += similar.Similarity;
            }
        }
        
        return candidates.Select(c => new RecommendedVideo
        {
            VideoId = c.Key,
            Score = c.Value,
            Source = "CollaborativeFiltering"
        }).ToList();
    }
}
```

---

### 2.3 热门推荐实现

```csharp
public class HotRecommendationService
{
    public async Task<List<HotVideo>> GetHotVideosAsync(
        Guid? categoryId,
        TimeRange timeRange,
        int count)
    {
        var videos = await _videoRepo.GetRecentPublishedAsync(categoryId, timeRange);
        
        var scoredVideos = videos.Select(v => new
        {
            Video = v,
            Score = CalculateHotScore(v)
        }).OrderByDescending(v => v.Score).Take(count);
        
        return scoredVideos.Select(v => new HotVideo
        {
            VideoId = v.Video.Id,
            Title = v.Video.Title,
            Score = v.Score,
            Source = "Hot"
        }).ToList();
    }
    
    private double CalculateHotScore(Video video)
    {
        // 热度公式
        var viewScore = Math.Log10(video.ViewCount + 1);
        var likeScore = video.LikeCount / (double)(video.ViewCount + 1) * 100;
        var timeDecay = Math.Exp(-0.05 * (DateTime.UtcNow - video.PublishTime).TotalDays);
        
        return (viewScore * 0.5 + likeScore * 0.3) * timeDecay;
    }
}
```

---

## 3. 数据库设计

### 3.1 UserBehaviors表（用户行为）

```sql
CREATE TABLE "UserBehaviors" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "VideoId" uuid NOT NULL REFERENCES "Videos"("Id"),
    "BehaviorType" integer NOT NULL,        -- 1=观看,2=点赞,3=投币,4=收藏,5=分享
    "WatchDuration" integer,                -- 观看时长（秒）
    "WatchProgress" decimal(5, 2),          -- 观看进度（0-1）
    "IsComplete" boolean,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW()
) PARTITION BY RANGE ("CreationTime");

CREATE INDEX "IX_UserBehaviors_UserId" ON "UserBehaviors"("UserId");
CREATE INDEX "IX_UserBehaviors_VideoId" ON "UserBehaviors"("VideoId");
CREATE INDEX "IX_UserBehaviors_CreationTime" ON "UserBehaviors"("CreationTime" DESC);
```

---

**文档版本**: v2.0  
**状态**: ✅ 详细设计完成