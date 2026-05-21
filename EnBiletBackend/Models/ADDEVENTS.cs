using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class ADDEVENTS
    {
        public int? eventID { get; set; }
        public int? venueID { get; set; }
        public int? showID { get; set; }
        public DateTime? date { get; set; }
        public string? imageKey { get; set; }
        public string? imageThumbKey { get; set; }
        public bool? isPublic { get; set; }
        public bool? ticketSale { get; set; }


    }
}
