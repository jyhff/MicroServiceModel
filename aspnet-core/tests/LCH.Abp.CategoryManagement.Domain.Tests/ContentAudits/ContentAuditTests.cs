using System;
using LCH.Abp.CategoryManagement.ContentAudits;
using Shouldly;
using Xunit;

namespace LCH.Abp.CategoryManagement.Tests.ContentAudits;

public class ContentAuditTests
{
    [Fact]
    public void Create_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var videoId = Guid.NewGuid();

        // Act
        var contentAudit = ContentAudit.Create(id, videoId);

        // Assert
        contentAudit.Id.ShouldBe(id);
        contentAudit.VideoId.ShouldBe(videoId);
        contentAudit.Status.ShouldBe(AuditStatus.Pending);
        contentAudit.AuditorId.ShouldBeNull();
        contentAudit.AuditTime.ShouldBeNull();
        contentAudit.Reason.ShouldBeNull();
        contentAudit.AIResult.ShouldBeNull();
    }

    [Fact]
    public void Approve_Should_Set_Status_To_Approved()
    {
        // Arrange
        var contentAudit = ContentAudit.Create(Guid.NewGuid(), Guid.NewGuid());
        var auditorId = Guid.NewGuid();
        var reason = "审核通过";

        // Act
        contentAudit.Approve(auditorId, reason);

        // Assert
        contentAudit.Status.ShouldBe(AuditStatus.Approved);
        contentAudit.AuditorId.ShouldBe(auditorId);
        contentAudit.AuditTime.ShouldNotBeNull();
        contentAudit.Reason.ShouldBe(reason);
    }

    [Fact]
    public void Approve_Without_AuditorId_Should_Work()
    {
        // Arrange
        var contentAudit = ContentAudit.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act - AI自动审核通过（业务规则）
        contentAudit.Approve();

        // Assert
        contentAudit.Status.ShouldBe(AuditStatus.Approved);
        contentAudit.AuditorId.ShouldBeNull();
        contentAudit.AuditTime.ShouldNotBeNull();
    }

    [Fact]
    public void Reject_Should_Set_Status_To_Rejected()
    {
        // Arrange
        var contentAudit = ContentAudit.Create(Guid.NewGuid(), Guid.NewGuid());
        var auditorId = Guid.NewGuid();
        var reason = "内容违规";

        // Act
        contentAudit.Reject(auditorId, reason);

        // Assert
        contentAudit.Status.ShouldBe(AuditStatus.Rejected);
        contentAudit.AuditorId.ShouldBe(auditorId);
        contentAudit.AuditTime.ShouldNotBeNull();
        contentAudit.Reason.ShouldBe(reason);
    }

    [Fact]
    public void MarkForManualReview_Should_Set_Status_To_NeedsManualReview()
    {
        // Arrange
        var contentAudit = ContentAudit.Create(Guid.NewGuid(), Guid.NewGuid());
        var reason = "需要人工审核";

        // Act
        contentAudit.MarkForManualReview(reason);

        // Assert
        contentAudit.Status.ShouldBe(AuditStatus.NeedsManualReview);
        contentAudit.Reason.ShouldBe(reason);
        contentAudit.AuditTime.ShouldBeNull(); // 人工审核尚未完成
    }

    [Fact]
    public void SetAIResult_Should_Set_AIResult_Property()
    {
        // Arrange
        var contentAudit = ContentAudit.Create(Guid.NewGuid(), Guid.NewGuid());
        var aiResult = "{\"score\":0.95,\"flags\":[]}";

        // Act
        contentAudit.SetAIResult(aiResult);

        // Assert
        contentAudit.AIResult.ShouldBe(aiResult);
    }

    [Fact]
    public void Reason_Should_Be_Truncated_When_Exceeds_MaxLength()
    {
        // Arrange
        var contentAudit = ContentAudit.Create(Guid.NewGuid(), Guid.NewGuid());
        var longReason = new string('x', 2000); // 超过 MaxReasonLength=1000

        // Act
        contentAudit.Reject(Guid.NewGuid(), longReason);

        // Assert
        contentAudit.Reason!.Length.ShouldBe(ContentAuditConsts.MaxReasonLength);
    }
}