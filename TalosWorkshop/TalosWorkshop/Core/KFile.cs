using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalosWorkshop.Core
{
    public class KFile
    {
        private int ID { get; set; } = -1;

        public KUser Owner { get; set; } = new();
        public string Icon { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";

        public KFile(int id)
        {
            ID = id;
        }
    }
}
