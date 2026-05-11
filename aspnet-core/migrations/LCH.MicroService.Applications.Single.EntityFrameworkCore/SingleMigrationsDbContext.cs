using LCH.Abp.DataProtectionManagement;
using LCH.Abp.DataProtectionManagement.EntityFrameworkCore;
using LCH.Abp.Gdpr;
using LCH.Abp.Gdpr.EntityFrameworkCore;
using LCH.Abp.LocalizationManagement;
using LCH.Abp.LocalizationManagement.EntityFrameworkCore;
using LCH.Abp.MessageService.Chat;
using LCH.Abp.MessageService.EntityFrameworkCore;
using LCH.Abp.MessageService.Groups;
using LCH.Abp.Notifications;
using LCH.Abp.Notifications.EntityFrameworkCore;
using LCH.Abp.Saas.Editions;
using LCH.Abp.Saas.EntityFrameworkCore;
using LCH.Abp.Saas.Tenants;
using LCH.Abp.TaskManagement;
using LCH.Abp.TaskManagement.EntityFrameworkCore;
using LCH.Abp.TextTemplating;
using LCH.Abp.TextTemplating.EntityFrameworkCore;
using LCH.Abp.WebhooksManagement;
using LCH.Abp.WebhooksManagement.EntityFrameworkCore;
using LCH.Platform.Datas;
using LCH.Platform.EntityFrameworkCore;
using LCH.Platform.Layouts;
using LCH.Platform.Menus;
using LCH.Platform.Messages;
using LCH.Platform.Packages;
using LCH.Platform.Portal;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.Applications;
using Volo.Abp.OpenIddict.Authorizations;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.OpenIddict.Scopes;
using Volo.Abp.OpenIddict.Tokens;
using Volo.Abp.PermissionManagement;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.EntityFrameworkCore;

namespace LCH.MicroService.Applications.Single.EntityFrameworkCore;

[ReplaceDbContext(typeof(IAuditLoggingDbContext))]
[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(IOpenIddictDbContext))]
[ReplaceDbContext(typeof(ISaasDbContext))]
[ReplaceDbContext(typeof(IFeatureManagementDbContext))]
[ReplaceDbContext(typeof(ISettingManagementDbContext))]
[ReplaceDbContext(typeof(IPermissionManagementDbContext))]
[ReplaceDbContext(typeof(ITextTemplatingDbContext))]
[ReplaceDbContext(typeof(ITaskManagementDbContext))]
[ReplaceDbContext(typeof(IWebhooksManagementDbContext))]
[ReplaceDbContext(typeof(IPlatformDbContext))]
[ReplaceDbContext(typeof(ILocalizationDbContext))]
[ReplaceDbContext(typeof(INotificationsDbContext))]
[ReplaceDbContext(typeof(INotificationsDefinitionDbContext))]
[ReplaceDbContext(typeof(IMessageServiceDbContext))]
[ReplaceDbContext(typeof(IAbpDataProtectionManagementDbContext))]
[ReplaceDbContext(typeof(IGdprDbContext))]

