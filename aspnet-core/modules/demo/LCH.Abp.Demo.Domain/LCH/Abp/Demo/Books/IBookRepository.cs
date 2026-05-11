using Volo.Abp.Domain.Repositories;

namespace LCH.Abp.Demo.Books;
public interface IBookRepository : IRepository<Book, Guid>
{
}
