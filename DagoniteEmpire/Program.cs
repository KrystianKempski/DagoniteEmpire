using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using DA_DataAccess.Data;
using DA_Business.Repository.CharacterReps;
using DA_Business.Repository.CharacterReps.IRepository;
using DagoniteEmpire.Service.IService;
using DagoniteEmpire.Service;
using DagoniteEmpire.Account;
using MudBlazor.Services;
using NLog.Web;
using DagoniteEmpire.Middleware;
using DA_Business.Services.Interfaces;
using DA_DataAccess;
using Microsoft.AspNetCore.Identity.UI.Services;
using DagoniteEmpire.Helper;
using DA_Models.CharacterModels;
using DA_Models.ChatModels;
using MudBlazor;
using DA_Business.Repository.ChatRepos;
using DA_Business.Services.Interfaces;
using DA_Business.Services;
using Cropper.Blazor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using DagoniteEmpire;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DA_Models;
using MimeKit;
using DA_Scribe.Extensions;
using Microsoft.AspNetCore.DataProtection;


public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        //builder.Services.AddAuthentication();

        builder.Services.AddRazorPages();

        builder.Services.AddSignalR();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();

        //identity 
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        var bearerAuthenticationSettings = new BearerAuthenticationSettings();
        builder.Configuration.GetSection("Authentication:Schemes:Bearer").Bind(bearerAuthenticationSettings);
        builder.Services.AddSingleton(bearerAuthenticationSettings);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = bearerAuthenticationSettings.ValidIssuer,
                    ValidAudience = bearerAuthenticationSettings.ValidAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(bearerAuthenticationSettings.JwtKey)),
                };
            })
            .AddIdentityCookies();               

        builder.Services.AddAuthorization();


        builder.Services.AddMudServices(c => { c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight; });
        builder.Host.UseNLog();


        /// DB context 
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                                    npgsqlOptions => 
                                    {
                                        npgsqlOptions.EnableRetryOnFailure();
                                        npgsqlOptions.UseVector(); // Enable pgvector for SCRIBE
                                    });
            if (builder.Environment.IsDevelopment())
            {
                options.EnableDetailedErrors();
            }
        });
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddSignInManager()
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext>>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Persist Data Protection keys in PostgreSQL so auth cookies stay valid
        // across restarts/redeploys and are shared between instances.
        builder.Services.AddDataProtection()
            .PersistKeysToDbContext<ApplicationDbContext>()
            .SetApplicationName("DagoniteEmpire");


        builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
            cfg.AddMaps(typeof(DA_Business.Mapper.MappingProfile).Assembly);
        });

        builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
        builder.Services.AddScoped<IMobRepository, MobRepository>();
        builder.Services.AddScoped<IAttributeRepository, AttributeRepository>();
        builder.Services.AddScoped<ISpecialSkillRepository, SpecialSkillRepository>();
        builder.Services.AddScoped<IBaseSkillRepository, BaseSkillRepository>();
        builder.Services.AddScoped<ITraitRepository<TraitCharacterDTO>, TraitCharacterRepository>();
        builder.Services.AddScoped<ITraitRepository<TraitRaceDTO>, TraitRaceRepository>();
        builder.Services.AddScoped<ITraitRepository<TraitEquipmentDTO>, TraitEquipmentRepository>();
        builder.Services.AddScoped<ITraitRepository<TraitProfessionDTO>, TraitProfessionRepository>();
        builder.Services.AddScoped<IBonusRepository, BonusRepository>();
        builder.Services.AddScoped<IRaceRepository, RaceRepository>();
        builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
        builder.Services.AddScoped<IWoundRepository, WoundRepository>();
        builder.Services.AddScoped<IProfessionRepository, ProfessionRepository>();
        builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        builder.Services.AddScoped<IEquipmentSlotRepository, EquipmentSlotRepository>();
        builder.Services.AddScoped<IWealthRecordRepository, WealthRecordRepository>();
        builder.Services.AddScoped<ISpellCircleRepository, SpellCircleRepository>();
        builder.Services.AddScoped<ISpellSlotRepository, SpellSlotRepository>();
        builder.Services.AddScoped<ISpellRepository, SpellRepository>();
        builder.Services.AddScoped<IPostRepository, PostRepository>();
        builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
        builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
        builder.Services.AddScoped<IBattlePhaseRepository, BattlePhaseRepository>();
        builder.Services.AddScoped<IBattleMapRepository, BattleMapRepository>();
        builder.Services.AddScoped<IBattleEventRepository, BattleEventRepository>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ICampaignSummaryService, CampaignSummaryService>();
        builder.Services.AddScoped<CallbackService>();
        builder.Services.AddScoped<IFileUpload, FileUpload>();
        builder.Services.AddScoped<IDbInitializer, DbInitializer>();
        builder.Services.AddScoped<ErrorHandlingMiddleware>();
        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<IWikiAccessService, WikiAccessService>();
        builder.Services.AddSingleton<IWikiLinkService, WikiLinkService>();
        builder.Services.AddScoped<WikiStaticFileMiddleware>();
        builder.Services.AddScoped<ITokenService,TokenService>();
        builder.Services.AddTransient<IChatManager, ChatManager>();
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

        // SCRIBE - AI Memory System
        builder.Services.AddScribe(builder.Configuration);

        builder.Services.AddCropper();
        builder.Services.AddServerSideBlazor()
            .AddCircuitOptions(options =>
            {
                options.DetailedErrors = builder.Environment.IsDevelopment()
                    || builder.Configuration.GetValue<bool>("DetailedErrors");
            })
            .AddHubOptions(options =>
            {
                options.MaximumReceiveMessageSize = 1024 * 1024;
            });

        builder.Services.AddControllersWithViews();
        builder.Services.Configure<EmailConfiguration>(options =>
        {
            builder.Configuration.GetSection("Email").Bind(options);
        });

        var app = builder.Build();
        app.UsePathBase("/");
        //app.UseStatusCodePages();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }
        else
        {
            app.UseMigrationsEndPoint();

        }
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<WikiStaticFileMiddleware>();
        app.UseStaticFiles();
        //app.MapStaticAssets();


        app.UseAntiforgery();
        //seed database
        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            await dbInitializer.Initialize();
        }

        app.MapHub<ChatHub>(ChatHub.HubUrl);
        app.MapControllers()
            .WithStaticAssets();
            //.RequireAuthorization(new AuthorizeAttribute
            //{
            //    AuthenticationSchemes = "JwtBearerDefaults.AuthenticationScheme",
            //    Policy = "PlayerPolicy"
            //});

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .WithStaticAssets();

        app.MapHealthChecks("/healthz");
        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.Run();

    }
}

