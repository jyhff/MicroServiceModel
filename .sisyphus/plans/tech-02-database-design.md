# 数据库设计文档（结合现有ABP架构）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 数据库设计 |
| 目标项目 | Bilibili视频平台 |
| 数据库 | PostgreSQL（继承现有配置） |
| 创建时间 | 2026-05-12 |

---

## 2. 现有数据库架构分析

### 2.1 ABP框架内置表结构

基于ABP Framework v10.0.2，现有数据库已包含以下内置表（继承使用）：

#### Identity模块（用户认证）

| 表名 | 说明 | 使用场景 |
|------|------|---------|
| `AbpUsers` | 用户基础表 | ✅ 继承使用（用户账号） |
| `AbpRoles` | 角色表 | ✅ 继承使用（权限管理） |
| `AbpUserRoles` | 用户角色关联 | ✅ 继承使用 |
| `AbpPermissions` | 权限表 | ✅ 继承使用 |
| `AbpUserPermissions` | 用户权限 | ✅ 继承使用 |
| `AbpRolePermissions` | 角色权限 | ✅ 继承使用 |
| `AbpClaims` | Claims表 | ✅ 继承使用（实名认证等） |
| `AbpUserClaims` | 用户Claims | ✅ 继承使用 |
| `AbpLogins` | 外部登录 | ✅ 继承使用（微信登录） |
| `AbpUserLogins` | 用户登录关联 | ✅ 继承使用 |
| `AbpTokens` | Token表 | ✅ 继承使用（JWT） |

#### OpenIddict模块（认证授权）

| 表名 | 说明 | 使用场景 |
|------|------|---------|
| `OpenIddictApplications` | OAuth应用 | ✅ 继承使用（API Gateway认证） |
| `OpenIddictAuthorizations` | 授权记录 | ✅ 继承使用 |
| `OpenIddictScopes` | Scope定义 | ✅ 继承使用（video-api、danmaku-api等） |
| `OpenIddictTokens` | Token存储 | ✅ 继承使用 |

#### Auditing模块（审计日志）

| 表名 | 说明 | 使用场景 |
|------|------|---------|
| `AbpAuditLogs` | 审计日志表 | ✅ 继承使用（操作记录） |
| `AbpEntityChanges` | 实体变更记录 | ✅ 继承使用 |
| `AbpEntityPropertyChanges` | 属性变更详情 | ✅ 继承使用 |

#### Background Jobs模块

| 表名 | 说明 | 使用场景 |
|------|------|---------|
| `AbpBackgroundJobs` | 后台任务表 | ✅ 继承使用（转码任务队列） |

#### Feature模块（功能开关）

| 表名 | 说明 | 使用场景 |
|------|------|---------|
| `AbpFeatures` | 功能定义 | ✅ 继承使用 |
| `AbpFeatureValues` | 功能值配置 | ✅ 继承使用（会员功能开关） |

#### Setting模块（配置管理）

| 表名 | 说明 | 使用场景 |
|------|------|---------|
| `AbpSettings` | 全局配置 | ✅ 继承使用 |
| `AbpUserSettings` | 用户配置 | ✅ 继承使用（用户偏好） |

#### Tenant模块（多租户）

| 表名 | 说明 | 使用场景 |
|------|------|---------|
| `AbpTenants` | 租户表 | ⚠️ 评估使用（如果支持多租户） |
| `AbpTenantConnectionStrings` | 租户数据库连接 | ⚠️ 评估使用 |

---

## 3. Bilibili新增表结构设计

### 3.1 用户模块扩展（UserModule）

#### 用户扩展信息表

```sql
-- 创建表：用户扩展信息（继承AbpUsers）
CREATE TABLE "BilibiliUsers" (
    "Id" UUID NOT NULL PRIMARY KEY,                    -- 关联AbpUsers.Id
    "Level" INTEGER NOT NULL DEFAULT 0,                -- 用户等级 LV0-LV6
    "LevelProgress" INTEGER NOT NULL DEFAULT 0,        -- 等级进度（0-100）
    "TotalExperience" INTEGER NOT NULL DEFAULT 0,      -- 总经验值
    "DailyExperience" INTEGER NOT NULL DEFAULT 0,      -- 今日获得经验
    "LastExperienceDate" DATE,                         -- 上次获得经验日期
    "CoinsBalance" INTEGER NOT NULL DEFAULT 0,         -- B币余额
    "CoinsTotalEarned" INTEGER NOT NULL DEFAULT 0,     -- 累计获得B币
    "CoinsTotalSpent" INTEGER NOT NULL DEFAULT 0,      -- 累计消耗B币
    "AvatarUrl" VARCHAR(500),                          -- 头像URL（MinIO存储）
    "Signature" VARCHAR(200),                          -- 个人签名
    "Gender" INTEGER DEFAULT 0,                        -- 性别：0未知 1男 2女
    "Birthday" DATE,                                   -- 生日
    "Region" VARCHAR(50),                              -- 地区
    "IsVip" BOOLEAN NOT NULL DEFAULT FALSE,            -- 是否会员（预留）
    "VipExpireDate" DATE,                              -- 会员到期日期（预留）
    "IsRealNameVerified" BOOLEAN NOT NULL DEFAULT FALSE, -- 是否实名认证
    "RealNameVerifyDate" DATE,                         -- 实名认证日期
    "RealName" VARCHAR(100),                           -- 真实姓名（加密存储）
    "IdCardNumber" VARCHAR(100),                       -- 身份证号（加密存储）
    "IsBanned" BOOLEAN NOT NULL DEFAULT FALSE,         -- 是否封禁
    "BanExpireDate" DATE,                              -- 封禁到期日期
    "BanReason" VARCHAR(500),                          -- 封禁原因
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatorId" UUID,
    "LastModificationTime" TIMESTAMP,
    "LastModifierId" UUID,
    
    CONSTRAINT "FK_BilibiliUsers_AbpUsers" FOREIGN KEY ("Id") 
        REFERENCES "AbpUsers" ("Id") ON DELETE CASCADE
);

-- 创建索引
CREATE INDEX "IX_BilibiliUsers_Level" ON "BilibiliUsers" ("Level");
CREATE INDEX "IX_BilibiliUsers_CoinsBalance" ON "BilibiliUsers" ("CoinsBalance");
CREATE INDEX "IX_BilibiliUsers_IsRealNameVerified" ON "BilibiliUsers" ("IsRealNameVerified");
CREATE INDEX "IX_BilibiliUsers_IsBanned" ON "BilibiliUsers" ("IsBanned");

-- 注释
COMMENT ON TABLE "BilibiliUsers" IS 'Bilibili用户扩展信息表';
COMMENT ON COLUMN "BilibiliUsers"."Level" IS '用户等级：LV0(0-100经验) LV1(101-200) ... LV6(601+)';
COMMENT ON COLUMN "BilibiliUsers"."DailyExperience" IS '今日获得经验，每日上限100，每日凌晨重置';
COMMENT ON COLUMN "BilibiliUsers"."CoinsBalance" IS 'B币余额，投币消耗';
```

