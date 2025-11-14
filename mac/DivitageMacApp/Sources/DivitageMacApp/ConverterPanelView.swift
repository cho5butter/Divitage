import SwiftUI
import UniformTypeIdentifiers
import AppKit

struct ConverterPanelView: View {
    @EnvironmentObject private var appState: AppState
    @State private var dropHighlight = false
    @State private var task: ConversionTask?

    @State private var sourceURL: URL?
    @State private var destinationURL: URL?

    private let dropTypes: [UTType] = [.fileURL]

    var body: some View {
        VStack(alignment: .leading, spacing: 24) {
            header
            dropZone
            destinationPicker
            logView
        }
        .padding(32)
        .background(.thinMaterial, in: RoundedRectangle(cornerRadius: 16, style: .continuous))
        .padding()
        .onReceive(NotificationCenter.default.publisher(for: .triggerConversion)) { _ in
            runConversion()
        }
    }

    private var header: some View {
        HStack {
            VStack(alignment: .leading, spacing: 4) {
                Text("Divitage for macOS")
                    .font(.largeTitle.bold())
                Text("ドラッグ＆ドロップでファイルを投入し、ワンクリックで変換します。")
                    .foregroundStyle(.secondary)
            }
            Spacer()
            Button(action: runConversion) {
                Label(appState.isProcessing ? "処理中" : "変換開始", systemImage: appState.isProcessing ? "hourglass" : "play.fill")
            }
            .buttonStyle(.borderedProminent)
            .disabled(appState.isProcessing || sourceURL == nil)
        }
    }

    private var dropZone: some View {
        ZStack {
            RoundedRectangle(cornerRadius: 12, style: .continuous)
                .strokeBorder(dropHighlight ? Color.accentColor : Color.secondary.opacity(0.4), style: StrokeStyle(lineWidth: 2, dash: [8]))
                .background(Color(nsColor: .windowBackgroundColor).opacity(dropHighlight ? 0.6 : 0.3))
            VStack(spacing: 8) {
                Image(systemName: "square.and.arrow.down")
                    .font(.system(size: 32))
                Text(sourceURL?.lastPathComponent ?? "ここにファイルをドロップ")
                Text("※ 複数ファイルの場合はフォルダごとドロップ")
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }
        }
        .frame(height: 200)
        .onDrop(of: dropTypes, isTargeted: $dropHighlight) { providers in
            guard let provider = providers.first else { return false }
            provider.loadFileRepresentation(forTypeIdentifier: UTType.fileURL.identifier) { url, _ in
                guard let url else { return }
                Task { @MainActor in
                    self.sourceURL = url
                    self.destinationURL = appState.outputDirectory?.appending(path: url.lastPathComponent)
                }
            }
            return true
        }
    }

    private var destinationPicker: some View {
        VStack(alignment: .leading, spacing: 8) {
            Label("出力先", systemImage: "externaldrive")
                .font(.headline)
            HStack {
                Text(destinationURL?.path(percentEncoded: false) ?? "未設定")
                    .lineLimit(1)
                Spacer()
                Button("選択") {
                    let panel = NSOpenPanel()
                    panel.canChooseFiles = false
                    panel.canChooseDirectories = true
                    panel.allowsMultipleSelection = false
                    if panel.runModal() == .OK {
                        destinationURL = panel.url
                        appState.outputDirectory = panel.url
                    }
                }
            }
        }
    }

    private var logView: some View {
        VStack(alignment: .leading, spacing: 12) {
            Label("アクティビティ", systemImage: "waveform")
            ScrollView {
                LazyVStack(alignment: .leading, spacing: 8) {
                    ForEach(appState.lastConversionLog) { log in
                        HStack(alignment: .firstTextBaseline, spacing: 12) {
                            Text(log.timestamp, style: .time)
                                .font(.caption)
                                .foregroundStyle(.secondary)
                            Text(log.message)
                                .font(.body.monospaced())
                        }
                    }
                }
                .frame(maxWidth: .infinity, alignment: .leading)
            }
            .frame(minHeight: 120)
        }
    }

    private func runConversion() {
        guard let sourceURL, let destinationURL else { return }
        let task = ConversionTask(displayName: sourceURL.lastPathComponent, sourceURL: sourceURL, destinationURL: destinationURL)
        self.task = task
        appState.runConversion(task: task)
    }
}

private extension Notification.Name {
    static let triggerConversion = Notification.Name("DivitageMacApp.triggerConversion")
}
