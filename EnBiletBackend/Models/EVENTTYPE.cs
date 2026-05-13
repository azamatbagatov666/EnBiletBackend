using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class EVENTTYPE
    {
        public int? eventID { get; set; }
        public int? showID { get; set; }
        public int? venueID { get; set; }
        public int? mapID { get; set; }
        public string? showName { get; set; }
        public string? date { get; set; }
        public string? city { get; set; }
        public string? venueName { get; set; }
        public bool? ticketSale { get; set; }
        public bool? isPublic { get; set; }
        public string? soldTickets { get; set; }

    }
}
