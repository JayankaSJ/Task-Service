using System;
using System.IO;
using System.Resources;
using System.Collections;

namespace Manager {
    public static class Resource {
        public static void Exract(string _name, Byte[] _resource) {
            string path = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, _name);
            if (!File.Exists(path)) {
                System.IO.File.WriteAllBytes(path, _resource);
            }
        }
    }
}
