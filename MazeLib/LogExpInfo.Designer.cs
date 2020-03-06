namespace MazeLib
{
    partial class LogExpInfo
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
            this.button_Ok = new System.Windows.Forms.Button();
            this.textBox_ExpSubject = new System.Windows.Forms.TextBox();
            this.textBox_ExpGroup = new System.Windows.Forms.TextBox();
            this.textBox_ExpCondition = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDown_ExpTrial = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown_ExpSession = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.labelLogDescrip = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ExpTrial)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ExpSession)).BeginInit();
            this.SuspendLayout();
            // 
            // button_Ok
            // 
            this.button_Ok.Location = new System.Drawing.Point(386, 185);
            this.button_Ok.Name = "button_Ok";
            this.button_Ok.Size = new System.Drawing.Size(75, 23);
            this.button_Ok.TabIndex = 5;
            this.button_Ok.Text = "Ok";
            this.button_Ok.UseVisualStyleBackColor = true;
            this.button_Ok.Click += new System.EventHandler(this.button_Ok_Click);
            // 
            // textBox_ExpSubject
            // 
            this.textBox_ExpSubject.Location = new System.Drawing.Point(82, 177);
            this.textBox_ExpSubject.Name = "textBox_ExpSubject";
            this.textBox_ExpSubject.Size = new System.Drawing.Size(100, 20);
            this.textBox_ExpSubject.TabIndex = 2;
            this.textBox_ExpSubject.Text = "1";
            this.textBox_ExpSubject.TextChanged += new System.EventHandler(this.textBox_ExpSubject_TextChanged);
            // 
            // textBox_ExpGroup
            // 
            this.textBox_ExpGroup.Location = new System.Drawing.Point(82, 123);
            this.textBox_ExpGroup.Name = "textBox_ExpGroup";
            this.textBox_ExpGroup.Size = new System.Drawing.Size(100, 20);
            this.textBox_ExpGroup.TabIndex = 0;
            // 
            // textBox_ExpCondition
            // 
            this.textBox_ExpCondition.Location = new System.Drawing.Point(82, 149);
            this.textBox_ExpCondition.Name = "textBox_ExpCondition";
            this.textBox_ExpCondition.Size = new System.Drawing.Size(100, 20);
            this.textBox_ExpCondition.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 126);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Group:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 152);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Condition:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Subject #:";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(298, 130);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = "Session #:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(301, 159);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Trial #:";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // numericUpDown_ExpTrial
            // 
            this.numericUpDown_ExpTrial.Location = new System.Drawing.Point(361, 157);
            this.numericUpDown_ExpTrial.Name = "numericUpDown_ExpTrial";
            this.numericUpDown_ExpTrial.Size = new System.Drawing.Size(100, 20);
            this.numericUpDown_ExpTrial.TabIndex = 4;
            this.numericUpDown_ExpTrial.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_ExpTrial.ValueChanged += new System.EventHandler(this.numericUpDown_ExpTrial_ValueChanged);
            // 
            // numericUpDown_ExpSession
            // 
            this.numericUpDown_ExpSession.Location = new System.Drawing.Point(361, 128);
            this.numericUpDown_ExpSession.Name = "numericUpDown_ExpSession";
            this.numericUpDown_ExpSession.Size = new System.Drawing.Size(100, 20);
            this.numericUpDown_ExpSession.TabIndex = 3;
            this.numericUpDown_ExpSession.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 9);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(331, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Please Enter the Experimental information for the Log FIle Information";
            // 
            // labelLogDescrip
            // 
            this.labelLogDescrip.AutoSize = true;
            this.labelLogDescrip.Location = new System.Drawing.Point(12, 32);
            this.labelLogDescrip.Name = "labelLogDescrip";
            this.labelLogDescrip.Size = new System.Drawing.Size(181, 13);
            this.labelLogDescrip.TabIndex = 14;
            this.labelLogDescrip.Text = "Please Enter the Log FIle Information";
            // 
            // LogExpInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(473, 215);
            this.Controls.Add(this.labelLogDescrip);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericUpDown_ExpSession);
            this.Controls.Add(this.numericUpDown_ExpTrial);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_ExpCondition);
            this.Controls.Add(this.textBox_ExpGroup);
            this.Controls.Add(this.textBox_ExpSubject);
            this.Controls.Add(this.button_Ok);
            this.Name = "LogExpInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Experimental Info";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ExpTrial)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_ExpSession)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Ok;
        private System.Windows.Forms.TextBox textBox_ExpSubject;
        private System.Windows.Forms.TextBox textBox_ExpGroup;
        private System.Windows.Forms.TextBox textBox_ExpCondition;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown_ExpTrial;
        private System.Windows.Forms.NumericUpDown numericUpDown_ExpSession;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label labelLogDescrip;
    }
}