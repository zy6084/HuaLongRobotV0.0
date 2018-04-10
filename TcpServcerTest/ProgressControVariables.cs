using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

namespace TcpServcerTest
    {
    class ProgressControVariables
        {
        public ConcurrentDictionary<string, bool> SetKeyValue()
            {
            ConcurrentDictionary<string, bool> dic = new ConcurrentDictionary<string, bool>();
            //Rotbot状态
            dic.TryAdd("Rotbot就绪", false);
            dic.TryAdd("从样件架接料并夹持", false);
            dic.TryAdd("样件架样件夹持反馈", false);
            //dic.TryAdd("放入拉力机并返回", false);
            dic.TryAdd("放入拉力机并返回反馈", false);
            dic.TryAdd("样件下半段下料至夹取位", false);
            dic.TryAdd("样件下半段下料至夹取位反馈", false);
            dic.TryAdd("样件下半段夹取位到达", false);
            dic.TryAdd("样件下半段下料", false);
            dic.TryAdd("样件上半段下料至夹取位", false);
            dic.TryAdd("样件上半段下料至夹取位反馈", false);
            dic.TryAdd("样件上半段夹取位到达", false);
            dic.TryAdd("样件上半段下料", false);
            //样件架PLC状态
            dic.TryAdd("上夹钳闭合", false);
            dic.TryAdd("下夹钳闭合", false);
            dic.TryAdd("试验完毕", false);
            //拉力机PLC状态
            dic.TryAdd("U型架上升命令触发", false);
            dic.TryAdd("U型架下降命令触发", false);
            dic.TryAdd("U型架停止命令触发", false);
            dic.TryAdd("拉力机准备就绪", false);
            dic.TryAdd("U型架升降到位", false);
            dic.TryAdd("开始试验", false);
            dic.TryAdd("模拟试验", false);
            dic.TryAdd("试验完毕", false);
            dic.TryAdd("U型架正在移动", false);
            dic.TryAdd("U型架移动到位并停止", false);
            dic.TryAdd("拉断后U型架抬升完成", false);
            dic.TryAdd("下夹钳打开", false);
            dic.TryAdd("上夹钳打开", false);
            dic.TryAdd("系统开始运行", false);
            dic.TryAdd("U型架当前高度获得", false);
            dic.TryAdd("U型架正在下降", false);
            dic.TryAdd("U型架下降到位", false);
            dic.TryAdd("试验数据保存完成", false);
            //复位命令状态
            dic.TryAdd("满足急停复位", false);
            dic.TryAdd("有急停信号", false);
            dic.TryAdd("暂停命令", false);
            dic.TryAdd("满足暂停复位", false);
            return dic;
            }
        }
    }
