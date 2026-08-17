using DA_Common;
using DA_Common.Localization;
using Microsoft.Extensions.Localization;

namespace DA_Models.CharacterModels
{
    /// <summary>
    /// Display-time localization for catalog equipment. Stored <see cref="EquipmentDTO.Name"/>
    /// stays the canonical English key (or free-text for custom items).
    /// </summary>
    public static class EquipmentDisplay
    {
        public static string DisplayName(this EquipmentDTO equipment)
            => LocCatalog.NameOrRaw(equipment?.Name, SD.BasicEquipment.Names);

        public static string DisplayName(this EquipmentDTO equipment, IStringLocalizer localizer)
            => LocCatalog.NameOrRaw(equipment?.Name, SD.BasicEquipment.Names, localizer);

        public static string DisplayType(this EquipmentDTO equipment)
            => LocCatalog.Name(equipment?.EquipmentType);

        public static string DisplayType(this EquipmentDTO equipment, IStringLocalizer localizer)
            => LocCatalog.Name(equipment?.EquipmentType, localizer);

        public static string DisplayDescription(this EquipmentDTO equipment)
            => LocCatalog.NameOrRaw(equipment?.Description, SD.BasicEquipment.CatalogDescriptions);

        public static string DisplayDescription(this EquipmentDTO equipment, IStringLocalizer localizer)
            => LocCatalog.NameOrRaw(equipment?.Description, SD.BasicEquipment.CatalogDescriptions, localizer);

        public static string DisplayShortDescr(this EquipmentDTO equipment)
            => LocCatalog.NameOrRaw(equipment?.ShortDescr, SD.BasicEquipment.CatalogDescriptions);

        public static string DisplayShortDescr(this EquipmentDTO equipment, IStringLocalizer localizer)
            => LocCatalog.NameOrRaw(equipment?.ShortDescr, SD.BasicEquipment.CatalogDescriptions, localizer);
    }
}
