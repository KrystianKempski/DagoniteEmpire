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
using DA_Scribe.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


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



        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>()
            .AddCheck<ScribeOllamaHealthCheck>(
                name: "scribe-ollama",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "scribe", "ollama" });

        // OpenTelemetry tracing for the Scribe stack. Only activates an OTLP
        // exporter when OTEL_EXPORTER_OTLP_ENDPOINT is set, so local/dev runs
        // pay no cost and don't need a collector.
        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(rb => rb.AddService(
                    serviceName: "DagoniteEmpire",
                    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0"))
                .WithTracing(t => t
                    .AddSource(DA_Scribe.Diagnostics.ScribeTelemetry.SourceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter());
        }

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
        builder.Services.AddScoped<ITokenService,TokenService>();
        builder.Services.AddTransient<IChatManager, ChatManager>();
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        builder.Services.AddHttpClient();
        builder.Services.AddHttpContextAccessor();

        // SCRIBE - AI Memory System
        builder.Services.AddScribe(builder.Configuration);

        // Rate limiting for SCRIBE endpoints (protects the GPU host from per-user bursts).
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            static string ResolvePartitionKey(HttpContext ctx)
            {
                var name = ctx.User.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(name))
                    return "u:" + name;

                var ip = ctx.Connection.RemoteIpAddress?.ToString();
                return "ip:" + (string.IsNullOrWhiteSpace(ip) ? "unknown" : ip);
            }

            // Per-user fixed window: 20 requests / minute.
            // Unauthenticated clients fall back to a per-IP bucket so a single
            // misbehaving anonymous host cannot exhaust everyone's budget.
            options.AddPolicy("scribe-query", httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    });
            });

            // Ingest / import are expensive — much tighter bucket.
            options.AddPolicy("scribe-ingest", httpContext =>
            {
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ResolvePartitionKey(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    });
            });
        });
        builder.Services.AddScoped<DagoniteEmpire.Service.Scribe.IScribeAgentService, DagoniteEmpire.Service.Scribe.ScribeAgentService>();
        builder.Services.AddHostedService<DagoniteEmpire.Service.Scribe.ScribeRetentionService>();

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
        app.UseStaticFiles();
        //app.MapStaticAssets();


        app.UseAntiforgery();
        app.UseRateLimiter();
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
        app.MapHealthChecks("/health/scribe", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("scribe"),
        });
        // Add additional endpoints required by the Identity /Account Razor components.
        app.MapAdditionalIdentityEndpoints();

        app.Run();

    }
}

