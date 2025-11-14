# Divitage

Divitage はメディアファイルをドラッグ＆ドロップでバッチ変換するデスクトップアプリです。Windows (WinUI 3)、macOS (SwiftUI)、Linux (Qt 6) の 3 プラットフォーム実装を同居させ、共通ドキュメントを `docs/` にまとめています。

## ディレクトリ構成

- `win/divitage`: 旧来の WPF 版 (参考用)。
- `win/DivitageWinUI`: Windows 11 以降向け WinUI 3 実装。NavigationView ベースでモダンな UI を備えます。
- `mac/DivitageMacApp`: SwiftUI 製 macOS 版。Swift Package Manager でビルドできます。
- `linux/DivitageLinuxApp`: Qt 6 (C++20) 製 Linux 版。CMake でビルドします。
- `docs/`: 要件定義書 (`requirements.md`) と設計書 (`design.md`)。Mermaid 図を含みます。

## セットアップ手順 (Windows / WinUI 3)

1. Windows 11 以降 + Visual Studio 2022 (17.8+) を用意します。
2. `win/DivitageWinUI/DivitageWinUI.sln` を開き、必要な Windows App SDK / Windows SDK を復元します。
3. `DivitageWinUI` プロジェクトを起動プロジェクトに設定し、任意の構成 (Debug/Release) で実行します。

## セットアップ手順 (macOS)

1. Xcode 15 以降、または CLI で `cd mac/DivitageMacApp && swift build` を実行します。
2. Xcode の場合は `Package.swift` を開き、`DivitageMacApp` を実行ターゲットに設定します。
3. 初回起動時にドラッグ＆ドロップと出力先アクセスの権限を許可してください。

## セットアップ手順 (Linux / Qt 6)

1. Qt 6.5+ と CMake 3.21+、C++20 コンパイラを用意します。
2. `cd linux/DivitageLinuxApp && cmake -S . -B build -DCMAKE_BUILD_TYPE=Release` を実行します。
3. `cmake --build build` でビルド後、`./build/DivitageLinuxApp` を起動します。
4. 左ナビゲーションから「コンバーター」「設定」「使い方」を切り替え、`Ctrl+Enter` で変換キューへ投入できます。

## 備考

- 新しく追加するコードは各プラットフォームのディレクトリ配下に配置し、共通仕様は `docs/` に追記してください。
- `.git` ディレクトリと `.gitignore` はルートで管理されています。
