# 视频系统产品需求文档 (PRD)

## 1. 模块概述

### 1.1 模块定位
视频系统是Bilibili平台的核心模块，负责视频的完整生命周期管理：从上传、转码、存储到播放分发。

### 1.2 核心功能
- **视频上传**：支持大文件、断点续传、分片上传
- **视频转码**：多清晰度自动转码（360P-4K）
- **视频存储**：对象存储（MinIO/Aliyun OSS）
- **视频播放**：多码率自适应、HLS/DASH协议
- **封面管理**：自动截取、自定义上传
- **视频管理**：UP主后台管理、数据统计

---

## 2. 功能详细设计

### 2.1 视频上传

#### 2.1.1 上传流程
```
┌─────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  用户    │─────│ 分片上传    │─────│ 合并文件    │─────│ 转码任务    │
│ 选择文件 │     │ (Web/APP)   │     │  生成M3U8   │     │  入队列     │
└─────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                                                               │
                                                               ▼
                                                        ┌─────────────┐
                                                        │ 消息队列    │
                                                        │ (RabbitMQ)  │
                                                        └─────────────┘
```

#### 2.1.2 分片上传
| 参数 | 值 | 说明 |
|------|-----|------|
| 分片大小 | 5MB | 每个分片5MB，可根据网络调整 |
| 并发数 | 3 | 同时上传3个分片 |
| 重试次数 | 3 | 失败自动重试 |
| 超时时间 | 60s | 单分片上传超时 |

**API设计**:
```http
// 1. 初始化上传
POST /api/video/upload/init
Authorization: Bearer {token}
{
  "fileName": "video.mp4",
  "fileSize": 1073741824,  // 1GB
  "fileHash": "md5_hash",   // 完整文件MD5
  "mimeType": "video/mp4"
}

Response:
{
  "uploadId": "uuid",
  "chunkSize": 5242880,     // 5MB
  "totalChunks": 205,       // 总分片数
  "uploadedChunks": [0,1,2] // 已上传分片索引
}

// 2. 上传分片
PUT /api/video/upload/chunk
Authorization: Bearer {token}
Content-Type: multipart/form-data

FormData:
- uploadId: "uuid"
- chunkIndex: 3
- chunkHash: "chunk_md5"
- file: [二进制分片数据]

// 3. 完成上传
POST /api/video/upload/complete
Authorization: Bearer {token}
{
  "uploadId": "uuid",
  "videoInfo": {
    "title": "视频标题",
    "description": "视频描述",
    "tags": ["标签1", "标签2"],
    "categoryId": "uuid",
    "coverType": "auto|custom",
    "coverUrl": "url_if_custom"
  }
}
```

#### 2.1.3 秒传机制
```csharp
// 秒传检测
public class FastUploadService
{
    public async Task<UploadResult> CheckFastUpload(string fileHash, long fileSize)
    {
        // 1. 查询是否存在相同文件
        var existingVideo = await _videoRepository
            .FirstOrDefaultAsync(v => v.FileHash == fileHash && v.FileSize == fileSize);
        
        if (existingVideo != null && existingVideo.Status == VideoStatus.Published)
        {
            // 2. 文件已存在且转码完成，直接引用
            return UploadResult.Success(existingVideo, isFastUpload: true);
        }
        
        // 3. 需要正常上传
        return UploadResult.RequireUpload();
    }
}
```

### 2.2 视频转码

#### 2.2.1 转码规格
| 清晰度 | 分辨率 | 码率 | 编码格式 | 适用场景 |
|--------|--------|------|---------|---------|
| 4K | 3840x2160 | 8000Kbps | H.265/AV1 | 会员专享 |
| 1080P高码率 | 1920x1080 | 6000Kbps | H.264 | 会员专享 |
| 1080P | 1920x1080 | 3000Kbps | H.264 | 默认最高 |
| 720P | 1280x720 | 1500Kbps | H.264 | 平衡画质 |
| 480P | 854x480 | 800Kbps | H.264 | 省流量 |
| 360P | 640x360 | 400Kbps | H.264 | 弱网环境 |
| 240P | 426x240 | 200Kbps | H.264 | 极速省流 |

