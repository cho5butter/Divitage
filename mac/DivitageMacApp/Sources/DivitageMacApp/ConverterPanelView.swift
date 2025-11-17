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
                .transition(.move(edge: .top).combined(with: .opacity))
            dropZone
                .transition(.scale.combined(with: .opacity))
            destinationPicker
                .transition(.move(edge: .leading).combined(with: .opacity))
            logView
                .transition(.move(edge: .bottom).combined(with: .opacity))
        }
        .padding(32)
        .background(.thinMaterial, in: RoundedRectangle(cornerRadius: 20, style: .continuous))
        .shadow(color: .black.opacity(0.1), radius: 20, y: 10)
        .padding()
        .onReceive(NotificationCenter.default.publisher(for: .triggerConversion)) { _ in
            runConversion()
        }
        .onAppear {
            withAnimation(.spring(response: 0.6, dampingFraction: 0.8).delay(0.1)) {
                // エントランスアニメーション用
            }
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
                    .font(.headline)
            }
            .buttonStyle(.borderedProminent)
            .controlSize(.large)
            .disabled(appState.isProcessing || sourceURL == nil)
            .scaleEffect(appState.isProcessing ? 0.95 : 1.0)
            .animation(.spring(response: 0.3, dampingFraction: 0.6), value: appState.isProcessing)
        }
    }

    private var dropZone: some View {
        ZStack {
            // Liquid Glass背景効果
            RoundedRectangle(cornerRadius: 16, style: .continuous)
                .fill(.ultraThinMaterial)
                .overlay(
                    RoundedRectangle(cornerRadius: 16, style: .continuous)
                        .stroke(
                            LinearGradient(
                                colors: dropHighlight
                                    ? [Color.accentColor, Color.accentColor.opacity(0.5)]
                                    : [Color.secondary.opacity(0.3), Color.secondary.opacity(0.1)],
                                startPoint: .topLeading,
                                endPoint: .bottomTrailing
                            ),
                            lineWidth: dropHighlight ? 3 : 2
                        )
                )
                .shadow(color: .black.opacity(dropHighlight ? 0.2 : 0.1), radius: dropHighlight ? 16 : 8, y: 4)

            VStack(spacing: 16) {
                Image(systemName: dropHighlight ? "arrow.down.doc.fill" : "square.and.arrow.down")
                    .font(.system(size: 48))
                    .symbolRenderingMode(.hierarchical)
                    .foregroundStyle(dropHighlight ? .accentColor : .secondary)

                VStack(spacing: 4) {
                    Text(sourceURL?.lastPathComponent ?? "ここにファイルをドロップ")
                        .font(.headline)
                        .foregroundStyle(.primary)

                    Text("※ 複数ファイルの場合はフォルダごとドロップ")
                        .font(.caption)
                        .foregroundStyle(.secondary)
                }
            }
            .padding()
        }
        .frame(height: 220)
        .scaleEffect(dropHighlight ? 1.02 : 1.0)
        .animation(.spring(response: 0.3, dampingFraction: 0.7), value: dropHighlight)
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
