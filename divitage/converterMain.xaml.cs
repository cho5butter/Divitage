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
    /// converterMain.xaml の相互作用ロジック
    /// </summary>
    public partial class converterMain : Page
    {
        private string[] fileNames;
        private int fileCount; //全体のファイル数
        private int counter;   //ファイルのカウンター
        public converterMain()
        {
            InitializeComponent();
        }

        private void Grid_PreviewDragOver(object sender, DragEventArgs e)
        {
            //前処理
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }

        private void Grid_PreviewDrop(object sender, DragEventArgs e)
        {
            this.fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            MessageBox.Show(fileNames[0]);
            MessageBox.Show(fileNames[1]);
        }

        private void verifyMovieFile()
        {
            //パラメータ初期化
            this.fileCount = 0;
            //処理後ファイル数
            var afterProcessFileNames = new List<string>();
            var errorFileName = new List<string>();
            //対応ファイル拡張子
            string[] allowExtensions = { ".avi", ".mp4", ".mov", ".wmv", ".flv", ".mpg" };
            //拡張子調査・情報取得
            foreach (string item in this.fileNames)
            {
                try
                {
                    string tmpExtension = System.IO.Path.GetExtension(item);
                    if (0 > Array.IndexOf(allowExtensions, tmpExtension))
                    {
                        //対応拡張子
                        errorFileName.Add("非対応形式であるため，次のファイルが処理できませんでした\n" + System.IO.Path.GetFileName(item));
                        continue;
                    }
                    afterProcessFileNames.Add(item);
                    MessageBox.Show("30");
                }
                catch (Exception err)
                {
                    errorFileName.Add("「" + err.Message + "」のため，次のファイルが処理できませんでした\n" + System.IO.Path.GetFileName(item));
                    continue;
                }


            }
            this.fileCount = afterProcessFileNames.Count;
            MessageBox.Show(this.fileCount.ToString());

        }
    }
}