#### 2.2.2 转码流程
```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ 消息队列    │─────│ FFmpeg转码  │─────│ 生成HLS     │
│ 消费任务    │     │ Worker      │     │ M3U8索引    │
└─────────────┘     └─────────────┘     └─────────────┘
       │                                        │
       ▼                                        ▼
┌─────────────┐                          ┌─────────────┐
│ 转码进度    │                          │ 上传存储    │
│ 实时更新    │                          │ (MinIO/OSS) │
└─────────────┘                          └─────────────┘
```

#### 2.2.3 FFmpeg转码实现
```csharp
// 使用FFMpegCore进行转码
public class VideoTranscoder
{
    public async Task<TranscodeResult> TranscodeAsync(TranscodeJob job)
    {
        var outputPath = $"{job.OutputDir}/{job.Resolution}.m3u8";
        
        // 根据分辨率选择预设
        var preset = GetFFmpegPreset(job.Resolution);
        
        var result = await FFMpegArguments
            .FromFileInput(job.InputPath)
            .OutputToFile(outputPath, false, options => options
                .WithVideoCodec(VideoCodec.LibX264)
                .WithAudioCodec(AudioCodec.Aac)
                .WithVideoBitrate(preset.VideoBitrate)
                .WithAudioBitrate(preset.AudioBitrate)
                .WithVideoFilters(filter => filter
                    .Scale(preset.Width, preset.Height)
                    .WithOption("format", "yuv420p"))
                .WithConstantRateFactor(23)
                .WithCustomArgument($"-hls_time 6 -hls_list_size 0 -f hls")
                .WithFastStart())
            .ProcessAsynchronously();
        
        return new TranscodeResult
        {
            Success = result.ExitCode == 0,
            OutputPath = outputPath,
            Duration = result.Duration
        };
    }
    
    private FFmpegPreset GetFFmpegPreset(string resolution)
    {
        return resolution switch
        {
            "1080p" => new FFmpegPreset(1920, 1080, "3000k", "128k"),
            "720p" => new FFmpegPreset(1280, 720, "1500k", "128k"),
            "480p" => new FFmpegPreset(854, 480, "800k", "96k"),
            "360p" => new FFmpegPreset(640, 360, "400k", "96k"),
            _ => throw new ArgumentException($"Unknown resolution: {resolution}")
        };
    }
}
```

#### 2.2.4 转码任务调度
```csharp
// 使用RabbitMQ进行任务调度
public class TranscodeJobConsumer : IConsumer<TranscodeJob>
{
    public async Task Consume(ConsumeContext<TranscodeJob> context)
    {
        var job = context.Message;
        
        // 1. 更新视频状态为转码中
        await _videoRepository.UpdateStatusAsync(job.VideoId, VideoStatus.Transcoding);
        
        // 2. 依次转码各个清晰度
        var resolutions = new[] { "1080p", "720p", "480p", "360p" };
        
        foreach (var resolution in resolutions)
        {
            // 更新进度
            await _progressService.UpdateProgress(job.VideoId, resolution, 0);
            
            // 执行转码
            var result = await _transcoder.TranscodeAsync(new TranscodeJob
            {
                VideoId = job.VideoId,
                InputPath = job.InputPath,
                Resolution = resolution,
                OutputDir = job.OutputDir
            });
            
            if (!result.Success)
            {
                await _videoRepository.UpdateStatusAsync(job.VideoId, VideoStatus.TranscodeFailed);
                throw new TranscodeException($"Transcode failed for {resolution}");
            }
            
            // 更新进度
            await _progressService.UpdateProgress(job.VideoId, resolution, 100);
        }
        
        // 3. 生成主M3U8（多码率自适应）
        await GenerateMasterM3U8(job.VideoId, job.OutputDir, resolutions);
        
        // 4. 更新视频状态为发布
        await _videoRepository.UpdateStatusAsync(job.VideoId, VideoStatus.Published);
        
        // 5. 发送转码完成通知
        await _notificationService.SendTranscodeCompleteNotification(job.UserId, job.VideoId);
    }
}
```

### 2.3 视频播放

#### 2.3.1 播放协议
**HLS (HTTP Live Streaming)**:
- 主M3U8文件：包含多个子M3U8（不同清晰度）
- 自适应码率：播放器根据网络自动切换
- 支持断点续播：记录播放进度

```m3u8
# 主M3U8 (master.m3u8)
#EXTM3U
#EXT-X-STREAM-INF:BANDWIDTH=400000,RESOLUTION=640x360
360p/360p.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=854x480
480p/480p.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=1500000,RESOLUTION=1280x720
720p/720p.m3u8
#EXT-X-STREAM-INF:BANDWIDTH=3000000,RESOLUTION=1920x1080
1080p/1080p.m3u8
```

