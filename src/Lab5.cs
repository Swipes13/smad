using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace smad1.src {
  class Lab5 {
    protected Random rand = new Random();
    int countExperiments = 1000;
    double noize = 0.05;

    protected double[] Tetta = new double[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 };
    protected double[] calcF(double[] x) {
      double error = 0.01;
      double e_ = rand.NextGaussianDouble()*error;
      return new double[9] { 1.0, x[0], x[1], x[2], x[3], x[4], x[5], x[6], x[4] + 10.0 * x[5] - 4.0 * x[6] + e_ };
    }
    public void GenerateData() {
      MatrixOld X = new MatrixOld(new int[] { countExperiments, Tetta.Count() });

      List<double> u_ = new List<double>();
      double x1 = 1.0, x2 = 10.0, x3 = 0.7, x4 = 100.0, x5 = 50.0, x6 = 1.0, x7 = 10.0;

      double avgU = 0.0;
      for (int i = 0; i < countExperiments; i++) {
        double[] point = new double[]{
          rand.NextDouble()*x1,rand.NextDouble()*x2,
          rand.NextDouble()*x3,rand.NextDouble()*x4,
          rand.NextDouble()*x5,rand.NextDouble()*x6,
          rand.NextDouble()*x7
        };
        double[] f = calcF(point);
        X[i] = f;
        u_.Add(calcU(f, Tetta));
        avgU += u_.Last();
      }
      avgU /= countExperiments;

      double sigma2 = rand.NextGaussianDouble() * avgU * noize;

      MatrixOld XT = X.Transpose();
      MatrixOld XTX = XT * X;
      XTX.GenerateNewMatrix();
      Matrix XTXCLSL = new Matrix(XTX.matrixNew);


      double det = XTXCLSL.Determinant().Re;

      double trace = XTXCLSL.Trace().Re;
      XTXCLSL *= 1.0/trace;
      
      det = XTXCLSL.Determinant().Re;
      
      var eiganVals = XTXCLSL.Eigenvalues();

      double maxL = ((Complex)(((ArrayList)eiganVals.Values[0])[0])).Re;
      double minL = ((Complex)(((ArrayList)eiganVals.Values[eiganVals.RowCount - 1])[eiganVals.ColumnCount - 1])).Re;

      double lmax_lmin = maxL / minL ;
    }
    protected double calcU(double[] f, double[] tetta) {
      double result = 0.0;
      for (int i = 0; i < tetta.Count(); i++)
        result += tetta[i] * f[i];
      return result;
    }
  }
}
