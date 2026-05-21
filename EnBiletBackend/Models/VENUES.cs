using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class VENUES
    {
        public int? venueID { get; set; }

        [MaxLength(200)]
        public string? venueName { get; set; }
        [MaxLength(50)]
        public string? city { get; set; }
        [MaxLength(500)]
        public string? address { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

    }
}
