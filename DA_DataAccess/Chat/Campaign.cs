using DA_DataAccess.CharacterClasses;
using MudBlazor.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.Chat
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public DateTime CreatedDate { get; set; }
        public ICollection<Character> Characters { get; set; } = new List<Character>();
        public string GameMaster { get; set; } = string.Empty;
        public bool IsFinished { get; set; }
    }
}
