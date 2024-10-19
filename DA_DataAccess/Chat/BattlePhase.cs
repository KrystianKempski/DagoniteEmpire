using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess.Chat
{
    public class BattlePhase
    {
        public int Id { get; set; }
        public int Name { get; set; }
        public int ChapterId { get; set; }
        public int CampaignId { get; set; }
        public int CurrentTurn { get; set; } = 1;
        public bool BattleOngoing { get; set; } = false;

    }
}
