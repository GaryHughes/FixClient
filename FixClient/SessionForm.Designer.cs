/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: SessionForm.Designer.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////

﻿namespace FixClient
{
	partial class SessionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SessionForm));
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this._gridPlaceHolder = new System.Windows.Forms.Panel();
            this._buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this._buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // OK
            // 
            this.OK.AutoSize = true;
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.OK.MinimumSize = new System.Drawing.Size(88, 27);
            this.OK.Name = "OK";
            this.OK.TabIndex = 0;
            this.OK.Text = "OK";
            // 
            // Cancel
            // 
            this.Cancel.AutoSize = true;
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.Cancel.MinimumSize = new System.Drawing.Size(88, 27);
            this.Cancel.Name = "Cancel";
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel";
            // 
            // _buttonPanel
            // 
            this._buttonPanel.AutoSize = true;
            this._buttonPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._buttonPanel.Controls.Add(this.Cancel);
            this._buttonPanel.Controls.Add(this.OK);
            this._buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this._buttonPanel.Name = "_buttonPanel";
            this._buttonPanel.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this._buttonPanel.TabIndex = 1;
            this._buttonPanel.WrapContents = false;
            // 
            // _gridPlaceHolder
            // 
            this._gridPlaceHolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this._gridPlaceHolder.Name = "_gridPlaceHolder";
            this._gridPlaceHolder.TabIndex = 0;
            // 
            // SessionForm
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(560, 720);
            this.Controls.Add(this._gridPlaceHolder);
            this.Controls.Add(this._buttonPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(420, 400);
            this.Name = "SessionForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configuration";
            this._buttonPanel.ResumeLayout(false);
            this._buttonPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Panel _gridPlaceHolder;
        private System.Windows.Forms.FlowLayoutPanel _buttonPanel;
	}
}