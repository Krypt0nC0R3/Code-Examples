using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalosWorkshop.Core
{
    public class KMail
    {
        private int ID { get; set; } = -1;
        public KUser Sender { get; set; } = new();
        public KUser Reciver { get; set; } = new();
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
