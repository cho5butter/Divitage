import SwiftUI

struct HowToUseView: View {
    private let steps: [String] = [
        "変換したいファイルまたはフォルダを用意します。",
        "Divitage for macOS のウィンドウにドラッグ＆ドロップします。",
        "右上の『変換開始』ボタンを押す、または ⌘⏎ を押します。",
        "完了通知を確認し、出力先フォルダを開きます。"
    ]

    var body: some View {
        ScrollView {
            VStack(alignment: .leading, spacing: 24) {
                Text("使い方")
                    .font(.largeTitle.bold())
                ForEach(Array(steps.enumerated()), id: \.offset) { index, step in
                    HStack(alignment: .top) {
                        Text("\(index + 1)")
                            .font(.system(size: 24, weight: .semibold))
                            .frame(width: 32, height: 32)
                            .background(Circle().fill(Color.accentColor.opacity(0.2)))
                        Text(step)
                            .font(.title3)
                    }
                }

                Divider()
                Text("ショートカット")
                    .font(.title2.bold())
                VStack(alignment: .leading, spacing: 12) {
                    ShortcutRow(label: "変換開始", keys: "⌘⏎")
                    ShortcutRow(label: "ヒントを表示", keys: "⌘H")
                }
            }
            .padding(32)
        }
    }
}

struct ShortcutRow: View {
    let label: String
    let keys: String

    var body: some View {
        HStack {
            Text(label)
            Spacer()
            Text(keys)
                .font(.system(.body, design: .monospaced))
                .padding(.horizontal, 8)
                .padding(.vertical, 4)
                .background(RoundedRectangle(cornerRadius: 4).strokeBorder(.secondary))
        }
    }
}
