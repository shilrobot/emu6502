namespace Emu6502
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.breakpointsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.memoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.systemToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.softResetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hardResetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rOMInfoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stackToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.zeroPageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.systemToolStripMenuItem,
            this.windowToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(575, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.recentToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "&Open...";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.recentToolStripMenuItem.Text = "&Recently Used";
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rOMInfoToolStripMenuItem,
            this.debuggerToolStripMenuItem,
            this.breakpointsToolStripMenuItem,
            this.memoryToolStripMenuItem,
            this.stackToolStripMenuItem,
            this.zeroPageToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "&Window";
            // 
            // debuggerToolStripMenuItem
            // 
            this.debuggerToolStripMenuItem.Name = "debuggerToolStripMenuItem";
            this.debuggerToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.debuggerToolStripMenuItem.Text = "&Debugger";
            // 
            // breakpointsToolStripMenuItem
            // 
            this.breakpointsToolStripMenuItem.Name = "breakpointsToolStripMenuItem";
            this.breakpointsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.breakpointsToolStripMenuItem.Text = "&Breakpoints";
            // 
            // memoryToolStripMenuItem
            // 
            this.memoryToolStripMenuItem.Name = "memoryToolStripMenuItem";
            this.memoryToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.memoryToolStripMenuItem.Text = "&Memory";
            // 
            // systemToolStripMenuItem
            // 
            this.systemToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.softResetToolStripMenuItem,
            this.hardResetToolStripMenuItem});
            this.systemToolStripMenuItem.Name = "systemToolStripMenuItem";
            this.systemToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.systemToolStripMenuItem.Text = "&System";
            // 
            // softResetToolStripMenuItem
            // 
            this.softResetToolStripMenuItem.Name = "softResetToolStripMenuItem";
            this.softResetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.softResetToolStripMenuItem.Text = "&Soft Reset";
            // 
            // hardResetToolStripMenuItem
            // 
            this.hardResetToolStripMenuItem.Name = "hardResetToolStripMenuItem";
            this.hardResetToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.hardResetToolStripMenuItem.Text = "&Hard Reset";
            // 
            // rOMInfoToolStripMenuItem
            // 
            this.rOMInfoToolStripMenuItem.Name = "rOMInfoToolStripMenuItem";
            this.rOMInfoToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.rOMInfoToolStripMenuItem.Text = "&ROM Info";
            // 
            // stackToolStripMenuItem
            // 
            this.stackToolStripMenuItem.Name = "stackToolStripMenuItem";
            this.stackToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.stackToolStripMenuItem.Text = "&Stack";
            // 
            // zeroPageToolStripMenuItem
            // 
            this.zeroPageToolStripMenuItem.Name = "zeroPageToolStripMenuItem";
            this.zeroPageToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.zeroPageToolStripMenuItem.Text = "&Zero Page";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(575, 474);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "NESbert";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem systemToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem softResetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hardResetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debuggerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem breakpointsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem memoryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rOMInfoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem stackToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem zeroPageToolStripMenuItem;
    }
}