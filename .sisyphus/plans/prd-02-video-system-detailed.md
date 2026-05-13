# 02-视频系统详细PRD

## 1. 模块概述

### 1.1 业务定位
视频系统是平台的核心业务模块，负责视频的上传、转码、存储、播放、管理全生命周期。

### 1.2 确认的需求边界

| 需求项 | 确认结果 |
|--------|---------|
| 视频时长 | 无限制 |
| 文件大小 | 无限制 |
| 清晰度会员 | 全部免费（无会员特权） |
| 转码方案 | CPU转码（成本低） |
| CDN方案 | 仅本地存储（MinIO） |

---

## 2. 功能详细设计

### 2.1 视频上传

#### 2.1.1 分片上传流程

```
┌──────────┐    ┌──────────────┐    ┌──────────────┐
│ 选择文件  │───→│ 计算文件Hash  │───→│ 秒传检测      │
└──────────┘    └──────────────┘    └──────────────┘
                                         │
                         ┌───────────────┴──────────────┐
                         │                              │
                    ┌────▼────┐                  ┌─────▼─────┐
                    │ 已存在   │                  │ 需上传     │
                    │ 秒传成功 │                  │ 初始化会话 │
                    └────┬────┘                  └─────┬─────┘
                         │                              │
                         ▼                              ▼
                    ┌────────┐                   ┌──────────────┐
                    │ 完成    │                   │ 分片上传循环  │
                    └────────┘                   └──────┬───────┘
                                                       │
                                                       ▼
                                                ┌──────────────┐
                                                │ 合并分片      │
                                                └──────┬───────┘
                                                       │
                                                       ▼
                                                ┌──────────────┐
                                                │ 触发转码      │
                                                └──────┬───────┘
                                                       │
                                                       ▼
                                                ┌──────────────┐
                                                │ 完成上传      │
                                                └──────────────┘
```

---

#### 2.1.2 初始化上传API

```http
POST /api/v1/video/upload/init
Authorization: Bearer {accessToken}
Content-Type: application/json

Request Body:
{
  "fileName": "test_video.mp4",
  "fileSize": 2147483648,        // 2GB，无限制
  "fileHash": "md5_hash_value",  // 文件完整MD5
  "mimeType": "video/mp4"
}

Response:
{
  "code": 0,
  "message": "上传初始化成功",
  "data": {
    "uploadId": "uuid",
    "chunkSize": 5242880,         // 5MB分片
    "totalChunks": 410,           // 总分片数
    "uploadedChunks": [],         // 已上传分片索引（用于断点续传）
    "isFastUpload": false,        // 是否秒传
    "expiresAt": "2024-06-01T12:00:00Z"  // 24小时后过期
  }
}

秒传响应:
{
  "code": 0,
  "message": "秒传成功",
  "data": {
    "uploadId": null,
    "isFastUpload": true,
    "videoId": "existing_video_uuid",
    "title": "已存在视频标题"
  }
}
```

---

#### 2.1.3 分片上传API

```http
PUT /api/v1/video/upload/{uploadId}/chunks/{chunkIndex}
Authorization: Bearer {accessToken}
Content-Type: application/octet-stream

Request Body: [二进制分片数据]

Response:
{
  "code": 0,
  "message": "分片上传成功",
  "data": {
    "chunkIndex": 3,
    "uploadedChunks": [0, 1, 2, 3],
    "totalChunks": 410,
    "progress": 0.98,             // 进度百分比
    "isComplete": false
  }
}

错误响应:
{
  "code": 20001,
  "message": "上传会话已过期"
}
```

---

#### 2.1.4 完成上传API

```http
POST /api/v1/video/upload/{uploadId}/complete
Authorization: Bearer {accessToken}
Content-Type: application/json

Request Body:
{
  "title": "视频标题",
  "description": "视频描述",
  "categoryId": "uuid",
  "tags": ["游戏", "教程"],
  "coverType": "auto",           // auto=自动截取, custom=自定义
  "coverUrl": null               // 如果custom，提供URL
}

Response:
{
  "code": 0,
  "message": "上传完成，转码任务已提交",
  "data": {
    "videoId": "uuid",
    "title": "视频标题",
    "status": "transcoding",      // uploading/transcoding/published/failed
    "transcodeProgress": 0,
    "estimatedTime": "10-30分钟"  // 预估转码时间
  }
}
```

