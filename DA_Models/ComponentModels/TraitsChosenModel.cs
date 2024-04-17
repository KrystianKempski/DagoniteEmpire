using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models.ComponentModels
{
    public class TraitsChosenModel
    {
        public List<TraitDTO> TraitsChosen { get; set; }
        public string TraitType { get; set; }
        public bool IsVisible { get; set; } = false;
    }
}
