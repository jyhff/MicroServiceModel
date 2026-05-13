# 用户系统产品需求文档 (PRD)

## 1. 模块概述

### 1.1 模块定位
用户系统是视频平台的核心基础模块，负责用户身份管理、权限控制、用户行为追踪和用户成长体系。

### 1.2 核心功能
- 用户注册与登录（多种方式）
- 用户等级与经验值系统
- 消息中心（通知、私信）
- 个人主页与用户信息管理
- 关注与粉丝关系
- 用户权限与角色管理

---

## 2. 功能详细设计

### 2.1 用户注册与登录

#### 2.1.1 注册方式
| 注册方式 | 优先级 | 说明 |
|---------|--------|------|
| 手机号注册 | P0 | 国内主流方式，短信验证码 |
| 邮箱注册 | P1 | 国际用户友好 |
| 第三方注册 | P1 | 微信、QQ、微博、GitHub OAuth |

#### 2.1.2 登录方式
- **账号密码登录**：支持手机号/邮箱 + 密码
- **短信验证码登录**：快捷登录，无密码
- **扫码登录**：手机端扫码Web端登录
- **第三方登录**：OAuth2.0集成
- **自动登录**：Token刷新机制，7天/30天免登录

#### 2.1.3 安全机制
```csharp
// 登录安全策略配置
public class LoginSecurityOptions
{
    // 密码策略
    public int MinPasswordLength { get; set; } = 8;
    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = false;
    public bool RequireSpecialChar { get; set; } = false;
    
    // 登录限制
    public int MaxFailedAttempts { get; set; } = 5;      // 最大失败次数
    public int LockoutDurationMinutes { get; set; } = 30; // 锁定时间
    
    // 多因素认证
    public bool EnableTwoFactor { get; set; } = false;
    public TwoFactorMethod TwoFactorMethod { get; set; } = TwoFactorMethod.SMS;
}
```

### 2.2 用户等级系统

#### 2.2.1 等级规则
| 等级 | 所需经验值 | 特权说明 |
|------|-----------|---------|
| LV0 | 0 | 基础功能，每日投币上限5 |
| LV1 | 100 | 可发送弹幕 |
| LV2 | 500 | 解锁高级弹幕（彩色） |
| LV3 | 1500 | 可上传视频，每日投币上限10 |
| LV4 | 4500 | 解锁专属头像框，每日投币上限20 |
| LV5 | 10800 | 可创建频道，每日投币上限50 |
| LV6 | 28800 | 解锁全部功能，每日投币上限无限制 |

#### 2.2.2 经验值获取
| 行为 | 经验值 | 每日上限 |
|------|--------|---------|
| 每日登录 | 5 | 1次 |
| 观看视频 | 1/分钟 | 20 |
| 发送弹幕 | 2 | 50 |
| 发表评论 | 3 | 30 |
| 视频被播放 | 0.1/次 | 无上限 |
| 获得点赞 | 1 | 无上限 |
| 获得投币 | 10 | 无上限 |
| 获得收藏 | 5 | 无上限 |
| 关注UP主 | 2 | 20 |

### 2.3 消息中心

#### 2.3.1 消息类型
1. **系统通知**：平台公告、账号安全、等级提升
2. **互动消息**：评论回复、点赞通知、关注通知
3. **私信消息**：用户间私信
4. **UP主消息**：视频审核结果、收益结算
5. **直播消息**：关注主播开播提醒

#### 2.3.2 推送渠道
- **站内信**：消息中心红点提示
- **WebSocket**：实时推送
- **邮件推送**：重要通知
- **短信推送**：安全相关
- **App推送**：移动端离线推送

### 2.4 个人主页

