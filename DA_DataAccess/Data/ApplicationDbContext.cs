using DA_DataAccess.CharacterClasses;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Character> Characters { get; set; }

        public DbSet<Attribute> Attributes { get; set; }

        public DbSet<BaseSkill> BaseSkills { get; set; }
        public DbSet<SpecialSkill> SpecialSkills { get; set; }

        public DbSet<ImageFile> ImageFiles { get; set; }

       // public DbSet<ApplicationUser> ApplicationUsers { get; set; }

    //    protected override void OnModelCreating(ModelBuilder modelBuilder)
    //    {
    //        base.OnModelCreating(modelBuilder);

    //        modelBuilder.Entity<Trait>().ToTable(nameof(Trait))
    //            .HasMany(a => a.Bonuses)
    //            .WithOne()
    //            .HasForeignKey(e => e.TraitId)
    //            .IsRequired();


    //        modelBuilder.Entity<Feature>().ToTable(nameof(Feature))
    //            .HasMany(a => a.TraitBonusesRelated)
    //            .WithOne()
    //            .HasForeignKey(e=> e.FeatureId)
    //            .IsRequired(false);
    //    }
    }
}
