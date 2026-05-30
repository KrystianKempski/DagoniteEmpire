using System.ComponentModel;
using System.Text;
using DA_Business.Repository.CharacterReps.IRepository;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace DagoniteEmpire.Service.Scribe
{
    /// <summary>
    /// Kernel plugin exposing character lookup to the LLM agent.
    /// </summary>
    public sealed class CharacterPlugin
    {
        private readonly ICharacterRepository _repo;
        private readonly ILogger<CharacterPlugin> _logger;

        public CharacterPlugin(ICharacterRepository repo, ILogger<CharacterPlugin> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public int? CampaignId { get; set; }

        [KernelFunction("get_character_by_name")]
        [Description("Looks up a single character (PC or NPC) by name. " +
                     "Returns name, race, profession and short description. " +
                     "Use this when the user asks about a specific character.")]
        public async Task<string> GetCharacterByNameAsync(
            [Description("Character name (NPCName), e.g. 'Garrick' or 'Black Dragon'.")] string name)
        {
            _logger.LogInformation("Agent get_character_by_name: {Name}", name);
            var ch = await _repo.GetByName(name, fullIncludes: false);
            if (ch is null || ch.Id == 0)
                return $"Nie znaleziono postaci o nazwie '{name}'.";

            return FormatCharacter(ch);
        }

        [KernelFunction("list_campaign_characters")]
        [Description("Lists all characters (PC and NPC) in the current campaign with brief info. " +
                     "Use this for questions like 'who is in this campaign' or to discover character names.")]
        public async Task<string> ListCampaignCharactersAsync(
            [Description("Optional max number of characters to return. Default 20.")] int limit = 20)
        {
            if (!CampaignId.HasValue)
                return "Brak kontekstu kampanii.";

            limit = Math.Clamp(limit, 1, 50);
            var chars = (await _repo.GetAllForCampaign(CampaignId.Value, fullIncludes: false))
                .Take(limit)
                .ToList();

            if (chars.Count == 0)
                return "W tej kampanii nie ma jeszcze postaci.";

            var sb = new StringBuilder();
            sb.AppendLine($"Postacie w kampanii ({chars.Count}):");
            foreach (var ch in chars)
                sb.AppendLine($"- {ch.NPCName} ({ch.NPCType}, {ch.RaceName} {ch.ProfessionName})");
            return sb.ToString();
        }

        private static string FormatCharacter(DA_Models.CharacterModels.CharacterDTO ch)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Postać: {ch.NPCName}");
            sb.AppendLine($"Typ: {ch.NPCType}");
            sb.AppendLine($"Rasa: {ch.RaceName}");
            sb.AppendLine($"Profesja: {ch.ProfessionName}");
            if (!string.IsNullOrWhiteSpace(ch.Description))
            {
                sb.AppendLine("Opis:");
                sb.AppendLine(ch.Description);
            }
            return sb.ToString();
        }
    }
}
