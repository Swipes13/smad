using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GuiExtension;

namespace smad5 {
  public partial class Form1 : Form {
    src.labs.Rgz rgz = new src.labs.Rgz();
    public Form1() {
      InitializeComponent();
    }
    private void doRgz() {
      if (!rgz.Initialized) return;
      rgz.GenerateOptimalModel();

    }
    private void doLab6() {
      src.labs.Lab6 lab6 = new src.labs.Lab6();
      try {
        lab6.GenerateData(this);
      }
      catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
    private void doLab5() {
      using (src.Writer.Open("output.txt")) {
        try {
          src.labs.Lab5 laba = new src.labs.Lab5();
          richTextBox1.Text = laba.GenerateData();
          for(int i=0; i<laba.RSSs.Count; i++){
            richTextBox2.Text += laba.RSSs[i].ToString() + Environment.NewLine;
            richTextBox3.Text += laba.Norms[i].ToString() + Environment.NewLine;
            richTextBox4.Text += laba.Lamdas[i].ToString() + Environment.NewLine;
          }
        }
        catch (Exception ex) {
          MessageBox.Show(ex.Message);
        }
      }
    }
    private void setDataToolStripMenuItem_Click(object sender, EventArgs e) {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Multiselect = false;
      if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
        if (ofd.FileName != "") {
          if (!rgz.SetData(ofd.FileName))
            MessageBox.Show("Чёт дате плохо");
          else
            doRgz();
        }
      }
    }
  }
}
