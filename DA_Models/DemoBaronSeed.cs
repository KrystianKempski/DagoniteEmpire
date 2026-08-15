using System.Reflection;
using System.Text.Json;

namespace DA_Models;

/// <summary>
/// Snapshot of the hand-authored demo baron ("Aldric Emberfall") captured from the
/// canonical local database. Loaded from an embedded JSON resource so every database —
/// fresh, production or dev — seeds the exact same rich character (portrait, class,
/// attributes and the full skill sheet). Foreign keys (profession, race, languages)
/// are stored by name and resolved by the seeder against the target database.
/// </summary>
public static class DemoBaronSeed
{
    private const string ResourceName = "DA_Models.SeedData.demo-baron-aldric.json";

    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public sealed record ProfessionData(
        string Name,
        string? Description,
        string? RelatedAttributeName,
        int ClassLevel,
        int CurrentFocusPoints,
        bool IsApproved,
        bool IsUniversal,
        int CasterType);

    public sealed record RaceData(
        string Name,
        int Index,
        string? Description,
        bool RaceApproved);

    public sealed record AttributeData(
        string Name,
        string? FeatureType,
        int Index,
        int BaseBonus,
        int RaceBonus,
        int GearBonus,
        int TraitBonus,
        int OtherBonuses,
        int TempBonuses,
        int HealthBonus);

    public sealed record BaseSkillData(
        string Name,
        string? FeatureType,
        int Index,
        int BaseBonus,
        int RaceBonus,
        int GearBonus,
        int TraitBonus,
        int OtherBonuses,
        int TempBonuses,
        int HealthBonus,
        string? RelatedAttribute1,
        string? RelatedAttribute2);

    public sealed record SpecialSkillData(
        string Name,
        string? FeatureType,
        int Index,
        int BaseBonus,
        int RaceBonus,
        int GearBonus,
        int TraitBonus,
        int OtherBonuses,
        int TempBonuses,
        int HealthBonus,
        string? RelatedAttribute1,
        string? RelatedAttribute2,
        string? RelatedBaseSkillName,
        string? ChosenAttribute,
        bool Editable);

    public sealed record EquipmentSlotData(
        int Count,
        int EquipmentID,
        bool IsEquipped,
        string? SlotType);

    public sealed record CharacterData(
        string NpcName,
        string? Description,
        int Age,
        int Relation,
        string? NpcType,
        string? ImageUrl,
        string? IconUrl,
        int AttributePoints,
        int CurrentExpPoints,
        int UsedExpPoints,
        int TraitBalance,
        int WeaponSet,
        int DateNumber,
        ProfessionData? Profession,
        RaceData? Race,
        string[] Languages,
        AttributeData[] Attributes,
        BaseSkillData[] BaseSkills,
        SpecialSkillData[] SpecialSkills,
        EquipmentSlotData[] EquipmentSlots);

    /// <summary>Reads and deserialises the embedded demo-baron snapshot.</summary>
    public static CharacterData Load()
    {
        var assembly = typeof(DemoBaronSeed).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded demo-baron snapshot '{ResourceName}' not found.");

        return JsonSerializer.Deserialize<CharacterData>(stream, Options)
            ?? throw new InvalidOperationException("Demo-baron snapshot deserialised to null.");
    }
}
