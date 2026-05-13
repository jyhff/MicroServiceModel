# 管理模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | AdminService（管理后台） |
| 服务地址 | http://admin-service:5010 |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. 模块概述

### 2.1 服务职责

- 视频审核管理（人工审核）
- 评论审核管理（人工审核）
- 弹幕举报处理
- 用户封禁管理

### 2.2 权限要求

- **仅超级管理员**：所有管理接口仅超级管理员可访问

---

## 3. API接口列表

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 获取待审核视频 | GET | /api/admin/videos/pending | 超级管理员 |
| 2 | 审核视频 | POST | /api/admin/videos/{id}/audit | 超级管理员 |
| 3 | 获取待审核评论 | GET | /api/admin/comments/pending | 超级管理员 |
| 4 | 审核评论 | POST | /api/admin/comments/{id}/audit | 超级管理员 |
| 5 | 处理弹幕举报 | POST | /api/admin/danmaku/reports/{id}/handle | 超级管理员 |
| 6 | 封禁用户 | POST | /api/admin/users/{id}/ban | 超级管理员 |

**总计：6个API接口**

---

## 4. 视频审核接口

### 4.1 获取待审核视频

#### 基本信息

```
GET /api/admin/videos/pending?auditStatus={int}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：超级管理员
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "userId": "...",
      "userName": "UP主",
      "title": "待审核视频",
      "description": "...",
      "coverImageUrl": "...",
      "durationSeconds": 120,
      "categoryId": "...",
      "auditStatus": 0,
      "creationTime": "..."
    }
  ],
  "totalCount": 50
}
```

---

### 4.2 审核视频

#### 基本信息

```
POST /api/admin/videos/{id}/audit
Authorization: Bearer {token}
Content-Type: application/json
权限：超级管理员
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| auditStatus | int | ✅ | 审核结果（1=通过 2=拒绝） |
| auditReason | string | ❌ | 审核意见 |

#### 请求示例

```json
{
  "auditStatus": 1,
  "auditReason": "内容符合规范"
}
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "auditStatus": 1,
  "auditTime": "...",
  "auditorId": "...",
  "isPublished": true,
  "publishTime": "..."
}
```

---

## 5. 评论审核接口

### 5.1 获取待审核评论

#### 基本信息

```
GET /api/admin/comments/pending?maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：超级管理员
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "videoId": "...",
      "videoTitle": "...",
      "userId": "...",
      "userName": "用户",
      "content": "评论内容",
      "auditStatus": 0,
      "creationTime": "..."
    }
  ],
  "totalCount": 100
}
```

---

### 5.2 审核评论

#### 基本信息

```
POST /api/admin/comments/{id}/audit
Authorization: Bearer {token}
Content-Type: application/json
权限：超级管理员
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| auditStatus | int | ✅ | 审核结果（1=通过 2=拒绝） |
| auditReason | string | ❌ | 审核意见 |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "auditStatus": 1,
  "auditTime": "...",
  "isPublished": true,
  "publishTime": "..."
}
```

---

## 6. 弹幕举报处理接口

### 6.1 处理弹幕举报

#### 基本信息

```
POST /api/admin/danmaku/reports/{id}/handle
Authorization: Bearer {token}
Content-Type: application/json
权限：超级管理员
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| handleStatus | int | ✅ | 处理结果（1=通过 2=拒绝） |
| handleResult | string | ❌ | 处理说明 |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "handleStatus": 1,
  "handleTime": "...",
  "danmakuAction": "deleted"
}
```

---

## 7. 用户封禁接口

### 7.1 封禁用户

#### 基本信息

```
POST /api/admin/users/{id}/ban
Authorization: Bearer {token}
Content-Type: application/json
权限：超级管理员
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| banExpireDate | date | ❌ | 封禁到期日期（永久封禁不填） |
| banReason | string | ✅ | 封禁原因 |

#### 成功响应（200 OK）

```json
{
  "userId": "...",
  "isBanned": true,
  "banExpireDate": "...",
  "banReason": "...",
  "bannedTime": "..."
}
```

---

## 8. 权限定义

| 权限名称 | 说明 |
|---------|------|
| Admin.Videos.Audit | 视频审核（超级管理员） |
| Admin.Comments.Audit | 评论审核（超级管理员） |
| Admin.Danmaku.Handle | 弹幕举报处理（超级管理员） |
| Admin.Users.Ban | 用户封禁（超级管理员） |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 管理模块API文档完成