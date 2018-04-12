
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
            Error, Active, ZQ1B, Return1, ZQ2B, Return2, ZBAXIS, ZQ3B, Return3, Break, ZB, JQ1, JQ2,M1,M2,M3,PS,PR,ES,ER,Manual
        }
        //Commands Robot To Server
        public enum Robot2PCCommandType
        {
            Error, Connection, Ready, ZQ1F, FeedBack1, ZQ2F, FeedBack2, ZBOK, ZQ3F, FeedBack3, Finish, DW1, DW2,PSF,PRF,ESF,ERF,ManualFeedBack
        }
        //Commands Server To PLC
        public enum PC2PLCCommandType
        {
            Error, Active, SetSample, DataFeedBack, WaitKey, Finish, Complete, SamplePlan, TopCaliperOpen, LowCaliperOpen, CatchSampleRequest, PS, PR, ES, ER
            }
        //Commands PLC To Server
        public enum PLC2PCCommandType
        {
            Error, Ready, Operation, CatchSample, Complete, UrgencyStop, ServoMotorError, MeasuerRrror, DataSend,PSF, PRF, ESF, ERF,CoderER,PLCER,RobotER, IntrusionDetectionER,DevER
            }
        public enum Coder2PCCommandType
        {
            Error, Ready, ReadyStatus, DoStatus, CompleteStatus, HeightValue, TopCaliperClampDone, LowCaliperClampDone, TopCaliperOpenDone, LowCaliperOpenDone, PtrZeroStatus, CaliperPositionUp, CaliperPositionDown, UpToZeroPoint, CaliperAllOpen, CaliperAllClamp, TopCaliperOpen, LowCaliperOpen, PSF, PRF, ESF, ERF
            }
        public enum PC2CoderCommandType
        {
            Error, Active, TopCaliperClamp, LowCaliperClamp, GrabHeight, TopCaliperOpen, LowCaliperOpen, GrabHeightDone,PS, PR, ES, ER
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

        public enum PlatformStatus
        {
            ReadyStatus = 0, DoStatus = 1, CompleteStatus = 2, TopCaliperClamp = 3, LowCaliperClamp = 4, TopCaliperClampDone = 5, LowCaliperClampDone = 6, TopCaliperOpen = 7, LowCaliperOpen = 8, TopCaliperOpenDone = 9, LowCaliperOpenDone = 10, CaliperPositionDown = 11, CaliperPositionUp = 12, PtrZeroStatus = 13,PauseStatus=14,EmergencyStop=15
        }
    }
}
