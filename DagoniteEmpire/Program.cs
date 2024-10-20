using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DA_DataAccess.Data;
using DA_Business.Repository.CharacterReps;
using DA_Business.Repository.CharacterReps.IRepository;
using DagoniteEmpire.Service.IService;
using DagoniteEmpire.Service;
using Syncfusion.Blazor;
using Syncfusion.Blazor.RichTextEditor;
using MudBlazor.Services;
using DA_Common;
using NLog.Web;
using DagoniteEmpire.Middleware;
using DA_DataAccess;
using Microsoft.AspNetCore.Identity.UI.Services;
using DagoniteEmpire.Helper;
using Microsoft.Extensions.Options;
using DA_Models.CharacterModels;
using DA_Models.ChatModels;
using MudBlazor;
using DA_Business.Repository.ChatRepos;
using DA_Business.Services.Interfaces;
using DA_Business.Services;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();

builder.Services.AddRazorPages();

builder.Services.AddSignalR();
builder.Services.AddServerSideBlazor();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddMudServices(c => { c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight; });
builder.Host.UseNLog();


/// DB context 
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableDetailedErrors();
    //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    //options.EnableSensitiveDataLogging();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(config =>
    {
        config.SignIn.RequireConfirmedAccount = true;
    }).AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();

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
builder.Services.AddScoped<ISpellCircleRepository, SpellCircleRepository>();
builder.Services.AddScoped<ISpellSlotRepository, SpellSlotRepository>();
builder.Services.AddScoped<ISpellRepository, SpellRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IChapterRepository, ChapterRepository>();
builder.Services.AddScoped<ICampaignRepository, CampaignRepository>();
builder.Services.AddScoped<IBattlePhaseRepository, BattlePhaseRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddSingleton<PanelActivation>();
builder.Services.AddScoped<IFileUpload, FileUpload>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<ErrorHandlingMiddleware>();
builder.Services.AddTransient<IChatManager, ChatManager>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<EmailConfiguration>(options =>
{
    builder.Configuration.GetSection("Email").Bind(options);
});

var app = builder.Build();


Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NAaF5cWWJCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdnWXdecnVTQmFZUUJ3Xko=");
//Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["SyncfusionKey"]);



if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseHttpsRedirection();

//chat endpoints

app.UseStaticFiles();

app.UseRouting();

SeedDatabase();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>(ChatHub.HubUrl);
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();



void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}