using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace smad1.src {
  class Lab3 : Lab2 {
    private List<double>[] Etta = new List<double>[3];
    private List<double>[] EttaMin = new List<double>[3];
    private List<double>[] EttaMax = new List<double>[3];
    protected Grid UEst = new Grid(count);
    protected Grid YEst = new Grid(count);

    private MatrixOld TettaMin;
    private MatrixOld TettaMax;
    private MatrixOld FSign;
    MatrixOld XTXInv;
    private double FRegression;
    private double FFisher_1_nm;
    private double FFisher_q_nm;
    private double sigmaEst;
    private double RSS;
    private double RSSH;

    private MatrixOld alphaVector;
    private double studentDist;

    public Lab3() {
      Tetta = new double[] { 1.0, 2.0, 3.0, 4.0, 0.0000005, 0.0000006, 0.0000007, 0.0000008, 0.0000009, 0.0 };
      TettaMin = new MatrixOld(new int[] { Tetta.Count(), 1 });
      TettaMax = new MatrixOld(new int[] { Tetta.Count(), 1 });
      FSign = new MatrixOld(new int[] { Tetta.Count(), 1 });
      DegreesOfFreedom = countExperiments - Tetta.Count();
    }
    public override void GenerateData() {
      base.GenerateData();
      sigmaEst = Math.Sqrt(sigmaEstQuad);

      MatrixOld iVector = MatrixOld.Generate(new int[] { Tetta.Count(), 1 }, 1.0);
      MatrixOld IXTX = MatrixOld.Diagonal(XTX);
      XTX.GenerateNewMatrix();
      Matrix XTXInvCLSL = new Matrix(XTX.matrixNew);
      XTXInv = new MatrixOld(XTX.dimensions);
      XTXInvCLSL = XTXInvCLSL.InverseLeverrier();

      alphaVector = MatrixOld.GaussSolve(IXTX, iVector);

      for (int i = 0; i < XTXInv.dimensions[0]; i++)
        for (int j = 0; j < XTXInv.dimensions[1]; j++)
          XTXInv[i][j] = ((Complex)(((ArrayList)XTXInvCLSL.Values[i])[j])).Re;

      MatrixOld alphaSqrtVector = MatrixOld.Sqrt(alphaVector);

      studentDist = 2.24;
      generateMinMaxTetta(alphaSqrtVector);

      int q = Tetta.Count() - 1;
      FFisher_1_nm = alglib.invfdistribution(1, Lab.countExperiments - Tetta.Count(), 0.05);
      FFisher_q_nm = alglib.invfdistribution(q, Lab.countExperiments - Tetta.Count(), 0.05);

      for (int i = 0; i < Tetta.Count(); i++) 
        FSign[i][0] = Math.Pow(TettaEst[i][0], 2.0) / (sigmaEstQuad * alphaVector[i][0]);
      RSS = Lab.countExperiments - Tetta.Count() * sigmaEstQuad;

      RSSH= .0;
      foreach (double val in y) 
        RSSH += Math.Pow(val - avgY, 2.0);

      FRegression = ((RSSH - RSS) / q) / RSS * (Lab.countExperiments - Tetta.Count());

      for (int i = 0; i < count; i++) {
        for (int j = 0; j < count; j++) {
          for (int k = 0; k < count; k++) {
            double[] x = new double[3] { left + i * step, left + j * step, left + k * step };
            UEst[i,j,k] = calcU(calcF(x), TettaEst.Transpose()[0]);
          }
        }
      }
    }
    private void prepareInterval(bool etta) {
      int fixIndex = count / 2;
      Etta = new List<double>[3];
      List<MatrixOld>[] f = new List<MatrixOld>[3];
      List<MatrixOld>[] fT = new List<MatrixOld>[3];
      List<double>[] ettaEst = new List<double>[3];
      for (int rep = 0; rep < 3; rep++) {
        f[rep] = new List<MatrixOld>();
        fT[rep] = new List<MatrixOld>();
        Etta[rep] = new List<double>();
        ettaEst[rep] = new List<double>();

        for (int i = 0; i < count; i++) {
          int ii, jj, kk;
          switch (rep) {
            case 0: ii = i; jj = kk = fixIndex; break;
            case 1: jj = i; ii = kk = fixIndex; break;
            default: kk = i; ii = jj = fixIndex; break;
          }

          double[] newF = allF[ii, jj, kk];

          f[rep].Add(new MatrixOld(newF));
          fT[rep].Add(f[rep].Last().Transpose());
          if (etta) {
            Etta[rep].Add(getEtta(f[rep].Last(), Tetta));
            ettaEst[rep].Add(getEtta(f[rep].Last(), TettaEst));
          }
          else {
            Etta[rep].Add(getY(f[rep].Last(), Tetta, ii, jj, kk));
            ettaEst[rep].Add(getY(f[rep].Last(), TettaEst, ii, jj, kk));
          }
        }
      }
      EttaMin = new List<double>[3];
      EttaMax = new List<double>[3];
      for (int rep = 0; rep < 3; rep++) {
        EttaMin[rep] = new List<double>();
        EttaMax[rep] = new List<double>();

        for (int i = 0; i < count; i++) {
          double StudEttaSigma = 0.0;
          double fTXTXInvF = (fT[rep][i] * XTXInv * f[rep][i]).First();
          if(etta) StudEttaSigma = (studentDist * Math.Sqrt(fTXTXInvF) * sigmaEst);
          else StudEttaSigma = (studentDist * Math.Sqrt(fTXTXInvF + 1) * sigmaEst);
          EttaMin[rep].Add(ettaEst[rep][i] - StudEttaSigma);
          EttaMax[rep].Add(ettaEst[rep][i] + StudEttaSigma);
        }
      }
    }
    public override void Write() {
      int l3DR = 7;
      DoubleRounder = 5;
      base.Write();
      String resOutput = "----------------Lab3----------------" + Environment.NewLine;
      resOutput += "Min tetta\tTetta\tTettaEst\tMaxTetta\n";
      for (int i = 0; i < Tetta.Count(); i++) {
        resOutput += Math.Round(TettaMin[i][0], l3DR).ToString() + "\t";
        resOutput += Math.Round(Tetta[i], l3DR).ToString() + "\t";
        resOutput += Math.Round(TettaEst[i][0], l3DR).ToString() + "\t";
        resOutput += Math.Round(TettaMax[i][0], l3DR).ToString() + "\n";
      }
      resOutput += Environment.NewLine + Environment.NewLine;
      resOutput += "F_1,n-m \t= " + Math.Round(FFisher_1_nm,DoubleRounder).ToString()+Environment.NewLine;
      resOutput += Environment.NewLine + "FSign" + Environment.NewLine;
      for (int i = 0; i < Tetta.Count(); i++)
        resOutput += Math.Round(FSign[i][0], DoubleRounder).ToString() + "\t" + ((FSign[i][0] > FFisher_1_nm) ? "Rejected" : "\tNot rejected") + Environment.NewLine;

      resOutput += Environment.NewLine + "FFisher \t= " + Math.Round(FFisher_q_nm, DoubleRounder).ToString() + Environment.NewLine;
      resOutput += "FRegress \t= " + Math.Round(FRegression, DoubleRounder).ToString() + Environment.NewLine;
      resOutput += (FFisher_q_nm < FRegression) ? "Significant" : "Not significant" + Environment.NewLine;

      String toFile = "";
      toFile += Environment.NewLine + Environment.NewLine + "Etta";
      prepareInterval(true);
      writeFunc(ref toFile);
      toFile += Environment.NewLine + "Y";
      prepareInterval(false);
      writeFunc(ref toFile);

      resOutput += Environment.NewLine + "----------------Lab3----------------" + Environment.NewLine;
      Writer.Instance().Write(resOutput);
      Writer.Instance().WriteToFile(toFile);
    }
    private void writeFunc(ref string resOutput) {
      for (int rep = 0; rep < 3; rep++) {
        resOutput +=  Environment.NewLine + "x" + (rep + 1).ToString() + ":" + Environment.NewLine;
        for (int i = 0; i < count; i++) {
          resOutput += (Lab.left + step * i).ToString() + "\t";
          resOutput += Math.Round(EttaMin[rep][i], DoubleRounder).ToString() + "\t";
          resOutput += Math.Round(Etta[rep][i], DoubleRounder).ToString() + "\t";
          resOutput += Math.Round(EttaMax[rep][i], DoubleRounder).ToString() + Environment.NewLine;
        }
      }
      resOutput += Environment.NewLine;
    }
    protected override double[] calcF(double[] x) {
      return new double[] { 1.0, x[0], x[1], x[2], Math.Pow(x[0], 2.0), Math.Pow(x[1], 2.0), Math.Pow(x[2], 2.0), x[0] * x[1], x[0] * x[2], x[1] * x[2] };
    }
    private double getEtta(MatrixOld f, double[] tetta) {
      double retValue = 0.0;

      for (int i = 0; i < f.dimensions[0]; i++)
        retValue += f[i][0] * tetta[i];

      return retValue;
    }
    private double getEtta(MatrixOld f, MatrixOld tetta) {
      double retValue = 0.0;

      for (int i = 0; i < f.dimensions[0]; i++)
        retValue  += f[i][0] * tetta[i][0];

      return retValue;
    }
    private double getY(MatrixOld f, double[] tetta, int i, int j, int k) {
      double retValue = 0.0;

      for (int it = 0; it < f.dimensions[0]; it++)
        retValue += f[it][0] * tetta[it];

      return retValue + E[i, j, k];
    }
    private double getY(MatrixOld f, MatrixOld tetta, int i, int j, int k) {
      double retValue = 0.0;

      for (int it = 0; it < f.dimensions[0]; it++)
        retValue += f[it][0] * tetta[it][0];

      return retValue + E[i,j,k];
    }

    private void generateMinMaxTetta(MatrixOld alphaSqrtVector) {
      for (int i = 0; i < TettaEst.dimensions[0]; i++) {
        TettaMin[i][0] = minTetta(TettaEst[i][0], alphaSqrtVector[i][0]);
        TettaMax[i][0] = maxTetta(TettaEst[i][0], alphaSqrtVector[i][0]);
      }
    }
    private double minTetta(double tetta, double alphaSqrt) {
      return tetta - sigmaEst * alphaSqrt * studentDist;
    }
    private double maxTetta(double tetta, double alphaSqrt) {
      return tetta + sigmaEst * alphaSqrt * studentDist;
    }
  }
}
