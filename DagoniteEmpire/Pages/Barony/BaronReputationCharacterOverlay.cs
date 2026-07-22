using DA_Business.Repository.CharacterReps.IRepository;
using DA_Common;
using DA_Common.Barony;
using DA_Models.BaronyModels;
using DA_Models.CharacterModels;
using DA_Models.ComponentModels;

namespace DagoniteEmpire.Pages.Barony
{
    /// <summary>
    /// Injects Prestige/Honor/Fear reputation skill bonuses as non-removable
    /// character traits (Traits tile). Not persisted to the database.
    /// </summary>
    public static class BaronReputationCharacterOverlay
    {
        public const string TraitDescrMarker = "Barony reputation";

        public static bool IsReputationTrait(TraitDTO? trait) =>
            trait is not null
            && !string.IsNullOrEmpty(trait.Descr)
            && trait.Descr.StartsWith(TraitDescrMarker, StringComparison.Ordinal);

        public static async Task ApplyAsync(
            AllParamsModel allParams,
            IBaronyRepository baronyRepo,
            int characterId)
        {
            // Clear any previous Temp-column overlay from the first implementation.
            allParams.ExternalSpecialSkillTempBonuses =
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            RemoveInjectedTraits(allParams.TraitsCharacter);

            if (characterId <= 0)
                return;

            var barony = await baronyRepo.GetByCharacterId(characterId);
            if (barony is null)
                return;

            var totals = await RefreshPhpTotalsAsync(baronyRepo, barony);
            foreach (var trait in BuildTraits(totals.Prestige, totals.Honor, totals.Fear))
                allParams.TraitsCharacter.Add(trait);
        }

        public static IReadOnlyList<TraitCharacterDTO> BuildTraits(int prestige, int honor, int fear)
        {
            var list = new List<TraitCharacterDTO>();
            TryAdd(list, BaronReputationTiers.ResolvePrestige(prestige), "Prestige");
            TryAdd(list, BaronReputationTiers.ResolveHonor(honor), "Honor");
            TryAdd(list, BaronReputationTiers.ResolveFear(fear), "Fear");
            return list;
        }

        private static void TryAdd(List<TraitCharacterDTO> list, ReputationTier tier, string ladder)
        {
            if (tier.SkillBonuses.Count == 0)
                return;

            var trait = new TraitCharacterDTO(isTemporary: false)
            {
                Id = 0,
                Name = $"{tier.Name}!",
                Descr = $"{TraitDescrMarker} · {ladder} (score threshold {tier.ThresholdLabel})",
                TraitValue = 0,
                TraitApproved = true,
                IsRemovable = false,
                IsUnique = true,
                Level = 0,
                Index = 10_000 + list.Count,
            };

            var i = 0;
            foreach (var (skill, value) in tier.SkillBonuses)
            {
                if (value == 0 || string.IsNullOrWhiteSpace(skill))
                    continue;

                trait.Bonuses.Add(new BonusDTO
                {
                    FeatureType = SD.FeatureSpecialSkill,
                    FeatureName = skill,
                    BonusValue = value,
                    Index = i++,
                    Description = string.Empty,
                });
            }

            if (trait.Bonuses.Count > 0)
                list.Add(trait);
        }

        private static void RemoveInjectedTraits(ICollection<TraitDTO> traits)
        {
            var stale = traits.Where(IsReputationTrait).ToList();
            foreach (var trait in stale)
                traits.Remove(trait);
        }

        private static async Task<PhpTotals> RefreshPhpTotalsAsync(
            IBaronyRepository baronyRepo,
            BaronyDTO barony)
        {
            var sources = await baronyRepo.GetBaronPhpSources(barony.Id);
            var artifacts = await baronyRepo.GetBaronArtifacts(barony.Id);
            var seat = await baronyRepo.EnsureSeat(barony.Id);
            var purposes = await baronyRepo.GetSeatPurposeTemplates(barony.Id);
            var seatContribution = BaronyCalc.SeatPhpContribution(seat, purposes);
            var itemsContribution = BaronyCalc.ArtifactsPhpContribution(artifacts, seat);
            var totals = BaronyCalc.SumPhpRows(BaronyCalc.BuildPhpRows(seatContribution, itemsContribution, sources));

            if (barony.Prestige == totals.Prestige
                && barony.Honor == totals.Honor
                && barony.Fear == totals.Fear)
                return totals;

            barony.Prestige = totals.Prestige;
            barony.Honor = totals.Honor;
            barony.Fear = totals.Fear;
            await baronyRepo.UpdateBarony(barony);
            return totals;
        }
    }
}
