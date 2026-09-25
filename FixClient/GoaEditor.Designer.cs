/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: GoaEditor.Designer.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////

namespace FixClient
{
    partial class GoaEditor
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
            this.goaTextBox = new System.Windows.Forms.TextBox();
            this.OKbutton = new System.Windows.Forms.Button();
            this.ctrlBcheckbox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // goaTextBox
            // 
            this.goaTextBox.AcceptsReturn = true;
            this.goaTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.goaTextBox.Location = new System.Drawing.Point(12, 12);
            this.goaTextBox.Multiline = true;
            this.goaTextBox.Name = "goaTextBox";
            this.goaTextBox.Size = new System.Drawing.Size(228, 110);
            this.goaTextBox.TabIndex = 0;
            // 
            // OKbutton
            // 
            this.OKbutton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.OKbutton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKbutton.Location = new System.Drawing.Point(250, 99);
            this.OKbutton.Name = "OKbutton";
            this.OKbutton.Size = new System.Drawing.Size(75, 23);
            this.OKbutton.AutoSize = true;
            this.OKbutton.MinimumSize = new System.Drawing.Size(75, 23);
            this.OKbutton.TabIndex = 1;
            this.OKbutton.Text = "OK";
            this.OKbutton.UseVisualStyleBackColor = true;
            // 
            // ctrlBcheckbox
            // 
            this.ctrlBcheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ctrlBcheckbox.AutoSize = true;
            this.ctrlBcheckbox.Checked = true;
            this.ctrlBcheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ctrlBcheckbox.Location = new System.Drawing.Point(250, 12);
            this.ctrlBcheckbox.Name = "ctrlBcheckbox";
            this.ctrlBcheckbox.Size = new System.Drawing.Size(79, 24);
            this.ctrlBcheckbox.TabIndex = 2;
            this.ctrlBcheckbox.Text = "Convert ^B";
            this.ctrlBcheckbox.UseVisualStyleBackColor = true;
            // 
            // GoaEditor
            // 
            this.AcceptButton = this.OKbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 134);
            this.Controls.Add(this.ctrlBcheckbox);
            this.Controls.Add(this.OKbutton);
            this.Controls.Add(this.goaTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 500);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(200, 71);
            this.Name = "GoaEditor";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "GOA Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox goaTextBox;
        private System.Windows.Forms.Button OKbutton;
        private System.Windows.Forms.CheckBox ctrlBcheckbox;
    }
}