#### 用户经验记录表

```sql
-- 创建表：用户经验获取记录
CREATE TABLE "UserExperienceRecords" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "ExperienceType" VARCHAR(50) NOT NULL,              -- 经验类型
    "ExperienceAmount" INTEGER NOT NULL,                -- 获得经验值
    "SourceId" UUID,                                    -- 来源ID（视频ID/评论ID等）
    "SourceType" VARCHAR(50),                           -- 来源类型
    "Description" VARCHAR(200),                         -- 描述
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_UserExperienceRecords_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE
);

-- 创建索引
CREATE INDEX "IX_UserExperienceRecords_UserId_CreationTime" 
    ON "UserExperienceRecords" ("UserId", "CreationTime");
CREATE INDEX "IX_UserExperienceRecords_ExperienceType" 
    ON "UserExperienceRecords" ("ExperienceType");

-- 经验类型枚举说明
COMMENT ON COLUMN "UserExperienceRecords"."ExperienceType" IS 
    '经验类型：LOGIN(登录+5), WATCH_VIDEO(观看+1), PUBLISH_VIDEO(发布+10), 
     LIKE_VIDEO(点赞+1), COIN_VIDEO(投币+1), COMMENT_VIDEO(评论+2)';
```

#### 用户B币记录表

```sql
-- 创建表：用户B币交易记录
CREATE TABLE "UserCoinRecords" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "TransactionType" VARCHAR(50) NOT NULL,             -- 交易类型
    "CoinAmount" INTEGER NOT NULL,                      -- B币数量（正数获得，负数消耗）
    "BalanceAfter" INTEGER NOT NULL,                    -- 交易后余额
    "SourceId" UUID,                                    -- 来源ID
    "SourceType" VARCHAR(50),                           -- 来源类型
    "TargetUserId" UUID,                                -- 目标用户（投币给谁）
    "Description" VARCHAR(200),                         -- 描述
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_UserCoinRecords_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserCoinRecords_TargetUser" FOREIGN KEY ("TargetUserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE SET NULL
);

-- 创建索引
CREATE INDEX "IX_UserCoinRecords_UserId_CreationTime" 
    ON "UserCoinRecords" ("UserId", "CreationTime");
CREATE INDEX "IX_UserCoinRecords_TransactionType" 
    ON "UserCoinRecords" ("TransactionType");

-- 交易类型说明
COMMENT ON COLUMN "UserCoinRecords"."TransactionType" IS 
    '交易类型：ADMIN_GRANT(管理员发放), WATCH_REWARD(观看奖励), 
     COIN_VIDEO(投币消耗), RECEIVE_COIN(收到投币)';
```

---

### 3.2 视频模块（VideoModule）

#### 视频基础信息表

