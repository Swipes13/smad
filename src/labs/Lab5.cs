using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smad5.src.math;

namespace smad5.src.labs {
  class Lab5 {
    int countExperiments = 50;
    double noize = 0.05;

    double error = 0.01;
    private List<double> e = new List<double>();
    private List<double> u = new List<double>();
    private List<double> y = new List<double>();

    public List<double> RSSs = new List<double>();
    public List<double> Norms = new List<double>();
    public List<double> Lamdas = new List<double>();
    public List<Matrix> Tettas = new List<Matrix>();
    public List<Matrix> Bettas = new List<Matrix>();
    public List<double> BettasScalars = new List<double>();

    protected double[] Tetta = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
    protected double[] calcF(double[] x) {
      return new double[9] { 1.0, x[0], x[1], x[2], x[3], x[4], x[5], x[6], x[7] };
    }
    private double normVector(Matrix vec) {
      return (vec.Transpose() * vec)[0, 0];
    }
    private double rss(Matrix x, Matrix y, Matrix tetta) {
      return normVector(y - x * tetta);
    }
    protected double calcU(double[] f, double[] tetta) {
      double result = 0.0;
      for (int i = 0; i < tetta.Count(); i++)
        result += tetta[i] * f[i];
      return result;
    }

    private Matrix MNK(Matrix A, Matrix B, Matrix c) {
      Matrix AT = A.Transpose();
      return (AT * A + B).Inverse() * AT * c;
    }

