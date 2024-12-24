using DA_DataAccess.Chat;
using DA_DataAccess;
using DA_Models.CharacterModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Business.Repository.CharacterReps.IRepository
{
    public interface ICampaignRepository
    {
        Task<CampaignDTO> Create(CampaignDTO objDTO);

        Task<int> Delete(int id);
        Task<IEnumerable<CampaignDTO>> GetAll(int? characterId=null);

        Task<CampaignDTO> GetById(int id);

        Task<CampaignDTO> Update(CampaignDTO objDTO);
        Task<bool> CheckIfCampaignBelongToUser(string userName, int campaignId);
    }
}
