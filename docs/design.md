# Divitage 設計書

## 1. アーキテクチャ概要
- Windows 版: WinUI 3 + Windows App SDK。`Window` に `NavigationView` を配置し、各 `Page` (Converter / Settings / HowTo) を `Frame` へナビゲーションする。
- macOS 版: SwiftUI + ObservableObject による単一ウィンドウアプリ。`NavigationSplitView` でコンバーター／設定／ヘルプを切り替え、`AppState` が共有状態を管理。
- Linux 版: Qt 6 Widgets (C++20)。`QMainWindow` + `QStackedWidget` 構成で `AppState` (QObject) がシグナル／スロットで UI を更新し、`QThread` ワーカーでファイルコピーを実行する。
- 共通ロジック: 変換キュー生成・ログ蓄積インターフェースを揃え、将来的に共有ライブラリ化を想定。

## 2. コンポーネント
```mermaid
graph LR
    subgraph Windows (WinUI 3)
        WMW[MainWindow]
        WCV[ConverterPage]
        WST[SettingsPage]
        WHT[HowToPage]
    end
    subgraph macOS
        APP[DivitageMacApp]
        AS[AppState]
        CVM[ConverterPanelView]
        SET[SettingsView]
        HOW[HowToUseView]
        CMD[AppCommands]
    end
    subgraph Linux
        LAPP[DivitageLinuxApp]
        LAS[AppState(Qt/C++)]
        LCV[ConverterPanel(QWidget)]
        LSET[SettingsPanel(QWidget)]
        LHOW[HowToUsePanel(QWidget)]
    end
    subgraph Docs
        REQ[requirements.md]
        DSG[design.md]
    end
    WMW --> WCV
    WMW --> WST
    WMW --> WHT
    APP --> AS
    APP --> CMD
    AS --> CVM
    AS --> SET
    AS --> HOW
    LAPP --> LAS
    LAPP --> LCV
    LAPP --> LSET
    LAPP --> LHOW
    LAS --> LCV
    LAS --> LSET
    LAS --> LHOW
    Docs -->|参照| AS
    Docs -->|参照| LAS
```
- `AppState`: 変換タスク、ログ、UI 選択状態を単一で保持。
- `ConverterPanelView` / `ConverterPage` / `ConverterPanel(QWidget)`: 各 OS のコンバーター UI。ドラッグ＆ドロップ、出力先選択、ログ表示を担う。
- `AppState(Qt/C++)`: Linux 版。XDG Config への設定永続化と `QThread` ベースの疑似変換処理を担当。
- `AppCommands` / `NavigationView`: OS ごとのショートカットやナビゲーションを司るコンポーネント。

## 3. データフロー
1. 入力 (D&D or ファイルダイアログ) → `ConversionTask` 生成。
2. `AppState.runConversion` / `AppState.startProcessing` (Linux) / WinUI の擬似ジョブが非同期タスクを起動し、UI をロックしながらログを更新。
3. 完了後にログへ成功メッセージとタイムスタンプを追加。

## 4. UI/UX 指針
- macOS 版は `NavigationSplitView` と標準ツールバーを採用し、フルスクリーン／サイズ変更を macOS のヒューマンインターフェイスガイドラインに従って許可。
- コンバータパネルはアクセントカラー連動のボタンと、トラックパッド操作向けの大型ドロップゾーンを設置。
- ログは `ScrollView + LazyVStack` でパフォーマンスを担保しつつ、モノスペースフォントで視認性を確保。

## 5. エラーハンドリング方針
- 入力不可ファイルはキューに追加せず UI にトースト表示 (今後実装予定)。
- 例外は `AppState` から `lastConversionLog` にエラーメッセージとして流す。

## 6. ビルド / デプロイ
- Windows: `win/DivitageWinUI/DivitageWinUI.sln` を Visual Studio 2022 で開き、Windows App SDK 1.5 + Windows 11 SDK 22621 でビルド。
- macOS: `cd mac/DivitageMacApp && swift build` で CLI ビルド、または Xcode で SPM プロジェクトを開き `.app` を署名して配布。
- Linux: `cd linux/DivitageLinuxApp && cmake -S . -B build && cmake --build build && ./build/DivitageLinuxApp`。

## 7. 今後の拡張
- 変換エンジンを共通 CLI として切り出し、両プラットフォームから呼び出す。
- iCloud Drive / OneDrive 連携による出力同期。
- 自動テスト (XCTest, UIAutomation) の追加。
