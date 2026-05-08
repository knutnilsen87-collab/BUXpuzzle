# Nature Light — Unity Implementation Spec

## 1. Purpose

This document translates the Nature Light style direction into implementation-ready production rules for Unity and the BUXPuzzle runtime/presentation stack.

This is not a mood board.  
This is a presentation contract.

---

## 2. Scope

Applies to:
- BoardView
- TileView
- TileInput feedback
- HUD presentation
- board background plate
- tile VFX
- tile SFX
- cascade presentation
- state visibility rules

Does not define:
- core engine logic
- evaluator logic
- simulation logic
- discovery progression systems

---

## 3. Implementation Principle

**BoardEngine remains the single source of truth.**  
Presentation may project state only.

Therefore:
- visual effects must follow engine state transitions
- visuals must never invent gameplay state
- no animation may hide authoritative resolution order
- redraw must reflect actual engine board contents after each accepted state change

---

## 4. Repo Documentation Contract

Recommended repo locations:
- `docs/style-guide/`
- `docs/implementation-spec/`
- `docs/archive/`

This document belongs in:
- `docs/implementation-spec/Nature_Light_Unity_Implementation_Spec.md`

---

## 5. Board Presentation Contract

### BoardView responsibilities
- bind to GameRoot / BoardEngine
- read authoritative board state
- spawn/update TileView instances
- redraw after accepted resolve cycles
- never own authoritative tile data
- never cache alternative gameplay state

### Required redraw rule
After accepted state change:

`Swap -> ResolveSummary -> redraw from engine`

### Redraw invariant
BoardView must render the engine state that exists now, not an inferred or transitional fake state.

---

## 6. TileView Contract

Each TileView must expose or support:
- coordinate display/update
- tile type display/update
- selected/highlight state
- visual state transitions
- symbol rendering
- silhouette identity
- material/scale/alpha animation hooks

TileView must not:
- decide gameplay outcomes
- persist hidden alternate board values
- mutate engine state
- simulate matches independently

---

## 7. Tile Identity Implementation

Each tile must be distinguishable by:
1. silhouette
2. symbol
3. color family

### Suggested enum mapping
Use six core visual families mapped to gameplay tile types:
- pebble
- leaf
- petal
- stone
- seed
- blossom

### Required per-type fields
Each tile presentation entry should define:
- `Id`
- `DisplayName`
- `PrimaryColor`
- `SecondaryColor`
- `SymbolSprite`
- `ShapeSprite / Mesh`
- `OutlineStrength`
- `GlowStrength`
- `SfxProfile`
- `VfxProfile`

---

## 8. Design Tokens

## Color tokens
Define as centralized tokens, not scattered literals.

Suggested tokens:
- `NL_Color_Leaf`
- `NL_Color_Sky`
- `NL_Color_Amber`
- `NL_Color_Coral`
- `NL_Color_Lavender`
- `NL_Color_Cream`
- `NL_Color_BoardBase`
- `NL_Color_BoardShadow`
- `NL_Color_HighlightSoft`
- `NL_Color_InvalidSwap`

## Material tokens
- `NL_Mat_Tile_Satin`
- `NL_Mat_Tile_GlassSoft`
- `NL_Mat_Board_Base`
- `NL_Mat_Glow_Soft`
- `NL_Mat_UI_Light`

## Motion tokens
- `NL_Time_SelectPulse`
- `NL_Time_MatchConfirm`
- `NL_Time_Clear`
- `NL_Time_Fall`
- `NL_Time_Land`
- `NL_Time_CascadeGap`
- `NL_Time_InvalidSwap`

## Scale tokens
- `NL_Scale_Select`
- `NL_Scale_MatchPulse`
- `NL_Scale_LandSettle`
- `NL_Scale_SpecialEmphasis`

---

## 9. Animation State Machine

### Recommended tile visual states
- `Idle`
- `Selected`
- `Hinted`
- `SwapPreview`
- `Matched`
- `Clearing`
- `Falling`
- `Landing`
- `SpecialCharged`
- `Disabled` (only if required by future modes)

### Rules
- state changes must be driven by engine/runtime events
- `Matched` must be short and readable
- `Clearing` must not visually outlast gameplay understanding
- `Falling` must remain readable with distance-based timing
- `Landing` must settle, not bounce like a toy

---

## 10. Timing Spec

Recommended starting tokens:

- select pulse: `0.08–0.12s`
- match confirm: `0.08–0.14s`
- clear dissolve/pop: `0.12–0.18s`
- empty-slot readability hold: `0.04–0.09s`
- fall short distance: `0.14–0.18s`
- fall medium distance: `0.18–0.22s`
- fall long distance: `0.22–0.26s`
- landing settle: `0.06–0.12s`
- invalid swap feedback: `0.14–0.20s`
- cascade recognition gap: `0.06–0.10s`

