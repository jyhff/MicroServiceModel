# 互动模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | InteractionService（互动服务） |
| 服务地址 | http://interaction-service:5004 |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. 模块概述

### 2.1 服务职责

- 视频点赞管理
- 视频投币管理（无限制投币）
- 视频收藏管理
- 视频评论管理（人工审核）
- 视频分享管理
- 用户关注管理
- 用户行为记录（推荐算法数据）

---

## 3. API接口列表

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 点赞视频 | POST | /api/interaction/videos/{videoId}/like | 需认证 |
| 2 | 取消点赞 | DELETE | /api/interaction/videos/{videoId}/like | 需认证 |
| 3 | 查询点赞状态 | GET | /api/interaction/videos/{videoId}/like | 需认证 |
| 4 | 投币视频 | POST | /api/interaction/videos/{videoId}/coin | 需认证 |
| 5 | 查询投币记录 | GET | /api/interaction/videos/{videoId}/coins | 公开 |
| 6 | 收藏视频 | POST | /api/interaction/videos/{videoId}/favorite | 需认证 |
| 7 | 获取收藏列表 | GET | /api/interaction/users/me/favorites | 需认证 |
| 8 | 创建收藏夹 | POST | /api/interaction/favorite-groups | 需认证 |
| 9 | 发送评论 | POST | /api/interaction/videos/{videoId}/comments | 需认证 |
| 10 | 获取评论列表 | GET | /api/interaction/videos/{videoId}/comments | 公开 |
| 11 | 删除评论 | DELETE | /api/interaction/comments/{commentId} | 需认证 |
| 12 | 分享视频 | POST | /api/interaction/videos/{videoId}/share | 需认证 |

**总计：12个API接口**

---

## 4. 点赞接口

### 4.1 点赞视频

#### 基本信息

```
POST /api/interaction/videos/{videoId}/like
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "userId": "...",
  "videoId": "...",
  "isLiked": true,
  "likeTime": "2026-05-12T10:00:00Z",
  "videoLikeCount": 501
}
```

---

### 4.2 取消点赞

#### 基本信息

```
DELETE /api/interaction/videos/{videoId}/like
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "videoId": "...",
  "isLiked": false,
  "videoLikeCount": 500
}
```

---

### 4.3 查询点赞状态

#### 基本信息

```
GET /api/interaction/videos/{videoId}/like
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "videoId": "...",
  "isLiked": true,
  "likeTime": "..."
}
```

---

## 5. 投币接口

### 5.1 投币视频

#### 基本信息

```
POST /api/interaction/videos/{videoId}/coin
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| videoId | UUID | ✅ | 视频ID |

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| coinAmount | int | ✅ | 投币数量（无限制，1-N） |

#### 请求示例

```json
{
  "coinAmount": 2
}
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "userId": "...",
  "videoId": "...",
  "coinAmount": 2,
  "receiverUserId": "...",
  "creationTime": "...",
  "userCoinBalance": 998,
  "videoCoinCount": 102
}
```

#### 错误响应（400 Bad Request）

```json
{
  "error": {
    "code": "Interaction:InsufficientCoins",
    "message": "B币余额不足"
  }
}
```

---

### 5.2 查询投币记录

#### 基本信息

```
GET /api/interaction/videos/{videoId}/coins?maxResultCount={int}&skipCount={int}
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "userId": "...",
      "userName": "用户A",
      "userAvatar": "...",
      "coinAmount": 2,
      "creationTime": "..."
    }
  ],
  "totalCount": 100,
  "totalCoins": 200
}
```

---

## 6. 收藏接口

### 6.1 收藏视频

#### 基本信息

```
POST /api/interaction/videos/{videoId}/favorite
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| favoriteGroupId | UUID | ❌ | 收藏夹ID（默认收藏到默认收藏夹） |
| isFavorite | boolean | ✅ | 是否收藏 |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "userId": "...",
  "videoId": "...",
  "favoriteGroupId": "...",
  "isFavorite": true,
  "favoriteTime": "...",
  "videoFavoriteCount": 201
}
```

---

### 6.2 获取收藏列表

#### 基本信息

```
GET /api/interaction/users/me/favorites?favoriteGroupId={id}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "videoId": "...",
      "videoTitle": "视频标题",
      "coverImageUrl": "...",
      "durationSeconds": 120,
      "favoriteTime": "..."
    }
  ],
  "totalCount": 50
}
```

---

### 6.3 创建收藏夹

#### 基本信息

```
POST /api/interaction/favorite-groups
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| name | string | ✅ | 收藏夹名称 |
| description | string | ❌ | 收藏夹描述 |
| isPublic | boolean | ❌ | 是否公开（默认false） |

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "userId": "...",
  "name": "我的收藏夹",
  "description": "...",
  "isPublic": false,
  "videoCount": 0,
  "creationTime": "..."
}
```

---

## 7. 评论接口

### 7.1 发送评论（需人工审核）

#### 基本信息

```
POST /api/interaction/videos/{videoId}/comments
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| content | string | ✅ | 评论内容（最大1000字符） |
| parentCommentId | UUID | ❌ | 父评论ID（楼中楼） |
| replyToUserId | UUID | ❌ | 回复的用户ID |

