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
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using DagoniteEmpire.Service;
using DagoniteEmpire.Service.IService;


namespace RichTextEditor.Data
{
    [ApiController]
    [Route("api/[controller]")]
    public class CharacterPostsController : ControllerBase
    {
        private readonly IPostRepository _postRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenService _tokenService;

        public CharacterPostsController(IHttpContextAccessor httpContextAccessor, IPostRepository postRepository, ITokenService tokenService)
        {
            _postRepository = postRepository;
            _httpContextAccessor = httpContextAccessor;
            _tokenService = tokenService;
        }


        [HttpPost("login")]
        [Consumes("application/json")]
        public async Task<string> LoginJwt()
        {
            return _tokenService.GenerateToken(); 
        }


        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Consumes("application/json")]
        public async Task<int> Get(int id)
        {
            try
            {
                if (_httpContextAccessor?.HttpContext == null)
                {
                    return -1;
                }

            var headers = _httpContextAccessor.HttpContext.Request.Headers;

            if (_httpContextAccessor.HttpContext.Request.Headers == null)
            {
                return -2;
            }
            var dateFrom = headers["dateFrom"];
            var dateTo = headers["dateTo"];
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
                return -3;
            }
        }
    }
}