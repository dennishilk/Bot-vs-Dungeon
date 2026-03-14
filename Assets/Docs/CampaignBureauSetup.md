# Dungeon Certification Bureau Campaign Setup

## Overview
This campaign/meta layer adds rank progression, bureau score, assignment chains, promotion messaging, and profile tracking without changing core dungeon gameplay loops.

## 1) Campaign Data Structures
- `CampaignAssignment`: assignment metadata, objective gates, reward, memo text.
- `CampaignTier`: grouped assignments + unlock threshold.
- `PlayerCareerData`: persistent career stats and unlocks.

## 2) Rank + Bureau Score System
- `BureauScoreManager` computes rewards for assignments, certifications, daily completions, and achievements.
- `CampaignManager` applies bureau score and promotes rank by threshold list.
- Rank examples in use:
  - Junior Architect
  - Certified Architect
  - Senior Hazard Planner
  - Brutality Compliance Officer
  - Grand Auditor of Peril

## 3) Assignment Progression Logic
- Active assignment completion is evaluated against `DungeonReport` output.
- Requirements supported:
  - minimum rating
  - minimum survival rate
  - at least one fatality
  - optional daily completion gate
- Completed assignments unlock next assignment in tier.
- Tier unlocks by bureau score.

## 4) Promotion Screen Logic
- `PromotionScreenController` subscribes to `CampaignManager.OnPromotionGranted`.
- Shows new title, promotion message, and newly unlocked features.

## 5) Profile Panel Logic
- `ProfilePanelController` displays:
  - current rank
  - bureau score
  - assignments completed
  - bots tested/fatalities
  - average survival rate
  - best dungeon rating

## 6) Campaign UI Hierarchy (recommended)
- `Canvas`
  - `MainMenuPanel`
    - `RankDisplayWidget`
    - `CampaignButton`
    - `ProfileButton`
  - `CampaignPanel` (toggle root)
    - `CampaignScreenController`
    - `AssignmentDetailsPanel`
  - `PromotionPanel` (toggle root)
    - `PromotionScreenController`
  - `ProfilePanel` (toggle root)
    - `ProfilePanelController`

## 7) Save/Load Career Integration
- Career save file: `career_save.json` under `Application.persistentDataPath`.
- Includes rank, score, completed assignments, unlocked tiers/features, and summary stats.

## 8) Example Assignment Set (3 tiers)
- Tier 1 (Junior):
  - Introductory Saw Placement Audit
  - Bomb Corridor Compliance Review
- Tier 2 (Certified):
  - Multi-Bot Survivability Assessment
  - Reckless Bot Fatality Investigation
- Tier 3 (Senior):
  - Pit Maze Licensing Trial
  - Daily Compliance Drill

## 9) Flavor Text Samples
- Assignment memo: "This corridor meets minimum saw-spacing requirements."
- Assignment memo: "Bomb placement remains technically legal, though deeply frowned upon."
- Assignment memo: "No surviving bot signatures were recovered. Filing continues."
- Promotion line: "Promotion approved. Please sign Form 44-B before your next lethal review."

## Inspector Wiring Notes
- Add `CampaignManager` to a persistent manager object.
- Assign `CertificationManager` and `BureauScoreManager` references.
- Wire UI scripts to matching TMP text fields and panel roots.
- In `MainMenuController`, assign campaign/profile panels to new serialized fields.
- In `DailyChallengeManager` and `AchievementManager`, assign `CampaignManager` for meta-score rewards.
