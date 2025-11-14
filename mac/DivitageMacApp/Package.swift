// swift-tools-version: 5.9
import PackageDescription

let package = Package(
    name: "DivitageMacApp",
    defaultLocalization: "ja",
    platforms: [
        .macOS(.v13)
    ],
    products: [
        .executable(name: "DivitageMacApp", targets: ["DivitageMacApp"])
    ],
    dependencies: [],
    targets: [
        .executableTarget(
            name: "DivitageMacApp",
            path: "Sources"
        )
    ]
)
