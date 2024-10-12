using DA_Common;
using DA_DataAccess.CharacterClasses;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public string Name { get; set; }
        public string Description { get; set; }
        public string DayTime { get; set; }
        public int DateNumber { get; set; } = 0;
        public string Place { get; set; }
        public ICollection<Post> Posts { get; set; }
        public DateTime CreatedDate { get; set; }
        public ICollection<Character> Characters { get; set; }
        public bool IsFinished { get; set; }

        [ForeignKey(nameof(Campaign))]
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
    }
}
