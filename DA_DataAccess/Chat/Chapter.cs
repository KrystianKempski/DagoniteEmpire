using DA_Common;
using DA_DataAccess.CharacterClasses;
using MudBlazor.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.Chat
{
    public class Chapter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DayTime { get; set; } = string.Empty;
        public int DateNumber { get; set; } = 0;
        public string Place { get; set; } = string.Empty;
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public DateTime CreatedDate { get; set; }
        public ICollection<Character> Characters { get; set; } = new List<Character>();
        public bool IsFinished { get; set; }

        [ForeignKey(nameof(Campaign))]
        public int CampaignId { get; set; }
        public virtual Campaign? Campaign { get; set; }
    }
}
