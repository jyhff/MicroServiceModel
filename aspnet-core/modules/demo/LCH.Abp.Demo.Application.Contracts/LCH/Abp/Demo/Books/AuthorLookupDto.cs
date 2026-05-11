using System;
using Volo.Abp.Application.Dtos;

namespace LCH.Abp.Demo.Books;
public class AuthorLookupDto : EntityDto<Guid>
{
    public string Name { get; set; }
}