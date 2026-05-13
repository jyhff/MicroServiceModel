# 01-用户系统详细PRD

## 1. 模块概述

### 1.1 业务定位
用户系统是平台的身份认证和用户成长体系核心，负责用户注册登录、等级成长、认证、关注关系等核心功能。

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 第三方登录 | 先支持账号密码，预留微信登录接口 |
| 等级上限 | LV6为最高等级 |
| 经验上限 | 每日上限100经验值（防刷） |
| 认证条件 | 仅实名认证，无粉丝要求 |

---

## 2. 功能详细设计

### 2.1 用户注册

#### 2.1.1 注册方式

**方式一：手机号注册**

| 字段 | 验证规则 | 说明 |
|------|---------|------|
| 手机号 | 11位数字，格式验证 | 国内用户 |
| 验证码 | 6位数字，5分钟有效 | 短信验证 |
| 密码 | 8-20位，至少含数字和字母 | BCrypt加密 |
| 昵称 | 2-20字符，唯一性检查 | 首次设置 |

**流程**:
```
1. 用户输入手机号
2. 发送短信验证码（调用第三方短信服务）
3. 用户输入验证码
4. 验证码校验（Redis存储，5分钟过期）
5. 设置密码和昵称
6. 创建用户记录（数据库）
7. 返回JWT Token
```

**API接口**:

```http
POST /api/v1/user/register/phone
Content-Type: application/json

Request Body:
{
  "phone": "13800138000",      // 手机号
  "verifyCode": "123456",      // 验证码
  "password": "Password123",   // 密码
  "nickname": "用户昵称"        // 昵称
}

Response:
{
  "code": 0,
  "message": "注册成功",
  "data": {
    "userId": "uuid",
    "token": {
      "accessToken": "jwt_token",
      "refreshToken": "refresh_token",
      "expiresIn": 7200
    },
    "user": {
      "id": "uuid",
      "phone": "138****8000",  // 脱敏
      "nickname": "用户昵称",
      "avatar": null,
      "level": 0,
      "experience": 0,
      "createdAt": "2024-06-01T10:00:00Z"
    }
  }
}

Error Response:
{
  "code": 10001,
  "message": "手机号已注册"
}
```

**错误码定义**:

| 错误码 | 说明 |
|--------|------|
| 10001 | 手机号已注册 |
| 10002 | 验证码错误或已过期 |
| 10003 | 密码格式不正确 |
| 10004 | 昵称已被使用 |
| 10005 | 手机号格式不正确 |

---

**方式二：邮箱注册**

| 字段 | 验证规则 | 说明 |
|------|---------|------|
| 邮箱 | 有效邮箱格式 | 国际用户 |
| 验证码 | 6位数字，10分钟有效 | 邮件验证 |
| 密码 | 8-20位，至少含数字和字母 | BCrypt加密 |
| 昵称 | 2-20字符，唯一性检查 | 首次设置 |

**API接口**:

```http
POST /api/v1/user/register/email
Content-Type: application/json

Request Body:
{
  "email": "user@example.com",
  "verifyCode": "123456",
  "password": "Password123",
  "nickname": "用户昵称"
}

Response: (同手机号注册)
```

---

**方式三：微信登录（预留接口）**

> 注意：当前不实现，仅预留接口和数据表结构

**预留API接口**:

```http
POST /api/v1/user/oauth/wechat
Content-Type: application/json

Request Body:
{
  "code": "wechat_auth_code",  // 微信授权码
  "state": "random_state"      // CSRF防护
}

Response:
{
  "code": 0,
  "message": "登录成功",
  "data": {
    "token": {...},
    "user": {...}
  }
}
```

---

#### 2.1.2 验证码发送

**短信验证码API**:

