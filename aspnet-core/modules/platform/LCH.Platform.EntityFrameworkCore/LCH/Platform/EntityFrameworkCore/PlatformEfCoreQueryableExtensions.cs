using LCH.Platform.Datas;
using LCH.Platform.Layouts;
using LCH.Platform.Menus;
using LCH.Platform.Packages;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LCH.Platform.EntityFrameworkCore;

public static class PlatformEfCoreQueryableExtensions
{

    public static IQueryable<Layout> IncludeDetails(this IQueryable<Layout> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable;
    }

    public static IQueryable<Menu> IncludeDetails(this IQueryable<Menu> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable;
    }

    public static IQueryable<Data> IncludeDetails(this IQueryable<Data> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable
            .AsSplitQuery()
            .Include(x => x.Items);
    }

    public static IQueryable<Package> IncludeDetails(this IQueryable<Package> queryable, bool include = true)
    {
        if (!include)
        {
            return queryable;
        }

        return queryable
            .AsSplitQuery()
            .Include(x => x.Blobs);
    }
}
