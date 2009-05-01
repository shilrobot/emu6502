namespace Emu6502
{
    partial class Debugger
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cFlag = new System.Windows.Forms.CheckBox();
            this.zFlag = new System.Windows.Forms.CheckBox();
            this.iFlag = new System.Windows.Forms.CheckBox();
            this.vFlag = new System.Windows.Forms.CheckBox();
            this.nFlag = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.spLabel = new System.Windows.Forms.Label();
            this.pcLabel = new System.Windows.Forms.Label();
            this.yLabel = new System.Windows.Forms.Label();
            this.xLabel = new System.Windows.Forms.Label();
            this.aLabel = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugGo = new System.Windows.Forms.ToolStripMenuItem();
            this.debugStepInto = new System.Windows.Forms.ToolStripMenuItem();
            this.debugStepOver = new System.Windows.Forms.ToolStripMenuItem();
            this.debugBreakpoints = new System.Windows.Forms.ToolStripMenuItem();
            this.interruptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.interruptReset = new System.Windows.Forms.ToolStripMenuItem();
            this.interruptNMI = new System.Windows.Forms.ToolStripMenuItem();
            this.interruptIRQ = new System.Windows.Forms.ToolStripMenuItem();
            this.disassembly2 = new Emu6502.DisassemblyWindow();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cFlag);
            this.groupBox1.Controls.Add(this.zFlag);
            this.groupBox1.Controls.Add(this.iFlag);
            this.groupBox1.Controls.Add(this.vFlag);
            this.groupBox1.Controls.Add(this.nFlag);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.spLabel);
            this.groupBox1.Controls.Add(this.pcLabel);
            this.groupBox1.Controls.Add(this.yLabel);
            this.groupBox1.Controls.Add(this.xLabel);
            this.groupBox1.Controls.Add(this.aLabel);
            this.groupBox1.Location = new System.Drawing.Point(577, 28);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupBox1.Size = new System.Drawing.Size(274, 488);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CPU Status";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 68);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 16);
            this.label1.TabIndex = 12;
            this.label1.Text = "       7654 3210";
            // 
            // cFlag
            // 
            this.cFlag.AutoSize = true;
            this.cFlag.Location = new System.Drawing.Point(122, 157);
            this.cFlag.Name = "cFlag";
            this.cFlag.Size = new System.Drawing.Size(15, 14);
            this.cFlag.TabIndex = 11;
            this.cFlag.UseVisualStyleBackColor = true;
            this.cFlag.CheckedChanged += new System.EventHandler(this.cFlag_CheckedChanged);
            // 
            // zFlag
            // 
            this.zFlag.AutoSize = true;
            this.zFlag.Location = new System.Drawing.Point(106, 157);
            this.zFlag.Name = "zFlag";
            this.zFlag.Size = new System.Drawing.Size(15, 14);
            this.zFlag.TabIndex = 10;
            this.zFlag.UseVisualStyleBackColor = true;
            this.zFlag.CheckedChanged += new System.EventHandler(this.zFlag_CheckedChanged);
            // 
            // iFlag
            // 
            this.iFlag.AutoSize = true;
            this.iFlag.Location = new System.Drawing.Point(90, 157);
            this.iFlag.Name = "iFlag";
            this.iFlag.Size = new System.Drawing.Size(15, 14);
            this.iFlag.TabIndex = 9;
            this.iFlag.UseVisualStyleBackColor = true;
            this.iFlag.CheckedChanged += new System.EventHandler(this.iFlag_CheckedChanged);
            // 
            // vFlag
            // 
            this.vFlag.AutoSize = true;
            this.vFlag.Location = new System.Drawing.Point(26, 157);
            this.vFlag.Name = "vFlag";
            this.vFlag.Size = new System.Drawing.Size(15, 14);
            this.vFlag.TabIndex = 7;
            this.vFlag.UseVisualStyleBackColor = true;
            this.vFlag.CheckedChanged += new System.EventHandler(this.vFlag_CheckedChanged);
            // 
            // nFlag
            // 
            this.nFlag.AutoSize = true;
            this.nFlag.Location = new System.Drawing.Point(10, 157);
            this.nFlag.Name = "nFlag";
            this.nFlag.Size = new System.Drawing.Size(15, 14);
            this.nFlag.TabIndex = 6;
            this.nFlag.UseVisualStyleBackColor = true;
            this.nFlag.CheckedChanged += new System.EventHandler(this.nFlag_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 138);
            this.label6.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(128, 16);
            this.label6.TabIndex = 5;
            this.label6.Text = "N V - B D I Z C";
            // 
            // spLabel
            // 
            this.spLabel.AutoSize = true;
            this.spLabel.Location = new System.Drawing.Point(9, 44);
            this.spLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.spLabel.Name = "spLabel";
            this.spLabel.Size = new System.Drawing.Size(56, 16);
            this.spLabel.TabIndex = 4;
            this.spLabel.Text = "SP $FF";
            // 
            // pcLabel
            // 
            this.pcLabel.AutoSize = true;
            this.pcLabel.Location = new System.Drawing.Point(9, 23);
            this.pcLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.pcLabel.Name = "pcLabel";
            this.pcLabel.Size = new System.Drawing.Size(72, 16);
            this.pcLabel.TabIndex = 3;
            this.pcLabel.Text = "PC $FFFF";
            // 
            // yLabel
            // 
            this.yLabel.AutoSize = true;
            this.yLabel.Location = new System.Drawing.Point(9, 116);
            this.yLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.yLabel.Name = "yLabel";
            this.yLabel.Size = new System.Drawing.Size(208, 16);
            this.yLabel.TabIndex = 2;
            this.yLabel.Text = "Y  $FF 1111 1111 256 -128";
            // 
            // xLabel
            // 
            this.xLabel.AutoSize = true;
            this.xLabel.Location = new System.Drawing.Point(9, 100);
            this.xLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.xLabel.Name = "xLabel";
            this.xLabel.Size = new System.Drawing.Size(208, 16);
            this.xLabel.TabIndex = 1;
            this.xLabel.Text = "X  $FF 1111 1111 256 -128";
            // 
            // aLabel
            // 
            this.aLabel.AutoSize = true;
            this.aLabel.Location = new System.Drawing.Point(9, 84);
            this.aLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.aLabel.Name = "aLabel";
            this.aLabel.Size = new System.Drawing.Size(208, 16);
            this.aLabel.TabIndex = 0;
            this.aLabel.Text = "A  $FF 1111 1111 256 -128";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.interruptToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(865, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // fileExit
            // 
            this.fileExit.Name = "fileExit";
            this.fileExit.Size = new System.Drawing.Size(92, 22);
            this.fileExit.Text = "E&xit";
            this.fileExit.Click += new System.EventHandler(this.fileExit_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugGo,
            this.debugStepInto,
            this.debugStepOver,
            this.debugBreakpoints});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "&Debug";
            // 
            // debugGo
            // 
            this.debugGo.Name = "debugGo";
            this.debugGo.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.debugGo.Size = new System.Drawing.Size(150, 22);
            this.debugGo.Text = "&Go / Break";
            this.debugGo.Click += new System.EventHandler(this.debugGo_Click);
            // 
            // debugStepInto
            // 
            this.debugStepInto.Name = "debugStepInto";
            this.debugStepInto.ShortcutKeys = System.Windows.Forms.Keys.F11;
            this.debugStepInto.Size = new System.Drawing.Size(150, 22);
            this.debugStepInto.Text = "Step &Into";
            // 
            // debugStepOver
            // 
            this.debugStepOver.Name = "debugStepOver";
            this.debugStepOver.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.debugStepOver.Size = new System.Drawing.Size(150, 22);
            this.debugStepOver.Text = "Step &Over";
            this.debugStepOver.Click += new System.EventHandler(this.debugStepOver_Click);
            // 
            // debugBreakpoints
            // 
            this.debugBreakpoints.Name = "debugBreakpoints";
            this.debugBreakpoints.Size = new System.Drawing.Size(150, 22);
            this.debugBreakpoints.Text = "Breakpoints...";
            this.debugBreakpoints.Click += new System.EventHandler(this.debugBreakpoints_Click);
            // 
            // interruptToolStripMenuItem
            // 
            this.interruptToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.interruptReset,
            this.interruptNMI,
            this.interruptIRQ});
            this.interruptToolStripMenuItem.Name = "interruptToolStripMenuItem";
            this.interruptToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.interruptToolStripMenuItem.Text = "&Interrupt";
            // 
            // interruptReset
            // 
            this.interruptReset.Name = "interruptReset";
            this.interruptReset.Size = new System.Drawing.Size(102, 22);
            this.interruptReset.Text = "&Reset";
            this.interruptReset.Click += new System.EventHandler(this.interruptReset_Click);
            // 
            // interruptNMI
            // 
            this.interruptNMI.Name = "interruptNMI";
            this.interruptNMI.Size = new System.Drawing.Size(102, 22);
            this.interruptNMI.Text = "&NMI";
            this.interruptNMI.Click += new System.EventHandler(this.interruptNMI_Click);
            // 
            // interruptIRQ
            // 
            this.interruptIRQ.Name = "interruptIRQ";
            this.interruptIRQ.Size = new System.Drawing.Size(102, 22);
            this.interruptIRQ.Text = "&IRQ";
            this.interruptIRQ.Click += new System.EventHandler(this.interruptIRQ_Click);
            // 
            // disassembly2
            // 
            this.disassembly2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.disassembly2.InstrNum = 0;
            this.disassembly2.Location = new System.Drawing.Point(0, 27);
            this.disassembly2.Name = "disassembly2";
            this.disassembly2.Nes = null;
            this.disassembly2.Size = new System.Drawing.Size(569, 502);
            this.disassembly2.StartAddress = 0;
            this.disassembly2.TabIndex = 1;
            this.disassembly2.Text = "disassemblyWindow1";
            // 
            // Debugger
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(865, 529);
            this.Controls.Add(this.disassembly2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "Debugger";
            this.Text = "Debugger";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Debugger_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label aLabel;
        private System.Windows.Forms.Label yLabel;
        private System.Windows.Forms.Label xLabel;
        private System.Windows.Forms.Label spLabel;
        private System.Windows.Forms.Label pcLabel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox nFlag;
        private System.Windows.Forms.CheckBox cFlag;
        private System.Windows.Forms.CheckBox zFlag;
        private System.Windows.Forms.CheckBox iFlag;
        private System.Windows.Forms.CheckBox vFlag;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileExit;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugGo;
        private System.Windows.Forms.ToolStripMenuItem debugStepInto;
        private System.Windows.Forms.ToolStripMenuItem debugStepOver;
        private System.Windows.Forms.ToolStripMenuItem interruptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem interruptReset;
        private System.Windows.Forms.ToolStripMenuItem interruptNMI;
        private System.Windows.Forms.ToolStripMenuItem interruptIRQ;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem debugBreakpoints;
        private DisassemblyWindow disassembly2;
    }
}