```http
POST /api/v1/user/sms/send
Content-Type: application/json

Request Body:
{
  "phone": "13800138000",
  "type": "register"  // register/login/reset
}

Response:
{
  "code": 0,
  "message": "验证码已发送",
  "data": {
    "expireIn": 300  // 5分钟有效
  }
}

Error Response:
{
  "code": 10006,
  "message": "发送过于频繁，请1分钟后重试"
}
```

**限流规则**:
- 同一手机号：每分钟最多1次
- 同一IP：每小时最多10次

**存储设计**:
```
Redis Key: sms:verify:{phone}
Value: {
  "code": "123456",
  "createdAt": 1704100800,
  "type": "register"
}
TTL: 300秒（5分钟）
```

---

**邮件验证码API**:

```http
POST /api/v1/user/email/send
Content-Type: application/json

Request Body:
{
  "email": "user@example.com",
  "type": "register"
}

Response: (同短信)
```

---

### 2.2 用户登录

#### 2.2.1 账号密码登录

**API接口**:

```http
POST /api/v1/user/login
Content-Type: application/json

Request Body:
{
  "account": "13800138000",  // 手机号或邮箱
  "password": "Password123",
  "deviceType": "web"       // web/mobile/app
}

Response:
{
  "code": 0,
  "message": "登录成功",
  "data": {
    "token": {
      "accessToken": "jwt_token",
      "refreshToken": "refresh_token",
      "expiresIn": 7200
    },
    "user": {
      "id": "uuid",
      "phone": "138****8000",
      "email": "u***@example.com",
      "nickname": "用户昵称",
      "avatar": "https://...",
      "level": 3,
      "experience": 1500,
      "followerCount": 120,
      "followingCount": 58,
      "isVerified": false,
      "createdAt": "2024-06-01T10:00:00Z"
    }
  }
}

Error Response:
{
  "code": 10007,
  "message": "账号或密码错误"
}
```

**登录安全策略**:

| 规则 | 配置 |
|------|------|
| 失败次数限制 | 5次失败后锁定30分钟 |
| 锁定机制 | Redis记录失败次数 |
| 设备管理 | 记录登录设备，最多5个设备 |
| 异地登录提醒 | IP变化时发送通知 |

**锁定Redis存储**:
```
Key: login:lock:{account}
Value: {
  "failCount": 5,
  "lockUntil": 1704101100
}
TTL: 1800秒（30分钟）
```

---

#### 2.2.2 短信验证码登录

**API接口**:

```http
POST /api/v1/user/login/sms
Content-Type: application/json

Request Body:
{
  "phone": "13800138000",
  "verifyCode": "123456"
}

Response: (同账号密码登录)
```

---

#### 2.2.3 Token刷新

**API接口**:

```http
POST /api/v1/user/token/refresh
Content-Type: application/json

Request Body:
{
  "refreshToken": "refresh_token_string"
}

Response:
{
  "code": 0,
  "message": "刷新成功",
  "data": {
    "accessToken": "new_jwt_token",
    "refreshToken": "new_refresh_token",
    "expiresIn": 7200
  }
}

Error Response:
{
  "code": 10008,
  "message": "Refresh Token无效或已过期"
}
```

---

#### 2.2.4 登出

**API接口**:

```http
POST /api/v1/user/logout
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "message": "登出成功"
}
```

---

### 2.3 用户等级系统

#### 2.3.1 等级规则表

| 等级 | 所需经验值 | 特权说明 | 备注 |
|------|-----------|---------|------|
| LV0 | 0 | 基础功能，每日投币上限5 | 新注册用户 |
| LV1 | 100 | 可发送弹幕 | 最低活跃门槛 |
| LV2 | 500 | 可使用高级弹幕（彩色） | 彈幕特权 |
| LV3 | 1500 | 可上传视频，每日投币上限10 | UP主门槛 |
| LV4 | 4500 | 解锁专属头像框，每日投币上限20 | 中级用户 |
| LV5 | 10800 | 可创建频道，每日投币上限50 | 高级用户 |
| LV6 | 28800 | 全部功能，每日投币无限制 | 最高等级 |

