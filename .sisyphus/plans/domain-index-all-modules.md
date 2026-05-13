# 频域设计文档总索引

## 1. 文档概述

本文档汇总了Bilibili视频平台各模块的领域设计文档，采用DDD（Domain-Driven Design）设计方法，包含聚合根、值对象、实体、领域服务、仓储接口和领域事件的完整设计。

---

## 2. 文档列表

| 序号 | 文档名称 | 模块名称 | 状态 |
|------|---------|---------|------|
| 01 | [domain-01-user-system.md](./domain-01-user-system.md) | 用户模块 | ✅ 完成 |
| 02 | [domain-02-video-system.md](./domain-02-video-system.md) | 视频模块 | ✅ 完成 |
| 03 | [domain-03-danmaku-system.md](./domain-03-danmaku-system.md) | 弹幕模块 | ✅ 完成 |
| 04 | [domain-04-recommend-system.md](./domain-04-recommend-system.md) | 推荐模块 | ✅ 完成 |
| 05 | [domain-05-search-system.md](./domain-05-search-system.md) | 搜索模块 | ✅ 完成 |
| 06 | [domain-06-live-system.md](./domain-06-live-system.md) | 直播模块 | ✅ 完成 |
| 07 | [domain-07-interaction-system.md](./domain-07-interaction-system.md) | 互动模块 | ✅ 完成 |
| 08 | [domain-08-category-system.md](./domain-08-category-system.md) | 分类管理模块 | ✅ 完成 |
| 09 | [domain-09-admin-system.md](./domain-09-admin-system.md) | 管理模块 | ✅ 完成 |

---

## 3. 各模块领域要素汇总

### 3.1 用户模块（domain-01）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | BilibiliUser | 用户聚合根 |
| 值对象 | UserLevel | 用户等级（LV0-LV6） |
| 值对象 | UserCoins | 用户B币余额 |
| 值对象 | UserProfile | 用户资料 |
| 值对象 | Verification | 认证信息 |
| 实体 | UserExperienceRecord | 经验记录 |
| 实体 | UserCoinRecord | B币记录 |
| 实体 | UserFollow | 关注记录 |
| 实体 | UserMessage | 用户消息 |
| 领域服务 | IUserLevelManager | 等级管理服务 |
| 领域服务 | IUserCoinManager | B币管理服务 |

---

### 3.2 视频模块（domain-02）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | Video | 视频聚合根 |
| 聚合根 | UploadSession | 分片上传聚合根 |
| 值对象 | VideoStatus | 视频状态 |
| 值对象 | VideoMetadata | 视频元数据 |
| 值对象 | VideoStorageInfo | 存储信息 |
| 值对象 | VideoStatistics | 统计数据 |
| 值对象 | VideoAuditStatus | 审核状态 |
| 值对象 | VideoResolution | 清晰度 |
| 实体 | VideoPart | 多分P视频 |
| 实体 | VideoTranscodingTask | 转码任务 |
| 实体 | VideoAuditRecord | 审核记录 |
| 实体 | UploadChunk | 上传分片 |
| 领域服务 | IVideoTranscodingService | 转码服务 |
| 领域服务 | IVideoAuditService | 审核服务 |
| 领域服务 | IVideoStorageService | 存储服务 |
| 领域服务 | IVideoStatisticsService | 统计服务 |

---

### 3.3 弹幕模块（domain-03）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | Danmaku | 弹幕聚合根 |
| 聚合根 | DanmakuRoom | 弹幕房间聚合根 |
| 值对象 | DanmakuContent | 弹幕内容 |
| 值对象 | DanmakuStyle | 弹幕样式 |
| 值对象 | DanmakuPosition | 弹幕位置 |
| 值对象 | DanmakuStatus | 弹幕状态 |
| 值对象 | DanmakuFilter | 弹幕过滤器 |
| 实体 | DanmakuSegment | 弹幕分段（分区表） |
| 实体 | UploadChunk | 上传分片 |
| 领域服务 | IDanmakuCollisionService | 碰撞检测服务 |
| 领域服务 | IDanmakuFilterService | 过滤服务 |
| 领域服务 | IDanmakuStatisticsService | 统计服务 |
| 领域服务 | IDanmakuRealtimeService | 实时推送服务 |

