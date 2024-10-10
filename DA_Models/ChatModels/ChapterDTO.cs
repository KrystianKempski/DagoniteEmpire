using DA_Models;
using DA_Models.CharacterModels;
using MudBlazor.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.Chat
{
    public class ChapterDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Day { get; set; } = string.Empty;
        public string Place { get; set; } = string.Empty;
        public ICollection<PostDTO> Posts { get; set; } = new List<PostDTO>();
        public DateTime CreatedDate { get; set; }
        public ICollection<CharacterDTO> Characters { get; set; } = new List<CharacterDTO>();
        public bool IsFinished { get; set; } = false;
        public int CampaignId { get; set; }

        public override string ToString() => Name;
    }
}
