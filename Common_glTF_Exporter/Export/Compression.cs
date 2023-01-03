﻿using Common_glTF_Exporter.Windows.MainWindow;
using System.Collections.Generic;
using System.IO;

namespace Common_glTF_Exporter.Export
{
    public class Compression
    {
        public static void Run(Preferences preferences)
        {
            if (preferences.compression.Equals(CompressionEnum.ZIP))
            {
                string gltfFile = string.Concat(preferences.path, "gltf");
                string binFile = string.Concat(preferences.path, "bin");
                string zipFile = string.Concat(preferences.path, "zip");
                List<string> files = new List<string> { gltfFile, binFile };

                ZIP.compress(zipFile, files);
                files.ForEach(x => File.Delete(x));
            }
        }
    }
}
