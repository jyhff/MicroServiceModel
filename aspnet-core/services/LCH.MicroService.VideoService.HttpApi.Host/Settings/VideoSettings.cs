using LCH.Abp.Video.Videos;
using System;
using System.Collections.Generic;

namespace LCH.MicroService.VideoService.Settings;

/// <summary>
/// 视频服务配置选项
/// 业务规则：
/// 1. 分片大小：5MB（用户需求）
/// 2. 会话过期：24小时（用户需求）
/// 3. 仅MinIO本地存储（用户需求）
/// 4. CPU转码（用户需求）
/// 5. 所有清晰度免费（用户需求）
/// </summary>
public class VideoSettings
{
    /// <summary>
    /// 分片大小（字节）
    /// 默认：5MB
    /// </summary>
    public int ChunkSize { get; set; } = 5 * 1024 * 1024;

    /// <summary>
    /// 上传会话过期时间（小时）
    /// 默认：24小时
    /// </summary>
    public int SessionExpirationHours { get; set; } = 24;

    /// <summary>
    /// 转码清晰度列表
    /// 默认：360P、480P、720P、1080P、4K
    /// </summary>
    public List<VideoResolution> TranscodingResolutions { get; set; } = new List<VideoResolution>
    {
        VideoResolution.P360,
        VideoResolution.P480,
        VideoResolution.P720,
        VideoResolution.P1080,
        VideoResolution.P4K
    };

    /// <summary>
    /// 最大并发转码任务数
    /// 默认：2（CPU转码，避免资源占用过高）
    /// </summary>
    public int MaxConcurrentTranscodingTasks { get; set; } = 2;

    /// <summary>
    /// 转码任务检查周期（毫秒）
    /// 默认：30秒
    /// </summary>
    public int TranscodingCheckPeriod { get; set; } = 30_000;

    /// <summary>
    /// 上传会话清理周期（毫秒）
    /// 默认：1小时
    /// </summary>
    public int SessionCleanupPeriod { get; set; } = 3_600_000;

    /// <summary>
    /// 存储桶配置
    /// </summary>
    public VideoStorageBucketSettings StorageBuckets { get; set; } = new VideoStorageBucketSettings();

    /// <summary>
    /// FFmpeg配置
    /// </summary>
    public FFmpegSettings FFmpeg { get; set; } = new FFmpegSettings();
}

/// <summary>
/// 视频存储桶配置
/// </summary>
public class VideoStorageBucketSettings
{
    /// <summary>
    /// 原始视频存储桶名称
    /// </summary>
    public string OriginalBucket { get; set; } = "video-original";

    /// <summary>
    /// 转码视频存储桶名称
    /// </summary>
    public string TranscodedBucket { get; set; } = "video-transcoded";

    /// <summary>
    /// HLS切片存储桶名称
    /// </summary>
    public string HlsBucket { get; set; } = "video-hls";

    /// <summary>
    /// 临时上传存储桶名称
    /// </summary>
    public string TempBucket { get; set; } = "video-temp";

    /// <summary>
    /// 封面图片存储桶名称
    /// </summary>
    public string CoverBucket { get; set; } = "video-cover";
}

/// <summary>
/// FFmpeg配置
/// </summary>
public class FFmpegSettings
{
    /// <summary>
    /// FFmpeg可执行文件路径
    /// 默认：ffmpeg（使用系统PATH）
    /// </summary>
    public string FFmpegPath { get; set; } = "ffmpeg";

    /// <summary>
    /// FFprobe可执行文件路径
    /// 默认：ffprobe（使用系统PATH）
    /// </summary>
    public string FFprobePath { get; set; } = "ffprobe";

    /// <summary>
    /// 转码预设
    /// 可选值：ultrafast, fast, medium, slow, veryslow
    /// 默认：medium（平衡速度和质量）
    /// </summary>
    public string Preset { get; set; } = "medium";

    /// <summary>
    /// 是否使用硬件加速
    /// 默认：false（CPU转码）
    /// </summary>
    public bool UseHardwareAcceleration { get; set; } = false;

    /// <summary>
    /// HLS切片时长（秒）
    /// 默认：10秒
    /// </summary>
    public int HlsSegmentDuration { get; set; } = 10;
}