#### 2.4.1 页面结构
```
┌─────────────────────────────────────────────────────────────┐
│  [背景图]                                                  │
│  ┌────────┐                                                 │
│  │ 头像   │  昵称 [认证标识]                                 │
│  └────────┘  签名                                           │
│              等级 LV6 │ 粉丝 1.2万 │ 关注 233 │ 获赞 58万    │
├─────────────────────────────────────────────────────────────┤
│ 主页 │ 投稿 │ 收藏 │ 追番 │ 设置 │                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  [内容区域]                                                  │
│                                                             │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐      │
│  │   视频卡片    │ │   视频卡片    │ │   视频卡片    │ ...  │
│  └──────────────┘ └──────────────┘ └──────────────┘      │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

#### 2.4.2 个人设置
- **基础信息**：头像、昵称、签名、性别、生日
- **隐私设置**：动态可见性、关注列表可见、私信权限
- **安全设置**：修改密码、绑定手机/邮箱、登录设备管理
- **通知设置**：各类消息开关
- **黑名单**：屏蔽用户列表

### 2.5 关注系统

#### 2.5.1 关注类型
- **普通关注**：接收UP主动态
- **特别关注**：动态优先展示
- **分组关注**：自定义分组（游戏区、生活区等）

#### 2.5.2 关系数据
```csharp
public class UserFollow
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }           // 关注者
    public Guid TargetUserId { get; set; }     // 被关注者
    public FollowType Type { get; set; }       // 关注类型
    public string GroupName { get; set; }      // 分组名称
    public DateTime FollowTime { get; set; }   // 关注时间
    public bool IsMutual { get; set; }         // 是否互相关注
}
```

---

## 3. 数据库设计

### 3.1 实体关系图
```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   AppUser    │─────│ UserFollow   │─────│   AppUser    │
└──────────────┘     └──────────────┘     └──────────────┘
         │                                           
         │                    ┌──────────────┐
         └────────────────────│  UserLevel   │
                              └──────────────┘
                                       │
                              ┌──────────────┐
                              │ UserExpLog   │
                              └──────────────┘
```

### 3.2 核心表结构

#### Users表
```sql
CREATE TABLE "Users" (
    "Id" uuid PRIMARY KEY,
    "UserName" varchar(256) NOT NULL,
    "NormalizedUserName" varchar(256),
    "Email" varchar(256),
    "NormalizedEmail" varchar(256),
    "EmailConfirmed" boolean DEFAULT false,
    "PasswordHash" varchar(256),
    "PhoneNumber" varchar(20),
    "PhoneNumberConfirmed" boolean DEFAULT false,
    "TwoFactorEnabled" boolean DEFAULT false,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean DEFAULT true,
    "AccessFailedCount" integer DEFAULT 0,
    "Avatar" varchar(500),
    "Signature" varchar(500),
    "Level" integer DEFAULT 0,
    "Experience" integer DEFAULT 0,
    "IsVerified" boolean DEFAULT false,
    "VerificationType" integer,
    "Gender" integer,
    "BirthDate" date,
    "FollowerCount" integer DEFAULT 0,
    "FollowingCount" integer DEFAULT 0,
    "CreationTime" timestamp with time zone NOT NULL,
    "LastModificationTime" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false
);
```

#### UserExperienceLogs表
```sql
CREATE TABLE "UserExperienceLogs" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "ActionType" integer NOT NULL,  -- 经验值获取行为类型
    "ExperienceGained" integer NOT NULL,
    "OldExperience" integer NOT NULL,
    "NewExperience" integer NOT NULL,
    "OldLevel" integer NOT NULL,
    "NewLevel" integer NOT NULL,
    "Description" varchar(500),
    "RelatedId" varchar(100),       -- 关联对象ID（视频ID、评论ID等）
    "CreationTime" timestamp with time zone NOT NULL
);
CREATE INDEX "IX_UserExperienceLogs_UserId" ON "UserExperienceLogs"("UserId");
CREATE INDEX "IX_UserExperienceLogs_CreationTime" ON "UserExperienceLogs"("CreationTime" DESC);
```

#### UserFollows表
```sql
CREATE TABLE "UserFollows" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "TargetUserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "FollowType" integer DEFAULT 0,
    "GroupName" varchar(100),
    "IsMutual" boolean DEFAULT false,
    "FollowTime" timestamp with time zone NOT NULL,
    "UnfollowTime" timestamp with time zone,
    "IsDeleted" boolean DEFAULT false
);
CREATE UNIQUE INDEX "IX_UserFollows_UserId_TargetUserId" ON "UserFollows"("UserId", "TargetUserId") WHERE "IsDeleted" = false;
CREATE INDEX "IX_UserFollows_TargetUserId" ON "UserFollows"("TargetUserId") WHERE "IsDeleted" = false;
```

---

## 4. API设计

### 4.1 认证相关API

#### 注册
```http
POST /api/user/register
Content-Type: application/json

