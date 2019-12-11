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
using System.Windows.Shapes;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace divitage
{
    /// <summary>
    /// confirmDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class confirmDialog : System.Windows.Window
    {
        private Mat m_frame;
        private string m_filePath;
        private int m_num;
        public confirmDialog(Mat frame, string item,int num)
        {
            InitializeComponent();
            this.m_frame = frame;
            this.m_filePath = item;
            this.m_num = num;
            this.showInfo();
        }

        private void showInfo()
        {
            //ファイル名の取得
            this.fileName.Text = System.IO.Path.GetFileName(this.m_filePath);
            //ファイル容量
            System.IO.FileInfo fi = new System.IO.FileInfo(this.m_filePath);
            this.fileSize.Text = (fi.Length / 1024.0 / 1024.0).ToString("F2") + "MB";
            //総フレーム数
            this.frameNumber.Text = this.m_num.ToString();
            //画像を表示
            this.previewImage.Source = this.m_frame.ToWriteableBitmap();

        }

        private void StartConvertButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //どこでもドラッグができるようにする
            if (e.ButtonState != MouseButtonState.Pressed) return;

            this.DragMove();
        }
    }
}
