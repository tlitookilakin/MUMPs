using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUMPs.models
{
    class MessageRepairEvent
    {
        public string EventName { get; set; }
        public string LocationName { get; set; }
        public MessageRepairEvent(string location = null, string Event = null)
        {
            EventName = Event;
            LocationName = location;
        }
    }
}
