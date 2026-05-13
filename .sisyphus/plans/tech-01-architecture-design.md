# 技术架构设计文档（结合现有项目）

## 1. 文档信息

| 项目 | 内容 |
|------|------|
| 文档类型 | 技术架构设计 |
| 目标项目 | Bilibili视频平台 |
| 基础架构 | LCH.Abp (ABP Framework v10.0.2 + .NET 10.0) |
| 创建时间 | 2026-05-12 |

---

## 2. 现有项目架构分析

### 2.1 基础架构

基于对现有项目的探索，当前项目已经具备以下基础设施：

#### 2.1.1 微服务架构组织

现有服务列表（位于 `aspnet-core/services/`）：

| 服务名称 | 功能 | 状态 |
|---------|------|------|
| PlatformManagement.HttpApi.Host | 平台管理服务 | ✅ 已存在 |
| IdentityServer.HttpApi.Host | 身份认证服务 | ✅ 已存在 |
| AuthServer.HttpApi.Host | 授权服务 | ✅ 已存在 |
| BackendAdmin.HttpApi.Host | 后台管理服务 | ✅ 已存在 |
| RealtimeMessage.HttpApi.Host | 实时消息服务 | ✅ 已存在（SignalR） |
| LocalizationManagement.HttpApi.Host | 本地化管理 | ✅ 已存在 |
| WebhooksManagement.HttpApi.Host | Webhook管理 | ✅ 已存在 |
| TaskManagement.HttpApi.Host | 任务管理 | ✅ 已存在 |
| WorkflowManagement.HttpApi.Host | 工作流管理 | ✅ 已存在 |
| WechatManagement.HttpApi.Host | 微信管理 | ✅ 已存在 |

**结论**: 项目已具备完整的微服务架构基础，我们只需扩展视频相关的服务。

---

#### 2.1.2 技术栈确认

| 技术组件 | 当前版本 | 说明 |
|---------|---------|------|
| **.NET Framework** | .NET 10.0 | 已确认（所有csproj都是net10.0） |
| **ABP Framework** | v10.0.2 | 已集成（Volo.Abp包版本） |
| **数据库** | PostgreSQL | 已配置（连接字符串配置） |
| **缓存** | Redis | 已集成（用于缓存、SignalR、数据保护） |
| **消息队列** | RabbitMQ | 已集成（通过CAP框架） |
| **搜索** | Elasticsearch | 已集成（审计日志存储） |
| **对象存储** | MinIO/Aliyun OSS | 已实现（OssManagement模块） |
| **实时通信** | SignalR | 已实现（RealtimeMessage服务） |

---

### 2.2 Bilibili视频平台架构扩展

基于现有架构，我们需要新增以下服务：

#### 2.2.1 新增服务列表

| 新增服务 | 功能 | 依赖现有模块 | 开发优先级 |
|---------|------|------------|-----------|
| **VideoService.HttpApi.Host** | 视频管理服务 | Identity、OssManagement | P0 |
| **DanmakuService.HttpApi.Host** | 弹幕服务 | RealtimeMessage、Redis | P0 |
| **TranscodeService.Worker** | 转码Worker | RabbitMQ、OssManagement | P0 |
| **SearchService.HttpApi.Host** | 搜索服务 | Elasticsearch | P1 |
| **RecommendService.HttpApi.Host** | 推荐服务 | Redis、UserBehavior数据 | P1 |
| **LiveService.HttpApi.Host** | 直播服务 | OssManagement、SignalR | P1 |
| **InteractionService.HttpApi.Host** | 互动服务 | Identity、Redis | P0 |

---

## 3. 系统架构设计

### 3.1 总体架构图

