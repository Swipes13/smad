using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smad1.src {
  public class Lab {
    protected static int DoubleRounder = 3;
    protected double[] Tetta = new double[] { 1.0, 2.0, 3.0, 4.0, 0.005, 0.006, 0.007, 8.0, 9.0 };
    protected int DegreesOfFreedom;

    protected static double step = 0.2;
    protected static double left = -1.0;
    protected static double right = 1.0;
    protected static int count = (int)((right - left) / step) + 1;
    protected static int countExperiments = (int)Math.Pow(count, 3.0);

    protected Random rand = new Random(10/*System.DateTime.Now.Millisecond*/);

    virtual protected double[] calcF(double[] x) {
      return null;
    }
    public virtual void GenerateData() { }
    public virtual void GenerateDataForLaba4() { }
    public virtual void Write() { }
    public virtual void SetNoize(double noize) { }
  }
}