---

#### 2.3.2 经验值获取规则

**每日上限**: 100经验值（防刷机制）

| 行为 | 经验值 | 每日上限 | 触发时机 |
|------|--------|---------|---------|
| 每日登录 | 5 | 5（仅1次） | 首次登录当天 |
| 观看视频 | 1/分钟 | 20 | 每分钟累计 |
| 发送弹幕 | 2 | 50 | 每次发送 |
| 发表评论 | 3 | 30 | 每次发表 |
| 视频被播放 | 0.1/次 | 无上限 | 视频播放时 |
| 获得点赞 | 1 | 无上限 | 其他用户点赞 |
| 获得投币 | 10 | 无上限 | 其他用户投币 |
| 获得收藏 | 5 | 无上限 | 其他用户收藏 |
| 关注UP主 | 2 | 20 | 每次关注 |

---

**API接口设计**:

```http
GET /api/v1/user/level/info
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "level": 3,
    "experience": 1500,
    "nextLevelExp": 4500,
    "progress": 33.3,        // 百分比
    "dailyExp": 45,          // 今日已获得
    "dailyExpLimit": 100,    // 今日上限
    "privileges": [
      "可发送弹幕",
      "可使用高级弹幕",
      "可上传视频",
      "每日投币上限10"
    ]
  }
}
```

---

**经验值记录API**:

```http
GET /api/v1/user/experience/logs?page=1&pageSize=20
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "items": [
      {
        "id": "uuid",
        "actionType": "watchVideo",
        "actionName": "观看视频",
        "experienceGained": 1,
        "relatedVideoId": "uuid",
        "createdAt": "2024-06-01T10:00:00Z"
      }
    ],
    "totalCount": 150
  }
}
```

---

### 2.4 用户认证

#### 2.4.1 实名认证流程

**需求**: 仅实名认证，无粉丝数要求

**认证类型**:
- 个人实名认证
- 企业实名认证（可选）

**流程**:
```
1. 用户上传身份证照片（正反面）
2. 用户填写真实姓名、身份证号
3. 调用第三方实名认证API（阿里云/腾讯云）
4. 验证成功，更新认证状态
5. 发送认证成功通知
```

**API接口**:

```http
POST /api/v1/user/verify/realname
Authorization: Bearer {accessToken}
Content-Type: multipart/form-data

Request:
FormData:
- realName: "真实姓名"
- idCard: "身份证号"
- idCardFront: [身份证正面照片]
- idCardBack: [身份证背面照片]

Response:
{
  "code": 0,
  "message": "认证申请已提交",
  "data": {
    "verifyId": "uuid",
    "status": "pending",      // pending/success/failed
    "estimatedTime": "1-3工作日"
  }
}
```

---

**认证状态查询API**:

```http
GET /api/v1/user/verify/status
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "isVerified": true,
    "verifyType": "personal",
    "realName": "张**",
    "verifyTime": "2024-06-01T10:00:00Z"
  }
}
```

---

### 2.5 用户信息管理

#### 2.5.1 个人信息查询

**API接口**:

```http
GET /api/v1/user/{userId}
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "id": "uuid",
    "nickname": "用户昵称",
    "avatar": "https://...",
    "signature": "个性签名",
    "level": 3,
    "experience": 1500,
    "followerCount": 120,
    "followingCount": 58,
    "likeCount": 5000,
    "isFollowing": false,
    "isVerified": true,
    "createdAt": "2024-06-01T10:00:00Z"
  }
}
```

---

#### 2.5.2 个人信息修改

**API接口**:

```http
PUT /api/v1/user/profile
Authorization: Bearer {accessToken}
Content-Type: application/json

Request Body:
{
  "nickname": "新昵称",
  "signature": "新签名",
  "avatar": "https://...",   // 新头像URL
  "gender": 1,              // 0=未知,1=男,2=女
  "birthday": "1990-01-01"
}

Response:
{
  "code": 0,
  "message": "修改成功"
}
```

