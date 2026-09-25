/////////////////////////////////////////////////
//
// FIX Client
//
// Copyright @ 2021 VIRTU Financial Inc.
// All rights reserved.
//
// Filename: ResetForm.Designer.cs
// Author:   Gary Hughes
//
/////////////////////////////////////////////////

﻿namespace FixClient
{
    partial class ResetForm
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
            this.iconPictureBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.noButton = new System.Windows.Forms.Button();
            this.yesButton = new System.Windows.Forms.Button();
            this.retainActiveGtdOrdersCheckBox = new System.Windows.Forms.CheckBox();
            this.retainActiveGtcOrdersCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.resetGeneratedIdsCheckBox = new System.Windows.Forms.CheckBox();
            this.optionsPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.contentPanel = new System.Windows.Forms.TableLayoutPanel();
            this.buttonPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.rootPanel = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.optionsPanel.SuspendLayout();
            this.contentPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.rootPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // iconPictureBox
            // 
            this.iconPictureBox.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.iconPictureBox.Name = "iconPictureBox";
            this.contentPanel.SetRowSpan(this.iconPictureBox, 2);
            this.iconPictureBox.Size = new System.Drawing.Size(48, 48);
            this.iconPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconPictureBox.TabIndex = 1;
            this.iconPictureBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.label1.MaximumSize = new System.Drawing.Size(360, 0);
            this.label1.Name = "label1";
            this.label1.TabIndex = 2;
            this.label1.Text = "This will erase the session message history and reset the sequence numbers, conti" +
    "nue?";
            // 
            // retainActiveGtcOrdersCheckBox
            // 
            this.retainActiveGtcOrdersCheckBox.AutoSize = true;
            this.retainActiveGtcOrdersCheckBox.Checked = true;
            this.retainActiveGtcOrdersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.retainActiveGtcOrdersCheckBox.Name = "retainActiveGtcOrdersCheckBox";
            this.retainActiveGtcOrdersCheckBox.TabIndex = 1;
            this.retainActiveGtcOrdersCheckBox.Text = "Retain active GTC orders";
            this.retainActiveGtcOrdersCheckBox.UseVisualStyleBackColor = true;
            // 
            // retainActiveGtdOrdersCheckBox
            // 
            this.retainActiveGtdOrdersCheckBox.AutoSize = true;
            this.retainActiveGtdOrdersCheckBox.Checked = true;
            this.retainActiveGtdOrdersCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.retainActiveGtdOrdersCheckBox.Name = "retainActiveGtdOrdersCheckBox";
            this.retainActiveGtdOrdersCheckBox.TabIndex = 2;
            this.retainActiveGtdOrdersCheckBox.Text = "Retain active GTD orders";
            this.retainActiveGtdOrdersCheckBox.UseVisualStyleBackColor = true;
            // 
            // resetGeneratedIdsCheckBox
            // 
            this.resetGeneratedIdsCheckBox.AutoSize = true;
            this.resetGeneratedIdsCheckBox.Checked = true;
            this.resetGeneratedIdsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.resetGeneratedIdsCheckBox.Name = "resetGeneratedIdsCheckBox";
            this.resetGeneratedIdsCheckBox.TabIndex = 4;
            this.resetGeneratedIdsCheckBox.Text = "Reset generated IDs (ClOrdID, ListID, AllocID etc)";
            this.resetGeneratedIdsCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionsPanel
            // 
            this.optionsPanel.AutoSize = true;
            this.optionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.optionsPanel.Controls.Add(this.retainActiveGtcOrdersCheckBox);
            this.optionsPanel.Controls.Add(this.retainActiveGtdOrdersCheckBox);
            this.optionsPanel.Controls.Add(this.resetGeneratedIdsCheckBox);
            this.optionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.optionsPanel.Name = "optionsPanel";
            this.optionsPanel.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.optionsPanel.TabIndex = 0;
            this.optionsPanel.WrapContents = false;
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBox1.Controls.Add(this.optionsPanel);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            // 
            // contentPanel
            // 
            this.contentPanel.AutoSize = true;
            this.contentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.contentPanel.BackColor = System.Drawing.SystemColors.Window;
            this.contentPanel.ColumnCount = 2;
            this.contentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.contentPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.contentPanel.Controls.Add(this.iconPictureBox, 0, 0);
            this.contentPanel.Controls.Add(this.label1, 1, 0);
            this.contentPanel.Controls.Add(this.groupBox1, 1, 1);
            this.contentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentPanel.Margin = new System.Windows.Forms.Padding(0);
            this.contentPanel.Name = "contentPanel";
            this.contentPanel.Padding = new System.Windows.Forms.Padding(24, 20, 16, 16);
            this.contentPanel.RowCount = 2;
            this.contentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.contentPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.contentPanel.TabIndex = 0;
            // 
            // yesButton
            // 
            this.yesButton.AutoSize = true;
            this.yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.yesButton.MinimumSize = new System.Drawing.Size(88, 27);
            this.yesButton.Name = "yesButton";
            this.yesButton.TabIndex = 0;
            this.yesButton.Text = "Yes";
            this.yesButton.UseVisualStyleBackColor = true;
            // 
            // noButton
            // 
            this.noButton.AutoSize = true;
            this.noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noButton.Margin = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.noButton.MinimumSize = new System.Drawing.Size(88, 27);
            this.noButton.Name = "noButton";
            this.noButton.TabIndex = 1;
            this.noButton.Text = "No";
            this.noButton.UseVisualStyleBackColor = true;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.buttonPanel.AutoSize = true;
            this.buttonPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.buttonPanel.Controls.Add(this.noButton);
            this.buttonPanel.Controls.Add(this.yesButton);
            this.buttonPanel.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.buttonPanel.Margin = new System.Windows.Forms.Padding(12);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.TabIndex = 1;
            this.buttonPanel.WrapContents = false;
            // 
            // rootPanel
            // 
            this.rootPanel.AutoSize = true;
            this.rootPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.rootPanel.ColumnCount = 1;
            this.rootPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.rootPanel.Controls.Add(this.contentPanel, 0, 0);
            this.rootPanel.Controls.Add(this.buttonPanel, 0, 1);
            this.rootPanel.Location = new System.Drawing.Point(0, 0);
            this.rootPanel.Margin = new System.Windows.Forms.Padding(0);
            this.rootPanel.Name = "rootPanel";
            this.rootPanel.RowCount = 2;
            this.rootPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.rootPanel.TabIndex = 0;
            // 
            // ResetForm
            // 
            this.AcceptButton = this.yesButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.CancelButton = this.noButton;
            this.ClientSize = new System.Drawing.Size(454, 220);
            this.Controls.Add(this.rootPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResetForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "FIX Client";
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox)).EndInit();
            this.optionsPanel.ResumeLayout(false);
            this.optionsPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.contentPanel.ResumeLayout(false);
            this.contentPanel.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.buttonPanel.PerformLayout();
            this.rootPanel.ResumeLayout(false);
            this.rootPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox iconPictureBox;
        private System.Windows.Forms.Button noButton;
        private System.Windows.Forms.Button yesButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox retainActiveGtdOrdersCheckBox;
        private System.Windows.Forms.CheckBox retainActiveGtcOrdersCheckBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox resetGeneratedIdsCheckBox;
        private System.Windows.Forms.FlowLayoutPanel optionsPanel;
        private System.Windows.Forms.TableLayoutPanel contentPanel;
        private System.Windows.Forms.FlowLayoutPanel buttonPanel;
        private System.Windows.Forms.TableLayoutPanel rootPanel;
    }
}