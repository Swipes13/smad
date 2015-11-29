using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace smad5.src {
  public class Writer : IDisposable {
    StreamWriter _streamWriter = null;
    static Writer writer = null;
    private Writer(String filename) {
      _streamWriter = new StreamWriter(filename);
    }

    public static Writer Instance() {
      if (writer == null) writer = new Writer("out.txt");
      return writer;
    }
    public static Writer Open(string filename) {
      if (writer == null) writer = new Writer(filename);
      return writer;
    }
    public void Write(String st) {
      Console.Write(st);
      if (_streamWriter != null) _streamWriter.Write(st);
    }
    public void WriteToFile(String st) {
      if (_streamWriter != null) _streamWriter.Write(st);
    }
    public void Write(double value) {
      Console.Write(value.ToString() + Environment.NewLine);
      if (_streamWriter != null) _streamWriter.Write(value.ToString() + Environment.NewLine);
    }
    public void NewLine() {
      Console.Write(Environment.NewLine);
      if (_streamWriter != null) _streamWriter.Write(Environment.NewLine);
    }
    private void close() {
      if (_streamWriter != null) {
        _streamWriter.Close(); _streamWriter = null;
      };
    }
    public void Dispose() {
      close();
    }
  }
}
