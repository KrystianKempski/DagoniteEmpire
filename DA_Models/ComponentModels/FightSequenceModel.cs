using DA_Common;
using DA_Models.CharacterModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
    public class FightSequenceModel
    {

        public BattlePropertyModel AttackerProps { get; set; } = null;
        public BattlePropertyModel DefenderProps { get; set; } = null;

        public ICollection<TraitCharacterDTO>? AttackerStates { get; set; } = new List<TraitCharacterDTO>();
        public ICollection<TraitCharacterDTO>? DefenderStates { get; set; } = new List<TraitCharacterDTO>();

        public HealthModel AttackerHealth { get; set; } = null;
        public HealthModel DefenderHealth { get; set; } = null;
        public string AttackStance { get; set; } = string.Empty;
        public string DefenceStance { get; set; } = string.Empty;
        public string ResultString { get; set; } = string.Empty;
        public bool UpdateAttackerNeeded { get; set; } = false;
        public bool UpdateDefenderNeeded { get; set; } = false;

        public void AddAttacker(AllParamsModel allParams)
        {
            AttackerProps = allParams.BattleProperties;
            AttackerStates = allParams.Character?.TraitsCharacter?.Where(t => t.TraitType == SD.TraitType_Temporary).ToList();
            AttackerHealth = allParams.Health;
        }
        public void AddAttacker(MobDTO mob)
        {
            AttackerProps = mob.BattleProperties;
            var states = mob.States.Split(", ");
            AttackerStates = new List<TraitCharacterDTO>();
            foreach (var state in states)
            {
                var trait = new TraitCharacterDTO(true);
                trait.Name = state;
                AttackerStates.Add(trait);
            }
            AttackerHealth = new MobHealthModel(mob);
        }
        public void AddDefender(AllParamsModel allParams)
        {
            DefenderProps = allParams.BattleProperties;
            DefenderStates = allParams.Character?.TraitsCharacter?.Where(t => t.TraitType == SD.TraitType_Temporary).ToList();
            DefenderHealth = allParams.Health;
        }
        public void AddDefender(MobDTO mob)
        {
            try
            {
                DefenderProps = mob.BattleProperties;
                var states = mob.States.Split(", ");
                DefenderStates = new List<TraitCharacterDTO>();
                foreach (var state in states)
                {
                    var trait = new TraitCharacterDTO(true);
                    var statesParams = mob.States.Split(" ");
                    trait.Name = statesParams[0];
                    trait.TraitValue = Int32.Parse(statesParams[1]);
                    DefenderStates.Add(trait);
                }
                DefenderHealth = new MobHealthModel(mob);
            }
            catch (Exception ex)
            {
                ;
            }
        }

        public void MakeAttack()
        {




        }


    }
}
