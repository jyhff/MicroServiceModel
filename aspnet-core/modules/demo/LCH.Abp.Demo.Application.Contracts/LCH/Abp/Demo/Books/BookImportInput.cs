using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace LCH.Abp.Demo.Books;
public class BookImportInput
{
    [Required]
    public IRemoteStreamContent Content { get; set; }   
}
