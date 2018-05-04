/*****************************************************************************************************
 *  Create Time : 2017-08-18
 *  Creator : Mervyn Deng
 *  Modifier：Azeroth
 *  Modifiy Time :2018-04-04
 *  Revoser :
 *  Content : Connection Service
 *  Version : 1.0
 *  Remark : 
 * ***************************************************************************************************/
using Gimela.Common.Logging;
using Gimela.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Data;
using System.Data.OleDb;
using System.Collections.Concurrent;

namespace TcpServcerTest
{
    public partial class FrmMain : Form
        {
        #region Params
        static AsyncTcpServer server;
        static TcpClient clientRobot;
        static TcpClient clientPLC;
        static TcpClient clientCoder;
        PC2RobotConnection rd = new PC2RobotConnection();
        PC2PLCConnection pd = new PC2PLCConnection();
        PC2CoderConnection cd = new PC2CoderConnection();
        public delegate void UpdateDisplayDelegate(string _msg);//数据刷新委托
        public delegate void CheckStateControlDelegate(int[] _seq);//流程刷新委托
        ConcurrentDictionary<string, bool> dic = new ConcurrentDictionary<string, bool>();
        FrmManualMode frmMM = new FrmManualMode();
        public void UpdateDisplay(string msg)
            {
            txtLog.Text += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + msg + "\r\n";
            //txtLog.Focus();//获取焦点
            txtLog.Select(txtLog.TextLength, 0);//光标定位到文本最后
            txtLog.ScrollToCaret();//滚动到光标处}
            }
        public void HeightDisplay(string value) { this.txtUPlatformHeightValue.Text = value; }
        public void CheckStateControl(int[] _seq)
            {
            for (int i = 0; i < checkedListBoxControl2.Items.Count; i++)
                this.checkedListBoxControl2.Items[i].CheckState = CheckState.Unchecked;
            foreach (var item in _seq)
                this.checkedListBoxControl2.Items[item].CheckState = CheckState.Checked;
            }
        decimal SampleLength = 0;
        decimal SampleDiameter = 0;
        float V_Height = 0;
        int i = 0;
        string fracturePositionResult = string.Empty;
        int positionFlag = 0;
        public List<string> strList = new List<string>();
        List<FileTimeInfo> ftl = null;

        MouseHook mh;

        FileLogFactory flf = new FileLogFactory();

        ILogger log = null;

        IPConfig ipC = new IPConfig();

        MsgAnaly ma = new MsgAnaly();
        StatusStructure ss = null;
        byte[] sendbt = null;
        #endregion

        public FrmMain()
            {
            log = flf.CreateLogger();
            try
                {
                InitializeComponent();
                dic = new ProcessControVariables().SetKeyValue();
                LogFactory.Assign(new ConsoleLogFactory());
                // 异步通讯初始化 端口10001 编码UTF8
                server = new AsyncTcpServer(10001);
                server.Encoding = Encoding.UTF8;
                // 客户端连接事件
                server.ClientConnected +=
                  new EventHandler<TcpClientConnectedEventArgs>(server_ClientConnected);
                // 客户端断开事件
                server.ClientDisconnected +=
                  new EventHandler<TcpClientDisconnectedEventArgs>(server_ClientDisconnected);
                // 接受string类型电文事件
                server.PlaintextReceived +=
                  new EventHandler<TcpDatagramReceivedEventArgs<string>>(server_PlaintextReceived);
                // 接受byte[]类型电文事件
                server.DatagramReceived += new EventHandler<TcpDatagramReceivedEventArgs<byte[]>>(server_DatagramReceived);
                frmMM.ManualOperationOrders += FrmMM_ManualOperationOrders;
                // 启动服务程序
                server.Start();
                }
            catch (Exception ex) { log.Exception(ex); }
            }