#### 2.3.2 播放API
```http
// 获取播放地址
GET /api/video/{videoId}/play
Authorization: Bearer {token}

Response:
{
  "videoId": "uuid",
  "title": "视频标题",
  "duration": 360,        // 秒
  "playUrl": "https://cdn.example.com/videos/uuid/master.m3u8",
  "qualities": [
    {
      "name": "1080P",
      "resolution": "1920x1080",
      "url": ".../1080p.m3u8",
      "bandwidth": 3000000
    },
    {
      "name": "720P",
      "resolution": "1280x720",
      "url": ".../720p.m3u8",
      "bandwidth": 1500000
    },
    {
      "name": "480P",
      "resolution": "854x480",
      "url": ".../480p.m3u8",
      "bandwidth": 800000
    },
    {
      "name": "360P",
      "resolution": "640x360",
      "url": ".../360p.m3u8",
      "bandwidth": 400000
    }
  ],
  "progress": 120,        // 上次观看进度（秒）
  "isFavorite": false,
  "likeCount": 12580,
  "coinCount": 3280
}
```

#### 2.3.3 防盗链机制
```csharp
// 签名URL生成
public class VideoUrlSigner
{
    public string GenerateSignedUrl(string videoId, string baseUrl, TimeSpan expiry)
    {
        var expiryTime = DateTimeOffset.UtcNow.Add(expiry).ToUnixTimeSeconds();
        var data = $"{videoId}:{expiryTime}";
        var signature = HMACSHA256(data, _secretKey);
        
        return $"{baseUrl}?videoId={videoId}&expires={expiryTime}&signature={signature}";
    }
    
    public bool VerifySignature(string videoId, long expires, string signature)
    {
        // 1. 检查是否过期
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expires)
            return false;
        
        // 2. 验证签名
        var data = $"{videoId}:{expires}";
        var expectedSignature = HMACSHA256(data, _secretKey);
        
        return signature == expectedSignature;
    }
}
```

### 2.4 视频封面

#### 2.4.1 封面规格
| 类型 | 尺寸 | 格式 | 大小限制 |
|------|------|------|---------|
| 默认封面 | 640x360 | JPG/PNG | 2MB |
| 高清封面 | 1280x720 | JPG/PNG | 5MB |
| WebP封面 | 640x360 | WebP | 500KB |

#### 2.4.2 自动截取封面
```csharp
// 从视频中截取封面
public async Task<string> ExtractThumbnailAsync(string videoPath, TimeSpan position)
{
    var outputPath = $"/tmp/{Guid.NewGuid()}.jpg";
    
    await FFMpeg.Snapshot(videoPath, outputPath, 
        captureTime: position,
        size: new Size(640, 360));
    
    // 上传到对象存储
    var coverUrl = await _ossService.UploadAsync(outputPath, "covers/");
    
    return coverUrl;
}

// 生成多个候选封面（视频1/4, 2/4, 3/4位置）
public async Task<List<string>> GenerateCandidateThumbnailsAsync(string videoPath, TimeSpan duration)
{
    var positions = new[]
    {
        TimeSpan.FromSeconds(duration.TotalSeconds * 0.25),
        TimeSpan.FromSeconds(duration.TotalSeconds * 0.5),
        TimeSpan.FromSeconds(duration.TotalSeconds * 0.75)
    };
    
    var tasks = positions.Select(p => ExtractThumbnailAsync(videoPath, p));
    var results = await Task.WhenAll(tasks);
    
    return results.ToList();
}
```

### 2.5 UP主视频管理

#### 2.5.1 视频列表
```http
GET /api/video/manage/list?page=1&pageSize=20&status=all
Authorization: Bearer {token}

Response:
{
  "items": [
    {
      "id": "uuid",
      "title": "视频标题",
      "status": "published",    // uploading/transcoding/published/failed
      "cover": "url",
      "duration": 360,
      "viewCount": 12580,
      "likeCount": 3280,
      "coinCount": 1280,
      "collectionCount": 580,
      "commentCount": 280,
      "createdAt": "2024-01-01 12:00:00",
      "transcodeProgress": 100  // 转码进度（%）
    }
  ],
  "totalCount": 156
}
```

