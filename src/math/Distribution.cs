using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smad5.src.math {
  public class Distribution {
    private static Distribution instance = null;
    private Random random = new Random();

    private static void Initialize() {
      if (instance == null)
        instance = new Distribution();
    }
    private Distribution() { }
    public static double Flat(double mathPredict, double Dispersion) {
      Initialize();
      return mathPredict + instance.random.NextDouble() * Dispersion;
    }
    public static double Normal(double mathPredict, double Dispersion) {
      Initialize();
      return mathPredict + instance.random.NextGaussianDouble() * Dispersion;
    }

  }
}
