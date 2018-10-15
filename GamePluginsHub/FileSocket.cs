using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace GamePluginsHub
{
    public class FileSocket
    {
        public TcpClient client;//通信的SOCKET
        private string FileSocketState;//通信状态
        private string OldFileSocketState="";//上一个状态
        private FileDescription File;//当前传输的文件的描述对象
        private long MaxSize=-1;//发送的数据流总大小,-1表示当前没有数据流传输
        private int SendSize;//已发送数据流的大小
        private int GetSize;//已接受数据流的文件大小
        private char NowDataType='p';//当前正在接收的数据流的类型,p类型为无效类型 
        private List<FileDescription> FileDes;//需要发送或接受的文件描述对象集合
        private bool SendEr;//是否是发送者,发送文件的那一方该值为true,接收方为false
        private bool Sending=false;//是否处于发送状态，发送状态下不会去读取流内容
        private static int SendSizeBag = 1024;//每次发送的数据包大小
        private Thread th;//接收数据的线程
        private Thread alive;//检测是否仍在链接中的线程
        private string[] RemoteHost=new string[2];//远端IP和端口,0为IP，1为端口
        private MemoryStream tmpStream=null;//临时储存数据区
        private string downloadPath;//存放文件的根目录

        public delegate void onStateChangeHnalder(object serder, string oldState, string newState);
        public delegate void onDataStringHandler(object serder, string msg);
        public delegate void onDataHandler(object serder, byte[] msg);
        public delegate void onDirFileHandler(object serder);
        public delegate void onFileTransferMissionCompleteHandler(object serder, FileDescription file);
        public delegate void onFileTransferMissionAccHandler(object serder, double acc);
        public delegate void onStringTransferMissionAccHandler(object serder, double acc);
        public delegate void onFileTransferMissionGetAccHandler(object serder,char type, double acc);
        public delegate void onFileSocketCloseHandler(object serder);
        /// <summary>
        /// 当收到非文本型数据时触发
        /// </summary>
        public event onDataHandler onData;

        /// <summary>
        /// 当收到文本型数据时触发
        /// </summary>
        public event onDataStringHandler onDataString;

        /// <summary>
        /// 当FILESOCKETSTATE状态改变时触发(触发时状态已改变)
        /// </summary>
        public event onStateChangeHnalder onStateChange;

        /// <summary>
        /// 当目录文件结构解析完成时触发
        /// </summary>
        public event onDirFileHandler onDirFile;

        /// <summary>
        /// 当某个文件传输并整合完成时触发
        /// </summary>
        public event onFileTransferMissionCompleteHandler onFileTransferMissionComplete;

        /// <summary>
        /// 发送文件数据的百分比变动时触发
        /// </summary>
        public event onFileTransferMissionAccHandler onFileTransferMissionAcc;

        /// <summary>
        /// 发送字符数据的百分比变动时触发
        /// </summary>
        public event onStringTransferMissionAccHandler onStringTransferMissionAcc;

        /// <summary>
        /// 接受任意数据的百分比变动时触发
        /// </summary>
        public event onFileTransferMissionGetAccHandler onFileTransferMissionGetAcc;

        /// <summary>
        /// 当FILESOCKET被关闭时触发
        /// </summary>
        public event onFileSocketCloseHandler onFileSocketClose;

        /// <summary>
        /// 作为服务端使用的构造函数
        /// </summary>
        /// <param name="s"></param>
        public FileSocket(TcpClient s)
        {
            client = s;
            string[] ip = client.Client.RemoteEndPoint.ToString().Split(':');
            RemoteHost[0] = ip[0];
            RemoteHost[1] = ip[1];
            FileSocketState = "等待数据";
            SendEr = true;
            th = new Thread(RecvMessage);
            th.Start();
            alive = new Thread(SocketConnectAlive);
            alive.Start();
            downloadPath = Application.StartupPath;
        }


        /// <summary>
        /// 作为客户端使用的构造函数
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param> 
        public FileSocket(string host, int port)
        {
            client = new TcpClient(host, port);
            string[] ip = client.Client.RemoteEndPoint.ToString().Split(':');
            RemoteHost[0] = ip[0];
            RemoteHost[1] = ip[1];
            FileSocketState = "正在连接";
            SendEr = false;
            th = new Thread(RecvMessage);
            th.Start();
            alive = new Thread(SocketConnectAlive);
            alive.Start();
        }


        private void RecvMessage()
        {
            while (FileSocketState != "关闭")
            {
                try
                {


                    NetworkStream netS = client.GetStream();
                    if (netS.DataAvailable == true && Sending == false)
                    {
                       
                        if (MaxSize == -1)
                        {
                            byte[] head = new byte[15];
                            int heads = 0;
                            while (heads < 15)
                            {
                                byte[] f = new byte[1];
                                netS.Read(f, 0, 1);
                                if (f[0] == 0 && head[0] == 0) continue;
                                head[heads] = f[0];
                                heads++;

                            }
                            MemoryStream tmpMs = new MemoryStream(head);
                            BinaryReader tmpMsReader = new BinaryReader(tmpMs);
                            string msg = Encoding.GetEncoding("GBK").GetString(tmpMsReader.ReadBytes(6));

                            if (msg == "USHIOP")
                            {
                                //合法数据包头
                                //进行设置
                                NowDataType = (char)tmpMsReader.ReadByte(); //当前传输类型
                                MaxSize = tmpMsReader.ReadInt64();//读取数据流总大小
                                tmpStream = new MemoryStream();
                            }
                            tmpMsReader.Close();
                            tmpMs.Dispose();
                            tmpMs.Close();

                        }
                        if (MaxSize != -1 && tmpStream != null)
                        {
                            byte[] result = new byte[MaxSize - GetSize < SendSizeBag ? (MaxSize - GetSize) : SendSizeBag];
                            GetSize = GetSize + netS.Read(result, 0, result.Length);
                            tmpStream.Write(result, 0, result.Length);
                            ChangeState("正在接受数据..." + GetSize + "/" + MaxSize);
                            onFileTransferMissionGetAcc?.Invoke(this, NowDataType, 100.0 * (Convert.ToDouble(GetSize) / MaxSize));
                            if (GetSize >= MaxSize)
                            {

                                ChangeState("接受数据完成");
                                MaxSize = -1;
                                GetSize = 0;
                                SendSize = 0;
                                if (NowDataType == 's')
                                {
                                    byte[] tmp = new byte[tmpStream.Length];
                                    tmpStream.Position = 0;
                                    tmpStream.Read(tmp, 0, Convert.ToInt32(tmpStream.Length));
                                    tmpStream.Dispose();
                                    tmpStream.Close();
                                    tmpStream = null;
                                    GetStringMessage(Encoding.GetEncoding("GBK").GetString(tmp));

                                }
                                if (NowDataType == 'd')
                                {
                                    ChangeState("正在整合文件" + File.getName() + "..");
                                    MaxSize = -1;
                                    GetSize = 0;
                                    SendSize = 0;
                                    long readsize = 0;
                                    tmpStream.Position = 0;
                                    FileStream save = new FileStream(File.getFullPath(), FileMode.Create);
                                    while (readsize < tmpStream.Length)
                                    {
                                        long bytesize = tmpStream.Length - readsize > SendSizeBag ? SendSizeBag : tmpStream.Length - readsize;
                                        byte[] tmp = new byte[bytesize];

                                        readsize = readsize + tmpStream.Read(tmp, 0, tmp.Length);
                                        save.Write(tmp, 0, tmp.Length);
                                        ChangeState("正在整合文件" + File.getName() + ".." + readsize.ToString() + "/" + tmpStream.Length);
                                    }
                                    tmpStream.Dispose();
                                    tmpStream.Close();
                                    tmpStream = null;
                                    save.Dispose();
                                    save.Close();
                                    save = null;
                                    onFileTransferMissionComplete?.Invoke(this, File);
                                }
                                NowDataType = 'p';
                            }
                        }
                    }
                }
                catch
                {
                    Stop();
                }
               
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Stop()
        {
            if (FileSocketState == "关闭") return;
             
            ChangeState("关闭");
            if (tmpStream != null)
            { 
                tmpStream.Dispose();
                tmpStream.Close();
            }
            th.Abort();
            alive.Abort();
            client.GetStream().Close();
            client.Close();
            onFileSocketClose?.Invoke(this);
        }

        /// <summary>
        ///  修改FILESOCKET的状态,会触发onChangeState事件
        /// </summary>
        /// <param name="msg"></param>
        public void ChangeState(string msg)
        {
            OldFileSocketState = FileSocketState;
            FileSocketState = msg;
            onStateChange?.Invoke(this,OldFileSocketState,FileSocketState);
        }

        /// <summary>
        /// 返回对方IP和端口号的数组
        /// [0]为IP,[1]为端口
        /// </summary>
        /// <returns></returns>
        public string[] GetRemote()
        {
            return RemoteHost;
        }

        /// <summary>
        /// 返回数据包,自动封装包头
        /// </summary>
        /// <param name="type">数据包的类型</param>
        /// <param name="len">数据包的长度</param>
        /// <returns></returns>
        private byte[] GetDataHead(char type,long len)
        {
            byte[] result = new byte[6 + 1 + 8];
            MemoryStream w = new MemoryStream(result);
            BinaryWriter write = new BinaryWriter(w);
            write.Write(Encoding.GetEncoding("GBK").GetBytes("USHIOP"));
            write.Write(type);
            write.Write(len); 
            write.Close();
            w.Close();
            return result;
        }

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="msg"></param>
        public void SendData(string msg)
        { 
            Sending = true;
            try
            {

                byte[] data = GetDataHead('s', Encoding.GetEncoding("GBK").GetBytes(msg).Length);
                MaxSize = data.Length + Encoding.GetEncoding("GBK").GetBytes(msg).Length;
                SendSize = 0;
                MemoryStream tmp = new MemoryStream();
                tmp.Write(data, 0, data.Length);
                tmp.Write(Encoding.GetEncoding("GBK").GetBytes(msg), 0, Encoding.GetEncoding("GBK").GetBytes(msg).Length);
                tmp.Position = 0;
                while (SendSize < MaxSize)
                {
                    byte[] send = new byte[MaxSize - SendSize < SendSizeBag ? (MaxSize - SendSize) : SendSizeBag];
                    tmp.Read(send, 0, send.Length);
                    client.GetStream().Write(send, 0, send.Length);
                    SendSize = SendSize + send.Length;
                    ChangeState("正在发送数据..." + SendSize + "/" + MaxSize);
                    onStringTransferMissionAcc?.Invoke(this, 100.0 * (Convert.ToDouble(SendSize) / MaxSize));
                }
                ChangeState("字符发送完成");
                tmp.Dispose();
                tmp.Close();
                MaxSize = -1;
                SendSize = 0;
            }
            catch
            {
                Stop();
            }
            Sending = false;
        }

        /// <summary>
        /// 发送文件,会阻塞线程,发完返回TRUE，没发完返回FALSE
        /// </summary> 
        /// <param name="file">需要发送的文件的文件描述对象</param>
        public void SendFile(FileDescription file)
        {
            Sending = true;
            try{


                FileStream f = new FileStream(file.getFullPath(), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                byte[] data = GetDataHead('d', f.Length);
                MaxSize = data.Length + f.Length;
                SendSize = 0;
                MemoryStream tmp = new MemoryStream(data);
                byte[] tmpsend = new byte[15];
                tmp.Read(tmpsend, 0, 15);
                client.GetStream().Write(tmpsend, 0, tmpsend.Length);
                SendSize = SendSize + tmpsend.Length;
                tmp.Dispose();
                tmp.Close();
                while (SendSize < MaxSize)
                {
                    byte[] send = new byte[MaxSize - SendSize < SendSizeBag ? (MaxSize - SendSize) : SendSizeBag];
                    f.Read(send, 0, send.Length);
                    client.GetStream().Write(send, 0, send.Length);
                    SendSize = SendSize + send.Length;
                    ChangeState("正在发送文件.." + File.getName() + "..." + SendSize + "/" + MaxSize);

                    onFileTransferMissionAcc?.Invoke(this, 100.0 * (Convert.ToDouble(SendSize) / MaxSize));
                }
                ChangeState("文件发送完成");
                MaxSize = -1;
                SendSize = 0;
            }
            catch
            {
                Stop();
            }
            Sending = false;
        }

        /// <summary>
        /// 获取该FILESOCKET类是发送者还是接收者
        /// TRUE=发送者,FALSE=接收者
        /// </summary>
        /// <returns></returns>
        public bool GetSendEr()
        {
            return SendEr;
        }

        /// <summary>
        /// 返回FILESOCKET的上一个状态
        /// </summary>
        /// <returns></returns>
        public string GetOldFileSocketState()
        {
            return OldFileSocketState;
        }

        /// <summary>
        /// 返回FILESOCKET的当前状态
        /// </summary>
        /// <returns></returns>
        public string GetFileSocketState()
        {
            return FileSocketState;
        }
        
        /// <summary>
        /// 接收文本型消息的处理函数
        /// </summary>
        private void GetStringMessage(string msg)
        {

            if (msg.IndexOf('@') != -1)
            {
                string[] rmsg = msg.Split('@');
                if(rmsg[0]=="GETDIRLIST")
                {
                    ChangeState("正在获取目录列表");
                    string send = Form1.GetDirList();//只有服务端有
                    if(send=="")
                    {
                        ChangeState("获取目录列表失败");
                        SendData("DIRLIST@FALSE");
                    }
                    else
                    {
                        ChangeState("获取目录列表成功，正在发送");
                        SendData("DIRLIST@" + send);
                    }
                }
                if (rmsg[0] == "GETDIR")
                {

                    ChangeState("正在获取" + rmsg[1] + "的文件结构");
                    string send = Form1.GetDirFilesFromDirName(rmsg[1]);//只有服务端有
                    if (send == "NULL")
                    {
                        ChangeState("获取" + rmsg[1] + "的文件失败");
                        SendData("DIR@" + rmsg[1] + "@FALSE");
                    }
                    else
                    {
                        ChangeState("获取" + rmsg[1] + "的文件成功");
                        SendData("DIR@" + rmsg[1] + "@" + send);
                        FileDes = GetFileDescriptionFromString(send, downloadPath+"\\"+rmsg[1]);
                        //ChangeState("目录[" + rmsg[1] + "]的文件结构已发送,共" + FileDes.Count().ToString() + "个文件");
                    }
                } 
                if(rmsg[0]=="GETFILE")
                {
                    ChangeState("正在获取文件:" + rmsg[1]);
                    File = null;
                    foreach(FileDescription i in FileDes)
                    {
                        if(i.getFullName()==rmsg[1])
                        {
                            File = i;
                            break;
                        }
                    }
                    if(File==null)
                    {
                        ChangeState("获取文件:" + rmsg[1] + "失败,该文件不存在");
                        SendData("FILE@" + rmsg[1] + "@FALSE");
                    }
                    else
                    {
                        ChangeState("获取文件:" + rmsg[1] + "成功,准备发送");
                        SendFile(File);
                    }
                }
                if (rmsg[0] == "DIR")
                {
                    ChangeState("收到关于[" + rmsg[1] + "]的请求回复");
                    if (rmsg[2] == "FALSE")
                    {
                        ChangeState("请求失败");
                    }
                    else
                    {
                        ChangeState(rmsg[2]);
                        FileDes = GetFileDescriptionFromString(rmsg[2], downloadPath);
                        ChangeState("文件数量:" + FileDes.Count().ToString());
                        onDirFile?.Invoke(this);
                    }
                }
                if(rmsg[0]=="FILE")
                {
                    ChangeState("收到关于文件[" + rmsg[1] + "]的请求回复");
                    File = null;
                    if(rmsg[2]=="FALSE")
                    {
                        ChangeState("请求失败");
                    }
                    else
                    {
                        ChangeState("回复内容:" + rmsg[2]);
                    }
                }
            }
            onDataString?.Invoke(this, msg);
        }

        /// <summary>
        /// 接受非文本型消息的处理函数
        /// </summary>
        /// <param name=""></param>
        private void GetByteMessage(byte[] result)
        {
            onData?.Invoke(this, result);
        }

        /// <summary>
        /// 请求指定目录结构
        /// </summary>
        /// <param name="dirname"></param>
        public void queryDir(string dirname,string downloadDirpath)
        {
            downloadPath = downloadDirpath;
            SendData("GETDIR@" + dirname);
        }

        /// <summary>
        /// 请求指定文件,返回true表示该文件存在,没有发送请求,返回false表示已发送请求
        /// </summary>
        /// <param name="file">文件名,不带后缀</param>
        public bool queryFile(FileDescription file)
        {
            File = file;
            if (File.Exist() == false)
            {
                SendData("GETFILE@" + file.getFullName());
                return false;
            }
            File = null;
            return true;
        }

        /// <summary>
        /// 请求仓库目录列表
        /// </summary>
        public void queryDirList()
        {
            SendData("GETDIRLIST@2");
        }

        /// <summary>
        /// 检测SOCKET是否仍然处于链接状态
        /// </summary>
        private void SocketConnectAlive()
        {
            while(client.Connected)
            {
                SocketConnectAliveTest();
                Thread.Sleep(3000);
            } 
        }

        /// <summary>
        /// 是否连接的心跳测试
        /// </summary>
        private void SocketConnectAliveTest()
        {

            if (MaxSize != -1) return;
            try
            {
                
                client.GetStream().WriteByte(0);
            }catch
            {
                Stop();
            }
        }

        /// <summary>
        /// 从文件描述字符串集合中获取所有文件描述的数组
        /// </summary>
        /// <param name="fstring"></param>
        /// <returns></returns>
        public List<FileDescription> GetFileDescriptionFromString(string fstring,string windowspath)
        {
            return FileDescription.getFileDescriptionFromString(fstring,windowspath);
        }

        /// <summary>
        /// 返回目录中第一个不存在的文件描述对象,全部存在时返回null
        /// </summary>
        /// <returns></returns>
        public FileDescription GetFileDescriptionFromFirstNotExise()
        {
            foreach (FileDescription i in FileDes)
            {
                if (i.Exist() == false)
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// 计算该Filesocket的传输目录中有多少文件已存在于本地
        /// </summary>
        /// <returns></returns>
        public  double GetFileSocketMissionCompleteAcc()
        {
            int t = 0, f = 0;
            foreach(FileDescription i in FileDes)
            {
                if(i.Exist()==true)
                {
                    t++;
                }
                else
                {
                    f++;
                }
            } 
            return 100.0*( Convert.ToDouble( t) / FileDes.Count());
        }
         
        /// <summary>
        /// 返回当前正在传输的文件描述对象
        /// </summary>
        /// <returns></returns>
        public FileDescription GetFileSocketTransferFile()
        {
            return File;
        }
    }
}
