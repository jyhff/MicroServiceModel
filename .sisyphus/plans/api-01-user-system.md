# 用户模块API接口定义文档

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 模块名称 | UserService（用户服务） |
| 服务地址 | http://user-service:5009 |
| API版本 | v1.0 |
| 创建时间 | 2026-05-12 |

---

## 2. 模块概述

### 2.1 服务职责

- 用户注册与登录（继承现有IdentityServer）
- 用户扩展信息管理（BilibiliUser）
- 用户等级系统（LV0-LV6）
- 用户B币管理
- 实名认证
- UP主认证
- 用户消息管理
- 用户关注关系

### 2.2 继承关系

- **继承ABP Identity模块**：基础用户认证、JWT Token
- **继承现有IdentityServer**：OAuth认证授权
- **扩展BilibiliUser实体**：用户等级、B币、实名认证

---

## 3. API接口列表

| 序号 | 接口名称 | HTTP方法 | 路径 | 权限 |
|------|---------|---------|------|------|
| 1 | 用户注册 | POST | /api/identity/register | 公开 |
| 2 | 用户登录 | POST | /api/identity/login | 公开 |
| 3 | Token刷新 | POST | /api/identity/refresh-token | 需认证 |
| 4 | 获取用户信息 | GET | /api/bilibili/users/{id} | 公开 |
| 5 | 获取我的信息 | GET | /api/bilibili/users/me | 需认证 |
| 6 | 更新用户信息 | PUT | /api/bilibili/users/me | 需认证 |
| 7 | 获取等级信息 | GET | /api/bilibili/users/me/level | 需认证 |
| 8 | 获取经验记录 | GET | /api/bilibili/users/me/experience-records | 需认证 |
| 9 | 获取B币余额 | GET | /api/bilibili/users/me/coins/balance | 需认证 |
| 10 | 获取B币记录 | GET | /api/bilibili/users/me/coins/records | 需认证 |
| 11 | 实名认证 | POST | /api/bilibili/users/me/verify/realname | 需认证 |
| 12 | UP主认证 | POST | /api/bilibili/users/me/verify/up | 需认证 |
| 13 | 获取消息列表 | GET | /api/bilibili/users/me/messages | 需认证 |
| 14 | 发送私信 | POST | /api/bilibili/users/me/messages | 需认证 |
| 15 | 关注用户 | POST | /api/bilibili/users/{id}/follow | 需认证 |
| 16 | 获取关注列表 | GET | /api/bilibili/users/{id}/following | 公开 |
| 17 | 获取粉丝列表 | GET | /api/bilibili/users/{id}/followers | 公开 |

**总计：17个API接口**

---

## 4. 认证相关接口

### 4.1 用户注册

#### 基本信息

```
POST /api/identity/register
Content-Type: application/json
权限：公开
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| userName | string | ✅ | 用户名（4-20字符） |
| email | string | ✅ | 邮箱地址 |
| password | string | ✅ | 密码（6-20字符） |
| captchaCode | string | ✅ | 验证码 |
| captchaKey | string | ✅ | 验证码Key |

#### 请求示例

```json
{
  "userName": "testuser",
  "email": "test@example.com",
  "password": "Test@123456",
  "captchaCode": "abc123",
  "captchaKey": "captcha_key_123"
}
```

#### 成功响应（200 OK）

```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "userName": "testuser",
  "email": "test@example.com",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_string",
  "expiresIn": 3600
}
```

#### 错误响应（400 Bad Request）

```json
{
  "error": {
    "code": "Identity:RegistrationFailed",
    "message": "注册失败",
    "validationErrors": [
      {
        "message": "用户名已存在",
        "members": ["userName"]
      }
    ]
  }
}
```

#### 错误码

| 错误码 | HTTP状态码 | 说明 |
|--------|-----------|------|
| Identity:RegistrationFailed | 400 | 注册失败 |
| Identity:UserNameAlreadyExists | 400 | 用户名已存在 |
| Identity:EmailAlreadyExists | 400 | 邮箱已存在 |
| Identity:InvalidPasswordFormat | 400 | 密码格式不正确 |
| Identity:InvalidCaptcha | 400 | 验证码错误 |

---

### 4.2 用户登录

#### 基本信息

```
POST /api/identity/login
Content-Type: application/json
权限：公开
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| userNameOrEmail | string | ✅ | 用户名或邮箱 |
| password | string | ✅ | 密码 |
| rememberMe | boolean | ❌ | 记住我（默认false） |