---

#### 2.1.5 断点续传查询

```http
GET /api/v1/video/upload/{uploadId}/progress
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "uploadId": "uuid",
    "uploadedChunks": [0, 1, 2, 5, 10],
    "totalChunks": 410,
    "progress": 1.2,
    "expiresAt": "2024-06-01T12:00:00Z",
    "isExpired": false
  }
}
```

---

#### 2.1.6 技术实现代码

```csharp
public class VideoUploadService
{
    private readonly IUploadSessionRepository _uploadSessionRepo;
    private readonly IVideoRepository _videoRepo;
    private readonly IBlobContainer _blobContainer;
    private readonly ITranscodeQueue _transcodeQueue;
    private readonly IDistributedCache _cache;
    
    // 分片大小：5MB
    private const int CHUNK_SIZE = 5 * 1024 * 1024;
    
    public async Task<InitUploadResult> InitUploadAsync(
        Guid userId,
        InitUploadInput input)
    {
        // 1. 检查文件类型
        if (!IsValidVideoType(input.MimeType))
            throw new BusinessException("20001", "不支持的文件类型");
        
        // 2. 秒传检测
        var existingVideo = await _videoRepo.FindByFileHashAsync(input.FileHash);
        if (existingVideo != null && existingVideo.Status == VideoStatus.Published)
        {
            return InitUploadResult.FastUpload(existingVideo.Id);
        }
        
        // 3. 计算分片数量
        var totalChunks = (int)Math.Ceiling(input.FileSize / (double)CHUNK_SIZE);
        
        // 4. 创建上传会话
        var session = new UploadSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = input.FileName,
            FileSize = input.FileSize,
            FileHash = input.FileHash,
            ChunkSize = CHUNK_SIZE,
            TotalChunks = totalChunks,
            UploadedChunks = new List<int>(),
            Status = UploadSessionStatus.InProgress,
            ExpiresAt = DateTime.UtcNow.AddHours(24),  // 24小时有效期
            CreationTime = DateTime.UtcNow
        };
        
        await _uploadSessionRepo.InsertAsync(session);
        
        // 5. 创建临时存储目录
        await _blobContainer.CreateDirectoryAsync($"temp/{session.Id}");
        
        return InitUploadResult.Success(session.Id, CHUNK_SIZE, totalChunks);
    }
    
    public async Task<UploadChunkResult> UploadChunkAsync(
        Guid uploadId,
        int chunkIndex,
        Stream chunkData)
    {
        // 1. 验证会话
        var session = await _uploadSessionRepo.GetAsync(uploadId);
        if (session == null)
            throw new BusinessException("20002", "上传会话不存在");
        
        if (session.ExpiresAt < DateTime.UtcNow)
            throw new BusinessException("20001", "上传会话已过期");
        
        // 2. 验证分片索引
        if (chunkIndex < 0 || chunkIndex >= session.TotalChunks)
            throw new BusinessException("20003", "分片索引无效");
        
        // 3. 检查是否已上传（断点续传）
        if (session.UploadedChunks.Contains(chunkIndex))
            return UploadChunkResult.AlreadyUploaded(chunkIndex);
        
        // 4. 保存分片到临时存储
        var chunkPath = $"temp/{uploadId}/{chunkIndex}";
        await _blobContainer.SaveAsync(chunkPath, chunkData);
        
        // 5. 更新已上传分片列表
        session.UploadedChunks.Add(chunkIndex);
        await _uploadSessionRepo.UpdateAsync(session);
        
        // 6. 计算进度
        var progress = session.UploadedChunks.Count / (double)session.TotalChunks;
        
        return UploadChunkResult.Success(
            chunkIndex,
            session.UploadedChunks.ToList(),
            session.TotalChunks,
            progress,
            session.UploadedChunks.Count == session.TotalChunks);
    }
    
    public async Task<CompleteUploadResult> CompleteUploadAsync(
        Guid uploadId,
        Guid userId,
        CompleteUploadInput input)
    {
        // 1. 验证会话
        var session = await _uploadSessionRepo.GetAsync(uploadId);
        if (session.Status != UploadSessionStatus.InProgress)
            throw new BusinessException("20004", "上传会话状态异常");
        
        // 2. 验证所有分片是否已上传
        if (session.UploadedChunks.Count != session.TotalChunks)
            throw new BusinessException("20005", "分片未全部上传完成");
        
        // 3. 合并分片
        var mergedFilePath = await MergeChunksAsync(session);
        
        // 4. 验证文件Hash
        var actualHash = await CalculateFileHash(mergedFilePath);
        if (actualHash != session.FileHash)
            throw new BusinessException("20006", "文件Hash校验失败");
        
        // 5. 创建视频记录
        var video = new Video
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = input.Title,
            Description = input.Description,
            CategoryId = input.CategoryId,
            Tags = input.Tags,
            Status = VideoStatus.Transcoding,
            FileHash = session.FileHash,
            FileSize = session.FileSize,
            OriginalFilePath = mergedFilePath,
            CreationTime = DateTime.UtcNow
        };
        
        await _videoRepo.InsertAsync(video);
        
        // 6. 发送转码任务到队列
        await _transcodeQueue.PublishAsync(new TranscodeTask
        {
            VideoId = video.Id,
            FilePath = mergedFilePath,
            UserId = userId
        });
        
        // 7. 清理临时文件和会话
        await _uploadSessionRepo.DeleteAsync(uploadId);
        
        return CompleteUploadResult.Success(video.Id, VideoStatus.Transcoding);
    }
    
    private async Task<string> MergeChunksAsync(UploadSession session)
    {
        var mergedFilePath = $"videos/{session.Id}/{session.FileName}";
        
        using (var outputStream = await _blobContainer.OpenWriteAsync(mergedFilePath))
        {
            for (int i = 0; i < session.TotalChunks; i++)
            {
                var chunkPath = $"temp/{session.Id}/{i}";
                using (var chunkStream = await _blobContainer.OpenReadAsync(chunkPath))
                {
                    await chunkStream.CopyToAsync(outputStream);
                }
            }
        }
        
        return mergedFilePath;
    }
    
    private bool IsValidVideoType(string mimeType)
    {
        var validTypes = new[] { "video/mp4", "video/avi", "video/mov", "video/webm" };
        return validTypes.Contains(mimeType.ToLower());
    }
}
```

