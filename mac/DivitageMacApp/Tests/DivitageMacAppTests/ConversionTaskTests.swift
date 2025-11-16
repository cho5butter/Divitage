import XCTest
@testable import DivitageMacApp

final class ConversionTaskTests: XCTestCase {

    func testConversionTaskCreation() {
        let sourceURL = URL(fileURLWithPath: "/tmp/source.mp4")
        let destURL = URL(fileURLWithPath: "/tmp/output.mp4")
        let displayName = "Test Video"

        let task = ConversionTask(displayName: displayName, sourceURL: sourceURL, destinationURL: destURL)

        XCTAssertEqual(task.displayName, displayName)
        XCTAssertEqual(task.sourceURL, sourceURL)
        XCTAssertEqual(task.destinationURL, destURL)
        XCTAssertNotNil(task.id)
    }

    func testConversionTaskUniqueIds() {
        let sourceURL = URL(fileURLWithPath: "/tmp/source.mp4")
        let destURL = URL(fileURLWithPath: "/tmp/output.mp4")

        let task1 = ConversionTask(displayName: "Task 1", sourceURL: sourceURL, destinationURL: destURL)
        let task2 = ConversionTask(displayName: "Task 2", sourceURL: sourceURL, destinationURL: destURL)

        XCTAssertNotEqual(task1.id, task2.id)
    }

    func testConverterLogCreation() {
        let timestamp = Date()
        let message = "Test log message"

        let log = ConverterLog(timestamp: timestamp, message: message)

        XCTAssertEqual(log.timestamp, timestamp)
        XCTAssertEqual(log.message, message)
        XCTAssertNotNil(log.id)
    }

    func testConverterLogUniqueIds() {
        let timestamp = Date()
        let log1 = ConverterLog(timestamp: timestamp, message: "Message 1")
        let log2 = ConverterLog(timestamp: timestamp, message: "Message 2")

        XCTAssertNotEqual(log1.id, log2.id)
    }

    func testConverterLogTimestampOrdering() {
        let now = Date()
        let earlier = now.addingTimeInterval(-60) // 1 minute ago

        let log1 = ConverterLog(timestamp: earlier, message: "Earlier")
        let log2 = ConverterLog(timestamp: now, message: "Later")

        XCTAssertLessThan(log1.timestamp, log2.timestamp)
    }
}
