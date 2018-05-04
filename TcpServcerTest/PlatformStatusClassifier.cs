
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
        public static int[] StatusClassifier(Params.ProcessName name)
        {
            try
            {
                ClassifierResults = new int[1] { 0 };
                switch (name)
                {
                    case Params.ProcessName.Ready:
                        ClassifierResults[0] = 0;
                        break;
                    case Params.ProcessName.Loading:
                        ClassifierResults[0] = 1;
                        break;
                    case Params.ProcessName.CatchSample:
                        ClassifierResults[0] = 2;
                        break;
                    case Params.ProcessName.ClampClosing:
                        ClassifierResults[0] = 3;
                        break;
                    case Params.ProcessName.Return1:
                        ClassifierResults[0] = 4;
                        break;
                    case Params.ProcessName.Test:
                        ClassifierResults[0] = 5;
                        break;
                    case Params.ProcessName.TestFinished:
                        ClassifierResults[0] = 6;
                        break;
                    case Params.ProcessName.Zq3b:
                        ClassifierResults[0] = 7;
                        break;
                    case Params.ProcessName.JQ2:
                        ClassifierResults[0] = 8;
                        break;
                    case Params.ProcessName.Return3:
                        ClassifierResults[0] = 9;
                        break;
                    case Params.ProcessName.Down:
                        ClassifierResults[0] = 10;
                        break;
                    case Params.ProcessName.Zq2b:
                        ClassifierResults[0] = 11;
                        break;                   
                    case Params.ProcessName.JQ1:
                        ClassifierResults[0] = 12;
                        break;
                    case Params.ProcessName.Return2:
                        ClassifierResults[0] = 13;
                        break;
                    case Params.ProcessName.Pause:
                        ClassifierResults[0] = 14;
                        break;
                    case Params.ProcessName.Emergency:
                        ClassifierResults[0] = 15;
                        break;
                    case Params.ProcessName.Communication:
                        ClassifierResults[0] = 16;
                        break;                   
                    }
            }
            catch (Exception ex) { log.Exception(ex); }
            return ClassifierResults;
        }
    }
}
