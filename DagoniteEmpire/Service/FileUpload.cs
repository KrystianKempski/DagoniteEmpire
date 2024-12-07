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

        public async Task<string> UploadFile(string fileUrl, string root)
        {
            try
            {
                if (root == string.Empty) throw new Exception("need folder name");

                var fileName = Guid.NewGuid().ToString() + ".webp";
                var folderDirectory = $"{_environment.WebRootPath}/upload/{root}";
                folderDirectory = folderDirectory.Replace(@"\", @"/");

                if (!Directory.Exists(folderDirectory))
                {
                    Directory.CreateDirectory(folderDirectory);
                }
                var filePath = Path.Combine(folderDirectory, fileName);


                {
                    using (var output = new MemoryStream())
                    {
                        byte[] newBytes = Convert.FromBase64String(fileUrl);

                        using (var image = new MagickImage(newBytes))
                        {
                            image.Format = MagickFormat.WebP;
                            await image.WriteAsync(filePath);
                        }
                    }
                }

                var fullPath = $"/upload/portraits/{fileName}";
                return fullPath;
            }
            catch (Exception ex)
            {
                await _jsRuntime.ToastrError(ex.Message);
            }
            return "";
        }
    
        public async Task<string> UploadFile(IBrowserFile file, string root)
        {
            try
            {
                if (root == string.Empty) throw new Exception("need folder name");
                FileInfo fileInfo = new(file.Name);
                var fileName = Guid.NewGuid().ToString() + ".webp";
                var folderDirectory = $"{_environment.WebRootPath}/upload/{root}";
                folderDirectory = folderDirectory.Replace(@"\\", @"/");
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

                var fullPath = $"/upload/portraits/{fileName}";
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
