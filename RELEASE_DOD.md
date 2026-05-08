# Release Definition of Done

The end product is complete only when BUXPuzzle can be submitted to both Apple App Store and Google Play Store.

## Product

- The game launches on supported iOS and Android devices.
- A player can complete a full match-3 session without crashes or blocking bugs.
- Touch input works reliably on physical phones.
- Swap, match, clear, drop, spawn, cascade, invalid-swap feedback, and progression are complete.
- Save/progression behavior is deterministic and survives app restarts.

## Presentation

- Nature Light visual direction is implemented consistently.
- Tile identity does not rely on color alone.
- Board readability holds on small screens and during cascades.
- HUD, animation timing, VFX, SFX, and optional haptics are production balanced.
- Placeholder art/audio is replaced or explicitly approved for release.

## Engineering

- Unity opens the project without package, auth, or compiler blockers.
- `Assets/Scenes/game.unity` runs without runtime exceptions.
- Release builds produce no critical Unity console errors.
- The build is reproducible from this repository.
- Core gameplay has focused automated tests or documented verification coverage.
- Performance, memory, loading, and battery behavior are acceptable on target devices.

## Store Readiness

- iOS archive uploads successfully to App Store Connect.
- Android signed `.aab` uploads successfully to Google Play Console.
- Bundle identifiers, signing certificates, provisioning profiles, and versioning are configured.
- App icon, splash screen, screenshots, descriptions, keywords, and ratings are ready.
- Privacy policy is published and linked.
- Tracking, analytics, ads, in-app purchases, and permissions are configured correctly or removed.
- TestFlight and Google Play internal testing have passed on physical devices.

## Submission

- One release candidate build is approved internally.
- Known critical and high-severity issues are closed.
- Store review feedback is resolved.
