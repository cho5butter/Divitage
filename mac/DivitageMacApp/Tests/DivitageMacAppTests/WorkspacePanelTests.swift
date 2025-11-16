import XCTest
@testable import DivitageMacApp

final class WorkspacePanelTests: XCTestCase {

    func testAllCases() {
        let allPanels = WorkspacePanel.allCases
        XCTAssertEqual(allPanels.count, 3)
        XCTAssertTrue(allPanels.contains(.converter))
        XCTAssertTrue(allPanels.contains(.settings))
        XCTAssertTrue(allPanels.contains(.howTo))
    }

    func testPanelIds() {
        XCTAssertEqual(WorkspacePanel.converter.id, "converter")
        XCTAssertEqual(WorkspacePanel.settings.id, "settings")
        XCTAssertEqual(WorkspacePanel.howTo.id, "howToUse")
    }

    func testPanelTitles() {
        XCTAssertEqual(WorkspacePanel.converter.title, "コンバーター")
        XCTAssertEqual(WorkspacePanel.settings.title, "設定")
        XCTAssertEqual(WorkspacePanel.howTo.title, "使い方")
    }

    func testPanelSystemImages() {
        XCTAssertEqual(WorkspacePanel.converter.systemImage, "arrow.triangle.2.circlepath")
        XCTAssertEqual(WorkspacePanel.settings.systemImage, "gear")
        XCTAssertEqual(WorkspacePanel.howTo.systemImage, "questionmark.circle")
    }

    func testPanelRawValue() {
        XCTAssertEqual(WorkspacePanel.converter.rawValue, "converter")
        XCTAssertEqual(WorkspacePanel.settings.rawValue, "settings")
        XCTAssertEqual(WorkspacePanel.howTo.rawValue, "howToUse")
    }

    func testPanelFromRawValue() {
        XCTAssertEqual(WorkspacePanel(rawValue: "converter"), .converter)
        XCTAssertEqual(WorkspacePanel(rawValue: "settings"), .settings)
        XCTAssertEqual(WorkspacePanel(rawValue: "howToUse"), .howTo)
        XCTAssertNil(WorkspacePanel(rawValue: "invalid"))
    }
}
