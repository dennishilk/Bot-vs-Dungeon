using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventLogger : MonoBehaviour
{
    public static EventLogger Instance { get; private set; }

    [SerializeField] private TMP_Text logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private int maxEventCount = 100;

    private readonly Queue<string> _events = new();

    public event Action<string> OnEventLogged;

    public IReadOnlyCollection<string> Events => _events;

    private void Awake()
    {
        Instance = this;
        RefreshText();
    }

    public void Log(string message)
    {
        string row = $"[{DateTime.Now:HH:mm:ss}] {message}";
        _events.Enqueue(row);

        while (_events.Count > maxEventCount)
        {
            _events.Dequeue();
        }

        RefreshText();
        OnEventLogged?.Invoke(row);
    }

    public void ClearLog()
    {
        _events.Clear();
        RefreshText();
    }

    private void RefreshText()
    {
        if (logText == null)
        {
            return;
        }

        logText.text = string.Join("\n", _events);

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
