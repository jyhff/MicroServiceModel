# Bilibili技术文档索引

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 技术文档索引 |
| 目标项目 | Bilibili视频平台 |
| 创建时间 | 2026-05-12 |

---

## 2. 文档目录结构

```
.sisyphus/plans/
├── prd-01-user-system-detailed.md            # 用户系统PRD
├── prd-02-video-system-detailed.md           # 视频系统PRD
├── prd-03-danmaku-system-detailed.md         # 弹幕系统PRD
├── prd-04-recommendation-system-detailed.md  # 推荐系统PRD
├── prd-05-search-system-detailed.md          # 搜索系统PRD
├── prd-06-live-system-detailed.md            # 直播系统PRD
├── prd-07-interaction-system-detailed.md     # 互动系统PRD
├── prd-08-category-management-detailed.md    # 分区管理PRD
├── prd-09-admin-system-detailed.md           # 管理后台PRD
├── prd-index-all-modules.md                  # PRD文档索引
│
├── tech-01-architecture-design.md            # 技术架构设计文档
├── tech-02-database-design.md                # 数据库设计文档
├── tech-03-api-interface-design.md           # API接口设计文档
├── tech-03-api-design.md                     # API接口详细设计文档
├── tech-04-code-architecture-design.md       # 代码架构设计文档
├── tech-04-code-architecture.md              # 代码架构详细设计文档
├── tech-05-deployment-operations.md          # 部署运维文档
├── tech-06-test-strategy.md                  # 测试策略文档（待创建）
└── tech-index-all-modules.md                 # 技术文档索引（当前文档）
```

---

## 3. PRD文档列表

| 序号 | 文档名称 | 功能模块 | 状态 | 说明 |
|------|---------|---------|------|------|
| 1 | prd-01-user-system-detailed.md | 用户系统 | ✅ 完成 | 注册登录、LV0-LV6等级、实名认证、消息中心 |
| 2 | prd-02-video-system-detailed.md | 视频系统 | ✅ 完成 | 无限制上传、CPU转码、MinIO存储、HLS播放 |
| 3 | prd-03-danmaku-system-detailed.md | 弹幕系统 | ✅ 完成 | WebSocket推送、PostgreSQL分区表、LV2+高级弹幕 |
| 4 | prd-04-recommendation-system-detailed.md | 推荐系统 | ✅ 完成 | 协同过滤+热门推荐、Redis缓存 |
| 5 | prd-05-search-system-detailed.md | 搜索系统 | ✅ 完成 | Elasticsearch实时搜索、ik分词、热搜榜 |
| 6 | prd-06-live-system-detailed.md | 直播系统 | ✅ 完成 | HLS直播、礼物100%给UP主、实时互动 |
| 7 | prd-07-interaction-system-detailed.md | 互动系统 | ✅ 完成 | 点赞、投币（无限制）、评论（人工审核）、分享 |
| 8 | prd-08-category-management-detailed.md | 分区管理 | ✅ 完成 | 10个主分区、AI自动审核通过 |
| 9 | prd-09-admin-system-detailed.md | 管理后台 | ✅ 完成 | 超级管理员、视频审核、评论审核、用户封禁 |

---

## 4. 技术文档列表

| 序号 | 文档名称 | 文档类型 | 状态 | 内容摘要 |
|------|---------|---------|------|---------|
| 1 | tech-01-architecture-design.md | 技术架构设计 | ✅ 完成 | ABP微服务架构、服务通信、基础设施、技术选型 |
| 2 | tech-02-database-design.md | 数据库设计 | ✅ 完成 | PostgreSQL分库策略、16张新增表、分区表设计、索引策略 |
| 3 | tech-03-api-interface-design.md | API接口设计 | ✅ 完成 | 48个HTTP API接口、2个WebSocket接口、ABP规范继承 |
| 4 | tech-03-api-design.md | API接口详细设计 | ✅ 完成 | RESTful规范、ABP响应格式、详细接口定义、代码示例 |
| 5 | tech-04-code-architecture-design.md | 代码架构设计 | ✅ 完成 | ABP DDD分层、模块结构、实体设计、Repository实现 |
| 6 | tech-04-code-architecture.md | 代码架构详细设计 | ✅ 完成 | 各模块目录结构、代码示例、跨模块通信、事件总线 |
| 7 | tech-05-deployment-operations.md | 部署运维 | ✅ 完成 | Docker Compose配置、K8s Deployment、监控配置、运维脚本 |
| 8 | tech-06-test-strategy.md | 测试策略 | ⏳ 待创建 | 单元测试、集成测试、E2E测试策略 |

---

## 5. 文档统计汇总

### 5.1 PRD文档统计

| 模块 | 功能点数 | API接口数 | 数据表数 | 状态 |
|------|---------|----------|---------|------|
| 用户系统 | 15 | 10 | 4 | ✅ 完成 |
| 视频系统 | 20 | 12 | 5 | ✅ 完成 |
| 弹幕系统 | 8 | 5 | 2 | ✅ 完成 |
| 推荐系统 | 5 | 2 | 0 | ✅ 完成 |
| 搜索系统 | 6 | 3 | 1 | ✅ 完成 |
| 直播系统 | 15 | 7 | 2 | ✅ 完成 |
| 互动系统 | 12 | 9 | 6 | ✅ 完成 |
| 分区管理 | 4 | 2 | 1 | ✅ 完成 |
| 管理后台 | 8 | 6 | 0 | ✅ 完成 |
| **合计** | **89** | **48+2(WS)** | **21** | **全部完成** |

### 5.2 技术文档统计