#### 请求示例

```json
{
  "userNameOrEmail": "testuser",
  "password": "Test@123456",
  "rememberMe": true
}
```

#### 成功响应（200 OK）

```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "userName": "testuser",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_string",
  "expiresIn": 3600
}
```

#### 错误响应（401 Unauthorized）

```json
{
  "error": {
    "code": "Identity:InvalidCredentials",
    "message": "用户名或密码错误"
  }
}
```

#### 错误码

| 错误码 | HTTP状态码 | 说明 |
|--------|-----------|------|
| Identity:InvalidCredentials | 401 | 用户名或密码错误 |
| Identity:UserLockedOut | 403 | 用户已锁定 |
| Identity:UserNotActive | 403 | 用户未激活 |

---

### 4.3 Token刷新

#### 基本信息

```
POST /api/identity/refresh-token
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| refreshToken | string | ✅ | Refresh Token |

#### 请求示例

```json
{
  "refreshToken": "refresh_token_string"
}
```

#### 成功响应（200 OK）

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new_refresh_token_string",
  "expiresIn": 3600
}
```

#### 错误响应（401 Unauthorized）

```json
{
  "error": {
    "code": "Identity:InvalidRefreshToken",
    "message": "Refresh Token无效或已过期"
  }
}
```

---

## 5. 用户信息接口

### 5.1 获取用户信息

#### 基本信息

```
GET /api/bilibili/users/{id}
权限：公开
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| id | UUID | ✅ | 用户ID |

#### 成功响应（200 OK）

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "userName": "testuser",
  "nickName": "测试用户",
  "avatar": "avatars/user_001.jpg",
  "signature": "这是我的个性签名",
  "gender": 1,
  "birthday": "1990-01-01",
  "level": 3,
  "levelProgress": 50,
  "totalExperience": 250,
  "isVerified": false,
  "isUpVerified": true,
  "isRealNameVerified": true,
  "videoCount": 50,
  "followerCount": 1000,
  "followingCount": 200,
  "isFollowed": false,
  "creationTime": "2026-01-01T00:00:00Z"
}
```

#### 错误响应（404 Not Found）

```json
{
  "error": {
    "code": "Bilibili:UserNotFound",
    "message": "用户不存在"
  }
}
```

---

### 5.2 获取我的信息

#### 基本信息

```
GET /api/bilibili/users/me
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "userName": "testuser",
  "email": "test@example.com",
  "nickName": "测试用户",
  "avatar": "avatars/user_001.jpg",
  "signature": "个性签名",
  "level": 3,
  "experience": 250,
  "nextLevelExperience": 400,
  "levelProgress": 50,
  "coinsBalance": 1000,
  "dailyExperience": 50,
  "dailyExperienceLimit": 100,
  "isRealNameVerified": true,
  "isUpVerified": true,
  "isVip": false,
  "videoCount": 50,
  "followerCount": 1000,
  "followingCount": 200,
  "creationTime": "2026-01-01T00:00:00Z"
}
```

---

### 5.3 更新用户信息

#### 基本信息