```
┌─────────────────────────────────────────────────────────────────┐
│                         客户端层                                │
│    Web端(Vue 3)  │  移动端(App)  │  管理后台(Vue Admin)          │
└─────────────────────────────┬───────────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────────┐
│                      API Gateway层                              │
│                   Ocelot / YARP (已存在)                        │
│  ┌──────────────┬──────────────┬──────────────┬──────────────┐ │
│  │   路由转发   │   认证鉴权   │   限流熔断   │   负载均衡   │ │
│  └──────────────┴──────────────┴──────────────┴──────────────┘ │
└─────────────────────────────┬───────────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────────┐
│                      微服务层                                   │
│                                                                 │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              现有服务（继承使用）                            │ │
│  ├──────────────┬──────────────┬──────────────┬────────────────┤ │
│  │ Identity     │ AuthServer   │ PlatformMgmt │ BackendAdmin  │ │
│  │ (认证)       │ (授权)       │ (平台)       │ (后台)        │ │
│  ├──────────────┼──────────────┼──────────────┼────────────────┤ │
│  │ RealtimeMsg  │ OssMgmt      │ Notification │ Localization  │ │
│  │ (SignalR)    │ (对象存储)   │ (通知)       │ (多语言)      │ │
│  └──────────────┴──────────────┴──────────────┴────────────────┘ │
│                                                                 │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │              新增服务（Bilibili专用）                        │ │
│  ├──────────────┬──────────────┬──────────────┬────────────────┤ │
│  │ VideoService │ DanmakuSvc   │ TranscodeWk  │ SearchService  │ │
│  │ (视频管理)   │ (弹幕推送)   │ (转码Worker) │ (搜索)        │ │
│  ├──────────────┼──────────────┼──────────────┼────────────────┤ │
│  │ RecommendSvc │ LiveService  │ Interaction  │ ContentAudit   │ │
│  │ (推荐)       │ (直播)       │ (互动)       │ (内容审核)    │ │
│  └──────────────┴──────────────┴──────────────┴────────────────┘ │
└─────────────────────────────┬───────────────────────────────────┘
                              │
┌─────────────────────────────▼───────────────────────────────────┐
│                    基础设施层                                   │
│                                                                 │
│  ┌──────────────┬──────────────┬──────────────┬────────────────┤ │
│  │ PostgreSQL   │ Redis        │ RabbitMQ     │ Elasticsearch  │ │
│  │ (主数据库)   │ (缓存/计数)  │ (消息队列)   │ (搜索引擎)    │ │
│  ├──────────────┼──────────────┼──────────────┼────────────────┤ │
│  │ MinIO        │ SignalR      │ CAP          │ FFmpeg         │ │
│  │ (对象存储)   │ (WebSocket)  │ (事件总线)   │ (转码引擎)    │ │
│  └──────────────┴──────────────┴──────────────┴────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

### 3.2 服务通信架构

#### 3.2.1 同步通信（HTTP/gRPC）

```
场景：API调用、查询操作

客户端 → Gateway → [HTTP/gRPC] → 微服务

示例：
- 用户登录：Gateway → IdentityService (HTTP)
- 视频查询：Gateway → VideoService (HTTP)
- 搜索请求：Gateway → SearchService (HTTP)
```

**配置方案**（基于现有Gateway）:

```csharp
// Ocelot配置（现有）
// 在 app-config.json 中添加新服务路由

{
  "Routes": [
    // 视频服务路由
    {
      "DownstreamPathTemplate": "/api/video/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "video-service",
          "Port": 5001
        }
      ],
      "UpstreamPathTemplate": "/api/video/{everything}",
      "UpstreamHttpMethod": ["Get", "Post", "Put", "Delete"]
    },
    
    // 弹幕服务路由（WebSocket）
    {
      "DownstreamPathTemplate": "/ws/danmaku",
      "DownstreamScheme": "ws",
      "DownstreamHostAndPorts": [
        {
          "Host": "danmaku-service",
          "Port": 5002
        }
      ],
      "UpstreamPathTemplate": "/ws/danmaku"
    },
    
    // 搜索服务路由
    {
      "DownstreamPathTemplate": "/api/search/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "search-service",
          "Port": 5003
        }
      ],
      "UpstreamPathTemplate": "/api/search/{everything}"
    }
  ]
}
```

---

#### 3.2.2 异步通信（RabbitMQ/CAP）

```
场景：事件驱动、解耦通信

