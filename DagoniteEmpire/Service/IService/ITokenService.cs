using DA_DataAccess;
using Microsoft.AspNetCore.Components.Forms;

namespace DagoniteEmpire.Service.IService
{
    public interface ITokenService
    {
        string GenerateToken();
    }
}