        #region 客户端连接事件
        void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
            {
            try
                {
                bool PLCComm = false; bool CoderComm = false; bool RobotComm = false;
                IPEndPoint ClientIP = e.TcpClient.Client.RemoteEndPoint as IPEndPoint;
                if (ClientIP.Address.ToString() == Properties.Settings.Default.样件架PLC端)
                    {
                    clientPLC = e.TcpClient;
                    PLCComm = true;
                    }
                if (ClientIP.Address.ToString() == Properties.Settings.Default.拉力机PLC端)
                    {
                    clientCoder = e.TcpClient;
                    CoderComm = true;
                    }

                if (ClientIP.Address.ToString() == Properties.Settings.Default.Robot端)
                    {
                    clientRobot = e.TcpClient;
                    RobotComm = true;
                    }
                if (PLCComm && CoderComm && RobotComm)
                    {
                    LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has connected.", null, dic["测试命令模式"]);
                    BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new Object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Communication) });
                    }
                }
            catch (Exception ex) { log.Exception(ex); }
            }

        private void FrmMM_ManualOperationOrders(object sender, OrderTypeEventArgs e)
            {
            switch (e.orderName)
                {
                case "ZQ1B":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ1B),false);
                    break;
                case "Return1":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return1), false);
                    break;
                case "ZQ2B":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ2B), false);
                    break;
                case "Return2":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return2), false);
                    break;
                case "ZQ3B":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B), false);
                    break;
                case "Return3":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return3), false);
                    break;
                case "JQ1":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ1), false);
                    break;
                case "JQ2":
                    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ2), false);
                    break;
                case "TopCaliperClamp":
                    MsgSend(clientCoder, new byte[] { 0x01, 0x00 }, false);
                    break;
                case "LowCaliperClamp":
                    MsgSend(clientCoder, new byte[] { 0x02, 0x00 }, false);
                    break;
                case "TopCaliperOpen":
                    MsgSend(clientCoder, new byte[] { 0x00, 0x40 }, false);
                    break;
                case "LowCaliperOpen":
                    MsgSend(clientCoder, new byte[] { 0x00, 0x80 }, false);
                    break;
                default:
                    break;
                }
            }
        #endregion

        #region 客户端断开事件
        void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
            {
            IPEndPoint ClientIP = (IPEndPoint)e.TcpClient.Client.RemoteEndPoint;
            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has disconnected.", null, dic["测试命令模式"]);
            }
        #endregion

        #region byte[]类型电文接受事件（PLC端通讯）
        void server_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
            {
            IPEndPoint ClientIP = (IPEndPoint)e.TcpClient.Client.RemoteEndPoint;
            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + "  has sent messages:", CommonFunction.byteToHexString(e.Datagram), dic["测试命令模式"]);
            try
                {
                #region 样架PLC端
                if (e.TcpClient == clientPLC)
                    {
                    LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + "  has sent messages:", CommonFunction.byteToHexString(e.Datagram), dic["测试命令模式"]);
                    switch (pd.receiveCommand(e.Datagram))
                        {
                        case Params.PLC2PCCommandType.Ready:
                            dic["上夹钳闭合"] = false;
                            dic["下夹钳闭合"] = false;
                            dic["批次试验完成"] = false;
                            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Active), dic["暂停模式选择"]);
                            tsbAdd_Click(null, null);
                            break;
                        case Params.PLC2PCCommandType.Operation:
                            break;
                        case Params.PLC2PCCommandType.DataSend:
                            dic["上夹钳闭合"] = false;
                            dic["下夹钳闭合"] = false;
                            dic["批次试验完成"] = false;
                            SampleDiameter = seDiameter.Value;
                            SampleLength = seLength.Value;
                            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.DataFeedBack), dic["暂停模式选择"]);
                            break;
                        case Params.PLC2PCCommandType.CatchSample:
                            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.WaitKey), dic["暂停模式选择"]);
                            if (dic["Robot就绪"] && !dic["从样件架接料并夹持"] && dic["U型架升降到位"] && !dic["批次试验完毕"])
                                {

                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ1B), dic["暂停模式选择"]);
                                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new Object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.CatchSample) });
                                dic["从样件架接料并夹持"] = true;
                                }
                            break;
                        case Params.PLC2PCCommandType.Complete:
                            dic["批次试验完毕"] = true; //该批次实验完毕
                            break;
                        case Params.PLC2PCCommandType.UrgencyStop:
                            break;
                        case Params.PLC2PCCommandType.ServoMotorError:
                            break;
                        case Params.PLC2PCCommandType.MeasuerRrror:
                            break;
                        case Params.PLC2PCCommandType.Error:
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + CommonFunction.byteToHexString(e.Datagram), " [Receive Unvalid Data!Please Check the PLC's Status]", dic["测试命令模式"]);
                            break;
                        case Params.PLC2PCCommandType.CoderER:
                            dic["有急停信号"] = true;
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + CommonFunction.byteToHexString(e.Datagram), " [样件架触发急停！请检查相关设备！]", false);
                            break;
                        case Params.PLC2PCCommandType.DevER:
                            dic["有急停信号"] = true;
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + CommonFunction.byteToHexString(e.Datagram), " [伺服电机触发急停！请检查相关设备！]", false);
                            break;
                        case Params.PLC2PCCommandType.IntrusionDetectionER:
                            dic["有急停信号"] = true;
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + CommonFunction.byteToHexString(e.Datagram), " [入侵检测触发急停！请检查相关设备！]", false);
                            break;
                        case Params.PLC2PCCommandType.RobotER:
                            dic["有急停信号"] = true;                                                 
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + CommonFunction.byteToHexString(e.Datagram), " [机器人触发急停！请检查相关设备！]", false);
                            break;
                        }
                    }
                #endregion

                #region 拉力机PLC端
                else if (e.TcpClient == clientCoder)
                    {
                    if (dic["系统开始运行"]) //系统开始运行
                        {
                        ss = ma.StatusAnalysis(e);
                        BeginInvoke(new UpdateDisplayDelegate(HeightDisplay), new object[] { ss.HeightValue.ToString() });
                        if (ss.HeightValue >= 1200)
                            {
                            MouseMovementControl(Params.MouseMovementType.ToStop, dic["暂停模式选择"]);
                            MessageBox.Show("拉力机移动到达上限位，请确认系统状态！");
                            }
                        if (dic["开始试验"] && dic["试验完毕"] && dic["拉断后U型架抬升完成"])
                            {
                            if (positionFlag == 1)
                                {
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.M1), dic["暂停模式选择"]);
                                Thread.Sleep(1000);
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B), dic["暂停模式选择"]);
                                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new Object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Zq3b) });
                                }
                            else if (positionFlag == 2)
                                {
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.M2), dic["暂停模式选择"]);
                                Thread.Sleep(1000);
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B), dic["暂停模式选择"]);
                                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new Object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Zq3b) });
                                }
                            }
                        if (!dic["样件上半段下料至夹取位"] && dic["开始试验"] && dic["试验完毕"] && dic["拉断后U型架抬升完成"]) //获取上升高度，命令机器人抓取
                            {
                            if ((positionFlag == 1 && dic["样件下半段下料至夹取位反馈"]) || positionFlag == 3)
                                {
                                if (!dic["U型架正在下降"])
                                    {
                                    if (!ss.isPlatformMoving)
                                        {
                                        MouseMovementControl(Params.MouseMovementType.ToDown, dic["暂停模式选择"]); BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new Object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Down) });
                                        }
                                    else dic["U型架正在下降"] = true;
                                    }
                                for (; ss.HeightValue < 800;)
                                    {
                                    MouseMovementControl(Params.MouseMovementType.ToStop, dic["暂停模式选择"]);
                                    //tsbRead_Click(null, null);
                                    dic["U型架下降到位"] = true;
                                    break;
                                    }
                                }

                            if (dic["U型架下降到位"])
                                {
                                if (positionFlag != 0)
                                    {
                                    if (positionFlag == 1 || positionFlag == 3)
                                        {
                                        rd.AxisValue = (ss.HeightValue - float.Parse(seZBHeight.Value.ToString())).ToString();
                                        MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZBAXIS), dic["暂停模式选择"]);
                                        Thread.Sleep(1000);
                                        MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ2B), dic["暂停模式选择"]);
                                        BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Zq2b) });
                                        }
                                    dic["样件上半段下料至夹取位"] = true;
                                    }
                                else
                                    {
                                    LogRefresh("未能正确获得断裂位置！", null, dic["测试命令模式"]);
                                    MessageBox.Show("未能正确获得断裂位置！");
                                    }
                                }
                            }
                        else if (!dic["放入拉力机并返回反馈"] && !dic["拉力机准备就绪"]) //初始化
                            {
                            #region 初始化逻辑修改：防止上下夹钳先后打开造成死机的情况发生，修改为先打开下夹钳，然后下降U型台，最后打开上夹钳的过程
                            //调整下夹钳
                            if (ss.isLowCaliperClamp)//下夹钳关闭
                                {
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperOpen), dic["暂停模式选择"]);
                                }
                            if (!ss.isLowCaliperClamp)
                                {
                                dic["下夹钳打开"] = true;//下夹钳打开
                                }

                            //调整上台高度
                            if ((ss.HeightValue > 805) || (ss.HeightValue < 795))
                                {
                                if (dic["下夹钳打开"])//下夹钳已打开完成
                                    {
                                    if (ss.HeightValue > 805)//下降
                                        {
                                        if (!dic["U型架上升命令触发"])
                                            {
                                            if (!ss.isPlatformMoving)
                                                {
                                                MouseMovementControl(Params.MouseMovementType.ToDown, dic["暂停模式选择"]);
                                                }

                                            else dic["U型架上升命令触发"] = true;
                                            }
                                        }
                                    if (ss.HeightValue < 795)//上升
                                        {
                                        if (!dic["U型架下降命令触发"])
                                            {
                                            if (!ss.isPlatformMoving)
                                                {
                                                MouseMovementControl(Params.MouseMovementType.ToUp, dic["暂停模式选择"]);
                                                }
                                            else dic["U型架下降命令触发"] = true;
                                            }
                                        }
                                    }
                                }
                            else
                                {
                                MouseMovementControl(Params.MouseMovementType.ToStop, dic["暂停模式选择"]);
                                dic["U型架升降到位"] = true;
                                }

                            //调整上夹钳
                            if (ss.isTopCaliperClamp && dic["下夹钳打开"] && dic["U型架升降到位"])
                                {
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperOpen), dic["暂停模式选择"]);
                                }
                            if (!ss.isTopCaliperClamp)
                                {
                                dic["上夹钳打开"] = true;
                                }


                            //全部调整完毕以后，执行后续过程
                            if (dic["上夹钳打开"] && dic["下夹钳打开"] && dic["U型架升降到位"])
                                {
                                MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.CatchSampleRequest), dic["暂停模式选择"]);
                                tsbAdd_Click(null, null);
                                dic["拉力机准备就绪"] = true;
                                }
                            #endregion
                            }
                        else if (dic["样件架样件夹持反馈"] && !dic["拉断后U型架抬升完成"]) //机器人抓取样件完成，送入拉力机位置
                            {
                            if (ss.isTopCaliperClamp)
                                {
                                Thread.Sleep(3000);
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return1), dic["暂停模式选择"]);
                                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Return1) });
                                }//上夹钳闭合，机器人退回
                            else
                                {
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperClamp), dic["暂停模式选择"]);
                                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.ClampClosing) });
                                }

                            if (dic["放入拉力机并返回反馈"])//机器人放入完成
                                {
                                if (!ss.isLowCaliperClamp)
                                    {
                                    MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperClamp), dic["暂停模式选择"]);
                                    BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.ClampClosing) });
                                    }

                                if (ss.isLowCaliperClamp && ss.isTopCaliperClamp && !dic["开始试验"]) //开始试验
                                    {
                                    if (Properties.Settings.Default.类别 == "1")
                                        {
                                        //试验版本
                                        MouseMovementControl(Params.MouseMovementType.ToStart, dic["暂停模式选择"]);
                                        if (ss.isPlatformMoving)
                                            {
                                            dic["开始试验"] = true;
                                            BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Test) });
                                            }
                                        }
                                    else
                                        {
                                        //模拟测试版本
                                        if (!dic["开始试验"])
                                            {
                                            MouseMovementControl(Params.MouseMovementType.ToUp, dic["暂停模式选择"]);
                                            BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Test) });
                                            }

                                        if (ss.isPlatformMoving)
                                            {
                                            dic["开始试验"] = true;
                                            if (ss.HeightValue > 830)
                                                {
                                                MouseMovementControl(Params.MouseMovementType.ToStop, dic["暂停模式选择"]);
                                                }
                                            }
                                        }
                                    }
                                if (dic["开始试验"])
                                    {
                                    if (ss.isPlatformMoving)
                                        dic["U型架正在移动"] = true;
                                    else
                                        {
                                        i++;
                                        if (i > 5)
                                            dic["U型架移动到位并停止"] = true;
                                        BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.TestFinished) });
                                        }
                                    }
                                }
                            if (dic["放入拉力机并返回反馈"] && dic["U型架正在移动"] && dic["U型架移动到位并停止"])//机器人放入样件完成，已经移动且停止（试验完毕）
                                {
                                if (!dic["U型架当前高度获得"])
                                    {
                                    V_Height = ss.HeightValue;
                                    dic["U型架当前高度获得"] = true;
                                    }
                                dic["试验完毕"] = true;
                                if (Properties.Settings.Default.类别 == "1")
                                    {
                                    MouseMovementControl(Params.MouseMovementType.ToSave, dic["暂停模式选择"]);
                                    Thread.Sleep(2000);
                                    tsbRead_Click(null, null);
                                    if (dic["试验数据保存完成"])
                                        {
                                        fracturePositionResult = dt.Rows[0][53].ToString();//“需要根据数据库文件就行索引修改”
                                        switch (fracturePositionResult)
                                            {
                                            case "中间":
                                                positionFlag = 1;
                                                break;
                                            case "断于下钳口":
                                                positionFlag = 2;
                                                break;
                                            case "断于上钳口":
                                                positionFlag = 3;
                                                break;
                                            }
                                        }
                                    }
                                dic["拉断后U型架抬升完成"] = true;
                                }
                            }

                        if (dic["样件上半段下料至夹取位反馈"] && !ss.isTopCaliperClamp) //机器人取出上断裂样件后返回
                            {
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return2), dic["暂停模式选择"]);
                            BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Return2) });
                            }

                        if (dic["样件下半段下料至夹取位反馈"] && !ss.isLowCaliperClamp) //机器人取出下断裂样件后返回
                            {
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return3), dic["暂停模式选择"]);
                            BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Return3) });
                            }
                        if (dic["样件上半段夹取位到达"])
                            {
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ1), dic["暂停模式选择"]);
                            BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.JQ1) });
                            if (ss.isTopCaliperClamp)
                                {
                                Thread.Sleep(10000);
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperOpen), dic["暂停模式选择"]);
                                }
                            }
                        if (dic["样件下半段夹取位到达"])
                            {
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ2), dic["暂停模式选择"]);
                            BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.JQ2) });
                            if (ss.isLowCaliperClamp)
                                {
                                Thread.Sleep(10000);
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperOpen), dic["暂停模式选择"]);
                                }
                            }
                        }
                    }
                #endregion
                }
            catch (Exception ex)
                {
                LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + " has some errors:", ex.Message, dic["测试命令模式"]);
                }
            }
        #endregion

        #region string类型电文接受事件（ROBOT端通讯）
        void server_PlaintextReceived(object sender, TcpDatagramReceivedEventArgs<string> e)
            {
            IPEndPoint ClientIP = (IPEndPoint)e.TcpClient.Client.RemoteEndPoint;
            try
                {
                if (e.TcpClient == clientRobot)
                    {
                    LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages:", e.Datagram, dic["测试命令模式"]);
                    switch (rd.receiveCommand(e.Datagram))
                        {
                        case Params.Robot2PCCommandType.Connection:
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Active), dic["暂停模式选择"]);
                            dic["样件下半段下料完成反馈"] = false;
                            dic["Rotbot就绪"] = false;
                            dic["样件架样件夹持反馈"] = false;
                            dic["样件上半段下料至夹取位反馈"] = false;
                            dic["样件下半段下料至夹取位反馈"] = false;
                            dic["从样件架接料并夹持"] = false;
                            dic["样件上半段下料至夹取位"] = false;
                            dic["样件下半段下料至夹取位"] = false;
                            dic["样件下半段下料至夹取位"] = false;
                            dic["样件下半段下料完成反馈"] = false;
                            dic["试验数据保存完成"] = false;
                            dic["试验完毕"] = false;
                            dic["U型架移动到位并停止"] = false;
                            dic["拉断后U型架抬升完成"] = false;
                            dic["开始试验"] = false;
                            dic["拉力机准备就绪"] = false;
                            dic["样件上半段夹取位到达"] = false;
                            dic["样件下半段夹取位到达"] = false;
                            dic["U型架当前高度获得"] = false;
                            dic["U型架正在下降"] = false;
                            dic["U型架下降到位"] = false;
                            positionFlag = 0;
                            rd.AxisValue = "0.0";
                            i = 0;
                            break;
                        case Params.Robot2PCCommandType.Ready:
                            dic["Rotbot就绪"] = true;

                            //MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ1B));
                            break;
                        case Params.Robot2PCCommandType.ZQ1F:
                            dic["样件架样件夹持反馈"] = true;
                            if (dic["样件架样件夹持反馈"])
                                {
                                MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Finish), dic["暂停模式选择"]);
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperClamp), dic["暂停模式选择"]);
                                dic["U型架上升命令触发"] = false;
                                dic["U型架下降命令触发"] = false;
                                dic["U型架升降到位"] = false;
                                }
                            break;
                        case Params.Robot2PCCommandType.FeedBack1:
                            Thread.Sleep(20000);
                            dic["U型架升降到位"] = true;
                            MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperClamp), dic["暂停模式选择"]);
                            break;
                        case Params.Robot2PCCommandType.DW1:
                            dic["样件上半段夹取位到达"] = true;
                            break;
                        case Params.Robot2PCCommandType.DW2:
                            dic["样件下半段夹取位到达"] = true;
                            break;
                        case Params.Robot2PCCommandType.ZQ2F:
                            dic["样件上半段下料至夹取位反馈"] = true;
                            break;
                        case Params.Robot2PCCommandType.ZQ3F:
                            dic["样件下半段下料至夹取位反馈"] = true;
                            break;
                        case Params.Robot2PCCommandType.FeedBack3:
                            dic["样件下半段下料完成反馈"] = true;
                            break;
                        case Params.Robot2PCCommandType.Finish:
                            dic["全部下料完成"] = true;
                            if (dic["全部下料完成"])
                                {
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Break), dic["暂停模式选择"]);
                                }
                            if (fracturePositionResult != "中间")
                                {
                                if (fracturePositionResult == "断于下钳口")
                                    {
                                    MessageBox.Show("样件断在下钳口，请手动操作机器人将下半段下料！");
                                    }
                                if (fracturePositionResult == "断于上钳口")
                                    {
                                    MessageBox.Show("样件断在上钳口，请手动操作机器人将上半段下料！");
                                    }
                                }
                            break;
                        case Params.Robot2PCCommandType.Error:
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + e.Datagram, " [Receive Unvalid Data!Please Check the Robot's Status]", dic["测试命令模式"]);
                            break;
                        }
                    }
                }
            catch (Exception ex)
                {
                LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + " has some errors:", ex.Message, dic["测试命令模式"]);
                }
            }
        #endregion

        #region 写日志文档
        private void LogRefresh(string content, string datagram, bool flag)
            {
            if (flag)
                {
                try
                    {
                    if (this.IsHandleCreated)
                        {
                        log.Info(content + datagram);
                        this.BeginInvoke(new UpdateDisplayDelegate(UpdateDisplay), new object[] { content + datagram });
                        }
                    }
                catch (Exception ex)
                    {
                    log.Exception(content + datagram + " Errorinfo:", ex);
                    this.BeginInvoke(new UpdateDisplayDelegate(UpdateDisplay), new object[] { content + datagram + " Errorinfo:" + ex.Message });
                    }
                }
            }
        #endregion

        #region 发送电文信息

        private void MsgSend(TcpClient _client, string _msg, bool _pauseFlag)
            {
            if (!_pauseFlag)
                {
                try
                    {
                    IPEndPoint ClientIP = (IPEndPoint)_client.Client.RemoteEndPoint;
                    server.Send(_client, _msg);
                    LogRefresh("服务器端 has sent messages to " + ipC.iPConfig[ClientIP.Address.ToString()] + ":", _msg, dic["测试命令模式"]);
                    }
                catch (Exception ex)
                    {
                    log.Exception(ex);
                    return;
                    }
                }
            else
                {
                return;
                }
            }

        private void MsgSend(TcpClient _client, byte[] _bt, bool _pauseFlag)
            {
            if (!_pauseFlag)
                {
                try
                    {
                    IPEndPoint ClientIP = (IPEndPoint)_client.Client.RemoteEndPoint;
                    ushort register = BitConverter.ToUInt16(_bt, 0);
                    byte[] bt = BitConverter.GetBytes(register);
                    server.Send(_client, _bt);
                    LogRefresh("服务器端 has sent messages to " + ipC.iPConfig[ClientIP.Address.ToString()] + ":", CommonFunction.byteToHexString(_bt), dic["测试命令模式"]);
                    }
                catch (Exception ex)
                    {
                    log.Exception(ex);
                    return;
                    }
                }
            else
                {
                return;
                }
            }
        #endregion

        #region Form Events
        private void Form1_Load(object sender, EventArgs e)
            {

            mh = new MouseHook();
            mh.SetHook();
            }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
            {
            mh.UnHook();
            }
        #endregion

        #region 读取实验结果
        private void tsbRead_Click(object sender, EventArgs e)
            {
            using (BackgroundWorker bw = new BackgroundWorker())
                {
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted_1);
                bw.DoWork += new DoWorkEventHandler(bw_DoWork_1);
                bw.RunWorkerAsync();
                }
            }
        DataTable dt = new DataTable();
        void bw_DoWork_1(object sender, DoWorkEventArgs e)
            {
            try
                {
                bool flag = false;
                dt = ReadAllData("拉伸", @"C:\Program Files (x86)\万能试验软件-CTS600-中文版\database\testdata.mdb", ref flag);
                }
            catch (Exception ex) { throw ex; }
            Thread.Sleep(1000);
            }

        void bw_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
            {
            try
                {
                List<ResultsStructure> ResultList = new List<ResultsStructure>();

                ResultsStructure rs = new ResultsStructure();
                rs.BatchNo = dt.Rows[0][0].ToString();
                rs.BatchSeq = Int16.Parse(dt.Rows[0][1].ToString());
                rs.FmValue = float.Parse(dt.Rows[0][9].ToString());
                rs.FeHValue = float.Parse(dt.Rows[0][10].ToString() == "" ? "0" : dt.Rows[0][10].ToString());
                rs.FeLValue = float.Parse(dt.Rows[0][11].ToString() == "" ? "0" : dt.Rows[0][10].ToString());


                rs.RmValue = float.Parse(dt.Rows[0][29].ToString());
                rs.A = double.Parse(dt.Rows[0][39].ToString());
                rs.FracturePosition = dt.Rows[0][53].ToString();
                ResultList.Add(rs);

                this.bindingSource1.DataSource = ResultList;
                dic["试验数据保存完成"] = true;
                }
            catch (Exception ex) { throw ex; }
            }
        public class FileTimeInfo
            {
            public string FileName;
            public DateTime FileCreateTime;
            }

        public static List<FileTimeInfo> GetLatestFileTimeInfo(string dir, string ext)
            {
            List<FileTimeInfo> list = new List<FileTimeInfo>();
            DirectoryInfo d = new DirectoryInfo(dir);
            foreach (FileInfo fi in d.GetFiles())
                {
                if (fi.Extension.ToUpper() == ext.ToUpper())
                    {
                    list.Add(new FileTimeInfo()
                        {
                        FileName = fi.FullName,
                        FileCreateTime = fi.CreationTime
                        });
                    }
                }
            return list.OrderByDescending(x => x.FileCreateTime).ToList();
            }
        #endregion

        #region 添加检验计划
        private void tsbAdd_Click(object sender, EventArgs e)
            {
            try
                {
                if (this.seBatchNum.Value > 0 && this.seLength.Value > 0 && this.seDiameter.Value > 0)
                    {
                    string BatchNum = Int16.Parse(seBatchNum.Value.ToString()).ToString("x8");
                    string diameter = Int16.Parse(seDiameter.Value.ToString()).ToString("x8");
                    string length = Int16.Parse(seLength.Value.ToString()).ToString("x8");
                    byte[] sampleProperty = new byte[] { 0xEE, CommonFunction.strToHexByte(BatchNum)[3], CommonFunction.strToHexByte(diameter)[3], CommonFunction.strToHexByte(length)[3], 0xFF };
                    pd.SampleProperty = sampleProperty;
                    MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.SamplePlan),dic["暂停模式选择"]);
                    Thread.Sleep(1000);
                    MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.SetSample), dic["暂停模式选择"]);
                    }
                }
            catch (Exception ex)
                {
                log.Exception(ex);
                return;
                }
            }
        #endregion

        #region 开始实验
        private void tsbStart_Click(object sender, EventArgs e)
            {
            dic["系统开始运行"] = true;
            dic["批次试验完毕"] = false;
            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Complete), dic["暂停模式选择"]);
            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.Active));
            }
        #endregion

        #region 控制拉力机升降
        private void MouseMovementControl(Params.MouseMovementType _moveType, bool _flag)
            {
            if (!_flag)
                {
                try
                    {
                    switch (_moveType)
                        {
                        case Params.MouseMovementType.ToDown:
                            MouseMovement.SetCursorPos(2900, 524);
                            break;
                        case Params.MouseMovementType.ToUp:
                            MouseMovement.SetCursorPos(2902, 284);
                            break;
                        case Params.MouseMovementType.ToStop:
                            MouseMovement.SetCursorPos(2900, 406);
                            break;
                        case Params.MouseMovementType.ToStart:
                            MouseMovement.SetCursorPos(2606, 80);
                            break;
                        case Params.MouseMovementType.ToSave:
                            MouseMovement.SetCursorPos(2853, 79);
                            break;
                        }
                    MouseMovement.mouse_event(MouseMovement.MouseEventFlag.LeftDown, 0, 0, 0, UIntPtr.Zero);
                    MouseMovement.mouse_event(MouseMovement.MouseEventFlag.LeftUp, 0, 0, 0, UIntPtr.Zero);
                    }
                catch (Exception ex) { log.Exception(ex); }
                }
            else
                {
                return;
                }
            }
        #endregion


        #region Read Access MDB
        public static DataTable ReadAllData(string tableName, string mdbPath, ref bool success)
            {
            DataTable dt = new DataTable();
            try
                {
                DataRow dr;

                //1、建立连接 C#操作Access之读取mdb  

                string strConn = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                + mdbPath + ";";

                OleDbConnection odcConnection = new OleDbConnection(strConn);

                //2、打开连接 C#操作Access之读取mdb  
                odcConnection.Open();

                //建立SQL查询   
                OleDbCommand odCommand = odcConnection.CreateCommand();

                //3、输入查询语句 C#操作Access之读取mdb  

                odCommand.CommandText = "select * from " + tableName + " order  by 开始时间 desc";

                //建立读取   
                OleDbDataReader odrReader = odCommand.ExecuteReader();

                //查询并显示数据   
                int size = odrReader.FieldCount;
                for (int i = 0; i < size; i++)
                    {
                    DataColumn dc;
                    dc = new DataColumn(odrReader.GetName(i));
                    dt.Columns.Add(dc);

                    }
                while (odrReader.Read())
                    {
                    dr = dt.NewRow();
                    for (int i = 0; i < size; i++)
                        {

                        dr[odrReader.GetName(i)] =
                        odrReader[odrReader.GetName(i)].ToString();

                        }
                    dt.Rows.Add(dr);
                    }
                //关闭连接 C#操作Access之读取mdb  
                odrReader.Close();
                odcConnection.Close();
                success = true;
                return dt;
                }
            catch
                {
                success = false;
                return dt;
                }
            }
        #endregion

        private void toolStripButton1_Click(object sender, EventArgs e)
            {
            MouseMovementControl(Params.MouseMovementType.ToSave, dic["暂停模式选择"]);
            }

        private void btnManual_Click(object sender, EventArgs e)
            {
            btnManual.Enabled = false;
            if (frmMM.IsDisposed)
                {
                FrmManualMode frmMM = new FrmManualMode();
                frmMM.ManualOperationOrders += FrmMM_ManualOperationOrders;
                }
            frmMM.Show();
            }


        private void btnPause_Click(object sender, EventArgs e)
            {
            if (dic["有急停信号"])
                {
                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new Object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Emergency) });
                }
            else
                {               
                MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.PS), false);
                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new Object[] { PlatformStatusClassifier.StatusClassifier(Params.ProcessName.Pause) });
                }
            btnManual.Enabled = true;
            dic["暂停模式选择"] = true;          
            if (dic["开始试验"] && !dic["U型架移动到位并停止"])
                {
                MouseMovementControl(Params.MouseMovementType.ToStop, dic["暂停模式选择"]);
                }         
            }

        private void tsbSave_Click(object sender, EventArgs e)
            {
            MouseMovementControl(Params.MouseMovementType.ToSave, dic["暂停模式选择"]);
            }

        //正常模式下只显示流程号和状态信息，点击后添加电文信息，再次点击取消电文信息
        private void tsbTest_Click(object sender, EventArgs e)
            {
            if (!dic["测试命令模式"])
                {
                dic["测试命令模式"] = true;
                }
            else
                {
                dic["测试命令模式"] = false;
                }
            }

        private void btnPauseReset_Click(object sender, EventArgs e)
            {
            if (dic["有急停信号"])
                {
                dic["有急停信号"] = false;
                MessageBox.Show("请确保各设备的急停状态已经复位！如果没有，请先复位设备，然后再次点击本按钮！！！");
                }
            btnManual.Enabled = false;
            dic["暂停模式选择"] = false;
            dic["样件下半段下料完成反馈"] = false;
            dic["Rotbot就绪"] = false;
            dic["样件架样件夹持反馈"] = false;
            dic["样件上半段下料至夹取位反馈"] = false;
            dic["样件下半段下料至夹取位反馈"] = false;
            dic["从样件架接料并夹持"] = false;
            dic["样件上半段下料至夹取位"] = false;
            dic["样件下半段下料至夹取位"] = false;
            dic["样件下半段下料至夹取位"] = false;
            dic["样件下半段下料完成反馈"] = false;
            dic["试验数据保存完成"] = false;
            dic["试验完毕"] = false;
            dic["U型架移动到位并停止"] = false;
            dic["拉断后U型架抬升完成"] = false;
            dic["开始试验"] = false;
            dic["拉力机准备就绪"] = false;
            dic["样件上半段夹取位到达"] = false;
            dic["样件下半段夹取位到达"] = false;
            dic["U型架当前高度获得"] = false;
            dic["U型架正在下降"] = false;
            dic["U型架下降到位"] = false;
            positionFlag = 0;
            rd.AxisValue = "0.0";
            i = 0;
            if (ss.HeightValue > 805 || ss.HeightValue < 795 || ss.isLowCaliperClamp || ss.isTopCaliperClamp)
                {
                MessageBox.Show("U型架未能升降到位或者夹钳未能打开，请手动调整！调整完毕后再次点击暂停复位按钮！！！");             
                }
            else
                {
                MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.CatchSampleRequest), false);
                checkedListBoxControl2.Items[14].CheckState = CheckState.Unchecked;
                checkedListBoxControl2.Items[15].CheckState = CheckState.Unchecked;
                }
            }
        }
}