```
PUT /api/bilibili/users/me
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证（仅本人）
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| nickName | string | ❌ | 昵称（最大50字符） |
| avatar | string | ❌ | 头像（Base64或URL） |
| signature | string | ❌ | 签名（最大200字符） |
| gender | integer | ❌ | 性别（0未知 1男 2女） |
| birthday | date | ❌ | 生日 |

#### 请求示例

```json
{
  "nickName": "新昵称",
  "avatar": "base64_image_data",
  "signature": "新的个性签名",
  "gender": 1,
  "birthday": "1990-01-01"
}
```

#### 成功响应（200 OK）

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "nickName": "新昵称",
  "avatar": "avatars/user_001_new.jpg",
  "signature": "新的个性签名",
  "gender": 1,
  "lastModificationTime": "2026-05-12T10:00:00Z"
}
```

---

## 6. 用户等级接口

### 6.1 获取等级信息

#### 基本信息

```
GET /api/bilibili/users/me/level
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "level": 3,
  "levelProgress": 50,
  "currentExperience": 250,
  "nextLevelExperience": 400,
  "totalExperience": 250,
  "dailyEarned": 50,
  "dailyLimit": 100,
  "dailyRemaining": 50,
  "levelPrivileges": [
    "基础弹幕",
    "高级弹幕",
    "高级弹幕样式"
  ],
  "levelRequirements": [
    {
      "level": 0,
      "minExperience": 0,
      "maxExperience": 100,
      "privileges": ["基础弹幕"]
    },
    {
      "level": 1,
      "minExperience": 101,
      "maxExperience": 200,
      "privileges": ["基础弹幕"]
    },
    {
      "level": 2,
      "minExperience": 201,
      "maxExperience": 400,
      "privileges": ["基础弹幕", "高级弹幕"]
    },
    {
      "level": 3,
      "minExperience": 401,
      "maxExperience": 600,
      "privileges": ["基础弹幕", "高级弹幕", "高级弹幕样式"]
    },
    {
      "level": 4,
      "minExperience": 601,
      "maxExperience": 800,
      "privileges": ["基础弹幕", "高级弹幕", "高级弹幕样式", "彩色弹幕"]
    },
    {
      "level": 5,
      "minExperience": 801,
      "maxExperience": 1000,
      "privileges": ["全部弹幕功能"]
    },
    {
      "level": 6,
      "minExperience": 1001,
      "maxExperience": null,
      "privileges": ["全部弹幕功能", "专属标识"]
    }
  ]
}
```

---

### 6.2 获取经验记录

#### 基本信息

```
GET /api/bilibili/users/me/experience-records?startDate={date}&endDate={date}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：需认证
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| startDate | date | ❌ | 开始日期（默认今日） |
| endDate | date | ❌ | 结束日期（默认今日） |
| maxResultCount | int | ❌ | 返回数量（默认10） |
| skipCount | int | ❌ | 跳过数量（默认0） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "userId": "00000000-0000-0000-0000-000000000001",
      "experienceType": "LOGIN",
      "experienceAmount": 5,
      "sourceId": null,
      "description": "每日登录",
      "creationTime": "2026-05-12T08:00:00Z"
    },
    {
      "id": "00000000-0000-0000-0000-000000000002",
      "userId": "00000000-0000-0000-0000-000000000001",
      "experienceType": "WATCH_VIDEO",
      "experienceAmount": 1,
      "sourceId": "video-id-123",
      "description": "完整观看视频",
      "creationTime": "2026-05-12T09:00:00Z"
    },
    {
      "id": "00000000-0000-0000-0000-000000000003",
      "userId": "00000000-0000-0000-0000-000000000001",
      "experienceType": "LIKE_VIDEO",
      "experienceAmount": 1,
      "sourceId": "video-id-123",
      "description": "点赞视频",
      "creationTime": "2026-05-12T09:05:00Z"
    }
  ],
  "totalCount": 5,
  "dailyTotal": 7,
  "dailyLimit": 100
}
```

#### 经验类型枚举

| 类型 | 经验值 | 说明 |
|------|--------|------|
| LOGIN | +5 | 每日登录 |
| WATCH_VIDEO | +1 | 完整观看视频（进度≥90%） |
| LIKE_VIDEO | +1 | 点赞视频 |
| COIN_VIDEO | +1 | 投币视频 |
| COMMENT_VIDEO | +2 | 评论视频 |
| SHARE_VIDEO | +2 | 分享视频 |
| PUBLISH_VIDEO | +10 | 发布视频 |

