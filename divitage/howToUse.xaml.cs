using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace divitage
{
    /// <summary>
    /// howToUse.xaml の相互作用ロジック
    /// </summary>
    public partial class howToUse : Page
    {
        private
            MainWindow mainWindow;
        public howToUse()
        {
            InitializeComponent();
            this.mainWindow = (MainWindow)App.Current.MainWindow;
        }

        private void returnButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.frame.Source = new Uri("converterMain.xaml", UriKind.Relative);
        }

        private void Image3_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //メール
            System.Diagnostics.Process.Start("https://c5bt.net/contact");
        }

        private void Image2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ブラウザ
            System.Diagnostics.Process.Start("https://c5bt.net/");
        }

        private void Image4_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Twitter
            System.Diagnostics.Process.Start("https://twitter.com/__cho__");
        }

        private void Image5_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Youtube
            System.Diagnostics.Process.Start("https://www.youtube.com/user/akutore");

        }
    }
}
