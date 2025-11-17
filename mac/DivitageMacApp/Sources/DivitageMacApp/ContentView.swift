import SwiftUI

struct ContentView: View {
    @EnvironmentObject private var appState: AppState

    var body: some View {
        NavigationSplitView(sidebar: {
            sidebar
        }, content: {
            Group {
                switch appState.selectedPanel {
                case .converter:
                    ConverterPanelView()
                case .settings:
                    SettingsView()
                case .howTo:
                    HowToUseView()
                }
            }
            .transition(.asymmetric(
                insertion: .move(edge: .trailing).combined(with: .opacity),
                removal: .move(edge: .leading).combined(with: .opacity)
            ))
            .animation(.spring(response: 0.4, dampingFraction: 0.8), value: appState.selectedPanel)
        }, detail: {
            DetailPlaceholderView(selection: appState.selectedPanel)
        })
        .navigationSplitViewStyle(.balanced)
        .toolbar { toolbar }
        .background(.regularMaterial)
    }

    private var sidebar: some View {
        List(selection: $appState.selectedPanel) {
            Section("ワークスペース") {
                ForEach(WorkspacePanel.allCases) { panel in
                    Label(panel.title, systemImage: panel.systemImage)
                        .tag(panel)
                }
            }
        }
        .listStyle(.sidebar)
        .frame(minWidth: 220)
    }

    private var toolbar: some ToolbarContent {
        ToolbarItemGroup(placement: .automatic) {
            if appState.selectedPanel == .converter {
                Button(action: { NotificationCenter.default.post(name: .triggerConversion, object: nil) }) {
                    Label("変換", systemImage: "play.fill")
                }
                .keyboardShortcut(.init(.return), modifiers: [.command])
                .disabled(appState.isProcessing)
            }

            Button(action: { appState.shouldShowOnboarding.toggle() }) {
                Label("ヒント", systemImage: "lightbulb")
            }
        }
    }
}

private struct DetailPlaceholderView: View {
    let selection: WorkspacePanel

    var body: some View {
        VStack(spacing: 12) {
            Image(systemName: selection.systemImage)
                .font(.system(size: 48))
                .foregroundStyle(.secondary)
            Text("\(selection.title)を操作できます")
                .font(.title3)
                .foregroundStyle(.secondary)
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity)
        .background(Color(NSColor.windowBackgroundColor))
    }
}
