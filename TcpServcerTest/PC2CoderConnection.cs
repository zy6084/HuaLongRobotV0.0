
/*****************************************************************************************************
 *  Create Time : 2017-08-14
 *  Creator : Mervyn Deng
 *  Update Time :
 *  Revoser :
 *  Content : PC2CoderConnection
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
    public class PC2CoderConnection
    {
        public string AxisValue { get; set; }//Z轴偏移量
        byte[] sendbt = null;
        Params.Coder2PCCommandType rct = new Params.Coder2PCCommandType();

        public byte[] createCommand(Params.PC2CoderCommandType BTSend)
        {
            try
            {
                switch (BTSend.ToString())
                {
                    case "Active":
                        sendbt = new byte[] { 0x00, 0x01 };
                        break;
                    case "GrabHeight":
                        sendbt = new byte[] { 0x04, 0x00 };
                        break;
                    case "TopCaliperClamp":
                        sendbt = new byte[] { 0x01, 0x00 };
                        break;
                    case "LowCaliperClamp":
                        sendbt = new byte[] { 0x02, 0x00 };
                        break;
                    case "TopCaliperOpen":
                        sendbt = new byte[] { 0x00, 0x40 };
                        break;
                    case "LowCaliperOpen":
                        sendbt = new byte[] { 0x00, 0x80 };
                        break;
                    case "GrabHeightDone":
                        sendbt = new byte[] { 0x04, 0x20 };
                        break;
                    case "ER":
                        sendbt = new byte[] { 0x00,0xA0};
                        break;
                    case "ES":
                        sendbt = new byte[] { 0x00, 0x0A };
                        break;
                    case "PS":
                        sendbt = new byte[] { 0x00, 0x0B };
                        break;
                    case "PR":
                        sendbt = new byte[] { 0x00, 0xB0 };
                        break;
                    }
                return sendbt;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public Params.Coder2PCCommandType receiveCommand(byte[] _msg)
        {
            try
            {
                string msg = ToHexString(_msg);
                switch (msg)
                {
                    case "0080":
                        rct = Params.Coder2PCCommandType.ReadyStatus;
                        break;
                    case "0100":
                        rct = Params.Coder2PCCommandType.DoStatus;
                        break;
                    case "0200":
                        rct = Params.Coder2PCCommandType.CompleteStatus;
                        break;
                    case "0400":
                        rct = Params.Coder2PCCommandType.TopCaliperClampDone;
                        break;
                    case "0800":
                        rct = Params.Coder2PCCommandType.LowCaliperClampDone;
                        break;
                    case "1000":
                        rct = Params.Coder2PCCommandType.TopCaliperOpenDone;
                        break;
                    case "2000":
                        rct = Params.Coder2PCCommandType.LowCaliperOpenDone;
                        break;
                    case "4000":
                        rct = Params.Coder2PCCommandType.PtrZeroStatus;
                        break;
                    case "8000":
                        rct = Params.Coder2PCCommandType.CaliperPositionUp;
                        break;
                    case "C000":
                        rct = Params.Coder2PCCommandType.CaliperPositionDown;
                        break;
                    case "4100":
                        rct = Params.Coder2PCCommandType.UpToZeroPoint;
                        break;
                    case "0045":
                        rct = Params.Coder2PCCommandType.CaliperAllOpen;
                        break;
                    case "0047":
                        rct = Params.Coder2PCCommandType.CaliperAllClamp;
                        break;
                    case "0048":
                        rct = Params.Coder2PCCommandType.TopCaliperOpen;
                        break;
                    case "0049":
                        rct = Params.Coder2PCCommandType.LowCaliperOpen;
                        break;
                    case "00B1":
                        rct = Params.Coder2PCCommandType.PRF;
                        break;
                    case "00A1":
                        rct = Params.Coder2PCCommandType.ESF;
                        break;
                    default:
                        rct = Params.Coder2PCCommandType.Error;
                        break;
                }
                if (msg.StartsWith("BB")) rct = Params.Coder2PCCommandType.HeightValue;//BB 高度 FF
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