---

### 2.2 视频转码

#### 2.2.1 转码规格表

| 清晰度 | 分辨率 | 视频码率 | 音频码率 | 编码 | 说明 |
|--------|--------|---------|---------|------|------|
| 1080P | 1920x1080 | 3000Kbps | 128Kbps | H.264 | 最高清晰度（免费） |
| 720P | 1280x720 | 1500Kbps | 128Kbps | H.264 | 高清（免费） |
| 480P | 854x480 | 800Kbps | 96Kbps | H.264 | 标清（免费） |
| 360P | 640x360 | 400Kbps | 96Kbps | H.264 | 流畅（免费） |

---

#### 2.2.2 转码任务队列

**RabbitMQ队列定义**:

```
Queue: transcode_tasks
Message Body:
{
  "videoId": "uuid",
  "filePath": "videos/uuid/original.mp4",
  "userId": "uuid",
  "createdAt": "2024-06-01T10:00:00Z"
}

队列配置:
- durable: true
- prefetch: 1（每个Worker一次处理1个任务）
- retry: 3次失败后丢弃
```

---

#### 2.2.3 FFmpeg转码命令

**转码Worker实现**:

```csharp
public class TranscodeWorkerService : BackgroundService
{
    private readonly ITranscodeService _transcodeService;
    private readonly IConfiguration _config;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 从RabbitMQ获取任务
                var task = await GetTranscodeTaskAsync();
                
                if (task != null)
                {
                    await ProcessTranscodeTaskAsync(task);
                }
                
                await Task.Delay(1000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transcode worker error");
            }
        }
    }
    
    private async Task ProcessTranscodeTaskAsync(TranscodeTask task)
    {
        try
        {
            // 1. 更新视频状态：转码中
            await UpdateVideoStatus(task.VideoId, VideoStatus.Transcoding);
            
            // 2. FFProbe分析视频
            var mediaInfo = await AnalyzeVideo(task.FilePath);
            
            // 3. 生成封面（自动截取3个候选）
            var covers = await GenerateCovers(task.VideoPath, mediaInfo.Duration);
            
            // 4. 转码多清晰度
            var resolutions = new[]
            {
                ("1080p", 1920, 1080, "3000k"),
                ("720p", 1280, 720, "1500k"),
                ("480p", 854, 480, "800k"),
                ("360p", 640, 360, "400k")
            };
            
            foreach (var (name, width, height, bitrate) in resolutions)
            {
                // 只转码低于原始分辨率的规格
                if (width <= mediaInfo.Width && height <= mediaInfo.Height)
                {
                    await TranscodeResolution(
                        task.VideoId,
                        task.FilePath,
                        name,
                        width,
                        height,
                        bitrate);
                }
            }
            
            // 5. 生成HLS主文件
            await GenerateMasterPlaylist(task.VideoId);
            
            // 6. 更新视频状态：已发布
            await UpdateVideoStatus(task.VideoId, VideoStatus.Published);
            
            // 7. 发送通知
            await SendTranscodeCompleteNotification(task.UserId, task.VideoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transcode failed: {VideoId}", task.VideoId);
            await UpdateVideoStatus(task.VideoId, VideoStatus.TranscodeFailed);
        }
    }
    
    private async Task TranscodeResolution(
        Guid videoId,
        string inputPath,
        string name,
        int width,
        int height,
        string bitrate)
    {
        var outputPath = $"videos/{videoId}/{name}.m3u8";
        
        // FFmpeg命令（CPU转码）
        var ffmpegArgs = $"-i {inputPath} " +
            $"-vf scale={width}:{height} " +
            $"-c:v libx264 -preset medium -crf 23 " +
            $"-b:v {bitrate} " +
            $"-c:a aac -b:a 128k " +
            $"-f hls " +
            $"-hls_time 10 " +              // 每个分片10秒
            $"-hls_list_size 0 " +          // 无限制分片数
            $"-hls_segment_filename videos/{videoId}/{name}_%03d.ts " +
            $"{outputPath}";
        
        // 执行FFmpeg
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };
        
        process.Start();
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
            throw new Exception($"Transcode failed: {name}");
        
        // 上传到MinIO
        await UploadToStorage(outputPath);
    }
    
    private async Task GenerateMasterPlaylist(Guid videoId)
    {
        var masterContent = "#EXTM3U\n#EXT-X-VERSION:3\n";
        
        // 添加各清晰度
        masterContent += "#EXT-X-STREAM-INF:BANDWIDTH=3000000,RESOLUTION=1920x1080\n";
        masterContent += "1080p.m3u8\n";
        
        masterContent += "#EXT-X-STREAM-INF:BANDWIDTH=1500000,RESOLUTION=1280x720\n";
        masterContent += "720p.m3u8\n";
        
        masterContent += "#EXT-X-STREAM-INF:BANDWIDTH=800000,RESOLUTION=854x480\n";
        masterContent += "480p.m3u8\n";
        
        masterContent += "#EXT-X-STREAM-INF:BANDWIDTH=400000,RESOLUTION=640x360\n";
        masterContent += "360p.m3u8\n";
        
        var masterPath = $"videos/{videoId}/master.m3u8";
        await _blobContainer.SaveTextAsync(masterPath, masterContent);
    }
}
```