#### 2.5.2 编辑视频信息
```http
PUT /api/video/manage/{videoId}
Authorization: Bearer {token}
Content-Type: application/json

{
  "title": "新标题",
  "description": "新描述",
  "tags": ["新标签1", "新标签2"],
  "categoryId": "uuid",
  "cover": "new_cover_url"
}
```

#### 2.5.3 删除视频
```http
DELETE /api/video/manage/{videoId}
Authorization: Bearer {token}

// 软删除，保留数据但标记为删除
```

---

## 3. 数据库设计

### 3.1 视频表
```sql
CREATE TABLE "Videos" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Title" varchar(200) NOT NULL,
    "Description" text,
    "Cover" varchar(500),
    "Duration" integer,              -- 时长（秒）
    "Status" integer DEFAULT 0,      -- 状态：0=上传中,1=转码中,2=已发布,3=失败
    "CategoryId" uuid REFERENCES "Categories"("Id"),
    "Tags" text[],                   -- 标签数组
    "OriginalFilePath" varchar(500), -- 原始文件路径
    "FileHash" varchar(64),          -- 文件MD5哈希
    "FileSize" bigint,               -- 文件大小（字节）
    "PlayUrl" varchar(500),          -- 播放地址
    "TranscodeProgress" integer DEFAULT 0,  -- 转码进度
    "ViewCount" bigint DEFAULT 0,
    "LikeCount" bigint DEFAULT 0,
    "CoinCount" bigint DEFAULT 0,
    "CollectionCount" bigint DEFAULT 0,
    "CommentCount" bigint DEFAULT 0,
    "DanmakuCount" bigint DEFAULT 0,
    "IsDeleted" boolean DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "PublishedAt" timestamp with time zone
);

CREATE INDEX "IX_Videos_UserId" ON "Videos"("UserId") WHERE "IsDeleted" = false;
CREATE INDEX "IX_Videos_CategoryId" ON "Videos"("CategoryId") WHERE "IsDeleted" = false;
CREATE INDEX "IX_Videos_Status" ON "Videos"("Status") WHERE "IsDeleted" = false;
CREATE INDEX "IX_Videos_FileHash" ON "Videos"("FileHash");
CREATE INDEX "IX_Videos_CreatedAt" ON "Videos"("CreatedAt" DESC) WHERE "IsDeleted" = false;
```

### 3.2 视频质量表
```sql
CREATE TABLE "VideoQualities" (
    "Id" uuid PRIMARY KEY,
    "VideoId" uuid NOT NULL REFERENCES "Videos"("Id"),
    "Name" varchar(20) NOT NULL,      -- 1080p, 720p等
    "Resolution" varchar(20),          -- 1920x1080
    "VideoBitrate" integer,            -- 码率（bps）
    "AudioBitrate" integer,
    "FilePath" varchar(500),           -- 文件路径
    "FileSize" bigint,
    "CreatedAt" timestamp with time zone NOT NULL
);

CREATE INDEX "IX_VideoQualities_VideoId" ON "VideoQualities"("VideoId");
```

### 3.3 视频上传任务表
```sql
CREATE TABLE "UploadTasks" (
    "Id" uuid PRIMARY KEY,
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "VideoId" uuid REFERENCES "Videos"("Id"),
    "FileName" varchar(200) NOT NULL,
    "FileSize" bigint NOT NULL,
    "FileHash" varchar(64) NOT NULL,
    "ChunkSize" integer NOT NULL,
    "TotalChunks" integer NOT NULL,
    "UploadedChunks" integer[] DEFAULT '{}',
    "Status" integer DEFAULT 0,        -- 0=进行中,1=已完成,2=失败
    "CreatedAt" timestamp with time zone NOT NULL,
    "CompletedAt" timestamp with time zone,
    "ExpiresAt" timestamp with time zone NOT NULL  -- 任务过期时间
);

CREATE INDEX "IX_UploadTasks_UserId" ON "UploadTasks"("UserId");
CREATE INDEX "IX_UploadTasks_FileHash" ON "UploadTasks"("FileHash");
```

---

## 4. 技术实现

### 4.1 技术栈
- **框架**: ABP Framework v10.0.2 + .NET 10.0
- **视频转码**: FFmpeg + FFMpegCore
- **消息队列**: RabbitMQ / CAP
- **对象存储**: MinIO / Aliyun OSS
- **数据库**: PostgreSQL
- **缓存**: Redis
- **播放器**: DPlayer / Video.js

