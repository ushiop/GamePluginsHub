using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO; 


namespace GamePluginsHub
{
    public partial class Form1 : Form
    {
        public delegate void Inove_ProcessBar(int Bar);
        private static List<DirDescription> dir = new List<DirDescription>();
        private static string WindowsPath = Process.GetCurrentProcess().MainModule.FileName.Substring(0,
                                        Process.GetCurrentProcess().MainModule.FileName.Length -
                                            (Process.GetCurrentProcess().MainModule.FileName.Length -
                                            Process.GetCurrentProcess().MainModule.FileName.LastIndexOf("\\"))
                                        );
        private ServerSocket Server;
        public delegate void Inove_ListBox_FulshClientList(FileSocket s);

        public Form1()
        {
            InitializeComponent();
        }

        private void form_Load(object sender, EventArgs e)
        {
            Tips.Text = "读取目录..";
            DirectoryInfo root = new DirectoryInfo(WindowsPath);
            foreach (DirectoryInfo i in root.GetDirectories())
            {
                Tips.Text = "读取" + i.Name;
                LoadBar.Value = 0;
                DirDescription p = new DirDescription(i.Name);
                Thread read = new Thread(new ParameterizedThreadStart(readDir));
                read.Start(p);
                while (p.getFindMissionState() == false)
                {
                    LoadBar.Value = p.getFindMissionBar();
                }
                LoadBar.Value = p.getFindMissionBar();
                dir.Add(p);
                TreeNode f = p.getDirTree();
                if (f != null)
                {
                    treeView1.Nodes.Add(p.getDirTree());
                }
            }
            LoadBar.Visible = false;
            Tips.Text = "输入端口号后点启动";
            Port.Visible = true;
            StartBut.Visible = true;
            StartBut.Text = "启动";

           
        }

        private void readDir(object obj)
        {
            DirDescription dir = (DirDescription)obj;
            dir.findChild();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }



        private void Port_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Back) return;
            if (e.KeyChar < '0' || e.KeyChar > '9' )
            {
                e.Handled = true;
                return;
            }
            if (Port.Text.Length == 0)
            {
                if (e.KeyChar == '0')
                {
                    MessageBox.Show("端口号首位不能为0");
                    e.Handled = true;
                    return;

                }
            }
        }

        private void StartBut_Click(object sender, EventArgs e)
        {
            if (Server == null ? false : Server.getServerState()) 
            {
                Server.Stop();
                Port.Enabled = true;
                Tips.Text = "输入端口号后点启动";
                StartBut.Text = "启动";
            }
            else
            {

                if (Port.Text.Length == 0)
                {
                    MessageBox.Show("端口号不能为空");
                    return;
                }

                Server = new ServerSocket(this,Convert.ToInt32(Port.Text));
                if(Server.Start()==false)
                {
                    MessageBox.Show("启动失败,该端口已被占用!");
                }
                else
                {
                    Port.Enabled = false;
                    Tips.Text = "服务运行中";
                    StartBut.Text = "关闭";
                }

            }

        }

        public void ListBox_FulshClientList(FileSocket client)
        {
            int index = -1;
            int tmp = 0;
            foreach(string i in listBox1.Items)
            {
                if(i.IndexOf(client.GetRemote()[0]+":"+client.GetRemote()[1])!=-1)
                {
                    index = tmp;
                    break;
                }
                tmp = tmp + 1;
            }
            if (index == -1)
            {
                listBox1.Items.Add(client.GetRemote()[0] + ":" + client.GetRemote()[1] + " [状态]:" + client.GetFileSocketState());
               
            }
            else
            {
                listBox1.Items[index] = client.GetRemote()[0] + ":" + client.GetRemote()[1] + " [状态]:" + client.GetFileSocketState();
            } 

        }

        /// <summary>
        /// 通过目录名称获取所有文件的描述集合
        /// </summary>
        /// <param name="dirname"></param>
        public static string GetDirFilesFromDirName(string dirname)
        {

            foreach(DirDescription i in dir)
            {
                if(i.getDirName()==dirname)
                {
                   
                    return i.getDirScriptionString();
                }
            }
            return "NULL";
        }

        /// <summary>
        /// 返回目录名称列表,由@分割
        /// </summary>
        /// <returns></returns>
        public static string GetDirList()
        {
            string msg = "";
            foreach(DirDescription i in dir)
            {
                msg = msg + i.getDirName() + "@";
            }
            return msg;
        }

    }
}
