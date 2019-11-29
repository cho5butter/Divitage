﻿using System;
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


namespace divitage
{
    /// <summary>
    /// setting.xaml の相互作用ロジック
    /// </summary>
    public partial class setting : Page
    {
        private
            MainWindow mainWindow;
        public setting()
        {
            InitializeComponent();
            this.mainWindow = (MainWindow)App.Current.MainWindow;
            this.loadSettingData();
        }

        private void loadSettingData()
        {
            this.settingSaveFolderPath.Text = Properties.Settings.Default.settingSavePath;
            int savePathOption = Properties.Settings.Default.settingSavePathOption;
            if(savePathOption == 0)
            {
                this.settingSaveSameDirectory.IsChecked = true;
            }
            else
            {
                this.settingSaveSpecifiedDirectory.IsChecked = true;
            }
            this.settingCheckBeforeConvert.IsChecked = Properties.Settings.Default.settingCheckBeforeConvert;
            int fileExtension = Properties.Settings.Default.settingImageSplitExtension;
            if(fileExtension == 0)
            {
                this.settingExtentionJPG.IsChecked = true;
            }
            else if(fileExtension == 1)
            {
                this.settingExtensionBMP.IsChecked = true;
            }
            else if(fileExtension == 2)
            {
                this.settingExtensionTIF.IsChecked = true;
            }
            else
            {
                this.settingExtensionPNG.IsChecked = true;
            }
            int nameConvention = Properties.Settings.Default.settingNameConvention;
            if(nameConvention == 0)
            {
                this.settingOriginalFilesName.IsChecked = true;
            }
            else if(nameConvention == 1)
            {
                this.settingDatePlusOriginalFilesName.IsChecked = true;
            }
            else if(nameConvention == 2)
            {
                this.settingTimePlusOriginalFilesName.IsChecked = true;
            }
            else if(nameConvention == 3)
            {
                this.settingDatePlusTimePlusOriginalFilesName.IsChecked = true;
            }
            else
            {
                this.settingRandomPlusOriginalFilesName.IsChecked = true;
            }
            this.SettingfileSplitInterval.Text = Properties.Settings.Default.settingSplitFrameInterval.ToString();
        }
        private void returnButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.frame.Source = new Uri("converterMain.xaml", UriKind.Relative);
            
        }