---

### 2.3 视频播放

#### 2.3.1 获取播放地址API

```http
GET /api/v1/video/{videoId}/play
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "videoId": "uuid",
    "title": "视频标题",
    "duration": 600,
    "masterPlaylist": "http://localhost:9000/videos/{videoId}/master.m3u8",
    "resolutions": [
      {
        "name": "1080P",
        "resolution": "1920x1080",
        "url": "http://localhost:9000/videos/{videoId}/1080p.m3u8",
        "bandwidth": 3000000
      },
      {
        "name": "720P",
        "resolution": "1280x720",
        "url": "http://localhost:9000/videos/{videoId}/720p.m3u8",
        "bandwidth": 1500000
      },
      {
        "name": "480P",
        "resolution": "854x480",
        "url": "http://localhost:9000/videos/{videoId}/480p.m3u8",
        "bandwidth": 800000
      },
      {
        "name": "360P",
        "resolution": "640x360",
        "url": "http://localhost:9000/videos/{videoId}/360p.m3u8",
        "bandwidth": 400000
      }
    ],
    "progress": 120,              // 上次观看进度
    "cover": "http://...",
    "user": {
      "id": "uuid",
      "nickname": "UP主",
      "avatar": "..."
    }
  }
}
```

---

#### 2.3.2 记录播放进度API

