using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    [SerializeField] private ArenaManager arenaManager;
    [SerializeField] private CertificationManager certificationManager;
    [SerializeField] private ProgressionManager progressionManager;
    [SerializeField] private AchievementPopup achievementPopup;

    [Header("Thresholds")]
    [SerializeField] private int trapEngineerTrapCount = 10;

    private readonly Queue<AchievementData> _popupQueue = new();
    private bool _popupPlaying;

    private void Awake()
    {
        if (arenaManager != null)
        {
            arenaManager.OnArenaChanged += HandleArenaChanged;
        }

        if (certificationManager != null)
        {
            certificationManager.OnCertificationCompleted += HandleCertificationCompleted;
        }
    }

    private void OnDestroy()
    {
        if (arenaManager != null)
        {
            arenaManager.OnArenaChanged -= HandleArenaChanged;
        }

        if (certificationManager != null)
        {
            certificationManager.OnCertificationCompleted -= HandleCertificationCompleted;
        }
    }

    private void HandleArenaChanged()
    {
        if (arenaManager == null)
        {
            return;
        }

        int trapCount = arenaManager.GetAllTiles().Count(t => t.Value.trap != null);
        if (trapCount >= trapEngineerTrapCount)
        {
            TryUnlock("trap_engineer", "Trap Engineer", "Place 10 traps in a single dungeon.");
        }
    }

    private void HandleCertificationCompleted(DungeonReport report, IReadOnlyList<RunResult> runs)
    {
        if (report == null || runs == null)
        {
            return;
        }

        if (runs.Any(r => !r.survived && !string.IsNullOrWhiteSpace(r.causeOfDeath) && r.causeOfDeath.Contains("Trap")))
        {
            TryUnlock("first_blood", "First Blood", "Kill a bot with a trap.");
        }

        if (string.Equals(report.rating, "Fair", System.StringComparison.OrdinalIgnoreCase))
        {
            TryUnlock("certified_architect", "Certified Architect", "Create a dungeon rated \"Fair\".");
        }

        if (runs.Count > 0 && runs.All(r => !r.survived))
        {
            TryUnlock("cruel_designer", "Cruel Designer", "Create a dungeon where all bots die.");
        }

        if (string.Equals(report.rating, "Impossible", System.StringComparison.OrdinalIgnoreCase))
        {
            TryUnlock("impossible_maze", "Impossible Maze", "Create a dungeon rated \"Impossible\".");
        }
    }

    private void TryUnlock(string id, string title, string description)
    {
        if (progressionManager == null)
        {
            return;
        }

        AchievementData data = new()
        {
            achievementID = id,
            title = title,
            description = description
        };

        bool unlocked = progressionManager.UnlockAchievement(data);
        if (!unlocked)
        {
            return;
        }

        _popupQueue.Enqueue(data);
        if (!_popupPlaying)
        {
            StartCoroutine(PlayPopupQueue());
        }
    }

    private System.Collections.IEnumerator PlayPopupQueue()
    {
        _popupPlaying = true;
        while (_popupQueue.Count > 0)
        {
            AchievementData next = _popupQueue.Dequeue();
            if (achievementPopup != null)
            {
                achievementPopup.Show(next);
                yield return new WaitForSecondsRealtime(3f);
            }
            else
            {
                yield return null;
            }
        }

        _popupPlaying = false;
    }
}
