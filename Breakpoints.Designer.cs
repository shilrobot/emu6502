namespace Emu6502
{
    partial class Breakpoints
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
            this.breakpointList = new System.Windows.Forms.ListBox();
            this.breakpointEdit = new System.Windows.Forms.TextBox();
            this.addBtn = new System.Windows.Forms.Button();
            this.removeBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // breakpointList
            // 
            this.breakpointList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.breakpointList.FormattingEnabled = true;
            this.breakpointList.ItemHeight = 16;
            this.breakpointList.Location = new System.Drawing.Point(12, 41);
            this.breakpointList.Name = "breakpointList";
            this.breakpointList.Size = new System.Drawing.Size(262, 260);
            this.breakpointList.TabIndex = 0;
            // 
            // breakpointEdit
            // 
            this.breakpointEdit.Location = new System.Drawing.Point(12, 12);
            this.breakpointEdit.Name = "breakpointEdit";
            this.breakpointEdit.Size = new System.Drawing.Size(100, 22);
            this.breakpointEdit.TabIndex = 1;
            // 
            // addBtn
            // 
            this.addBtn.Location = new System.Drawing.Point(118, 11);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(75, 23);
            this.addBtn.TabIndex = 2;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // removeBtn
            // 
            this.removeBtn.Location = new System.Drawing.Point(199, 11);
            this.removeBtn.Name = "removeBtn";
            this.removeBtn.Size = new System.Drawing.Size(75, 23);
            this.removeBtn.TabIndex = 3;
            this.removeBtn.Text = "Remove";
            this.removeBtn.UseVisualStyleBackColor = true;
            // 
            // Breakpoints
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(285, 319);
            this.Controls.Add(this.removeBtn);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.breakpointEdit);
            this.Controls.Add(this.breakpointList);
            this.Name = "Breakpoints";
            this.Text = "Breakpoints";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox breakpointList;
        private System.Windows.Forms.TextBox breakpointEdit;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.Button removeBtn;
    }
}