```http
POST /api/v1/video/{videoId}/progress
Authorization: Bearer {accessToken}
Content-Type: application/json

Request Body:
{
  "currentTime": 125.5,           // 当前播放时间（秒）
  "duration": 600,                // 视频时长
  "resolution": "720p"            // 当前清晰度
}

Response:
{
  "code": 0,
  "message": "进度已记录"
}
```

---

### 2.4 视频管理（UP主后台）

#### 2.4.1 视频列表API

```http
GET /api/v1/video/manage/list?page=1&pageSize=20&status=all
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "data": {
    "items": [
      {
        "id": "uuid",
        "title": "视频标题",
        "cover": "http://...",
        "duration": 600,
        "status": "published",
        "transcodeProgress": 100,
        "viewCount": 10000,
        "likeCount": 500,
        "coinCount": 200,
        "collectCount": 100,
        "commentCount": 50,
        "createdAt": "2024-06-01T10:00:00Z",
        "publishedAt": "2024-06-01T10:30:00Z"
      }
    ],
    "totalCount": 50
  }
}
```

---

#### 2.4.2 编辑视频API

```http
PUT /api/v1/video/manage/{videoId}
Authorization: Bearer {accessToken}
Content-Type: application/json

Request Body:
{
  "title": "新标题",
  "description": "新描述",
  "categoryId": "uuid",
  "tags": ["新标签"],
  "cover": "http://new_cover_url"
}

Response:
{
  "code": 0,
  "message": "修改成功"
}
```

---

#### 2.4.3 删除视频API

```http
DELETE /api/v1/video/manage/{videoId}
Authorization: Bearer {accessToken}

Response:
{
  "code": 0,
  "message": "视频已删除"
}
```

---

## 3. 数据库设计

### 3.1 核心表结构

#### Videos表（视频主表）

```sql
CREATE TABLE "Videos" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "Title" varchar(200) NOT NULL,
    "Description" text,
    "Cover" varchar(500),
    "CategoryId" uuid NOT NULL REFERENCES "Categories"("Id"),
    "Tags" jsonb DEFAULT '[]',
    "Status" integer DEFAULT 0,              -- 0=上传中,1=转码中,2=已发布,3=转码失败
    "Duration" integer,                     -- 时长（秒）
    "FileSize" bigint,                      -- 文件大小
    "Width" integer,                        -- 原始宽度
    "Height" integer,                       -- 原始高度
    "FileHash" varchar(64) NOT NULL,       -- 文件MD5
    "OriginalFilePath" varchar(500),        -- 原始文件路径
    "MasterPlaylistPath" varchar(500),      -- HLS主文件路径
    "ViewCount" bigint DEFAULT 0,
    "LikeCount" bigint DEFAULT 0,
    "CoinCount" bigint DEFAULT 0,
    "CollectCount" bigint DEFAULT 0,
    "CommentCount" bigint DEFAULT 0,
    "DanmakuCount" bigint DEFAULT 0,
    "TranscodeProgress" integer DEFAULT 0,  -- 转码进度（%）
    "IsDeleted" boolean DEFAULT false,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW(),
    "PublishTime" timestamp with time zone,
    "DeletionTime" timestamp with time zone
);

CREATE INDEX "IX_Videos_UserId" ON "Videos"("UserId") WHERE "IsDeleted" = false;
CREATE INDEX "IX_Videos_CategoryId" ON "Videos"("CategoryId") WHERE "IsDeleted" = false;
CREATE INDEX "IX_Videos_Status" ON "Videos"("Status");
CREATE INDEX "IX_Videos_FileHash" ON "Videos"("FileHash");
CREATE INDEX "IX_Videos_PublishTime" ON "Videos"("PublishTime" DESC) WHERE "Status" = 2;
CREATE INDEX "IX_Videos_ViewCount" ON "Videos"("ViewCount" DESC) WHERE "Status" = 2;
CREATE INDEX "IX_Videos_Tags" ON "Videos" USING GIN("Tags");
```