```sql
-- 创建表：视频基础信息
CREATE TABLE "Videos" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,                             -- UP主ID
    "Title" VARCHAR(200) NOT NULL,                      -- 视频标题
    "Description" VARCHAR(5000),                        -- 视频简介
    "CoverImageUrl" VARCHAR(500),                       -- 尘面URL（MinIO）
    "CategoryId" UUID NOT NULL,                         -- 所属分区ID
    "DurationSeconds" INTEGER NOT NULL DEFAULT 0,       -- 视频时长（秒）
    "OriginalFileUrl" VARCHAR(500),                     -- 原始文件URL（MinIO）
    "HlsMasterUrl" VARCHAR(500),                        -- HLS Master URL
    "Status" INTEGER NOT NULL DEFAULT 0,                -- 视频状态
    "PublishTime" TIMESTAMP,                            -- 发布时间
    "IsPublished" BOOLEAN NOT NULL DEFAULT FALSE,       -- 是否已发布
    "AuditStatus" INTEGER NOT NULL DEFAULT 0,           -- 审核状态
    "AuditTime" TIMESTAMP,                              -- 审核时间
    "AuditorId" UUID,                                   -- 审核人ID
    "AuditReason" VARCHAR(500),                         -- 审核意见
    "TotalViews" BIGINT NOT NULL DEFAULT 0,             -- 总播放量
    "TotalLikes" BIGINT NOT NULL DEFAULT 0,             -- 总点赞数
    "TotalCoins" BIGINT NOT NULL DEFAULT 0,             -- 总投币数
    "TotalComments" BIGINT NOT NULL DEFAULT 0,          -- 总评论数
    "TotalShares" BIGINT NOT NULL DEFAULT 0,            -- 总分享数
    "DanmakuCount" BIGINT NOT NULL DEFAULT 0,           -- 弹幕数量
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,         -- 是否删除
    "DeletionTime" TIMESTAMP,                           -- 删除时间
    "DeleterId" UUID,                                   -- 删除人
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    "CreatorId" UUID NOT NULL,
    "LastModificationTime" TIMESTAMP,
    "LastModifierId" UUID,
    
    CONSTRAINT "FK_Videos_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Videos_Categories" FOREIGN KEY ("CategoryId") 
        REFERENCES "Categories" ("Id")
);

-- 创建索引
CREATE INDEX "IX_Videos_UserId_Status" ON "Videos" ("UserId", "Status");
CREATE INDEX "IX_Videos_CategoryId_Status" ON "Videos" ("CategoryId", "Status");
CREATE INDEX "IX_Videos_Status_IsPublished" ON "Videos" ("Status", "IsPublished");
CREATE INDEX "IX_Videos_TotalViews_DESC" ON "Videos" ("TotalViews" DESC);
CREATE INDEX "IX_Videos_PublishTime_DESC" ON "Videos" ("PublishTime" DESC);
CREATE INDEX "IX_Videos_AuditStatus" ON "Videos" ("AuditStatus");

-- 视频状态枚举说明
COMMENT ON TABLE "Videos" IS '视频基础信息表';
COMMENT ON COLUMN "Videos"."Status" IS 
    '视频状态：0=DRAFT(草稿), 1=UPLOADING(上传中), 2=TRANSCODING(转码中), 
     3=AUDITING(审核中), 4=PUBLISHED(已发布), 5=FAILED(失败), 6=DELETED(已删除)';
COMMENT ON COLUMN "Videos"."AuditStatus" IS 
    '审核状态：0=PENDING(待审), 1=APPROVED(通过), 2=REJECTED(拒绝), 3=AUTO_APPROVED(AI自动通过)';
```

#### 视频转码任务表

```sql
-- 创建表：视频转码任务
CREATE TABLE "VideoTranscodeTasks" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" UUID NOT NULL,
    "Resolution" VARCHAR(20) NOT NULL,                  -- 清晰度
    "Width" INTEGER NOT NULL,                           -- 宽度
    "Height" INTEGER NOT NULL,                          -- 高度
    "Bitrate" INTEGER NOT NULL,                         -- 码率（kbps）
    "Status" INTEGER NOT NULL DEFAULT 0,                -- 任务状态
    "ProgressPercent" INTEGER NOT NULL DEFAULT 0,       -- 进度百分比
    "OutputFileUrl" VARCHAR(500),                       -- 输出文件URL
    "HlsPlaylistUrl" VARCHAR(500),                      -- HLS Playlist URL
    "StartTime" TIMESTAMP,                              -- 开始时间
    "EndTime" TIMESTAMP,                                -- 结束时间
    "ErrorMessage" VARCHAR(1000),                       -- 错误信息
    "WorkerId" VARCHAR(100),                            -- 执行Worker ID
    "RetryCount" INTEGER NOT NULL DEFAULT 0,            -- 重试次数
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_VideoTranscodeTasks_Videos" FOREIGN KEY ("VideoId") 
        REFERENCES "Videos" ("Id") ON DELETE CASCADE
);

-- 创建索引
CREATE INDEX "IX_VideoTranscodeTasks_VideoId" ON "VideoTranscodeTasks" ("VideoId");
CREATE INDEX "IX_VideoTranscodeTasks_Status" ON "VideoTranscodeTasks" ("Status");

-- 清晰度说明
COMMENT ON COLUMN "VideoTranscodeTasks"."Resolution" IS 
    '清晰度：360P(360p), 480P(480p), 720P(720p), 1080P(1080p)';
COMMENT ON COLUMN "VideoTranscodeTasks"."Status" IS 
    '任务状态：0=PENDING(待处理), 1=PROCESSING(处理中), 2=COMPLETED(完成), 
     3=FAILED(失败), 4=CANCELLED(取消)';
```

#### 视频标签关联表

```sql
-- 创建表：视频标签
CREATE TABLE "VideoTags" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(50) NOT NULL UNIQUE,                 -- 标签名称
    "UseCount" BIGINT NOT NULL DEFAULT 0,               -- 使用次数
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX "IX_VideoTags_UseCount_DESC" ON "VideoTags" ("UseCount" DESC);

-- 创建表：视频标签关联
CREATE TABLE "VideoTagRelations" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" UUID NOT NULL,
    "TagId" UUID NOT NULL,
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_VideoTagRelations_Videos" FOREIGN KEY ("VideoId") 
        REFERENCES "Videos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_VideoTagRelations_VideoTags" FOREIGN KEY ("TagId") 
        REFERENCES "VideoTags" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_VideoTagRelations_VideoId_TagId" UNIQUE ("VideoId", "TagId")
);

CREATE INDEX "IX_VideoTagRelations_VideoId" ON "VideoTagRelations" ("VideoId");
CREATE INDEX "IX_VideoTagRelations_TagId" ON "VideoTagRelations" ("TagId");
```

---

### 3.3 弹幕模块（DanmakuModule）

#### 弹幕表（分区表设计）

