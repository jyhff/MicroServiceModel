using LCH.Abp.DataProtection;

namespace LCH.Abp.Demo.Books;

public class BookAuth : DataAuthBase<Book, Guid>
{
    public BookAuth()
    {
    }

    public BookAuth(
        Guid entityId, 
        string role, 
        string organizationUnit, 
        Guid? tenantId = null) 
        : base(entityId, role, organizationUnit, tenantId)
    {
    }
}
