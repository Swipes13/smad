using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace smad5.src.math {
  class Matrix {
    private double[,] matrix;
    private MatrixQ matrixq;

    public Matrix this[int index] {
      get {
        Matrix retVal = new Matrix(1, LenghtY());

        for (int i = 0; i < LenghtY(); i++)
          retVal[0, i] = matrix[index, i];

        return retVal;
      }
      set {
        for (int i = 0; i < value.LenghtY(); i++)
          matrix[index, i] = value[0, i];
      }
    }
    public double this[int i, int j] {
      get { return matrix[i, j]; }
      set { matrix[i, j] = value; }
    }

    public int LenghtX() { return matrix.GetLength(0); }
    public int LenghtY() { return matrix.GetLength(1); }
    public String DimsToString() {
      return "[" + LenghtX() + "," + LenghtY() + "]";
    }

    public Matrix(int size) {
      matrix = new double[size, size];
    }
    public Matrix(int sizeX, int sizeY) {
      matrix = new double[sizeX, sizeY];
    }
    public Matrix(int sizeX, double[] values) {
      matrix = new double[values.Count() / sizeX, sizeX];
      int i = 0, j = 0;
      foreach (double val in values) {
        matrix[i, j] = val;
        j++;
        if (j == sizeX) {
          i++;
          j = 0;
        }
      }
    }
    public Matrix(Matrix m) {
      matrix = new double[m.LenghtX(), m.LenghtY()];
      for (int i = 0; i < m.LenghtX(); i++)
        for (int j = 0; j < m.LenghtY(); j++)
          matrix[i, j] = m[i,j];

    }

    public double Determinant() {
      matrixq = new MatrixQ(matrix);
      return matrixq.Determinant().Re;
    }

    public static Matrix Identity(int dim) {
      Matrix m = new Matrix(dim, dim);
      for (int i = 0; i < dim; i++)
        m[i, i] = 1.0;
      return m;
    }
    public Matrix Transpose() {
      Matrix transpose = new Matrix(LenghtY(), LenghtX());

      for (int i = 0; i < LenghtX(); i++)
        for (int j = 0; j < LenghtY(); j++)
          transpose[j, i] = this.matrix[i, j];

      return transpose;
    }
    public static Matrix Diagonal(double[] dValues) {
      Matrix matrix = new Matrix(dValues.Count());
      for (int i = 0; i < dValues.Count(); i++)
        matrix[i, i] = dValues[i];

      return matrix;
    }
    public static Matrix DiagonalSqrt(Matrix mat) {
      Matrix matrix = new Matrix(mat.LenghtX(), mat.LenghtY());
      for (int i = 0; i < mat.LenghtX(); i++)
        matrix[i, i] = Math.Sqrt(mat[i, i]);

      return matrix;
    }
    public static Matrix Diagonal(Matrix mat) {
      Matrix matrix = new Matrix(mat.LenghtX(), mat.LenghtY());
      for (int i = 0; i < mat.LenghtX(); i++)
        matrix[i, i] = mat[i,i];

      return matrix;
    }
    public static bool MultiDistr(double[] expctedValues, Matrix correlation, ref Matrix result) {
      int count = result.LenghtX();
      int mq = result.LenghtY();

      Matrix A = new Matrix(mq, mq);
      Matrix N = new Matrix(count, mq);
      Matrix K = correlation;

      if (K.Determinant() <= 0.0) return false;

      for (int i = 0; i < mq; i++) {
        for (int j = 0; j <= i; j++) {
          double sumA = 0;
          double sumA2 = 0;
          for (int k = 0; k < j; k++) {
            sumA += A[i, k] * A[j, k];
            sumA2 += A[j, k] * A[j, k];
          }
          A[i, j] = (K[i, j] - sumA) / Math.Sqrt(K[j, j] - sumA2);
        }
      }

      for (int i = 0; i < count; i += 2) {
        for (int j = 0; j < mq; j++) {
          N[i, j] = src.math.Distribution.Normal(0.0,1.0);
          if (i + 1 < count) N[i + 1, j] = src.math.Distribution.Normal(0.0, 1.0);
        }
      }
      for (int i = 0; i < count; i++) {
        for (int j = 0; j < mq; j++) {
          result[i, j] = expctedValues[j];
          for (int k = 0; k < mq; k++) {
            result[i, j] += A[j, k] * N[i, k];
          }
        }
      }
      return true;
    }
    public static bool MultiDistr(double[] expctedValues, double[] dispersion, Matrix correlation, ref Matrix result) {
      int count = result.LenghtX();
      int mq = result.LenghtY();

      Matrix A = new Matrix(mq, mq);
      Matrix N = new Matrix(count, mq);
      Matrix K = new Matrix(mq, mq);

      for (int i = 0; i < mq; i++)
        for (int j = 0; j < mq; j++)
          K[i, j] = correlation[i, j] * Math.Sqrt(dispersion[i] * dispersion[j]);

      if (K.Determinant() <= 0.0) return false;

      for (int i = 0; i < mq; i++) {
        for (int j = 0; j <= i; j++) {
          double sumA = 0;
          double sumA2 = 0;
          for (int k = 0; k < j; k++) {
            sumA += A[i, k] * A[j, k];
            sumA2 += A[j, k] * A[j, k];
          }
          A[i, j] = (K[i, j] - sumA) / Math.Sqrt(K[j, j] - sumA2);
        }
      }

      for (int i = 0; i < count; i += 2) {
        for (int j = 0; j < mq; j++) {
          N[i, j] = src.math.Distribution.Normal(0.0, 1.0);
          if (i + 1 < count) N[i + 1, j] = src.math.Distribution.Normal(0.0, 1.0);
        }
      }

      for (int i = 0; i < count; i++) {
        for (int j = 0; j < mq; j++) {
          result[i, j] = expctedValues[j];
          for (int k = 0; k < mq; k++) {
            result[i, j] += A[j, k] * N[i, k];
          }
        }
      }
      return true;
    }

    static public Matrix operator *(Matrix m1, Matrix m2) {
      if (m1.LenghtY() != m2.LenghtX())
        throw new InvalidOperationException("Умножение матриц " + m1.DimsToString() + "*" + m2.DimsToString() + "!");
      Matrix retValue = new Matrix(m1.LenghtX(), m2.LenghtY());

      for (int i = 0; i < m1.LenghtX(); i++)
        for (int j = 0; j < m2.LenghtY(); j++)
          for (int k = 0; k < m1.LenghtY(); k++)
            retValue[i, j] += m1[i, k] * m2[k, j];

      return retValue;
    }
    static public Matrix operator *(double value, Matrix m1) {
      Matrix retValue = new Matrix(m1.LenghtX(), m1.LenghtY());

      for (int i = 0; i < m1.LenghtX(); i++)
        for (int j = 0; j < m1.LenghtY(); j++)
          retValue[i, j] = m1[i, j] * value;

      return retValue;
    }
    static public Matrix operator *(Matrix m1,double value) {
      Matrix retValue = new Matrix(m1.LenghtX(), m1.LenghtY());

      for (int i = 0; i < m1.LenghtX(); i++)
        for (int j = 0; j < m1.LenghtY(); j++)
          retValue[i, j] = m1[i, j] * value;

      return retValue;
    }
    static public Matrix operator +(Matrix m1, Matrix m2) {
      if (m1.LenghtX() != m2.LenghtX() || m1.LenghtY() != m2.LenghtY())
        throw new InvalidOperationException("Сложение матриц " + m1.DimsToString() + "+" + m2.DimsToString() + "!");

      Matrix retValue = new Matrix(m1.LenghtX(), m1.LenghtY());

      for (int i = 0; i < m1.LenghtX(); i++)
        for (int j = 0; j < m1.LenghtY(); j++)
          retValue[i, j] = m1[i, j] + m2[i, j];

      return retValue;
    }
    static public Matrix operator -(Matrix m1, Matrix m2) {
      if (m1.LenghtX() != m2.LenghtX() || m1.LenghtY() != m2.LenghtY())
        throw new InvalidOperationException("Вычитание матриц " + m1.DimsToString() + "-" + m2.DimsToString() + "!");

      Matrix retValue = new Matrix(m1.LenghtX(), m1.LenghtY());

      for (int i = 0; i < m1.LenghtX(); i++)
        for (int j = 0; j < m1.LenghtY(); j++)
          retValue[i, j] = m1[i, j] - m2[i, j];

      return retValue;
    }
    static public Matrix operator /(Matrix m1, double val) {
      Matrix retValue = new Matrix(m1.LenghtX(), m1.LenghtY());

      for (int i = 0; i < m1.LenghtX(); i++)
        for (int j = 0; j < m1.LenghtY(); j++)
          retValue[i, j] = m1[i, j] / val;

      return retValue;
    }
    public Matrix Decompose() {
      if (LenghtX() != LenghtY())
        throw new InvalidOperationException("Факторизация матрицы размерности " + DimsToString() + "!");

      Matrix ret = new Matrix(LenghtX(), LenghtY());
      for (int i = 0; i < LenghtX(); i++) {

        double temp;
        for (int j = 0; j < i; j++) {
          if (matrix[i, j] != matrix[j,i])
            throw new InvalidOperationException("Факторизация несимметричной матрицы!");
          temp = 0;
          for (int k = 0; k < j; k++) 
            temp += ret[i, k] * ret[j, k];

          ret[i, j] = (matrix[i, j] - temp) / ret[j, j];
        }

        //Находим значение диагонального элемента
        temp = matrix[i,i];
        for (int k = 0; k < i; k++)
          temp -= ret[i, k] * ret[i, k];

        ret[i, i] = Math.Sqrt(temp);
      }
      return ret;
    }
    public Matrix Inverse() {
      matrixq = new MatrixQ(matrix);
      matrixq = matrixq.InverseLeverrier();

      Matrix retValue = new Matrix(this);
      for (int i = 0; i < LenghtX(); i++)
        for (int j = 0; j < LenghtY(); j++)
          retValue[i, j] = ((Complex)(((ArrayList)matrixq.Values[i])[j])).Re;

      return retValue;
    }
    public double Trace() {
      matrixq = new MatrixQ(matrix);
      return matrixq.Trace().Re;
    }
    public double MinEiganValue() {
      matrixq = new MatrixQ(matrix);
      var eiganVals = matrixq.Eigenvalues();

      return ((Complex)(((ArrayList)eiganVals.Values[eiganVals.RowCount - 1])[eiganVals.ColumnCount - 1])).Re;
    }
    public double MaxEiganValue() {
      matrixq = new MatrixQ(matrix);
      var eiganVals = matrixq.Eigenvalues();

      return ((Complex)(((ArrayList)eiganVals.Values[0])[0])).Re;
    }
    public double[] EiganValues() {
      matrixq = new MatrixQ(matrix);
      var eiganVals = matrixq.Eigenvalues();
      double[] ret = new double[eiganVals.RowCount];

      for (int i = 0; i < eiganVals.RowCount; i++) 
        ret[i] = eiganVals[i+1, 1].Re;
      

        return ret;
    }

    static public Matrix GaussSolve(Matrix A, Matrix b) {
      Matrix retValue = new Matrix(b);

      for (int i = 0; i < b.LenghtX(); i++) {
        int maxIndex = getMaxElement(A, i);
        if (maxIndex != i) {
          swapGauss(A, i, maxIndex);
          swapGauss(retValue, 0, maxIndex);
        }
        gaussLine(A, retValue, i);
      }
      for (int i = b.LenghtX() - 1; i >= 0; i--) {
        double sum = 0.0;
        for (int j = i + 1; j < b.LenghtX(); j++)
          sum += A[i, j] * retValue[j, 0];
        retValue[i, 0] = (retValue[i, 0] - sum) / A[i, i];
      }
      return retValue;
    }
    static int getMaxElement(Matrix A, int index) {
      int maxIndex = index;
      for (int i = index; i < A.LenghtX(); i++) {
        if (Math.Abs(A[maxIndex, index]) < Math.Abs(A[i, index]))
          maxIndex = i;
      }
      return maxIndex;
    }
    static void swapGauss(Matrix A, int index, int maxIndex) {
      double value = 0.0;
      for (int i = index; i < A.LenghtY(); i++) {
        value = A[index, i];
        A[index, i] = A[maxIndex, i];
        A[maxIndex, i] = value;
      }
    }
    static void gaussLine(Matrix A, Matrix b, int index) {
      double value = 0.0;
      for (int i = index + 1; i < A.LenghtY(); i++) {
        if (A[i, index] == 0) continue;
        value = A[i, index] / A[index, index];
        b[i, 0] -= b[index, 0] * value;
        for (int j = index; j < A.LenghtY(); j++)
          A[i,j] -= A[index,j] * value;
      }
    }
  }
}
