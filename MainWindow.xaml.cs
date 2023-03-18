using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using XPhotos.Common;

namespace XPhotos {

    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window {

        readonly List<XPProject> openPrgs = new List<XPProject>();
        readonly List<TabItem> tabs = new List<TabItem>();
        readonly List<string> _n = new List<string>();
        XPProject cur;

        public MainWindow() {
            InitializeComponent();
        }

        private void ContextButton_Click(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement btn)
                btn.ContextMenu.IsOpen = true;
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog Dlg = new OpenFileDialog {
                Title = "Select image",
                Filter = "Any (*.*)|*.*|XPhotos project (*.xphotoprg)|*.xphotoprg"
            };
            if (Dlg.ShowDialog() == true) {
                for (int i = 0; i < Dlg.FileNames.Length; i++) {
                    OpenPrj(Dlg.FileNames[i]);
                }
            }

        }

        void CreateNewTab(XPProject project) {

            BitmapImage bitmap = project.Bitmap;
            Image img = new Image() {
                Source = bitmap
            };
            Canvas can = new Canvas();
            can.Children.Add(img);
            Viewbox viewbox = new Viewbox() {
                Child = can
            };

            ContextMenu menu = new ContextMenu();
            MenuItem i1 = new MenuItem {
                Header = "Close"
            };
            MenuItem i2 = new MenuItem {
                Header = "Save"
            };
            MenuItem i3 = new MenuItem {
                Header = "Save as..."
            };
            i1.Click += (object sender, RoutedEventArgs e) => {
                var prj = openPrgs[prjView.SelectedIndex];
                if(prj.IsSafe)
                    CloseTab(openPrgs.IndexOf(prj));
                else {
                    var msg = MessageBox.Show("File isn't saved. Do you want to continue?", "XPhotos", MessageBoxButton.YesNoCancel, MessageBoxImage.Stop, MessageBoxResult.Cancel);
                    if (msg == MessageBoxResult.Yes)
                        CloseTab(openPrgs.IndexOf(prj));
                    else if (msg == MessageBoxResult.No)
                        SaveCall(project, true);
                    else return;
                }
            };
            i2.Click += (object sender, RoutedEventArgs e) => {
                if (string.IsNullOrEmpty(project.Path)) {
                    SaveFileDialog fileDialog = new SaveFileDialog() {
                        Filter = "XPhotos project (*.xphotoprg)|*.xphotoprg"
                    };
                    if (fileDialog.ShowDialog() == true) {
                        XPProjectManager.SaveAsPrj(fileDialog.FileName, project, new MemoryStream());
                    }
                    return;
                }
                XPProjectManager.SaveAsPrj(project.Path, project, new MemoryStream());
            };
            i3.Click += (object sender, RoutedEventArgs e) => { 
                SaveCall(project);
            };
            menu.Items.Add(i1);
            menu.Items.Add(i2);
            menu.Items.Add(i3);

            TabItem newTab = new TabItem() {
                Header = project.Name,
                Content = viewbox,
                DataContext = project,
                ContextMenu = menu
            };
            newTab.MouseRightButtonDown += (object sender, MouseButtonEventArgs e) => {
                newTab.ContextMenu.IsOpen = true;
            };
            newTab.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) => {
                cur = openPrgs[openPrgs.IndexOf(newTab.DataContext as XPProject)];
            };

            prjView.Items.Add(newTab);
            tabs.Add(newTab);
            prjView.SelectedItem = newTab;

        }

        void SaveCall(XPProject project, bool closeAfter=false) {
            SaveFileDialog fileDialog = new SaveFileDialog() {
                Filter = "XPhotos project (*.xphotoprg)|*.xphotoprg|JPG file (*.jpg)|.jpg|JPEG file (*.jpeg)|.jpeg|PNG file (*.png)|.png"
            };
            if (fileDialog.ShowDialog() == true) {
                switch (fileDialog.FilterIndex) {
                    case 0:
                        XPProjectManager.SaveAsPrj(fileDialog.FileName, project, new MemoryStream());
                        break;
                    case 1:
                        SaveAs(MediaType.JPG, project, fileDialog.FileName);
                        break;
                    case 2:
                        SaveAs(MediaType.JPEG, project, fileDialog.FileName);
                        break;
                    case 3:
                        SaveAs(MediaType.PNG, project, fileDialog.FileName);
                        break;
                }
            }
            if (closeAfter) CloseTab(openPrgs.IndexOf(project));
        }

        void CloseTab(int index) {
            //Remove from all lists.
            prjView.Items.Remove(tabs[index]);
            openPrgs.Remove(tabs[index].DataContext as XPProject);
            tabs.Remove(tabs[index]);
        }

        void UpdateTab(int index, string name) => tabs[index].Header = name;
        void ChangeSafeValueOfTab(int index, bool isSafe) {
            switch (isSafe) {
                case false:
                    tabs[index].Header = tabs[index].Header + " *";
                    break;
                case true:
                    tabs[index].Header = _n[index];
                    break;
            }
        }

        private void ForcePrjOpen(object sender, DragEventArgs e) {
            //Note that you can have more than one file.
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            //Open as projects.
            for (int i = 0; i < files.Length; i++) {
                OpenPrj(files[i]);
            }
        }

        void OpenPrj(string filePath) {
            XPProject project = new XPProject(filePath.Split('\\')[filePath.Split('\\').Length - 1], XPProjectManager.GetBitmap(filePath), filePath);
            openPrgs.Add(project);
            _n.Add(project.Name);
            CreateNewTab(project);
        }

        void SaveAs(MediaType type, XPProject project, string path) {

            switch (type) {
                case MediaType.Project:
                    break;
                case MediaType.JPG:
                    JpegBitmapEncoder jpgEnc = new JpegBitmapEncoder();
                    jpgEnc.Frames.Add(BitmapFrame.Create(project.Bitmap));
                    using (MemoryStream ms = new MemoryStream()) {
                        jpgEnc.Save(ms);
                        File.WriteAllBytes(path, ms.ToArray());
                        ChangeSafeValueOfTab(openPrgs.IndexOf(project), true);
                    }
                    break;
                case MediaType.JPEG:
                    JpegBitmapEncoder jpegEnc = new JpegBitmapEncoder();
                    jpegEnc.Frames.Add(BitmapFrame.Create(project.Bitmap));
                    using (MemoryStream ms = new MemoryStream()) {
                        jpegEnc.Save(ms);
                        File.WriteAllBytes(path, ms.ToArray());
                        ChangeSafeValueOfTab(openPrgs.IndexOf(project), true);
                    }
                    break;
                case MediaType.PNG:
                    PngBitmapEncoder pngEnc = new PngBitmapEncoder();
                    pngEnc.Frames.Add(BitmapFrame.Create(project.Bitmap));
                    using (MemoryStream ms = new MemoryStream()) {
                        pngEnc.Save(ms);
                        File.WriteAllBytes(path, ms.ToArray());
                        ChangeSafeValueOfTab(openPrgs.IndexOf(project), true);
                    }
                    break;
            }

        }

    }

}
