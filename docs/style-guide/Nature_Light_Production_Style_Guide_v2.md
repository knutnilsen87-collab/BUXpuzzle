# Nature Light — Production Style Guide v2

## 1. Creative North Star

**Theme name:** Nature Light  
**Genre fit:** Deterministic match-3 puzzle platform  
**Experience goal:** Calm clarity, warm satisfaction, natural elegance, readable motion

Nature Light is a soft, modern, non-saccharine visual direction for BUXPuzzle. The game should feel inviting, clean, tactile, and emotionally warm without drifting into candy excess, casino energy, or overstimulating mobile puzzle aesthetics.

The theme must support:
- board readability first
- deterministic comprehension
- comfortable repeat play
- high clarity during cascades
- broad appeal without generic blandness

### Emotional keywords
- calm
- luminous
- natural
- tactile
- fresh
- readable
- warm
- gentle
- satisfying
- focused

### Not the vibe
- neon
- sugar rush
- toy plastic overload
- casino juice
- loud candy chaos
- aggressive combo spectacle
- white flash spam
- hyper-feminized glossy sweetness
- muddy fantasy darkness

---

## 2. Core Design Principle

**All visual and audio juice must preserve board readability and state comprehension.**

This is not decoration-first design.  
This is puzzle-first presentation.

Every effect must help the player understand:
- what changed
- why it changed
- what is stable
- what is still resolving

---

## 3. Audience Positioning

Nature Light is intended to feel:
- accessible to broad casual puzzle audiences
- modern enough for premium presentation
- soft enough for comfort play
- neutral enough to avoid overly gendered visual coding
- elegant enough to feel intentional rather than generic

---

## 4. Visual Identity

### High-level visual language
The world should feel like:
- sunlight through leaves
- polished natural materials
- glass, stone, wood, petals, dew
- clean air, soft bloom, gentle color separation

### Surface qualities
Prefer:
- satin
- matte glow
- subtle translucency
- polished natural texture
- restrained specular highlights

Avoid:
- harsh gloss
- sticky candy gel
- chrome shine
- metallic arcade lighting
- thick plastic toy finish

---

## 5. Color Strategy

### Palette behavior
Use a bright but softened palette with natural light bias.  
Colors must be distinguishable without relying only on hue.

Every tile must be separable by:
1. color
2. symbol
3. silhouette

### Palette qualities
Prefer:
- leaf green
- sky blue
- amber/yellow
- coral/red
- lavender/purple
- warm ivory/cream accents

Avoid:
- neon green
- saturated laser cyan
- toxic magenta
- overcompressed rainbow contrast
- overly dark jewel tones

### Contrast rule
Tiles must remain readable against the board and background on small screens and during motion.

---

## 6. Tile Identity System

Each tile must be recognizable by first glance and peripheral glance.

### Mandatory identity stack
Every tile needs:
- unique silhouette
- unique central symbol
- distinct internal value contrast
- clear edge definition
- stable readability during fall and cascade

### Recommended silhouette matrix
Use silhouettes that are clearly different, not “kind of rounded variations.”

Preferred silhouette families:
- circle / pebble
- tall leaf / oval
- diamond / petal point
- hex / stone
- rounded square / seed block
- blossom / floral multi-lobe

### Symbol rules
Symbols should be:
- simple
- centered
- high contrast
- readable at small size
- visually calm, not busy

Good symbol directions:
- leaf vein
- water droplet
- petal
- sunburst
- seed
- sprout
- polished stone mark

Avoid:
- intricate line art
- tiny decorative filigree
- multi-layer micro details
- cluttered iconography

---

## 7. Board and Background Rules

### Board priority
The board is the primary information surface.

### Background rules
Background must:
- stay visually behind gameplay
- have low detail frequency
- avoid edge noise behind tiles
- support focus, not compete with it

Avoid:
- moving scenery behind board
- sharp contrast directly under tiles
- high-frequency foliage patterns
- bright white halos behind active gameplay

---

## 8. Motion Language

### Motion character
Motion should feel:
- soft but precise
- grounded
- responsive
- readable
- elegant, never frantic

### Motion verbs
- settle
- float
- drop
- release
- bloom
- shimmer
- breathe

Not:
- explode wildly
- snap violently
- slam
- bounce like rubber toys
- jitter as spectacle

---

## 9. Match / Resolve Presentation