生产者 → [RabbitMQ] → 消费者

示例：
- 视频上传完成 → VideoService → TranscodeQueue → TranscodeWorker
- 弹幕发送 → DanmakuService → NotificationQueue → NotificationService
- 用户行为 → InteractionService → RecommendQueue → RecommendService
```

**CAP配置**（基于现有）:

```csharp
// 现有配置：Directory.Packages.props 已有 DotNetCoreCAPPackageVersion
// 扩展配置：在新服务中使用CAP

public class VideoServiceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // 配置CAP（继承现有配置）
        context.Services.AddCap(x =>
        {
            x.UseRabbitMQ(options =>
            {
                options.HostName = "rabbitmq";
                options.UserName = "admin";
                options.Password = "password";
            });
            
            x.UsePostgreSQL<VideoDbContext>();
            
            // 添加视频转码队列
            x.PublishOptions.DefaultGroupName = "video.transcode";
        });
    }
}
```

---

#### 3.2.3 实时通信（SignalR）

```
场景：弹幕推送、直播互动

客户端 → [WebSocket] → SignalR Hub → 广播到房间

示例：
- 弹幕实时推送：DanmakuHub → 所有观众
- 直播互动：LiveHub → 所有观众
```

**继承现有实现**（RealtimeMessage模块）:

```csharp
// 现有：RealtimeMessage.HttpApi.Host 已有SignalR实现
// 扩展：新建DanmakuHub继承现有Hub架构

// 新增弹幕Hub（继承现有SignalR架构）
public class DanmakuHub : AbpHub
{
    // 使用现有的连接管理
    // 使用现有的认证机制
    // 扩展弹幕特定逻辑
}

// 配置SignalR（继承现有）
public class DanmakuServiceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSignalR()
            .AddStackExchangeRedis(options =>
            {
                // 使用现有Redis配置
                options.Configuration.ConnectionString = redisConnectionString;
            });
    }
    
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<DanmakuHub>("/hubs/danmaku");
        });
    }
}
```

---

## 4. 数据存储架构

### 4.1 PostgreSQL数据库设计

#### 4.1.1 数据库分库策略

基于现有多数据库架构：

| 数据库名 | 用途 | 服务归属 |
|---------|------|---------|
| PlatformDB | 平台数据（现有） | PlatformManagement |
| IdentityDB | 用户数据（现有） | IdentityServer |
| VideoDB | 视频数据（新增） | VideoService |
| DanmakuDB | 弹幕数据（新增） | DanmakuService |
| InteractionDB | 互动数据（新增） | InteractionService |
| SearchDB | 搜索索引（新增） | SearchService |

**连接字符串配置**（继承现有模式）:

```json
{
  "ConnectionStrings": {
    "Default": "Host=postgres;Database=PlatformDB;...",
    "Identity": "Host=postgres;Database=IdentityDB;...",
    "Video": "Host=postgres;Database=VideoDB;...",
    "Danmaku": "Host=postgres;Database=DanmakuDB;...",
    "Interaction": "Host=postgres;Database=InteractionDB;..."
  }
}
```

---

### 4.2 Redis缓存架构

#### 4.2.1 Redis Key设计规范

基于现有Redis使用模式：

```
命名规范: {module}:{resource}:{id}:{field}