        private void DecideSavePath_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog copd = new CommonOpenFileDialog("保存先選択");
            copd.IsFolderPicker = true;
            CommonFileDialogResult ret = copd.ShowDialog();
            if(ret == CommonFileDialogResult.Ok)
            {
                settingSaveFolderPath.Text = copd.FileName;
            }
            copd.Dispose();
        }

        /*
         * 設定変更画面
         */

        private void SettingSaveSameDirectory_Click(object sender, RoutedEventArgs e)
        {
            //保存箇所指定オプション（同フォルダ）
            Properties.Settings.Default.settingSavePathOption = 0;
            this.settingDefaultSave();
        }

        private void SettingSaveSpecifiedDirectory_Click(object sender, RoutedEventArgs e)
        {
            //保存箇所指定オプション（指定フォルダ）
            Properties.Settings.Default.settingSavePathOption = 1;
            this.settingDefaultSave();
        }

        private void SettingCheckBeforeConvert_Click(object sender, RoutedEventArgs e)
        {
            //変換前に確認する
            Properties.Settings.Default.settingCheckBeforeConvert = (bool)this.settingCheckBeforeConvert.IsChecked;
            this.settingDefaultSave();

        }

        private void SettingExtensionBMP_Click(object sender, RoutedEventArgs e)
        {
            //画像分割ファイル（BMP)
            Properties.Settings.Default.settingImageSplitExtension = 0;
            this.settingDefaultSave();
        }

        private void SettingExtensionTIF_Click(object sender, RoutedEventArgs e)
        {
            //画像分割ファイル（TIF）
            Properties.Settings.Default.settingImageSplitExtension = 1;
            this.settingDefaultSave();
        }

        private void SettingExtensionPNG_Click(object sender, RoutedEventArgs e)
        {
            //画像分割ファイル（PNG）
            Properties.Settings.Default.settingImageSplitExtension = 2;
            this.settingDefaultSave();
        }

        private void SettingExtentionJPG_Click(object sender, RoutedEventArgs e)
        {
            //画像分割ファイル（JPG）
            Properties.Settings.Default.settingImageSplitExtension = 3;
            this.settingDefaultSave();
        }

        private void SettingOriginalFilesName_Click(object sender, RoutedEventArgs e)
        {
            //命名規則変更（元ファイル名）
            Properties.Settings.Default.settingNameConvention = 0;
            this.settingDefaultSave();
        }

        private void SettingDatePlusOriginalFilesName_Click(object sender, RoutedEventArgs e)
        {
            //命名規則変更（日付+元ファイル名）
            Properties.Settings.Default.settingNameConvention = 1;
            this.settingDefaultSave();
        }

        private void SettingTimePlusOriginalFilesName_Click(object sender, RoutedEventArgs e)
        {
            //命名規則変更（時間+元ファイル名）
            Properties.Settings.Default.settingNameConvention = 2;
            this.settingDefaultSave();
        }

        private void SettingDatePlusTimePlusOriginalFilesName_Click(object sender, RoutedEventArgs e)
        {
            //命名規則変更（日付+時間+元ファイル名）
            Properties.Settings.Default.settingNameConvention = 3;
            this.settingDefaultSave();
        }

        private void SettingRandomPlusOriginalFilesName_Click(object sender, RoutedEventArgs e)
        {
            //命名規則変更（ランダム文字+元ファイル名）
            Properties.Settings.Default.settingNameConvention = 4;
            this.settingDefaultSave();
        }

        private void SettingfileSplitInterval_LostFocus(object sender, RoutedEventArgs e)
        {
            //フレーム分割間隔
            Properties.Settings.Default.settingSplitFrameInterval = Convert.ToInt32(this.SettingfileSplitInterval.Text);
            this.settingDefaultSave();
        }

        private void SettingSplitBySpecifiedNum_Click(object sender, RoutedEventArgs e)
        {
            //指定された枚数ごとに分割（指定枚数）
            Properties.Settings.Default.settingInterval = 0;
            this.settingDefaultSave();
        }

        private void SettingSplitBySpecifiedPer_Click(object sender, RoutedEventArgs e)
        {
            //指定された枚数ごとに分割（指定％）
            Properties.Settings.Default.settingInterval = 1;
            this.settingDefaultSave();
        }

        private void SettingSpeficiedStartAndEndFrame_Click(object sender, RoutedEventArgs e)
        {
            //フレームの開始・終了を指定
            Properties.Settings.Default.settingStartOrEndFrame = (bool)this.settingSpeficiedStartAndEndFrame.IsChecked;
            if ((bool)this.settingSpeficiedStartAndEndFrame.IsChecked)
            {
                //開始・終了フレームボックスを有効
                this.settingStartFrame.IsEnabled = true;
                this.settingEndFrame.IsEnabled = true;
            } else
            {
                //開始・終了フレームボックスを無効
                this.settingStartFrame.IsEnabled = false;
                this.settingEndFrame.IsEnabled = false;
            }
            this.settingDefaultSave();
        }

        private void SettingStartFrame_LostFocus(object sender, RoutedEventArgs e)
        {
            //開始フレーム数
            Properties.Settings.Default.settingStartFrame = Convert.ToInt32(this.settingStartFrame.Text);
            this.settingDefaultSave();
        }

        private void SettingEndFrame_LostFocus(object sender, RoutedEventArgs e)
        {
            //終了フレーム数
            Properties.Settings.Default.settingEndFrame = Convert.ToInt32(this.settingEndFrame.Text);
            this.settingDefaultSave();
        }

        private void settingDefaultSave()
        {
            Properties.Settings.Default.Save();
        }
    }
}