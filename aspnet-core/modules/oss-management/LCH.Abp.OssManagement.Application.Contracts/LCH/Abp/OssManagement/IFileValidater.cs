using System.Threading.Tasks;

namespace LCH.Abp.OssManagement;

public interface IFileValidater
{
    Task ValidationAsync(UploadFile input);
}