These must be reviewed under cascade load.

---

## 11. Redraw and Resolve Visual Contract

### Required resolve presentation order
1. accepted swap
2. match recognition
3. clear effect
4. short readable empty phase
5. gravity motion
6. landing
7. cascade pause
8. next resolve iteration if needed

### Failure conditions
Presentation is considered incorrect if:
- fall begins before clear is cognitively readable
- board appears to change without a visible cause
- tile identities blur during movement
- multiple effects hide authoritative order
- redraw shows stale data

---

## 12. VFX Implementation Rules

### Allowed VFX families
- pollen motes
- soft sparkle
- petal dust
- dew shimmer
- light rings
- subtle refractive glow

### Forbidden VFX families
- explosive confetti
- gambling showers
- large white flash
- screen-filling glow fog
- heavy electric energy arcs
- sticky candy burst splatter

### Budget rules
- one strong focal effect at a time in normal flow
- cap simultaneous active particle intensity
- lower secondary effect strength during cascades
- special events may intensify, but board readability must survive

---

## 13. Audio Event Map

Recommended event categories:

### Input
- `sfx_tile_select`
- `sfx_tile_deselect`
- `sfx_swap_valid`
- `sfx_swap_invalid`

### Resolution
- `sfx_match_clear_small`
- `sfx_match_clear_medium`
- `sfx_match_clear_large`
- `sfx_tile_fall`
- `sfx_tile_land`
- `sfx_cascade_step`

### Special / system
- `sfx_hint_soft`
- `sfx_special_charge`
- `sfx_special_trigger`
- `sfx_session_win_soft`
- `sfx_session_fail_soft`

### Audio rules
- invalid swap must refuse gently
- landing sounds must not stack into noise walls
- cascade escalation should be harmonic, not just louder
- no reward stingers that dwarf board events

---

## 14. HUD Implementation Rules

HUD must be subordinate to the board.

### Rules
- combo text only escalates meaningfully on cascade 2+
- avoid persistent large banners
- no bright flashing edge overlays
- move counters / objective chips must remain calm and legible
- hint indicators must be visually softer than active matches

---

## 15. Board Background Implementation

Board plate should:
- frame gameplay
- provide contrast
- feel premium and calm
- avoid texture competition

Implementation suggestions:
- soft vignette around board area only
- light material depth
- low-frequency natural surface treatment
- minimal animated background unless proven harmless

---

## 16. Accessibility Implementation Requirements

### Must pass
- color-independent recognition
- small-screen readability
- symbol visibility in motion
- contrast stability against board
- limited effect clutter
- audio distinction without aggression

### Review test
Capture footage on small display scale and check:
- can each tile type be identified instantly?
- can cascades still be parsed?
- does VFX ever cover symbol centers?
- do landings remain readable?

---

## 17. Prefab Rules

### Tile prefab must contain
- TileView component
- visible primary render element
- collider appropriate to actual input mode
- symbol render child
- optional glow child
- optional highlight child

### Tile prefab must not contain
- gameplay authority
- random tile identity logic
- hardcoded engine mutation calls

### Board prefab / scene requirements
- BoardView must have TilePrefab assigned
- BoardView must not run with null TilePrefab in production scenes
- missing TilePrefab is a setup failure, not a tolerated state

---

## 18. Logging / Diagnostics Presentation Hooks

Presentation layer should log enough to diagnose:
- redraw start / finish
- tile count / board dimensions
- accepted vs rejected swap
- match clear counts
- cascade iteration counts
- missing prefab / missing bind failures

Presentation logs should support, not replace, engine truth.

---

## 19. Verification Gates

This spec is not “implemented” until these pass:

### Gate A — setup
- scene loads
- no missing refs
- TilePrefab assigned
- board visible

### Gate B — input
- tile select visible
- valid swap visualized
- invalid swap gently refused

### Gate C — resolution
- match is readable
- clear is readable
- fall is readable
- landing is readable
- cascades remain parseable

### Gate D — stability
- no stale redraw
- no state/view desync observed
- runtime logs align with visual outcome

### Gate E — accessibility
- tile identities survive small-screen scaling
- color is not sole discriminator
- VFX does not overpower symbols

---

## 20. Review Checklist

Before accepting any visual implementation, ask:

- Does this improve or reduce board readability?
- Does it explain state change, or only decorate it?
- Is the silhouette still readable during motion?
- Is the effect intensity appropriate during cascades?
- Would this still work on a small phone screen?
- Does it fit Nature Light, or drift into generic mobile puzzle noise?

If uncertain, reduce intensity and simplify.

---

## 21. Final Engineering Rule

**Presentation must make deterministic gameplay easier to understand, never harder.**

If an effect is beautiful but weakens comprehension, it fails spec.
