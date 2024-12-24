using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using ImageMagick;
using Microsoft.Extensions.Hosting;

namespace RichTextEditor.Data
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileUploadController : ControllerBase
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        private readonly IWebHostEnvironment hostingEnv;

        public FileUploadController(IWebHostEnvironment env)
        {
            this.hostingEnv = env;
        }

        [HttpPost("[action]")]
        [Route("api/FileUpload/Save")]
        public void Save(IList<IFormFile> UploadFiles)
        {
            try
            {
                foreach (var file in UploadFiles)
                {
                    if (UploadFiles != null)
                    {
                        Response.StatusCode = 204;
                        FileInfo fileInfo = new(file.Name);
                        var fileName = Guid.NewGuid().ToString() + ".webp";
                        var folderDirectory = hostingEnv.WebRootPath + "/upload/postImages";

                        if (!Directory.Exists(folderDirectory))
                        {
                            Directory.CreateDirectory(folderDirectory);
                        }
                        var filePath = Path.Combine(folderDirectory, fileName);

                        using (MemoryStream input = new MemoryStream())
                        {
                            file.CopyTo(input);
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
                                        input.CopyTo(output);
                                    }
                                    input.Flush();
                                    image.Write(filePath);

                                    Response.Headers.Add("name", fileName);
                                    Response.StatusCode = 200;
                                    Response.ContentType = "application/json; charset=utf-8";
                                }
                            }
                        }
                    }
                }
                
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
        }
    }
}