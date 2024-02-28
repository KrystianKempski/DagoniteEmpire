using DA_DataAccess;
using DagoniteEmpire.Helper;
using DagoniteEmpire.Service.IService;
using ImageMagick;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;


namespace DagoniteEmpire.Service
{
    public class FileUpload : IFileUpload
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IJSRuntime _jsRuntime;
        private readonly IMagickNET _magickNet;


        public FileUpload(IWebHostEnvironment environment, IJSRuntime jsRuntime)
        {
            _environment = environment;
            _jsRuntime = jsRuntime;
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
            try
            {
                FileInfo fileInfo = new(file.Name);
                var fileName = Guid.NewGuid().ToString() + ".webp";
                var folderDirectory = $"{_environment.WebRootPath}\\images\\portraits";

                if (!Directory.Exists(folderDirectory))
                {
                    Directory.CreateDirectory(folderDirectory);
                }
                var filePath = Path.Combine(folderDirectory, fileName);


                await using (MemoryStream input = new MemoryStream())
                {
                    await file.OpenReadStream(5120000).CopyToAsync(input);
                    using (var output = new MemoryStream())
                    {
                        input.Position = 0;
                        using (var image = new MagickImage(input))
                        {
                            image.Format = MagickFormat.WebP;
                            if (image.Width > 1024 || image.Height > 1024)
                            {
                                input.Position = input.Length;
                                image.Resize(1024, 1024);
                                image.Write(output);
                            }
                            else
                            {
                                input.Position = 0;
                                await input.CopyToAsync(output);
                            }
                            await image.WriteAsync(filePath);
                        }
                    }
                }

                var fullPath = $"/images/portraits/{fileName}";
                return fullPath;
            }
            catch (Exception ex)
            {
                await _jsRuntime.ToastrError(ex.Message);
            }
            return "";
        }
    }
}