| 文档类型 | 页数估计 | 主要内容 | 状态 |
|---------|---------|---------|------|
| 技术架构设计 | 15页 | 服务架构图、通信机制、技术选型 | ✅ 完成 |
| 数据库设计 | 20页 | 表结构、索引、分区表、EF Core代码 | ✅ 完成 |
| API接口设计 | 25页 | 48个HTTP接口、WebSocket接口 | ✅ 完成 |
| 代码架构设计 | 20页 | 模块结构、代码示例、事件总线 | ✅ 完成 |
| 部署运维 | 15页 | Docker/K8s配置、监控、脚本 | ✅ 完成 |
| 测试策略 | 10页 | 测试覆盖策略（待创建） | ⏳ 待创建 |
| **合计** | **105页** | **完整技术文档** | **83%完成** |

---

## 6. 关键决策记录

### 6.1 技术选型决策

| 决策项 | 选择 | 原因 | 文档位置 |
|--------|------|------|---------|
| 框架版本 | .NET 10.0 + ABP v10.0.2 | 继承现有项目 | tech-01 |
| 数据库 | PostgreSQL | 继承现有配置 | tech-02 |
| 缓存 | Redis | 继承现有配置 | tech-01 |
| 消息队列 | RabbitMQ + CAP | 继承现有配置 | tech-01 |
| 搜索引擎 | Elasticsearch | 继承现有配置 | tech-01 |
| 对象存储 | MinIO | 继承现有OssManagement | tech-01 |
| 实时通信 | SignalR + Redis Scaleout | 继承现有RealtimeMessage | tech-01 |
| 视频转码 | FFmpeg CPU | 成本低、足够满足需求 | tech-01 |
| CDN | 无（仅MinIO本地） | 确认需求、成本低 | tech-01 |

### 6.2 需求边界决策

| 决策项 | 选择 | 原因 | 文档位置 |
|--------|------|------|---------|
| 视频时长限制 | 无限制 | 用户需求 | prd-02 |
| 视频大小限制 | 无限制 | 用户需求 | prd-02 |
| 清晰度选择 | 全部免费 | 用户需求 | prd-02 |
| 投币数量 | 无限制 | 用户需求 | prd-07 |
| 评论审核 | 人工审核 | 内容安全 | prd-07 |
| 弹幕高级功能 | LV2+可用 | 等级激励 | prd-03 |
| 礼物分成 | 100%给UP主 | 用户需求 | prd-06 |
| 分类审核 | AI自动通过 | 提高效率 | prd-08 |
| 管理员角色 | 仅超级管理员 | 简化权限 | prd-09 |

---

## 7. 文档使用指南

### 7.1 查阅顺序建议

```
推荐查阅顺序：

1. 产品需求文档（PRD）
   ├── prd-index-all-modules.md（PRD索引）
   ├── prd-01-user-system-detailed.md（用户系统）
   ├── prd-02-video-system-detailed.md（视频系统）
   └── 其他PRD文档...

2. 技术设计文档
   ├── tech-01-architecture-design.md（总体架构）
   ├── tech-02-database-design.md（数据库设计）
   ├── tech-03-api-interface-design.md（API接口）
   ├── tech-04-code-architecture-design.md（代码架构）
   ├── tech-05-deployment-operations.md（部署运维）
   └── tech-06-test-strategy.md（测试策略）

3. 实施开发
   ├── 参考代码架构设计文档创建模块
   ├── 参考API接口文档实现接口
   ├── 参考数据库设计文档创建表结构
   └── 参考部署运维文档配置环境
```

---

## 8. 文档维护记录

| 更新时间 | 更新内容 | 更新人 | 版本 |
|---------|---------|--------|------|
| 2026-05-12 | 创建PRD文档索引 | Prometheus | v1.0 |
| 2026-05-12 | 创建技术架构设计文档 | Prometheus | v1.0 |
| 2026-05-12 | 创建数据库设计文档 | Prometheus | v1.0 |
| 2026-05-12 | 创建API接口设计文档 | Prometheus | v1.0 |
| 2026-05-12 | 创建代码架构设计文档 | Prometheus | v1.0 |
| 2026-05-12 | 创建部署运维文档 | Prometheus | v1.0 |

---

## 9. 下一步计划

| 任务 | 状态 | 说明 |
|------|------|------|
| 创建测试策略文档 | ⏳ 待创建 | 单元测试、集成测试、E2E测试 |
| 开始实施开发 | ⏳ 待启动 | 运行 `/start-work` 开始执行 |
| 创建API Gateway配置 | ⏳ 待实施 | 继承现有Ocelot配置 |
| 创建FFmpeg转码配置 | ⏳ 待实施 | 转码Worker具体实现 |

---

## 10. 文档质量检查清单

### 10.1 PRD文档检查

| 检查项 | 状态 | 说明 |
|--------|------|------|
| 功能需求明确 | ✅ | 所有模块功能已定义 |
| 接口定义完整 | ✅ | HTTP + WebSocket接口已定义 |
| 数据表设计完整 | ✅ | 21张新增表已定义 |
| 业务流程清晰 | ✅ | 核心流程已说明 |
| 状态跟踪完整 | ✅ | 所有TODO已跟踪 |

### 10.2 技术文档检查

| 检查项 | 状态 | 说明 |
|--------|------|------|
| 技术选型明确 | ✅ | 继承现有ABP架构 |
| 架构设计完整 | ✅ | 微服务架构已设计 |
| 代码结构清晰 | ✅ | ABP DDD分层已定义 |
| 部署配置完整 | ✅ | Docker/K8s已配置 |
| 监控配置完整 | ✅ | Prometheus已配置 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 技术文档索引完成