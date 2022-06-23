using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS_Overlay_Host
{
    public class Advertisement
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public string LinkToImage { get; set; }

        public int Interval { get; set; }
    }
}
