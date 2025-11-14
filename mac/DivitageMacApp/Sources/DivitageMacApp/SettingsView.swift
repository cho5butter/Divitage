import SwiftUI

struct SettingsView: View {
    @EnvironmentObject private var appState: AppState
    @AppStorage("launchAtLogin") private var launchAtLogin = false
    @State private var autoCleanup = true

    var body: some View {
        Form {
            Section("一般") {
                Toggle("ログイン時に起動", isOn: $launchAtLogin)
                Toggle("処理後に一時ファイルを削除", isOn: $autoCleanup)
                Picker("テーマ", selection: .constant(AppTheme.system)) {
                    ForEach(AppTheme.allCases) { theme in
                        Text(theme.label).tag(theme)
                    }
                }
                .labelsHidden()
            }

            Section("出力") {
                LabeledContent("保存先") {
                    Text(appState.outputDirectory?.path(percentEncoded: false) ?? "未設定")
                        .lineLimit(2)
                }
                Button("Finder で表示") {
                    if let url = appState.outputDirectory {
                        NSWorkspace.shared.open(url)
                    }
                }
                .disabled(appState.outputDirectory == nil)
            }
        }
        .padding(32)
        .frame(maxWidth: 600)
    }
}

enum AppTheme: String, CaseIterable, Identifiable {
    case system
    case light
    case dark

    var id: String { rawValue }

    var label: String {
        switch self {
        case .system: return "システムに合わせる"
        case .light: return "ライト"
        case .dark: return "ダーク"
        }
    }
}