---

## 7. 用户B币接口

### 7.1 获取B币余额

#### 基本信息

```
GET /api/bilibili/users/me/coins/balance
Authorization: Bearer {token}
权限：需认证
```

#### 成功响应（200 OK）

```json
{
  "coinsBalance": 1000,
  "coinsTotalEarned": 2000,
  "coinsTotalSpent": 1000,
  "lastUpdateTime": "2026-05-12T10:00:00Z"
}
```

---

### 7.2 获取B币记录

#### 基本信息

```
GET /api/bilibili/users/me/coins/records?maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：需认证
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| maxResultCount | int | ❌ | 返回数量（默认20） |
| skipCount | int | ❌ | 跳过数量（默认0） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "userId": "00000000-0000-0000-0000-000000000001",
      "transactionType": "COIN_VIDEO",
      "coinAmount": -2,
      "balanceAfter": 998,
      "sourceId": "video-id-123",
      "targetUserId": "up-owner-id",
      "description": "投币给视频《测试视频》",
      "creationTime": "2026-05-12T09:00:00Z"
    },
    {
      "id": "00000000-0000-0000-0000-000000000002",
      "userId": "00000000-0000-0000-0000-000000000001",
      "transactionType": "RECEIVE_COIN",
      "coinAmount": 5,
      "balanceAfter": 1005,
      "sourceId": "video-id-456",
      "description": "收到投币",
      "creationTime": "2026-05-12T10:00:00Z"
    },
    {
      "id": "00000000-0000-0000-0000-000000000003",
      "userId": "00000000-0000-0000-0000-000000000001",
      "transactionType": "WATCH_REWARD",
      "coinAmount": 1,
      "balanceAfter": 1006,
      "description": "观看视频奖励",
      "creationTime": "2026-05-12T11:00:00Z"
    }
  ],
  "totalCount": 50
}
```

#### B币交易类型枚举

| 类型 | 金额 | 说明 |
|------|------|------|
| ADMIN_GRANT | +N | 管理员发放 |
| WATCH_REWARD | +1 | 观看奖励 |
| RECEIVE_COIN | +N | 收到投币 |
| COIN_VIDEO | -N | 投币消耗 |

---

## 8. 认证接口

### 8.1 实名认证

#### 基本信息

```
POST /api/bilibili/users/me/verify/realname
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| realName | string | ✅ | 真实姓名（加密存储） |
| idCardNumber | string | ✅ | 身份证号（加密存储） |

#### 请求示例

```json
{
  "realName": "张三",
  "idCardNumber": "123456789012345678"
}
```

#### 成功响应（200 OK）

```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "isRealNameVerified": true,
  "realNameVerifyTime": "2026-05-12T10:00:00Z",
  "message": "实名认证成功"
}
```

#### 错误响应（400 Bad Request）

```json
{
  "error": {
    "code": "Bilibili:RealNameVerificationFailed",
    "message": "实名认证失败",
    "details": "身份证号格式不正确"
  }
}
```

---

### 8.2 UP主认证

#### 基本信息

```
POST /api/bilibili/users/me/verify/up
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| verificationType | string | ✅ | 认证类型（Individual/Organization/Media） |
| description | string | ✅ | 认证说明 |
| materials | array | ❌ | 认证材料（图片URL列表） |

#### 请求示例

```json
{
  "verificationType": "Individual",
  "description": "我是个人UP主，主要制作游戏视频",
  "materials": [
    "materials/user_001_doc1.jpg",
    "materials/user_001_doc2.jpg"
  ]
}
```

#### 成功响应（200 OK）

```json
{
  "userId": "00000000-0000-0000-0000-000000000001",
  "upVerificationStatus": "Pending",
  "upVerifyTime": "2026-05-12T10:00:00Z",
  "message": "UP主认证申请已提交，等待审核"
}
```

