using System.Text;
using System.Text.RegularExpressions;
using DA_Business.Repository.CharacterReps.IRepository;
using DA_Business.Services.Interfaces;
using DA_DataAccess.Chat;
using DA_Models.ChatModels;

namespace DA_Business.Services
{
    public class CampaignSummaryService : ICampaignSummaryService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IChapterRepository _chapterRepository;
        private readonly IPostRepository _postRepository;

        public CampaignSummaryService(
            ICampaignRepository campaignRepository,
            IChapterRepository chapterRepository,
            IPostRepository postRepository)
        {
            _campaignRepository = campaignRepository;
            _chapterRepository = chapterRepository;
            _postRepository = postRepository;
        }

        public async Task<CampaignSummaryResult?> GenerateChapterSummaryAsync(int chapterId)
        {
            var chapter = await _chapterRepository.GetById(chapterId);
            if (chapter is null || chapter.Id < 1)
                return null;

            var campaign = await _campaignRepository.GetById(chapter.CampaignId);
            var posts = (await _postRepository.GetAll(chapterId)).ToList();

            var sb = new StringBuilder();
            AppendChapterHeader(sb, chapter, campaign?.Name ?? "Nieznana");
            AppendPosts(sb, posts);

            return new CampaignSummaryResult
            {
                Content = sb.ToString(),
                FileName = BuildFileName(campaign?.Name, chapter.Name),
                PostCount = posts.Count,
                ChapterCount = 1
            };
        }

        public async Task<CampaignSummaryResult?> GenerateCampaignSummaryAsync(int campaignId)
        {
            var campaign = await _campaignRepository.GetById(campaignId);
            if (campaign is null || campaign.Id < 1)
                return null;

            var chapters = (await _chapterRepository.GetAll(campaignId))
                .OrderBy(c => c.CreatedDate)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine($"KAMPANIA: {campaign.Name}");
            if (!string.IsNullOrWhiteSpace(campaign.Description))
                sb.AppendLine($"Opis: {campaign.Description}");
            sb.AppendLine($"Mistrz Gry: {campaign.GameMaster}");
            sb.AppendLine($"Utworzona: {campaign.CreatedDate:dd-MM-yyyy HH:mm}");
            sb.AppendLine($"Rozdziałów: {chapters.Count}");
            sb.AppendLine("================================================================================");
            sb.AppendLine();

            var totalPosts = 0;
            foreach (var chapter in chapters)
            {
                var posts = (await _postRepository.GetAll(chapter.Id)).ToList();
                totalPosts += posts.Count;

                AppendChapterHeader(sb, chapter, campaign.Name);
                AppendPosts(sb, posts);
                sb.AppendLine();
            }

            return new CampaignSummaryResult
            {
                Content = sb.ToString(),
                FileName = BuildFileName(campaign.Name, null),
                PostCount = totalPosts,
                ChapterCount = chapters.Count
            };
        }

        private static void AppendChapterHeader(StringBuilder sb, ChapterDTO chapter, string campaignName)
        {
            sb.AppendLine("================================================================================");
            sb.AppendLine($"ROZDZIAŁ: {chapter.Name}");
            sb.AppendLine($"Kampania: {campaignName}");
            sb.AppendLine($"Data świata: {chapter.Date}, {chapter.DayTime}");
            if (!string.IsNullOrWhiteSpace(chapter.Place))
                sb.AppendLine($"Miejsce: {chapter.Place}");
            if (!string.IsNullOrWhiteSpace(chapter.Description))
                sb.AppendLine($"Opis: {chapter.Description}");
            sb.AppendLine($"Rozpoczęty: {chapter.CreatedDate:dd-MM-yyyy HH:mm}");
            sb.AppendLine("================================================================================");
            sb.AppendLine();
        }

        private static void AppendPosts(StringBuilder sb, IReadOnlyList<PostDTO> posts)
        {
            if (!posts.Any())
            {
                sb.AppendLine("(Brak postów w tym rozdziale)");
                sb.AppendLine();
                return;
            }

            foreach (var post in posts)
            {
                sb.AppendLine($"[{GetAuthorName(post)}]");
                sb.AppendLine(StripHtml(post.Content));
                sb.AppendLine();
            }
        }

        private static string GetAuthorName(PostDTO post)
        {
            if (!string.IsNullOrWhiteSpace(post.AlternativeName))
                return post.AlternativeName;

            return post.Character?.NPCName ?? "Nieznany";
        }

        private static string BuildFileName(string? campaignName, string? chapterName)
        {
            var parts = new List<string> { "podsumowanie" };
            if (!string.IsNullOrWhiteSpace(campaignName))
                parts.Add(SanitizeFileName(campaignName));
            if (!string.IsNullOrWhiteSpace(chapterName))
                parts.Add(SanitizeFileName(chapterName));

            parts.Add(DateTime.Now.ToString("yyyy-MM-dd"));
            return string.Join("_", parts) + ".txt";
        }

        private static string SanitizeFileName(string name)
        {
            var sanitized = Regex.Replace(name.Trim(), @"[^\w\-\.]", "_");
            sanitized = Regex.Replace(sanitized, @"_+", "_");
            return sanitized.Trim('_');
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var result = Regex.Replace(
                html,
                @"<(script|style)[^>]*>[\s\S]*?</\1>",
                string.Empty,
                RegexOptions.IgnoreCase);

            result = Regex.Replace(
                result,
                @"<br\s*/?>|</p>|</div>",
                "\n",
                RegexOptions.IgnoreCase);

            result = Regex.Replace(result, @"<[^>]+>", string.Empty);
            result = System.Net.WebUtility.HtmlDecode(result);
            result = Regex.Replace(result, @"[ \t]+", " ");
            result = Regex.Replace(result, @"\n\s*\n", "\n\n");

            return result.Trim();
        }
    }
}
