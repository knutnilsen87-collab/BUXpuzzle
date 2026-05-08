# Feedback System (Lag 1)

Guarantees:
- Never > 10–15s without feedback
- touch → feedback < 300ms target
- Throttled to avoid spam

Usage:
- Call FeedbackSystem.Play(new FeedbackEvent { Type=Match, Intensity=1 })
- Cascade should increase Intensity
- Specials use higher Intensity

Assets (later):
- Hook AudioClips + ParticleSystems
- Respect ReducedMotion + Sound toggles