---

### 2.6 关注系统

#### 2.6.1 关注UP主

**API接口**:

```http
POST /api/v1/user/follow
Authorization: Bearer {accessToken}
Content-Type: application/json

Request Body:
{
  "targetUserId": "uuid",
  "followType": "normal"    // normal=普通,special=特别关注
}

Response:
{
  "code": 0,
  "message": "关注成功",
  "data": {
    "isFollowing": true,
    "isMutual": false,      // 是否互关
    "followerCount": 121    // UP主粉丝数+1
  }
}
```

---

#### 2.6.2 取消关注

**API接口**:

```http
DELETE /api/v1/user/follow/{targetUserId}
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "message": "取消关注成功",
  "data": {
    "isFollowing": false,
    "followerCount": 120
  }
}
```

---

#### 2.6.3 关注列表

**API接口**:

```http
GET /api/v1/user/followings?page=1&pageSize=20
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "items": [
      {
        "userId": "uuid",
        "nickname": "UP主昵称",
        "avatar": "https://...",
        "signature": "签名",
        "level": 6,
        "followerCount": 10000,
        "isMutual": true,
        "followTime": "2024-06-01T10:00:00Z"
      }
    ],
    "totalCount": 58
  }
}
```

---

#### 2.6.4 粉丝列表

**API接口**:

```http
GET /api/v1/user/followers?page=1&pageSize=20
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "items": [
      {
        "userId": "uuid",
        "nickname": "粉丝昵称",
        "avatar": "https://...",
        "level": 2,
        "isFollowing": false,
        "followTime": "2024-06-01T10:00:00Z"
      }
    ],
    "totalCount": 120
  }
}
```

---

## 3. 数据库设计

### 3.1 核心表结构

#### Users表（用户主表）

```sql
CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserName" varchar(256) NOT NULL,
    "NormalizedUserName" varchar(256),
    "Email" varchar(256),
    "NormalizedEmail" varchar(256),
    "EmailConfirmed" boolean DEFAULT false,
    "PasswordHash" varchar(256) NOT NULL,
    "PhoneNumber" varchar(20),
    "PhoneNumberConfirmed" boolean DEFAULT false,
    "TwoFactorEnabled" boolean DEFAULT false,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean DEFAULT true,
    "AccessFailedCount" integer DEFAULT 0,
    "SecurityStamp" varchar(256),
    "ConcurrencyStamp" varchar(256),
    
    -- 自定义字段
    "Nickname" varchar(50) NOT NULL,
    "Avatar" varchar(500),
    "Signature" varchar(500),
    "Gender" integer DEFAULT 0,
    "Birthday" date,
    "Level" integer DEFAULT 0 CHECK ("Level" >= 0 AND "Level" <= 6),
    "Experience" integer DEFAULT 0,
    "FollowerCount" integer DEFAULT 0,
    "FollowingCount" integer DEFAULT 0,
    "LikeCount" bigint DEFAULT 0,
    "CoinCount" bigint DEFAULT 0,
    "IsVerified" boolean DEFAULT false,
    "VerifyType" integer,
    "VerifyTime" timestamp with time zone,
    "RealName" varchar(100),
    "IdCard" varchar(100),
    "IsDeleted" boolean DEFAULT false,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW(),
    "LastModificationTime" timestamp with time zone,
    "DeleterId" uuid,
    "DeletionTime" timestamp with time zone
);

CREATE UNIQUE INDEX "IX_Users_UserName" ON "Users"("NormalizedUserName") 
WHERE "IsDeleted" = false;
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users"("NormalizedEmail") 
WHERE "IsDeleted" = false AND "Email" IS NOT NULL;
CREATE UNIQUE INDEX "IX_Users_PhoneNumber" ON "Users"("PhoneNumber") 
WHERE "IsDeleted" = false AND "PhoneNumber" IS NOT NULL;
CREATE INDEX "IX_Users_Level" ON "Users"("Level");
CREATE INDEX "IX_Users_CreationTime" ON "Users"("CreationTime" DESC);
```