### 4.2 转码Worker架构
```csharp
// 转码Worker服务
public class TranscodeWorkerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var consumer = scope.ServiceProvider.GetRequiredService<ITranscodeConsumer>();
        
        // 启动多个消费者实例（并发处理）
        var tasks = Enumerable.Range(0, _options.WorkerCount)
            .Select(_ => consumer.StartConsumingAsync(stoppingToken));
        
        await Task.WhenAll(tasks);
    }
}

// 转码消费者
public interface ITranscodeConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken);
}

public class RabbitMqTranscodeConsumer : ITranscodeConsumer
{
    public async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        var channel = await _rabbitMqService.CreateChannelAsync();
        await channel.QueueDeclareAsync("transcode_queue", durable: true);
        
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, ea) =>
        {
            var job = JsonSerializer.Deserialize<TranscodeJob>(ea.Body.ToArray());
            
            try
            {
                await ProcessTranscodeJobAsync(job);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transcode job failed");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true);
            }
        };
        
        await channel.BasicConsumeAsync("transcode_queue", false, consumer);
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
```

### 4.3 文件存储服务
```csharp
public interface IVideoStorageService
{
    Task<string> UploadChunkAsync(string uploadId, int chunkIndex, Stream chunkData);
    Task<string> MergeChunksAsync(string uploadId, string fileName, int totalChunks);
    Task DeleteVideoAsync(string videoId);
    Task<string> GetPlayUrlAsync(string videoId, int expiryMinutes = 30);
}

public class MinioVideoStorageService : IVideoStorageService
{
    public async Task<string> MergeChunksAsync(string uploadId, string fileName, int totalChunks)
    {
        var tempDir = $"/tmp/uploads/{uploadId}";
        var mergedPath = $"/tmp/{fileName}";
        
        // 按顺序合并分片
        using (var outputStream = File.Create(mergedPath))
        {
            for (int i = 0; i < totalChunks; i++)
            {
                var chunkPath = $"{tempDir}/{i}";
                using (var inputStream = File.OpenRead(chunkPath))
                {
                    await inputStream.CopyToAsync(outputStream);
                }
            }
        }
        
        // 上传到MinIO
        var objectName = $"uploads/{uploadId}/{fileName}";
        await _minioClient.PutObjectAsync(_bucketName, objectName, mergedPath);
        
        // 清理临时文件
        Directory.Delete(tempDir, true);
        File.Delete(mergedPath);
        
        return objectName;
    }
}
```

---

## 5. 性能优化

### 5.1 CDN加速
- **静态资源**：封面、静态文件走CDN
- **视频文件**：HLS分片走CDN边缘节点
- **回源配置**：CDN未命中时回源到对象存储

### 5.2 预加载策略
- **热门视频**：预加载前10%的内容
- **相关视频**：用户观看时预加载推荐视频封面

### 5.3 转码资源池
- **Worker池**：根据队列长度动态扩缩容
- **优先级**：会员视频优先转码
- **时间窗口**：低峰期批量处理非紧急任务

---

## 6. 监控与告警

### 6.1 关键指标
| 指标 | 告警阈值 | 说明 |
|------|---------|------|
| 转码队列长度 | >100 | 积压任务过多 |
| 转码失败率 | >5% | 转码质量问题 |
| 平均转码时间 | >10分钟 | 转码资源不足 |
| 视频播放成功率 | <99% | CDN或存储问题 |
| 上传成功率 | <99% | 网络或存储问题 |

---

## 7. 附录

### 7.1 错误码
| 错误码 | 描述 |
|--------|------|
| 20001 | 上传文件过大 |
| 20002 | 不支持的文件格式 |
| 20003 | 分片上传失败 |
| 20004 | 文件合并失败 |
| 20005 | 转码失败 |
| 20006 | 视频不存在 |
| 20007 | 无播放权限 |

### 7.2 相关文档
- [视频转码技术文档](./Video-Transcode.md)
- [FFmpeg使用指南](./FFmpeg-Guide.md)
- [对象存储配置文档](./OSS-Config.md)

---

**文档版本**: v1.0  
**创建时间**: 2026-05-12  
**技术栈**: ABP v10.0.2 + .NET 10.0 + FFmpeg + PostgreSQL + MinIO
