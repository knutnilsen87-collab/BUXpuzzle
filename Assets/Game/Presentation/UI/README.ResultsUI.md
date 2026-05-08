# Results Screen UI

Layout rules:
- Same layout for win and lose
- Only copy/FX differ
- 4 cards ALWAYS visible slots

Cards:
1) Instant reward (token)
2) XP progress
3) Fragment progress
4) Skill highlight OR Tip learned

Integration:
- GameRoot.EndLevel() → ResultsData
- Bind ResultsData directly to ResultsScreen.Show()
- NEVER hide screen on loss
