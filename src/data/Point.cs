using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smad5.src.data {
  public class Point {
    public Point(double[] point, double y) {
      X = new double[point.Count()];
      for (int i = 0; i < point.Count(); i++)
        X[i] = point[i];
      Y = y;
    }
    public double[] X;
    public double Y;
  }
}
