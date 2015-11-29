using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using smad1.src;
using System.Threading.Tasks;

namespace smad1.src {
  class Lab2 : Lab1 {
    protected double alphaDistribution = 0.05;
    protected MatrixOld TettaEst;
    protected double sigmaEstQuad;
    protected MatrixOld XTX;

    protected MatrixOld eEst;
    private MatrixOld yEst;
    private double F;
    private double FT;
    private int infinity = 1000000;
    protected MatrixOld yMatrix;

    public override void Write() {
      base.Write();
      String resOutput = "----------------Lab2----------------" + Environment.NewLine;
      resOutput += "Tetta\tTettaEst" + Environment.NewLine;
      for (int i = 0; i < Tetta.Count(); i++)
        resOutput += Math.Round(Tetta[i], DoubleRounder).ToString() + "\t" + Math.Round(TettaEst[i][0], DoubleRounder).ToString() + Environment.NewLine;
      resOutput += Environment.NewLine + "Sigma2\tSigmaEst2" + Environment.NewLine + Math.Round(sigma2, DoubleRounder).ToString() + "\t" + Math.Round(sigmaEstQuad, DoubleRounder).ToString() + Environment.NewLine + Environment.NewLine;
      resOutput += "F\tFT" + Environment.NewLine + Math.Round(F, DoubleRounder).ToString() + "\t" + Math.Round(FT, DoubleRounder).ToString() + Environment.NewLine;
      resOutput += "----------------Lab2----------------" + Environment.NewLine + Environment.NewLine;

      Writer.Instance().Write(resOutput);
    }
    public override void GenerateDataForLaba4() {
      base.GenerateDataForLaba4();
      genData();
    }
    private void genData() {
      MatrixOld X = new MatrixOld(new int[] { Lab.countExperiments, Tetta.Count() });
      yMatrix = new MatrixOld(new int[] { Lab.countExperiments, 1 });

      for (int i = 0, index = 0; i < Lab.count; i++) {
        for (int j = 0; j < Lab.count; j++) {
          for (int k = 0; k < Lab.count; k++, index++) {
            double[] x = new double[3] { Lab.left + i * Lab.step, Lab.left + j * Lab.step, Lab.left + k * Lab.step };
            X[index] = calcF(x);
            yMatrix[index, 0] = Y[i, j, k];
          }
        }
      }
      MatrixOld XT = X.Transpose();
      XTX = XT * X;
      MatrixOld XTY = XT * yMatrix;
      TettaEst = MatrixOld.GaussSolve(XTX, XTY);
      yEst = X * TettaEst;
      eEst = yMatrix - yEst;
      sigmaEstQuad = (eEst.Transpose() * eEst) / (Lab.countExperiments - Tetta.Count());
      F = sigmaEstQuad / sigma2;
      FT = alglib.invfdistribution(Lab.countExperiments - Tetta.Count(), infinity, alphaDistribution);
    }
    public override void GenerateData() {
      base.GenerateData();
      genData();
    }
  }
 
}
