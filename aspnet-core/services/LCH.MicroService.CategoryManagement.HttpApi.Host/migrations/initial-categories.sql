-- Initial Categories Data Migration Script
-- 10 Main Categories (Level 0) with Sub-categories (Level 1)
-- Table: AbpCategories

-- 1. 动画 (animation)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000001', 'animation', '动画', NULL, NULL, '动画相关内容分区', 1, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000001', 'bangumi', '番剧', '10000000-0000-0000-0000-000000000001', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000001', 'domestic-animation', '国产动画', '10000000-0000-0000-0000-000000000001', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000001', 'animation-short', '动画短片', '10000000-0000-0000-0000-000000000001', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 2. 游戏 (game)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000002', 'game', '游戏', NULL, NULL, '游戏相关内容分区', 2, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000002', 'single-player', '单机游戏', '10000000-0000-0000-0000-000000000002', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000002', 'online-game', '网络游戏', '10000000-0000-0000-0000-000000000002', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000002', 'e-sports', '电子竞技', '10000000-0000-0000-0000-000000000002', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 3. 音乐 (music)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000003', 'music', '音乐', NULL, NULL, '音乐相关内容分区', 3, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000003', 'music-mv', '音乐MV', '10000000-0000-0000-0000-000000000003', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000003', 'cover', '翻唱', '10000000-0000-0000-0000-000000000003', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000003', 'vocaloid', 'VOCALOID', '10000000-0000-0000-0000-000000000003', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 4. 科技 (tech)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000004', 'tech', '科技', NULL, NULL, '科技相关内容分区', 4, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000004', 'tech-review', '科技评测', '10000000-0000-0000-0000-000000000004', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000004', 'popular-science', '科普', '10000000-0000-0000-0000-000000000004', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000004', 'digital', '数码', '10000000-0000-0000-0000-000000000004', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 5. 生活 (life)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000005', 'life', '生活', NULL, NULL, '生活相关内容分区', 5, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000005', 'life-record', '生活记录', '10000000-0000-0000-0000-000000000005', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000005', 'food', '美食', '10000000-0000-0000-0000-000000000005', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000005', 'travel', '旅行', '10000000-0000-0000-0000-000000000005', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 6. 知识 (knowledge)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000006', 'knowledge', '知识', NULL, NULL, '知识相关内容分区', 6, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000006', 'tutorial', '教程', '10000000-0000-0000-0000-000000000006', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000006', 'knowledge-sharing', '知识分享', '10000000-0000-0000-0000-000000000006', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000006', 'course', '课程', '10000000-0000-0000-0000-000000000006', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 7. 影视 (movie)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000007', 'movie', '影视', NULL, NULL, '影视相关内容分区', 7, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000007', 'film', '电影', '10000000-0000-0000-0000-000000000007', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000007', 'tv-series', '电视剧', '10000000-0000-0000-0000-000000000007', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000007', 'documentary', '纪录片', '10000000-0000-0000-0000-000000000007', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 8. 娱乐 (entertainment)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000008', 'entertainment', '娱乐', NULL, NULL, '娱乐相关内容分区', 8, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000008', 'variety', '综艺', '10000000-0000-0000-0000-000000000008', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000008', 'entertainment-news', '娱乐资讯', '10000000-0000-0000-0000-000000000008', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000003-0000-0000-0000-000000000008', 'celebrity', '明星', '10000000-0000-0000-0000-000000000008', NULL, NULL, 3, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 9. 动漫 (anime)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000009', 'anime', '动漫', NULL, NULL, '动漫相关内容分区', 9, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000009', 'anime-works', '动画作品', '10000000-0000-0000-0000-000000000009', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000009', 'animation-news', '动画资讯', '10000000-0000-0000-0000-000000000009', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

-- 10. 其他 (other)
INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000000-0000-0000-0000-000000000010', 'other', '其他', NULL, NULL, '其他内容分区', 10, 0, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000001-0000-0000-0000-000000000010', 'other-content', '其他内容', '10000000-0000-0000-0000-000000000010', NULL, NULL, 1, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);

INSERT INTO "AbpCategories" ("Id", "Name", "DisplayName", "ParentId", "Icon", "Description", "SortOrder", "Level", "IsEnabled", "CreationTime", "CreatorId", "LastModificationTime", "LastModifierId", "IsDeleted", "DeleterId", "DeletionTime")
VALUES ('10000002-0000-0000-0000-000000000010', 'uncategorized', '未分类', '10000000-0000-0000-0000-000000000010', NULL, NULL, 2, 1, true, NOW(), NULL, NULL, NULL, false, NULL, NULL);