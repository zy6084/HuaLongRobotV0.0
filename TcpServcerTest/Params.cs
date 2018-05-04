
/*****************************************************************************************************
 *  Create Time : 2017-08-15
 *  Creator : Mervyn Deng
 *  Update Time :
 *  Revoser :
 *  Content : PC2RobotCommandType
 *  Version : 1.1
 *  Remark :
 * ***************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServcerTest
{
    public class Params
    {
        //Commands Server To Robot 
        public enum PC2RobotCommandType
        {
            Error, Active, ZQ1B, Return1, ZQ2B, Return2, ZBAXIS, ZQ3B, Return3, Break, ZB, JQ1, JQ2,M1,M2,M3
        }
        //Commands Robot To Server
        public enum Robot2PCCommandType
        {
            Error, Connection, Ready, ZQ1F, FeedBack1, ZQ2F, FeedBack2, ZBOK, ZQ3F, FeedBack3, Finish, DW1, DW2
        }
        //Commands Server To PLC
        public enum PC2PLCCommandType
        {
            Error, Active, SetSample, DataFeedBack, WaitKey, Finish, Complete, SamplePlan, TopCaliperOpen, LowCaliperOpen, CatchSampleRequest, PS, PR
            }
        //Commands PLC To Server
        public enum PLC2PCCommandType
        {
            Error, Ready, Operation, CatchSample, Complete, UrgencyStop, ServoMotorError, MeasuerRrror, DataSend,PSF, PRF,CoderER,PLCER,RobotER, IntrusionDetectionER,DevER,CoderERF, PLCERF, RobotERF, IntrusionDetectionERF, DevERF
            }
        public enum Coder2PCCommandType
        {
            Error, Ready, ReadyStatus, DoStatus, CompleteStatus, HeightValue, TopCaliperClampDone, LowCaliperClampDone, TopCaliperOpenDone, LowCaliperOpenDone, PtrZeroStatus, CaliperPositionUp, CaliperPositionDown, UpToZeroPoint, CaliperAllOpen, CaliperAllClamp, TopCaliperOpen, LowCaliperOpen
            }
        public enum PC2CoderCommandType
        {
            Error, Active, TopCaliperClamp, LowCaliperClamp, GrabHeight, TopCaliperOpen, LowCaliperOpen, GrabHeightDone
            }

        public string MsgCodeContent()
        {
            string content = string.Empty;
            return content;
        }

        public enum MouseMovementType
        {
            ToUp = 0, ToDown = 1, ToStop = 2, ToStart = 3, ToSave = 4
        }

        //public enum PlatformStatus
        //{
        //    TopCaliperClamp = 0, LowCaliperClamp = 1, TopCaliperClampDone = 2, LowCaliperClampDone = 3, TopCaliperOpen = 4, LowCaliperOpen = 5, TopCaliperOpenDone = 6, LowCaliperOpenDone = 7,PlatformAscending=8, PlatformDescending = 9,PlatformStop=10
        //}

        public enum ProcessName
            {
            Ready = 0, Loading = 1, CatchSample = 2, ClampClosing = 3, Return1=4,Test = 5, TestFinished = 6, Zq3b=7, JQ2 = 8, Return3 = 9, Down = 10, Zq2b =11, JQ1 = 12, Return2 = 13, Pause = 14, Emergency = 15,Communication=16
            }
    }
}
