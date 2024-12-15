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
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Repository.ChatRepos;
using System.Linq;
using Abp.Json;
using Abp.Collections.Extensions;
using Humanizer;
using Microsoft.JSInterop;
using MudBlazor;


namespace RichTextEditor.Data
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterPostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CharacterPostsController(IHttpContextAccessor httpContextAccessor, IPostRepository postRepository)
        {
            _postRepository = postRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{id}")]
        [Consumes("application/json")]
        public async Task<string> Get(int id)
        {
            try
            {
                if(_httpContextAccessor?.HttpContext == null)
                {
                    return "Null context";
                }

            var headers = _httpContextAccessor.HttpContext.Request.Headers;

                if (_httpContextAccessor.HttpContext.Request.Headers == null)
                {
                    return "null header";
                }
                string aa = "";
                foreach (var head in headers)
                {
                    aa += head.Value.ToString();
                }
                return aa;

                var dateFrom = headers["date_from"];
            var dateTo = headers["date_to"];
            int postCount = 0;
            DateTime? from = null, to = null;
            if (dateFrom.IsNullOrEmpty() == false)
            {
                from = DateTime.Parse(dateFrom);                        
                
            }
            if (dateTo.IsNullOrEmpty() == false)
            {
                to = DateTime.Parse(dateTo);

            }
            postCount = await _postRepository.GetCharacterPostCount(id,from,to);

            return "";
            }catch(Exception ex)
            {
                return ex.Message;
            }
        }
    }
}