---

## 9. 消息接口

### 9.1 获取消息列表

#### 基本信息

```
GET /api/bilibili/users/me/messages?messageType={type}&isRead={boolean}&maxResultCount={int}&skipCount={int}
Authorization: Bearer {token}
权限：需认证
```

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| messageType | string | ❌ | 消息类型（System/Private/Reply/At/Notice） |
| isRead | boolean | ❌ | 是否已读 |
| maxResultCount | int | ❌ | 返回数量（默认20） |
| skipCount | int | ❌ | 跳过数量（默认0） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "id": "00000000-0000-0000-0000-000000000001",
      "receiverId": "00000000-0000-0000-0000-000000000001",
      "senderId": "00000000-0000-0000-0000-000000000002",
      "senderName": "用户B",
      "senderAvatar": "avatars/user_002.jpg",
      "messageType": "Private",
      "title": null,
      "content": "你好，我想问一下你的视频是怎么制作的？",
      "relatedType": null,
      "relatedId": null,
      "isRead": false,
      "readTime": null,
      "creationTime": "2026-05-12T09:00:00Z"
    },
    {
      "id": "00000000-0000-0000-0000-000000000002",
      "receiverId": "00000000-0000-0000-0000-000000000001",
      "senderId": null,
      "messageType": "System",
      "title": "系统通知",
      "content": "恭喜您升级到LV3！",
      "isRead": true,
      "readTime": "2026-05-12T08:00:00Z",
      "creationTime": "2026-05-12T07:00:00Z"
    }
  ],
  "totalCount": 100,
  "unreadCount": 20
}
```

---

### 9.2 发送私信

#### 基本信息

```
POST /api/bilibili/users/me/messages
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| receiverId | UUID | ✅ | 接收者ID |
| content | string | ✅ | 消息内容（最大1000字符） |

#### 请求示例

```json
{
  "receiverId": "00000000-0000-0000-0000-000000000002",
  "content": "你好，很喜欢你的视频！"
}
```

#### 成功响应（200 OK）

```json
{
  "id": "00000000-0000-0000-0000-000000000003",
  "receiverId": "00000000-0000-0000-0000-000000000002",
  "senderId": "00000000-0000-0000-0000-000000000001",
  "messageType": "Private",
  "content": "你好，很喜欢你的视频！",
  "isRead": false,
  "creationTime": "2026-05-12T10:00:00Z"
}
```

---

## 10. 关注接口

### 10.1 关注用户

#### 基本信息

```
POST /api/bilibili/users/{id}/follow
Authorization: Bearer {token}
Content-Type: application/json
权限：需认证
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| id | UUID | ✅ | 要关注的用户ID |

#### 请求参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| isFollow | boolean | ✅ | 是否关注（true=关注，false=取消关注） |

#### 请求示例

```json
{
  "isFollow": true
}
```

#### 成功响应（200 OK）

```json
{
  "followerId": "00000000-0000-0000-0000-000000000001",
  "followingId": "00000000-0000-0000-0000-000000000002",
  "isFollowing": true,
  "followTime": "2026-05-12T10:00:00Z",
  "followingCount": 201,
  "targetUserFollowerCount": 1001
}
```

---

### 10.2 获取关注列表

#### 基本信息

```
GET /api/bilibili/users/{id}/following?maxResultCount={int}&skipCount={int}
权限：公开
```

#### 路径参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| id | UUID | ✅ | 用户ID |

#### 查询参数

| 参数名 | 类型 | 必填 | 说明 |
|--------|------|------|------|
| maxResultCount | int | ❌ | 返回数量（默认20） |
| skipCount | int | ❌ | 跳过数量（默认0） |

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "userId": "00000000-0000-0000-0000-000000000002",
      "userName": "UP主A",
      "userAvatar": "avatars/user_002.jpg",
      "userLevel": 5,
      "userSignature": "游戏UP主",
      "videoCount": 100,
      "followerCount": 5000,
      "followTime": "2026-05-01T00:00:00Z"
    }
  ],
  "totalCount": 200
}
```