#### 请求示例

```json
{
  "content": "这个视频很好！",
  "parentCommentId": null,
  "replyToUserId": null
}
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "userId": "...",
  "videoId": "...",
  "content": "这个视频很好！",
  "parentCommentId": null,
  "auditStatus": "PendingReview",
  "isPublished": false,
  "message": "评论已提交，等待人工审核",
  "creationTime": "..."
}
```

#### 说明

- **评论需人工审核**：管理员审核通过后才公开显示
- **审核状态**：PendingReview（待审）、Approved（通过）、Rejected（拒绝）

---

### 7.2 获取评论列表（已审核通过）

#### 基本信息

```
GET /api/interaction/videos/{videoId}/comments?maxResultCount={int}&skipCount={int}&sortOrder={string}
权限：公开
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| maxResultCount | int | ❌ | 返回数量（默认20） |
| skipCount | int | ❌ | 跳过数量（默认0） |
| sortOrder | string | ❌ | 排序（hot/new，默认hot） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "...",
      "userId": "...",
      "userName": "用户A",
      "userAvatar": "...",
      "userLevel": 3,
      "content": "评论内容",
      "likeCount": 50,
      "replyCount": 10,
      "parentCommentId": null,
      "replies": [
        {
          "id": "...",
          "userId": "...",
          "userName": "用户B",
          "content": "回复内容",
          "replyToUserName": "用户A",
          "likeCount": 5,
          "creationTime": "..."
        }
      ],
      "publishTime": "...",
      "creationTime": "..."
    }
  ],
  "totalCount": 200
}
```

---

### 7.3 删除评论

#### 基本信息

```
DELETE /api/interaction/comments/{commentId}
Authorization: Bearer {token}
权限：需认证（仅评论作者）
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "isDeleted": true,
  "deletionTime": "..."
}
```

---

## 8. 分享接口

### 8.1 分享视频

#### 基本信息

```
POST /api/interaction/videos/{videoId}/share
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| sharePlatform | string | ✅ | 分享平台 |

#### 请求示例

```json
{
  "sharePlatform": "WECHAT"
}
```

#### 成功响应（200 OK）

```json
{
  "id": "...",
  "userId": "...",
  "videoId": "...",
  "sharePlatform": "WECHAT",
  "shareTime": "...",
  "videoShareCount": 51,
  "experienceEarned": 2
}
```

#### 分享平台枚举

| 平台 | 说明 |
|------|------|
| WECHAT | 微信 |
| WEIBO | 微博 |
| QQ | QQ |
| TWITTER | Twitter |
| FACEBOOK | Facebook |
| COPY_LINK | 复制链接 |

---

## 9. 错误码汇总

| 错误码 | HTTP状态码 | 说明 |
|--------|-----------|------|
| Interaction:InsufficientCoins | 400 | B币余额不足 |
| Interaction:AlreadyLiked | 400 | 已点赞 |
| Interaction:NotLiked | 400 | 未点赞 |
| Interaction:CommentNotFound | 404 | 评论不存在 |
| Interaction:NotCommentOwner | 403 | 不是评论作者 |
| Interaction:CommentAuditPending | 400 | 评论待审核 |
| Interaction:CommentRejected | 400 | 评论审核拒绝 |

---

## 10. 接口性能指标

| 接口 | 平均响应时间 | QPS上限 | 说明 |
|------|------------|---------|------|
| 点赞视频 | <50ms | 2000 | 高频接口 |
| 投币视频 | <100ms | 1000 | 中频接口 |
| 收藏视频 | <50ms | 500 | 低频接口 |
| 发送评论 | <100ms | 500 | 低频接口 |
| 获取评论列表 | <100ms | 1000 | 中频接口 |
| 分享视频 | <50ms | 500 | 低频接口 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 互动模块API文档完成