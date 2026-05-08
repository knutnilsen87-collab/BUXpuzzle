# Gameplay Scene Setup

Unity steps (once):
1) Open BUXPuzzle in Unity
2) Create new Scene: Assets/Scenes/Gameplay.unity
3) Add empty GameObject: GameRoot
4) Attach GameRoot.cs

What this scene wires:
- BoardEngine (seeded)
- FeedbackSystem (Lag 1)
- RewardPipeline (Lag 2–4)
- SessionTelemetryGuard (P0)

Expected:
- No errors on Play
- Session start logged
- Reward telemetry fires on EndLevel()
