using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smad1.src {
  public class MatrixOld {
    public int[] dimensions;
    public double[][] matrix;
    public double[,] matrixNew;

    public double First() {
      return matrix[0][0];
    }
    public void GenerateNewMatrix() {
      for (int i = 0; i < dimensions[0]; i++)
        for (int j = 0; j < dimensions[1]; j++) 
          matrixNew[i, j] = (matrix[i][j]);
    }
    public double this[int i, int j] {
      get { return matrix[i][j]; }
      set { matrix[i][j] = value; matrixNew[i, j] = matrix[i][j]; }
    }
    public double[] this[int i] {
      get { return matrix[i]; }
      set { matrix[i] = value; for(int it=0; it<matrix[i].Count(); it++) matrixNew[i,it] = matrix[i][it]; }
    }
    public static MatrixOld Diagonal(MatrixOld origianl) {
      MatrixOld m = new MatrixOld(new int[] { origianl.dimensions[0], origianl.dimensions[0] });
      for (int i = 0; i < origianl.dimensions[0]; i++)
        m[i][i] = origianl[i][i];
      return m;
    }
    public static MatrixOld Identity(int dim) {
      MatrixOld m = new MatrixOld(new int[] { dim, dim });
      for (int i = 0; i < dim; i++)
        m[i][i] = 1.0;
      return m;
    }
    public static MatrixOld Generate(int[] dims, double allSet) {
      MatrixOld m = new MatrixOld(dims);
      for (int i = 0; i < m.dimensions[0]; i++)
        for (int j = 0; j < m.dimensions[1]; j++)
          m[i][j] = allSet;
      return m;
    }
    public MatrixOld(int[] dims) {
      dimensions = dims;
      matrix = new double[dims[0]][];
      for (int i = 0; i < dims[0]; i++)
        matrix[i] = new double[dims[1]];
      matrixNew = new double[dimensions[0], dimensions[1]];
    }
    public MatrixOld(MatrixOld m) {
      dimensions = m.dimensions;
      matrix = new double[dimensions[0]][];
      for (int i = 0; i < dimensions[0]; i++)
        matrix[i] = new double[dimensions[1]];
      matrixNew = new double[dimensions[0], dimensions[1]];

      for (int i = 0; i < dimensions[0]; i++)
        for (int j = 0; j < dimensions[1]; j++) {
          matrix[i][j] = m[i, j];
          matrixNew[i,j] = matrix[i][j];
        }
    }
    public MatrixOld(double[] vector) {
      dimensions = new int[] { vector.Count(), 1 };
      matrix = new double[dimensions[0]][];
      for (int i = 0; i < dimensions[0]; i++)
        matrix[i] = new double[dimensions[1]];
      matrixNew = new double[dimensions[0], dimensions[1]];
      for (int i = 0; i < vector.Count(); i++)
        matrix[i][0] = matrixNew[i,0] = vector[i];
    }

    static public MatrixOld operator *(MatrixOld m1, MatrixOld m2) {
      MatrixOld retValue = new MatrixOld(new int[] { m1.dimensions[0], m2.dimensions[1] });

      for (int i = 0; i < m1.dimensions[0]; i++) {
        for (int j = 0; j < m2.dimensions[1]; j++) {
          for (int k = 0; k < m1.dimensions[1]; k++) {
            retValue.matrix[i][j] += m1.matrix[i][k] * m2.matrix[k][j];
          }
        }
      }
      return retValue;
    }
    static public MatrixOld operator -(MatrixOld m1, MatrixOld m2) {
      MatrixOld retValue = new MatrixOld(new int[] { m1.dimensions[0], m2.dimensions[1] });

      for (int i = 0; i < m1.dimensions[0]; i++) {
        for (int j = 0; j < m1.dimensions[1]; j++) {
          retValue[i, j] = m1[i, j] - m2[i, j];
        }
      }
      return retValue;
    }
    static public double operator /(MatrixOld m1, double m2) {
      return m1[0, 0] / m2;
    }
    public static MatrixOld Sqrt(MatrixOld original) {
      MatrixOld ret = new MatrixOld(original);

      for (int i = 0; i < ret.dimensions[0]; i++)
        for (int j = 0; j < ret.dimensions[1]; j++)
          ret[i][j] = Math.Sqrt(ret[i][j]);
      return ret;
    }
    public MatrixOld Transpose() {
      MatrixOld transpose = new MatrixOld(new int[] { this.dimensions[1], this.dimensions[0] });

      for (int i = 0; i < this.dimensions[0]; i++) {
        for (int j = 0; j < this.dimensions[1]; j++) {
          transpose.matrix[j][i] = this.matrix[i][j];
        }
      }
      return transpose;
    }
    static public MatrixOld GaussSolve(MatrixOld A, MatrixOld b) {
      MatrixOld retValue = new MatrixOld(b);

      for (int i = 0; i < b.dimensions[0]; i++) {
        int maxIndex = getMaxElement(A, i);
        if (maxIndex != i) {
          swapGauss(A, i, maxIndex);
          swapGauss(retValue, 0, maxIndex);
        }
        gaussLine(A, retValue, i);
      }
      for (int i = b.dimensions[0] - 1; i >= 0; i--) {
        double sum = 0.0;
        for (int j = i + 1; j < b.dimensions[0]; j++)
          sum += A[i, j] * retValue[j, 0];
        retValue[i, 0] = (retValue[i, 0] - sum) / A[i, i];
      }
      return retValue;
    }
    static int getMaxElement(MatrixOld A, int index) {
      int maxIndex = index;
      for (int i = index; i < A.dimensions[0]; i++) {
        if (Math.Abs(A[maxIndex, index]) < Math.Abs(A[i, index]))
          maxIndex = i;
      }
      return maxIndex;
    }
    static void swapGauss(MatrixOld A, int index, int maxIndex) {
      double value = 0.0;
      for (int i = index; i < A.dimensions[1]; i++) {
        value = A[index, i];
        A[index, i] = A[maxIndex, i];
        A[maxIndex, i] = value;
      }
    }
    static void gaussLine(MatrixOld A, MatrixOld b, int index) {
      double value = 0.0;
      for (int i = index + 1; i < A.dimensions[1]; i++) {
        if (A[i, index] == 0) continue;
        value = A[i, index] / A[index, index];
        b[i, 0] -= b[index, 0] * value;
        for (int j = index; j < A.dimensions[1]; j++)
          A[i][j] -= A[index][j] * value;
      }
    }
  }

}
