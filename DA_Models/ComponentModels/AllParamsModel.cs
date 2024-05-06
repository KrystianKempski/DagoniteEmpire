using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.CategoryTypes;

namespace DA_Models.ComponentModels
{
    public class AllParamsModel
    {
        public CharacterDTO Character { get; set; } = new CharacterDTO();
        public RaceDTO CurrentRace { get; set; } = new RaceDTO();
        public ProfessionDTO Profession { get; set; } = new ProfessionDTO();
        public IEnumerable<AttributeDTO> Attributes { get; set; } = Enumerable.Empty<AttributeDTO>();
        public IEnumerable<BaseSkillDTO> BaseSkills { get; set; } = Enumerable.Empty<BaseSkillDTO>();
        public ICollection<SpecialSkillDTO> SpecialSkills { get; set; } = new HashSet<SpecialSkillDTO>();
        public ICollection<TraitAdvDTO> TraitsAdv { get; set; } = new List<TraitAdvDTO>();
        public ICollection<TraitDTO> Traits { get; set; } = new List<TraitDTO>();
        public ICollection<RaceDTO> Races { get; set; } = new List<RaceDTO>();
        public ICollection<EquipmentDTO> Equipment { get; set; } = new List<EquipmentDTO>();
    }
}
