# 分区与内容管理产品需求文档 (PRD)

## 1. 模块概述

### 1.2 核心功能
- 分区管理（CRUD）
- 内容审核
- 标签管理
- 举报处理

---

## 2. 分区管理

### 2.1 分区结构
| 分区 | 子分区 |
|------|--------|
| 动画 | 番剧、国产动画、动画电影 |
| 游戏 | 单机游戏、网络游戏、电子竞技 |
| 影视 | 电影、电视剧、纪录片 |
| 生活 | 美食、旅行、运动、日常 |
| 知识 | 科学、历史、职场、学习 |
| 科技 | 数码、科技、编程 |
| 音乐 | 翻唱、演奏、MV |
| 舞蹈 | 宅舞、明星舞蹈 |
| 时尚 | 美妆、穿搭、时尚资讯 |

### 2.2 分区表
```sql
CREATE TABLE "Categories" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(100) NOT NULL,
    "ParentId" uuid REFERENCES "Categories"("Id"),
    "Icon" varchar(500),
    "SortOrder" integer DEFAULT 0,
    "IsEnabled" boolean DEFAULT true,
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE INDEX "IX_Categories_ParentId" ON "Categories"("ParentId");
```

---

## 3. 内容审核

### 3.1 审核流程
```
视频上传 → AI审核 → [通过] → 公开
               ↓
            [可疑] → 人工审核 → [通过/拒绝]
               ↓
            [违规] → 拒绝上传
```

### 3.2 审核规则
| 违规类型 | 处理方式 |
|---------|---------|
| 色情低俗 | 拒绝+账号警告 |
| 暴力恐怖 | 拒绝+封号 |
| 政治敏感 | 人工复审 |
| 广告营销 | 限流或下架 |
| 侵权内容 | 下架+版权投诉 |

---

## 4. 标签管理

```sql
CREATE TABLE "Tags" (
    "Id" uuid PRIMARY KEY,
    "Name" varchar(50) NOT NULL UNIQUE,
    "CategoryId" uuid REFERENCES "Categories"("Id"),
    "VideoCount" integer DEFAULT 0,
    "IsHot" boolean DEFAULT false,
    "CreationTime" timestamp with time zone NOT NULL
);

CREATE INDEX "IX_Tags_CategoryId" ON "Tags"("CategoryId");
```

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0
