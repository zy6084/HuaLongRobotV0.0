
/*****************************************************************************************************
 *  Create Time : 2017-08-15
 *  Creator : Mervyn Deng
 *  Update Time :
 *  Revoser :
 *  Content : PC2PLCConnection
 *  Version : 1.1
 *  Remark :
 * ***************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServcerTest
{
    public class PC2PLCConnection
    {
        public byte[] SampleProperty { get; set; }
        byte[] sendbt = null;
        Params.PLC2PCCommandType rct = new Params.PLC2PCCommandType();
        public byte[] createCommand(Params.PC2PLCCommandType BTSend)
        {
            try
            {
                switch (BTSend.ToString())
                {
                    case "Active":
                        sendbt = new byte[] { 0x00, 0x01 };
                        break;
                    case "SetSample":
                        sendbt = new byte[] { 0x00, 0x02 };
                        break;
                    case "DataFeedBack":
                        sendbt = new byte[] { 0x00, 0x04 };
                        break;
                    case "WaitKey":
                        sendbt = new byte[] { 0x00, 0x08 };
                        break;
                    case "Finish":
                        sendbt = new byte[] { 0x00, 0x10 };
                        break;
                    case "Complete":
                        sendbt = new byte[] { 0x00, 0x20 };
                        break;
                    case "SamplePlan":
                        sendbt = SampleProperty;//EE 支数 厚度上限 厚度下限 FF
                        break;
                    case "TopCaliperClamp":
                        sendbt = new byte[] { 0x01, 0x00 };
                        break;
                    case "LowCaliperClamp":
                        sendbt = new byte[] { 0x02, 0x00 };
                        break;
                    case "CatchSampleRequest":
                        sendbt = new byte[] { 0x00, 0x22 };
                        break;
                    case "TopCaliperOpen":
                        sendbt = new byte[] { 0x00, 0x40 };
                        break;
                    case "LowCaliperOpen":
                        sendbt = new byte[] { 0x00, 0x80 };
                        break;
                    }
                return sendbt;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public Params.PLC2PCCommandType receiveCommand(byte[] _msg)
        {
            try
            {
                string msg = ToHexString(_msg);
                switch (msg)
                {
                    case "0008":
                        rct = Params.PLC2PCCommandType.Ready;
                        break;
                    case "0010":
                        rct = Params.PLC2PCCommandType.Operation;
                        break;
                    case "0020":
                        rct = Params.PLC2PCCommandType.CatchSample;
                        break;
                    case "0040":
                        rct = Params.PLC2PCCommandType.Complete;
                        break;
                    case "0001":
                        rct = Params.PLC2PCCommandType.UrgencyStop;
                        break;
                    case "0002":
                        rct = Params.PLC2PCCommandType.ServoMotorError;
                        break;
                    case "0004":
                        rct = Params.PLC2PCCommandType.MeasuerRrror;
                        break;
                    default:
                        rct = Params.PLC2PCCommandType.Error;
                        break;
                }
                if (msg.StartsWith("EE")) rct = Params.PLC2PCCommandType.DataSend;//EE 长 宽 高 FF
                //if (msg.StartsWith("BB")) rct = Params.PLC2PCCommandType.HeightValue;//BB 高度 FF
                return rct;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            } return hexString;
        }
    }
}