示例：
用户缓存: user:info:{userId}
视频缓存: video:detail:{videoId}
弹幕缓存: danmaku:list:{videoId}:{start}:{end}
计数器:   video:count:{videoId}
限流:     rate:limit:{userId}:{action}
经验值:   exp:daily:{userId}:{date}
```

#### 4.2.2 缓存策略

| 数据类型 | 缓存策略 | TTL | 说明 |
|---------|---------|-----|------|
| 用户信息 | Cache-Aside | 10分钟 | 高频读取 |
| 视频信息 | Cache-Aside | 10分钟 | 高频读取 |
| 视频计数 | Write-Through | 永久 | 实时更新 |
| 弹幕列表 | Cache-Aside | 5分钟 | 时间段查询 |
| 推荐结果 | Cache-Aside | 1小时 | 算法计算结果 |
| 搜索建议 | Cache-Aside | 1小时 | 搜索联想 |

---

### 4.3 对象存储架构

#### 4.3.1 MinIO配置（继承现有OssManagement）

基于现有MinIO配置：

```csharp
// 现有：OssManagement模块已实现
// 扩展：VideoService使用现有MinIO配置

public class VideoStorageService
{
    private readonly IBlobContainer _videoContainer;
    
    public VideoStorageService(IBlobContainerProvider containerProvider)
    {
        // 使用现有BlobContainer配置
        _videoContainer = containerProvider.GetContainer("videos");
    }
    
    public async Task<string> SaveVideoChunk(string uploadId, int chunkIndex, Stream chunk)
    {
        var path = $"temp/{uploadId}/{chunkIndex}";
        await _videoContainer.SaveAsync(path, chunk);
        return path;
    }
}
```

#### 4.3.2 文件组织结构

```
MinIO Bucket: bilibili-videos

目录结构:
├── temp/                       # 临时文件（上传中）
│   └── {uploadId}/            # 上传会话临时目录
│       ├── 0                  # 分片文件
│       ├── 1
│       └── ...
│
├── videos/                     # 正式视频文件
│   └── {videoId}/             # 视频目录
│       ├── original.mp4       # 原始文件
│       ├── 1080p.m3u8         # HLS主文件
│       ├── 1080p_000.ts       # HLS分片
│       ├── 720p.m3u8
│       ├── 480p.m3u8
│       └── master.m3u8        # 多码率索引
│
├── covers/                     # 视频封面
│   └── {videoId}/
│       ├── cover_1.jpg
│       ├── cover_2.jpg
│       └── cover_3.jpg
│
└── danmaku/                    # 弹幕备份
    └── {videoId}/
        └── {date}.json        # 按日期备份
```

---

## 5. 关键技术实现

### 5.1 视频转码架构

#### 5.1.1 转码Worker设计

```
┌─────────────────────────────────────────────────────────────┐
│                   转码任务流程                               │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  VideoService (生产者)                                      │
│       │                                                     │
│       ↓ 发布任务                                            │
│  ┌─────────────┐                                            │
│  │ RabbitMQ    │                                            │
│  │ transcode_q │                                            │
│  └─────────────┐                                            │
│       │                                                     │
│       ↓ 消费任务                                            │
│  ┌─────────────┐                                            │
│  │ Transcode   │  Worker Pool (3个并发)                     │
│  │ Worker 1    │                                            │
│  └─────────────┘                                            │
│       │                                                     │
│       ↓ FFmpeg转码                                          │
│  ┌─────────────┐                                            │
│  │ FFmpeg      │  CPU转码（360P/480P/720P/1080P）           │
│  │ Process     │                                            │
│  └─────────────┘                                            │
│       │                                                     │
│       ↓ 上传HLS                                             │
│  ┌─────────────┐                                            │
│  │ MinIO       │  存储M3U8和TS文件                           │
│  └─────────────┘                                            │
│       │                                                     │
│       ↓ 更新状态                                            │
│  VideoService (状态更新)                                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**实现代码**（基于现有BackgroundService）:

