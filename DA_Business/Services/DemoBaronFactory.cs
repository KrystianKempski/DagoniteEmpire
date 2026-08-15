using DA_Common;
using DA_DataAccess.CharacterClasses;
using DA_DataAccess.Data;
using DA_Models;
using Microsoft.EntityFrameworkCore;
using Attribute = DA_DataAccess.CharacterClasses.Attribute;

namespace DA_Business.Services
{
    /// <summary>
    /// Builds the demo baron character from the embedded snapshot (<see cref="DemoBaronSeed"/>),
    /// resolving profession / race / languages by name against the target database. Shared by the
    /// per-session demo provisioning (and the startup cleanup) so the demo baron is always the
    /// snapshot character, independent of any pre-existing DB record on dev/production.
    /// </summary>
    public static class DemoBaronFactory
    {
        /// <summary>
        /// Materialises a new (unsaved) demo baron under <paramref name="userName"/> from the snapshot.
        /// Missing profession/race lookups are created once; the caller adds the character and saves.
        /// </summary>
        public static async Task<Character> BuildAsync(
            ApplicationDbContext ctx, string userName, DemoBaronSeed.CharacterData snapshot)
        {
            int professionId = 0;
            if (snapshot.Profession is { } prof)
            {
                var profession = await ctx.Professions.FirstOrDefaultAsync(p => p.Name == prof.Name);
                if (profession is null)
                {
                    profession = new Profession
                    {
                        Name = prof.Name,
                        Description = prof.Description ?? string.Empty,
                        RelatedAttributeName = prof.RelatedAttributeName ?? string.Empty,
                        ClassLevel = prof.ClassLevel,
                        CurrentFocusPoints = prof.CurrentFocusPoints,
                        IsApproved = prof.IsApproved,
                        IsUniversal = prof.IsUniversal,
                        CasterType = (SpellcasterType)prof.CasterType,
                    };
                    ctx.Professions.Add(profession);
                    await ctx.SaveChangesAsync();
                }
                professionId = profession.Id;
            }

            int? raceId = null;
            if (snapshot.Race is { } raceData)
            {
                var race = await ctx.Races.FirstOrDefaultAsync(r => r.Name == raceData.Name);
                if (race is null)
                {
                    race = new Race
                    {
                        Name = raceData.Name,
                        Index = raceData.Index,
                        Description = raceData.Description ?? string.Empty,
                        RaceApproved = raceData.RaceApproved,
                    };
                    ctx.Races.Add(race);
                    await ctx.SaveChangesAsync();
                }
                raceId = race.Id;
            }

            var created = new Character
            {
                UserName = userName,
                Relation = (Relation)snapshot.Relation,
                NPCName = snapshot.NpcName,
                Description = snapshot.Description,
                Age = snapshot.Age,
                ImageUrl = snapshot.ImageUrl,
                IconUrl = snapshot.IconUrl,
                NPCType = snapshot.NpcType ?? SD.NPCType.Duke,
                AttributePoints = snapshot.AttributePoints,
                CurrentExpPoints = snapshot.CurrentExpPoints,
                UsedExpPoints = snapshot.UsedExpPoints,
                TraitBalance = snapshot.TraitBalance,
                WeaponSet = snapshot.WeaponSet,
                DateNumber = snapshot.DateNumber,
                RaceId = raceId,
                ProfessionId = professionId,
                IsApproved = true,
                Attributes = snapshot.Attributes
                    .OrderBy(a => a.Index)
                    .Select(a => new Attribute
                    {
                        Name = a.Name,
                        FeatureType = a.FeatureType,
                        Index = a.Index,
                        BaseBonus = a.BaseBonus,
                        RaceBonus = a.RaceBonus,
                        GearBonus = a.GearBonus,
                        TraitBonus = a.TraitBonus,
                        OtherBonuses = a.OtherBonuses,
                        TempBonuses = a.TempBonuses,
                        HealthBonus = a.HealthBonus,
                    })
                    .ToList(),
                BaseSkills = snapshot.BaseSkills
                    .OrderBy(s => s.Index)
                    .Select(s => new BaseSkill
                    {
                        Name = s.Name,
                        FeatureType = s.FeatureType,
                        Index = s.Index,
                        BaseBonus = s.BaseBonus,
                        RaceBonus = s.RaceBonus,
                        GearBonus = s.GearBonus,
                        TraitBonus = s.TraitBonus,
                        OtherBonuses = s.OtherBonuses,
                        TempBonuses = s.TempBonuses,
                        HealthBonus = s.HealthBonus,
                        RelatedAttribute1 = s.RelatedAttribute1,
                        RelatedAttribute2 = s.RelatedAttribute2,
                    })
                    .ToList(),
                SpecialSkills = snapshot.SpecialSkills
                    .Select(s => new SpecialSkill
                    {
                        Name = s.Name,
                        FeatureType = s.FeatureType,
                        Index = s.Index,
                        BaseBonus = s.BaseBonus,
                        RaceBonus = s.RaceBonus,
                        GearBonus = s.GearBonus,
                        TraitBonus = s.TraitBonus,
                        OtherBonuses = s.OtherBonuses,
                        TempBonuses = s.TempBonuses,
                        HealthBonus = s.HealthBonus,
                        RelatedAttribute1 = s.RelatedAttribute1,
                        RelatedAttribute2 = s.RelatedAttribute2,
                        RelatedBaseSkillName = s.RelatedBaseSkillName,
                        ChosenAttribute = s.ChosenAttribute,
                        Editable = s.Editable,
                    })
                    .ToList(),
                EquipmentSlots = snapshot.EquipmentSlots
                    .Select(e => new EquipmentSlot
                    {
                        Count = e.Count,
                        EquipmentID = e.EquipmentID,
                        IsEquipped = e.IsEquipped,
                        SlotType = e.SlotType,
                    })
                    .ToList(),
            };

            if (snapshot.Languages.Length > 0)
            {
                var names = snapshot.Languages;
                created.Languages = await ctx.Languages
                    .Where(l => names.Contains(l.Name))
                    .ToListAsync();
            }

            return created;
        }
    }
}
