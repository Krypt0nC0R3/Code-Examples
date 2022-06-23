using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS_Overlay_Host
{
    class SiteMessage
    {
        public string FileType;
        public string Message;

        public SiteMessage()
        {

        }

        public SiteMessage(string FileType,string Message)
        {
            this.FileType = FileType;
            this.Message = Message;
        }
    }
}