```sql
-- 创建表：弹幕分区表父表
CREATE TABLE "Danmakus" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "Content" VARCHAR(500) NOT NULL,                    -- 弹幕内容
    "PositionTime" DECIMAL(10,2) NOT NULL,              -- 出现时间（秒）
    "PositionX" DECIMAL(10,4),                          -- X坐标（高级弹幕）
    "PositionY" DECIMAL(10,4),                          -- Y坐标（高级弹幕）
    "DanmakuType" INTEGER NOT NULL DEFAULT 0,           -- 弹幕类型
    "FontSize" INTEGER NOT NULL DEFAULT 25,             -- 字体大小
    "FontColor" VARCHAR(20) NOT NULL DEFAULT '#FFFFFF', -- 字体颜色
    "IsSendByVip" BOOLEAN NOT NULL DEFAULT FALSE,       -- 是否会员发送
    "SendTime" TIMESTAMP NOT NULL DEFAULT NOW(),        -- 发送时间
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_Danmakus_Videos" FOREIGN KEY ("VideoId") 
        REFERENCES "Videos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Danmakus_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE
) PARTITION BY RANGE ("PositionTime");

-- 创建分区表（按时间段分区，每个分区10分钟）
CREATE TABLE "Danmakus_0_600" PARTITION OF "Danmakus"
    FOR VALUES FROM (0) TO (600);
CREATE TABLE "Danmakus_600_1200" PARTITION OF "Danmakus"
    FOR VALUES FROM (600) TO (1200);
CREATE TABLE "Danmakus_1200_1800" PARTITION OF "Danmakus"
    FOR VALUES FROM (1200) TO (1800);
CREATE TABLE "Danmakus_1800_2400" PARTITION OF "Danmakus"
    FOR VALUES FROM (1800) TO (2400);
CREATE TABLE "Danmakus_2400_3000" PARTITION OF "Danmakus"
    FOR VALUES FROM (2400) TO (3000);
CREATE TABLE "Danmakus_3000_MAX" PARTITION OF "Danmakus"
    FOR VALUES FROM (3000) TO (MAXVALUE);

-- 创建索引（在每个分区上创建）
CREATE INDEX "IX_Danmakus_0_600_VideoId_PositionTime" 
    ON "Danmakus_0_600" ("VideoId", "PositionTime");
CREATE INDEX "IX_Danmakus_600_1200_VideoId_PositionTime" 
    ON "Danmakus_600_1200" ("VideoId", "PositionTime");
CREATE INDEX "IX_Danmakus_1200_1800_VideoId_PositionTime" 
    ON "Danmakus_1200_1800" ("VideoId", "PositionTime");

-- 弹幕类型说明
COMMENT ON TABLE "Danmakus" IS '弹幕分区表（按时间分段分区，优化查询性能）';
COMMENT ON COLUMN "Danmakus"."DanmakuType" IS 
    '弹幕类型：0=ROLL(滚动弹幕), 1=TOP(顶部弹幕), 2=BOTTOM(底部弹幕), 
     3=ADVANCED(高级弹幕，LV2+可用)';
COMMENT ON COLUMN "Danmakus"."PositionTime" IS '弹幕出现时间（视频播放秒数）';
```

---

### 3.4 互动模块（InteractionModule）

#### 点赞记录表

```sql
-- 创建表：用户点赞记录
CREATE TABLE "UserLikes" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "VideoId" UUID NOT NULL,
    "IsLiked" BOOLEAN NOT NULL DEFAULT TRUE,            -- 是否点赞
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    "LastModificationTime" TIMESTAMP,
    
    CONSTRAINT "FK_UserLikes_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserLikes_Videos" FOREIGN KEY ("VideoId") 
        REFERENCES "Videos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "UQ_UserLikes_UserId_VideoId" UNIQUE ("UserId", "VideoId")
);

-- 创建索引
CREATE INDEX "IX_UserLikes_VideoId" ON "UserLikes" ("VideoId");
CREATE INDEX "IX_UserLikes_UserId" ON "UserLikes" ("UserId");
```

#### 投币记录表

```sql
-- 创建表：用户投币记录
CREATE TABLE "UserCoins" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,                             -- 投币用户
    "VideoId" UUID NOT NULL,
    "CoinAmount" INTEGER NOT NULL,                      -- 投币数量（无限制）
    "ReceiverUserId" UUID NOT NULL,                     -- 收币用户（UP主）
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_UserCoins_BilibiliUsers_Sender" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserCoins_Videos" FOREIGN KEY ("VideoId") 
        REFERENCES "Videos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserCoins_BilibiliUsers_Receiver" FOREIGN KEY ("ReceiverUserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE
);

-- 创建索引
CREATE INDEX "IX_UserCoins_VideoId" ON "UserCoins" ("VideoId");
CREATE INDEX "IX_UserCoins_UserId" ON "UserCoins" ("UserId");
CREATE INDEX "IX_UserCoins_ReceiverUserId" ON "UserCoins" ("ReceiverUserId");
```

#### 评论表

