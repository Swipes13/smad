using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smad1.src {
  class Lab4 : Lab2 {
    public override void Write() {
      base.Write();
      String resOutput = "----------------Lab4----------------" + Environment.NewLine;

      resOutput += sigma2.ToString() + Environment.NewLine;

      resOutput += "----------------Lab4----------------" + Environment.NewLine + Environment.NewLine;

      Writer.Instance().Write(resOutput);
    }
    public override void GenerateData() {
      base.GenerateDataForLaba4();
      //base.GenerateData();
      double sigmaNew2 = 0;
      for (int i = 0; i < eEst.dimensions[0]; i++) {
        sigmaNew2 += Math.Pow(eEst[i, 0], 2.0);
      }
      sigmaNew2 = Math.Sqrt(sigmaNew2 / eEst.dimensions[0]);

      sigma2 = Math.Pow(sigmaNew2, 2.0);

      MatrixOld Ct = new MatrixOld(eEst.dimensions);
      for (int i = 0; i < eEst.dimensions[0]; i++) {
        Ct[i, 0] = Math.Pow(eEst[i, 0], 2.0) / sigmaNew2;
      }
      MatrixOld Z = new MatrixOld(new int [] {eEst.dimensions[0], 2});
      for (int i = 0; i < eEst.dimensions[0]; i++) {
        Z[i, 0] = 1;
        Z[i, 1] = sigmaNew[i];
      }

      //MatrixOld alphaNew = new MatrixOld(new int[]{ 2, 1 });

      MatrixOld ZT = Z.Transpose();
      MatrixOld ZTZ = ZT * Z;
      MatrixOld ZTCt = ZT * Ct;
      MatrixOld alphaNew = MatrixOld.GaussSolve(ZTZ, ZTCt);

      MatrixOld alphaNewT = alphaNew.Transpose();
      MatrixOld Cnew = alphaNewT * ZT;
      MatrixOld CnewT = Cnew.Transpose();

      double cSR = 0;

      for (int i = 0; i < eEst.dimensions[0]; i++) {
        cSR += Ct[i, 0];
      }
      cSR /= eEst.dimensions[0];

      double ESS = 0;
      for (int i = 0; i < eEst.dimensions[0]; i++) {
        ESS += Math.Pow(CnewT[i, 0] - cSR,2.0);
      }
      ESS /= 2.0;

      double ESSXi = alglib.invchisquaredistribution(1.0, 0.05);

      int n_c = countExperiments / 3;
      int n_new = countExperiments - n_c;


      List<double> X1sorted = new List<double>();
      List<double> X2sorted = new List<double>();
      List<double> X3sorted = new List<double>();
      List<double> Ysorted = new List<double>();
      List<double> Sigmasorted = sigmaNew.OrderBy(x => x).ToList();
      for (int i = 0; i < Y.count; i++) {
        for (int j = 0; j < Y.count; j++) {
          for (int k = 0; k < Y.count; k++) {
            Ysorted.Add(Y[i, j, k]);
          }
        }
      }
      Ysorted = Ysorted.OrderBy(x => x).ToList();
      for (int i = 0; i < countExperiments; ) {
        for (int j = 0; j < count; j++) {
          X1sorted.Add(allX[j, 0, 0][0]);
          X2sorted.Add(allX[0, j, 0][1]);
          X3sorted.Add(allX[0, 0, j][2]);
          i++;
        }
      }
      X1sorted = X1sorted.OrderBy(x => x).ToList();
      X2sorted = X2sorted.OrderBy(x => x).ToList();
      X3sorted = X3sorted.OrderBy(x => x).ToList();

      List<double[]> X_sort1 = new List<double[]>();
      List<double[]> X_sort2 = new List<double[]>();
      List<double> Y_sort1 = new List<double>();
      List<double> Y_sort2 = new List<double>();
      List<double> Sigma_sort1 = new List<double>();
      List<double> Sigma_sort2 = new List<double>();

      for (int i = 0; i < n_c; i++) {
        X_sort1.Add(new double[] { X1sorted[i], X2sorted[i], X3sorted[i] });
        X_sort2.Add(new double[] { X1sorted[n_new + i], X2sorted[n_new + i], X3sorted[n_new + i] });
        Y_sort1.Add(Ysorted[i]);
        Y_sort2.Add(Ysorted[n_new + i]);
        Sigma_sort1.Add(Sigmasorted[i]);
        Sigma_sort2.Add(Sigmasorted[n_new + i]);
      }

      MatrixOld XMatSort1 = new MatrixOld(new int[] { n_c, Tetta.Count() });
      MatrixOld XMatSort2 = new MatrixOld(new int[] { n_c, Tetta.Count() });
      for (int i = 0; i < n_c; i++) {
        XMatSort1[i] = calcF(X_sort1[i]);
        XMatSort2[i] = calcF(X_sort2[i]);
      }
      MatrixOld XMatSort1T = XMatSort1.Transpose();
      MatrixOld XMatSort1TXMatSort1 = XMatSort1T * XMatSort1;

      MatrixOld XMatSort1TYSort1 = XMatSort1T * new MatrixOld(Y_sort1.ToArray());
      MatrixOld TettaNewSort1 = MatrixOld.GaussSolve(XMatSort1TXMatSort1, XMatSort1TYSort1);

      MatrixOld XMatSort2T = XMatSort2.Transpose();
      MatrixOld XMatSort2TXMatSort2 = XMatSort2T * XMatSort2;

      MatrixOld XMatSort2TYSort2 = XMatSort2T * new MatrixOld(Y_sort2.ToArray());
      MatrixOld TettaNewSort2 = MatrixOld.GaussSolve(XMatSort2TXMatSort2, XMatSort2TYSort2);
 
    }
  }
}