## Match detection beat
A match should communicate:
- recognition
- brief confirmation
- clean resolution
- readable aftermath

### Recommended sequence
1. match highlight  
2. soft pulse / light ring  
3. dissolve or pop  
4. short empty-slot readability moment  
5. fall  
6. landing settle  
7. cascade repeat if needed

### Key principle
There must be a tiny readable gap between disappearance and fall.  
Without this, the game risks becoming visually cheap and cognitively noisy.

---

## 10. Timing Budget

These are production starting points, not sacred laws.

### Baseline timings
- match recognition pulse: 80–140 ms
- pop/dissolve: 120–180 ms
- empty-slot hold: 40–90 ms
- fall start delay after clear: 20–60 ms
- fall duration: 140–240 ms depending on distance
- landing settle: 60–120 ms
- cascade gap before next recognition pulse: 60–100 ms

### Timing philosophy
- fast enough to feel responsive
- slow enough to remain readable
- never stack multiple big events into the same instant unless readability survives

---

## 11. VFX Language

### VFX should feel like
- light pollen
- petal dust
- dew sparkle
- sun motes
- soft refracted air

### VFX should not feel like
- fireworks machine
- gambling rewards
- confetti spam
- explosive candy syrup
- electric arcade burst

### VFX intensity budget
Normal gameplay must remain low-intensity.

Rules:
- max one strong focal effect per match phase
- combo emphasis only escalates meaningfully on cascade 2+
- particle count should scale down when multiple systems trigger at once
- no large full-screen white flash
- no persistent heavy glow layers over the board

---

## 12. Audio Direction

### Core sound identity
Audio should combine:
- organic tactility
- light glassiness
- soft wood resonance
- subtle airy synth support

### Sound references in spirit
- wood taps
- glass chimes
- petal-soft pops
- gentle leaf swishes
- warm sparkle tails

Avoid:
- harsh digital clicks
- slot-machine reward noise
- exaggerated candy squish
- metallic arcade stingers
- overcompressed bright UI spam

### Audio intent by action
- select tile: soft tactile confirmation
- valid swap: clean confident movement cue
- invalid swap: gentle refusal, never punitive
- match clear: satisfying soft release
- fall/landing: light physical continuity
- cascade: subtle escalation, not chaos
- special moment: richer harmonic lift, not volume spike

---

## 13. HUD / UI Rules

UI must support puzzle focus.

### UI tone
- clean
- airy
- restrained
- readable
- elegant

### Avoid
- giant combo banners
- flashing competitive overlays
- overly thick outlines
- loud reward spam
- crowded icon clusters

### UI motion
HUD motion should be softer and slower than tile motion.

---

## 14. Accessibility Requirements

### Non-negotiables
- tiles must not rely on color alone
- each tile must have silhouette + symbol distinction
- contrast must hold on smaller displays
- effects must not obscure board comprehension
- success/failure sounds must be distinguishable without aggression

### Small-screen requirement
If tile identity breaks on a small mobile screen, the design fails regardless of desktop appearance.

---

## 15. Do / Don’t

### Do
- prioritize board readability
- use calm light and natural material cues
- keep symbols simple and strong
- make cascades readable
- let empty space exist briefly after clears
- make VFX support state change
- use glow as signal, not decoration spam

### Don’t
- use neon reward language
- let the background fight the board
- make combo UI bigger than gameplay
- use similar silhouettes for multiple tiles
- add detail that only looks good in mockups
- use hard white flash or overbright bloom
- make the board feel sticky, sugary, or toy-plastic

---

## 16. Runtime Stress Test

The theme is only valid if it remains readable during:
- multi-step cascades
- simultaneous falls
- hint/highlight activity
- HUD updates
- special tile activation
- repeated rapid inputs

Question:
**Can the player still parse cause and effect instantly?**

If not, reduce visual intensity.

---

## 17. Signature Distinctiveness

To avoid becoming generic, Nature Light should own a subtle signature.

Recommended signature direction:
- gentle pollen-light particles
- clean, luminous symbol centers
- a softly lit board plate
- restrained nordic-natural calm rather than tropical excess
- a premium “quiet freshness” instead of candy sweetness

Distinctive, but not loud.

---

## 18. Final Production Rule

**Readability wins over beauty.**  
**Calm clarity wins over spectacle.**  
**Nature Light is successful only when the board feels beautiful because it is understandable.**
