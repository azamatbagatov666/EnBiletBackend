using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class SHOWS
    {
        public int? showID { get; set; }
        [MaxLength(200)]

        public string showName { get; set; } = null!;
        public string? description { get; set; }

        public string? imageKey { get; set; }

        public string? imageThumbKey { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
    }
}