{
  "username": "string",
  "password": "string",
  "phoneNumber": "string",
  "verifyCode": "string"
}
```

#### 登录
```http
POST /api/user/login
Content-Type: application/json

{
  "loginType": "phone|email|password|thirdParty",
  "account": "string",
  "password": "string",
  "verifyCode": "string",
  "thirdPartyToken": "string"
}

Response:
{
  "token": "jwt_token",
  "refreshToken": "refresh_token",
  "expiresIn": 7200,
  "user": {
    "id": "uuid",
    "username": "string",
    "avatar": "url",
    "level": 6,
    "isVerified": true
  }
}
```

### 4.2 用户管理API

#### 获取用户信息
```http
GET /api/user/{userId}
Authorization: Bearer {token}

Response:
{
  "id": "uuid",
  "username": "string",
  "avatar": "url",
  "signature": "string",
  "level": 6,
  "experience": 28800,
  "nextLevelExp": 0,
  "followerCount": 12000,
  "followingCount": 233,
  "likeCount": 580000,
  "isFollowing": false,
  "isVerified": true,
  "verificationType": "personal|enterprise",
  "createdAt": "2024-01-01"
}
```

#### 更新用户信息
```http
PUT /api/user/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "username": "string",
  "signature": "string",
  "avatar": "url",
  "gender": 0,
  "birthDate": "1995-01-01"
}
```

### 4.3 等级经验API

#### 获取经验记录
```http
GET /api/user/experience-logs?page=1&pageSize=20
Authorization: Bearer {token}

Response:
{
  "items": [
    {
      "id": "uuid",
      "actionType": "watchVideo",
      "actionName": "观看视频",
      "experienceGained": 5,
      "oldLevel": 5,
      "newLevel": 6,
      "description": "观看视频《xxx》获得经验",
      "createdAt": "2024-01-01 12:00:00"
    }
  ],
  "totalCount": 100
}
```

### 4.4 关注API

#### 关注用户
```http
POST /api/user/follow
Authorization: Bearer {token}
Content-Type: application/json

{
  "targetUserId": "uuid",
  "followType": "normal|special",
  "groupName": "默认分组"
}
```

#### 获取关注列表
```http
GET /api/user/followings?userId={userId}&page=1&pageSize=20
Authorization: Bearer {token}
```

#### 获取粉丝列表
```http
GET /api/user/followers?userId={userId}&page=1&pageSize=20
Authorization: Bearer {token}
```

---

## 5. 技术实现

### 5.1 技术栈
- **框架**: ABP Framework v10.0.2 + .NET 10.0
- **身份认证**: IdentityServer4 / OpenIddict
- **数据存储**: PostgreSQL + Redis
- **消息队列**: RabbitMQ (经验值异步计算)
- **缓存**: Redis分布式缓存

### 5.2 核心服务类

```csharp
// 用户服务接口
public interface IUserService
{
    Task<UserDto> GetUserAsync(Guid userId);
    Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileInput input);
    Task FollowUserAsync(Guid userId, FollowUserInput input);
    Task UnfollowUserAsync(Guid userId, Guid targetUserId);
    Task<PagedResultDto<UserFollowDto>> GetFollowingsAsync(Guid userId, GetFollowingsInput input);
    Task<PagedResultDto<UserFollowerDto>> GetFollowersAsync(Guid userId, GetFollowersInput input);
}

