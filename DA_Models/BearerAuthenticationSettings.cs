using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_Models
{
    public class BearerAuthenticationSettings
    {
        public string JwtKey { get; set; }
        public string ValidIssuer { get; set; }
        public string ValidAudience { get; set; }
    }
}
