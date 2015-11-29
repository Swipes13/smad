using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using smad1.src;

namespace smad1 {
  class Program {
    static void Main(string[] args) {
      using (Writer.Open("output.txt")) {
        Lab5 laba = new Lab5();
        laba.GenerateData();
        //laba.Write();
      }
      Console.ReadKey();
    }
  }
}
