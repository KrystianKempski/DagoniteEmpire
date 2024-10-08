using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface IMobRepository
    {
        public Task<MobDTO> Create(MobDTO objDTO);

        public Task<MobDTO> Update(MobDTO objDTO);
        public Task<int> Delete(int id);

        public Task<MobDTO> GetById(int id);
        public Task<IEnumerable<MobDTO>> GetAll();
        public Task<IEnumerable<MobDTO>> GetAllForCampaign(int campaignId);
    }
}
