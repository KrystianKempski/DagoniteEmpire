﻿using DA_DataAccess;
using Microsoft.AspNetCore.Components.Forms;

namespace DagoniteEmpire.Service.IService
{
    public interface IFileUpload
    {
        Task<string> UploadFile(IBrowserFile file,string root);
        Task<string> UploadFile(string fileUrl, string root);
        bool DeleteFile(string filePath);
    }
}