```sql
-- 创建表：视频评论
CREATE TABLE "Comments" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "ParentCommentId" UUID,                             -- 父评论ID（楼中楼）
    "Content" VARCHAR(1000) NOT NULL,                   -- 评论内容
    "AuditStatus" INTEGER NOT NULL DEFAULT 0,           -- 审核状态（人工审核）
    "AuditTime" TIMESTAMP,                              -- 审核时间
    "AuditorId" UUID,                                   -- 审核人ID
    "AuditReason" VARCHAR(500),                         -- 审核意见
    "IsPublished" BOOLEAN NOT NULL DEFAULT FALSE,       -- 是否发布
    "PublishTime" TIMESTAMP,                            -- 发布时间
    "LikeCount" INTEGER NOT NULL DEFAULT 0,             -- 点赞数
    "ReplyCount" INTEGER NOT NULL DEFAULT 0,            -- 回复数
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,         -- 是否删除
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_Comments_Videos" FOREIGN KEY ("VideoId") 
        REFERENCES "Videos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Comments_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Comments_ParentComment" FOREIGN KEY ("ParentCommentId") 
        REFERENCES "Comments" ("Id") ON DELETE CASCADE
);

-- 创建索引
CREATE INDEX "IX_Comments_VideoId_AuditStatus" ON "Comments" ("VideoId", "AuditStatus");
CREATE INDEX "IX_Comments_VideoId_PublishTime_DESC" ON "Comments" ("VideoId", "PublishTime" DESC);
CREATE INDEX "IX_Comments_UserId" ON "Comments" ("UserId");
CREATE INDEX "IX_Comments_ParentCommentId" ON "Comments" ("ParentCommentId");
CREATE INDEX "IX_Comments_AuditStatus" ON "Comments" ("AuditStatus");

-- 审核状态说明
COMMENT ON COLUMN "Comments"."AuditStatus" IS 
    '审核状态：0=PENDING(待审), 1=APPROVED(通过), 2=REJECTED(拒绝)';
```

#### 分享记录表

```sql
-- 创建表：分享记录
CREATE TABLE "VideoShares" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "SharePlatform" VARCHAR(50) NOT NULL,               -- 分享平台
    "ShareTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_VideoShares_Videos" FOREIGN KEY ("VideoId") 
        REFERENCES "Videos" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_VideoShares_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE
);

-- 创建索引
CREATE INDEX "IX_VideoShares_VideoId" ON "VideoShares" ("VideoId");
CREATE INDEX "IX_VideoShares_UserId" ON "VideoShares" ("UserId");

COMMENT ON COLUMN "VideoShares"."SharePlatform" IS 
    '分享平台：WECHAT(微信), WEIBO(微博), QQ, TWITTER, FACEBOOK, COPY_LINK';
```

---

### 3.5 直播模块（LiveModule）

#### 直播房间表

```sql
-- 创建表：直播房间
CREATE TABLE "LiveRooms" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,                             -- 主播ID
    "Title" VARCHAR(200) NOT NULL,                      -- 直播标题
    "CoverImageUrl" VARCHAR(500),                       -- 尘面URL
    "CategoryId" UUID NOT NULL,                         -- 直播分区
    "Description" VARCHAR(500),                         -- 直播简介
    "Status" INTEGER NOT NULL DEFAULT 0,                -- 房间状态
    "StreamUrl" VARCHAR(500),                           -- 推流地址
    "PlayUrl" VARCHAR(500),                             -- 播放地址（HLS）
    "ViewerCount" INTEGER NOT NULL DEFAULT 0,           -- 当前观众数
    "TotalViewerCount" BIGINT NOT NULL DEFAULT 0,       -- 累计观众数
    "StartTime" TIMESTAMP,                              -- 开播时间
    "EndTime" TIMESTAMP,                                -- 关播时间
    "DurationSeconds" INTEGER NOT NULL DEFAULT 0,       -- 直播时长
    "IsBanned" BOOLEAN NOT NULL DEFAULT FALSE,          -- 是否封禁
    "BanReason" VARCHAR(500),                           -- 封禁原因
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_LiveRooms_BilibiliUsers" FOREIGN KEY ("UserId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LiveRooms_Categories" FOREIGN KEY ("CategoryId") 
        REFERENCES "Categories" ("Id")
);

-- 创建索引
CREATE INDEX "IX_LiveRooms_UserId" ON "LiveRooms" ("UserId");
CREATE INDEX "IX_LiveRooms_Status" ON "LiveRooms" ("Status");
CREATE INDEX "IX_LiveRooms_CategoryId" ON "LiveRooms" ("CategoryId");
CREATE INDEX "IX_LiveRooms_ViewerCount_DESC" ON "LiveRooms" ("ViewerCount" DESC);

COMMENT ON COLUMN "LiveRooms"."Status" IS 
    '房间状态：0=OFFLINE(未开播), 1=LIVING(直播中), 2=BANNED(封禁)';
```

#### 直播礼物表

```sql
-- 创建表：直播礼物定义
CREATE TABLE "LiveGifts" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(50) NOT NULL,                        -- 礼物名称
    "ImageUrl" VARCHAR(500) NOT NULL,                   -- 礼物图片URL
    "Price" DECIMAL(10,2) NOT NULL,                     -- 礼物价格（元）
    "AnimationUrl" VARCHAR(500),                        -- 动画特效URL
    "Description" VARCHAR(200),                         -- 礼物描述
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX "IX_LiveGifts_Price" ON "LiveGifts" ("Price");

-- 创建表：直播礼物赠送记录
CREATE TABLE "LiveGiftRecords" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "LiveRoomId" UUID NOT NULL,
    "SenderId" UUID NOT NULL,                           -- 发送者
    "ReceiverId" UUID NOT NULL,                         -- 接收者（主播）
    "GiftId" UUID NOT NULL,
    "GiftCount" INTEGER NOT NULL DEFAULT 1,             -- 礼物数量
    "TotalPrice" DECIMAL(10,2) NOT NULL,                -- 总价格
    "SendTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_LiveGiftRecords_LiveRooms" FOREIGN KEY ("LiveRoomId") 
        REFERENCES "LiveRooms" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LiveGiftRecords_Sender" FOREIGN KEY ("SenderId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LiveGiftRecords_Receiver" FOREIGN KEY ("ReceiverId") 
        REFERENCES "BilibiliUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_LiveGiftRecords_LiveGifts" FOREIGN KEY ("GiftId") 
        REFERENCES "LiveGifts" ("Id")
);

-- 创建索引
CREATE INDEX "IX_LiveGiftRecords_LiveRoomId" ON "LiveGiftRecords" ("LiveRoomId");
CREATE INDEX "IX_LiveGiftRecords_ReceiverId" ON "LiveGiftRecords" ("ReceiverId");
CREATE INDEX "IX_LiveGiftRecords_SendTime_DESC" ON "LiveGiftRecords" ("SendTime" DESC);

COMMENT ON COLUMN "LiveGiftRecords"."TotalPrice" IS '总价格 = Gift.Price * GiftCount，100%给UP主';
```

