using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MazeLib;

namespace MazeLib
{
    public partial class LogExpInfo : Form
    {
        public MazePathItem mzPath;
        public LogExpInfo(MazePathItem mzP)
        {
            mzPath = mzP;
           
            InitializeComponent();
            labelLogDescrip.Text = "Filename: " + mzPath.logFileName + "\nWalker: " + mzPath.Walker + "\nDate: ";
            labelLogDescrip.Text+=mzPath.Date + "\nMaze: " + mzPath.Maze + "\nMaze List: " + mzPath.Mel;
            labelLogDescrip.Text +="\nList Index: " + mzPath.MelIndex;

            textBox_ExpGroup.Text = mzPath.ExpGroup;
            textBox_ExpCondition.Text = mzPath.ExpCondition;
            textBox_ExpSubject.Text = mzPath.ExpSubjectID;
            numericUpDown_ExpSession.Value = mzPath.ExpSession;
            numericUpDown_ExpTrial.Value = mzPath.ExpTrial;
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            mzPath.ExpGroup = textBox_ExpGroup.Text;
            mzPath.ExpCondition = textBox_ExpCondition.Text;
            mzPath.ExpSubjectID = textBox_ExpSubject.Text;
            mzPath.ExpSession = (int)numericUpDown_ExpSession.Value;
            mzPath.ExpTrial = (int)numericUpDown_ExpTrial.Value;
            this.Close();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox_ExpSubject_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void numericUpDown_ExpTrial_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
