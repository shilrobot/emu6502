namespace Emu6502
{
    partial class Form1
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
            this.hexBox = new System.Windows.Forms.TextBox();
            this.outputBox = new System.Windows.Forms.TextBox();
            this.disassemble = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hexBox
            // 
            this.hexBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexBox.Location = new System.Drawing.Point(12, 12);
            this.hexBox.Multiline = true;
            this.hexBox.Name = "hexBox";
            this.hexBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.hexBox.Size = new System.Drawing.Size(300, 458);
            this.hexBox.TabIndex = 0;
            // 
            // outputBox
            // 
            this.outputBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputBox.Location = new System.Drawing.Point(318, 12);
            this.outputBox.Multiline = true;
            this.outputBox.Name = "outputBox";
            this.outputBox.ReadOnly = true;
            this.outputBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputBox.Size = new System.Drawing.Size(320, 498);
            this.outputBox.TabIndex = 1;
            // 
            // disassemble
            // 
            this.disassemble.Location = new System.Drawing.Point(12, 476);
            this.disassemble.Name = "disassemble";
            this.disassemble.Size = new System.Drawing.Size(300, 34);
            this.disassemble.TabIndex = 2;
            this.disassemble.Text = "DISASEMBERU";
            this.disassemble.UseVisualStyleBackColor = true;
            this.disassemble.Click += new System.EventHandler(this.disassemble_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 522);
            this.Controls.Add(this.disassemble);
            this.Controls.Add(this.outputBox);
            this.Controls.Add(this.hexBox);
            this.Name = "Form1";
            this.Text = "6502";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox hexBox;
        private System.Windows.Forms.TextBox outputBox;
        private System.Windows.Forms.Button disassemble;
    }
}

