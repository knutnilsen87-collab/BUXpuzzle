# Store Readiness

This file tracks the remaining work for Apple App Store and Google Play submission.

## Current Local Status

- Unity import and compile: passing on `6000.3.3f1`.
- Release validation: passing with signing/icon warnings.
- Runtime smoke test: passing in Play Mode batchmode.
- Windows build: passing, output `Builds/Windows/BUXPuzzle.exe`.
- Android AAB build: passing, output `Builds/Android/BUXPuzzle.aab`.
- iOS Xcode export: blocked on this Windows machine because the iOS build target is unsupported here.

## External Release Blockers

- Android release keystore is not configured.
- Google Play Console upload/signing has not been configured with the final app record.
- iOS signing, provisioning profiles, and Apple Team ID are not configured.
- iOS export/archive requires macOS/Xcode plus Unity iOS build support.
- App icons and store artwork are not final.
- TestFlight testing requires an Apple Developer account and physical iOS device.
- Google Play internal testing requires a Play Console app and physical Android device.
- Privacy policy must be published at a public URL before store submission.

## Store Assets Needed

- App icon set for iOS and Android.
- Splash/loading artwork.
- Store screenshots for required device sizes.
- Short description.
- Full description.
- Keywords / tags.
- Support URL.
- Privacy policy URL.

## Release Candidate Gate

The release candidate is acceptable only when:

- Android `.aab` builds with real signing.
- iOS Xcode project archives with real signing.
- The same release candidate has passed physical-device smoke tests.
- Store metadata and privacy answers match the actual app behavior.
