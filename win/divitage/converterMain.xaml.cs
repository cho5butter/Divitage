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
using OpenCvSharp.Extensions;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows.Threading;

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
        MainWindow mw;         //メインウインドウ
        TaskbarIcon tbi;       //通知
        public converterMain()
        {
            InitializeComponent();
            this.mw = (MainWindow)App.Current.MainWindow;
            this.tbi = new TaskbarIcon();
            this.tbi.Icon = Properties.Resources.colorIcon;
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
                        errorFileName += ("　(" + errCount + ") このファイル形式は非対応です\n　" + System.IO.Path.GetFileName(item)) + "\n";
                        continue;
                    }
                    afterProcessFileNames.Add(item);
                }
                catch (Exception err)
                {
                    //例外処理
                    errCount++;
                    errorFileName += ("　(" + errCount + ")" + err.Message + "\n　" + System.IO.Path.GetFileName(item)) + "\n";
                    continue;
                }


            }
            if (errorFileName != "")
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
            this.startTransition();
            this.counter = 0;
            foreach (string item in this.fileNames)
            {
                this.DoEvents();
                this.splitMovie(item);
            }
            //画面遷移終了
            this.endTransiton();
            if (this.counter != 0) this.tbi.ShowBalloonTip("変換が完了しました", this.counter + "個のファイルの分割が正常に完了しました", Properties.Resources.colorIcon, true);
        }

        private void splitMovie(string item)
        {
            //格納用フォルダ生成
            string folderPath = this.makeFolder(item);
            //拡張子保存
            string extension = this.getExtension();
            //画像の出力枚数
            int tmpCounter = 0;
            try
            {
                //動画ファイル分割
                VideoCapture vcap = new VideoCapture(item);
                for (int pos = 0; pos < vcap.FrameCount; pos++)
                {
                    bool use = isThisUsingFrame(pos, vcap.FrameCount);
                    if (!use) continue;//設定範囲外のときは処理を実行しない
                    Mat frame = new Mat();
                    vcap.PosFrames = pos;
                    vcap.Read(frame);
                    if (tmpCounter == 0 && Properties.Settings.Default.settingCheckBeforeConvert)
                    {
                        bool doSplit = this.showConfirmDialog(frame, item, vcap.FrameCount);
                        if(!doSplit)
                        {
                            this.deleteFolder(folderPath);
                            this.endTransiton();
                            return;
                        }
                    }
                    frame.SaveImage(string.Format("{0}/{1}.{2}", folderPath, pos + 1, extension));
                    frame.Dispose();
                    float fileProgress = ((float)(pos + 1) / vcap.FrameCount) * 100;
                    float progressPercent = fileProgress * ((float)(this.counter + 1) / this.fileCount);
                    tmpCounter++;
                    if (fileProgress % 1 != 0) continue;//処理軽減
                    this.discText.Text = string.Format("処理が{0}％完了しました．現在({1}/{2})のファイルを処理中", progressPercent, (this.counter + 1), this.fileCount);
                    mw.progressBar.Value = progressPercent;
                    this.DoEvents();
                }
                vcap.Dispose();
                if(tmpCounter == 0)
                {
                    //すべてが設定範囲外だった場合だよん
                    this.tbi.ShowBalloonTip("以下のファイルの処理がスキップされました", string.Format("設定項目の条件に満たさなかったため，「{0}」の処理は行われませんでした．", System.IO.Path.GetFileNameWithoutExtension(item)), BalloonIcon.Warning);
                    deleteFolder(folderPath);
                }
                else
                {
                    this.counter++;
                }
                mw.progressBar.Value = 100;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                MessageBox.Show("以下のエラーが発生したため処理を中止しました\n" + e.Message, "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                //途中フォルダ削除
                this.deleteFolder(folderPath);
                this.endTransiton();
                return;
            }
        }

        private bool showConfirmDialog(Mat frame, string path, int num)
        {
            //確認画面表示オプション
            confirmDialog cd = new confirmDialog(frame, path, num);
            cd.ShowDialog();
            bool doSplit = cd.Result;
            cd.Close();
            return doSplit;
        }
        private void startTransition()
        {
            //開始時の画面遷移
            this.topIcon.Visibility = Visibility.Collapsed;
            this.topLoading.Visibility = Visibility.Visible;
            this.processStart.Visibility = Visibility.Collapsed;
            this.discText.Text = "現在分割中です．完了までお待ち下さい．";
            this.Grid.AllowDrop = false;

        }
        private void endTransiton()
        {
            //終了時の画面遷移＆各パラメータリセット
            this.topIcon.Visibility = Visibility.Visible;
            this.topLoading.Visibility = Visibility.Collapsed;
            this.processStart.Visibility = Visibility.Visible;
            this.discText.Text = "動画ファイルをここにドラッグ＆ドロップするか以下のボタンからファイルを選択して下さい";
            this.Grid.AllowDrop = true;
        }

        private string makeFolder(string path)
        {
            DateTime dt = DateTime.Now;
            //MessageBox.Show(dt.ToString("HHmm"));
            string date = dt.ToString("yyyyMMdd");
            string time = dt.ToString("HHmm");
            System.Guid g = System.Guid.NewGuid();
            string random = g.ToString().Substring(0, 8);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);

            string folderName;
            //変換後ファイル格納用のフォルダ作成
            switch (Properties.Settings.Default.settingNameConvention)
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
            if (Properties.Settings.Default.settingSavePathOption == 0)
            {
                folderName = System.IO.Directory.GetParent(path) + "/" + folderName;
            }
            else
            {
                folderName = Properties.Settings.Default.settingSavePath + "/" + folderName;
            }
            //フォルダー生成
            try
            {
                System.IO.Directory.CreateDirectory(folderName);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }
            return folderName;
        }

        private void deleteFolder(string path)
        {
            //フォルダの削除
            try
            {
                System.IO.Directory.Delete(path);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

        }

        private string getExtension()
        {
            string extension;
            switch (Properties.Settings.Default.settingImageSplitExtension)
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

        private void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            var callback = new DispatcherOperationCallback(ExitFrames);
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, callback, frame);
            Dispatcher.PushFrame(frame);
        }

        private object ExitFrames(object obj)
        {
            ((DispatcherFrame)obj).Continue = false;
            return null;
        }

        private bool isThisUsingFrame(int frame, int maxframe)
        {
            bool isUse = true;
            if(Properties.Settings.Default.settingInterval == 0)
            {
                //枚数ごとに分割
                if(frame % (int)Properties.Settings.Default.settingSplitFrameInterval == 0)
                {
                    isUse = true;
                }
                else
                {
                    isUse = false; ;
                }
            }
            else
            {
                //％ごとに分割
                int interval = Properties.Settings.Default.settingSplitFrameInterval;
                if(interval > 100)
                {
                    interval = 100;
                }
                int intFrame = (int)((maxframe / 100.0) * interval);
                intFrame = intFrame == 0 ? 1 : intFrame;//0の場合は強制格上げ
                if(frame % intFrame == 0)
                {
                    isUse = true;
                }
                else
                {
                    isUse = false;
                }
            }
            if(isUse && Properties.Settings.Default.settingStartOrEndFrame)
            {
                if (
                    1 + frame < Properties.Settings.Default.settingStartFrame ||
                    Properties.Settings.Default.settingEndFrame < 1 + frame
                    ) isUse = false;
            }
            return isUse;
        }
    }
}