    public String GenerateData() {
      String text = "";
      int n = countExperiments;
      int m = Tetta.Count();

      Matrix X = new Matrix(n, m);
      double x1 = 1.0, x2 = 0.5, x3 = 0.7, x4 = 0.4, x5 = 0.3, x6 = 1.0, x7 = 2.0;

      text += "1. Generating data"+ Environment.NewLine;
      double avgU = 0.0;
      List<double[]> points = new List<double[]>();

      for (int i = 0; i < n; i++) {
        double e_ = math.Distribution.Normal(0.0, error);
        double x1_ = math.Distribution.Flat(-x1, x1 * 2.0);
        double x2_ = math.Distribution.Flat(-x2, x2 * 2.0);
        double x3_ = math.Distribution.Flat(-x3, x3 * 2.0);
        double x4_ = math.Distribution.Flat(-x4, x4 * 2.0);
        double x5_ = math.Distribution.Flat(-x5, x5 * 2.0);
        double x6_ = math.Distribution.Flat(-x6, x6 * 2.0);
        double x7_ = math.Distribution.Flat(-x7, x7 * 2.0);
        double x8_ = x1_ + 2.0 * x2_ - 3.0 * x3_ + e_;
        //double x8_ = math.Distribution.Flat(-x6, x6 * 2.0);

        double[] point = new double[]{
          x1_,x2_,x3_,x4_,x5_,x6_,x7_, x8_ };
        points.Add(point);
        double[] f = calcF(point);
        X[i] = new Matrix(m, f);
        u.Add(calcU(f, Tetta));
        avgU += u.Last();
      }
      avgU /= countExperiments;

      double sigma2 = avgU * noize;

      for (int i = 0; i < countExperiments; i++) {
        e.Add(math.Distribution.Normal(0.0, sigma2));
        y.Add(u[i] + e.Last());
      }

      text += Environment.NewLine + "2. Calculating multi-conjugation effect data" + Environment.NewLine;
      Matrix XT = X.Transpose();
      Matrix XTX = XT * X;
      double detXTX = XTX.Determinant();
      text += "det(XTX) = \t" + detXTX.ToString() + Environment.NewLine;

      Matrix XTX_Trace = XTX / XTX.Trace();
      double detXTX_Trace = XTX_Trace.Determinant();
      text += "det(XTX/trace) = \t" + detXTX_Trace.ToString() + Environment.NewLine;

      double maxL = XTX.MaxEiganValue(); 
      double minL = XTX.MinEiganValue();
      text += "minLambda = \t\t" + minL.ToString() + Environment.NewLine;

      double lmax_lmin = maxL / minL;
      text += "Neiman-Goldstein = \t" + lmax_lmin.ToString() + Environment.NewLine;

      Matrix R = new Matrix(Tetta.Count(), Tetta.Count());

      double r_max = 0.0;
      for (int j = 0; j < Tetta.Count(); j++) {
        for (int i = 0; i < Tetta.Count(); i++) {
          double sum_up = 0.0, sum_d1 = 0.0, sum_d2 = 0.0;
          for (int k = 0; k < countExperiments; k++) {
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
      text += "Max pair conjugation = \t" + r_max.ToString() + Environment.NewLine;

      Matrix RInverse = R.Inverse();

      r_max = 0.0;
      for (int i = 0; i < Tetta.Count(); i++) {
        double ri = Math.Sqrt(1.0 - 1.0 / RInverse[i,i]);
        if (ri > r_max)
          r_max = ri;
      }
      text += "Max conjugation = \t" + r_max.ToString() + Environment.NewLine;
      
      text += Environment.NewLine + "3. Ridge assessment" + Environment.NewLine;

      Matrix Z = X * Matrix.DiagonalSqrt(XTX).Inverse();

      Matrix Y = new Matrix(1, y.ToArray());
      for (double lambda = 0.0; lambda <= 50; lambda += 1) {
        Matrix Л = Matrix.Diagonal(XTX) * lambda;
        Tettas.Add(MNK(X, Л,Y));

        RSSs.Add(rss(X, Y, Tettas.Last()));
        Norms.Add(normVector(Tettas.Last()));
        Lamdas.Add(lambda);

        Bettas.Add(MNK(Z,lambda*Matrix.Identity(Z.LenghtY()),Y));
        BettasScalars.Add(Math.Sqrt(normVector(Bettas.Last())));
      }
 
      int center = Lamdas.Count/2;
      text += "Lambda = " + Lamdas[center].ToString() + Environment.NewLine + "Tetta = " + Environment.NewLine;
      for (int i = 0; i < Tettas.First().LenghtX(); i++) 
        text += Tettas[center][i,0].ToString() + Environment.NewLine;

      Matrix allBettas = new Matrix(BettasScalars.Count, BettasScalars.ToArray());
      double sigmaEst = rss(X, Y, Tettas.First()) / (n - m);

      Matrix D = Matrix.DiagonalSqrt(XTX) * Tettas.First();
      double lambdaNew = m * sigmaEst / Math.Sqrt(normVector(D));

      Tettas.Add(MNK(X,Matrix.Diagonal(XTX)*lambdaNew,Y));

      RSSs.Add(rss(X, Y, Tettas.Last()));
      Norms.Add(normVector(Tettas.Last()));
      Lamdas.Add(lambdaNew);

      text += "Lambda* = " + lambdaNew.ToString() + Environment.NewLine + "Tetta = " + Environment.NewLine;
      for (int i = 0; i < Tettas.Last().LenghtX(); i++)
        text += Tettas.Last()[i, 0].ToString() + Environment.NewLine;

      text += Environment.NewLine + "3. Main components" + Environment.NewLine;


      double[] x_center = new double[points[0].Count()];
      double y_center = 0.0;
      for(int i = 0; i < points.Count; i++){
        y_center += y[i];
        for(int j=0;j<points[i].Count();j++)
          x_center[j] += points[i][j];
      }
      y_center /= (double)points.Count;
      for(int i = 0; i < points[0].Count(); i++)
        x_center[i] /= (double)points.Count;

      Matrix X_center = new Matrix(n, m);
      List<double> y_cent = new List<double>();
      for(int i = 0; i < points.Count; i++){
        double[] point = new double[points[0].Count()];
        for (int j = 0; j < points[i].Count(); j++)
          point[j] = points[i][j] - x_center[j];//X_center[i,j] = points[i][j] - x_center[j];
        double[] f = calcF(point);
        X_center[i] = new Matrix(m, f);
        y_cent.Add(y[i] - y_center);
      }
      Matrix Y_center = new Matrix(y_cent.Count, y_cent.ToArray());
      Matrix X_centerT = X_center.Transpose();
      Matrix X_centerTX_center = X_centerT*X_center;
      var eiganVals = X_centerTX_center.EiganValues();
      text += "eiganValues:" + Environment.NewLine;
      
      Matrix nulMat =  new Matrix(m,1);
      List<Matrix> eiVecs = new List<Matrix>();
      for (int i =  eiganVals.Count() -1; i >=0; i--) {
        text += eiganVals[i].ToString() + Environment.NewLine;
        Matrix a_lI = X_centerTX_center - eiganVals[i] * Matrix.Identity(X_centerTX_center.LenghtX());
        eiVecs.Add(Matrix.GaussSolve(a_lI, nulMat));
      }
      Matrix V = new Matrix(m, eiganVals.Count());
      for (int i = 0; i < m; i++)
        for (int j = 0; j < eiganVals.Count(); j++) 
          V[i, j] = eiVecs[i][j,0];
      return text;

      Matrix Z_ = X_center * V; 
      int notCool = 3;
      Matrix Vr = new Matrix(m,eiganVals.Count() - notCool);
      Matrix Zr = new Matrix(Z_.LenghtX(), Z_.LenghtY()- notCool);

      for (int i = 0; i < m; i++)
        for (int j = 0; j < eiganVals.Count() - notCool; j++) 
          Vr[i, j] = V[i, j];

      for (int i = 0; i < Z_.LenghtX(); i++)
        for (int j = 0; j < Z_.LenghtY() - notCool; j++)
          Zr[i, j] = Z_[i, j];

      Matrix bb = (Zr.Transpose() * Zr).Inverse() * Zr.Transpose() * Y_center;
      Matrix tettaCenter = Vr * bb;
      return text;
    }
  }
}
