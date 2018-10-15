using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace GamePluginsHub
{
    /// <summary>
    /// 目录描述类
    /// 用于描述目录结构
    /// </summary>
    class DirDescription
    {
        
        private static string WindowsPath = Process.GetCurrentProcess().MainModule.FileName.Substring(0,
                                            Process.GetCurrentProcess().MainModule.FileName.Length-
                                                (Process.GetCurrentProcess().MainModule.FileName.Length- 
                                                Process.GetCurrentProcess().MainModule.FileName.LastIndexOf("\\"))
                                            );//目录的实际地址
        private string DirName;//目录名称
        private string DirPath;//目录完整路径 
        private DirDescription Root;//父级目录对象
        private List<DirDescription> Child=new List<DirDescription>();//子目录对象数组
        private List<FileDescription> File=new List<FileDescription>();//目录内文件对象数组
        private int LoadMissionMax = 0;//读取目录和文件的总任务数量
        private int LoadMissionNow = 0;//读取目录和文件的已完成任务数量
        private bool findMissionOK = false;//是否已生成完毕目录结构

        /// <summary>
        /// 根目录构造函数
        /// 用于描述一个目录的顶层目录
        /// </summary>
        /// <param name="dirname">根目录的名字</param>
        public DirDescription(string dirname)
        {
            DirName = dirname;
            DirPath = "";
            Root = null;
        }

        /// <summary>
        /// 根目录构造函数
        /// 用于描述一个具有父目录的目录
        /// </summary>
        /// <param name="rootDir">父目录的DirDescription对象引用</param>
        /// <param name="dirname">该目录的名字</param>
        public DirDescription(string dirname,DirDescription rootDir)
        {
            Root = rootDir;
            DirName = dirname;
            DirPath = Root.getDirPath() + "\\" + dirname;
        }

        /// <summary>
        /// 返回目录的完整路径
        /// </summary>
        /// <returns></returns>
        public string getDirPath()
        {
            return Root == null ? DirName: DirPath;
        } 

        /// <summary>
        /// 返回父级目录对象
        /// </summary>
        /// <returns></returns>
        public DirDescription getRoot()
        {
            return Root;
        }

        /// <summary>
        /// 返回最顶层父级目录对象
        /// </summary>
        /// <returns></returns>
        public DirDescription getSuperRoot()
        {
            DirDescription tmp = this;
            if (tmp.Root == null) return tmp;
            while(tmp.Root!=null)
            {
                tmp = tmp.getRoot();
            }
            return tmp;
        }

        /// <summary>
        /// 让该目录对象开始生成目录结构
        /// 返回FALSE表示目录不存在,返回TRUE表示生成完毕
        /// </summary>
        public bool findChild()
        {
            string path = WindowsPath + "\\" + getDirPath();
            
               
            if (Directory.Exists(path) == false) return false;
            int Files = Directory.GetFiles(path).Length;//当前目录下的文件数量
            int Dirs = Directory.GetDirectories(path).Length;//当前目录下的目录数量
            LoadMissionMax = Files + Dirs;
            LoadMissionNow = 0;
            DirectoryInfo Dir = new DirectoryInfo(path);
            foreach (FileInfo i in Dir.GetFiles())
            {
                File.Add(new FileDescription(i.FullName, getDirPath()));
                LoadMissionNow++;
            }
            foreach(DirectoryInfo i in Dir.GetDirectories())
            { 
                DirDescription tmp = new DirDescription(i.Name, this);
                Child.Add(tmp);
                tmp.findChild();
                LoadMissionNow++;
            }
             
            findMissionOK = true;
            return true;
        }

        /// <summary>
        /// 返回目录名
        /// </summary>
        /// <returns></returns>
        public string getDirName()
        {
            return DirName;
        }

        /// <summary>
        /// 返回该目录结构生成进度(0-100);
        ///
        /// </summary>
        /// <returns></returns>
        public int getFindMissionBar()
        {
            return LoadMissionMax==0?0: ((int)LoadMissionNow / LoadMissionMax)*100;
        }

        /// <summary>
        /// 返回该目录结构生成是否完毕
        /// </summary>
        /// <returns></returns>
        public bool getFindMissionState()
        {
            return findMissionOK;
        }

        /// <summary>
        /// 返回该目录下的所有文件描述类
        /// </summary>
        /// <returns></returns>
        public List<FileDescription> getDirFiles()
        {
            return File;
        }

        /// <summary>
        /// 返回该目录下的所有目录描述类
        /// </summary>
        /// <returns></returns>
        public List<DirDescription> getDirChilds()
        {
            return Child;
        }

        /// <summary>
        /// 返回该目录下的树形节点图
        /// </summary>
        /// <returns></returns>
        public TreeNode getDirTree()
        {
            if (getDirFiles().Count() == 0 && getDirChilds().Count() == 0) return null;
            TreeNode tmp = new TreeNode(DirName);
            foreach (FileDescription i in File)
            {
                TreeNode f = new TreeNode(i.getFullName());
                f.Nodes.Add("路径:" + i.getDir());
                f.Nodes.Add("MD5:" + i.getMD5());

                tmp.Nodes.Add(f);
            }

            foreach (DirDescription i in Child)
            {
                TreeNode f = i.getDirTree();
                if (f != null)
                {
                    tmp.Nodes.Add(f);
                }

            }
            if (tmp.Nodes.Count == 0) return null;
            return tmp;
        }


        /// <summary>
        /// 返回该目录的所有文件的结构描述字符串
        /// </summary>
        /// <returns></returns>
        public string getDirScriptionString()
        {
            string str = "";
            foreach(FileDescription i in File)
            {
                str = str + i.getFileDescriptionString();
            }
            foreach(DirDescription i in Child)
            {
                str = str + i.getDirScriptionString();
            }
            return str;
        }

    }
}
