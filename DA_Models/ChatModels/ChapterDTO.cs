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
        public string Name { get; set; }
        public string Description { get; set; }
        public string Day { get; set; }
        public string Place { get ; set; }
        public ICollection<PostDTO> Posts { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<CharacterDTO> Characters { get; set; }
        public bool IsFinished { get; set; }
        public int CampaignId { get; set; }
    }
}
