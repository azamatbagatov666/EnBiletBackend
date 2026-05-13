using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class SHOWS
    {
        public int? showID { get; set; }
        public string showName { get; set; } = null!;
        public string? description { get; set; }
        public string? imageKey { get; set; }
        public string? imageThumbKey { get; set; }
        public DateTime createdAt { get; set; }
    }
}
