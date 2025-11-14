import Foundation
import Combine

enum WorkspacePanel: String, CaseIterable, Identifiable {
    case converter = "converter"
    case settings = "settings"
    case howTo = "howToUse"

    var id: String { rawValue }

    var title: String {
        switch self {
        case .converter: return "コンバーター"
        case .settings: return "設定"
        case .howTo: return "使い方"
        }
    }

    var systemImage: String {
        switch self {
        case .converter: return "arrow.triangle.2.circlepath"
        case .settings: return "gear"
        case .howTo: return "questionmark.circle"
        }
    }
}

final class AppState: ObservableObject {
    @Published var selectedPanel: WorkspacePanel = .converter
    @Published var outputDirectory: URL? = FileManager.default.urls(for: .downloadsDirectory, in: .userDomainMask).first
    @Published var lastConversionLog: [ConverterLog] = []
    @Published var isProcessing: Bool = false
    @Published var shouldShowOnboarding: Bool = true

    func runConversion(task: ConversionTask) {
        guard !isProcessing else { return }
        isProcessing = true
        lastConversionLog.removeAll(keepingCapacity: true)

        Task.detached(priority: .userInitiated) { [weak self] in
            try? await Task.sleep(nanoseconds: 500_000_000) // 擬似処理
            await MainActor.run {
                self?.lastConversionLog.append(.init(timestamp: Date(), message: "変換完了: \(task.displayName)"))
                self?.isProcessing = false
            }
        }
    }
}

struct ConversionTask: Identifiable {
    let id = UUID()
    let displayName: String
    let sourceURL: URL
    let destinationURL: URL
}

struct ConverterLog: Identifiable {
    let id = UUID()
    let timestamp: Date
    let message: String
}
