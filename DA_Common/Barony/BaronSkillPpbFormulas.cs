namespace DA_Common.Barony
{
    /// <summary>
    /// Baron Card — PPB influence from the baron's character skills and attributes.
    /// Skill names match CharacterSeeder / sheet labels.
    /// <see cref="Skills.Magic"/> is a custom Knowledge specialty (not in the default seeder).
    /// </summary>
    public static class BaronSkillPpbFormulas
    {
        public static class Skills
        {
            public const string PlantsAndMushrooms = "Plants and mushrooms";
            public const string AnimalsCare = "Animals care";
            public const string Beasts = "Beasts";
            public const string MathematicsAndLogic = "Mathematics and logic";
            public const string RacesAndNations = "Races and nations";
            public const string Trade = "Trade";
            public const string Craft = "Craft";
            public const string Bluff = "Bluff";
            public const string PublicSpeech = "Public speech";
            public const string SenseMotives = "Sense motives";
            public const string Intimidate = "Intimidate";
            public const string HistoryAndReligion = "History and religion";
            public const string Persuasion = "Persuasion";
            public const string Investigation = "Investigation";
            public const string Observation = "Observation";
            public const string Tracking = "Tracking";
            public const string Vigilance = "Vigilance";
            public const string Gambling = "Gambling";
            public const string Acting = "Acting";
            public const string Knowledge = "Knowledge";
            public const string FineArts = "Fine arts";
            public const string Linguistics = "Linguistics";
            public const string Diplomacy = "Diplomacy";
            /// <summary>Custom Knowledge specialty — 0 until present on the sheet.</summary>
            public const string Magic = "Magic";
            public const string Perception = "Perception";
            public const string Survival = "Survival";
            public const string Deceit = "Deceit";
            public const string StrategyAndTactics = "Strategy and tactics";
            public const string Inspire = "Inspire";
            public const string Geography = "Geography";
        }

        public static class Attrs
        {
            /// <summary>Permanent attribute modifier (<c>ModifierAbsolute</c>), not the raw score.</summary>
            public const string Intelligence = "Intelligence";
            /// <summary>Permanent attribute modifier (<c>ModifierAbsolute</c>), not the raw score.</summary>
            public const string Willpower = "Willpower";
        }

        public static PpbVector Compute(
            Func<string, decimal> specialSkill,
            Func<string, decimal> baseSkill,
            Func<string, decimal> attribute)
        {
            ArgumentNullException.ThrowIfNull(specialSkill);
            ArgumentNullException.ThrowIfNull(baseSkill);
            ArgumentNullException.ThrowIfNull(attribute);

            decimal Spec(string name) => specialSkill(name);
            decimal Base(string name) => baseSkill(name);
            decimal Attr(string name) => attribute(name);

            var v = new PpbVector();
            v.EnsureSize();

            v[Ppb.Food] = Avg3(
                Spec(Skills.PlantsAndMushrooms),
                Spec(Skills.AnimalsCare),
                Spec(Skills.Beasts));

            v[Ppb.Economy] = Avg3(
                Spec(Skills.MathematicsAndLogic),
                Spec(Skills.RacesAndNations),
                Spec(Skills.Trade));

            v[Ppb.Production] = Base(Skills.Craft) + Attr(Attrs.Intelligence);

            v[Ppb.Loyalty] = Avg3(
                Spec(Skills.Bluff),
                Spec(Skills.PublicSpeech),
                Spec(Skills.SenseMotives));

            v[Ppb.Stability] = Avg3(
                Spec(Skills.Intimidate),
                Spec(Skills.HistoryAndReligion),
                Spec(Skills.Persuasion));

            v[Ppb.Law] = Avg3(
                Spec(Skills.Investigation),
                Spec(Skills.Observation),
                Spec(Skills.Tracking));

            v[Ppb.Corruption] = -Avg3(
                Spec(Skills.Vigilance),
                Spec(Skills.Gambling),
                Spec(Skills.Acting)) / 3m;

            v[Ppb.Science] = Base(Skills.Knowledge) + Attr(Attrs.Intelligence);

            v[Ppb.Magic] = Spec(Skills.Magic) + Attr(Attrs.Willpower);

            v[Ppb.Culture] = Avg3(
                Spec(Skills.FineArts),
                Spec(Skills.Linguistics),
                Spec(Skills.Diplomacy));

            v[Ppb.Intelligence] =
                Base(Skills.Perception) / 2m
                + Base(Skills.Survival) / 2m
                + Base(Skills.Deceit);

            v[Ppb.Defense] = Avg3(
                Spec(Skills.StrategyAndTactics),
                Spec(Skills.Inspire),
                Spec(Skills.Geography));

            FloorAll(v);
            return v;
        }

        /// <summary>
        /// Domain Panel — additive column from skill-unit total X (skills + bonus sources).
        /// Identity for PPBs with additive; Food/Economy/Production/Defense excluded (percent only).
        /// Corruption is stored signed (negative reduces corruption). Treasury excluded.
        /// </summary>
        public static PpbVector MapToAdvisorAdditive(PpbVector skillInfluence)
        {
            var v = new PpbVector();
            v.EnsureSize();
            foreach (Ppb key in Enum.GetValues<Ppb>())
            {
                if (!HasAdvisorAdditive(key))
                    continue;
                v[key] = skillInfluence[key];
            }
            return v;
        }

        private const decimal AdvisorPercentDivisorDefault = 100m;
        private const decimal AdvisorPercentDivisorSixty = 60m;

        /// <summary>
        /// Domain Panel — percent column from skill-unit total X (skills + bonus sources).
        /// Food, Economy, Production, Defense use X/60; others X/100. Treasury excluded.
        /// Corruption stored signed.
        /// </summary>
        public static PpbVector MapToAdvisorPercent(PpbVector skillInfluence) =>
            MapSkillInfluenceToAdvisorPercent(skillInfluence);

        private static PpbVector MapSkillInfluenceToAdvisorPercent(PpbVector skillInfluence)
        {
            var v = new PpbVector();
            v.EnsureSize();
            foreach (Ppb key in Enum.GetValues<Ppb>())
            {
                if (key == Ppb.Treasury)
                    continue;

                v[key] = ToAdvisorPercentPoints(key, skillInfluence[key]);
            }
            return v;
        }

        private static decimal ToAdvisorPercentPoints(Ppb key, decimal skillValue)
        {
            var divisor = UsesSixtyPercentDivisor(key)
                ? AdvisorPercentDivisorSixty
                : AdvisorPercentDivisorDefault;
            var points = skillValue * 100m / divisor;
            return UsesSixtyPercentDivisor(key)
                ? decimal.Round(points, 1)
                : points;
        }

        private static bool UsesSixtyPercentDivisor(Ppb key) => key switch
        {
            Ppb.Food or Ppb.Economy or Ppb.Production or Ppb.Defense => true,
            _ => false,
        };

        private static bool HasAdvisorAdditive(Ppb key) => key switch
        {
            Ppb.Food or Ppb.Economy or Ppb.Production or Ppb.Defense or Ppb.Treasury => false,
            _ => true,
        };

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Food => $"= {Avg3Formula(Skills.PlantsAndMushrooms, Skills.AnimalsCare, Skills.Beasts)}",
            Ppb.Economy => $"= {Avg3Formula(Skills.MathematicsAndLogic, Skills.RacesAndNations, Skills.Trade)}",
            Ppb.Production => $"= {Skills.Craft} + {Attrs.Intelligence} mod",
            Ppb.Loyalty => $"= {Avg3Formula(Skills.Bluff, Skills.PublicSpeech, Skills.SenseMotives)}",
            Ppb.Stability => $"= {Avg3Formula(Skills.Intimidate, Skills.HistoryAndReligion, Skills.Persuasion)}",
            Ppb.Law => $"= {Avg3Formula(Skills.Investigation, Skills.Observation, Skills.Tracking)}",
            Ppb.Corruption => $"= −({Avg3Formula(Skills.Vigilance, Skills.Gambling, Skills.Acting)}) / 9",
            Ppb.Science => $"= {Skills.Knowledge} + {Attrs.Intelligence} mod",
            Ppb.Magic => $"= {Skills.Magic} + {Attrs.Willpower} mod",
            Ppb.Culture => $"= {Avg3Formula(Skills.FineArts, Skills.Linguistics, Skills.Diplomacy)}",
            Ppb.Intelligence =>
                $"= {Skills.Perception} / 2 + {Skills.Survival} / 2 + {Skills.Deceit}",
            Ppb.Defense => $"= {Avg3Formula(Skills.StrategyAndTactics, Skills.Inspire, Skills.Geography)}",
            _ => null,
        };

        public static string CatalogDescription =>
            "Baron influence from character skills and permanent attribute modifiers "
            + "(excludes wounds and temporary states; floored to whole numbers). "
            + "Hover each PPB value for its formula.";

        /// <summary>Domain Panel — tooltip on the baron's name in Baron and Advisors.</summary>
        public static string BaronAdvisorNameTooltip =>
            "The baron's abilities shape nearly every parameter of the barony. "
            + "Each value in this row comes from the Baron Card (From Skills). "
            + "Hover a cell for how it maps; open Baron Card for the full skill breakdown.";

        /// <summary>
        /// Domain Panel — additive column tooltip. References Baron Card From Skills PPB labels, not raw skills.
        /// </summary>
        public static string? ExplainAdvisorAdditive(Ppb key)
        {
            if (!HasAdvisorAdditive(key))
                return null;

            return $"= {FromSkillsSkillLabel(key)}";
        }

        /// <summary>
        /// Domain Panel — percent column tooltip. Linked From Skills value as X/divisor (Treasury excluded).
        /// </summary>
        public static string? ExplainAdvisorPercent(Ppb key) => key switch
        {
            Ppb.Treasury => null,
            Ppb.Corruption => $"= {FromSkillsSkillLabel(Ppb.Corruption)}/100",
            Ppb.Food or Ppb.Economy or Ppb.Production or Ppb.Defense
                => $"= {FromSkillsSkillLabel(key)}/60",
            _ => $"= {FromSkillsSkillLabel(key)}/100",
        };

        private static string FromSkillsSkillLabel(Ppb key) =>
            $"{PpbCatalog.NameEnglish(key).ToLowerInvariant()} skill";

        private static string Avg3Formula(string a, string b, string c) =>
            $"({a} + {b} + {c}) / 3";

        private static decimal Avg3(decimal a, decimal b, decimal c) => (a + b + c) / 3m;

        private static void FloorAll(PpbVector v)
        {
            foreach (Ppb key in Enum.GetValues<Ppb>())
                v[key] = Math.Floor(v[key]);
        }
    }
}
