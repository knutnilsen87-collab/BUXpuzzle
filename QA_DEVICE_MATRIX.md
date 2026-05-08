# QA Device Matrix

Physical device QA is required before App Store or Google Play submission.

## Current Automated Coverage

- Unity import and script compilation passed.
- Release validation passed with signing/icon warnings.
- Batchmode Play Mode smoke passed: board, tile colliders, input raycast, accepted swap, rejected swap, and runtime symbols.
- Windows build passed.
- Android AAB build passed.
- iOS build attempt failed on Windows because the iOS target is unsupported in this environment.

## Required iOS Coverage

- One recent iPhone with current iOS.
- One older supported iPhone.
- Safe area check on notched/dynamic-island screen.
- TestFlight install and launch.

## Required Android Coverage

- One recent Android phone.
- One lower-end Android phone.
- Different aspect ratio from iPhone test device.
- Google Play internal test install and launch.

## Smoke Checklist

- App launches cold.
- Board is visible.
- Touch selects tiles.
- Valid swap resolves.
- Invalid swap is gently refused.
- Audio plays without clipping.
- Progress saves after a completed session.
- App survives background and foreground.
- No critical errors or crashes.