---

#### UserThirdPartyAccounts表（第三方账号关联，预留）

```sql
CREATE TABLE "UserThirdPartyAccounts" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Provider" varchar(50) NOT NULL,      // wechat/github/google
    "ProviderUserId" varchar(256) NOT NULL,
    "AccessToken" varchar(500),
    "RefreshToken" varchar(500),
    "ExpiresAt" timestamp with time zone,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW(),
    
    UNIQUE("Provider", "ProviderUserId")
);

CREATE INDEX "IX_UserThirdPartyAccounts_UserId" ON "UserThirdPartyAccounts"("UserId");
```

---

#### UserExperienceLogs表（经验值记录）

```sql
CREATE TABLE "UserExperienceLogs" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "ActionType" integer NOT NULL,           -- 1=登录,2=观看,3=弹幕,4=评论...
    "ActionName" varchar(100) NOT NULL,
    "ExperienceGained" integer NOT NULL,
    "OldExperience" integer NOT NULL,
    "NewExperience" integer NOT NULL,
    "OldLevel" integer NOT NULL,
    "NewLevel" integer NOT NULL,
    "RelatedId" uuid,                        -- 关联对象ID（视频ID等）
    "Description" varchar(500),
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW()
) PARTITION BY RANGE ("CreationTime");

-- 按月份分区（经验值记录量大）
CREATE TABLE "UserExperienceLogs_2024_06" PARTITION OF "UserExperienceLogs"
FOR VALUES FROM ('2024-06-01') TO ('2024-07-01');

CREATE TABLE "UserExperienceLogs_2024_07" PARTITION OF "UserExperienceLogs"
FOR VALUES FROM ('2024-07-01') TO ('2024-08-01');

CREATE INDEX "IX_UserExperienceLogs_UserId" ON "UserExperienceLogs"("UserId");
CREATE INDEX "IX_UserExperienceLogs_CreationTime" ON "UserExperienceLogs"("CreationTime" DESC);
```

---

#### UserDailyExperience表（每日经验统计）

```sql
CREATE TABLE "UserDailyExperience" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Date" date NOT NULL,
    "TotalExperience" integer DEFAULT 0,
    "LoginExperience" integer DEFAULT 0,
    "WatchExperience" integer DEFAULT 0,
    "DanmakuExperience" integer DEFAULT 0,
    "CommentExperience" integer DEFAULT 0,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW(),
    
    UNIQUE("UserId", "Date")
);

CREATE INDEX "IX_UserDailyExperience_UserId_Date" ON "UserDailyExperience"("UserId", "Date" DESC);
```

---

#### UserFollows表（关注关系）

```sql
CREATE TABLE "UserFollows" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "TargetUserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "FollowType" integer DEFAULT 0,          -- 0=普通,1=特别关注
    "IsMutual" boolean DEFAULT false,
    "FollowTime" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UnfollowTime" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false,
    
    UNIQUE("UserId", "TargetUserId") WHERE "IsDeleted" = false
);

CREATE INDEX "IX_UserFollows_UserId" ON "UserFollows"("UserId") WHERE "IsDeleted" = false;
CREATE INDEX "IX_UserFollows_TargetUserId" ON "UserFollows"("TargetUserId") WHERE "IsDeleted" = false;
```

---

#### RefreshTokens表（刷新Token）

