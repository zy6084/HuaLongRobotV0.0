using Gimela.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServcerTest
{
    public class MsgAnaly
    {
        public StatusStructure StatusAnalysis(TcpDatagramReceivedEventArgs<byte[]> e)
        {
            try
            {
                StatusStructure ss = new StatusStructure();
                ss.BeginFlag = CommonFunction.byteToHexString(new byte[] { e.Datagram[0] });
                ss.HeightValue = BitConverter.ToSingle(new byte[] { e.Datagram[4], e.Datagram[3], e.Datagram[2], e.Datagram[1] }, 0);
                byte[] status = new byte[] { e.Datagram[5], 0 };
                switch (CommonFunction.byteToHexString(status))
                {
                    case "0000":
                        ss.isPlatformMoving = false;
                        ss.isLowCaliperClamp = true;
                        ss.isTopCaliperClamp = true;
                        break;
                    case "0100":
                        ss.isPlatformMoving = false;
                        ss.isLowCaliperClamp = true;
                        ss.isTopCaliperClamp = false;
                        break;
                    case "0200":
                        ss.isPlatformMoving = false;
                        ss.isLowCaliperClamp = false;
                        ss.isTopCaliperClamp = true;
                        break;
                    case "0400":
                        ss.isPlatformMoving = true;
                        ss.isLowCaliperClamp = true;
                        ss.isTopCaliperClamp = true;
                        break;
                    case "0300":
                        ss.isPlatformMoving = false;
                        ss.isLowCaliperClamp = false;
                        ss.isTopCaliperClamp = false;
                        break;
                    case "0700":
                        ss.isPlatformMoving = true;
                        ss.isLowCaliperClamp = false;
                        ss.isTopCaliperClamp = false;
                        break;
                    case "0600":
                        ss.isPlatformMoving = true;
                        ss.isLowCaliperClamp = false;
                        ss.isTopCaliperClamp = true;
                        break;
                }
                ss.EndFlag = CommonFunction.byteToHexString(new byte[] { e.Datagram[6] });
                return ss;
            }
            catch (Exception ex) { throw ex; }
        }

        public byte[] CommandAnalysis(Params.PC2CoderCommandType p2c)
        {
            CommandStructure cs = new CommandStructure();
            cs.BeginFlag = 0xFF;
            switch (p2c)
            {
                case Params.PC2CoderCommandType.LowCaliperClamp:
                    cs.CaliperMovement = 0x08;
                    break;
                case Params.PC2CoderCommandType.LowCaliperOpen:
                    cs.CaliperMovement = 0x02;
                    break;
                case Params.PC2CoderCommandType.TopCaliperClamp:
                    cs.CaliperMovement = 0x04;
                    break;
                case Params.PC2CoderCommandType.TopCaliperOpen:
                    cs.CaliperMovement = 0x01;
                    break;
            }
            cs.EndFlag = 0x7F;
            byte[] sendbt = new byte[] { cs.BeginFlag, cs.CaliperMovement, cs.EndFlag };
            return sendbt;
        }
    }
}
