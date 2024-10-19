using DA_Models.CharacterModels;
using DA_Models.ComponentModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ChatModels
{
    public class BattlePhaseDTO
    {
        public BattlePhaseDTO() { }
        public BattlePhaseDTO(IDictionary<int, AllParamsModel?> charParams, ICollection<MobDTO> mobs, int campId, int chapterId) 
        {
            CharacterAllParams= charParams;
            Mobs = mobs;
            CampaignId = campId;
            ChapterId = chapterId;
            BattleOngoing = true;
        }

        public int Id { get; set; }
        public int Name { get; set; }
        public int ChapterId { get; set; }
        public int CampaignId { get; set; }
        public int CurrentTurn { get; set; } = 1;
        public bool BattleOngoing { get; set; }  = false;

        // private variables
        private IDictionary<int, AllParamsModel?> CharacterAllParams { get; set; } = new Dictionary<int, AllParamsModel?>();
        private ICollection<MobDTO> Mobs = new List<MobDTO>();

        // public functions 
        public void NextTurn() { }
        public void StartBattle() { }
        public void EndBattle() { }

    }
}
