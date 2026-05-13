# 推荐系统产品需求文档 (PRD)

## 1. 模块概述

### 1.1 模块定位
推荐系统通过个性化算法为用户提供精准的视频内容推荐，提升用户停留时长和内容消费深度。系统采用多路召回+精排的多阶段推荐架构。

### 1.2 核心功能
- **个性化推荐**：基于用户兴趣和行为的个性化内容
- **热门推荐**：全站热门视频推荐
- **协同过滤**：基于用户相似度的推荐
- **内容推荐**：基于视频标签和内容的推荐
- **实时推荐**：基于实时行为的即时推荐
- **A/B测试**：多策略对比实验

---

## 2. 推荐策略

### 2.1 推荐场景

| 场景 | 召回策略 | 排序策略 | 位置 |
|------|---------|---------|------|
| 首页推荐 | 个性化、热门、协同 | 精排模型 | 首页信息流 |
| 相关推荐 | 内容相似、协同 | 精排模型 | 播放页侧边 |
| 搜索结果 | 文本匹配、热度 | 相关性+热度 | 搜索结果页 |
| 分区推荐 | 内容匹配、热度 | 热度+时间 | 分区页 |
| 新用户冷启动 | 热门、兴趣标签 | 热度 | 首页 |

### 2.2 召回层策略

#### 2.2.1 召回策略组合
```
┌────────────────────────────────────────────────────────────┐
│                      召回层 (Recall)                        │
├─────────────┬─────────────┬─────────────┬─────────────────┤
│  个性化召回  │  协同过滤   │  内容召回   │   热门召回      │
├─────────────┼─────────────┼─────────────┼─────────────────┤
│  用户画像   │  User-CF    │  标签匹配   │   全站热门      │
│  兴趣标签   │  Item-CF    │  向量相似   │   分区热门      │
│  历史行为   │  矩阵分解   │  文本相似   │   趋势上升      │
└─────────────┴─────────────┴─────────────┴─────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────┐
│                      融合排序层 (Rank)                      │
├────────────────────────────────────────────────────────────┤
│  LightGBM / 深度学习模型 / 多目标优化                       │
└────────────────────────────────────────────────────────────┘
```

#### 2.2.2 各召回策略实现

**1. 基于用户画像的召回**
```csharp
public class UserProfileRecaller : IRecaller
{
    public async Task<List<RecallResult>> RecallAsync(Guid userId, int count)
    {
        // 1. 获取用户画像
        var userProfile = await _userProfileService.GetAsync(userId);
        var userTags = userProfile.InterestTags.Take(10).ToList();
        
        // 2. 根据兴趣标签召回视频
        var videos = await _videoRepository.GetByTagsAsync(
            userTags, 
            excludeWatched: true,
            maxResults: count * 3);
        
        return videos.Select(v => new RecallResult
        {
            VideoId = v.Id,
            Score = CalculateTagScore(v.Tags, userTags),
            Source = "UserProfile",
            Reason = $"基于你对{string.Join(",", userTags.Take(3))}的兴趣"
        }).ToList();
    }
    
    private float CalculateTagScore(List<string> videoTags, List<string> userTags)
    {
        // 计算标签匹配度
        var intersection = videoTags.Intersect(userTags).Count();
        return (float)intersection / userTags.Count;
    }
}
```

**2. 协同过滤召回（User-CF）**
```csharp
public class UserCFRecaller : IRecaller
{
    public async Task<List<RecallResult>> RecallAsync(Guid userId, int count)
    {
        // 1. 找到相似用户
        var similarUsers = await _userSimilarityService.GetSimilarUsers(userId, topK: 20);
        
        // 2. 获取相似用户喜欢的视频
        var candidateVideos = new Dictionary<Guid, float>();
        
        foreach (var (similarUserId, similarity) in similarUsers)
        {
            var likedVideos = await _userBehaviorService.GetLikedVideos(similarUserId, days: 7);
            
            foreach (var videoId in likedVideos)
            {
                if (!candidateVideos.ContainsKey(videoId))
                    candidateVideos[videoId] = 0;
                
                candidateVideos[videoId] += similarity;
            }
        }
        
        // 3. 过滤已观看，按得分排序
        var watchedVideos = await _userBehaviorService.GetWatchedVideos(userId, days: 30);
        var filtered = candidateVideos
            .Where(x => !watchedVideos.Contains(x.Key))
            .OrderByDescending(x => x.Value)
            .Take(count);
        
        return filtered.Select(x => new RecallResult
        {
            VideoId = x.Key,
            Score = x.Value,
            Source = "UserCF",
            Reason = "和你兴趣相似的用户喜欢"
        }).ToList();
    }
}
```

