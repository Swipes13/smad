using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smad1.src {
  public class Grid {
    public int count;
    double[][][] grd;
    public Grid(int count_) {
      count = count_;

      grd = new double[count][][];
      for (int i = 0; i < count; i++) 
        grd[i] = new double[count][];
      
      for (int i = 0; i < count; i++) 
        for (int j = 0; j < count; j++) 
          grd[i][j] = new double[count];
    }
    public double this[int i,int j,int k] {
      get { return grd[i][j][k]; }
      set { grd[i][j][k] = value; }
    }
  }
  class Lab1 : Lab {
    protected double sigma2 = .0;
    protected Grid E = new Grid(count);
    protected Grid Y = new Grid(count);
    protected Grid U = new Grid(count);
    protected double[, ,][] allX = new double[count, count, count][];
    protected double[, ,][] allF = new double[count, count, count][];
    private static double noizePart = 0.05;  // 5-15%   

    private double omega2 = .0;
    private double avgU = .0;
    protected double avgY = .0;

    private List<double> e = new List<double>();
    private List<double> u = new List<double>();
    protected List<double> y = new List<double>();
    protected List<double> sigmaNew = new List<double>();

    public override void SetNoize(double noize) { noizePart = noize; }
    public override void GenerateDataForLaba4() {
      for (int i = 0; i < count; i++) {
        for (int j = 0; j < count; j++) {
          for (int k = 0; k < count; k++) {
            double[] x = new double[3] { left + i * step, left + j * step, left + k * step };
            allX[i, j, k] = x;
            allF[i, j, k] = calcF(x);
            u.Add(calcU(allF[i, j, k], Tetta));
            U[i, j, k] = u.Last();
            avgU += u.Last();
          }
        }
      }
      avgU /= u.Count;
      
      for (int i = 0; i < u.Count; i++)
        sigmaNew.Add(Math.Sqrt(Math.Abs(u[i]) * noizePart));

      double qwer = rand.NextGaussianDouble();
      for (int i = 0; i < u.Count; i++) {
        e.Add(rand.NextGaussianDouble() * sigmaNew[i]);
        y.Add(u[i] + e.Last());
        avgY += y.Last();
      }
      avgY /= y.Count;

      int index = 0;
      for (int i = 0; i < count; i++) {
        for (int j = 0; j < count; j++) {
          for (int k = 0; k < count; k++) {
            E[i, j, k] = e[index];
            Y[i, j, k] = y[index];
            index++;
          }
        }
      }
    }
    public override void GenerateData() {
      for (int i = 0; i < count; i++) {
        for (int j = 0; j < count; j++) {
          for (int k = 0; k < count; k++) {
            double[] x = new double[3] { left + i * step, left + j * step, left + k * step };
            allF[i,j,k] = calcF(x);
            u.Add(calcU(allF[i, j, k], Tetta));
            U[i, j, k] = u.Last();
            avgU += u.Last();
          }
        }
      }
      avgU /= u.Count;

      for (int i = 0; i < u.Count; i++) 
        omega2 += Math.Pow(u[i] - avgU, 2.0) / (u.Count - 1);

      sigma2 = omega2 * noizePart;
      double sigma = Math.Sqrt(sigma2);

      for (int i = 0; i < u.Count; i++)
        sigmaNew.Add(sigma);

      double qwer = rand.NextGaussianDouble();
      for (int i = 0; i < u.Count; i++) {
        e.Add(rand.NextGaussianDouble() * sigma);
        y.Add(u[i] + e.Last());
        avgY += y.Last();
      }
      avgY /= y.Count;

      int index = 0;
      for (int i = 0; i < count; i++) {
        for (int j = 0; j < count; j++) {
          for (int k = 0; k < count; k++) {
            E[i, j, k] = e[index];
            Y[i, j, k] = y[index];
            index++;
          }
        }
      }
    }
    public override void Write() {
      String resOutput = "----------------Lab1----------------" + Environment.NewLine;
      resOutput += "Tetta \t= {";
      for (int i = 0; i < Tetta.Count() - 1; i++)
        resOutput += Tetta[i].ToString() + ",";
      resOutput += Tetta.Last().ToString() + "}" + Environment.NewLine;
      resOutput += "Sigma2 \t= " + sigma2.ToString() + Environment.NewLine;
      resOutput += "Noize \t= " + (noizePart * 100).ToString() + "%" + Environment.NewLine;
      resOutput += "Avg U \t= " + avgU.ToString() + Environment.NewLine;
      resOutput += "----------------Lab1----------------" + Environment.NewLine + Environment.NewLine;

      Writer.Instance().Write(resOutput);
    }

    protected override double[] calcF(double[] x) {
      return new double[9] { 1.0, x[0], x[1], x[2], Math.Pow(x[0], 2.0), Math.Pow(x[1], 2.0), Math.Pow(x[2], 2.0), x[0] * x[1], x[0] * x[2] };
    }

    protected double calcU(double[] f, double[] tetta) {
      double result = 0.0;
      for (int i = 0; i < tetta.Count(); i++)
        result += tetta[i] * f[i];
      return result;
    }
  }
}
