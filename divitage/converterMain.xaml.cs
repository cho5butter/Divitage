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
            this.verifyMovieFile();
        }

        private void verifyMovieFile()
        {
            //パラメータ初期化
            this.fileCount = 0;
            int errCount = 0;
            //処理後ファイル数
            var afterProcessFileNames = new List<string>();
            string errorFileName = "";
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
                        errCount++;
                        errorFileName += ( "　(" + errCount + ") このファイル形式は非対応です\n　" + System.IO.Path.GetFileName(item)) +"\n";
                        continue;
                    }
                    afterProcessFileNames.Add(item);
                    MessageBox.Show("30");
                }
                catch (Exception err)
                {
                    //例外処理
                    errCount++;
                    errorFileName += ("　(" + errCount + ")" + err.Message + "\n　" + System.IO.Path.GetFileName(item)) + "\n";
                    continue;
                }


            }
            if(errorFileName != "")
            {
                //エラーが発生していた場合のアラート
                errorFileName =
                    errCount +
                    "個のファイルの分割が以下の理由でスキップされました\n" +
                    "--------------------------------------------------------------------\n" +
                    errorFileName +
                    "--------------------------------------------------------------------\n";
                MessageBox.Show(errorFileName, "以下のファイルの処理に失敗しました", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            this.fileCount = afterProcessFileNames.Count;
            if (this.fileCount == 0) return;//ファイルがない場合はここで処理中止 
            

        }
    }
}
