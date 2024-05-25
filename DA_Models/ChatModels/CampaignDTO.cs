using DA_DataAccess.CharacterClasses;
using DA_Models.CharacterModels;
using MudBlazor.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.Chat
{
    public class CampaignDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<ChapterDTO> Chapters { get; set; } = new List<ChapterDTO>();
        public DateTime CreatedDate { get; set; }
        public ICollection<CharacterDTO> Characters { get; set; } = new List<CharacterDTO>();
        public string GameMaster { get; set; } = string.Empty;
        public bool IsFinished { get; set; } = false;
    }
}