---

### 3.6 分区管理模块（CategoryModule）

#### 视频分区表

```sql
-- 创建表：视频分区
CREATE TABLE "Categories" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "Name" VARCHAR(50) NOT NULL,                        -- 分区名称
    "Code" VARCHAR(20) NOT NULL UNIQUE,                 -- 分区代码
    "IconUrl" VARCHAR(500),                             -- 分区图标URL
    "Description" VARCHAR(200),                         -- 分区描述
    "ParentId" UUID,                                    -- 父分区ID
    "Level" INTEGER NOT NULL DEFAULT 1,                 -- 分区层级
    "SortOrder" INTEGER NOT NULL DEFAULT 0,             -- 排序
    "VideoCount" BIGINT NOT NULL DEFAULT 0,             -- 视频数量
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,           -- 是否启用
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW(),
    
    CONSTRAINT "FK_Categories_Parent" FOREIGN KEY ("ParentId") 
        REFERENCES "Categories" ("Id") ON DELETE SET NULL
);

-- 创建索引
CREATE INDEX "IX_Categories_ParentId" ON "Categories" ("ParentId");
CREATE INDEX "IX_Categories_Code" ON "Categories" ("Code");
CREATE INDEX "IX_Categories_SortOrder" ON "Categories" ("SortOrder");

-- 插入10个主分区数据
INSERT INTO "Categories" ("Id", "Name", "Code", "SortOrder", "Level") VALUES
    (gen_random_uuid(), '动画', 'animation', 1, 1),
    (gen_random_uuid(), '游戏', 'game', 2, 1),
    (gen_random_uuid(), '音乐', 'music', 3, 1),
    (gen_random_uuid(), '舞蹈', 'dance', 4, 1),
    (gen_random_uuid(), '知识', 'knowledge', 5, 1),
    (gen_random_uuid(), '科技', 'tech', 6, 1),
    (gen_random_uuid(), '生活', 'life', 7, 1),
    (gen_random_uuid(), '美食', 'food', 8, 1),
    (gen_random_uuid(), '时尚', 'fashion', 9, 1),
    (gen_random_uuid(), '娱乐', 'entertainment', 10, 1);

COMMENT ON TABLE "Categories" IS '视频分区表，10个主分区';
```

---

### 3.7 搜索索引同步表

```sql
-- 创建表：Elasticsearch同步队列
CREATE TABLE "SearchSyncQueue" (
    "Id" UUID NOT NULL PRIMARY KEY DEFAULT gen_random_uuid(),
    "EntityId" UUID NOT NULL,                           -- 实体ID
    "EntityType" VARCHAR(50) NOT NULL,                  -- 实体类型
    "SyncStatus" INTEGER NOT NULL DEFAULT 0,            -- 同步状态
    "SyncTime" TIMESTAMP,                               -- 同步时间
    "ErrorMessage" VARCHAR(500),                        -- 错误信息
    "CreationTime" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- 创建索引
CREATE INDEX "IX_SearchSyncQueue_SyncStatus" ON "SearchSyncQueue" ("SyncStatus");
CREATE INDEX "IX_SearchSyncQueue_EntityType" ON "SearchSyncQueue" ("EntityType");

COMMENT ON COLUMN "SearchSyncQueue"."EntityType" IS '实体类型：VIDEO, USER, COMMENT';
COMMENT ON COLUMN "SearchSyncQueue"."SyncStatus" IS 
    '同步状态：0=PENDING(待同步), 1=SYNCED(已同步), 2=FAILED(失败)';
```

---

### 3.8 管理后台表（AdminModule）

#### 系统配置表（继承AbpSettings）

```sql
-- 扩展系统配置（使用AbpSettings表）
-- 示例配置项
INSERT INTO "AbpSettings" ("Name", "Value", "ProviderName", "ProviderKey") VALUES
    ('Bilibili.Video.MaxDurationSeconds', '0', 'G', NULL),                  -- 视频时长限制（0=无限制）
    ('Bilibili.Video.MaxFileSizeMB', '0', 'G', NULL),                       -- 文件大小限制（0=无限制）
    ('Bilibili.Video.DefaultResolutions', '360p,480p,720p,1080p', 'G', NULL), -- 默认转码清晰度
    ('Bilibili.User.MaxDailyExperience', '100', 'G', NULL),                  -- 每日经验上限
    ('Bilibili.Comment.RequireAudit', 'true', 'G', NULL),                    -- 评论需审核
    ('Bilibili.Danmaku.AdvancedMinLevel', '2', 'G', NULL),                   -- 高级弹幕最低等级
    ('Bilibili.Live.GiftRevenueShare', '1.0', 'G', NULL);                    -- 礼物分成比例（100%）
```

---

## 4. 数据库关系图（ER图）

