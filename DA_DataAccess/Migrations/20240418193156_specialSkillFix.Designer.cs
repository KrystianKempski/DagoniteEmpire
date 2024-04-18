﻿// <auto-generated />
using System;
using DA_DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DA_DataAccess.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240418193156_specialSkillFix")]
    partial class specialSkillFix
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.15")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CharacterTraitAdv", b =>
                {
                    b.Property<int>("CharactersId")
                        .HasColumnType("integer");

                    b.Property<int>("TraitsAdvId")
                        .HasColumnType("integer");

                    b.HasKey("CharactersId", "TraitsAdvId");

                    b.HasIndex("TraitsAdvId");

                    b.ToTable("CharacterTraitAdv");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Attribute", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BaseBonus")
                        .HasColumnType("integer");

                    b.Property<int>("CharacterId")
                        .HasColumnType("integer");

                    b.Property<string>("FeatureType")
                        .HasColumnType("text");

                    b.Property<int>("GearBonus")
                        .HasColumnType("integer");

                    b.Property<int>("HealthBonus")
                        .HasColumnType("integer");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("OtherBonuses")
                        .HasColumnType("integer");

                    b.Property<int>("RaceBonus")
                        .HasColumnType("integer");

                    b.Property<int>("TempBonuses")
                        .HasColumnType("integer");

                    b.Property<int>("TraitBonus")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CharacterId");

                    b.ToTable("Attributes");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.BaseSkill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BaseBonus")
                        .HasColumnType("integer");

                    b.Property<int>("CharacterId")
                        .HasColumnType("integer");

                    b.Property<string>("FeatureType")
                        .HasColumnType("text");

                    b.Property<int>("GearBonus")
                        .HasColumnType("integer");

                    b.Property<int>("HealthBonus")
                        .HasColumnType("integer");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("OtherBonuses")
                        .HasColumnType("integer");

                    b.Property<int>("RaceBonus")
                        .HasColumnType("integer");

                    b.Property<string>("RelatedAttribute1")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RelatedAttribute2")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TempBonuses")
                        .HasColumnType("integer");

                    b.Property<int>("TraitBonus")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CharacterId");

                    b.ToTable("BaseSkills");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Bonus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("BonusValue")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("FeatureName")
                        .HasColumnType("text");

                    b.Property<string>("FeatureType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<int>("TraitId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TraitId");

                    b.ToTable("Bonuses");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Character", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Age")
                        .HasColumnType("integer");

                    b.Property<int>("AttributePoints")
                        .HasColumnType("integer");

                    b.Property<string>("Class")
                        .HasColumnType("text");

                    b.Property<int>("CurrentExpPoints")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("text");

                    b.Property<string>("NPCName")
                        .HasColumnType("text");

                    b.Property<string>("NPCType")
                        .HasColumnType("text");

                    b.Property<int?>("RaceId")
                        .HasColumnType("integer");

                    b.Property<int>("TraitBalance")
                        .HasColumnType("integer");

                    b.Property<int>("UsedExpPoints")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RaceId");

                    b.ToTable("Characters");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Race", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("RaceApproved")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.ToTable("Races");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.SpecialSkill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("BaseBonus")
                        .HasColumnType("integer");

                    b.Property<int>("CharacterId")
                        .HasColumnType("integer");

                    b.Property<string>("ChosenAttribute")
                        .HasColumnType("text");

                    b.Property<bool>("Editable")
                        .HasColumnType("boolean");

                    b.Property<string>("FeatureType")
                        .HasColumnType("text");

                    b.Property<int>("GearBonus")
                        .HasColumnType("integer");

                    b.Property<int>("HealthBonus")
                        .HasColumnType("integer");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("OtherBonuses")
                        .HasColumnType("integer");

                    b.Property<int>("RaceBonus")
                        .HasColumnType("integer");

                    b.Property<string>("RelatedAttribute1")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RelatedAttribute2")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RelatedBaseSkillName")
                        .HasColumnType("text");

                    b.Property<int>("TempBonuses")
                        .HasColumnType("integer");

                    b.Property<int>("TraitBonus")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CharacterId");

                    b.ToTable("SpecialSkills");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Trait", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Descr")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<bool>("IsUnique")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("SummaryDescr")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("TraitApproved")
                        .HasColumnType("boolean");

                    b.Property<string>("TraitType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("TraitValue")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Traits");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Trait");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("DA_DataAccess.ImageFile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ImageName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("fileData")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.HasKey("Id");

                    b.ToTable("ImageFiles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityUser");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("RaceTraitRace", b =>
                {
                    b.Property<int>("RacesId")
                        .HasColumnType("integer");

                    b.Property<int>("TraitsId")
                        .HasColumnType("integer");

                    b.HasKey("RacesId", "TraitsId");

                    b.HasIndex("TraitsId");

                    b.ToTable("RaceTraitRace");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.TraitAdv", b =>
                {
                    b.HasBaseType("DA_DataAccess.CharacterClasses.Trait");

                    b.HasDiscriminator().HasValue("TraitAdv");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.TraitRace", b =>
                {
                    b.HasBaseType("DA_DataAccess.CharacterClasses.Trait");

                    b.HasDiscriminator().HasValue("TraitRace");
                });

            modelBuilder.Entity("DA_DataAccess.ApplicationUser", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityUser");

                    b.Property<string>("PlayerType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasDiscriminator().HasValue("ApplicationUser");
                });

            modelBuilder.Entity("CharacterTraitAdv", b =>
                {
                    b.HasOne("DA_DataAccess.CharacterClasses.Character", null)
                        .WithMany()
                        .HasForeignKey("CharactersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DA_DataAccess.CharacterClasses.TraitAdv", null)
                        .WithMany()
                        .HasForeignKey("TraitsAdvId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Attribute", b =>
                {
                    b.HasOne("DA_DataAccess.CharacterClasses.Character", "Character")
                        .WithMany("Attributes")
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.BaseSkill", b =>
                {
                    b.HasOne("DA_DataAccess.CharacterClasses.Character", "Character")
                        .WithMany("BaseSkills")
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Bonus", b =>
                {
                    b.HasOne("DA_DataAccess.CharacterClasses.Trait", "Trait")
                        .WithMany("Bonuses")
                        .HasForeignKey("TraitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Trait");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Character", b =>
                {
                    b.HasOne("DA_DataAccess.CharacterClasses.Race", "Race")
                        .WithMany("Characters")
                        .HasForeignKey("RaceId");

                    b.Navigation("Race");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.SpecialSkill", b =>
                {
                    b.HasOne("DA_DataAccess.CharacterClasses.Character", "Character")
                        .WithMany("SpecialSkills")
                        .HasForeignKey("CharacterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Character");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RaceTraitRace", b =>
                {
                    b.HasOne("DA_DataAccess.CharacterClasses.Race", null)
                        .WithMany()
                        .HasForeignKey("RacesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DA_DataAccess.CharacterClasses.TraitRace", null)
                        .WithMany()
                        .HasForeignKey("TraitsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Character", b =>
                {
                    b.Navigation("Attributes");

                    b.Navigation("BaseSkills");

                    b.Navigation("SpecialSkills");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Race", b =>
                {
                    b.Navigation("Characters");
                });

            modelBuilder.Entity("DA_DataAccess.CharacterClasses.Trait", b =>
                {
                    b.Navigation("Bonuses");
                });
#pragma warning restore 612, 618
        }
    }
}