**3. 协同过滤召回（Item-CF）**
```csharp
public class ItemCFRecaller : IRecaller
{
    public async Task<List<RecallResult>> RecallAsync(Guid userId, int count)
    {
        // 1. 获取用户最近观看/喜欢的视频
        var recentVideos = await _userBehaviorService.GetRecentVideos(userId, count: 10);
        
        // 2. 找到相似视频
        var candidateVideos = new Dictionary<Guid, float>();
        
        foreach (var videoId in recentVideos)
        {
            var similarVideos = await _videoSimilarityService.GetSimilarVideos(videoId, topK: 10);
            
            foreach (var (similarVideoId, similarity) in similarVideos)
            {
                if (!candidateVideos.ContainsKey(similarVideoId))
                    candidateVideos[similarVideoId] = 0;
                
                candidateVideos[similarVideoId] += similarity;
            }
        }
        
        // 3. 返回TopN
        return candidateVideos
            .OrderByDescending(x => x.Value)
            .Take(count)
            .Select(x => new RecallResult
            {
                VideoId = x.Key,
                Score = x.Value,
                Source = "ItemCF",
                Reason = "和你看过的视频相似"
            }).ToList();
    }
}
```

**4. 热门召回**
```csharp
public class HotRecaller : IRecaller
{
    public async Task<List<RecallResult>> RecallAsync(Guid userId, int count)
    {
        // 1. 获取用户所在分区的热门视频
        var userCategory = await _userBehaviorService.GetPreferredCategory(userId);
        
        // 2. 多维度热度计算
        var hotVideos = await _videoRepository.GetHotVideosAsync(
            categoryId: userCategory?.Id,
            timeRange: TimeRange.Last7Days,
            maxResults: count);
        
        return hotVideos.Select(v => new RecallResult
        {
            VideoId = v.Id,
            Score = CalculateHotScore(v),
            Source = "Hot",
            Reason = $"{userCategory?.Name ?? "全站"}热门"
        }).ToList();
    }
    
    // 热度计算公式
    private float CalculateHotScore(Video video)
    {
        // 威尔逊区间 + 时间衰减
        var likeScore = WilsonScore(video.LikeCount, video.ViewCount);
        var viewScore = Math.Log10(video.ViewCount + 1);
        var timeDecay = Math.Exp(-0.05 * (DateTime.UtcNow - video.PublishTime).TotalDays);
        
        return (likeScore * 0.3f + viewScore * 0.5f) * timeDecay;
    }
    
    // 威尔逊区间下界
    private float WilsonScore(int positive, int total)
    {
        if (total == 0) return 0;
        var p = (float)positive / total;
        var z = 1.96f; // 95%置信区间
        var n = total;
        return (p + z * z / (2 * n) - z * MathF.Sqrt((p * (1 - p) + z * z / (4 * n)) / n)) / (1 + z * z / n);
    }
}
```

### 2.3 精排层（Ranking）

#### 2.3.1 特征工程
```csharp
public class RankingFeatures
{
    // 用户特征
    public class UserFeatures
    {
        public Guid UserId { get; set; }
        public int Level { get; set; }
        public List<string> InterestTags { get; set; }
        public float AvgWatchDuration { get; set; }
        public int WatchCount7d { get; set; }
        public int LikeCount7d { get; set; }
        public List<string> PreferredCategories { get; set; }
        public DateTime LastActiveTime { get; set; }
    }
    
    // 视频特征
    public class VideoFeatures
    {
        public Guid VideoId { get; set; }
        public Guid UserId { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; }
        public int Duration { get; set; }
        public long ViewCount { get; set; }
        public long LikeCount { get; set; }
        public float LikeRate { get; set; }
        public float CompleteRate { get; set; }
        public DateTime PublishTime { get; set; }
        public bool IsVerifiedUp { get; set; }
    }
    
    // 交叉特征
    public class CrossFeatures
    {
        public float TagMatchScore { get; set; }  // 用户标签和视频标签匹配度
        public float CategoryMatchScore { get; set; }  // 分类匹配度
        public float UserAuthorInteract { get; set; }  // 用户与UP主互动历史
        public float RecencyScore { get; set; }  // 视频新鲜度
    }
}
```

