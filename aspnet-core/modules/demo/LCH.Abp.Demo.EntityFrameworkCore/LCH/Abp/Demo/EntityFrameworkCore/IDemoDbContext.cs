using LCH.Abp.Demo.Authors;
using LCH.Abp.Demo.Books;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace LCH.Abp.Demo.EntityFrameworkCore;

[ConnectionStringName(DemoDbProterties.ConnectionStringName)]
public interface IDemoDbContext : IEfCoreDbContext
{
    DbSet<Book> Books { get; }

    DbSet<Author> Authors { get; }
}