```sql
CREATE TABLE "RefreshTokens" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Token" varchar(500) NOT NULL,
    "JwtId" varchar(100),                    -- 关联的JWT ID
    "IsRevoked" boolean DEFAULT false,
    "RevokedReason" varchar(200),
    "ExpiresAt" timestamp with time zone NOT NULL,
    "DeviceType" varchar(50),
    "DeviceId" varchar(100),
    "IP" varchar(50),
    "UserAgent" varchar(500),
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW()
);

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens"("UserId");
CREATE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens"("Token");
CREATE INDEX "IX_RefreshTokens_ExpiresAt" ON "RefreshTokens"("ExpiresAt");
```

---

#### UserVerifyRequests表（认证申请）

```sql
CREATE TABLE "UserVerifyRequests" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "VerifyType" integer NOT NULL,           -- 1=个人,2=企业
    "RealName" varchar(100) NOT NULL,
    "IdCard" varchar(100) NOT NULL,
    "IdCardFrontUrl" varchar(500) NOT NULL,
    "IdCardBackUrl" varchar(500) NOT NULL,
    "Status" integer DEFAULT 0,              -- 0=待审核,1=通过,2=拒绝
    "RejectReason" varchar(500),
    "ReviewerId" uuid,
    "ReviewTime" timestamp with time zone,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW()
);

CREATE INDEX "IX_UserVerifyRequests_UserId" ON "UserVerifyRequests"("UserId");
CREATE INDEX "IX_UserVerifyRequests_Status" ON "UserVerifyRequests"("Status");
```

---

## 4. 技术实现代码示例

### 4.1 用户注册服务

```csharp
public class UserRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IVerifyCodeService _verifyCodeService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDistributedCache _cache;
    
    public async Task<RegisterResult> RegisterByPhoneAsync(RegisterByPhoneInput input)
    {
        // 1. 验证手机号格式
        if (!IsValidPhone(input.Phone))
            throw new BusinessException("10005", "手机号格式不正确");
        
        // 2. 检查手机号是否已注册
        var existingUser = await _userRepository.FindByPhoneAsync(input.Phone);
        if (existingUser != null)
            throw new BusinessException("10001", "手机号已注册");
        
        // 3. 验证验证码
        var verifyCodeValid = await _verifyCodeService.VerifyAsync(
            input.Phone, 
            input.VerifyCode, 
            VerifyCodeType.Register);
        if (!verifyCodeValid)
            throw new BusinessException("10002", "验证码错误或已过期");
        
        // 4. 验证密码格式
        if (!IsValidPassword(input.Password))
            throw new BusinessException("10003", "密码格式不正确");
        
        // 5. 检查昵称唯一性
        var existingNickname = await _userRepository.FindByNicknameAsync(input.Nickname);
        if (existingNickname != null)
            throw new BusinessException("10004", "昵称已被使用");
        
        // 6. 创建用户
        var user = new User
        {
            Id = Guid.NewGuid(),
            PhoneNumber = input.Phone,
            PhoneNumberConfirmed = true,
            Nickname = input.Nickname,
            Level = 0,
            Experience = 0,
            PasswordHash = _passwordHasher.HashPassword(null, input.Password),
            CreationTime = DateTime.UtcNow
        };
        
        await _userRepository.InsertAsync(user);
        
        // 7. 初始化每日经验记录
        await InitializeDailyExperience(user.Id);
        
        // 8. 生成JWT Token
        var token = await _jwtTokenService.GenerateTokenAsync(user);
        
        // 9. 删除验证码
        await _verifyCodeService.DeleteAsync(input.Phone);
        
        return new RegisterResult
        {
            UserId = user.Id,
            Token = token,
            User = MapToDto(user)
        };
    }
    
    private bool IsValidPhone(string phone)
    {
        return Regex.IsMatch(phone, @"^1[3-9]\d{9}$");
    }
    
    private bool IsValidPassword(string password)
    {
        // 8-20位，至少含数字和字母
        return password.Length >= 8 && password.Length <= 20 &&
               password.Any(char.IsDigit) && password.Any(char.IsLetter);
    }
    
    private async Task InitializeDailyExperience(Guid userId)
    {
        var dailyExp = new UserDailyExperience
        {
            UserId = userId,
            Date = DateTime.UtcNow.Date,
            TotalExperience = 0
        };
        
        await _dailyExpRepository.InsertAsync(dailyExp);
    }
}
```

