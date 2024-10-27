using DA_Common;
using DA_DataAccess.CharacterClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.CharacterModels
{
    public class TraitRaceDTO: TraitDTO
    {
        public TraitRaceDTO() { }
        public override string TraitType { get; set; } = SD.TraitType_Race;
        public TraitRaceDTO(TraitDTO traitDTO, RaceDTO raceDTO)
        {
            foreach (var prop in traitDTO.GetType().GetProperties())
            {
                if (GetType().GetProperty(prop.Name).CanWrite)
                    GetType().GetProperty(prop.Name).SetValue(this, prop.GetValue(traitDTO, null), null);
            }
            if(Races != null)
            {
                var race = Races.FirstOrDefault(r => r.Id == raceDTO.Id);
                if (race is null)
                    Races.Add(raceDTO);
                else
                    race = raceDTO;
            }
        }
        public ICollection<RaceDTO> Races { get; set; } = new List<RaceDTO>();
    }
}
