using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnBiletBackend.Models
{
    public class EventSeatDto
    {
        public string CellId { get; set; } = null!;
        public decimal Price { get; set; }
        public string Status { get; set; } = null!;
    }
}
