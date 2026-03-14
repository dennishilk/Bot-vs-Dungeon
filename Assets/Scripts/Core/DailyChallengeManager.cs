using System;
using System.IO;
using TMPro;
using UnityEngine;

public class DailyChallengeManager : MonoBehaviour
{
    [SerializeField] private DailyChallengeGenerator generator;
    [SerializeField] private DungeonSaveManager saveManager;
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private ReplayViewer replayViewer;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text budgetText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private CampaignManager campaignManager;

    [TextArea]
    [SerializeField] private string dailyObjectiveTemplate = "Survive the daily laboratory run.";

    private string _resultsFilePath;
    private DailyChallengeResultData _resultData;
    private DungeonSaveData _todaysDungeon;
    private string _challengeId;

    private void Awake()
    {
        _resultsFilePath = Path.Combine(Application.persistentDataPath, "daily_challenge_results.json");
        _challengeId = DateTime.UtcNow.ToString("yyyyMMdd");
        _todaysDungeon = generator != null ? generator.GenerateForDate(DateTime.UtcNow.Date) : null;
        LoadResults();
        EnsureDailyDungeonSaved();
        RefreshUI();

        if (simulationManager != null)
        {
            simulationManager.OnRunFinished += HandleRunFinished;
        }
    }

    private void OnDestroy()
    {
        if (simulationManager != null)
        {
            simulationManager.OnRunFinished -= HandleRunFinished;
        }
    }

    public void PlayChallenge()
    {
        if (_todaysDungeon == null || saveManager == null)
        {
            SetStatus("Daily challenge is unavailable.", false);
            return;
        }

        saveManager.ApplySaveData(_todaysDungeon, out _);
        bool started = simulationManager != null && simulationManager.StartSimulation();
        SetStatus(started ? "Daily challenge started." : "Could not start simulation.", started);
    }

    public void ViewResults()
    {
        if (_resultData == null)
        {
            SetStatus("No daily challenge attempts recorded.", false);
            return;
        }

        string best = _resultData.bestCompletionTime > 0 ? $"{_resultData.bestCompletionTime:0.00}s" : "--";
        SetStatus($"Attempts: {_resultData.attempts} | Completed: {_resultData.completed} | Best Time: {best}", true);
    }

    public void ReplayRuns()
    {
        if (replayViewer == null)
        {
            SetStatus("Replay viewer is not configured.", false);
            return;
        }

        bool ok = replayViewer.SelectReplay(0, out string msg);
        if (ok)
        {
            replayViewer.Play();
        }

        SetStatus(ok ? "Playing latest replay." : msg, ok);
    }

    private void HandleRunFinished(RunResult result)
    {
        if (_resultData == null || result == null)
        {
            return;
        }

        _resultData.attempts++;
        if (result.survived)
        {
            bool firstCompletion = !_resultData.completed;
            _resultData.completed = true;
            if (firstCompletion)
            {
                campaignManager?.AwardDailyChallengeCompletion();
            }
            _resultData.bestSurvived = true;
            if (_resultData.bestCompletionTime <= 0f || result.completionTime < _resultData.bestCompletionTime)
            {
                _resultData.bestCompletionTime = result.completionTime;
            }
        }

        SaveResults();
        RefreshUI();
    }

    private void EnsureDailyDungeonSaved()
    {
        if (_todaysDungeon == null || saveManager == null)
        {
            return;
        }

        saveManager.SaveDailyChallengeLayout($"daily_{_challengeId}", _todaysDungeon, out _);
    }

    private void RefreshUI()
    {
        if (titleText != null)
        {
            titleText.text = $"Today's Dungeon ({_challengeId})";
        }

        if (objectiveText != null)
        {
            objectiveText.text = dailyObjectiveTemplate;
        }

        if (budgetText != null && _todaysDungeon != null)
        {
            budgetText.text = $"Trap Budget: {_todaysDungeon.trapBudgetUsed}";
        }

        if (statusText != null)
        {
            statusText.text = _resultData != null && _resultData.completed
                ? "Status: Completed"
                : "Status: Not completed";
        }
    }

    private void SetStatus(string message, bool success)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = success ? new Color(0.5f, 0.95f, 0.6f) : new Color(0.95f, 0.45f, 0.45f);
        }
    }

    private void LoadResults()
    {
        if (!File.Exists(_resultsFilePath))
        {
            _resultData = new DailyChallengeResultData { challengeId = _challengeId, bestCompletionTime = -1f };
            return;
        }

        DailyChallengeResultData loaded = JsonUtility.FromJson<DailyChallengeResultData>(File.ReadAllText(_resultsFilePath));
        if (loaded == null || loaded.challengeId != _challengeId)
        {
            _resultData = new DailyChallengeResultData { challengeId = _challengeId, bestCompletionTime = -1f };
            SaveResults();
            return;
        }

        _resultData = loaded;
    }

    private void SaveResults()
    {
        if (_resultData == null)
        {
            return;
        }

        File.WriteAllText(_resultsFilePath, JsonUtility.ToJson(_resultData, true));
    }
}