---

### 3.4 推荐模块（domain-04）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | UserBehavior | 用户行为聚合根 |
| 聚合根 | Recommendation | 推荐聚合根 |
| 聚合根 | UserInterest | 用户兴趣聚合根 |
| 值对象 | BehaviorType | 行为类型 |
| 值对象 | BehaviorWeight | 行为权重 |
| 值对象 | RecommendScore | 推荐得分 |
| 值对象 | RecommendReason | 推荐理由 |
| 实体 | SimilarUser | 相似用户 |
| 领域服务 | ICollaborativeFilteringService | 协同过滤服务 |
| 领域服务 | IHotRecommendationService | 热门推荐服务 |
| 领域服务 | IPersonalRecommendationService | 个性化推荐服务 |
| 领域服务 | IRecommendationEngineService | 推荐引擎服务 |

---

### 3.5 搜索模块（domain-05）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | SearchQuery | 搜索查询聚合根 |
| 聚合根 | SearchResult | 搜索结果聚合根 |
| 聚合根 | SearchIndex | 搜索索引聚合根 |
| 聚合根 | SearchHistory | 搜索历史聚合根 |
| 值对象 | SearchKeyword | 搜索关键词 |
| 值对象 | SearchFilter | 搜索过滤器 |
| 值对象 | SearchSort | 搜索排序 |
| 值对象 | SearchHighlight | 搜索高亮 |
| 值对象 | SearchScore | 搜索得分 |
| 领域服务 | ISearchService | 搜索服务 |
| 领域服务 | ISearchIndexService | 索引管理服务 |
| 题领域服务 | ISearchAnalyzerService | 搜索分析服务 |

---

### 3.6 直播模块（domain-06）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | LiveStream | 直播流聚合根 |
| 聚合根 | LiveRoom | 直播间聚合根 |
| 聚合根 | LiveGift | 礼物记录聚合根 |
| 值对象 | LiveStatus | 直播状态 |
| 值对象 | LiveStreamKey | 推流密钥 |
| 值对象 | LiveStatistics | 直播统计 |
| 值对象 | GiftAmount | 礼物金额 |
| 值对象 | GiftShare | 礼物分成 |
| 实体 | LiveViewer | 直播观众 |
| 题领域服务 | ILiveStreamService | 直播流服务 |
| 领域服务 | ILiveRoomService | 直播间服务 |
| 题领域服务 | ILiveGiftService | 礼物服务 |
| 题领域服务 | ILiveStatisticsService | 统计服务 |

---

### 3.7 互动模块（domain-07）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | Like | 点赞聚合根 |
| 聚合根 | Favorite | 收藏聚合根 |
| 聚合根 | Coin | 投币聚合根 |
| 聚合根 | Comment | 评论聚合根 |
| 聚合根 | Share | 分享聚合根 |
| 聚合根 | WatchHistory | 观看历史聚合根 |
| 聚合根 | FavoriteFolder | 收藏夹聚合根 |
| 值对象 | LikeTarget | 点赞目标 |
| 值对象 | LikeStatus | 点赞状态 |
| 值对象 | CoinAmount | 投币金额 |
| 值对象 | CoinTransfer | 投币转账 |
| 值对象 | CommentContent | 评论内容 |
| 值对象 | CommentStatus | 评论状态 |
| 题领域服务 | IInteractionService | 互动服务 |
| 题领域服务 | ICommentService | 评论服务 |
| 题领域服务 | IFavoriteFolderService | 收藏夹服务 |

---

