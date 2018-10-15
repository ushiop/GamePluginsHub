using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace GamePluginsHub
{
    /// <summary>
    /// 文件描述类
    /// 用于描述一个文件的信息
    /// </summary>
    public class FileDescription
    {
        private string FileName;//文件名(不包含后缀)
        private string FileFullName;//文件名(包含后缀)
        private string FileFullPath;//文件绝对完整路径(包含文件名)
        private string FileFullPathDir;//文件绝对完整路径(不包含文件名)
        private string FileMd5;//文件MD5码
        private string FileDir;//文件相对目录路径;


        public FileDescription(string filepath,string dir)
        {
            int lastSplitPos = filepath.LastIndexOf("\\");
            int lastTypePos = filepath.LastIndexOf(".");
            FileName = filepath.Substring(lastSplitPos + 1, lastTypePos - (lastSplitPos + 1));
            FileFullName = filepath.Substring(lastSplitPos + 1);
            FileFullPath = filepath;
            FileFullPathDir = filepath.Substring(0,filepath.Length-( filepath.Length - (lastSplitPos+1)));
 
            FileMd5 = File.GetFileMd5(FileFullPath);
            FileDir = dir;
        }

        /// <summary>
        /// 由文件描述字符串集合生成文件描述对象的构造方法
        /// </summary>
        /// <param name="fFullName">文件完整名称(包含后缀)</param>
        /// <param name="fDir">文件相对路径,若路径内不包含'\'则会自动转换为当前程序的根目录路径</param>
        /// <param name="fMD5">文件MD5</param>
        /// <param name="wDir">该文件的磁盘根目录路径</param>
        public FileDescription(string fFullName,string fDir,string fMD5,string wDir)
        {
            int lastSpliePos = fDir.IndexOf('\\');
            int lastTypePos = fFullName.IndexOf(".");
            FileName = fFullName.Substring(0, lastTypePos);
            FileFullName = fFullName;
            FileMd5 = fMD5;
            FileDir = fDir;
            if(lastSpliePos==-1)
            {
                FileFullPath = wDir + "\\"+ FileFullName;
            }
            else
            {
                FileFullPath = wDir + "\\" + FileDir.Substring(lastSpliePos + 1) + "\\" + FileFullName;
            }
            FileFullPathDir = FileFullPath.Substring(0, FileFullPath.Length - (FileFullPath.Length - (FileFullPath.LastIndexOf("\\")+1)));
        }

        /// <summary>
        /// 返回不包含后缀的文件名
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return FileName;
        }

        /// <summary>
        /// 返回包含后缀的文件名
        /// </summary>
        /// <returns></returns>
        public string getFullName()
        {
            return FileFullName;
        }

 
        /// <summary>
        /// 返回包含后缀的文件真实路径
        /// </summary>
        /// <returns></returns>
        public string getFullPath()
        {
            return FileFullPath;
        }

        /// <summary>
        /// 返回文件的DM5码
        /// </summary>
        /// <returns></returns>
        public string getMD5()
        {
            return FileMd5;
        }

        /// <summary>
        /// 返回文件的当前目录名(相对)
        /// </summary>
        /// <returns></returns>
        public string getDir()
        {
            return FileDir;
        }

        /// <summary>
        /// 返回该文件的结构描述字符串
        /// </summary>
        /// <returns></returns>
        public string getFileDescriptionString()
        {
            string str = "#";
            str = str + "FILEMD5:" + getMD5()+"/";
            str = str + "FILENAME:" + getFullName() + "/";
            str = str + "FILEDIR:" + getDir();
            str = str + "#";
            return str;
        }

        /// <summary>
        /// 获取FileFullPath下该文件是否存在
        /// true为存在,false为不存在
        /// 同名且不同内容的文件会自动备份,同内容不同名的文件会自动改为正确的名字
        /// </summary>
        /// <returns></returns>
        public bool Exist()
        {
            if(System.IO.File.Exists(FileFullPath)==true)
            {
                string md5=File.GetFileMd5(FileFullPath);
                if(md5==FileMd5)
                {
                    return true;
                }
                else
                { 
                    System.IO.File.Move(FileFullPath, FileFullPathDir + "BACKUP_" + DateTime.UtcNow.Ticks+"_" + FileFullName);
                }
            }
            if (Directory.Exists(FileFullPathDir) == false)
            {
                Directory.CreateDirectory(FileFullPathDir);
                return false;
            }
            DirectoryInfo dir = new DirectoryInfo(FileFullPathDir);
            foreach (FileInfo i in dir.GetFiles())
            {
                string md5 = File.GetFileMd5(i.FullName);
                if (md5 == FileMd5)
                {
                    System.IO.File.Move(i.FullName,FileFullPath);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 从文件描述字符串集合中返回每一个文件的文件描述对象
        /// </summary>
        /// <param name="DescriptionString">由DirDescription转换出来的文件描述字符串合集</param>
        /// <param name="wDir">指定该文件应该以哪个目录作为根目录</param>
        /// <returns></returns>
        public static List<FileDescription> getFileDescriptionFromString(string DescriptionString,string wDir)
        {
            List<FileDescription> files = new List<FileDescription>();
            string[] filelist = DescriptionString.Split('#');
            foreach(string i in filelist)
            {
                if (i == "") continue;
                string[] filevalue = i.Split('/');
                string md5="", path="", name="";
                foreach(string z in filevalue)
                {
                    string[] value = z.Split(':');
                    if (value[0] == "FILEMD5")
                    {
                        md5 = value[1];
                    }
                    if(value[0]=="FILENAME")
                    {
                        name = value[1];
                    }
                    if(value[0]=="FILEDIR")
                    {
                        path = value[1];
                    }
                }
                files.Add(new FileDescription(name, path, md5,wDir));
            } 
            return files;
        }
        
    }
}
