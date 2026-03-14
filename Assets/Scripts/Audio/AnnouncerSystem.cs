using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AnnouncerSystem : MonoBehaviour
{
    [Serializable]
    private class AnnouncementLine
    {
        public AnnouncerEvent announcerEvent;
        [TextArea] public string text = string.Empty;
        public AudioClip voiceClip;
    }

    [SerializeField] private AudioSource voiceSource;
    [SerializeField] private TMP_Text announcerText;
    [SerializeField] private float textDisplayDuration = 2.5f;
    [SerializeField] private float announcementCooldown = 3f;
    [SerializeField] private AnnouncementLine[] lines;

    private float _lastAnnouncementTime = -999f;
    private Coroutine _textRoutine;

    public void Announce(AnnouncerEvent announcerEvent, float intensity = 1f)
    {
        if (Time.time - _lastAnnouncementTime < announcementCooldown)
        {
            return;
        }

        AnnouncementLine line = FindLine(announcerEvent);
        if (line == null)
        {
            return;
        }

        _lastAnnouncementTime = Time.time;

        if (announcerText != null)
        {
            if (_textRoutine != null)
            {
                StopCoroutine(_textRoutine);
            }

            _textRoutine = StartCoroutine(ShowTextRoutine(line.text));
        }

        if (voiceSource != null && line.voiceClip != null)
        {
            voiceSource.PlayOneShot(line.voiceClip, Mathf.Clamp01(intensity));
        }
    }

    private AnnouncementLine FindLine(AnnouncerEvent announcerEvent)
    {
        if (lines == null)
        {
            return null;
        }

        foreach (AnnouncementLine line in lines)
        {
            if (line != null && line.announcerEvent == announcerEvent)
            {
                return line;
            }
        }

        return null;
    }

    private IEnumerator ShowTextRoutine(string text)
    {
        announcerText.text = text;
        announcerText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(textDisplayDuration);
        announcerText.gameObject.SetActive(false);
    }
}
