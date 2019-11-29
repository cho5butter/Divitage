using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace divitage
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //どこでもドラッグができるようにする
            if (e.ButtonState != MouseButtonState.Pressed) return;

            this.DragMove();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            //最小化ボタン処理
            this.WindowState = WindowState.Minimized;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            //閉じるボタン処理
            this.Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            //最大化禁止
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
        }

        private void settingButton_Click(object sender, RoutedEventArgs e)
        {
            //設定ページの推移
            frame.Source = new Uri("setting.xaml", UriKind.Relative);
        }

        private void howToUseButton_Click(object sender, RoutedEventArgs e)
        {
            //使い方ページの推移
            frame.Source = new Uri("howToUse.xaml", UriKind.Relative);

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Aero Snap時にボーダーを無くす
            Control c = (Control)sender;
            if (c.Height == SystemParameters.PrimaryScreenHeight || c.Height == SystemParameters.WorkArea.Height)
            {
                this.BorderThickness = new Thickness(0);
            } else
            {
                this.BorderThickness = new Thickness(20);
            }
        }
    }
}