```csharp
// 新增：TranscodeWorkerService (继承ABP BackgroundService)
public class TranscodeWorkerService : AbpBackgroundService
{
    private readonly IRabbitMqMessageConsumer _consumer;
    private readonly FFmpegService _ffmpegService;
    private readonly IBlobContainer _blobContainer;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 使用现有RabbitMQ连接
        await _consumer.SubscribeAsync<TranscodeTask>(
            queueName: "transcode_tasks",
            handler: async task =>
            {
                await ProcessTranscodeAsync(task);
            });
    }
    
    private async Task ProcessTranscodeAsync(TranscodeTask task)
    {
        try
        {
            // 1. FFProbe分析
            var mediaInfo = await FFProbe.AnalyseAsync(task.FilePath);
            
            // 2. 多清晰度转码（CPU）
            var resolutions = GetResolutions(mediaInfo);
            
            foreach (var res in resolutions)
            {
                await _ffmpegService.TranscodeAsync(
                    task.FilePath,
                    res.Name,
                    res.Width,
                    res.Height);
            }
            
            // 3. 生成HLS Master
            await GenerateMasterPlaylist(task.VideoId);
            
            // 4. 上传到MinIO（使用现有BlobContainer）
            await UploadToStorage(task.VideoId);
            
            // 5. 更新视频状态
            await UpdateVideoStatus(task.VideoId, VideoStatus.Published);
        }
        catch (Exception ex)
        {
            await UpdateVideoStatus(task.VideoId, VideoStatus.TranscodeFailed);
        }
    }
}
```

---

### 5.2 弹幕系统架构

#### 5.2.1 WebSocket推送架构

```
┌─────────────────────────────────────────────────────────────┐
│                    弹幕推送流程                              │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  客户端 WebSocket连接                                       │
│       │                                                     │
│       ↓                                                     │
│  ┌─────────────┐                                            │
│  │ SignalR     │  DanmakuHub                                │
│  │ Gateway     │                                            │
│  └─────────────┘                                            │
│       │                                                     │
│       ↓ 连接认证                                            │
│  ┌─────────────┐                                            │
│  │ Identity    │  JWT Token验证                             │
│  │ Service     │                                            │
│  └─────────────┘                                            │
│       │                                                     │
│       ↓ 加入房间                                            │
│  ┌─────────────┐                                            │
│  │ Redis       │  用户连接映射                               │
│  │ Group       │  video:{videoId}:connections               │
│  └─────────────┘                                            │
│       │                                                     │
│       ↓ 发送弹幕                                            │
│  ┌─────────────┐                                            │
│  │ PostgreSQL  │  持久化存储                                 │
│  │ DanmakuDB   │  分区表                                     │
│  └─────────────┘                                            │
│       │                                                     │
│       ↓ 广播                                                │
│  ┌─────────────┐                                            │
│  │ SignalR     │  Clients.Group广播                         │
│  │ Redis Bcker│  Scaleout (跨服务器广播)                     │
│  └─────────────┘                                            │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

**实现配置**（继承现有SignalR）:

```csharp
// 配置SignalR Redis Scaleout（继承现有配置）
public class DanmakuServiceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        // 使用现有Redis配置
        context.Services.AddSignalR()
            .AddStackExchangeRedis(configuration["Redis:Configuration"], 
                options =>
                {
                    options.ChannelPrefix = "danmaku:";
                });
    }
    
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        
        app.UseEndpoints(endpoints =>
        {
            // 注册弹幕Hub
            endpoints.MapHub<DanmakuHub>("/hubs/danmaku");
        });
    }
}
```

---

## 6. 认证授权架构

### 6.1 JWT认证（继承现有IdentityServer）

基于现有IdentityServer配置：

```csharp
// 现有：IdentityServer已配置
// 扩展：新服务使用现有JWT认证

