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
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenCvSharp;

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
            //ドロップファイル選択
            this.fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            this.verifyMovieFile();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ファイル選択ボタン
            CommonOpenFileDialog copd = new CommonOpenFileDialog("動画ファイルを選択して下さい");
            copd.Multiselect = true;
            CommonFileDialogResult ret = copd.ShowDialog();
            if (ret != CommonFileDialogResult.Ok) return;
            this.fileNames = copd.FileNames.ToArray();
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
            this.convertProgress();

        }

        private void convertProgress()
        {
            //画面遷移
            this.counter = 0;
            foreach(string item in this.fileNames)
            {
                this.splitMovie(item);
                this.counter++;
            }
            //画面遷移終了
        }

        private void splitMovie(string item)
        {
            //格納用フォルダ生成
            string folderPath = this.makeFolder(item);
            //拡張子保存
            string extension = this.getExtension();
            try
            {
                //動画ファイル分割
                VideoCapture vcap = new VideoCapture(item);
                for (int pos = 0; pos < vcap.FrameCount; pos++)
                {
                    Mat frame = new Mat();
                    vcap.PosFrames = pos;
                    vcap.Read(frame);
                    if (this.counter == 0 && pos == 0) this.showConfirmDialog(frame, item);
                    frame.SaveImage(string.Format("{0}/{1}.{2}", folderPath, pos + 1, extension));
                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
                MessageBox.Show("以下のエラーが発生したため処理を中止しました\n" + e.Message , "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                //途中フォルダ削除
                this.deleteFolder(folderPath);
                this.endTransiton();
                return;
            }

        }

        private void showConfirmDialog(Mat frame, string path)
        {
            //確認画面表示オプション

        }
        private void startTransition()
        {
            //開始時の画面遷移
        }
        private void endTransiton()
        {
            //終了時の画面遷移＆各パラメータリセット
        }

        private string makeFolder(string path)
        {
            DateTime dt = DateTime.Today;
            string date = dt.ToString("yyyyMMdd");
            string time = dt.ToString("HHmm");
            System.Guid g = System.Guid.NewGuid();
            string random = g.ToString().Substring(0, 8);
            string fileName = System.IO.Path.GetFileName(path);

            string folderName;
            //変換後ファイル格納用のフォルダ作成
            switch(Properties.Settings.Default.settingNameConvention)
            {
                case 0:
                    //元ファイル名そのまま
                    folderName = fileName;
                    break;
                case 1:
                    //日付+元ファイル名
                    folderName = date + "_" + fileName;
                    break;
                case 2:
                    //時間+元ファイル名
                    folderName = time + "_" + fileName;
                    break;
                case 3:
                    //日付+時間+元ファイル名
                    folderName = date + "_" + time + "_" + fileName;
                    break;
                default:
                    //ランダム文字列+元ファイル名
                    folderName = random + "_" + fileName;
                    break;
            }
            //ファイル保存場所
            if(Properties.Settings.Default.settingSavePathOption == 1)
            {
                folderName = Properties.Settings.Default.settingSavePath + folderName;
            }
            //フォルダー生成
            System.IO.Directory.CreateDirectory(folderName);
            return folderName;
        }

        private void deleteFolder(string path)
        {
            //フォルダの削除

        }

        private string getExtension()
        {
            string extension;
            switch(Properties.Settings.Default.settingImageSplitExtension)
            {
                case 0:
                    extension = "jpg";
                    break;
                case 1:
                    extension = "bmp";
                    break;
                case 2:
                    extension = "tif";
                    break;
                case 3:
                    extension = "png";
                    break;
                default:
                    extension = "gif";
                    break;
            }
            return extension;
        }
    }
}
