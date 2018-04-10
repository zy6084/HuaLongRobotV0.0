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
using System.Timers;
using System.Data;
using System.Data.OleDb;

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
        public delegate void CheckStateControlDelegate(int[] _seq);//状态控制委托
        public void UpdateDisplay(string msg)
        {
            txtLog.Text += DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss   ") + msg + "\r\n";
            //txtLog.Focus();//获取焦点
            txtLog.Select(txtLog.TextLength, 0);//光标定位到文本最后
            txtLog.ScrollToCaret();//滚动到光标处}
        }
        public void HeightDisplay(string value) { this.txtUPlatformHeightValue.Text = value; }
        public void CheckStateControl(int[] _seq)
        {
            for (int i = 0; i < checkedListBoxControl1.Items.Count; i++)
                this.checkedListBoxControl1.Items[i].CheckState = CheckState.Unchecked;
            foreach (var item in _seq)
                this.checkedListBoxControl1.Items[item].CheckState = CheckState.Checked;
        }
        //数值变量
        string SampleLength = string.Empty; //样本长度
        string SampleWidth = string.Empty; //样本宽度
        string SampleHeight = string.Empty; //样本厚度
        string SampleDiameter = string.Empty;

        //Robot状态标记
        bool RobotReady = false;
        bool RobotFeedBack3flag = false;
        bool RobotFeedBack1flag = false;
        bool RobotFinishflag = false;
        bool RobotZq1b = false;
        bool RobotZq1f = false;
        bool RobotZq2b = false;
        bool RobotZq2f = false;
        bool RobotZq3b = false;
        bool RobotZq3f = false;
        //bool RobotReturn1 = false;
        bool RobotDW1 = false;
        bool RobotDW2 = false;
        //PLC状态标记
        bool PLCTopCaliperClamp = false;
        bool PLCLowCaliperClamp = false;
        bool PLCDoStatus = false;
        bool PLCCompleteStatus = false;
        bool PLCActive = false;
        bool PLCSampleComplete = false;
        //Coder状态标记
        bool PlatformUp = false;
        bool PlatformDown = false;
        bool PlatformStop = false;
        //bool PlatformAllClamp = false;
        bool PlatformIniDone = false;
        bool PlatformHeightDone = false;
        bool Testing = false;//开始实验
        bool isTestLens = false; //自动移动，代替测试中实验功能
        bool TestingFinish = false;//实验完毕
        bool isMoving = false;//上台开始移动
        bool isStop = false;//上开静止
        bool MovingDone = false;//拉断后向上移动5秒完成
        bool isLowCaliperClamp = false;
        bool isTopCaliperClamp = false;
        bool isSysBegin = false;
        bool isHeightGet = false;
        bool isHeightDown = false;
        bool DownMovingDone = false;
        float V_Height = 0;
        int i = 0;
        string fracturePositionResult = string.Empty;
        bool saveComplete = false;
        int positionFlag = 0;
        public List<string> strList = new List<string>();
        List<FileTimeInfo> ftl = null;

        MouseHook mh;

        System.Timers.Timer timer = new System.Timers.Timer(4000);
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
                // 启动服务程序
                server.Start();
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer2_Elapsed);

                ButtonList.Add(zq1b_Click);
                ButtonList.Add(return1_Click);
                ButtonList.Add(zb20_Click);
                ButtonList.Add(zq2b_Click);
                ButtonList.Add(return2_Click);
                ButtonList.Add(zq3b_Click);
                ButtonList.Add(return3_Click);
            }
            catch (Exception ex) { log.Exception(ex); }
        }

        #region 客户端连接事件
        void server_ClientConnected(object sender, TcpClientConnectedEventArgs e)
        {
            try
            {
                IPEndPoint ClientIP = e.TcpClient.Client.RemoteEndPoint as IPEndPoint;
                if (ClientIP.Address.ToString() == Properties.Settings.Default.样件架PLC端) clientPLC = e.TcpClient;
                if (ClientIP.Address.ToString() == Properties.Settings.Default.拉力机PLC端) clientCoder = e.TcpClient;
                if (ClientIP.Address.ToString() == Properties.Settings.Default.Robot端) clientRobot = e.TcpClient;
                LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has connected.", null);
            }
            catch (Exception ex) { log.Exception(ex); }
        }
        #endregion

        #region 客户端断开事件
        void server_ClientDisconnected(object sender, TcpClientDisconnectedEventArgs e)
        {
            IPEndPoint ClientIP = (IPEndPoint)e.TcpClient.Client.RemoteEndPoint;
            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has disconnected.", null);
        }
        #endregion

        #region byte[]类型电文接受事件（PLC端通讯）
        void server_DatagramReceived(object sender, TcpDatagramReceivedEventArgs<byte[]> e)
        {
            IPEndPoint ClientIP = (IPEndPoint)e.TcpClient.Client.RemoteEndPoint;
            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + "  has sent messages:", CommonFunction.byteToHexString(e.Datagram));
            try
            {
                #region 样架PLC端
                if (e.TcpClient == clientPLC)
                {
                    //LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + "  has sent messages:", CommonFunction.byteToHexString(e.Datagram));
                    switch (pd.receiveCommand(e.Datagram))
                    {
                        case Params.PLC2PCCommandType.Ready:
                            PLCTopCaliperClamp = false;
                            PLCLowCaliperClamp = false;
                            PLCDoStatus = false;
                            PLCCompleteStatus = false;
                            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Active));
                            tsbAdd_Click(null, null);
                            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.Active));//////////////////////////////////////////////////////////////////////////////////////////////
                            break;
                        case Params.PLC2PCCommandType.Operation:
                            break;
                        case Params.PLC2PCCommandType.DataSend:
                            SampleLength = CommonFunction.byteToHexString(e.Datagram).Substring(2, 4);
                            SampleWidth = CommonFunction.byteToHexString(e.Datagram).Substring(6, 4);
                            SampleHeight = CommonFunction.byteToHexString(e.Datagram).Substring(10, 4);

                            PLCTopCaliperClamp = false;
                            PLCLowCaliperClamp = false;
                            PLCDoStatus = false;
                            PLCCompleteStatus = false;
                            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.DataFeedBack));
                            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.GrabHeight));//////////////////////////////////////////////////////////////////////////////////////////////
                            if (!PlatformStop)
                                timer.Start();
                            break;
                        case Params.PLC2PCCommandType.CatchSample:
                            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.WaitKey));
                            if (RobotReady && !RobotZq1b && PlatformHeightDone && !PLCSampleComplete)
                            {
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ1B));
                                RobotZq1b = true;
                                //MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Finish));
                            }
                            break;
                        case Params.PLC2PCCommandType.Complete:
                            PLCSampleComplete = true; //该批次实验完毕
                            //MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Complete));

                            //isSysBegin = false;
                            break;
                        case Params.PLC2PCCommandType.UrgencyStop:
                            break;
                        case Params.PLC2PCCommandType.ServoMotorError:
                            break;
                        case Params.PLC2PCCommandType.MeasuerRrror:
                            break;
                        case Params.PLC2PCCommandType.Error:
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + CommonFunction.byteToHexString(e.Datagram), " [Receive Unvalid Data!Please Check the PLC's Status]");
                            break;
                    }
                }
                #endregion

                #region 拉力机PLC端
                else if (e.TcpClient == clientCoder)
                {
                    if (isSysBegin) //系统开始运行
                    {
                        
                        ss = ma.StatusAnalysis(e);
                        this.BeginInvoke(new UpdateDisplayDelegate(HeightDisplay), new object[] { ss.HeightValue.ToString() });
                        if (ss.HeightValue >= 1200)
                        {
                            MouseMovementControl(Params.MouseMovementType.ToStop);
                            MessageBox.Show("拉力机移动到达上限位，请确认系统状态！");
                        }
                        if (Testing && TestingFinish && MovingDone)
                        {
                            if (positionFlag == 1)
                                {
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.M1));
                                Thread.Sleep(1000);
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B));
                                }
                            else if (positionFlag==2)
                                {
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.M2));
                                Thread.Sleep(1000);
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B));
                                }
                            //MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B));
                        }
                        if (RobotFeedBack1flag && !RobotZq2b && Testing && TestingFinish && MovingDone) //获取上升高度，命令机器人抓取
                        {
                            if ((positionFlag==1&& RobotFeedBack3flag)||positionFlag==3)
                                {
                                if (!isHeightDown)
                                    {
                                    if (!ss.isPlatformMoving) MouseMovementControl(Params.MouseMovementType.ToDown);
                                    else isHeightDown = true;
                                    }
                                for (; ss.HeightValue < 800;)
                                    {
                                    MouseMovementControl(Params.MouseMovementType.ToStop);
                                    //tsbRead_Click(null, null);
                                    DownMovingDone = true;
                                    break;
                                    }
                                }
                           
                            if (DownMovingDone)
                            {
                                if (positionFlag!=0)
                                    {
                                    if (positionFlag == 1 || positionFlag == 3)
                                        {
                                        rd.AxisValue = (ss.HeightValue - float.Parse(seZBHeight.Value.ToString())).ToString();
                                        MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZBAXIS));
                                        Thread.Sleep(1000);
                                        MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ2B));
                                        }
                                    RobotZq2b = true;
                                    }
                                else
                                    {
                                    LogRefresh("未能正确获得断裂位置！", "");
                                    MessageBox.Show("未能正确获得断裂位置！");
                                    }
                            }
                        }
                        else if (!RobotFeedBack1flag && !PlatformIniDone) //初始化
                        {
                            #region 初始化逻辑
                            ////调整夹钳
                            //if (ss.isLowCaliperClamp)
                            //{
                            //    //byte[] sendbt = new byte[] { 0xFF, 0x02, 0x7F };
                            //    //MsgSend(clientCoder, sendbt);
                            //    MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperOpen));
                            //}
                            //if (!ss.isLowCaliperClamp) isLowCaliperClamp = true;
                            //if (ss.isTopCaliperClamp && isLowCaliperClamp)
                            //{
                            //    //byte[] sendbt = new byte[] { 0xFF, 0x01, 0x7F };
                            //    //MsgSend(clientCoder, sendbt);
                            //    MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperOpen));
                            //}
                            //if (!ss.isTopCaliperClamp) isTopCaliperClamp = true;
                            ////调整上台高度
                            //if ((ss.HeightValue > 805) || (ss.HeightValue < 795))
                            //{
                            //    if (isLowCaliperClamp && isTopCaliperClamp)
                            //    {
                            //        if (ss.HeightValue > 805)//下降
                            //        {
                            //            if (!PlatformUp)
                            //            {
                            //                if (!ss.isPlatformMoving) MouseMovementControl(Params.MouseMovementType.ToDown);
                            //                else PlatformUp = true;
                            //            }
                            //        }
                            //        if (ss.HeightValue < 795)//上升
                            //        {
                            //            if (!PlatformDown)
                            //            {
                            //                if (!ss.isPlatformMoving) MouseMovementControl(Params.MouseMovementType.ToUp);
                            //                else PlatformDown = true;
                            //            }
                            //        }
                            //        //if ((795 < ss.HeightValue) && (ss.HeightValue < 805))//停止
                            //        //{
                            //        //    MouseMovementControl(Params.MouseMovementType.ToStop);
                            //        //    //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.GrabHeightDone));
                            //        //    PlatformHeightDone = true;
                            //        //    PlatformIniDone = true;
                            //        //    MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.CatchSampleRequest));
                            //        //    tsbAdd_Click(null, null);
                            //        //}
                            //    }
                            //}
                            //else
                            //{
                            //    MouseMovementControl(Params.MouseMovementType.ToStop);
                            //    PlatformHeightDone = true;
                            //    PlatformIniDone = true;
                            //    MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.CatchSampleRequest));
                            //    tsbAdd_Click(null, null);
                            //}
                            #endregion

                              #region 初始化逻辑修改：防止上下夹钳先后打开造成死机的情况发生，修改为先打开下夹钳，然后下降U型台，最后打开上夹钳的过程
                            //调整下夹钳
                            if (ss.isLowCaliperClamp)//下夹钳关闭
                            {
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperOpen));
                            }
                            if (!ss.isLowCaliperClamp) isLowCaliperClamp = true;//下夹钳打开

                            //调整上台高度
                            if ((ss.HeightValue > 805) || (ss.HeightValue < 795))
                            {
                                if (isLowCaliperClamp)//下夹钳已打开完成
                                {
                                    if (ss.HeightValue > 805)//下降
                                    {
                                        if (!PlatformUp)
                                        {
                                            if (!ss.isPlatformMoving) MouseMovementControl(Params.MouseMovementType.ToDown);
                                            else PlatformUp = true;
                                        }
                                    }
                                    if (ss.HeightValue < 795)//上升
                                    {
                                        if (!PlatformDown)
                                        {
                                            if (!ss.isPlatformMoving) MouseMovementControl(Params.MouseMovementType.ToUp);
                                            else PlatformDown = true;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                MouseMovementControl(Params.MouseMovementType.ToStop);
                                PlatformHeightDone = true;
                            }

                            //调整上夹钳
                            if (ss.isTopCaliperClamp && isLowCaliperClamp && PlatformHeightDone)
                            {
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperOpen));
                            }
                            if (!ss.isTopCaliperClamp) isTopCaliperClamp = true;

                            //全部调整完毕以后，执行后续过程
                            if (isTopCaliperClamp && isLowCaliperClamp && PlatformHeightDone)
                            {
                                MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.CatchSampleRequest));
                                tsbAdd_Click(null, null);
                                PlatformIniDone = true;
                            }
                            #endregion
                        }
                        else if (RobotZq1f && !MovingDone) //机器人抓取样件完成，送入拉力机位置
                        {
                            if (ss.isTopCaliperClamp)
                            { Thread.Sleep(3000); MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return1)); }//上夹钳闭合，机器人退回
                            else MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperClamp));
                            if (RobotFeedBack1flag)//机器人放入完成
                            {
                                if (!ss.isLowCaliperClamp) MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperClamp));
                                if (ss.isLowCaliperClamp && ss.isTopCaliperClamp && !Testing) //开始实验
                                {
                                    if (Properties.Settings.Default.类别 == "1")
                                    {
                                        //Testing = true; //lbErrorInfo.Text = "点击开始实验！"; 
                                        //实验版本
                                        MouseMovementControl(Params.MouseMovementType.ToStart);
                                        if (ss.isPlatformMoving) Testing = true;
                                    }
                                    else
                                    {
                                        //自动拉伸版本
                                        if (!isTestLens)
                                            MouseMovementControl(Params.MouseMovementType.ToUp);
                                        if (ss.isPlatformMoving)
                                        {
                                            isTestLens = true;
                                            if (ss.HeightValue > 830)
                                            {
                                                MouseMovementControl(Params.MouseMovementType.ToStop);
                                                Testing = true;

                                            }
                                        }
                                    }
                                }
                                if (Testing)
                                {
                                    if (ss.isPlatformMoving)
                                        isMoving = true;
                                    else
                                    {
                                        i++;
                                        if (i > 5)
                                            isStop = true;
                                    }
                                    MessageBox.Show("请选择机器人动作方式", "模式选择",  MessageBoxButtons.YesNoCancel);
                                    switch (DialogResult)
                                        {
                                        case DialogResult.Yes:
                                            positionFlag = 1;
                                            break;
                                        case DialogResult.No:
                                            positionFlag = 2;
                                            break;
                                        case DialogResult.Cancel:
                                            positionFlag = 3;
                                            break;
                                        }
                                    }
                            }
                            if (RobotFeedBack1flag && isMoving && isStop)//机器人放入样件完成，已经移动且停止（实验完毕）
                            {
                                if (!isHeightGet)
                                {
                                    V_Height = ss.HeightValue;
                                    isHeightGet = true;
                                    //MouseMovementControl(Params.MouseMovementType.ToUp);
                                }
                                this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.CompleteStatus) });
                                PLCCompleteStatus = true;
                                TestingFinish = true;
                                if (Properties.Settings.Default.类别=="1")
                                    {
                                    MouseMovementControl(Params.MouseMovementType.ToSave);
                                    Thread.Sleep(2000);
                                    tsbRead_Click(null, null);
                                    if (saveComplete)
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
                                MovingDone = true;
                                //if (Testing && TestingFinish && PLCCompleteStatus && isStop)
                                //{
                                //继续向上移动一段距离，方便取出断裂的样件，避免碰撞
                                //MouseMovementControl(Params.MouseMovementType.ToUp);
                                //Thread.Sleep(10000);

                                //Real-Time height>fracturedHeight+videoExtenderPreset
                                //for (; ss.HeightValue > V_Height +Properties.Settings.Default.fracturedUpDisplacement; )
                                //{
                                //    MouseMovementControl(Params.MouseMovementType.ToStop);
                                //    Thread.Sleep(1000);
                                //    MouseMovementControl(Params.MouseMovementType.ToSave);
                                //    Thread.Sleep(2000);
                                //    tsbRead_Click(null, null);
                                //    MovingDone = true;
                                //    break;
                                //}
                                //}
                                }
                        }

                        if (RobotZq2f && !ss.isTopCaliperClamp) //机器人取出上断裂样件后返回
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return2));
                        if (RobotZq3f && !ss.isLowCaliperClamp) //机器人取出下断裂样件后返回
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return3));
                        if (RobotDW1)
                        {
                            //if (ss.isTopCaliperClamp)
                            //{
                            //    MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperOpen));
                            //    this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.TopCaliperOpen) });
                            //}
                            //else
                            //{
                            //    //Thread.Sleep(TimeSpan.FromMilliseconds(double.Parse(textEdit1.Text)));
                            //    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ1));
                            //}
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ1));
                            if (ss.isTopCaliperClamp)
                            {
                                Thread.Sleep(10000);
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperOpen));
                                BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.TopCaliperOpen) });
                            }
                        }
                        if (RobotDW2)
                        {
                            //if (ss.isLowCaliperClamp)
                            //{
                            //    MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperOpen));
                            //    this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.LowCaliperOpen) });
                            //}
                            //else
                            //{
                            //    //Thread.Sleep(TimeSpan.FromMilliseconds(double.Parse(textEdit1.Text)));
                            //    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ2));
                            //}
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ2));
                            if (ss.isLowCaliperClamp)
                            {
                                Thread.Sleep(10000);
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperOpen));
                                this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.LowCaliperOpen) });
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + " has some errors:", ex.Message);
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
                    LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages:", e.Datagram);
                    switch (rd.receiveCommand(e.Datagram))
                    {
                        case Params.Robot2PCCommandType.Connection:
                            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Active));
                            RobotFeedBack3flag = false;
                            RobotFinishflag = false;
                            RobotReady = false;
                            RobotZq1f = false;
                            RobotZq2f = false;
                            RobotZq3f = false;
                            RobotZq1b = false;
                            RobotZq2b = false;
                            RobotZq3b = false;
                            RobotFeedBack1flag = false;
                            RobotFeedBack3flag = false;
                            positionFlag = 0;
                            saveComplete = false;
                            //RobotReturn1 = false;
                            rd.AxisValue = "0.0";
                            TestingFinish = false;//实验完毕
                            isMoving = false;//上台开始移动
                            isStop = false;//上开静止
                            MovingDone = false;//拉断后向上移动5秒完成
                            Testing = false;
                            PlatformIniDone = false;
                            RobotDW1 = false;
                            RobotDW2 = false;
                            isHeightGet = false;
                            isHeightDown = false;
                            isTestLens = false;
                            DownMovingDone = false;
                            i = 0;
                            break;
                        case Params.Robot2PCCommandType.Ready:
                            RobotReady = true;

                            //MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ1B));
                            break;
                        case Params.Robot2PCCommandType.ZQ1F:
                            RobotZq1f = true;
                            if (RobotZq1f)
                            {
                                MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Finish));
                                //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.TopCaliperClamp));//////////////////////////////////////////////////////////////////////////////////////////////
                                //sendbt = new byte[] { 0xFF, 0x04, 0x7F };
                                //MsgSend(clientCoder, sendbt);
                                MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperClamp));
                                //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.GrabHeightDone));//////////////////////////////////////////////////////////////////////////////////////////////

                                this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.TopCaliperClamp) });
                                PlatformUp = false;
                                PlatformDown = false;
                                PlatformHeightDone = false;
                            }
                            break;
                        case Params.Robot2PCCommandType.FeedBack1:
                            Thread.Sleep(20000);
                            RobotFeedBack1flag = true;
                            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.LowCaliperClamp));//////////////////////////////////////////////////////////////////////////////////////////////
                            //sendbt = new byte[] { 0xFF, 0x00, 0x7F };
                            //MsgSend(clientCoder, sendbt);
                            MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperClamp));
                            this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.LowCaliperClamp) });
                            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.GrabHeight));
                            break;
                        case Params.Robot2PCCommandType.DW1:
                            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.TopCaliperOpen));//////////////////////////////////////////////////////////////////////////////////////////////
                            //sendbt = new byte[] { 0xFF, 0x01, 0x7F };
                            //MsgSend(clientCoder, sendbt);
                            RobotDW1 = true;
                            //while (ss.isTopCaliperClamp)
                            //{
                            //    MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.TopCaliperOpen));
                            //    Thread.Sleep(500);
                            //}
                            //this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.TopCaliperOpen) });
                            //Thread.Sleep(TimeSpan.FromMilliseconds(double.Parse(textEdit1.Text)));
                            //MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ1));
                            break;
                        case Params.Robot2PCCommandType.DW2:
                            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.LowCaliperOpen));//////////////////////////////////////////////////////////////////////////////////////////////
                            //sendbt = new byte[] { 0xFF, 0x11, 0x7F };
                            //MsgSend(clientCoder, sendbt);
                            RobotDW2 = true;
                            //while (ss.isLowCaliperClamp)
                            //{
                            //    MsgSend(clientCoder, ma.CommandAnalysis(Params.PC2CoderCommandType.LowCaliperOpen));
                            //    Thread.Sleep(500);
                            //}
                            //this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.LowCaliperOpen) });
                            //Thread.Sleep(TimeSpan.FromMilliseconds(double.Parse(textEdit1.Text)));
                            //MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.JQ2));
                            break;
                        case Params.Robot2PCCommandType.ZQ2F:
                            RobotZq2f = true;
                            //if (RobotZq2f)
                            //{
                            //    MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.TopCaliperOpen));
                            //    this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.TopCaliperOpen) });
                            //}
                            break;
                        //case Params.Robot2PCCommandType.FeedBack3:
                        //    MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B));
                        //    break;
                        case Params.Robot2PCCommandType.ZQ3F:
                            RobotZq3f = true;
                            //if (RobotZq3f)
                            //{
                            //    MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.LowCaliperOpen));
                            //    this.BeginInvoke(new CheckStateControlDelegate(CheckStateControl), new object[] { PlatformStatusClassifier.StatusClassifier(Params.PlatformStatus.LowCaliperOpen) });
                            //}
                            break;
                        case Params.Robot2PCCommandType.FeedBack3:
                            RobotFeedBack3flag = true;
                            break;
                        case Params.Robot2PCCommandType.Finish:
                            RobotFinishflag = true;
                            if (RobotFeedBack3flag && RobotFinishflag)
                            {
                                MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Break));
                            }
                            break;
                        case Params.Robot2PCCommandType.Error:
                            LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + "  has sent messages: " + e.Datagram, " [Receive Unvalid Data!Please Check the Robot's Status]");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogRefresh(ipC.iPConfig[ClientIP.Address.ToString()] + e.TcpClient.Client.RemoteEndPoint.ToString() + " has some errors:", ex.Message);
            }
        }
        #endregion

        #region 写日志文档
        private void LogRefresh(string content, string datagram)
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
        #endregion

        #region 发送电文信息
        private void MsgSend(TcpClient _client, string _msg)
        {
            try
            {
                IPEndPoint ClientIP = (IPEndPoint)_client.Client.RemoteEndPoint;
                server.Send(_client, _msg);
                LogRefresh("服务器端 has sent messages to " + ipC.iPConfig[ClientIP.Address.ToString()] + ":", _msg);
            }
            catch (Exception ex)
            {
                log.Exception(ex);
                return;
            }
        }

        private void MsgSend(TcpClient _client, byte[] _bt)
        {
            try
            {
                IPEndPoint ClientIP = (IPEndPoint)_client.Client.RemoteEndPoint;
                ushort register = BitConverter.ToUInt16(_bt, 0);
                byte[] bt = BitConverter.GetBytes(register);
                server.Send(_client, _bt);
                LogRefresh("服务器端 has sent messages to " + ipC.iPConfig[ClientIP.Address.ToString()] + ":", CommonFunction.byteToHexString(_bt));
            }
            catch (Exception ex)
            {
                log.Exception(ex);
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
        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Thread.CurrentThread.IsBackground = false;
            //if (!PlatformStop)
            //{
            //    MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.GrabHeight));//////////////////////////////////////////////////////////////////////////////////////////////
            //}
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
                //FileStream fs = new FileStream(@"E:\20171208-1138.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                //StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);

                //ftl = GetLatestFileTimeInfo(@"E:\", ".dat");

                //string[] readContentInString = File.ReadAllLines(ftl[0].FileName, Encoding.GetEncoding("GBK"));
                //readContentInString.Aggregate(string.Empty, (result, current) => result += current);
                //foreach (var item in readContentInString) strList.Add(item);
                //sr.Close();
                //fs.Close();

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
                //string batchNo = strList[0].Split('"')[1];
                //int RecordNum = Int16.Parse(strList[5].Split(',')[0]);
                List<ResultsStructure> ResultList = new List<ResultsStructure>();
                //for (int i = 0; i < RecordNum; i++)
                //{
                //    ResultsStructure rs = new ResultsStructure();
                //    rs.BatchNo = batchNo;
                //    rs.BatchSeq = i;
                //    rs.FmValue = Math.Round(double.Parse(strList[22 + 31 * i].Split(',')[0]) / 1000, 1);
                //    rs.FeHValue = Math.Round(double.Parse(strList[23 + 31 * i].Split(',')[0]) / 1000, 1);
                //    rs.FeLValue = Math.Round(double.Parse(strList[24 + 31 * i].Split(',')[0]) / 1000, 1);
                //    rs.LuValue = double.Parse(strList[32 + 31 * i].Split(',')[0]);
                //    rs.Fb02Value = Math.Round(double.Parse(strList[25 + 31 * i].Split(',')[0]) / 1000, 1);
                //    rs.RmValue = Math.Round(double.Parse(strList[22 + 31 * i].Split(',')[0]) / 1000, 1) / Math.Round(Math.PI * (0.022 / 2) * (0.022 / 2), 6) / 1000;
                //    rs.Rp02Value = Math.Round(double.Parse(strList[25 + 31 * i].Split(',')[0]) / 1000, 1) / Math.Round(Math.PI * (0.022 / 2) * (0.022 / 2), 6) / 1000;
                //    ResultList.Add(rs);
                //    //Rm(抗拉强度) = Fm/横截面积
                //    //Rp(规定塑性延伸强度) = 引伸计标距Le百分率时对应的应力 Fp(0.2)/横截面积
                //    //R(应力) = 任意时刻的力/原始横截面积
                //}

                ResultsStructure rs = new ResultsStructure();
                rs.BatchNo = dt.Rows[0][0].ToString();
                rs.BatchSeq = Int16.Parse(dt.Rows[0][1].ToString());
                rs.FmValue = float.Parse(dt.Rows[0][9].ToString());
                rs.FeHValue = float.Parse(dt.Rows[0][10].ToString() == "" ? "0" : dt.Rows[0][10].ToString());
                rs.FeLValue = float.Parse(dt.Rows[0][11].ToString() == "" ? "0" : dt.Rows[0][10].ToString());


                rs.RmValue = float.Parse(dt.Rows[0][29].ToString());
                //rs.Rp02Value = float.Parse(ddt.Rows[0][32].ToString());
                rs.A = double.Parse(dt.Rows[0][39].ToString());
                rs.FracturePosition = dt.Rows[0][53].ToString();
                ResultList.Add(rs);
                //Rm(抗拉强度) = Fm/横截面积
                //Rp(规定塑性延伸强度) = 引伸计标距Le百分率时对应的应力 Fp(0.2)/横截面积
                //R(应力) = 任意时刻的力/原始横截面积

                this.bindingSource1.DataSource = ResultList;
                saveComplete = true;
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
                if (this.seBatchNum.Value > 0 && this.seHeightLowValue.Value > 0 && this.seHeightTopValue.Value > 0)
                {
                    if (this.seHeightTopValue.Value >= this.seHeightLowValue.Value)
                    {
                        string BatchNum = Int16.Parse(seBatchNum.Value.ToString()).ToString("x8");
                        string ThickTop = Int16.Parse(seHeightTopValue.Value.ToString()).ToString("x8");
                        string ThickLow = Int16.Parse(seHeightLowValue.Value.ToString()).ToString("x8");
                        byte[] sampleProperty = new byte[] { 0xEE, CommonFunction.strToHexByte(BatchNum)[3], CommonFunction.strToHexByte(ThickTop)[3], CommonFunction.strToHexByte(ThickLow)[3], 0xFF };
                        pd.SampleProperty = sampleProperty;
                        MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.SamplePlan));
                        Thread.Sleep(1000);
                        MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.SetSample));
                    }
                    else
                    {
                        MessageBox.Show("样件厚度上限不能小于厚度下限！");
                        return;
                    }
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
            isSysBegin = true;
            PLCActive = false;
            PLCSampleComplete = false;
            MsgSend(clientPLC, pd.createCommand(Params.PC2PLCCommandType.Complete));
            //MsgSend(clientCoder, cd.createCommand(Params.PC2CoderCommandType.Active));
        }
        #endregion

        #region 控制拉力机升降
        private void MouseMovementControl(Params.MouseMovementType _moveType)
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
        #endregion

        #region 机器人动作单步测试命令
        System.Timers.Timer timer2 = new System.Timers.Timer(17000);
        int seq;
        private void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (seq > 6) seq = 0;
            else
            {
                ButtonList[seq](null, null);
                seq++;
            }
        }
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            //重复发送
            //seq = 0;
            //timer2.Start();
            //positionFlag = 3;
            //operationManual = true;
            //Testing = true;
            //TestingFinish = true;
            //MovingDone = true;
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            //停止发送
            //timer2.Stop();
            //positionFlag = 2;
            //operationManual = true;
            //Testing = true;
            //TestingFinish = true;
            //MovingDone = true;
            }

        private void zq1b_Click(object sender, EventArgs e)
        {
            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ1B));
        }

        private void return1_Click(object sender, EventArgs e)
        {
            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return1));
        }

        private void zb20_Click(object sender, EventArgs e)
        {
            rd.AxisValue = "10";
            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZBAXIS));
        }

        private void zq2b_Click(object sender, EventArgs e)
        {
            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ2B));
        }

        private void return2_Click(object sender, EventArgs e)
        {
            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return2));
        }

        private void zq3b_Click(object sender, EventArgs e)
        {
            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.ZQ3B));
        }

        private void return3_Click(object sender, EventArgs e)
        {
            MsgSend(clientRobot, rd.createCommand(Params.PC2RobotCommandType.Return3));
        }
        List<System.EventHandler> ButtonList = new List<System.EventHandler>();
        #endregion

        private void sbMoving_Click(object sender, EventArgs e)
        {
            isMoving = true;
        }

        private void sbClearData_Click(object sender, EventArgs e)
        {
            //批次未结束置位
            //this.bindingSource1.Clear();
            //PLCSampleComplete = false;
            //positionFlag = 1;
            //operationManual = true;
            //Testing = true;
            //TestingFinish = true;
            //MovingDone = true;
            }

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
            MouseMovementControl(Params.MouseMovementType.ToSave);
        }

        private void simpleButton8_Click(object sender, EventArgs e)
            {

            }

        private void btnManual_Click(object sender, EventArgs e)
            {

            }

        private void btnAuto_Click(object sender, EventArgs e)
            {

            }
        }
}
