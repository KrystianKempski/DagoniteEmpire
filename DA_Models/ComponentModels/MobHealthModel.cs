using DA_Models.CharacterModels;

namespace DA_Models.ComponentModels
{
    public class MobHealthModel : HealthModel
    {
        private MobDTO Mob;

        public MobHealthModel(MobDTO mob) : base(null)
        {
            Mob = mob;
        }
        public override int MaxWounds
        {
            get => Mob.MaxWounds;
        }
        public override int CurrentWounds
        {
            get => Mob.CurrentWounds;
        }
        public override int HealingModyfier
        {
            get => 100;
        }
        public override void AddWound(WoundDTO wound)
        {
            // Mob HP is a pool: fight persistence writes CurrentWounds directly.
        }
        public override void FillPropertiesContainer(IEnumerable<WoundDTO>? properties)
        {
            // ignore this
        }

        /// <summary>Successful pain resistance ignores one third of incoming damage, rounded to nearest whole.</summary>
        public static int DamageIgnoredByPainResistance(int damage)
        {
            if (damage <= 0)
                return 0;
            return (int)Math.Round(damage / 3.0, MidpointRounding.AwayFromZero);
        }

        public static int ApplyIncomingDamage(int damage, bool painResistanceSuccess)
        {
            if (damage <= 0)
                return 0;
            if (!painResistanceSuccess)
                return damage;
            return Math.Max(0, damage - DamageIgnoredByPainResistance(damage));
        }

        /// <summary>Remaining HP as a 0–1 ratio. Wounds past max count as 0 HP.</summary>
        public static double RemainingHpRatio(int currentWounds, int maxWounds)
        {
            if (maxWounds <= 0)
                return 0;
            var remaining = Math.Max(0, maxWounds - currentWounds);
            return (double)remaining / maxWounds;
        }

        /// <summary>
        /// Attack/defence penalty from remaining HP:
        /// ≥75% → 0, ≥50% → 2, ≥25% → 4, otherwise 6.
        /// </summary>
        public static int CombatPenalty(int currentWounds, int maxWounds)
        {
            var ratio = RemainingHpRatio(currentWounds, maxWounds);
            if (ratio >= 0.75)
                return 0;
            if (ratio >= 0.50)
                return 2;
            if (ratio >= 0.25)
                return 4;
            return 6;
        }

        public static string FormatHpLog(int currentWounds, int maxWounds)
        {
            var remaining = Math.Max(0, maxWounds - currentWounds);
            var percent = maxWounds <= 0 ? 0 : (int)Math.Round(100.0 * remaining / maxWounds);
            var penalty = CombatPenalty(currentWounds, maxWounds);
            var penaltyText = penalty == 0
                ? "no attack/defence penalty"
                : $"-{penalty} attack/defence";
            return $"HP {remaining}/{maxWounds} ({percent}%, {penaltyText})";
        }
    }
}
