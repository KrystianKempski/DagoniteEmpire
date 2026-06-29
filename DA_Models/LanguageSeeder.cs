using DA_Common;
using DA_Models.CharacterModels;

namespace DA_Models
{
    public static class LanguageSeeder
    {
        public static IEnumerable<LanguageDTO> GetAll()
        {
            var index = 0;
            foreach (var language in HumanLanguages())
                yield return WithIndex(language, ++index);
            foreach (var language in RacialLanguages())
                yield return WithIndex(language, ++index);
            foreach (var language in ExoticLanguages())
                yield return WithIndex(language, ++index);
        }

        private static LanguageDTO WithIndex(LanguageDTO language, int index)
        {
            language.Index = index;
            language.IsApproved = true;
            return language;
        }

        private static IEnumerable<LanguageDTO> HumanLanguages()
        {
            yield return Lang("wspólny", SD.Languages.CategoryHuman, "klasyczny", "Imperium, Wolne Miasta");
            yield return Lang("klasyczny imperialny", SD.Languages.CategoryHuman, "klasyczny", "martwy język imperium");
            yield return Lang("starokildradzki", SD.Languages.CategoryHuman, "klasyczny",
                "zapomniany język elit Kildradu (Obecnie Wschodnia Marchia)");
            yield return Lang("taledin", SD.Languages.CategoryHuman, "taledin", "ludy z północnych krain");
            yield return Lang("Solime", SD.Languages.CategoryHuman, "Solime", "Solime");
            yield return Lang("Thralkled", SD.Languages.CategoryHuman, "klasyczny", "Thralkled");
            yield return Lang("stara mowa Vorgoweldów", SD.Languages.CategoryHuman, "klasyczny", "Plemiona Vorgoweldów");
            yield return Lang("dialekt dalyjczyków", SD.Languages.CategoryHuman, "klasyczny", "Dalyjczycy");
            yield return Lang("felvgardzki", SD.Languages.CategoryHuman, "klasyczny", "felvgard");
            yield return Lang("nindu", SD.Languages.CategoryHuman, "nindu", "cesarstwo Gu-ilan");
            yield return Lang("rashi", SD.Languages.CategoryHuman, "rashi", "rashi");
            yield return Lang("suochiański", SD.Languages.CategoryHuman, "rashi", "suochiański");
            yield return Lang("bingdoński", SD.Languages.CategoryHuman, "bingdoński", "bingdoński");
            yield return Lang("klasyczny Gu-ilański", SD.Languages.CategoryHuman, "nindu",
                "język elit w cesarstwie Gu-ilan");
        }

        private static IEnumerable<LanguageDTO> RacialLanguages()
        {
            yield return Lang("Dwarvish", SD.Languages.CategoryRacial, "Dwarvish", "Dwarves");
            yield return Lang("Elvish", SD.Languages.CategoryRacial, "Elvish", "Elves");
            yield return Lang("Giant", SD.Languages.CategoryRacial, "Dwarvish", "Ogres, Giants");
            yield return Lang("Gnomish", SD.Languages.CategoryRacial, "Dwarvish", "Gnomes");
            yield return Lang("Goblin", SD.Languages.CategoryRacial, "Dwarvish", "Goblinoids");
            yield return Lang("Halfling", SD.Languages.CategoryRacial, "Common", "Halflings");
            yield return Lang("Orc", SD.Languages.CategoryRacial, "Dwarvish", "Orcs");
        }

        private static IEnumerable<LanguageDTO> ExoticLanguages()
        {
            yield return Lang("Abyssal", SD.Languages.CategoryExotic, "Infernal", "Demons");
            yield return Lang("Celestial", SD.Languages.CategoryExotic, "Celestial", "Celestials");
            yield return Lang("Draconic", SD.Languages.CategoryExotic, "Draconic", "Dragons, dragonborn");
            yield return Lang("Deep Speech", SD.Languages.CategoryExotic, "-", "Aboleths, cloakers");
            yield return Lang("Infernal", SD.Languages.CategoryExotic, "Infernal", "Devils");
            yield return Lang("Primordial", SD.Languages.CategoryExotic, "Dwarvish", "Elementals");
            yield return Lang("Sylvan", SD.Languages.CategoryExotic, "Elvish", "Fey creatures");
            yield return Lang("Undercommon", SD.Languages.CategoryExotic, "Elvish", "Underworld traders");
        }

        private static LanguageDTO Lang(string name, string category, string script, string description) =>
            new()
            {
                Name = name,
                Category = category,
                Script = script,
                Description = description
            };
    }
}
