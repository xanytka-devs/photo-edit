using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace XPhotos {

    public partial class Styles : ResourceDictionary {

        void CloseBtn_Click(object sender, RoutedEventArgs e) {
            var win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.Close();
        }

        void RestoreBtn_Click(object sender, RoutedEventArgs e) {

            var win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.WindowState = (win.WindowState == WindowState.Normal) ?
                win.WindowState = WindowState.Maximized :
                win.WindowState = WindowState.Normal;

        }

        void MinimizeBtn_Click(object sender, RoutedEventArgs e) {

            var win = (Window)((FrameworkElement)sender).TemplatedParent;
            win.WindowState = WindowState.Minimized;

        }

    }
}
