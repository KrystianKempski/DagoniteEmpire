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
    public class PostDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CharacterId { get; set; }
        public CharacterDTO? Character { get; set; }
        public int ChapterId { get; set; }
    }
}
