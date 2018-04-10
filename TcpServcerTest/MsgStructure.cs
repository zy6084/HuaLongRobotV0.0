using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServcerTest
{
    public class StatusStructure
    {
        public string BeginFlag { get; set; }
        public float HeightValue { get; set; }
        public bool isPlatformMoving { get; set; }
        public bool isLowCaliperClamp { get; set; }
        public bool isTopCaliperClamp { get; set; }
        public string EndFlag { get; set; }
    }
    public class CommandStructure
    {
        public byte BeginFlag { get; set; }
        public byte EndFlag { get; set; }
        public byte CaliperMovement { get; set; }
    }
}