### 3.8 分类管理模块（domain-08）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | Category | 分类聚合根 |
| 聚合根 | ContentAudit | 内容审核聚合根 |
| 值对象 | AuditStatus | 审核状态 |
| 值对象 | AuditResult | AI审核结果 |
| 题领域服务 | ICategoryService | 分类服务 |
| 题领域服务 | IContentAuditService | 内容审核服务 |

---

### 3.9 管理模块（domain-09）

| 类型 | 名称 | 说明 |
|------|------|------|
| 聚合根 | AdminUser | 管理员聚合根 |
| 聚合根 | BanRecord | 封禁记录聚合根 |
| 聚合根 | AuditRecord | 审核记录聚合根 |
| 聚合根 | SystemConfig | 系统配置聚合根 |
| 值对象 | AdminRole | 管理员角色 |
| 值对象 | DashboardStats | 仪表盘统计 |
| 题领域服务 | IAdminService | 管理服务 |
| 题领域服务 | IAdminUserService | 管理员用户服务 |

---

## 4. 核心业务规则汇总

| 模块 | 业务规则 | 用户需求来源 |
|------|---------|-------------|
| 用户 | 等级上限LV6 | 确认 |
| 用户 | 每日经验上限100 | 确认 |
| 用户 | 投币数量无限制 | 确认 |
| 视频 | 视频时长无限制 | 确认 |
| 视频 | 文件大小无限制 | 确认 |
| 视频 | 所有清晰度免费 | 确认 |
| 视频 | CPU转码 | 确认 |
| 视频 | 仅MinIO本地存储 | 确认 |
| 弹幕 | 弹幕并发无限制 | 确认 |
| 弹幕 | PostgreSQL分区表 | 确认 |
| 弹幕 | LV2+可使用高级弹幕 | 确认 |
| 推荐 | 协同过滤+热门 | 确认 |
| 推荐 | 实时同步Elasticsearch | 确认 |
| 搜索 | 实时同步索引 | 确认 |
| 直播 | HLS延迟<10秒 | 确认 |
| 直播 | 礼物100%给UP主 | 确认 |
| 直播 | 所有用户可开播 | 确认 |
| 互动 | 投币上限无限制 | 确认 |
| 互动 | 评论先审后发（人工审核） | 确认 |
| 分类 | 10个主分区 | 确认 |
| 分类 | AI自动审核通过 | 确认 |
| 管理 | 仅超级管理员 | 确认 |

---

## 5. 技术栈汇总

| 技术栈 | 选择 |
|--------|------|
| 框架 | .NET 10.0 + ABP Framework v10.0.2 |
| 数据库 | PostgreSQL（弹幕分区表） |
| 缓存 | Redis（实时数据缓冲） |
| 消息队列 | RabbitMQ（异步事件处理） |
| 搜索引擎 | Elasticsearch（全文搜索、推荐） |
| 存储 | MinIO（本地对象存储） |
| 转码 | FFmpeg（CPU转码） |
| WebSocket | 弹幕实时推送 |
| 直播流 | RTMP推流 + HLS播放 |

---

## 6. DDD设计规范

### 6.1 聚合根原则

- 每个聚合根有唯一ID
- 聚合根控制内部一致性
- 外部引用使用ID引用，不直接引用实体
- 聚合根发布领域事件

### 6.2 值对象原则

- 不可变（Immutable）
- 通过值相等性比较
- 无唯一标识

### 6.3 实体原则

- 有唯一标识
- 可变（Mutable）
- 属于聚合根生命周期

### 6.4 题领域服务原则

- 无状态
- 处理跨聚合根逻辑
- 提供业务能力接口

### 6.5 仓储原则

- 只处理聚合根
- 提供查询和持久化能力
- 不包含业务逻辑

---

## 7. 文档维护说明

- **创建时间**: 2026-05-12
- **文档数量**: 9份模块文档 + 1份索引文档
- **更新策略**: 需求变更时同步更新各模块文档
- **版本管理**: 所有文档使用v1.0版本号

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 领域设计文档总索引完成