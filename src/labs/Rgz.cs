using System;
using System.Drawing;
using System.Windows.Forms;
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
    public struct MultiCollinear {
      public double DetXTXTrace;
      public double MinLambda;
      public double NeimanGoldstein;
      public double MaxPairConjugation;
      public double MaxConjugation;
    }
    public MultiCollinear ForMultCol = new MultiCollinear();
    
    public enum OptimalModelAlgorithm { Insertion, Exception }
    enum Field { Unit, Linear, LinearCollision, Quad }
    Field[] _fields;
    public List<List<int>> _realX = new List<List<int>>();
    public List<String> _labelsX = new List<string>();
    public List<TettaRSS> _tettaRss = new List<TettaRSS>();
    public List<Point> Data = new List<Point>();
    Matrix _Y;
    bool _initialized = false;
    public List<double> _mellous = new List<double>();
    public List<double> _manyKrit = new List<double>();
    public List<double> _mSEP = new List<double>();
    public List<double> _aEV = new List<double>();
    public List<bool[]> _functions = new List<bool[]>();
    public List<double> _diffsY = new List<double>();
    public List<Dictionary<int, double>> _allFCrits = new List<Dictionary<int, double>>();
    private void clearAllLists() {
      _tettaRss.Clear();
      _mellous.Clear();
      _manyKrit.Clear();
      _mSEP.Clear();
      _aEV.Clear();
      _functions.Clear();
      _diffsY.Clear();
      _allFCrits.Clear();
    }
    private List<bool> _trueFunc = new List<bool>();

    delegate double[] fullFunction(double[] point);
    fullFunction func;
    protected double[] calcF(double[] x, bool[] need) {
      List<double> res = new List<double>();
      var f = func(x);
      for (int i = 0; i < need.Count(); i++)
        if (need[i]) res.Add(f[i]);

      return res.ToArray();
    }

    public bool Initialized { get { return _initialized; } private set { } }
   
    public void SetLinearTrueFunc() {
      _trueFunc.Clear();
      for (int i = 0; i < _fields.Count(); i++) {
        if (_fields[i] == Field.Unit || _fields[i] == Field.Linear)
          _trueFunc.Add(true);
        else _trueFunc.Add(false);
      }
    }
    public void SetLinearWithCollisionsTrueFunc() {
      _trueFunc.Clear();
      for (int i = 0; i < _fields.Count(); i++) {
        if (_fields[i] != Field.Quad)
          _trueFunc.Add(true);
        else _trueFunc.Add(false);
      }
    }
    public void SetQuadTrueFunc() {
      _trueFunc.Clear();
      for (int i = 0; i < _fields.Count(); i++) {
        _trueFunc.Add(true);
      }
    }
    private Matrix generateX(int n, int m, bool[] need) {
      Matrix ret = new Matrix(n, m);
      for (int j = 0; j < n; j++) {
        var point = new double[Data[j].X.Count()];
        for(int i=0; i<Data[j].X.Count();i++) point[i] = Data[j].X[i];
        double[] f = calcF(point, need);
        ret[j] = new Matrix(m, f);
      }

      return ret;
    }
    public bool SetData(String filename) {
      _initialized = false;
      Data.Clear();
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

          Data.Add(new Point(point, Convert.ToSingle(linaeList.Last().Replace('.', ','))));
        }
        sr.Close();
        _initialized = true;

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

        double[] y = new double[Data.Count];
        for (int i = 0; i < Data.Count; i++) y[i] = Data[i].Y;
        _Y = new Matrix(1, y.ToArray());

        return true;
      }
      catch (Exception) { return false; }
    }
    private void makeFields() {
      List<Field> flds = new List<Field>();
      flds.Add(Field.Unit);
      _labelsX.Add("1");
      _realX.Add(new List<int>());
      for (int i = 0; i < Data.First().X.Count(); i++) {
        flds.Add(Field.Linear);
        var q = new List<int>();
        q.Add(i);
        _realX.Add(q);
        _labelsX.Add("X" + (i + 1).ToString());
        for (int j = i; j < Data.First().X.Count(); j++) {
          var qw = new List<int>();
          qw.Add(i); qw.Add(j);
          _realX.Add(qw);
          if (i == j) { flds.Add(Field.Quad); _labelsX.Add("X" + (i + 1).ToString() + "^2"); }
          else { flds.Add(Field.LinearCollision); _labelsX.Add("X" + (i + 1).ToString() + "*" + "X" + (j + 1).ToString()); }
        }
      }
      _fields = flds.ToArray();
    }
    private double calcR2(Matrix y, int n) {
      double avgY = 0.0, r2 = 0.0;
      for (int i = 0; i < n; i++) avgY += y[i][0, 0];
      avgY /= n;
      for (int i = 0; i < n; i++) {
        var t = y[i][0, 0] - avgY;
        r2 += t * t;
      }
      return r2;
    }
    public void DrawCorellationField(PictureBox pbField, int ind) {
      if (!_initialized) return;
      double[][] data = new double[Data.Count][];
      for (int i = 0; i < Data.Count; i++) {
        double mul = 1.0;
        for(int j=0; j<_realX[ind].Count();j++) 
          mul *= Data[i].X[_realX[ind][j]];
        data[i] = new double[] { mul, Data[i].Y };
      }
      var dList = data.OrderBy(x => x[0]).ToList();
      var dListY = data.OrderBy(x => x[1]).ToList();

      double c = 0.1;
      double minX = dList.First()[0] - c;
      double maxX = dList.Last()[0] + c;
      double minY = dListY.First()[1] - c;
      double maxY = dListY.Last()[1] + c;
      double distX = maxX - minX;
      double distY = maxY - minY;

      Bitmap bmp = new Bitmap(pbField.Size.Width, pbField.Size.Height);
      Graphics gr = Graphics.FromImage(bmp);
      gr.FillRectangle(Brushes.White, 0, 0, pbField.Size.Width, pbField.Size.Height);

      for (int i = 0; i < Data.Count; i++) {
        var xPixel = (dList[i][0] - minX) / distX;
        var yPixel = (dList[i][1] - minY) / distY;
        yPixel = 1.0 - yPixel;

        var pixX = ((double)pbField.Size.Width) * xPixel;
        var pixY = ((double)pbField.Size.Height) * yPixel;

        gr.FillRectangle(Brushes.Blue, ((int)pixX) - 1, ((int)pixY) - 1, 3, 3);
      }
      pbField.Image = bmp;
    }
    public void CheckMultiColliniar() {
      if (!_initialized) return;
      int n = Data.Count; 
      int m = 0; foreach (bool b in _trueFunc) if (b) m++;
      var X = generateX(n, m, _trueFunc.ToArray());
      var XTX = X.Transpose() * X;

      var XTXTrace = XTX * 1/XTX.Trace();
      ForMultCol.DetXTXTrace = XTXTrace.Determinant();
      ForMultCol.MinLambda = XTXTrace.MinEiganValue();
      ForMultCol.NeimanGoldstein = XTXTrace.MaxEiganValue() / ForMultCol.MinLambda;

      Matrix R = new Matrix(m, m);

      double r_max = 0.0;
      for (int j = 0; j < m; j++) {
        for (int i = 0; i < m; i++) {
          double sum_up = 0.0, sum_d1 = 0.0, sum_d2 = 0.0;
          for (int k = 0; k < n; k++) {
            sum_up += X[k, i] * X[k, j];
            sum_d1 += X[k, i] * X[k, i];
            sum_d2 += X[k, j] * X[k, j];
          }
          double rij = sum_up / (Math.Sqrt(sum_d1) * Math.Sqrt(sum_d2));
          if (i != j && Math.Abs(rij) > r_max)
            r_max = Math.Abs(rij);
          R[i, j] = rij;
        }
        R[j, j] = 1.0;
      }
      ForMultCol.MaxPairConjugation = r_max;

      R = R.Inverse();
      r_max = 0.0;
      for (int i = 0; i < m; i++) {
        double ri = Math.Sqrt(1.0 - 1.0 / R[i, i]);
        if (ri > r_max)
          r_max = ri;
      }
      ForMultCol.MaxConjugation = r_max;
    }
    public void CheckGeteroskedastic() { }
    public void CheckAutoCorellation() { }
    public void GenerateOptimalModel(OptimalModelAlgorithm alg) {
      if (!_initialized) return;
      clearAllLists();
      // По анализу полей
      SetLinearWithCollisionsTrueFunc();

      var n = Data.Count;
      int fullM = 0;
      foreach (bool b in _trueFunc) if (b) fullM++;

      double fullR2 = calcR2(_Y, n);

      TettaRSS fullXTettaRss = TettaRSS.Compute(generateX(Data.Count, fullM, _trueFunc.ToArray()), _Y);
      var sigma2 = fullXTettaRss.Rss / (n - fullM);

      if (alg == OptimalModelAlgorithm.Exception)
        exceptionAlgorithm(n, fullM, fullR2, sigma2);
      else
        insertionAlgorithm(n, fullM, fullR2, sigma2);
    }
    public void CheckAdequacy() { }
    public void DrawE() { }
    public void GetMaxY() { }
    public void GenConfidentInterval() { }

    private void insertionAlgorithm(int n, int fullM, double fullR2, double sigma2) {
      bool[] needFunc = new bool[_trueFunc.Count];
      needFunc[0] = true;
      var m = 1; var nu1 = 1;

      for (int i = 0; i < fullM - 1; i++) {
        TettaRSS tettaRssBefore = TettaRSS.Compute(generateX(n, m, needFunc), _Y);
        computeAllKrits(n, m, fullR2, sigma2, tettaRssBefore);
        _functions.Add(needFunc.ToArray());

        Dictionary<int, double> FCrit = new Dictionary<int, double>();
        var nu2 = n - m - 1; double nu2_nu1 = ((double)nu2 / (double)nu1);

        for (int q = m, index = 0, newM = m + 1; q < fullM; q++, index++) {
          var newNeedToAdd = new bool[_trueFunc.Count];
          for (int w = 0; w < _trueFunc.Count; w++) newNeedToAdd[w] = needFunc[w];
          while (newNeedToAdd[index] || !_trueFunc[index]) index++;
          newNeedToAdd[index] = true;

          TettaRSS newTettaRss = TettaRSS.Compute(generateX(n, newM, newNeedToAdd), _Y);
          var fCrit = nu2_nu1 * (tettaRssBefore.Rss - newTettaRss.Rss) / newTettaRss.Rss;
          FCrit.Add(index, fCrit);
        }
        var sorted = FCrit.OrderByDescending(x => x.Value).ToList();
        m++; needFunc[sorted[0].Key] = true;
      }
      computeAllKrits(n, m, fullR2, sigma2, TettaRSS.Compute(generateX(n, m, needFunc), _Y));
      _functions.Add(needFunc.ToArray());
    }
    private void exceptionAlgorithm(int n, int fullM, double fullR2, double sigma2) {
      bool[] needFunc = new bool[_trueFunc.Count];
      for (int i = 0; i < _trueFunc.Count; i++) needFunc[i] = _trueFunc[i];
      var m = fullM; var nu2 = 1;

      for (int i = 0; i < fullM - 1; i++) {
        Dictionary<int, double> FCrit = new Dictionary<int, double>();
        var nu1 = n - m + 1; double nu2_nu1 = ((double)nu2 / (double)nu1);

        TettaRSS tettaRssBefore = TettaRSS.Compute(generateX(n, m, needFunc), _Y);
        computeAllKrits(n, m, fullR2, sigma2, tettaRssBefore);
        _functions.Add(needFunc.ToArray());

        for (int q = fullM - m, index = 0, newM = m - 1; q < fullM; q++, index++) {
          var newNeedToAdd = new bool[_trueFunc.Count];
          for (int w = 0; w < _trueFunc.Count; w++) newNeedToAdd[w] = needFunc[w];
          while (!newNeedToAdd[index] || !_trueFunc[index]) index++;
          newNeedToAdd[index] = false;

          TettaRSS newTettaRss = TettaRSS.Compute(generateX(n, newM, newNeedToAdd), _Y);
          var fCrit = nu2_nu1 * (newTettaRss.Rss - tettaRssBefore.Rss) / newTettaRss.Rss;
          FCrit.Add(index, fCrit);
        }
        var sorted = FCrit.OrderBy(x => x.Value).ToList();
        m--; needFunc[sorted[0].Key] = false;
      }
      computeAllKrits(n, m, fullR2, sigma2, TettaRSS.Compute(generateX(n, m, needFunc), _Y));
      _functions.Add(needFunc.ToArray());
    }
    private void computeAllKrits(int n, int m, double fullR2, double sigma2, TettaRSS tettaRss) {
      _tettaRss.Add(tettaRss);
      _mellous.Add(tettaRss.Rss / sigma2 + 2.0 * m - n);

      double newR2 = calcR2(tettaRss.Y, n);
      _manyKrit.Add(newR2 / fullR2);

      var msep = tettaRss.Rss / (n * (n - m)) * (1 + n + m * (n + 1) / (n - m - 2));
      _mSEP.Add(msep);
      var aev = tettaRss.Rss * m / (n * (n - m));
      _aEV.Add(aev);

      var fullSum = 0.0;
      for (int d = 0; d < n; d++) {
        var dif = Math.Abs(_Y[d][0, 0] - tettaRss.Y[d][0, 0]);
        fullSum += dif;
      }
      _diffsY.Add(fullSum);
    }
  }
}
