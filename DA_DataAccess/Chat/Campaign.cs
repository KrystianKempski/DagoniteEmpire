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
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<Character> Characters { get; set; }
        public string GameMaster { get; set; }
        public bool IsFinished { get; set; }
    }
}
