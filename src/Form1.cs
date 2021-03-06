﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GuiExtension;
using OptimalModelAlgorithm = smad5.src.labs.Rgz.OptimalModelAlgorithm;
using Rgz = smad5.src.labs.Rgz;

namespace smad5 {
  public partial class Form1 : Form {
    Rgz rgz = new Rgz();
    String _ns = Environment.NewLine;
    String _tb = "\t";

    public Form1() {
      InitializeComponent();
      var ns = Environment.NewLine;
    }
    private bool doRgz(OptimalModelAlgorithm alg) {
      if (!rgz.Initialized) {
        rtbLog.Text += _ns + "RGZ not inited! Check data!" + _ns;
        return false;
      }
      rgz.GenerateOptimalModel(alg);
      rtbLog.Text += alg.ToString() + Environment.NewLine;
      fillLog();
      return true;
    }
    private void fillRss(RichTextBox rtb) {
      rtb.Text = "";
      for (int i = 0; i < rgz.ForOptMdl.TettaRss.Count; i++)
        rtb.Text += rgz.ForOptMdl.TettaRss[i].Rss.ToString() + _ns;
      rtb.Text += _ns;
    }
    private void fillKrit(RichTextBox rtb,List<double> krit) {
      rtb.Text = "";
      for (int i = 0; i < krit.Count; i++) rtb.Text += krit[i].ToString() + _ns;
      rtb.Text += _ns;
    }
    private void fillLog() {
      for (int i = 0; i < rgz.ForOptMdl.Mellous.Count; i++) {
        for (int j = 0; j < rgz.ForOptMdl.Functions[i].Count(); j++)
          rtbLog.Text += rgz.ForOptMdl.LabelsX[j] + _tb;
        rtbLog.Text += _ns;

        for (int j = 0; j < rgz.ForOptMdl.Functions[i].Count(); j++)
          rtbLog.Text += rgz.ForOptMdl.Functions[i][j].ToString() + _tb;
        
        rtbLog.Text += _ns;

        rtbLog.Text += "RSS" + _tb;
        rtbLog.Text += rgz.ForOptMdl.TettaRss[i].Rss.ToString() + _ns;
        rtbLog.Text += "mellous" + _tb;
        rtbLog.Text += rgz.ForOptMdl.Mellous[i].ToString() + _ns;
        rtbLog.Text += "manyCrit" + _tb;
        rtbLog.Text += rgz.ForOptMdl.ManyKrit[i].ToString() + _ns;
        rtbLog.Text += "MSEP" + _tb;
        rtbLog.Text += rgz.ForOptMdl.MSEP[i].ToString() + _ns;
        rtbLog.Text += "AEV" + _tb;
        rtbLog.Text += rgz.ForOptMdl.AEV[i].ToString() + _ns;
        rtbLog.Text += _ns;

        rtbLog.SelectionStart = rtbLog.Text.Length;
        rtbLog.ScrollToCaret();
      }
      rtbLog.Text += _ns;
    }
    private void doLab6() {
      src.labs.Lab6 lab6 = new src.labs.Lab6();
      try {
        lab6.GenerateData(this);
      }
      catch (Exception ex) { MessageBox.Show(ex.Message); }
    }
    private void fillDataDGV(DataGridView dgv) {
      dgv.Rows.Clear();
      dgv.Columns.Clear();
      int rowLen = rgz.Data.First().X.Count()+1;
      dgv.Columns.Add("Num", "Num");
      for (int i = 0; i < rowLen - 1; i++)
        dgv.Columns.Add("X" + (i + 1).ToString(), "X" + (i + 1).ToString());
      dgv.Columns.Add("Y", "Y");

      for (int i = 0; i < rgz.Data.Count; i++) {
        String[] row = new String[rowLen+1];
        row[0] = (i+1).ToString();
        for (int j = 0; j < rgz.Data[i].X.Count(); j++)
          row[1+j] = Math.Round(rgz.Data[i].X[j], 2).ToString();
        row[row.Count() - 1] = Math.Round(rgz.Data[i].Y, 2).ToString();
        dgv.Rows.Add(row);
      }
    }
    private void fillAllRtbs() {
      if (!rgz.Initialized) return;
      fillRss(rtbRSS);
      fillKrit(rtbMellous, rgz.ForOptMdl.Mellous);
      writeFunc(rtbMellous, rgz.ForOptMdl.BestFMell);
      fillKrit(rtbManyKrit, rgz.ForOptMdl.ManyKrit);
      writeFunc(rtbManyKrit, rgz.ForOptMdl.BestFR);
      fillKrit(rtbMsep, rgz.ForOptMdl.MSEP);
      writeFunc(rtbMsep, rgz.ForOptMdl.BestFMSEP);
      fillKrit(rtbAev, rgz.ForOptMdl.AEV);
      writeFunc(rtbAev, rgz.ForOptMdl.BestFAEV);
      fillKrit(rtbE, rgz.ForOptMdl.E);
      writeFunc(rtbE, rgz.ForOptMdl.BestFE);
    }
    private void writeFunc(RichTextBox rtb, List<bool> list) {
      for (int i = 0; i < list.Count; i++) {
        if (list[i]) {
          rtb.Text += rgz.ForOptMdl.LabelsX[i] + " + ";
        }
      }
      rtb.Text = rtb.Text.Substring(0, rtb.Text.Length - 3);
    }
    private void setDataToolStripMenuItem_Click(object sender, EventArgs e) {
      OpenFileDialog ofd = new OpenFileDialog();
      ofd.Multiselect = false;
      if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
        if (ofd.FileName != "") {
          if (!rgz.SetData(ofd.FileName)) {
            MessageBox.Show("Чёт дате плохо");
            rtbLog.Text += "Error data file" + _ns;
          }
          else {
            fillDataDGV(dataGridView1);
            rtbLog.Text += "Data file " + ofd.FileName + " extract [OK]" + _ns;

            drawCorrelationFields(flpFields);
            setFuncByFields();
            fillModelLabel();
          }
        }
      }
    }
    private void logToolStripMenuItem_Click(object sender, EventArgs e) {
      logToolStripMenuItem.Checked = !logToolStripMenuItem.Checked;
      spcntMain.Panel2Collapsed = !logToolStripMenuItem.Checked;
    }
    private void doRgzFill(OptimalModelAlgorithm alg) {
      if(!doRgz(alg)) return;
      fillAllRtbs();
    }
    private void drawCorrelationFields(FlowLayoutPanel flp) {
      flp.Controls.Clear();
      int w = 230; int h = 200; int lSize = 18;
      Size s = new Size(w, h);
      for (int i = 1; i < rgz.ForOptMdl.LabelsX.Count; i++) {
        Panel pnl = new Panel();
        pnl.Size = s;
        pnl.Padding = new System.Windows.Forms.Padding(2);
        pnl.MouseDown += pnl_MouseDown;
        pnl.Name = (i - 1).ToString();
        pnl.Tag = "non";

        PictureBox pbField = new PictureBox();
        pbField.Dock = DockStyle.Fill;
        pbField.Size = new System.Drawing.Size(w - lSize, h - lSize);
        pbField.Name = (i - 1).ToString();
        pbField.MouseDown += pbField_MouseDown;

        rgz.DrawCorellationField(pbField, i);

        pnl.Controls.Add(pbField);

        Label lbl = new Label();
        lbl.Text = rgz.ForOptMdl.LabelsX[i];
        lbl.AutoSize = false;
        lbl.Dock = DockStyle.Bottom;
        lbl.BackColor = Color.RoyalBlue;
        lbl.ForeColor = Color.White;
        lbl.Size = new System.Drawing.Size(w, lSize);
        lbl.TextAlign = ContentAlignment.MiddleCenter;
        lbl.Name = (i - 1).ToString();
        lbl.MouseDown += lbl_MouseDown;

        Label lbl2 = new Label();
        lbl2.Text = "Y";
        lbl2.AutoSize = false;
        lbl2.Dock = DockStyle.Left;
        lbl2.BackColor = Color.White;
        lbl2.ForeColor = Color.Red;
        lbl2.Size = new System.Drawing.Size(lSize, h);
        lbl2.TextAlign = ContentAlignment.MiddleCenter;
        lbl2.Name = (i - 1).ToString();
        lbl2.MouseDown += lbl_MouseDown;

        pnl.Controls.Add(lbl);
        pnl.Controls.Add(lbl2);


        flp.Controls.Add(pnl);
      }

      rtbLog.Text += "Correlation fields draw [OK]" + _ns;
    }
    void lbl_MouseDown(object sender, MouseEventArgs e) {
      int ind = Convert.ToInt32(((Label)sender).Name);
      computePanel(ind);
    }
    void pbField_MouseDown(object sender, MouseEventArgs e) {
      int ind = Convert.ToInt32(((PictureBox)sender).Name);
      computePanel(ind);
    }
    void pnl_MouseDown(object sender, MouseEventArgs e) {
      int ind = Convert.ToInt32(((Panel)sender).Name);
      computePanel(ind);
    }
    private void computePanel(int ind) {
      if (flpFields.Controls[ind].Tag.ToString() == "non") {
        flpFields.Controls[ind].Tag = "sel";
        flpFields.Controls[ind].BackColor = Color.HotPink;
      }
      else {
        flpFields.Controls[ind].Tag = "non";
        flpFields.Controls[ind].BackColor = Color.Gainsboro;
      }
      setFuncByFields();
      fillModelLabel();
    }
    private void btnCalc_Click(object sender, EventArgs e) {
      if (cmbOptimalModel.SelectedIndex == -1) {
        MessageBox.Show("Choose Optimal model algorithm!");
        return;
      }
      if (!rgz.Initialized) {
        MessageBox.Show("Check data!");
        return;
      }

      setFuncByFields();

      if (cmbOptimalModel.SelectedIndex == 0) doRgzFill(OptimalModelAlgorithm.Insertion);
      else doRgzFill(OptimalModelAlgorithm.Exception);
      rtbLog.SelectionStart = rtbLog.Text.Length;
      rtbLog.ScrollToCaret();
      fillFKrit();

      computeRgzTests(rgz.TrueFunc);
      drawE(rgz.TrueFunc);

      if (chbExmnAllOptFuncs.Checked)
        computeOptimalModels();
    }
    private void fillMultiCollinear(List<bool> func) {
      rtbMC1.Text += funcToString(func) + _ns + rgz.ForMultCol.DetXTXTrace.ToString() + _ns + _ns;
      rtbMC2.Text += funcToString(func) + _ns + rgz.ForMultCol.MinLambda.ToString() + _ns + _ns;
      rtbMC3.Text += funcToString(func) + _ns + rgz.ForMultCol.NeimanGoldstein.ToString() + _ns + _ns;
      rtbMC4.Text += funcToString(func) + _ns + rgz.ForMultCol.MaxPairConjugation.ToString() + _ns + _ns;
      rtbMC5.Text += funcToString(func) + _ns + rgz.ForMultCol.MaxConjugation.ToString() + _ns + _ns;
    }
    private void fillHeteroskedastic(List<bool> func) {
      //rtbHeter.Text = "";
      rtbHeter.Text += funcToString(func) + _ns;
      rtbHeter.Text += "alpha = " + rgz.ForGeter.alpha.ToString() + _ns;
      rtbHeter.Text += "ESS_2 = " + rgz.ForGeter.ESS.ToString() + _ns;
      rtbHeter.Text += "ESSChi = " + rgz.ForGeter.ESSChi.ToString() + _ns;
      rtbHeter.Text += "FDistr = " + rgz.ForGeter.FDistr.ToString() + _ns;
      rtbHeter.Text += "RSS2/RSS1 = " + rgz.ForGeter.Rss2Rss1.ToString() + _ns;

      if (rgz.ForGeter.ESS < rgz.ForGeter.ESSChi)
        rtbHeter.Text += _ns + "H of homo Accept";
      else rtbHeter.Text += _ns + "H of homo Not accept";
      
      if (rgz.ForGeter.Rss2Rss1 < rgz.ForGeter.FDistr)
        rtbHeter.Text += _ns + "H of homo Not rejected";
      else rtbHeter.Text += _ns + "H of homo Rejected";
      rtbHeter.Text += _ns + _ns;

    }
    private void fillAutoCorellation(List<bool> func) {
      //rtbAutoCor.Text = "";
      rtbAutoCor.Text += funcToString(func) + _ns;
      rtbAutoCor.Text += "DW = " + rgz.ForAutoCor.DW.ToString() + _ns;
      rtbAutoCor.Text += "dU_0.05 = " + rgz.ForAutoCor.dUa.ToString() + _ns;
      rtbAutoCor.Text += "dL_0.05 = " + rgz.ForAutoCor.dLa.ToString() + _ns;
      rtbAutoCor.Text += _ns;
      if (rgz.ForAutoCor.DW >= 4 - rgz.ForAutoCor.dLa) {
        rtbAutoCor.Text += "H of autocorrelation Rejected." + _ns;
        rtbAutoCor.Text += "Negative autocorrelation." + _ns + _ns;
        return;
      }
      if (rgz.ForAutoCor.DW >= 4 - rgz.ForAutoCor.dUa) {
        rtbAutoCor.Text += "Undefined autocorrelation." + _ns + _ns;
        return;
      }
      if (rgz.ForAutoCor.DW >= rgz.ForAutoCor.dUa) {
        rtbAutoCor.Text += "H of autocorrelation Not Rejected." + _ns + _ns;
        return;
      }
      if (rgz.ForAutoCor.DW >= 0) {
        rtbAutoCor.Text += "H of autocorrelation Rejected." + _ns;
        rtbAutoCor.Text += "Positive autocorrelation." + _ns + _ns;
        return;
      }
    }
    private void fillFKrit() {
      rtbFKrit.Text = "";

      for (int i = 0; i < rgz.ForOptMdl.Mellous.Count; i++) {
        for (int j = 0; j < rgz.ForOptMdl.Functions[i].Count(); j++)
          rtbFKrit.Text += rgz.ForOptMdl.LabelsX[j] + _tb;
        rtbFKrit.Text += _ns;

        for (int j = 0; j < rgz.ForOptMdl.Functions[i].Count(); j++)
          rtbFKrit.Text += rgz.ForOptMdl.Functions[i][j].ToString() + _tb;
        rtbFKrit.Text += _ns;

        if (i < rgz.ForOptMdl.Mellous.Count - 1) {
          rtbFKrit.Text += "Fkrit:" + _ns;
          foreach (var d in rgz.ForOptMdl.AllFCrits[i]) 
            rtbFKrit.Text += (d.Key).ToString() + _tb + (d.Value).ToString() + _ns;
        }

        rtbFKrit.Text += "RSS" + _tb;
        rtbFKrit.Text += rgz.ForOptMdl.TettaRss[i].Rss.ToString() + _ns;
        rtbFKrit.Text += "mellous" + _tb;
        rtbFKrit.Text += rgz.ForOptMdl.Mellous[i].ToString() + _ns;
        rtbFKrit.Text += "manyCrit" + _tb;
        rtbFKrit.Text += rgz.ForOptMdl.ManyKrit[i].ToString() + _ns;
        rtbFKrit.Text += "MSEP" + _tb;
        rtbFKrit.Text += rgz.ForOptMdl.MSEP[i].ToString() + _ns;
        rtbFKrit.Text += "AEV" + _tb;
        rtbFKrit.Text += rgz.ForOptMdl.AEV[i].ToString() + _ns;
        rtbFKrit.Text += _ns;

      }
    }
    private void setFuncByFields() {
      rgz.TrueFunc.Clear();
      rgz.TrueFunc.Add(true);

      for (int i = 0; i < flpFields.Controls.Count; i++) {
        if (flpFields.Controls[i].Tag.ToString() == "sel")
          rgz.TrueFunc.Add(true);
        else
          rgz.TrueFunc.Add(false);
      }
    }
    private void fillAdequacy(List<bool> func) {
      rtbAdeq.Text += funcToString(func) + _ns;
      //rtbAdeq.Text = "";
      rtbAdeq.Text += "F = " + rgz.ForAdequat.SigmaDiv.ToString() + _ns;
      rtbAdeq.Text += "F_0.05 = " + rgz.ForAdequat.FDist.ToString() + _ns;

      if (rgz.ForAdequat.SigmaDiv < rgz.ForAdequat.FDist) {
        rtbAdeq.Text += "Adequat model." + _ns + _ns;
        return;
      }
      rtbAdeq.Text += "Non adequat model." + _ns + _ns;
    }
    private void fillLastWork(List<bool> func) {
      rtbCInter.Text += funcToString(func) + _ns;
      rtbCInter.Text += "minB = " + rgz.ForAdequat.MinInt.ToString() + _ns;
      rtbCInter.Text += "yMax = " + rgz.ForAdequat.Ymax.ToString() + _ns;
      rtbCInter.Text += "maxB = " + rgz.ForAdequat.MaxInt.ToString() + _ns;
      rtbCInter.Text += _ns + _ns;
    }
    private void drawE(List<bool> func) {
      flpRssPbxs.Controls.Clear();
      int w = 230; int h = 200; int lSize = 18;
      Size s = new Size(w, h);

      for (int i = 0; i < func.Count; i++) {
        if (!func[i]) continue;

        Panel pnl = new Panel();
        pnl.Size = s;

        PictureBox pbField = new PictureBox();
        pbField.Dock = DockStyle.Fill;
        pbField.Size = new System.Drawing.Size(w - lSize, h - lSize);

        rgz.DrawE(pbField, i);

        pnl.Controls.Add(pbField);

        Label lbl = new Label();
        lbl.Text = rgz.ForOptMdl.LabelsX[i];
        lbl.AutoSize = false;
        lbl.Dock = DockStyle.Bottom;
        lbl.BackColor = Color.RoyalBlue;
        lbl.ForeColor = Color.White;
        lbl.Size = new System.Drawing.Size(w, lSize);
        lbl.TextAlign = ContentAlignment.MiddleCenter;

        Label lbl2 = new Label();
        lbl2.Text = "E";
        lbl2.AutoSize = false;
        lbl2.Dock = DockStyle.Left;
        lbl2.BackColor = Color.White;
        lbl2.ForeColor = Color.Red;
        lbl2.Size = new System.Drawing.Size(lSize, h);
        lbl2.TextAlign = ContentAlignment.MiddleCenter;

        pnl.Controls.Add(lbl);
        pnl.Controls.Add(lbl2);

        flpRssPbxs.Controls.Add(pnl);
      }
    }
    private void fillModelLabel() {
      lblModel.Text = funcToString(rgz.TrueFunc);
    }
    private String funcToString(List<bool> func) {
      var str = "";
      if (!rgz.Initialized) return str;

      for (int i = 0; i < func.Count; i++)
        if (func[i])
          str += rgz.ForOptMdl.LabelsX[i] + " + ";

      if (str == "") return str;
      str = str.Substring(0, str.Length - 3);
      return str;
    }
    private void computeOptimalModels() {
      if (!rgz.Initialized) return;

      List<List<bool>> optModels = new List<List<bool>>();
      optModels.Add(rgz.TrueFunc);
      addFuncToList(ref optModels, rgz.ForOptMdl.BestFMell);
      addFuncToList(ref optModels, rgz.ForOptMdl.BestFR);
      addFuncToList(ref optModels, rgz.ForOptMdl.BestFAEV);
      addFuncToList(ref optModels, rgz.ForOptMdl.BestFMSEP);
      addFuncToList(ref optModels, rgz.ForOptMdl.BestFE);

      for (int i = 1; i < optModels.Count; i++) {
        computeRgzTests(optModels[i]);
      }
    }
    private void addFuncToList(ref List<List<bool>> list, List<bool> func) {
      for (int o = 0; o < list.Count; o++) {
        bool findNEq = false;
        for (int i = 0; i < func.Count; i++) {
          if (list[o][i] != func[i]) { findNEq = true; break; }
        }
        if (!findNEq) return;
      }
      list.Add(func);
    }
    private void computeRgzTests(List<bool> func) {
      rgz.TrueFunc = func;

      rgz.CheckMultiColliniar();
      fillMultiCollinear(func);

      rgz.CheckGeteroskedastic();
      fillHeteroskedastic(func);

      rgz.CheckAutoCorellation();
      fillAutoCorellation(func);

      rgz.CheckAdequacy();
      fillAdequacy(func);

      rgz.GetMaxY();
      fillLastWork(func);
    }
  }
}
