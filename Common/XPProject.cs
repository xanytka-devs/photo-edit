using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace XPhotos.Common {

    public class XPProject {

        public string Name { get; set; } //Name of project.
        public BitmapImage Bitmap { get; set; } //Image in project.
        public string Path { get; set; } //Path to project.
        public bool IsSafe { get; set; }  //Check if all changes
                                          //in project is saved.
        public bool IsProjectBased { get; set; }  //Check if this is project
                                                  //and not just open image.
        public XPProject(string name, BitmapImage bitmap, string path) {
            Name = name;
            Bitmap = bitmap;
            Path = path;
        }

    }

    public class XPProjectManager { 

        /// <summary>
        /// Will save project at given path.
        /// </summary>
        /// <param name="path">Path to project.</param>
        /// <param name="prj">Project data.</param>
        /// <param name="ms">Memory string with image data.</param>
        /// <returns>Boolean, indecating success.</returns>
        public static bool SaveAsPrj(string path, XPProject prj, MemoryStream ms) {
            try {
                //Change safety internaly.
                prj.IsSafe = true;
                prj.IsProjectBased = true;
                prj.Path = path;
                //Assign directories.
                var dM = path.Split('.')[0] + ".xphotoprg.tmp";
                var dP = Path.Combine(dM, "main.png");
                if (!Directory.Exists(dM)) Directory.CreateDirectory(dM);
                //Save image.
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(prj.Bitmap));
                encoder.Save(ms);
                File.WriteAllBytes(dP, ms.ToArray());
                //Save project data.
                File.WriteAllText(Path.Combine(dM, "data.xppdf"), FormatIntoXPPDF(prj));
                ZipFile.CreateFromDirectory(dM, path);
                Directory.Delete(dM, true);
                return true;
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                Logger.Log($"Failed to save project in {path} as {prj.Name}");
                Logger.PushLog();
                return false;
            }
        }

        /// <summary>
        /// Will open project.
        /// </summary>
        /// <param name="path">Path to .xphotoprg</param>
        /// <returns>Project data.</returns>
        public static XPProject OpenPrj(string path) {
            try {
                //Open project file.
                string tmpDir = path.Split('.')[0] + ".xphotoprg.tmp";
                ZipFile.ExtractToDirectory(path, tmpDir);
                //Return project
                return ReadFromXPPDF(Path.Combine(tmpDir, "data.xppdf"));
            }
            catch (Exception ex) {
                Debug.WriteLine(ex);
                Logger.Log($"Failed to open project in {path}");
                Logger.PushLog();
                return null;
            }

        }

        static string FormatIntoXPPDF(XPProject project) {
            string output = string.Empty;
            Random rnd = new Random();
            output += $"XPhotosProject {rnd.Next()}\n";
            output += "{ \n";
            output += $" ProjectToken: {project.Path} \n";
            output += $" Path: {project.Path} \n";
            output += "}";
            return output;
        }

        static XPProject ReadFromXPPDF(string path) {
            if(!File.Exists(path)) return null;
            var data = File.ReadAllText(path).Split('\n');
            var _name = data[0].Split(' ')[1];
            var _path = data[3].Split(' ')[1];
            var pathToBM = path.Split('/')[path.Split('/').Length - 2];
            return new XPProject(_name, GetBitmap(Path.Combine(pathToBM, "main.png")), _path);
        }

        public static BitmapImage GetBitmap(string filePath) {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.CreateOptions = BitmapCreateOptions.None;
            bi.CacheOption = BitmapCacheOption.Default;
            bi.StreamSource = new MemoryStream(File.ReadAllBytes(filePath));
            bi.EndInit();
            return bi;
        }

    }

}
