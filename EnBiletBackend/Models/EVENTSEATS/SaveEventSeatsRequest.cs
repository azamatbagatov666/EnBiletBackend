using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class SaveEventSeatsRequest
    {
        public int eventID { get; set; }
        public int mapID { get; set; }
        public List<EventSeatDto> Seats { get; set; } = new();
    }


}
