using Abp.Domain.Entities;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
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
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
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
        public DbSet<EquipmentSlot> EquipmentSlots { get; set; }
        public DbSet<TraitCharacter> TraitsCharacter { get; set; }
        public DbSet<TraitRace> TraitsRace { get; set; }
        public DbSet<TraitEquipment> TraitsEquipment { get; set; }
        public DbSet<TraitProfession> TraitsProfession { get; set; }
        public DbSet<Wound> Wounds { get; set; }

        public DbSet<Profession> Professions { get; set; }
        //public DbSet<ProfessionSkill> ProfessionSkills { get; set; }
        public DbSet<SpellCircle> SpellCircles { get; set; }
        public DbSet<SpellSlot> SpellSlots { get; set; }
        public DbSet<Spell> Spells { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<ProfessionSkill>()
            //    .HasOne(a => a.ActiveProfession)
            //    .WithMany(y => y.ActiveSkills)
            //    .HasForeignKey(a => a.ActiveProfessionId).OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<ProfessionSkill>()
            //    .HasOne(a => a.PassiveProfession)
            //    .WithMany(y => y.PassiveSkills)
            //    .HasForeignKey(a => a.PassiveProfessionId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(d => d.FromUser)
                .WithMany(p => p.ChatMessagesFromUsers)
                .HasForeignKey(d => d.FromUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            modelBuilder.Entity<ChatMessage>()
                .HasOne(d => d.ToUser)
                .WithMany(p => p.ChatMessagesToUsers)
                .HasForeignKey(d => d.ToUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            //modelBuilder.Entity<Character>()
            //    .HasOne(d => d.Head)
            //    .WithOne(p => p.Characters)
            //    .HasForeignKey(d => d.ToUserId)
            //    .OnDelete(DeleteBehavior.ClientSetNull);

        }
    }
}
