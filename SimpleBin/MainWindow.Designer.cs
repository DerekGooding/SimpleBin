﻿namespace SimpleBin
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            TrayIcon = new NotifyIcon(components);
            TrayMenu = new ContextMenuStrip(components);
            ElementsToolStripItem = new ToolStripMenuItem();
            SizeToolStripItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            ClearToolStripItem = new ToolStripMenuItem();
            SettingsToolStripItem = new ToolStripMenuItem();
            ExitToolStripItem = new ToolStripMenuItem();
            tableLayoutPanel1 = new TableLayoutPanel();
            groupBox1 = new GroupBox();
            RemoveFromStartupBtn = new Button();
            AddToStartupBtn = new Button();
            TrayMenu.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // TrayIcon
            // 
            TrayIcon.ContextMenuStrip = TrayMenu;
            resources.ApplyResources(TrayIcon, "TrayIcon");
            TrayIcon.MouseClick += TrayIcon_MouseClick;
            // 
            // TrayMenu
            // 
            TrayMenu.ImageScalingSize = new Size(24, 24);
            TrayMenu.Items.AddRange(new ToolStripItem[] { ElementsToolStripItem, SizeToolStripItem, toolStripSeparator1, ClearToolStripItem, SettingsToolStripItem, ExitToolStripItem });
            TrayMenu.Name = "TrayMenu";
            TrayMenu.RenderMode = ToolStripRenderMode.System;
            resources.ApplyResources(TrayMenu, "TrayMenu");
            // 
            // ElementsToolStripItem
            // 
            resources.ApplyResources(ElementsToolStripItem, "ElementsToolStripItem");
            ElementsToolStripItem.Name = "ElementsToolStripItem";
            // 
            // SizeToolStripItem
            // 
            resources.ApplyResources(SizeToolStripItem, "SizeToolStripItem");
            SizeToolStripItem.Name = "SizeToolStripItem";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            resources.ApplyResources(toolStripSeparator1, "toolStripSeparator1");
            // 
            // ClearToolStripItem
            // 
            ClearToolStripItem.Name = "ClearToolStripItem";
            resources.ApplyResources(ClearToolStripItem, "ClearToolStripItem");
            ClearToolStripItem.Click += ClearToolStripItem_Click;
            // 
            // SettingsToolStripItem
            // 
            SettingsToolStripItem.Name = "SettingsToolStripItem";
            resources.ApplyResources(SettingsToolStripItem, "SettingsToolStripItem");
            SettingsToolStripItem.Click += SettingsToolStripItem_Click;
            // 
            // ExitToolStripItem
            // 
            ExitToolStripItem.Name = "ExitToolStripItem";
            resources.ApplyResources(ExitToolStripItem, "ExitToolStripItem");
            ExitToolStripItem.Click += ExitToolStripItem_Click;
            // 
            // tableLayoutPanel1
            // 
            resources.ApplyResources(tableLayoutPanel1, "tableLayoutPanel1");
            tableLayoutPanel1.Controls.Add(groupBox1, 0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(RemoveFromStartupBtn);
            groupBox1.Controls.Add(AddToStartupBtn);
            resources.ApplyResources(groupBox1, "groupBox1");
            groupBox1.Name = "groupBox1";
            groupBox1.TabStop = false;
            // 
            // RemoveFromStartupBtn
            // 
            resources.ApplyResources(RemoveFromStartupBtn, "RemoveFromStartupBtn");
            RemoveFromStartupBtn.Name = "RemoveFromStartupBtn";
            RemoveFromStartupBtn.UseVisualStyleBackColor = true;
            RemoveFromStartupBtn.Click += RemoveFromStartupBtn_Click;
            // 
            // AddToStartupBtn
            // 
            resources.ApplyResources(AddToStartupBtn, "AddToStartupBtn");
            AddToStartupBtn.Name = "AddToStartupBtn";
            AddToStartupBtn.UseVisualStyleBackColor = true;
            AddToStartupBtn.Click += AddToStartupBtn_Click;
            // 
            // MainWindow
            // 
            resources.ApplyResources(this, "$this");
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(tableLayoutPanel1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "MainWindow";
            FormClosing += Form1_FormClosing;
            TrayMenu.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.NotifyIcon TrayIcon;
        private ContextMenuStrip TrayMenu;
        private ToolStripMenuItem ElementsToolStripItem;
        private ToolStripMenuItem SizeToolStripItem;
        private ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ClearToolStripItem;
        private System.Windows.Forms.ToolStripMenuItem SettingsToolStripItem;
        private ToolStripMenuItem ExitToolStripItem;
        private TableLayoutPanel tableLayoutPanel1;
        private GroupBox groupBox1;
        private Button RemoveFromStartupBtn;
        private Button AddToStartupBtn;
    }
}
