# Telemetry Guards (P0)

Events:
- session_start
- first_reward_time  (target < 5s)
- reward_granted     (already implemented in RewardPipeline usage)
- empty_session_detected  <-- MUST be ~0%
- rage_quit (< 120s)
- session_end

Rules:
- empty_session_detected == true → P0 bug
- first_reward_time > 5s often → onboarding failure
- rage_quit spike → difficulty / feedback issue

Integration:
- Create SessionTelemetryGuard on app/session start
- Call OnRewardGranted() whenever ANY reward is granted
- Call OnSessionEnd() on quit / background
