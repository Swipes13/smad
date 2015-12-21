using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using smad5.src.math;
using smad5.src.data;
using Point = smad5.src.data.Point;

namespace smad5.src.labs {
  public class Lab6 {
    public struct TettaRSS {
      public static TettaRSS Compute(Matrix X, Matrix y) {
        TettaRSS ret = new TettaRSS();
        ret.Tetta = Matrix.MNK(X, y);
        ret.Rss = Matrix.RSS(X, y, ret.Tetta);
        ret.Y = X * ret.Tetta;
        return ret;
      }
      public Matrix Tetta;
      public Matrix Y;
      public double Rss;
    }

    List<Point> data = new List<Point>();
    int countMax = 7;

    protected double[] calcF(double[] x, bool[] need) {
      List<double> res = new List<double>();
      var f = func(x);
      for (int i = 0; i < need.Count(); i++)
        if (need[i]) res.Add(f[i]);

      return res.ToArray();
    }
    private double[] func(double[] x) {
      return new double[] { 1.0, x[0], x[1], x[2], x[0] * x[1], x[0] * x[2], x[1] * x[2] };//, x[0] * x[0], x[1] * x[1], x[2] * x[2] };
    }

    private Matrix genX(int n, int m, bool[] need) {
      Matrix ret = new Matrix(n, m);
      for (int j = 0; j < n; j++) {
        var point = new double[] { data[j].X[0], data[j].X[1], data[j].X[2] };
        double[] f = calcF(point, need);
        ret[j] = new Matrix(m, f);
      }

      return ret;
    }
    public void GenerateData(Form1 form) {
      /*
      double[] X1 = new double[]{80, 80, 75, 62, 62, 62, 62, 62, 58, 58, 58, 58, 58, 58, 50, 50, 50, 50, 50, 56, 70};
      double[] X2 = new double[]{27, 27, 25, 24, 22, 23, 24, 24, 23, 18, 18, 17, 18, 19, 18, 18, 19, 19, 20, 20, 20};
      double[] X3 = new double[]{89, 88, 90, 87, 87, 87, 93, 93, 87, 80, 89, 88, 82, 93, 89, 86, 72, 79, 80, 82, 91};
      double[] y = new double[]{42, 37, 37, 28, 18, 18, 19, 20, 15, 14, 14, 13, 11, 12, 8, 7, 8, 8, 9, 15, 15};

      List<Point> data2 = new List<Point>();
      for (int i = 0; i < 21; i++) data.Add(new Point(new double[]{X1[i], X2[i], X3[i]}, y[i]));
      for (int i = 0; i < 21; i++) data2.Add(new Point(new double[]{X1[i] * X1[i], X2[i]*X2[i], X3[i]*X3[i]}, y[i]));
      data2 = data2.OrderBy(x => x.X[2]).ToList();

      double minX = data2.First().X[2] - 1.0;
      double maxX = data2.Last().X[2] + 1.0;
      double minY = 7.0 - 1.0;
      double maxY = 42.0 + 1.0;
      double distX = maxX - minX;
      double distY = maxY - minY;
      Bitmap bmp = new Bitmap(form.pictureBox1.Size.Width,form.pictureBox1.Size.Height);
      Graphics gr = Graphics.FromImage(bmp);
      gr.FillRectangle(Brushes.White, 0, 0, form.pictureBox1.Size.Width, form.pictureBox1.Size.Height);

      for (int i = 0; i < 21; i++) {
        var xPixel = (data2[i].X[2] - minX) / distX;
        var yPixel = (data2[i].Y - minY) / distY;
        yPixel = 1.0 - yPixel;

        var pixX = ((double)form.pictureBox1.Size.Width) * xPixel;
        var pixY = ((double)form.pictureBox1.Size.Height) * yPixel;

        gr.FillRectangle(Brushes.Blue,((int)pixX)-1,((int)pixY)-1,3,3);
      }

      form.pictureBox1.Image = bmp;


      Matrix Y = new Matrix(1, y.ToArray());

      var n = data.Count;
      var m = 0;

      double avgY = 0.0, r2 = 0.0;
      for (int i = 0; i < n; i++) avgY += y[i];
      avgY /= n;
      for (int i = 0; i < n; i++) {
        var t = y[i] - avgY;
        r2 += t * t;
      }
      List<bool> allModel = new List<bool>();
      for (int i = 0; i < countMax; i++) allModel.Add(true);

      var needAddAll = allModel.ToArray();
      foreach (bool b in needAddAll) if (b) m++;

      var xxx = genX(n, m, needAddAll);

      var tetta123 = Matrix.MNK(xxx, Y);
      var rss1234 = Matrix.RSS(xxx, Y, tetta123);


      TettaRSS mainTettaRss = TettaRSS.Compute(xxx, Y);
      var sigma2 = mainTettaRss.Rss / (n - m);

      m = 0;
      for (int i = 1; i < countMax; i++) allModel[i] = false;
      var needToAdd = allModel.ToArray();
      foreach(bool b in needToAdd) if(b) m++;

      List<double> Mellous = new List<double>();
      List<double> ManyKrit = new List<double>();
      List<double> MSEP = new List<double>();
      List<double> AEV = new List<double>();
      List<bool[]> functions = new List<bool[]>();
      List<double> diffsY = new List<double>();
      form.richTextBox5.Text += "Mellous" + Environment.NewLine;
      form.richTextBox2.Text += "MSEP" + Environment.NewLine;
      form.richTextBox3.Text += "AEV" + Environment.NewLine;
      form.richTextBox4.Text += "ManyKrit" + Environment.NewLine;

      for (int i = 0; i < countMax - 1; i++) {
        TettaRSS tettaRssBefore = TettaRSS.Compute(genX(n, m, needToAdd), Y);
        Dictionary<int, double> FCrit = new Dictionary<int, double>();

        var nu1 = 1;
        var nu2 = n - m - 1;

        int index = 0;
        for (int q = m; q < countMax; q++) {
          var newNeedToAdd = new bool[countMax];
          for (int w = 0; w < countMax; w++) newNeedToAdd[w] = needToAdd[w];
          while (newNeedToAdd[index]) index++;
          newNeedToAdd[index] = true;
          var newM = m + 1;

          TettaRSS newTettaRss = TettaRSS.Compute(genX(n, newM, newNeedToAdd), Y);

          var fCrit = (nu2 / nu1) * (tettaRssBefore.Rss - newTettaRss.Rss) / newTettaRss.Rss;
          FCrit.Add(index, fCrit);
          index++;
        }
        var sorted = FCrit.OrderByDescending(x => x.Value).ToList();

        m++;
        needToAdd[sorted[0].Key] = true;
        functions.Add(needToAdd.ToArray());

        foreach (var b in functions.Last())
          form.richTextBox6.Text += b.ToString() + " ";
        form.richTextBox6.Text += Environment.NewLine;

        foreach (var fc in FCrit) {
          form.richTextBox6.Text += fc.Value.ToString() + Environment.NewLine;
        }
        form.richTextBox6.Text += Environment.NewLine;


        var newTRss = TettaRSS.Compute(genX(n, m, needToAdd), Y);
        var mel = newTRss.Rss / sigma2 + 2.0 * m - n;
        Mellous.Add(mel);
        //form.richTextBox5.Text += Mellous.Last().ToString() + Environment.NewLine;
        form.richTextBox5.Text += newTRss.Rss.ToString() + Environment.NewLine;
        
        double newAvgY = 0.0, newR2 = 0.0;
        for (int a = 0; a < n; a++) newAvgY += newTRss.Y[a][0, 0];
        newAvgY /= n;
        for (int a = 0; a < n; a++) {
          var t = newTRss.Y[a][0, 0] - newAvgY;
          newR2 += t * t;
        }

        ManyKrit.Add(newR2 / r2);
        form.richTextBox4.Text += ManyKrit.Last().ToString() + Environment.NewLine;

        var msep = newTRss.Rss / (n * (n - m)) * (1 + n + m * (n + 1) / (n - m - 2));
        MSEP.Add(msep);
        form.richTextBox2.Text += MSEP.Last().ToString() + Environment.NewLine;

        var aev = newTRss.Rss * m / (n * (n - m));
        AEV.Add(aev);
        form.richTextBox3.Text += AEV.Last().ToString() + Environment.NewLine;

        var fullSum = 0.0;
        for (int d = 0; d < n; d++) {
          var dif = Math.Abs(y[d] - newTRss.Y[d][0, 0]);
          fullSum += dif;
        }
        diffsY.Add(fullSum);
      }
      var goodTR = TettaRSS.Compute(genX(n, 2, functions[0]), Y);
      for(int i=0; i<2; i++)
        form.richTextBox1.Text += goodTR.Tetta[i][0, 0].ToString() + Environment.NewLine;
      
      form.richTextBox1.Text += "y\tyest\tdif" + Environment.NewLine;
      for (int i = 0; i < n; i++) {
        var dif = Math.Abs(y[i] - goodTR.Y[i][0, 0]);
        form.richTextBox1.Text += y[i].ToString() + "\t" + Math.Round(goodTR.Y[i][0, 0], 3).ToString() + "\t" + Math.Round(dif, 3).ToString() + Environment.NewLine;
      }

      form.richTextBox1.Text += Environment.NewLine;
      for(int i=0; i<diffsY.Count; i++)
        form.richTextBox1.Text += diffsY[i].ToString() + Environment.NewLine;

    }
      */
    }
  }
}