---

#### VideoResolutions表（多清晰度）

```sql
CREATE TABLE "VideoResolutions" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" uuid NOT NULL REFERENCES "Videos"("Id"),
    "Name" varchar(20) NOT NULL,            -- 1080p, 720p等
    "Width" integer NOT NULL,
    "Height" integer NOT NULL,
    "VideoBitrate" integer NOT NULL,        -- 视频码率
    "AudioBitrate" integer NOT NULL,        -- 音频码率
    "FilePath" varchar(500) NOT NULL,       -- M3U8文件路径
    "FileSize" bigint,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW()
);

CREATE INDEX "IX_VideoResolutions_VideoId" ON "VideoResolutions"("VideoId");
```

---

#### UploadSessions表（上传会话）

```sql
CREATE TABLE "UploadSessions" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" uuid NOT NULL REFERENCES "Users"("Id"),
    "FileName" varchar(200) NOT NULL,
    "FileSize" bigint NOT NULL,
    "FileHash" varchar(64) NOT NULL,
    "MimeType" varchar(100),
    "ChunkSize" integer NOT NULL,           -- 分片大小
    "TotalChunks" integer NOT NULL,
    "UploadedChunks" jsonb DEFAULT '[]',    -- 已上传分片索引
    "Status" integer DEFAULT 0,             -- 0=进行中,1=已完成,2=已过期
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW(),
    "CompletionTime" timestamp with time zone
);

CREATE INDEX "IX_UploadSessions_UserId" ON "UploadSessions"("UserId");
CREATE INDEX "IX_UploadSessions_FileHash" ON "UploadSessions"("FileHash");
CREATE INDEX "IX_UploadSessions_ExpiresAt" ON "UploadSessions"("ExpiresAt");
```

---

#### VideoViews表（播放记录）

```sql
CREATE TABLE "VideoViews" (
    "Id" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "VideoId" uuid NOT NULL REFERENCES "Videos"("Id"),
    "UserId" uuid REFERENCES "Users"("Id"),  -- 未登录为NULL
    "Progress" integer DEFAULT 0,            -- 播放进度（秒）
    "Resolution" varchar(20),                -- 观看清晰度
    "IP" varchar(50) NOT NULL,
    "DeviceType" varchar(50),                -- web/mobile/app
    "WatchDuration" integer DEFAULT 0,       -- 本次观看时长
    "IsCompleted" boolean DEFAULT false,     -- 是否看完
    "CreationTime" timestamp with time zone NOT NULL DEFAULT NOW()
) PARTITION BY RANGE ("CreationTime");

CREATE TABLE "VideoViews_2024_06" PARTITION OF "VideoViews"
FOR VALUES FROM ('2024-06-01') TO ('2024-07-01');

CREATE INDEX "IX_VideoViews_VideoId" ON "VideoViews"("VideoId");
CREATE INDEX "IX_VideoViews_UserId" ON "VideoViews"("UserId");
CREATE INDEX "IX_VideoViews_CreationTime" ON "VideoViews"("CreationTime" DESC);
```

---

## 4. 错误码定义

| 错误码 | 说明 | HTTP状态码 |
|--------|------|-----------|
| 20001 | 不支持的文件类型 | 400 |
| 20002 | 上传会话不存在 | 404 |
| 20003 | 分片索引无效 | 400 |
| 20004 | 上传会话状态异常 | 400 |
| 20005 | 分片未全部上传 | 400 |
| 20006 | 文件Hash校验失败 | 400 |
| 20007 | 视频不存在 | 404 |
| 20008 | 无权限修改 | 403 |
| 20009 | 视频转码失败 | 500 |
| 20010 | 视频已删除 | 404 |

---

**文档版本**: v2.0  
**创建日期**: 2026-05-12  
**状态**: ✅ 详细设计完成