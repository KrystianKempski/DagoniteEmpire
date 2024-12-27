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
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }

        [ForeignKey(nameof(Character))]
        public int CharacterId { get; set; }
        public virtual Character? Character { get; set; }

        [ForeignKey(nameof(Chapter))]
        public int ChapterId { get; set; }
        public virtual Chapter? Chapter { get; set; }
        public string? AlternativeName { get; set; } = null;
        public string? AlternativeImageUrl { get; set; } = null;
    }
}
