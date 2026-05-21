using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class SEATMAPS
    {
        public int? mapID { get; set; }
        [MaxLength(50)]

        public string? mapName { get; set; }
        public int? venueID { get; set; }
        public bool? isSeated { get; set; }
        public string? layoutJS { get; set; }
        public string? venueName { get; set; }
        public int? maxCapacity { get; set; }

    }
}
