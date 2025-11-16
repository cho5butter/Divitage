import XCTest
@testable import DivitageMacApp

final class AppStateTests: XCTestCase {
    var appState: AppState!

    override func setUp() {
        super.setUp()
        appState = AppState()
    }

    override func tearDown() {
        appState = nil
        super.tearDown()
    }

    func testInitialState() {
        XCTAssertEqual(appState.selectedPanel, .converter)
        XCTAssertNotNil(appState.outputDirectory)
        XCTAssertFalse(appState.isProcessing)
        XCTAssertTrue(appState.lastConversionLog.isEmpty)
    }

    func testPanelSelection() {
        appState.selectedPanel = .settings
        XCTAssertEqual(appState.selectedPanel, .settings)

        appState.selectedPanel = .howTo
        XCTAssertEqual(appState.selectedPanel, .howTo)

        appState.selectedPanel = .converter
        XCTAssertEqual(appState.selectedPanel, .converter)
    }

    func testOutputDirectoryChange() {
        let newURL = URL(fileURLWithPath: "/tmp/test")
        appState.outputDirectory = newURL
        XCTAssertEqual(appState.outputDirectory, newURL)
    }

    func testConversionProcessing() async {
        let sourceURL = URL(fileURLWithPath: "/tmp/source.txt")
        let destURL = URL(fileURLWithPath: "/tmp/dest.txt")
        let task = ConversionTask(displayName: "Test Task", sourceURL: sourceURL, destinationURL: destURL)

        XCTAssertFalse(appState.isProcessing)

        appState.runConversion(task: task)

        // Processing should start immediately
        XCTAssertTrue(appState.isProcessing)

        // Wait for conversion to complete
        try? await Task.sleep(nanoseconds: 1_000_000_000) // 1 second

        // Processing should be complete
        XCTAssertFalse(appState.isProcessing)
        XCTAssertFalse(appState.lastConversionLog.isEmpty)
    }

    func testMultipleConversionsNotAllowed() {
        let task1 = ConversionTask(displayName: "Task 1", sourceURL: URL(fileURLWithPath: "/tmp/1.txt"), destinationURL: URL(fileURLWithPath: "/tmp/out1.txt"))
        let task2 = ConversionTask(displayName: "Task 2", sourceURL: URL(fileURLWithPath: "/tmp/2.txt"), destinationURL: URL(fileURLWithPath: "/tmp/out2.txt"))

        appState.runConversion(task: task1)
        XCTAssertTrue(appState.isProcessing)

        let logCountAfterFirst = appState.lastConversionLog.count

        // Try to run second conversion while first is processing
        appState.runConversion(task: task2)

        // Log should not have changed since second conversion shouldn't start
        XCTAssertEqual(appState.lastConversionLog.count, logCountAfterFirst)
    }
}
