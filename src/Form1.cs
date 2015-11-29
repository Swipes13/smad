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
    public Form1() {
      InitializeComponent();
      doLab();
    }

    private void doLab() {
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
  }
}
