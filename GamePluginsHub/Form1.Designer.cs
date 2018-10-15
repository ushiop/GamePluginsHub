namespace GamePluginsHub
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.Tips = new System.Windows.Forms.Label();
            this.LoadBar = new System.Windows.Forms.ProgressBar();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.Port = new System.Windows.Forms.TextBox();
            this.StartBut = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tips
            // 
            this.Tips.AutoSize = true;
            this.Tips.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Tips.Location = new System.Drawing.Point(12, 23);
            this.Tips.Name = "Tips";
            this.Tips.Size = new System.Drawing.Size(69, 19);
            this.Tips.TabIndex = 0;
            this.Tips.Text = "label1";
            // 
            // LoadBar
            // 
            this.LoadBar.Location = new System.Drawing.Point(114, 12);
            this.LoadBar.Name = "LoadBar";
            this.LoadBar.Size = new System.Drawing.Size(964, 37);
            this.LoadBar.TabIndex = 1;
            // 
            // treeView1
            // 
            this.treeView1.LineColor = System.Drawing.Color.DarkSlateGray;
            this.treeView1.Location = new System.Drawing.Point(14, 55);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(565, 502);
            this.treeView1.TabIndex = 2;
            // 
            // Port
            // 
            this.Port.Location = new System.Drawing.Point(193, 21);
            this.Port.MaxLength = 5;
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(106, 21);
            this.Port.TabIndex = 3;
            this.Port.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Port_KeyPress);
            // 
            // StartBut
            // 
            this.StartBut.Location = new System.Drawing.Point(346, 12);
            this.StartBut.Name = "StartBut";
            this.StartBut.Size = new System.Drawing.Size(711, 37);
            this.StartBut.TabIndex = 4;
            this.StartBut.Text = "button1";
            this.StartBut.UseVisualStyleBackColor = true;
            this.StartBut.Click += new System.EventHandler(this.StartBut_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(6, 20);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(492, 472);
            this.listBox1.TabIndex = 5;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.listBox1);
            this.groupBox1.Location = new System.Drawing.Point(585, 55);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(504, 502);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "客户端列表";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1090, 567);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.StartBut);
            this.Controls.Add(this.Port);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.LoadBar);
            this.Controls.Add(this.Tips);
            this.Name = "Form1";
            this.Text = "Game Plugins Hub";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.form_Load);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label Tips;
        private System.Windows.Forms.ProgressBar LoadBar;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.TextBox Port;
        private System.Windows.Forms.Button StartBut;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

