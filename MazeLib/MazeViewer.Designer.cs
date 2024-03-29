﻿namespace MazeLib
{
    partial class MazeViewer
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // MazeViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.AliceBlue;
            this.Name = "MazeViewer";
            this.Size = new System.Drawing.Size(358, 322);
            this.VisibleChanged += new System.EventHandler(this.MazeViewer_VisibleChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MazeViewer_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MazeViewer_MouseClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MazeViewer_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MazeViewer_MouseMove);
            
            this.Resize += new System.EventHandler(this.MazeViewer_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
    }
}
