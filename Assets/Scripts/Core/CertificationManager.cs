using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CertificationManager : MonoBehaviour
{
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private DungeonReportPanel reportPanel;
    [SerializeField] private DeathMarkerSpawner deathMarkerSpawner;
    [SerializeField] private float delayBetweenRuns = 0.75f;
    [SerializeField] private bool showPathHistoryByDefault = true;
    [SerializeField] private float safeHpThreshold = 50f;
    [SerializeField] private float fairHpThreshold = 30f;

    private readonly List<RunResult> _runResults = new();

    public event Action<DungeonReport, IReadOnlyList<RunResult>> OnCertificationCompleted;
    private int _trapActivationEstimate;

    public bool IsCertificationRunning { get; private set; }
    public int CurrentRunIndex { get; private set; }
    public int TotalRunsInCertification { get; private set; }
    public DungeonReport LastReport { get; private set; }

    private void Awake()
    {
        if (simulationManager != null)
        {
            simulationManager.OnRunFinished += HandleRunFinished;
        }

        DebugPathVisualizer.ShowPathHistory = showPathHistoryByDefault;
    }

    private void OnDestroy()
    {
        if (simulationManager != null)
        {
            simulationManager.OnRunFinished -= HandleRunFinished;
        }
    }

    public void StartCertificationRun()
    {
        if (IsCertificationRunning || simulationManager == null)
        {
            return;
        }

        StartCoroutine(CertificationRoutine());
    }

    public void SetPathHistoryVisible(bool isVisible)
    {
        DebugPathVisualizer.ShowPathHistory = isVisible;
    }

    public void ClearMarkersAndHistory()
    {
        deathMarkerSpawner?.ClearMarkers();
        DebugPathVisualizer.ClearPathHistory();
    }

    private IEnumerator CertificationRoutine()
    {
        IsCertificationRunning = true;
        CurrentRunIndex = 0;
        TotalRunsInCertification = 3;
        _runResults.Clear();
        _trapActivationEstimate = 0;

        reportPanel?.Hide();
        ClearMarkersAndHistory();

        BotPersonality[] personalities =
        {
            BotPersonality.Careful,
            BotPersonality.Balanced,
            BotPersonality.Reckless
        };

        foreach (BotPersonality personality in personalities)
        {
            CurrentRunIndex++;
            bool runComplete = false;
            RunResult latest = null;

            void OnFinished(RunResult result)
            {
                latest = result;
                runComplete = true;
            }

            simulationManager.OnRunFinished += OnFinished;
            bool started = simulationManager.StartSimulationWithPersonality(personality, false);

            if (!started)
            {
                simulationManager.OnRunFinished -= OnFinished;
                continue;
            }

            EventLogger.Instance?.Log($"Certification run {CurrentRunIndex}/{TotalRunsInCertification}: {personality}");
            while (!runComplete)
            {
                yield return null;
            }

            simulationManager.OnRunFinished -= OnFinished;

            if (latest != null)
            {
                _runResults.Add(latest);
                if (!latest.survived)
                {
                    _trapActivationEstimate++;
                }

                deathMarkerSpawner?.SpawnMarkerForRun(latest);
            }

            yield return new WaitForSecondsRealtime(delayBetweenRuns);
        }

        DungeonReport report = DungeonReport.Build(_runResults, _trapActivationEstimate, safeHpThreshold, fairHpThreshold);
        LastReport = report;
        reportPanel?.ShowReport(report);
        OnCertificationCompleted?.Invoke(report, _runResults.AsReadOnly());
        IsCertificationRunning = false;
        CurrentRunIndex = 0;
        TotalRunsInCertification = 0;
    }

    private void HandleRunFinished(RunResult result)
    {
        if (!IsCertificationRunning)
        {
            deathMarkerSpawner?.SpawnMarkerForRun(result);
        }
    }
}
