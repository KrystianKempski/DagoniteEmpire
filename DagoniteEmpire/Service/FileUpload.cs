using DA_DataAccess;
using DagoniteEmpire.Service.IService;
using Microsoft.AspNetCore.Components.Forms;

namespace DagoniteEmpire.Service
{
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _environment;

        public FileUpload(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        public bool DeleteFile(string filePath)
        {
            if (File.Exists(_environment.WebRootPath + filePath))
            {
                File.Delete(_environment.WebRootPath + filePath);
                return true;
            }
            return false;
        }

        public async Task<string> UploadFile(IBrowserFile file)
        {
            FileInfo fileInfo = new(file.Name);
            var fileName = Guid.NewGuid().ToString() + fileInfo.Extension;
            var folderDirectory = $"{_environment.WebRootPath}\\images\\portraits";
            if (!Directory.Exists(folderDirectory))
            {
                Directory.CreateDirectory(folderDirectory);
            }
            var filePath = Path.Combine(folderDirectory, fileName);

            await using FileStream fs = new FileStream(filePath, FileMode.Create);
            await file.OpenReadStream().CopyToAsync(fs);

            var fullPath = $"/images/portraits/{fileName}";
            return fullPath;
        }
    }
}
