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
using DA_Common;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSyncfusionBlazor();

/// DB context 
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString)); // or builder.Configuration.GetConnectionString("DefaultConnection")
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
//          .AddJsonFile("licenses.json", optional: true, reloadOnChange: true); ;
builder.Configuration.AddJsonFile("licenses.json",
        optional: true,
        reloadOnChange: true);
//AddJsonFile("licenses.json");

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddDefaultUI().AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IAttributeRepository, AttributeRepository>();
builder.Services.AddScoped<ISpecialSkillRepository, SpecialSkillRepository>();
builder.Services.AddScoped<IBaseSkillRepository, BaseSkillRepository>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();
//builder.Services.AddSingleton<IConfiguration>(provider => new ConfigurationBuilder()
//            .AddEnvironmentVariables()
//            .AddJsonFile("licenses.json", optional: true, reloadOnChange: true)
//            .Build());

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();



var app = builder.Build();


Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1NAaF1cXmhIfEx1RHxQdld5ZFRHallYTnNWUj0eQnxTdEFjW39fcXJWTmFYVk1/Vg=="); ;




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

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

SeedDatabase();
app.UseAuthentication();
app.UseAuthorization();

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