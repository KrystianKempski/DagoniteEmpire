using Abp.Domain.Entities;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Scribe;
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
        public DbSet<Mob> Mobs { get; set; }        
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
        public DbSet<SpellCircle> SpellCircles { get; set; }
        public DbSet<SpellSlot> SpellSlots { get; set; }
        public DbSet<Spell> Spells { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<BattlePhase> BattlePhases { get; set; }
        public DbSet<WealthRecord> WealthRecords { get; set; }

        // SCRIBE - AI Memory System
        public DbSet<ScribeMemory> ScribeMemories { get; set; }
        public DbSet<ScribeChunk> ScribeChunks { get; set; }
        public DbSet<ScribeConversation> ScribeConversations { get; set; }
        public DbSet<ScribeMessage> ScribeMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enable pgvector extension for SCRIBE
            modelBuilder.HasPostgresExtension("vector");
            
            // Configure SCRIBE entities
            modelBuilder.Entity<ScribeChunk>(entity =>
            {
                // Vector column configuration
                entity.Property(e => e.Embedding)
                    .HasColumnType("vector(768)");
                
                // Index for vector similarity search using HNSW
                entity.HasIndex(e => e.Embedding)
                    .HasMethod("hnsw")
                    .HasOperators("vector_cosine_ops");
                
                // Indexes for filtering
                entity.HasIndex(e => e.CampaignId);
                entity.HasIndex(e => e.MemoryType);
                entity.HasIndex(e => e.IsPublic);
            });
            
            modelBuilder.Entity<ScribeMemory>(entity =>
            {
                entity.HasIndex(e => e.SourceCampaignId);
                entity.HasIndex(e => e.Type);
            });
            
            modelBuilder.Entity<ScribeConversation>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CampaignId);
            });

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
