using DA_DataAccess;
using Microsoft.AspNetCore.Components.Forms;

namespace DagoniteEmpire.Service.IService
{
    public interface IFileUpload
    {
        Task<string> UploadFile(IBrowserFile file);

        bool DeleteFile(string filePath);
    }
}
