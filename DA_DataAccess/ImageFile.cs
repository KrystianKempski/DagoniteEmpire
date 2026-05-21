using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA_DataAccess
{
    public class ImageFile
    {
        public int Id { get; set; }
        public string ImageName { get; set; } = string.Empty;

        public byte[] fileData { get; set; } = Array.Empty<byte>();
    }
}
