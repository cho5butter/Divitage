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
    }
}
