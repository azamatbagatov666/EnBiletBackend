using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class EVENTSEATS
    {
        public int? seatID { get; set; }
        public int? eventID { get; set; }
        public string? cellID { get; set; }
        public int? price { get; set; }
        public string? status { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public string? reserved_until { get; set; }
        public string? reserved_by { get; set; }

    }
}
