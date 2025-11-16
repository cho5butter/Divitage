# Divitage for macOS (SwiftUI)

SwiftUI を用いた macOS ネイティブアプリケーション実装です。ドラッグ＆ドロップ対応のコンバーターパネル、設定パネル、使い方ガイドを 1 つのウィンドウにまとめています。

## 必要要件

- macOS 13.0 (Ventura) 以降
- Swift 5.9 以降
- Xcode 15.0 以降 (開発時)

## ビルド手順

Swift Package Managerを使用してビルドします:

```bash
cd mac/DivitageMacApp
swift build -c release
```

ビルドされた実行ファイルは `.build/release/DivitageMacApp` に出力されます。

## 実行

```bash
.build/release/DivitageMacApp
```

または、Xcodeで開いて実行することもできます:

```bash
open Package.swift
```

## テストの実行

```bash
cd mac/DivitageMacApp
swift test
```

詳細な出力でテストを実行する場合:

```bash
swift test --verbose
```

特定のテストを実行する場合:

```bash
swift test --filter AppStateTests
swift test --filter WorkspacePanelTests
swift test --filter ConversionTaskTests
```

## プロジェクト構造

```
DivitageMacApp/
├── Package.swift           # Swift Package Manager設定
├── Sources/
│   └── DivitageMacApp/
│       ├── DivitageMacApp.swift      # アプリエントリーポイント
│       ├── AppState.swift            # アプリケーション状態管理
│       ├── ContentView.swift         # メインビュー
│       ├── ConverterPanelView.swift  # コンバーターパネル
│       ├── SettingsView.swift        # 設定パネル
│       ├── HowToUseView.swift        # 使い方ガイド
│       └── AppCommands.swift         # メニューコマンド
└── Tests/
    └── DivitageMacAppTests/
        ├── AppStateTests.swift
        ├── WorkspacePanelTests.swift
        └── ConversionTaskTests.swift
```

## CI/CD

GitHub Actionsで自動的にビルドとテストが実行されます。ワークフローファイルは `.github/workflows/build-test-linux-mac.yml` を参照してください。

## 開発

### デバッグビルド

```bash
swift build
```

### クリーンビルド

```bash
swift package clean
swift build
```

### 依存関係の更新

```bash
swift package update
```