```
┌──────────────────────────────────────────────────────────────────────────┐
│                          Bilibili数据库关系图                             │
└──────────────────────────────────────────────────────────────────────────┘

AbpUsers (ABP内置)
    │
    │ 1:1
    ↓
BilibiliUsers (用户扩展)
    │
    │ 1:N
    ├──────────────────┬──────────────────┬──────────────────┬─────────────┐
    ↓                  ↓                  ↓                  ↓             ↓
Videos              Danmakus           UserLikes          Comments     LiveRooms
    │                  │                  │                  │             │
    │ N:1              │ N:1              │ N:1              │ N:1         │ N:1
    ↓                  ↓                  ↓                  ↓             ↓
Categories          Videos             Videos             Videos       Categories
    │
    │ N:N
    ↓
VideoTags ← VideoTagRelations

Videos
    │ 1:N
    ├──────────────────┬──────────────────┬──────────────────┐
    ↓                  ↓                  ↓                  ↓
VideoTranscodeTasks  UserCoins          VideoShares        SearchSyncQueue

LiveRooms
    │ 1:N
    ↓
LiveGiftRecords → LiveGifts
```

---

## 5. ABP集成代码示例

### 5.1 创建实体类（继承ABP Entity）

```csharp
// Video实体（继承ABP Entity）
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Entities.Auditing;

namespace LCH.MicroService.Video.Domain.Entities
{
    /// <summary>
    /// 视频实体（继承ABP审计实体）
    /// </summary>
    public class Video : FullAuditedAggregateRoot<Guid>
    {
        // 基础信息
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CoverImageUrl { get; set; }
        public Guid CategoryId { get; set; }
        
        // 视频信息
        public int DurationSeconds { get; set; }
        public string OriginalFileUrl { get; set; }
        public string HlsMasterUrl { get; set; }
        
        // 状态
        public VideoStatus Status { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishTime { get; set; }
        
        // 审核
        public AuditStatus AuditStatus { get; set; }
        public DateTime? AuditTime { get; set; }
        public Guid? AuditorId { get; set; }
        public string AuditReason { get; set; }
        
        // 统计
        public long TotalViews { get; set; }
        public long TotalLikes { get; set; }
        public long TotalCoins { get; set; }
        public long TotalComments { get; set; }
        public long DanmakuCount { get; set; }
        
        // 软删除
        public bool IsDeleted { get; set; }
        public DateTime? DeletionTime { get; set; }
        public Guid? DeleterId { get; set; }
        
        // 导航属性
        public ICollection<VideoTranscodeTask> TranscodeTasks { get; set; }
        public ICollection<Danmaku> Danmakus { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }
    
    // 枚举定义
    public enum VideoStatus
    {
        Draft = 0,
        Uploading = 1,
        Transcoding = 2,
        Auditing = 3,
        Published = 4,
        Failed = 5,
        Deleted = 6
    }
    
    public enum AuditStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        AutoApproved = 3
    }
}
```

### 5.2 创建DbContext（继承ABP DbContext）

```csharp
// VideoDbContext（继承ABP DbContext）
using Volo.Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LCH.MicroService.Video.Domain.Entities;

namespace LCH.MicroService.Video.EntityFrameworkCore
{
    public class VideoDbContext : AbpDbContext<VideoDbContext>
    {
        public DbSet<Video> Videos { get; set; }
        public DbSet<VideoTranscodeTask> TranscodeTasks { get; set; }
        public DbSet<VideoTag> Tags { get; set; }
        public DbSet<VideoTagRelation> TagRelations { get; set; }
        public DbSet<Category> Categories { get; set; }
        
        public VideoDbContext(DbContextOptions<VideoDbContext> options)
            : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // 配置Video实体
            modelBuilder.Entity<Video>(b =>
            {
                b.ToTable("Videos");
                b.HasKey(x => x.Id);
                
                // 索引
                b.HasIndex(x => new { x.UserId, x.Status });
                b.HasIndex(x => new { x.CategoryId, x.Status });
                b.HasIndex(x => x.TotalViews).IsDescending();
                
                // 关系
                b.HasMany(x => x.TranscodeTasks)
                  .WithOne()
                  .HasForeignKey(x => x.VideoId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
            
            // 配置分区表
            modelBuilder.Entity<Category>(b =>
            {
                b.ToTable("Categories");
                b.HasKey(x => x.Id);
                
                b.HasIndex(x => x.Code).IsUnique();
                b.HasIndex(x => x.SortOrder);
                
                // 自关联（父子分区）
                b.HasOne(x => x.Parent)
                  .WithMany()
                  .HasForeignKey(x => x.ParentId)
                  .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
```

### 5.3 创建Repository（继承ABP Repository）

