using DA_Common;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DA_Common.SD;

namespace DA_Models.ComponentModels
{
    public class MobHealthModel : HealthModel
    {
        private MobDTO Mob;

        public MobHealthModel(MobDTO mob) : base(null)
        {
            Mob = mob;
        }
        public override int MaxWounds
        {
            get => Mob.MaxWounds;
        }
        public override int CurrentWounds
        {
            get => Mob.CurrentWounds;
        }
        public override int HealingModyfier
        {
            get => 100;
        }
        public override void AddWound(WoundDTO wound)
        {
            // here add to Mob Current WOunds
        }
        public override void FillPropertiesContainer(IEnumerable<WoundDTO>? properties)
        {
            // ignore this
        }
    }
}
