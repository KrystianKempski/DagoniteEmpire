using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DA_DataAccess.Data;
using DA_Business.Repository.CharacterReps;
using DA_Business.Repository.CharacterReps.IRepository;
using DagoniteEmpire.Service.IService;
using DagoniteEmpire.Service;
using DagoniteEmpire.Account;
using Syncfusion.Blazor;
using MudBlazor.Services;
using NLog.Web;
using DagoniteEmpire.Middleware;
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


public class Program
{
    public static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddAuthentication();

        builder.Services.AddRazorPages();

        builder.Services.AddSignalR();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddCascadingAuthenticationState();

        //identity 
        builder.Services.AddScoped<IdentityUserAccessor>();
        builder.Services.AddScoped<IdentityRedirectManager>();
        builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
            .AddIdentityCookies();


        builder.Services.AddSyncfusionBlazor();
        builder.Services.AddMudServices(c => { c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight; });
        builder.Host.UseNLog();


        /// DB context 
        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                                    options => options.EnableRetryOnFailure());
            options.EnableDetailedErrors();
        });
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddSignInManager()
            //.AddDefaultUI()
            .AddRoles<IdentityRole>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddRoleStore<RoleStore<IdentityRole, ApplicationDbContext>>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();



        builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

        //builder.Services.AddIdentity<ApplicationUser, IdentityRole>(config =>
        //    {
        //        config.SignIn.RequireConfirmedAccount = true;
        //    }).AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
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
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<CallbackService>();
        builder.Services.AddScoped<IFileUpload, FileUpload>();
        builder.Services.AddScoped<IDbInitializer, DbInitializer>();
        builder.Services.AddScoped<ErrorHandlingMiddleware>();
        builder.Services.AddTransient<IChatManager, ChatManager>();
        //    builder.Services.AddTransient<IEmailSender, EmailSender>();
        builder.Services.AddTransient<IEmailSender, EmailSender>();


        builder.Services.AddCropper();
        builder.Services.AddServerSideBlazor()
            .AddHubOptions(options =>
            {
                options.MaximumReceiveMessageSize = 320 * 1024 * 100;
            });

        builder.Services.AddControllersWithViews();
        builder.Services.Configure<EmailConfiguration>(options =>
        {
            builder.Configuration.GetSection("Email").Bind(options);
        });

        var app = builder.Build();
        //app.UseStatusCodePages();

        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NDaF5cWWtCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWH9ceHVRRWdYVUd3WUI=");
        //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["SyncfusionKey"]);

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
        app.MapStaticAssets();


        app.UseAntiforgery();
        //seed database
        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
            dbInitializer.Initialize();
        }

        app.MapHub<ChatHub>(ChatHub.HubUrl);
        app.MapControllers()
            .WithStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .WithStaticAssets();

        app.MapHealthChecks("/healthz");
        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.Run();

    }
}

