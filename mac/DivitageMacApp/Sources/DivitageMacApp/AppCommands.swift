import SwiftUI

struct AppCommands: Commands {
    @ObservedObject var appState: AppState

    var body: some Commands {
        CommandMenu("Divitage") {
            Button("変換開始") {
                NotificationCenter.default.post(name: .triggerConversion, object: nil)
            }
            .keyboardShortcut(.init(.return), modifiers: [.command])
            .disabled(appState.isProcessing)

            Divider()

            Picker("モード", selection: $appState.selectedPanel) {
                ForEach(WorkspacePanel.allCases) { panel in
                    Text(panel.title).tag(panel)
                }
            }
        }
    }
}
