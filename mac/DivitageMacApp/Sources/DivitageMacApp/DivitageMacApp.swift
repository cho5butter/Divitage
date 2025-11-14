import SwiftUI

@main
struct DivitageMacApp: App {
    @StateObject private var appState = AppState()
    @Environment(\.scenePhase) private var scenePhase

    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(appState)
                .frame(minWidth: 900, minHeight: 600)
        }
        .commands { AppCommands(appState: appState) }
        .windowResizability(.contentSize)
    }
}