#### 2.3.2 排序模型
```python
# LightGBM排序模型示例
import lightgbm as lgb
import pandas as pd

# 特征工程
def extract_features(user_id, video_id):
    user = get_user_features(user_id)
    video = get_video_features(video_id)
    
    features = {
        'user_level': user.level,
        'user_watch_count_7d': user.watch_count_7d,
        'user_like_count_7d': user.like_count_7d,
        'video_duration': video.duration,
        'video_view_count': video.view_count,
        'video_like_count': video.like_count,
        'video_like_rate': video.like_count / video.view_count if video.view_count > 0 else 0,
        'video_age_days': (datetime.now() - video.publish_time).days,
        'tag_match_score': calculate_tag_match(user.interest_tags, video.tags),
        'category_match': 1 if video.category in user.preferred_categories else 0,
        'user_author_interaction': get_user_author_interaction(user_id, video.user_id),
        'recency_score': math.exp(-0.05 * video_age_days)
    }
    
    return features

# 训练模型
def train_ranking_model():
    # 加载训练数据
    train_data = pd.read_csv('ranking_train_data.csv')
    
    # 定义特征和目标
    feature_cols = ['user_level', 'user_watch_count_7d', 'video_like_rate', 
                    'tag_match_score', 'category_match', 'recency_score']
    label_col = 'label'  # 0=未点击, 1=点击, 2=点赞, 3=收藏, 4=完播
    
    # 训练LightGBM
    train_dataset = lgb.Dataset(train_data[feature_cols], label=train_data[label_col])
    
    params = {
        'objective': 'lambdarank',
        'metric': 'ndcg',
        'ndcg_eval_at': [5, 10, 20],
        'learning_rate': 0.05,
        'num_leaves': 31,
        'feature_fraction': 0.8,
        'bagging_fraction': 0.8,
        'bagging_freq': 5,
        'verbose': -1
    }
    
    model = lgb.train(params, train_dataset, num_boost_round=100)
    model.save_model('ranking_model.txt')
    
    return model

# 预测
def predict_rank(user_id, candidate_videos):
    model = lgb.Booster(model_file='ranking_model.txt')
    
    features = [extract_features(user_id, vid) for vid in candidate_videos]
    features_df = pd.DataFrame(features)
    
    scores = model.predict(features_df)
    
    # 按得分排序
    results = sorted(zip(candidate_videos, scores), key=lambda x: x[1], reverse=True)
    return results
```

### 2.4 推荐Pipeline

```csharp
public class RecommendationPipeline
{
    private readonly List<IRecaller> _recallers;
    private readonly IRanker _ranker;
    
    public async Task<List<RecommendedVideo>> RecommendAsync(Guid userId, int count = 20)
    {
        // 1. 多路召回
        var recallTasks = _recallers.Select(r => r.RecallAsync(userId, count * 5));
        var recallResults = await Task.WhenAll(recallTasks);
        
        // 2. 合并去重
        var candidates = recallResults
            .SelectMany(r => r)
            .GroupBy(r => r.VideoId)
            .Select(g => new CandidateVideo
            {
                VideoId = g.Key,
                RecallScores = g.ToDictionary(x => x.Source, x => x.Score),
                Reasons = g.Select(x => x.Reason).Distinct().ToList()
            })
            .ToList();
        
        // 3. 精排
        var ranked = await _ranker.RankAsync(userId, candidates);
        
        // 4. 多样性控制（MMR算法）
        var diverseResults = ApplyMMR(ranked, lambda: 0.5);
        
        // 5. 返回结果
        return diverseResults.Take(count).ToList();
    }
    
    // MMR (Maximal Marginal Relevance) 多样性算法
    private List<RecommendedVideo> ApplyMMR(List<RankedVideo> videos, float lambda)
    {
        var selected = new List<RankedVideo>();
        var remaining = videos.ToList();
        
        while (remaining.Count > 0 && selected.Count < videos.Count)
        {
            RankedVideo best = null;
            float bestScore = float.MinValue;
            
            foreach (var video in remaining)
            {
                float score = lambda * video.Score;
                
                // 减去与已选视频的相似度
                foreach (var sel in selected)
                {
                    float sim = CalculateSimilarity(video, sel);
                    score -= (1 - lambda) * sim;
                }
                
                if (score > bestScore)
                {
                    bestScore = score;
                    best = video;
                }
            }
            
            selected.Add(best);
            remaining.Remove(best);
        }
        
        return selected;
    }
}
```

