using System.ComponentModel;
using System.Text;
using DA_Business.Repository.CharacterReps.IRepository;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace DagoniteEmpire.Service.Scribe
{
    /// <summary>
    /// Kernel plugin exposing chapter metadata to the LLM agent.
    /// </summary>
    public sealed class ChapterPlugin
    {
        private readonly IChapterRepository _repo;
        private readonly ILogger<ChapterPlugin> _logger;

        public ChapterPlugin(IChapterRepository repo, ILogger<ChapterPlugin> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public int? CampaignId { get; set; }

        [KernelFunction("list_campaign_chapters")]
        [Description("Lists chapters in the current campaign with their place, in-game date and status. " +
                     "Use this to discover chapter IDs or build a timeline of events.")]
        public async Task<string> ListCampaignChaptersAsync()
        {
            if (!CampaignId.HasValue)
                return "Brak kontekstu kampanii.";

            var chapters = (await _repo.GetAll(CampaignId.Value)).ToList();
            if (chapters.Count == 0)
                return "W tej kampanii nie ma jeszcze rozdziałów.";

            var sb = new StringBuilder();
            sb.AppendLine($"Rozdziały kampanii ({chapters.Count}):");
            foreach (var ch in chapters.OrderBy(c => c.DateNumber))
            {
                var status = ch.IsFinished ? "ukończony" : "trwa";
                sb.AppendLine($"- #{ch.Id} {ch.Name} | miejsce: {ch.Place} | {status}");
            }
            return sb.ToString();
        }

        [KernelFunction("get_chapter")]
        [Description("Returns details of a single chapter: name, description, place, date, status. " +
                     "Does NOT return the post contents — use the memory search for those.")]
        public async Task<string> GetChapterAsync(
            [Description("Chapter ID (integer).")] int chapterId)
        {
            var ch = await _repo.GetById(chapterId);
            if (ch is null || ch.Id == 0)
                return $"Nie znaleziono rozdziału o ID {chapterId}.";

            var sb = new StringBuilder();
            sb.AppendLine($"Rozdział #{ch.Id}: {ch.Name}");
            sb.AppendLine($"Miejsce: {ch.Place}");
            sb.AppendLine($"Pora dnia: {ch.DayTime}");
            sb.AppendLine($"Status: {(ch.IsFinished ? "ukończony" : "trwa")}");
            if (!string.IsNullOrWhiteSpace(ch.Description))
            {
                sb.AppendLine("Opis:");
                sb.AppendLine(ch.Description);
            }
            return sb.ToString();
        }
    }
}
