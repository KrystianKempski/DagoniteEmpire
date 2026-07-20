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

            v[Ppb.Corruption] = Avg3(
                Spec(Skills.Vigilance),
                Spec(Skills.Gambling),
                Spec(Skills.Acting));

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

        public static string? ExplainAdditive(Ppb key) => key switch
        {
            Ppb.Food =>
                $"= ⌊({Skills.PlantsAndMushrooms} + {Skills.AnimalsCare} + {Skills.Beasts}) / 3⌋",
            Ppb.Economy =>
                $"= ⌊({Skills.MathematicsAndLogic} + {Skills.RacesAndNations} + {Skills.Trade}) / 3⌋",
            Ppb.Production =>
                $"= ⌊{Skills.Craft} + {Attrs.Intelligence} mod⌋",
            Ppb.Loyalty =>
                $"= ⌊({Skills.Bluff} + {Skills.PublicSpeech} + {Skills.SenseMotives}) / 3⌋",
            Ppb.Stability =>
                $"= ⌊({Skills.Intimidate} + {Skills.HistoryAndReligion} + {Skills.Persuasion}) / 3⌋",
            Ppb.Law =>
                $"= ⌊({Skills.Investigation} + {Skills.Observation} + {Skills.Tracking}) / 3⌋",
            Ppb.Corruption =>
                $"= ⌊({Skills.Vigilance} + {Skills.Gambling} + {Skills.Acting}) / 3⌋",
            Ppb.Science =>
                $"= ⌊{Skills.Knowledge} + {Attrs.Intelligence} mod⌋",
            Ppb.Magic =>
                $"= ⌊{Skills.Magic} + {Attrs.Willpower} mod⌋",
            Ppb.Culture =>
                $"= ⌊({Skills.FineArts} + {Skills.Linguistics} + {Skills.Diplomacy}) / 3⌋",
            Ppb.Intelligence =>
                $"= ⌊{Skills.Perception} / 2 + {Skills.Survival} / 2 + {Skills.Deceit}⌋",
            Ppb.Defense =>
                $"= ⌊({Skills.StrategyAndTactics} + {Skills.Inspire} + {Skills.Geography}) / 3⌋",
            _ => null,
        };

        public static string CatalogDescription =>
            "Baron influence from character skills and permanent attribute modifiers "
            + "(excludes wounds and temporary states; floored to whole numbers). "
            + "Hover each PPB value for its formula.";

        private static decimal Avg3(decimal a, decimal b, decimal c) => (a + b + c) / 3m;

        private static void FloorAll(PpbVector v)
        {
            foreach (Ppb key in Enum.GetValues<Ppb>())
                v[key] = Math.Floor(v[key]);
        }
    }
}