---

## 3. 用户画像

### 3.1 画像构建
```csharp
public class UserProfileService
{
    public async Task<UserProfile> BuildProfileAsync(Guid userId)
    {
        // 1. 获取用户行为数据
        var behaviors = await _userBehaviorRepository.GetRecentAsync(userId, days: 30);
        
        // 2. 统计标签兴趣
        var tagWeights = new Dictionary<string, float>();
        foreach (var behavior in behaviors)
        {
            foreach (var tag in behavior.VideoTags)
            {
                if (!tagWeights.ContainsKey(tag))
                    tagWeights[tag] = 0;
                
                // 根据行为类型加权
                var weight = behavior.BehaviorType switch
                {
                    BehaviorType.Watch => 1f,
                    BehaviorType.Like => 3f,
                    BehaviorType.Collect => 5f,
                    BehaviorType.Share => 4f,
                    BehaviorType.Comment => 2f,
                    _ => 1f
                };
                
                // 时间衰减
                var daysAgo = (DateTime.UtcNow - behavior.Timestamp).TotalDays;
                var timeDecay = (float)Math.Exp(-0.1 * daysAgo);
                
                tagWeights[tag] += weight * timeDecay;
            }
        }
        
        // 3. 获取Top兴趣标签
        var topTags = tagWeights
            .OrderByDescending(x => x.Value)
            .Take(20)
            .Select(x => x.Key)
            .ToList();
        
        // 4. 获取偏好分类
        var categoryWeights = behaviors
            .GroupBy(b => b.CategoryId)
            .Select(g => new { CategoryId = g.Key, Weight = g.Sum(b => b.WatchDuration) })
            .OrderByDescending(x => x.Weight)
            .Take(5)
            .Select(x => x.CategoryId)
            .ToList();
        
        return new UserProfile
        {
            UserId = userId,
            InterestTags = topTags,
            PreferredCategories = categoryWeights,
            LastUpdated = DateTime.UtcNow
        };
    }
}
```

### 3.2 画像存储
```sql
CREATE TABLE "UserProfiles" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "InterestTags" jsonb NOT NULL,             -- { "tag": weight, ... }
    "PreferredCategories" uuid[],            -- 偏好分类ID
    "WatchPatterns" jsonb,                     -- 观看时段分布
    "DevicePreference" varchar(50),           -- 偏好设备
    "AvgSessionDuration" integer,             -- 平均会话时长(秒)
    "AvgDailyWatch" integer,                  -- 平均日观看数
    "ActiveLevel" integer DEFAULT 0,          -- 活跃度等级
    "LastUpdated" timestamp with time zone NOT NULL,
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE UNIQUE INDEX "IX_UserProfiles_UserId" ON "UserProfiles"("UserId");
```

---

## 4. 推荐API

### 4.1 获取推荐列表
```http
GET /api/recommendation/feed?page=1&pageSize=20
Authorization: Bearer {token}

Response:
{
  "items": [
    {
      "id": "uuid",
      "title": "视频标题",
      "cover": "url",
      "duration": 300,
      "viewCount": 10000,
      "likeCount": 500,
      "user": {
        "id": "uuid",
        "username": "UP主",
        "avatar": "url",
        "isVerified": true
      },
      "recommendReason": "根据你的兴趣推荐",  // 推荐理由
      "recommendScore": 0.95,               // 推荐得分
      "isAd": false                         // 是否广告
    }
  ],
  "hasMore": true,
  "refreshToken": "token_for_next_page"
}
```

