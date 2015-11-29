using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smad5.src.math {
  static class RandomExtension {
    private static bool _haveNext = false;
    private static double _nextGauss = .0;

    public static double NextGaussianDouble(this Random rand) {
      if (_haveNext) {
        _haveNext = false;
        return _nextGauss;
      }
      else {
        double v1, v2, s;
        do {
          v1 = 2 * rand.NextDouble() - 1.0;
          v2 = 2 * rand.NextDouble() - 1.0;
          s = Math.Pow(v1, 2.0) + Math.Pow(v2, 2.0);
        } while (s >= 1 || s == 0);
        double mltp = Math.Sqrt(-2.0 * Math.Log(s) / s);
        _nextGauss = v2 * mltp;
        _haveNext = true;
        return v1 * mltp;
      }
    }
  }
}
