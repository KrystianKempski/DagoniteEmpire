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
using DagoniteEmpire.Helper;

namespace RichTextEditor.Data
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterPostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJSRuntime _jsRuntime;

        public CharacterPostsController(IHttpContextAccessor httpContextAccessor, IPostRepository postRepository, IJSRuntime jsRuntime)
        {
            _postRepository = postRepository;
            _httpContextAccessor = httpContextAccessor; 
            _jsRuntime = jsRuntime;
        }

        [HttpGet("{id}")]
        [Consumes("application/json")]
        public async Task<int> Get(int id)
        {
            try
            {

                //var re = Request;
                //var headers = re.Headers;
            if(_httpContextAccessor?.HttpContext == null)
            {
                await _jsRuntime.ToastrError("No header detected");
            }

            var headers = _httpContextAccessor.HttpContext.Request.Headers;
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

            return postCount;
            }catch(Exception ex)
            {
                return 0;
            }
        }
    }
}