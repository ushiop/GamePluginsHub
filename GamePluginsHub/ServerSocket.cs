using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Threading;

namespace GamePluginsHub
{
    class ServerSocket
    {
        private TcpListener ServerMain;//主socket
        private bool ServerState = false;//服务器是否启动
        private List<FileSocket> ClientSockets = new List<FileSocket>();//客户端SOCKET列表
        private static Form1 window;
        private Thread recvThread;

        public delegate void onCloseServerHandler(object s, EventArgs e);
        public event onCloseServerHandler onCloseServer;

        /// <summary>
        /// 监听指定端口
        /// </summary>
        /// <param name="port"></param>
        public ServerSocket(Form1 form,int port)
        {
            ServerMain = new TcpListener(IPAddress.Any,port);
            window = form;
        }
            
        /// <summary>
        /// 开始监听
        /// </summary>
        /// <returns></returns>
        public bool Start()
        { 
            try
            {
                ServerMain.Start();
                ServerState = true;
                recvThread = new Thread(RecvMessage);
                recvThread.Start();
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 停止监听
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            try
            {

                recvThread.Abort();
                ServerState = false;
                ServerMain.Stop();
                onCloseServer?.Invoke(this, new EventArgs());
                
                foreach (FileSocket i in ClientSockets)
                {
                    i.Stop();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取SOCKET是否开始监听
        /// </summary>
        /// <returns></returns>
        public bool getServerState()
        {
            return ServerState;
        }
        
        /// <summary>
        /// 监听线程
        /// </summary>
        private void RecvMessage()
        {
            while(ServerState==true)
            {
                TcpClient c=ServerMain.AcceptTcpClient();
                FileSocket client = new FileSocket(c);
                client.onStateChange += Client_onStateChange1;
                
                ClientSockets.Add(client); 
                Form1.Inove_ListBox_FulshClientList f = new Form1.Inove_ListBox_FulshClientList(window.ListBox_FulshClientList);
                window.BeginInvoke(f, client);
 

            }
        }

        private void Client_onStateChange1(object serder, string oldState, string newState)
        {
            Form1.Inove_ListBox_FulshClientList f = new Form1.Inove_ListBox_FulshClientList(window.ListBox_FulshClientList);
            window.BeginInvoke(f, (FileSocket)serder);
        }

 
    }
}
