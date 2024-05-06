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
        public DbSet<Trait> Traits { get; set; }
        public DbSet<Bonus> Bonuses { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<ImageFile> ImageFiles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<TraitAdv> TraitsAdv { get; set; }
        public DbSet<TraitRace> TraitsRace { get; set; }
        public DbSet<TraitEquipment> TraitsEquipment { get; set; }

        public DbSet<Profession> Professions { get; set; }
        public DbSet<ProfessionSkill> ProfessionSkills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProfessionSkill>()
                .HasOne(a => a.ActiveProfession)
                .WithMany(y => y.ActiveSkills)
                .HasForeignKey(a => a.ActiveProfessionId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProfessionSkill>()
                .HasOne(a => a.PassiveProfession)
                .WithMany(y => y.PassiveSkills)
                .HasForeignKey(a => a.PassiveProfessionId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