// 经验值服务
public interface IExperienceService
{
    Task AddExperienceAsync(Guid userId, ExperienceActionType actionType, int? relatedId = null);
    Task<int> GetUserLevelAsync(Guid userId);
    Task<int> GetUserExperienceAsync(Guid userId);
    Task<LevelInfoDto> GetLevelInfoAsync(Guid userId);
}

// 经验值计算
public class ExperienceCalculator
{
    public int CalculateExperience(ExperienceActionType actionType, ExperienceContext context)
    {
        var baseExp = GetBaseExperience(actionType);
        var dailyLimit = GetDailyLimit(actionType);
        
        // 检查今日是否已达上限
        if (context.TodayCount >= dailyLimit)
            return 0;
            
        return baseExp;
    }
}
```

### 5.3 经验值异步处理

```csharp
// 经验值事件
public class ExperienceGainedEvent
{
    public Guid UserId { get; set; }
    public ExperienceActionType ActionType { get; set; }
    public string? RelatedId { get; set; }
    public DateTime Timestamp { get; set; }
}

// 事件处理器
public class ExperienceGainedEventHandler : ILocalEventHandler<ExperienceGainedEvent>
{
    public async Task HandleEventAsync(ExperienceGainedEvent eventData)
    {
        // 1. 计算应得经验
        var exp = _experienceCalculator.Calculate(eventData);
        
        // 2. 更新用户经验
        await _userRepository.IncrementExperience(eventData.UserId, exp);
        
        // 3. 检查是否升级
        var user = await _userRepository.GetAsync(eventData.UserId);
        var newLevel = CalculateLevel(user.Experience);
        
        if (newLevel > user.Level)
        {
            await _userRepository.UpdateLevel(eventData.UserId, newLevel);
            
            // 发送升级通知
            await _notificationService.SendLevelUpNotification(eventData.UserId, newLevel);
        }
        
        // 4. 记录经验日志
        await _expLogRepository.InsertAsync(new UserExperienceLog
        {
            UserId = eventData.UserId,
            ActionType = eventData.ActionType,
            ExperienceGained = exp,
            // ...
        });
    }
}
```

---

## 6. 性能优化

### 6.1 缓存策略
| 数据类型 | 缓存方式 | 过期时间 | 说明 |
|---------|---------|---------|------|
| 用户信息 | Redis | 10分钟 | 频繁读取 |
| 关注关系 | Redis | 5分钟 | 关注状态 |
| 粉丝数/关注数 | Redis | 1分钟 | 统计计数 |
| 等级配置 | MemoryCache | 永久 | 配置数据 |

### 6.2 数据库优化
- **索引优化**: 手机号、邮箱、用户名索引
- **读写分离**: 读多写少场景
- **分库分表**: 用户ID哈希分片（亿级用户时）

### 6.3 限流策略
- **注册频率**: IP限制，每小时5次
- **登录频率**: 账号限制，每分钟5次
- **关注频率**: 用户限制，每分钟20次

---

## 7. 安全合规

### 7.1 数据保护
- 密码使用BCrypt/PBKDF2加密存储
- 手机号、邮箱脱敏展示
- GDPR合规：支持数据导出、删除

### 7.2 防护措施
- 登录IP异常检测
- 异地登录提醒
- 账号冻结机制

---

## 8. 附录

### 8.1 错误码
| 错误码 | 描述 |
|--------|------|
| 10001 | 用户名已存在 |
| 10002 | 手机号已注册 |
| 10003 | 邮箱已注册 |
| 10004 | 用户名或密码错误 |
| 10005 | 账号已被锁定 |
| 10006 | 验证码错误 |
| 10007 | 已关注该用户 |

### 8.2 相关文档
- [认证授权API文档](./Auth-API.md)
- [数据库设计文档](./Database-Design.md)

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0 + PostgreSQL
