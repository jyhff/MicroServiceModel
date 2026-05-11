using JetBrains.Annotations;
using LCH.Abp.DataProtection;
using LCH.Abp.DataProtection.EntityFrameworkCore;
using LCH.Abp.Demo.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace LCH.Abp.Demo.Books;
public class EfCoreBookRepository : EfCoreDataProtectionRepository<DemoDbContext, Book, Guid, BookAuth>, IBookRepository
{
    public EfCoreBookRepository(
        [NotNull] IDbContextProvider<DemoDbContext> dbContextProvider,
        [NotNull] IDataAuthorizationService dataAuthorizationService,
        [NotNull] IEntityTypeFilterBuilder entityTypeFilterBuilder,
        [NotNull] IEntityPropertyResultBuilder entityPropertyResultBuilder) 
        : base(dbContextProvider, dataAuthorizationService, entityTypeFilterBuilder, entityPropertyResultBuilder)
    {
    }
}
