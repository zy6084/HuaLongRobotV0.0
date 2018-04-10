
/*****************************************************************************************************
 *  Create Time : 2017-07-15
 *  Creator : Mervyn Deng
 *  Update Time :
 *  Revoser :
 *  Content : PlatformStatusClassifier
 *  Version : 1.1
 *  Remark :
 * ***************************************************************************************************/
using Gimela.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpServcerTest
{
    public static class PlatformStatusClassifier
    {
        static ILogger log = null;
        public static int[] ClassifierResults { get; set; }
        public static int[] StatusClassifier(Params.PlatformStatus _status)
        {
            try
            {
                ClassifierResults = new int[1] { 0 };
                switch (_status)
                {
                    case Params.PlatformStatus.CaliperPositionUp:
                        ClassifierResults[0] = 12;
                        break;
                    case Params.PlatformStatus.CaliperPositionDown:
                        ClassifierResults[0] = 11;
                        break;
                    case Params.PlatformStatus.PtrZeroStatus:
                        ClassifierResults[0] = 13;
                        break;
                    case Params.PlatformStatus.TopCaliperClamp:
                        ClassifierResults[0] = 3;
                        break;
                    case Params.PlatformStatus.TopCaliperClampDone:
                        ClassifierResults[0] = 5;
                        break;
                    case Params.PlatformStatus.LowCaliperClamp:
                        ClassifierResults[0] = 4;
                        break;
                    case Params.PlatformStatus.LowCaliperClampDone:
                        ClassifierResults[0] = 6;
                        break;
                    case Params.PlatformStatus.TopCaliperOpen:
                        ClassifierResults[0] = 7;
                        break;
                    case Params.PlatformStatus.TopCaliperOpenDone:
                        ClassifierResults[0] = 9;
                        break;
                    case Params.PlatformStatus.LowCaliperOpen:
                        ClassifierResults[0] = 8;
                        break;
                    case Params.PlatformStatus.LowCaliperOpenDone:
                        ClassifierResults[0] = 10;
                        break;
                }
            }
            catch (Exception ex) { log.Exception(ex); }
            return ClassifierResults;
        }
    }
}
