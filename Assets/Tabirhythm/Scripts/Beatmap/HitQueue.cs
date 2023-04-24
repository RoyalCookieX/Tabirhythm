using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Tabirhythm
{
    public enum HitAction
    {
        Release,
        Press,
    }

    public enum HitStatus
    {
        Miss,
        Early,
        Perfect,
        Late,
    }

    public struct HitResult
    {
        public HitStatus status;
        public double error;
    }

    public class HitQueue : MonoBehaviour
    {
        private record HitEvent
        {
            public bool next;
            public bool evaluated;
            public HitAction action;
            public double time;

            public HitEvent(bool next, HitAction action, double time)
            {
                this.next = next;
                this.evaluated = false;
                this.action = action;
                this.time = time;
            }
        }

        public double Time { get; set; }

        [Header("Events")]
        [SerializeField] private UnityEvent<int, HitResult> _onEvaluate;

        private Dictionary<int, Queue<HitEvent>> _hitEventTracks = new Dictionary<int, Queue<HitEvent>>();

        public void EnqueueNote(int trackId, WindowInfo window)
        {
            if (!_hitEventTracks.ContainsKey(trackId))
                _hitEventTracks.Add(trackId, new Queue<HitEvent>());

            Queue<HitEvent> hitEventTrack = _hitEventTracks[trackId];
            double hitEventTime = Time + window.time;
            switch (window.action)
            {
                case NoteAction.Rest:
                    break;
                case NoteAction.Step:
                {
                    hitEventTrack.Enqueue(new HitEvent(false, HitAction.Press, hitEventTime));
                    break;
                }
                case NoteAction.Hold:
                {
                    hitEventTrack.Enqueue(new HitEvent(true, HitAction.Press, hitEventTime));
                    hitEventTrack.Enqueue(new HitEvent(false, HitAction.Release, hitEventTime + window.holdDuration));
                    break;
                }
            }
        }

        public void Evaluate(int trackId, HitAction action, double windowDuration)
        {
            if (!_hitEventTracks.ContainsKey(trackId))
                return;

            Queue<HitEvent> hitEventTrack = _hitEventTracks[trackId];

            // get next hit event
            HitEvent hitEvent = null;
            foreach(HitEvent e in hitEventTrack)
            {
                if (e.evaluated)
                    continue;
                
                hitEvent = e;
                break;
            }
            if (hitEvent == null || hitEvent.action != action)
                return;

            double error = Time - hitEvent.time;
            double halfWindowDuration = windowDuration * 0.5f;
            HitStatus status = error < -halfWindowDuration
                ? HitStatus.Early
                : error > halfWindowDuration
                    ? HitStatus.Late
                    : HitStatus.Perfect;
            InvokeHit(trackId, error, status);
            hitEvent.evaluated = true;
        }

        public void DequeueNote(int trackId)
        {
            if (!_hitEventTracks.ContainsKey(trackId))
                return;

            Queue<HitEvent> hitEventTrack = _hitEventTracks[trackId];
            if (hitEventTrack.Count == 0)
                return;

            while(true)
            {
                HitEvent hitEvent = hitEventTrack.Dequeue();
                if (!hitEvent.evaluated)
                    InvokeHit(trackId, 0.0, HitStatus.Miss);
                if (!hitEvent.next)
                    break;
            }
        }

        private void InvokeHit(int trackId, double error, HitStatus status)
        {
            HitResult result = new HitResult { error = error, status = status };
            _onEvaluate?.Invoke(trackId, result);
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.fontSize = 30;

            float yOffset = 0.0f;
            float cornerOffset = 30.0f;
            float textWidth = 300.0f;
            float textHeight = 50.0f;
            foreach (var(trackId, hitEventTrack) in _hitEventTracks)
            {
                Rect trackRect = new Rect(cornerOffset, cornerOffset + yOffset, textWidth, textHeight);
                GUI.Label(trackRect, $"{trackId}", style);
                yOffset += textHeight;

                int hitEventIndex = 0;
                foreach(HitEvent hitEvent in hitEventTrack)
                {
                    Rect hitEventRect = new Rect(cornerOffset, cornerOffset + yOffset + hitEventIndex * textHeight, textWidth, textHeight);
                    GUI.Label(hitEventRect, $"[{hitEvent.time:#.####}]: {hitEvent.action} {(hitEvent.evaluated ? 'X' : 'O')}", style);
                    hitEventIndex++;
                }
                yOffset += hitEventTrack.Count * textHeight;
            }
        }
    }
}
