using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smad5.src.math;
using smad5.src.data;
using Point = smad5.src.data.Point;
using TettaRSS = smad5.src.labs.Lab6.TettaRSS;

namespace smad5.src.labs {

  public class Rgz {
    enum Field { Unit, Linear, LinearCollision, Quad }
    Field[] fields;

    delegate double[] fullFunction(double[] point);
    fullFunction func;
    protected double[] calcF(double[] x, bool[] need) {
      List<double> res = new List<double>();
      var f = func(x);
      for (int i = 0; i < need.Count(); i++)
        if (need[i]) res.Add(f[i]);

      return res.ToArray();
    }

    List<Point> data = new List<Point>();
    public bool Initialized { get { return initialized; } private set { } }
    bool initialized = false;
    private List<bool> trueFunc = new List<bool>();
   
    public void SetLinearTrueFunc() {
      trueFunc.Clear();
      for (int i = 0; i < fields.Count(); i++) {
        if (fields[i] == Field.Unit || fields[i] == Field.Linear)
          trueFunc.Add(true);
        else trueFunc.Add(false);
      }
    }
    public void SetLinearWithCollisionsTrueFunc() {
      trueFunc.Clear();
      for (int i = 0; i < fields.Count(); i++) {
        if (fields[i] != Field.Quad)
          trueFunc.Add(true);
        else trueFunc.Add(false);
      }
    }
    public void SetQuadTrueFunc() {
      trueFunc.Clear();
      for (int i = 0; i < fields.Count(); i++) {
        trueFunc.Add(true);
      }
    }
    
    private Matrix generateX(int n, int m, bool[] need) {
      Matrix ret = new Matrix(n, m);
      for (int j = 0; j < n; j++) {
        var point = new double[data[j].X.Count()];
        for(int i=0; i<data[j].X.Count();i++) point[i] = data[j].X[i];
        double[] f = calcF(point, need);
        ret[j] = new Matrix(m, f);
      }

      return ret;
    }

    public bool SetData(String filename) {
      initialized = false;
      data.Clear();
      try {
        StreamReader sr = new StreamReader(filename);
        while (!sr.EndOfStream) {
          var line = sr.ReadLine();
          var lines = line.Split(new char[] { '\t', ' ' });
          var linaeList = lines.ToList();
          for (int i = linaeList.Count - 1; i >= 0; i--)
            if (linaeList[i] == "") linaeList.RemoveAt(i);

          double[] point = new double[linaeList.Count - 1];
          for (int i = 0; i < linaeList.Count - 1; i++)
            point[i] = Convert.ToSingle(linaeList[i].Replace('.',','));

          data.Add(new Point(point, Convert.ToSingle(linaeList.Last().Replace('.', ','))));
        }
        sr.Close();
        initialized = true;

        makeFields();

        func = x => {
          List<double> retValue = new List<double>();
          retValue.Add(1.0);
          for (int i = 0; i < x.Count(); i++) {
            retValue.Add(x[i]);
            for (int j = i; j < x.Count(); j++) 
              retValue.Add(x[i] * x[j]);
          }
          return retValue.ToArray();
        };

        return true;
      }
      catch (Exception) { return false; }
    }
    private void makeFields() {
      List<Field> flds = new List<Field>();
      flds.Add(Field.Unit);
      for (int i = 0; i < data.First().X.Count(); i++) {
        flds.Add(Field.Linear);
        for (int j = i; j < data.First().X.Count(); j++) {
          if (i == j) flds.Add(Field.Quad);
          else flds.Add(Field.LinearCollision);
        }
      }
      fields = flds.ToArray();
    }
    public Rgz() { }

    public void DrawCorellationFields() { }
    public void CheckMultiColliniar() { }
    public void CheckGeteroskedastic() { }
    public void CheckAutoCorellation() { }
    public void GenerateOptimalModel() {
      if (!initialized) return;

      // По анализу полей
      SetQuadTrueFunc();
      var n = data.Count;
      int fullM = 0;
      foreach (bool b in trueFunc) if (b) fullM++;
      bool[] needFunc = new bool[trueFunc.Count];

      double[] y = new double[data.Count];
      for (int i = 0; i < data.Count; i++) y[i] = data[i].Y;
      Matrix Y = new Matrix(1, y.ToArray());

      var fullX = generateX(data.Count, fullM, trueFunc.ToArray());
      var fullTetta = Matrix.MNK(fullX, Y);
      var fullRss = Matrix.RSS(fullX, Y, fullTetta);

      needFunc[0] = true;
      var m = 1;
      List<double> RSS_List = new List<double>();

      for (int i = 0; i < fullM - 1; i++) {
        TettaRSS tettaRssBefore = TettaRSS.Compute(generateX(n, m, needFunc), Y);
        RSS_List.Add(tettaRssBefore.Rss);
        Dictionary<int, double> FCrit = new Dictionary<int, double>();
        var nu1 = 1;
        var nu2 = n - m - 1;

        int index = 0;
        for (int q = m; q < fullM; q++) {
          var newNeedToAdd = new bool[fullM];
          for (int w = 0; w < fullM; w++) newNeedToAdd[w] = needFunc[w];
          while (newNeedToAdd[index]) index++;
          newNeedToAdd[index] = true;
          var newM = m + 1;

          TettaRSS newTettaRss = TettaRSS.Compute(generateX(n, newM, newNeedToAdd), Y);
          var fCrit = (nu2 / nu1) * (tettaRssBefore.Rss - newTettaRss.Rss) / newTettaRss.Rss;
          FCrit.Add(index, fCrit);
          index++;
        }
        var sorted = FCrit.OrderByDescending(x => x.Value).ToList();

        m++;
        needFunc[sorted[0].Key] = true;
      }
      RSS_List.Add(TettaRSS.Compute(generateX(n, m, needFunc), Y).Rss);
    }
    public void CheckAdequat() { }
    public void DrawE() { }
    public void GetMaxY() { }
    public void GenTrustInterval() { }
  }
}