---

### 10.3 获取粉丝列表

#### 基本信息

```
GET /api/bilibili/users/{id}/followers?maxResultCount={int}&skipCount={int}
权限：公开
```

#### 成功响应（200 OK）

```json
{
  "items": [
    {
      "userId": "00000000-0000-0000-0000-000000000003",
      "userName": "粉丝A",
      "userAvatar": "avatars/user_003.jpg",
      "userLevel": 2,
      "followTime": "2026-05-10T00:00:00Z"
    }
  ],
  "totalCount": 1000
}
```

---

## 11. 权限定义

### 11.1 用户模块权限

| 权限名称 | 说明 |
|---------|------|
| BilibiliUsers.View | 查看用户信息（公开） |
| BilibiliUsers.Update | 更新自己的信息（需认证） |
| BilibiliUsers.Verify | 实名认证、UP主认证（需认证） |
| BilibiliUsers.Message | 发送私信（需认证） |
| BilibiliUsers.Follow | 关注用户（需认证） |

---

## 12. 错误码汇总

| 错误码 | HTTP状态码 | 说明 |
|--------|-----------|------|
| Bilibili:UserNotFound | 404 | 用户不存在 |
| Bilibili:UserBanned | 403 | 用户已封禁 |
| Bilibili:RealNameAlreadyVerified | 400 | 已实名认证 |
| Bilibili:RealNameVerificationFailed | 400 | 实名认证失败 |
| Bilibili:UPVerificationPending | 400 | UP主认证申请待审核 |
| Bilibili:InsufficientCoins | 400 | B币余额不足 |
| Bilibili:DailyExperienceLimitReached | 400 | 今日经验已达上限 |
| Bilibili:CannotFollowSelf | 400 | 不能关注自己 |
| Bilibili:AlreadyFollowing | 400 | 已关注该用户 |
| Bilibili:NotFollowing | 400 | 未关注该用户 |

---

## 13. 接口调用示例

### 13.1 JavaScript/TypeScript调用示例

```typescript
// 用户登录示例
async function login(userName: string, password: string) {
  const response = await fetch('/api/identity/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      userNameOrEmail: userName,
      password: password
    })
  });
  
  if (response.ok) {
    const data = await response.json();
    // 保存Token到本地存储
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    return data;
  } else {
    const error = await response.json();
    throw new Error(error.error.message);
  }
}

// 获取用户信息示例
async function getUserInfo(userId: string) {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch(`/api/bilibili/users/${userId}`, {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
}

// 关注用户示例
async function followUser(userId: string) {
  const token = localStorage.getItem('accessToken');
  
  const response = await fetch(`/api/bilibili/users/${userId}/follow`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      isFollow: true
    })
  });
  
  return await response.json();
}
```

---

## 14. 接口依赖关系

### 14.1 依赖其他服务

| 服务 | 依赖接口 | 用途 |
|------|---------|------|
| IdentityServer | /api/identity/* | 用户认证、JWT Token |
| VideoService | /api/video/videos | 获取用户视频列表 |
| InteractionService | /api/interaction/* | 获取互动统计数据 |

---

## 15. 接口性能指标

| 接口 | 平均响应时间 | QPS上限 | 说明 |
|------|------------|---------|------|
| 用户登录 | <100ms | 1000 | 高频接口 |
| Token刷新 | <50ms | 5000 | 高频接口 |
| 获取用户信息 | <50ms | 5000 | 高频接口，需缓存 |
| 更新用户信息 | <100ms | 500 | 低频接口 |
| 获取等级信息 | <50ms | 1000 | 中频接口 |
| 关注用户 | <100ms | 1000 | 中频接口 |

---

## 16. 接口版本历史

| 版本 | 更新时间 | 更新内容 |
|------|---------|---------|
| v1.0 | 2026-05-12 | 初始版本，17个接口 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 用户模块API文档完成