---

### 4.2 经验值服务

```csharp
public class ExperienceService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserDailyExperienceRepository _dailyExpRepository;
    private readonly IUserExperienceLogRepository _expLogRepository;
    private readonly IDistributedCache _cache;
    
    // 经验值获取配置
    private readonly Dictionary<ExperienceAction, int> _experienceConfig = new()
    {
        [ExperienceAction.Login] = 5,
        [ExperienceAction.WatchVideo] = 1,        // 每分钟
        [ExperienceAction.SendDanmaku] = 2,
        [ExperienceAction.SendComment] = 3,
        [ExperienceAction.FollowUser] = 2
    };
    
    private readonly Dictionary<ExperienceAction, int> _dailyLimits = new()
    {
        [ExperienceAction.Login] = 5,
        [ExperienceAction.WatchVideo] = 20,
        [ExperienceAction.SendDanmaku] = 50,
        [ExperienceAction.SendComment] = 30,
        [ExperienceAction.FollowUser] = 20
    };
    
    public async Task<AddExperienceResult> AddExperienceAsync(
        Guid userId, 
        ExperienceAction action,
        Guid? relatedId = null,
        string description = null)
    {
        // 1. 获取用户
        var user = await _userRepository.GetAsync(userId);
        
        // 2. 检查今日经验是否已达上限（100）
        var todayExp = await GetTodayExperience(userId);
        if (todayExp.TotalExperience >= 100)
            return AddExperienceResult.LimitReached();
        
        // 3. 检查该行为今日是否已达上限
        var actionExpKey = GetActionExpKey(action);
        var actionExp = todayExp.GetType().GetProperty(actionExpKey)?.GetValue(todayExp) as int ?? 0;
        var actionLimit = _dailyLimits[action];
        
        if (actionExp >= actionLimit)
            return AddExperienceResult.ActionLimitReached(action);
        
        // 4. 计算应得经验值
        var expGained = _experienceConfig[action];
        
        // 观看视频时，按分钟计算
        if (action == ExperienceAction.WatchVideo)
        {
            expGained = 1; // 每分钟1经验
        }
        
        // 5. 更新用户经验值
        var oldExp = user.Experience;
        var oldLevel = user.Level;
        user.Experience += expGained;
        
        // 6. 检查是否升级
        var newLevel = CalculateLevel(user.Experience);
        if (newLevel > user.Level)
        {
            user.Level = newLevel;
            // TODO: 发送升级通知
        }
        
        await _userRepository.UpdateAsync(user);
        
        // 7. 更新今日经验统计
        await UpdateDailyExperience(userId, action, expGained);
        
        // 8. 记录经验日志
        var expLog = new UserExperienceLog
        {
            UserId = userId,
            ActionType = (int)action,
            ActionName = action.ToString(),
            ExperienceGained = expGained,
            OldExperience = oldExp,
            NewExperience = user.Experience,
            OldLevel = oldLevel,
            NewLevel = user.Level,
            RelatedId = relatedId,
            Description = description,
            CreationTime = DateTime.UtcNow
        };
        
        await _expLogRepository.InsertAsync(expLog);
        
        return AddExperienceResult.Success(expGained, user.Level, newLevel > oldLevel);
    }
    
    private int CalculateLevel(int experience)
    {
        // 等级经验表
        var levelExp = new[] { 0, 100, 500, 1500, 4500, 10800, 28800 };
        
        for (int i = levelExp.Length - 1; i >= 0; i--)
        {
            if (experience >= levelExp[i])
                return i;
        }
        
        return 0;
    }
    
    private async Task<UserDailyExperience> GetTodayExperience(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var dailyExp = await _dailyExpRepository.FindByUserIdAndDateAsync(userId, today);
        
        if (dailyExp == null)
        {
            dailyExp = new UserDailyExperience
            {
                UserId = userId,
                Date = today,
                TotalExperience = 0
            };
            await _dailyExpRepository.InsertAsync(dailyExp);
        }
        
        return dailyExp;
    }
    
    private async Task UpdateDailyExperience(Guid userId, ExperienceAction action, int expGained)
    {
        var todayExp = await GetTodayExperience(userId);
        
        todayExp.TotalExperience += expGained;
        
        // 更新对应行为经验
        var actionExpKey = GetActionExpKey(action);
        var property = todayExp.GetType().GetProperty(actionExpKey);
        if (property != null)
        {
            var currentValue = (int)property.GetValue(todayExp);
            property.SetValue(todayExp, currentValue + expGained);
        }
        
        await _dailyExpRepository.UpdateAsync(todayExp);
    }
    
    private string GetActionExpKey(ExperienceAction action)
    {
        return action switch
        {
            ExperienceAction.Login => "LoginExperience",
            ExperienceAction.WatchVideo => "WatchExperience",
            ExperienceAction.SendDanmaku => "DanmakuExperience",
            ExperienceAction.SendComment => "CommentExperience",
            _ => "TotalExperience"
        };
    }
}

public enum ExperienceAction
{
    Login = 1,
    WatchVideo = 2,
    SendDanmaku = 3,
    SendComment = 4,
    FollowUser = 5,
    ReceiveLike = 6,
    ReceiveCoin = 7,
    ReceiveCollect = 8
}
```

