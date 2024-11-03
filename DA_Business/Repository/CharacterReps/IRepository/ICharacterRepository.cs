using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ICharacterRepository
    {
        public Task<CharacterDTO> Create(CharacterDTO objDTO);

        public Task<CharacterDTO> Update(CharacterDTO objDTO);
        public Task<int> Delete(int id);

        public Task<CharacterDTO> GetById(int id,bool fullIncludes = false);
        public Task<CharacterDTO> GetByName(string npcName, bool fullIncludes = false);
        public Task<IEnumerable<CharacterDTO>> GetAll(int? id = null, bool fullIncludes = false);
        public Task<IEnumerable<CharacterDTO>> GetAllForUser(string userName, bool fullIncludes = false);

        public Task<IEnumerable<CharacterDTO>> GetAllForCampaign(int campaignId, bool fullIncludes = false);

        public Task<IEnumerable<CharacterDTO>> GetAllApproved(string? userName = null, bool fullIncludes = false);
        public Task<string> GetPortraitUrl(int id);
    }
}
