using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Game.Levels
{
    public sealed class LevelRepository
    {
        private const string LevelFolder = "Game/Content/Levels";
        private readonly Dictionary<int, LevelSpecV2> _cache = new Dictionary<int, LevelSpecV2>();

        public LevelSpecV2 GetLevel(int levelId)
        {
            int id = Mathf.Max(1, levelId);
            if (_cache.TryGetValue(id, out var cached))
            {
                return cached;
            }

            if (TryLoadJsonLevel(id, out var spec))
            {
                var validation = LevelValidator.Validate(spec);
                if (validation.IsValid)
                {
                    _cache[id] = spec;
                    return spec;
                }

                Debug.LogWarning("[LevelRepository] Invalid JSON level " + id + ": " + validation.Message);
            }

            var fallback = LevelSpecV2Adapter.FromLegacy(LevelManager.GetLevel(id));
            _cache[id] = fallback;
            return fallback;
        }

        public LevelSpec GetLegacyLevel(int levelId)
        {
            return LevelSpecV2Adapter.ToLegacy(GetLevel(levelId));
        }

        public bool TryLoadJsonLevel(int levelId, out LevelSpecV2 spec)
        {
            spec = null;
            string path = Path.Combine(Application.dataPath, LevelFolder, "Level_" + levelId + ".json");
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                var dto = JsonUtility.FromJson<LevelJsonDto>(json);
                if (dto == null)
                {
                    return false;
                }

                spec = Map(dto, levelId);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LevelRepository] Failed to load level " + levelId + ": " + ex.Message);
                return false;
            }
        }

        private static LevelSpecV2 Map(LevelJsonDto dto, int fallbackId)
        {
            int id = dto.levelId > 0 ? dto.levelId : fallbackId;
            var spec = new LevelSpecV2
            {
                LevelId = id,
                WorldId = dto.worldId <= 0 ? 1 : dto.worldId,
                DisplayName = string.IsNullOrEmpty(dto.displayName) ? "Level " + id : dto.displayName,
                Width = dto.width <= 0 ? 8 : dto.width,
                Height = dto.height <= 0 ? 8 : dto.height,
                MoveLimit = dto.moveLimit <= 0 ? 20 : dto.moveLimit,
                ParMoves = dto.parMoves,
                Seed = dto.seed != 0 ? dto.seed : DeterministicEndlessLevelGenerator.SeedForLevel(id),
                RulesetId = string.IsNullOrEmpty(dto.rulesetId) ? "main_path_v1" : dto.rulesetId,
                AllowLose = dto.allowLose,
                ForceWinBias = dto.forceWinBias,
                DesignerNote = dto.designerNote,
                NewMechanic = ParseMechanic(dto.mechanic),
                BoardRows = dto.boardRows ?? Array.Empty<string>()
            };

            if (dto.goals != null && dto.goals.Length > 0)
            {
                var objectives = new ObjectiveSpec[dto.goals.Length];
                for (int i = 0; i < dto.goals.Length; i++)
                {
                    objectives[i] = new ObjectiveSpec
                    {
                        Type = ParseObjective(dto.goals[i].type),
                        Target = Mathf.Max(1, dto.goals[i].target),
                        TargetTileType = dto.goals[i].targetTileType
                    };
                }

                spec.Objectives = objectives;
            }

            return spec;
        }

        private static LevelObjectiveType ParseObjective(string value)
        {
            if (string.IsNullOrEmpty(value)) return LevelObjectiveType.MakeMatches;
            switch (value.Trim().ToLowerInvariant())
            {
                case "score":
                case "reachscore": return LevelObjectiveType.ReachScore;
                case "legacymatches":
                case "matches":
                case "makematches": return LevelObjectiveType.MakeMatches;
                case "createspecials": return LevelObjectiveType.CreateSpecials;
                case "clearblockers": return LevelObjectiveType.ClearBlockers;
                case "cleartilesoftype": return LevelObjectiveType.ClearTilesOfType;
                case "dropobjectstoexit": return LevelObjectiveType.DropObjectsToExit;
                case "triggercascades": return LevelObjectiveType.TriggerCascades;
                case "finishunderpar": return LevelObjectiveType.FinishUnderPar;
                default: return LevelObjectiveType.MakeMatches;
            }
        }

        private static LevelMechanic ParseMechanic(string value)
        {
            if (string.IsNullOrEmpty(value)) return LevelMechanic.None;
            switch (value.Trim().ToLowerInvariant())
            {
                case "sunbeam": return LevelMechanic.Sunbeam;
                case "bloombomb": return LevelMechanic.BloomBomb;
                case "sunorb": return LevelMechanic.SunOrb;
                case "moss": return LevelMechanic.Moss;
                case "vine": return LevelMechanic.Vine;
                case "dewdrop": return LevelMechanic.DewDrop;
                default: return LevelMechanic.None;
            }
        }
    }
}
