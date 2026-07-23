using Abp.Domain.Entities;
using DA_DataAccess.BaronyData;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Chat;
using DA_DataAccess.Scribe;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext, IDataProtectionKeyContext
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
        public DbSet<Language> Languages { get; set; }
        public DbSet<ImageFile> ImageFiles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
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
        public DbSet<BattleMap> BattleMaps { get; set; }
        public DbSet<BattleEvent> BattleEvents { get; set; }
        public DbSet<WealthRecord> WealthRecords { get; set; }

        // SCRIBE - AI Memory System
        public DbSet<ScribeMemory> ScribeMemories { get; set; }
        public DbSet<ScribeChunk> ScribeChunks { get; set; }
        public DbSet<ScribeConversation> ScribeConversations { get; set; }
        public DbSet<ScribeMessage> ScribeMessages { get; set; }

        // BARONIA - warstwa zarządzania baronią
        public DbSet<Barony> Baronies { get; set; }
        public DbSet<Advisor> Advisors { get; set; }
        public DbSet<AvailableAdvisor> AvailableAdvisors { get; set; }
        public DbSet<BaronyBuilding> BaronyBuildings { get; set; }
        public DbSet<SocialGroupRelation> SocialGroupRelations { get; set; }
        public DbSet<Decree> Decrees { get; set; }
        public DbSet<BaronyEvent> BaronyEvents { get; set; }
        public DbSet<CommunityModifier> CommunityModifiers { get; set; }
        public DbSet<BaronInfluenceModifier> BaronInfluenceModifiers { get; set; }
        public DbSet<AdvisorInfluenceModifier> AdvisorInfluenceModifiers { get; set; }
        public DbSet<Fief> Fiefs { get; set; }
        public DbSet<TerrainTile> TerrainTiles { get; set; }
        public DbSet<TerrainMapDomain> TerrainMapDomains { get; set; }
        public DbSet<TerrainImprovement> TerrainImprovements { get; set; }
        public DbSet<BaronyProject> BaronyProjects { get; set; }
        public DbSet<BaronyRelation> BaronyRelations { get; set; }
        public DbSet<BaronyRelationModifier> BaronyRelationModifiers { get; set; }
        public DbSet<BaronySeat> BaronySeats { get; set; }
        public DbSet<SeatRoom> SeatRooms { get; set; }
        public DbSet<SeatRoomTrait> SeatRoomTraits { get; set; }
        public DbSet<SeatTile> SeatTiles { get; set; }
        public DbSet<SeatPurposeTemplate> SeatPurposeTemplates { get; set; }
        public DbSet<BaronyResourceSource> BaronyResourceSources { get; set; }
        public DbSet<BaronPurseSource> BaronPurseSources { get; set; }
        public DbSet<BaronPhpSource> BaronPhpSources { get; set; }
        public DbSet<BaronArtifact> BaronArtifacts { get; set; }
        public DbSet<BaronTimeModifier> BaronTimeModifiers { get; set; }
        public DbSet<BaronTimeAction> BaronTimeActions { get; set; }
        public DbSet<BaronLetterThread> BaronLetterThreads { get; set; }
        public DbSet<BaronLetterMessage> BaronLetterMessages { get; set; }
        public DbSet<BaronyUnit> BaronyUnits { get; set; }
        public DbSet<BuildingTemplate> BuildingTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

            if (!isSqlite)
            {
                // Enable pgvector extension for SCRIBE (PostgreSQL only)
                modelBuilder.HasPostgresExtension("vector");
            }

            modelBuilder.Entity<BaronyRelationModifier>(entity =>
            {
                entity.HasOne(m => m.Relation)
                    .WithMany(r => r.Modifiers)
                    .HasForeignKey(m => m.RelationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BaronyRelation>(entity =>
            {
                entity.HasIndex(e => e.FiefId);
            });

            modelBuilder.Entity<BaronPhpSource>(entity =>
            {
                entity.HasIndex(e => e.BaronyId);
            });

            modelBuilder.Entity<BaronArtifact>(entity =>
            {
                entity.HasIndex(e => e.BaronyId);
                entity.HasIndex(e => e.SeatRoomId);
            });

            modelBuilder.Entity<BaronTimeModifier>(entity =>
            {
                entity.HasIndex(e => e.BaronyId);
            });

            modelBuilder.Entity<BaronTimeAction>(entity =>
            {
                entity.HasIndex(e => e.BaronyId);
            });

            modelBuilder.Entity<BaronLetterThread>(entity =>
            {
                entity.HasIndex(e => e.BaronyId);
                entity.HasIndex(e => e.RelationId);
                entity.HasMany(e => e.Messages)
                    .WithOne(e => e.Thread!)
                    .HasForeignKey(e => e.ThreadId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BaronLetterMessage>(entity =>
            {
                entity.HasIndex(e => e.ThreadId);
            });

            modelBuilder.Entity<BaronyUnit>(entity =>
            {
                entity.HasIndex(e => e.BaronyId);
            });

            modelBuilder.Entity<BaronyProject>(entity =>
            {
                entity.HasIndex(e => e.UnitId);
            });

            // Configure SCRIBE entities
            modelBuilder.Entity<ScribeChunk>(entity =>
            {
                if (!isSqlite)
                {
                    entity.Property(e => e.Embedding)
                        .HasColumnType("vector(768)");

                    entity.HasIndex(e => e.Embedding)
                        .HasMethod("hnsw")
                        .HasOperators("vector_cosine_ops");
                }
                else
                {
                    entity.Ignore(e => e.Embedding);
                }

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

            modelBuilder.Entity<TerrainTile>(entity =>
            {
                entity.HasIndex(e => new { e.BaronyId, e.X, e.Y }).IsUnique();
            });

            modelBuilder.Entity<SeatTile>(entity =>
            {
                entity.HasIndex(e => new { e.SeatId, e.Level, e.X, e.Y }).IsUnique();
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
