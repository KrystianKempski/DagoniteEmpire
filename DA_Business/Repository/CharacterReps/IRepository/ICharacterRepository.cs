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

        public Task<CharacterDTO> GetById(int id);
        public Task<CharacterDTO> GetByName(string npcName);
        public Task<IEnumerable<CharacterDTO>> GetAll(int? id = null);
        public Task<IEnumerable<CharacterDTO>> GetAllForUser(string userName);
        public Task<IEnumerable<CharacterDTO>> GetAllInfoForUser(string userName);

        public Task<IEnumerable<CharacterDTO>> GetAllForCampaign(int campaignId);

        public Task<IEnumerable<CharacterDTO>> GetAllApproved(string? userName = null);
        public Task<IEnumerable<CharacterDTO>> GetAllInfoApproved(string? userName = null);
        public Task<string> GetPortraitUrl(int id);
    }
}
