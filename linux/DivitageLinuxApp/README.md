# Divitage for Linux (Qt6)

Qt 6 + Widgets を用いた C++ 実装です。ドラッグ＆ドロップ対応のコンバーターパネル、設定パネル、使い方ガイドを 1 つのウィンドウにまとめています。

## 必要要件

- Qt 6.5 以降
- CMake 3.21+
- C++20 コンパイラ (GCC 11 / Clang 14 以降など)

## ビルド手順

```bash
cd linux/DivitageLinuxApp
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build
./build/DivitageLinuxApp
```

初回起動時は `Downloads/DivitageOutput` 以下に出力先を生成します。`Ctrl+Enter` で変換をキューに投入し、完了時はシステムトレイ通知を表示します。

## テストの実行

```bash
cd linux/DivitageLinuxApp
cmake -S . -B build -DCMAKE_BUILD_TYPE=Debug
cmake --build build
cd build
ctest --output-on-failure
```

個別のテストを実行する場合:

```bash
./build/tests/test_appstate
./build/tests/test_filepath
```

## CI/CD

GitHub Actionsで自動的にビルドとテストが実行されます。ワークフローファイルは `.github/workflows/build-test-linux-mac.yml` を参照してください。
