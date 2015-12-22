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
    public struct AutoCorellation {
      public double DW;
      public double dHa;
    }
    public AutoCorellation ForAutoCor = new AutoCorellation();
    public struct Geterescedestic {
      public double ESS;
      public double ESSChi;
      public double alpha;
      public double FDistr;
      public double Rss2Rss1;
    } 
    public Geterescedestic ForGeter = new Geterescedestic();
    public struct MultiCollinear {
      public double DetXTXTrace;
      public double MinLambda;
      public double NeimanGoldstein;
      public double MaxPairConjugation;
      public double MaxConjugation;
    }
    public MultiCollinear ForMultCol = new MultiCollinear();
    public class OptimalModel {
      public OptimalModel() {
        RealX = new List<List<int>>();
        LabelsX = new List<string>();
        TettaRss = new List<TettaRSS>();
        Mellous = new List<double>();
        ManyKrit = new List<double>();
        MSEP = new List<double>();
        AEV = new List<double>();
        Functions = new List<bool[]>();
        DiffsY = new List<double>();
        AllFCrits = new List<List<KeyValuePair<int, double>>>();
      }
      public void Clear() {
        TettaRss.Clear();
        Mellous.Clear();
        ManyKrit.Clear();
        MSEP.Clear();
        AEV.Clear();
        Functions.Clear();
        DiffsY.Clear();
        AllFCrits.Clear();
      }
      public List<List<int>> RealX;
      public List<String> LabelsX;
      public List<TettaRSS> TettaRss;
      public List<double> Mellous;
      public List<double> ManyKrit;
      public List<double> MSEP;
      public List<double> AEV;
      public List<bool[]> Functions;
      public List<double> DiffsY;
      public List<List<KeyValuePair<int, double>>> AllFCrits;
    }
    public OptimalModel ForOptMdl = new OptimalModel();

    public Rgz() { ForGeter.alpha = 0.05; }

    public enum OptimalModelAlgorithm { Insertion, Exception }
    enum Field { Unit, Linear, LinearCollision, Quad }
    Field[] _fields;

    public List<Point> Data = new List<Point>();
    Matrix _Y;
    Matrix _X;
    TettaRSS _TettaEst;
    Matrix _eEst;
    public bool Initialized { get { return _initialized; } private set { } }
    bool _initialized = false;

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
        int index = 0;
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
          index++;
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
      ForOptMdl.LabelsX.Add("1");
      ForOptMdl.RealX.Add(new List<int>());
      for (int i = 0; i < Data.First().X.Count(); i++) {
        flds.Add(Field.Linear);
        var q = new List<int>();
        q.Add(i);
        ForOptMdl.RealX.Add(q);
        ForOptMdl.LabelsX.Add("X" + (i + 1).ToString());
        for (int j = i; j < Data.First().X.Count(); j++) {
          var qw = new List<int>();
          qw.Add(i); qw.Add(j);
          ForOptMdl.RealX.Add(qw);
          if (i == j) { flds.Add(Field.Quad); ForOptMdl.LabelsX.Add("X" + (i + 1).ToString() + "^2"); }
          else { flds.Add(Field.LinearCollision); ForOptMdl.LabelsX.Add("X" + (i + 1).ToString() + "*" + "X" + (j + 1).ToString()); }
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
        for (int j = 0; j < ForOptMdl.RealX[ind].Count(); j++)
          mul *= Data[i].X[ForOptMdl.RealX[ind][j]];
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
      _X = generateX(n, m, _trueFunc.ToArray());
      _TettaEst = TettaRSS.Compute(_X,_Y);
      var XTX = _X.Transpose() * _X;

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
            sum_up += _X[k, i] * _X[k, j];
            sum_d1 += _X[k, i] * _X[k, i];
            sum_d2 += _X[k, j] * _X[k, j];
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
    public void CheckGeteroskedastic() {
      if (!_initialized) return;
      int n = Data.Count;
      int m = 0; foreach (bool b in _trueFunc) if (b) m++;
      breuschPaganTest(n, m);
      goldfeldKvandton(n, m);
    }
    public void CheckAutoCorellation() {
      if (!_initialized) return;
      int n = Data.Count;
      double up = 0.0;
      double down = _eEst[0,0];
      for (int i = 1; i < n; i++) {
        down += Math.Pow(_eEst[i, 0],2.0);
        up += Math.Pow(_eEst[i, 0] - _eEst[i-1, 0], 2.0);
      }
      ForAutoCor.DW = up / down;    }
    public void GenerateOptimalModel(OptimalModelAlgorithm alg) {
      if (!_initialized) return;
      ForOptMdl.Clear();
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
        ForOptMdl.Functions.Add(needFunc.ToArray());

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
        ForOptMdl.AllFCrits.Add(FCrit.ToList());
        var sorted = FCrit.OrderByDescending(x => x.Value).ToList();
        m++; needFunc[sorted[0].Key] = true;
      }
      computeAllKrits(n, m, fullR2, sigma2, TettaRSS.Compute(generateX(n, m, needFunc), _Y));
      ForOptMdl.Functions.Add(needFunc.ToArray());
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
        ForOptMdl.Functions.Add(needFunc.ToArray());

        for (int q = fullM - m, index = 0, newM = m - 1; q < fullM; q++, index++) {
          var newNeedToAdd = new bool[_trueFunc.Count];
          for (int w = 0; w < _trueFunc.Count; w++) newNeedToAdd[w] = needFunc[w];
          while (!newNeedToAdd[index] || !_trueFunc[index]) index++;
          newNeedToAdd[index] = false;

          TettaRSS newTettaRss = TettaRSS.Compute(generateX(n, newM, newNeedToAdd), _Y);
          var fCrit = nu2_nu1 * (newTettaRss.Rss - tettaRssBefore.Rss) / newTettaRss.Rss;
          FCrit.Add(index, fCrit);
        }
        ForOptMdl.AllFCrits.Add(FCrit.ToList());
        var sorted = FCrit.OrderBy(x => x.Value).ToList();
        m--; needFunc[sorted[0].Key] = false;
      }
      computeAllKrits(n, m, fullR2, sigma2, TettaRSS.Compute(generateX(n, m, needFunc), _Y));
      ForOptMdl.Functions.Add(needFunc.ToArray());
    }
    private void computeAllKrits(int n, int m, double fullR2, double sigma2, TettaRSS tettaRss) {
      ForOptMdl.TettaRss.Add(tettaRss);
      ForOptMdl.Mellous.Add(tettaRss.Rss / sigma2 + 2.0 * m - n);

      double newR2 = calcR2(tettaRss.Y, n);
      ForOptMdl.ManyKrit.Add(newR2 / fullR2);

      var msep = tettaRss.Rss / (n * (n - m)) * (1 + n + m * (n + 1) / (n - m - 2));
      ForOptMdl.MSEP.Add(msep);
      var aev = tettaRss.Rss * m / (n * (n - m));
      ForOptMdl.AEV.Add(aev);

      var fullSum = 0.0;
      for (int d = 0; d < n; d++) {
        var dif = Math.Abs(_Y[d][0, 0] - tettaRss.Y[d][0, 0]);
        fullSum += dif;
      }
      ForOptMdl.DiffsY.Add(fullSum);
    }
    private void breuschPaganTest(int n, int m) {
      _eEst = _Y - _TettaEst.Y;
      double sigmaNew2 = _TettaEst.Rss / (double)(n - m);

      Matrix Ct = new Matrix(n);
      double cSR = 0;
      for (int i = 0; i < n; i++) {
        Ct[i, 0] = Math.Pow(_eEst[i, 0], 2.0) * lenghtToCenter(Data, i) / sigmaNew2;
        cSR += Ct[i, 0];
      } cSR /= n;

      Matrix Z = new Matrix(n, 2);
      for (int i = 0; i < n; i++) {
        Z[i, 0] = 1;
        Z[i, 1] = lenghtToCenter(Data, i);
      }

      Matrix alphaNew = Matrix.MNK(Z,Ct);
      Matrix Cnew = alphaNew.Transpose() * Z.Transpose();
      Matrix CnewT = Cnew.Transpose();

      ForGeter.ESS = 0;
      for (int i = 0; i < n; i++)
        ForGeter.ESS += Math.Pow(CnewT[i, 0] - cSR, 2.0);

      ForGeter.ESS /= 2.0;
      ForGeter.ESSChi = alglib.invchisquaredistribution(1.0, ForGeter.alpha);
    }
    private void goldfeldKvandton(int n, int m) {
      List<KeyValuePair<double, Point>> dataCopy = new List<KeyValuePair<double, Point>>();
      for (int i = 0; i < n; i++)
        dataCopy.Add(new KeyValuePair<double, Point>(lenghtToCenter(Data, i), Data[i]));
      var dataSort = dataCopy.OrderBy(x => x.Key).ToList();

      int n_c = n / 3;
      int n_new = n - n_c;
      Matrix XSort1 = new Matrix(n_new, m);
      Matrix XSort2 = new Matrix(n_new, m);
      Matrix YSort1 = new Matrix(n_new, 1);
      Matrix YSort2 = new Matrix(n_new, 1);
      for (int i = 0; i < n_c; i++) {
        var point1 = dataSort[i].Value.X;
        var point2 = dataSort[n_new + i].Value.X;
        double[] f1 = calcF(point1, _trueFunc.ToArray());
        double[] f2 = calcF(point2, _trueFunc.ToArray());
        XSort1[i] = new Matrix(m, f1);
        XSort2[i] = new Matrix(m, f2);
        YSort1[i, 0] = dataSort[i].Value.Y;
        YSort2[i, 0] = dataSort[n_new + i].Value.Y;
      }
      var rss1 = Matrix.RSS(XSort1, YSort1, Matrix.MNK(XSort1, YSort1));
      var rss2 = Matrix.RSS(XSort1, YSort1, Matrix.MNK(XSort2, YSort2));

      ForGeter.FDistr = alglib.invfdistribution(n - n_c - 2 * m, n - n_c - 2 * m, ForGeter.alpha);
      ForGeter.Rss2Rss1 = rss2 / rss1;
    }
    private double lenghtToCenter(List<Point> data, int index){
      List<double> xCntr = new List<double>();
      for (int i = 0; i < data.First().X.Count(); i++)
        xCntr.Add(0.0);
      for (int i = 0; i < data.Count; i++) 
        for (int j = 0; j < data[i].X.Count(); j++)
          xCntr[j] += data[i].X[j];

      for (int i = 0; i < xCntr.Count; i++)
        xCntr[i] /= data.Count;

      double sum = 0.0;
      for (int i = 0; i < xCntr.Count; i++)
        sum += Math.Pow(xCntr[i] - data[index].X[i], 2.0);
      return Math.Sqrt(sum);
    }  }
}