---

## 5. UI界面设计要点

### 5.1 注册页面

```
┌─────────────────────────────────────────────┐
│            Bilibili - 注册                   │
├─────────────────────────────────────────────┤
│                                             │
│   手机号注册     邮箱注册                     │
│   ─────────    ─────────                    │
│                                             │
│   手机号                                      │
│   ┌─────────────────────────┐              │
│   │ 13800138000           │ [获取验证码] │
│   └─────────────────────────┘              │
│                                             │
│   验证码                                      │
│   ┌─────────────────────────┐              │
│   │ 123456                 │              │
│   └─────────────────────────┘              │
│                                             │
│   密码                                        │
│   ┌─────────────────────────┐              │
│   │ Password123            │              │
│   └─────────────────────────┘              │
│   (密码需8-20位，包含数字和字母)              │
│                                             │
│   昵称                                        │
│   ┌─────────────────────────┐              │
│   │ 用户昵称               │              │
│   └─────────────────────────┘              │
│                                             │
│   [同意用户协议和隐私政策]                     │
│                                             │
│   ┌─────────────────────────┐              │
│   │      注册               │              │
│   └─────────────────────────┘              │
│                                             │
│   已有账号？立即登录                          │
└─────────────────────────────────────────────┘
```

---

## 6. 错误码完整定义

| 错误码 | 说明 | HTTP状态码 |
|--------|------|-----------|
| 10001 | 手机号已注册 | 400 |
| 10002 | 验证码错误或已过期 | 400 |
| 10003 | 密码格式不正确 | 400 |
| 10004 | 昵称已被使用 | 400 |
| 10005 | 手机号格式不正确 | 400 |
| 10006 | 发送验证码过于频繁 | 429 |
| 10007 | 账号或密码错误 | 401 |
| 10008 | Refresh Token无效 | 401 |
| 10009 | 账号已被锁定 | 403 |
| 10010 | Token已过期 | 401 |
| 10011 | 用户不存在 | 404 |
| 10012 | 无权限访问 | 403 |
| 10013 | 今日经验已达上限 | 400 |

---

**文档版本**: v2.0  
**创建日期**: 2026-05-12  
**状态**: ✅ 详细设计完成