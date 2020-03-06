using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace MazeLib
{
    public partial class MeasurementRegionManager : Form
    {
        MeasurementRegionCollection curList = null;
        public MeasurementRegionManager(ref MeasurementRegionCollection inp)
        {
            InitializeComponent();
            curList = inp;
        }

        private void MeasurementRegionManager_Load(object sender, EventArgs e)
        {
            UpdateLeftPaneList();
        }

        private void UpdateLeftPaneList()
        {
            listBox1.Items.Clear();
            foreach (MeasurementRegion m in curList.Regions)
            {
                listBox1.Items.Add(m.Name);
            }

        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                foreach (MeasurementRegion m in curList.Regions)
                {
                    if (m.Name.CompareTo(listBox1.Items[listBox1.SelectedIndex]) == 0)
                    {
                        listBox1.Items.Remove(m.Name);
                        curList.Regions.Remove(m);
                        propertyGrid1.SelectedObject = null;
                        return;                        
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a region from the list!");
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > -1)
            {
                foreach (MeasurementRegion m in curList.Regions)
                {
                    if (m.Name.CompareTo(listBox1.Items[listBox1.SelectedIndex]) == 0)
                    {
                        propertyGrid1.SelectedObject = m;                        
                        return;
                    }
                }

            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            int i = listBox1.SelectedIndex;
            UpdateLeftPaneList();
            listBox1.SelectedIndex = i;
        }

        private void toolStripButtonOpen_Click(object sender, EventArgs e)
        {
            //open file..
            OpenFileDialog a = new OpenFileDialog();
            a.Filter = "Maze Region Definition Files (*.mzReg) |*.mzReg| All files (*.*) |*.*";
            a.FilterIndex = 1;
            a.RestoreDirectory = true;
            if (a.ShowDialog() == DialogResult.OK)
            {
                curList.ReadFromFile(a.FileName);
            }
            UpdateLeftPaneList();
        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            //save to file..
            saveRegionDefinitionFile();
        }

        public string saveRegionDefinitionFile(string messageText="Save Region Definition File")
        {
            SaveFileDialog a = new SaveFileDialog();
            a.Title = messageText;
            a.Filter = "Maze Region Definition Files | *.mzReg";
            a.FilterIndex = 1;
            a.DefaultExt = ".mzReg";
            a.RestoreDirectory = true;

            if (a.ShowDialog() == DialogResult.OK)
            {
                curList.WriteToFile(a.FileName);
                return a.FileName;
            }
            else
                return "";
        }

        private void MeasurementRegionManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (MeasurementRegion m in curList.Regions)
            {
                m.ConvertFromMazeToScreen();
            }
        }

        private void toolStripContainer1_TopToolStripPanel_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