### 4.2 获取相关推荐
```http
GET /api/recommendation/related?videoId={videoId}&count=10

Response:
{
  "videoId": "uuid",
  "items": [
    {
      "id": "uuid",
      "title": "相似视频标题",
      "recommendReason": "和你看过的视频相似"
    }
  ]
}
```

### 4.3 记录反馈
```http
POST /api/recommendation/feedback
Authorization: Bearer {token}
Content-Type: application/json

{
  "videoId": "uuid",
  "action": "dislike",     // like, dislike, not_interested, click
  "reason": "重复推荐"      // 反馈原因
}
```

---

## 5. 冷启动策略

### 5.1 新用户冷启动
```csharp
public class ColdStartService
{
    public async Task<List<Video>> GetColdStartVideos(Guid userId)
    {
        // 1. 获取热门视频（按分类）
        var hotVideos = await _videoService.GetHotVideosAsync(topK: 100);
        
        // 2. 按分类分组
        var videosByCategory = hotVideos.GroupBy(v => v.CategoryId).ToList();
        
        // 3. 每个分类取TopN，保证多样性
        var diverseVideos = new List<Video>();
        foreach (var group in videosByCategory)
        {
            diverseVideos.AddRange(group.Take(3));
        }
        
        // 4. 随机打乱
        return diverseVideos.OrderBy(x => Guid.NewGuid()).ToList();
    }
    
    // 引导选择兴趣
    public async Task<List<Category>> GetInterestCategories()
    {
        return await _categoryService.GetPopularCategoriesAsync(topK: 12);
    }
}
```

### 5.2 新视频冷启动
```csharp
public class VideoColdStartService
{
    public async Task BoostNewVideo(Guid videoId)
    {
        // 1. 新视频流量扶持（前24小时）
        await _recommendationCache.SetAsync(
            $"boost:{videoId}", 
            1, 
            TimeSpan.FromHours(24));
        
        // 2. 推送给可能感兴趣的用户（种子用户）
        var targetUsers = await _userService.GetSeedUsersAsync(
            video.CategoryId, 
            count: 1000);
        
        // 3. 发送Push通知
        await _notificationService.SendBatchAsync(
            targetUsers, 
            new NewVideoNotification { VideoId = videoId });
    }
}
```

---

## 6. 技术架构

### 6.1 技术栈
- **框架**: ABP Framework v10.0.2 + .NET 10.0
- **特征存储**: Redis + Elasticsearch
- **召回服务**: Go/Java高性能服务
- **排序模型**: Python + LightGBM/TensorFlow
- **消息队列**: RabbitMQ（实时特征更新）
- **数据仓库**: ClickHouse（离线分析）

### 6.2 实时特征流
```
用户行为 → Kafka → Flink流处理 → Redis特征更新
                              ↓
                         Elasticsearch索引更新
```

### 6.3 离线计算
```
用户行为日志 → Hive/Spark → 用户画像 → PostgreSQL/Redis
                          ↓
                    协同过滤模型训练
                          ↓
                    模型发布到推荐服务
```

---

## 7. 评估指标

| 指标 | 说明 | 目标 |
|------|------|------|
| CTR | 点击率 | >15% |
| 完播率 | 完整观看比例 | >60% |
| 平均观看时长 | 用户停留时长 | >10分钟 |
| 多样性 | 推荐内容多样性 | >0.5 |
| 新鲜度 | 新内容占比 | >20% |
| NDCG@10 | 排序质量 | >0.8 |

---

## 8. A/B测试

### 8.1 实验框架
```csharp
public class ABTestService
{
    // 分配实验组
    public ExperimentGroup AssignGroup(string experimentId, Guid userId)
    {
        var hash = HashCode.Combine(experimentId.GetHashCode(), userId.GetHashCode());
        var bucket = Math.Abs(hash) % 100;
        
        // 50%对照组, 50%实验组
        if (bucket < 50)
            return ExperimentGroup.Control;
        else
            return ExperimentGroup.Treatment;
    }
}
```

### 8.2 常见实验
- **召回策略实验**：不同召回方式的对比
- **排序模型实验**：新模型vs旧模型
- **多样性实验**：不同MMR参数的效果
- **冷启动实验**：新用户引导策略

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0 + LightGBM + Redis + Elasticsearch