public class VideoServiceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        // 继承现有JWT配置
        context.Services.AddAuthentication()
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = false;
                options.Audience = "video-service";
            });
    }
}
```

---

## 7. 监控与运维架构

### 7.1 监控体系（基于现有）

基于现有Docker/K8s配置：

```
┌─────────────────────────────────────────────────────────────┐
│                    监控体系                                  │
├──────────────┬──────────────┬──────────────┬────────────────┤
│ Prometheus   │ Grafana      │ ELK Stack    │ 健康检查       │
│ (指标采集)   │ (可视化)     │ (日志分析)   │ (HealthChecks) │
└──────────────┴──────────────┴──────────────┴────────────────┘
```

**健康检查配置**（继承现有）:

```csharp
// 使用现有HealthChecks配置
public class VideoServiceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHealthChecks()
            .AddNpgSql(connectionString)              // PostgreSQL检查
            .AddRedis(redisConnectionString)          // Redis检查
            .AddRabbitMQ(rabbitMqConnectionString);   // RabbitMQ检查
    }
}
```

---

## 8. 扩展服务创建指南

### 8.1 创建新服务步骤

基于现有模块创建流程：

```
步骤1: 创建服务项目（参考现有服务结构）
├── LCH.MicroService.Video.HttpApi.Host/
│   ├── Controllers/
│   ├── EventBus/
│   ├── Program.cs
│   ├── VideoServiceModule.cs
│   ├── appsettings.json
│   └── Dockerfile
│
步骤2: 创建领域模块（参考现有模块结构）
├── LCH.MicroService.Video.Domain/
│   ├── Entities/
│   ├── Repositories/
│   ├── Services/
│   └── VideoDomainModule.cs
│
步骤3: 创建应用服务层
├── LCH.MicroService.Video.Application/
│   ├── Services/
│   ├── Dtos/
│   └── VideoApplicationModule.cs
│
步骤4: 创建HttpApi层
├── LCH.MicroService.Video.HttpApi/
│   ├── Controllers/
│   └ VideoHttpApiModule.cs
│
步骤5: 创建Contracts层
├── LCH.MicroService.Video.Application.Contracts/
│   ├── Services/
│   ├── Dtos/
│   └ VideoContractsModule.cs
│
步骤6: 创建EntityFrameworkCore层
├── LCH.MicroService.Video.EntityFrameworkCore/
│   ├── DbContexts/
│   ├── Entities/
│   ├── Repositories/
│   └ VideoEntityFrameworkCoreModule.cs
```

---

## 9. 配置文件示例

### 9.1 appsettings.json（新服务）

```json
{
  "ConnectionStrings": {
    "Default": "Host=postgres;Database=VideoDB;Username=postgres;Password=password"
  },
  
  "Redis": {
    "Configuration": "redis:6379,password=password"
  },
  
  "RabbitMQ": {
    "Host": "rabbitmq",
    "UserName": "admin",
    "Password": "password"
  },
  
  "Elasticsearch": {
    "Url": "http://elasticsearch:9200"
  },
  
  "MinIO": {
    "Endpoint": "minio:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "BucketName": "bilibili-videos"
  },
  
  "AuthServer": {
    "Authority": "http://auth-server:5000"
  },
  
  "FFmpeg": {
    "Path": "/usr/bin/ffmpeg",
    "TempDirectory": "/tmp/transcode"
  }
}
```

---

## 10. Docker配置

### 10.1 Dockerfile（新服务）

```dockerfile
# VideoService Dockerfile（继承现有模式）
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LCH.MicroService.Video.HttpApi.Host.dll"]
```

---

## 11. 技术选型确认

| 技术 | 选型 | 原因 | 状态 |
|------|------|------|------|
| 框架 | ABP v10.0.2 + .NET 10.0 | 已存在，成熟稳定 | ✅ 确认 |
| 数据库 | PostgreSQL | 已存在，性能稳定 | ✅ 确认 |
| 缓存 | Redis | 已存在，功能全面 | ✅ 硟认 |
| 消息队列 | RabbitMQ + CAP | 已存在，可靠性强 | ✅ 确认 |
| 搜索 | Elasticsearch | 已存在，成熟方案 | ✅ 确认 |
| 对象存储 | MinIO | 已存在，S3兼容 | ✅ 确认 |
| 实时通信 | SignalR + Redis Scaleout | 已存在，性能好 | ✅ 硟认 |
| 转码 | FFmpeg CPU | 成本低，足够 | ✅ 确认 |
| CDN | 无（仅MinIO本地） | 确认需求，成本低 | ✅ 硟认 |

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**状态**: ✅ 技术架构设计完成