[ConnectionStringName("Default")]
public class SingleMigrationsDbContext : 
    AbpDbContext<SingleMigrationsDbContext>,
    IAuditLoggingDbContext,
    IIdentityDbContext,
    IOpenIddictDbContext,
    ISaasDbContext,
    IFeatureManagementDbContext,
    ISettingManagementDbContext,
    IPermissionManagementDbContext,
    ITextTemplatingDbContext,
    ITaskManagementDbContext,
    IWebhooksManagementDbContext,
    IPlatformDbContext,
    ILocalizationDbContext,
    INotificationsDbContext,
    INotificationsDefinitionDbContext,
    IMessageServiceDbContext,
    IAbpDataProtectionManagementDbContext,
    IGdprDbContext
{
    public SingleMigrationsDbContext(DbContextOptions<SingleMigrationsDbContext> options)
        : base(options)
    {

    }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<AuditLogExcelFile> AuditLogExcelFiles { get; set; }

    public DbSet<IdentityUser> Users { get; set; }

    public DbSet<IdentityRole> Roles { get; set; }

    public DbSet<IdentityClaimType> ClaimTypes { get; set; }

    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }

    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }

    public DbSet<IdentityLinkUser> LinkUsers { get; set; }

    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    public DbSet<IdentitySession> Sessions { get; set; }

    public DbSet<OpenIddictApplication> Applications { get; set; }

    public DbSet<OpenIddictAuthorization> Authorizations { get; set; }

    public DbSet<OpenIddictScope> Scopes { get; set; }

    public DbSet<OpenIddictToken> Tokens { get; set; }

    public DbSet<Edition> Editions { get; set; }

    public DbSet<Tenant> Tenants { get; set; }

    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    public DbSet<FeatureGroupDefinitionRecord> FeatureGroups { get; set; }

    public DbSet<FeatureDefinitionRecord> Features { get; set; }

    public DbSet<FeatureValue> FeatureValues { get; set; }

    public DbSet<Setting> Settings { get; set; }

    public DbSet<SettingDefinitionRecord> SettingDefinitionRecords { get; set; }

    public DbSet<PermissionGroupDefinitionRecord> PermissionGroups { get; set; }

    public DbSet<PermissionDefinitionRecord> Permissions { get; set; }

    public DbSet<PermissionGrant> PermissionGrants { get; set; }

    public DbSet<TextTemplate> TextTemplates { get; set; }

    public DbSet<TextTemplateDefinition> TextTemplateDefinitions { get; set; }

    public DbSet<BackgroundJobInfo> BackgroundJobInfos { get; set; }

    public DbSet<BackgroundJobAction> BackgroundJobAction { get; set; }

    public DbSet<BackgroundJobLog> BackgroundJobLogs { get; set; }

    public DbSet<WebhookSendRecord> WebhookSendRecord { get; set; }

    public DbSet<WebhookGroupDefinitionRecord> WebhookGroupDefinitionRecords { get; set; }

    public DbSet<WebhookDefinitionRecord> WebhookDefinitionRecords { get; set; }

    public DbSet<Menu> Menus { get; set; }

    public DbSet<Layout> Layouts { get; set; }

    public DbSet<RoleMenu> RoleMenus { get; set; }

    public DbSet<UserMenu> UserMenus { get; set; }

    public DbSet<UserFavoriteMenu> UserFavoriteMenus { get; set; }

    public DbSet<Data> Datas { get; set; }

    public DbSet<DataItem> DataItems { get; set; }

    public DbSet<Package> Packages { get; set; }

    public DbSet<PackageBlob> PackageBlobs { get; set; }

    public DbSet<Enterprise> Enterprises { get; set; }

    public DbSet<EmailMessage> EmailMessages { get; set; }

    public DbSet<SmsMessage> SmsMessages { get; set; }

    public DbSet<Resource> Resources { get; set; }

    public DbSet<Language> Languages { get; set; }

    public DbSet<Text> Texts { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<UserNotification> UserNotifications { get; set; }

    public DbSet<UserSubscribe> UserSubscribes { get; set; }

    public DbSet<NotificationDefinitionGroupRecord> NotificationDefinitionGroupRecords { get; set; }

    public DbSet<NotificationDefinitionRecord> NotificationDefinitionRecords { get; set; }

    public DbSet<UserMessage> UserMessages { get; set; }

    public DbSet<GroupMessage> GroupMessages { get; set; }

    public DbSet<UserChatFriend> UserChatFriends { get; set; }

    public DbSet<UserChatSetting> UserChatSettings { get; set; }

    public DbSet<GroupChatBlack> GroupChatBlacks { get; set; }

    public DbSet<ChatGroup> ChatGroups { get; set; }

    public DbSet<UserChatGroup> UserChatGroups { get; set; }

    public DbSet<UserChatCard> UserChatCards { get; set; }

    public DbSet<UserGroupCard> UserGroupCards { get; set; }

    public DbSet<EntityTypeInfo> EntityTypeInfos { get; set; }

    public DbSet<GdprRequest> Requests { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureAuditLogging();
        modelBuilder.ConfigureIdentity();
        modelBuilder.ConfigureOpenIddict();
        modelBuilder.ConfigureSaas();
        modelBuilder.ConfigureFeatureManagement();
        modelBuilder.ConfigureSettingManagement();
        modelBuilder.ConfigurePermissionManagement();
        modelBuilder.ConfigureTextTemplating();
        modelBuilder.ConfigureTaskManagement();
        modelBuilder.ConfigureWebhooksManagement();
        modelBuilder.ConfigurePlatform();
        modelBuilder.ConfigureLocalization();
        modelBuilder.ConfigureNotifications();
        modelBuilder.ConfigureNotificationsDefinition();
        modelBuilder.ConfigureMessageService();
        modelBuilder.ConfigureDataProtectionManagement();
        modelBuilder.ConfigureGdpr();

    }
}
