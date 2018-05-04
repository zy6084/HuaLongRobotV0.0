
/*****************************************************************************************************
 *  Create Time : 2017-08-15
 *  Creator : Mervyn Deng
 *  Update Time :
 *  Revoser :
 *  Content : PC2RobotConnection
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
    public class PC2RobotConnection
    {
        public string AxisValue { get; set; }//Z轴偏移量
        StringBuilder sendStb = new StringBuilder();
        Params.Robot2PCCommandType rct = new Params.Robot2PCCommandType();
        public string createCommand(Params.PC2RobotCommandType BTSend)
        {
            try
            {
                sendStb.Clear();
                switch (BTSend.ToString())
                {
                    case "Active":
                        sendStb.Append("active");
                        break;
                    case "ZQ1B":
                        sendStb.Append("zq1b");
                        break;
                    case "Return1":
                        sendStb.Append("return1");
                        break;
                    case "ZQ2B":
                        sendStb.Append("zq2b");
                        break;
                    case "Return2":
                        sendStb.Append("return2");
                        break;
                    case "ZB":
                        sendStb.Append("zb");
                        break;
                    case "ZBAXIS":
                        sendStb.Append("zb"+AxisValue);
                        break;
                    case "ZQ3B":
                        sendStb.Append("zq3b");
                        break;
                    case "Return3":
                        sendStb.Append("return3");
                        break;
                    case "Break":
                        sendStb.Append("break");
                        break;
                    case "JQ1":
                        sendStb.Append("jq1");
                        break;
                    case "JQ2":
                        sendStb.Append("jq2");
                        break;
                    case "M1":
                        sendStb.Append("m1");
                        break;
                    case "M2":
                        sendStb.Append("m2");
                        break;
                    case "M3":
                        sendStb.Append("m3");
                        break;    
                    }
                return sendStb.ToString();
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        public Params.Robot2PCCommandType receiveCommand(string _msg)
        {
            try
            {
                switch (_msg)
                {
                    case "con":
                        rct = Params.Robot2PCCommandType.Connection;
                        break;
                    case "ready":
                        rct = Params.Robot2PCCommandType.Ready;
                        break;
                    case "zq1f":
                        rct = Params.Robot2PCCommandType.ZQ1F;
                        break;
                    case "feedback1":
                        rct = Params.Robot2PCCommandType.FeedBack1;
                        break;
                    case "zq2f":
                        rct = Params.Robot2PCCommandType.ZQ2F;
                        break;
                    case "feedback2":
                        rct = Params.Robot2PCCommandType.FeedBack2;
                        break;
                    case "zbok":
                        rct = Params.Robot2PCCommandType.ZBOK;
                        break;
                    case "zq3f":
                        rct = Params.Robot2PCCommandType.ZQ3F;
                        break;
                    case "feedback3":
                        rct = Params.Robot2PCCommandType.FeedBack3;
                        break;
                    case "finish":
                        rct = Params.Robot2PCCommandType.Finish;
                        break;
                    case "dw1":
                        rct = Params.Robot2PCCommandType.DW1;
                        break;
                    case "dw2":
                        rct = Params.Robot2PCCommandType.DW2;
                        break; 
                    default:
                        rct = Params.Robot2PCCommandType.Error;
                        break;
                }
                return rct;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }
    }
}
