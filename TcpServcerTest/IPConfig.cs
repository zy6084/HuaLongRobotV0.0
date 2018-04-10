using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServcerTest
{
    public class IPConfig : List<string>
    {
        public Dictionary<string, string> iPConfig = new Dictionary<string, string>();
        public IPConfig()
        {
            iPConfig.Add(Properties.Settings.Default.样件架PLC端, "样件架PLC端");
            iPConfig.Add(Properties.Settings.Default.Robot端, "Robot端");
            iPConfig.Add(Properties.Settings.Default.拉力机PLC端, "拉力机PLC端");
            foreach (KeyValuePair<string, string> kvp in iPConfig)
            {
                this.Add(kvp.Key);
            }
        }
    }
}
