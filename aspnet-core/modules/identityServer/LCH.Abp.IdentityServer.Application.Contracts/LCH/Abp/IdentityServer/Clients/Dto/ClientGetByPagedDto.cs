using Volo.Abp.Application.Dtos;

namespace LCH.Abp.IdentityServer.Clients;

public class ClientGetByPagedDto : PagedAndSortedResultRequestDto
{
    public string Filter { get; set; }
}