```csharp
// VideoRepository（继承ABP Repository）
using Volo.Abp.Domain.Repositories;
using LCH.MicroService.Video.Domain.Entities;

namespace LCH.MicroService.Video.Domain.Repositories
{
    /// <summary>
    /// 视频Repository接口
    /// </summary>
    public interface IVideoRepository : IRepository<Video, Guid>
    {
        /// <summary>
        /// 获取用户视频列表
        /// </summary>
        Task<List<Video>> GetByUserIdAsync(
            Guid userId, 
            VideoStatus? status = null,
            int maxResultCount = 10,
            int skipCount = 0);
        
        /// <summary>
        /// 获取分区视频列表（按播放量排序）
        /// </summary>
        Task<List<Video>> GetByCategoryIdAsync(
            Guid categoryId,
            int maxResultCount = 10,
            int skipCount = 0);
        
        /// <summary>
        /// 获取热门视频
        /// </summary>
        Task<List<Video>> GetHotVideosAsync(
            int maxResultCount = 10);
        
        /// <summary>
        /// 增加播放量（原子操作）
        /// </summary>
        Task IncrementViewCountAsync(Guid videoId);
    }
}

// VideoRepository实现
using Volo.Abp.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LCH.MicroService.Video.EntityFrameworkCore.Repositories
{
    public class VideoRepository : EfCoreRepository<VideoDbContext, Video, Guid>, 
        IVideoRepository
    {
        public VideoRepository(IDbContextProvider<VideoDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }
        
        public async Task<List<Video>> GetByUserIdAsync(
            Guid userId, 
            VideoStatus? status = null,
            int maxResultCount = 10,
            int skipCount = 0)
        {
            var dbSet = await GetDbSetAsync();
            
            var query = dbSet
                .Where(x => x.UserId == userId && !x.IsDeleted);
            
            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }
            
            return await query
                .OrderByDescending(x => x.PublishTime)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync();
        }
        
        public async Task<List<Video>> GetHotVideosAsync(int maxResultCount = 10)
        {
            var dbSet = await GetDbSetAsync();
            
            return await dbSet
                .Where(x => x.Status == VideoStatus.Published && !x.IsDeleted)
                .OrderByDescending(x => x.TotalViews)
                .Take(maxResultCount)
                .ToListAsync();
        }
        
        public async Task IncrementViewCountAsync(Guid videoId)
        {
            var dbContext = await GetDbContextAsync();
            
            // 使用EF Core原子更新
            await dbContext.Database.ExecuteSqlRawAsync(
                "UPDATE \"Videos\" SET \"TotalViews\" = \"TotalViews\" + 1 WHERE \"Id\" = {0}",
                videoId);
        }
    }
}
```

---

## 6. 数据迁移脚本

### 6.1 创建数据库迁移

```csharp
// VideoDbContext迁移（使用ABP迁移工具）
using Microsoft.EntityFrameworkCore.Migrations;

namespace LCH.MicroService.Video.EntityFrameworkCore.Migrations
{
    public partial class InitialVideoSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 创建Categories表
            migrationBuilder.CreateTable("Categories", table => new
            {
                Id = Column<Guid>(nullable: false),
                Name = Column<string>(maxLength: 50, nullable: false),
                Code = Column<string>(maxLength: 20, nullable: false),
                IconUrl = Column<string>(maxLength: 500, nullable: true),
                Description = Column<string>(maxLength: 200, nullable: true),
                ParentId = Column<Guid>(nullable: true),
                Level = Column<int>(nullable: false, defaultValue: 1),
                SortOrder = Column<int>(nullable: false, defaultValue: 0),
                VideoCount = Column<long>(nullable: false, defaultValue: 0),
                IsActive = Column<bool>(nullable: false, defaultValue: true),
                CreationTime = Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Categories", x => x.Id);
                table.ForeignKey(
                    "FK_Categories_Categories_ParentId",
                    x => x.ParentId,
                    "Categories",
                    "Id",
                    onDelete: ReferentialAction.SetNull);
            });
            
            // 创建Videos表
            migrationBuilder.CreateTable("Videos", table => new
            {
                Id = Column<Guid>(nullable: false),
                UserId = Column<Guid>(nullable: false),
                Title = Column<string>(maxLength: 200, nullable: false),
                Description = Column<string>(maxLength: 5000, nullable: true),
                // ... 其他字段
                CreationTime = Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Videos", x => x.Id);
                table.ForeignKey(
                    "FK_Videos_Categories_CategoryId",
                    x => x.CategoryId,
                    "Categories",
                    "Id");
            });
            
            // 创建索引
            migrationBuilder.CreateIndex(
                "IX_Videos_UserId_Status",
                "Videos",
                new[] { "UserId", "Status" });
            
            // 插入初始数据
            migrationBuilder.Sql(@"
                INSERT INTO \"Categories\" (\"Id\", \"Name\", \"Code\", \"SortOrder\", \"Level\", \"CreationTime\") VALUES
                (gen_random_uuid(), '动画', 'animation', 1, 1, NOW()),
                (gen_random_uuid(), '游戏', 'game', 2, 1, NOW()),
                -- ... 其他分区
            ");
        }
        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Videos");
            migrationBuilder.DropTable("Categories");
        }
    }
}
```

---

## 7. 数据库配置文件

### 7.1 appsettings.json配置

```json
{
  "ConnectionStrings": {
    "Default": "Host=postgres;Database=VideoDB;Username=postgres;Password=password;Port=5432"
  },
  
  "AbpDbContextOptions": {
    "PostgreSQL": {
      "EnableSensitiveDataLogging": false,
      "EnableDetailedErrors": true
    }
  }
}
```

---

## 8. 统计汇总

### 8.1 表数量统计

| 模块 | 新增表数 | 继承ABP表数 | 总计 |
|------|---------|-----------|------|
| 用户模块 | 3 | 11 | 14 |
| 视频模块 | 4 | 0 | 4 |
| 弹幕模块 | 1（分区表） | 0 | 1 |
| 互动模块 | 4 | 0 | 4 |
| 直播模块 | 2 | 0 | 2 |
| 分区管理 | 1 | 0 | 1 |
| 搜索模块 | 1 | 0 | 1 |
| 管理模块 | 0 | 2 | 2 |
| **合计** | **16** | **13** | **29** |

### 8.2 索引数量统计

- 主键索引：29个（每表1个）
- 外键索引：约15个
- 业务索引：约20个
- 分区索引：弹幕表分区索引约5个
- **总计约69个索引**

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 数据库设计完成