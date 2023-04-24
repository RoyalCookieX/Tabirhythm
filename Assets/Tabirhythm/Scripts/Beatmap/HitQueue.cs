using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
            public double time;
            public NoteAction noteAction;
            public HitAction hitAction;
            public Action<NoteAction, HitAction, HitStatus> actionOnHit;

            public HitEvent(bool next, double time, NoteAction noteAction, HitAction hitAction, Action<NoteAction, HitAction, HitStatus> actionOnHit)
            {
                this.next = next;
                this.evaluated = false;
                this.time = time;
                this.noteAction = noteAction;
                this.hitAction = hitAction;
                this.actionOnHit = actionOnHit;
            }

            public void InvokeHit(HitStatus hitStatus) => actionOnHit?.Invoke(noteAction, hitAction, hitStatus);
        }

        public double Time { get; set; }

        [Header("Events")]
        [SerializeField] private UnityEvent<int, HitResult> _onEvaluate;

        private Dictionary<int, Queue<HitEvent>> _hitEventTracks = new Dictionary<int, Queue<HitEvent>>();
        GUIStyle _style = new GUIStyle();

        public void EnqueueNote(int trackId, WindowInfo window, Action<NoteAction, HitAction, HitStatus> actionOnHit = null)
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
                    hitEventTrack.Enqueue(new HitEvent(false, hitEventTime, window.action, HitAction.Press, actionOnHit));
                    break;
                }
                case NoteAction.Hold:
                {
                    hitEventTrack.Enqueue(new HitEvent(true, hitEventTime, window.action, HitAction.Press, actionOnHit));
                    hitEventTrack.Enqueue(new HitEvent(false, hitEventTime + window.holdDuration, window.action, HitAction.Release, actionOnHit));
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
            
            if (hitEvent == null)
                return;

            if (hitEvent.hitAction != action)
                return;

            double error = Time - hitEvent.time;
            double halfWindowDuration = windowDuration * 0.5f;
            HitStatus status = error < -halfWindowDuration
                ? HitStatus.Early
                : error > halfWindowDuration
                    ? HitStatus.Late
                    : HitStatus.Perfect;
            InvokeEvaluate(trackId, error, status);
            hitEvent.InvokeHit(status);
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
                    InvokeMiss(trackId);

                if (!hitEvent.next)
                    break;
            }
        }

        private void InvokeEvaluate(int trackId, double error, HitStatus status)
        {
            HitResult result = new HitResult { error = error, status = status };
            _onEvaluate?.Invoke(trackId, result);
        }

        private void InvokeMiss(int trackId) => InvokeEvaluate(trackId, 0.0, HitStatus.Miss);

        private void OnEnable()
        {
            _style.normal.textColor = Color.black;
            _style.fontSize = 30;
        }

        private void OnGUI()
        {
            float yOffset = 0.0f;
            float cornerOffset = 30.0f;
            float textWidth = 100.0f;
            float textHeight = 30.0f;
            foreach (var(trackId, hitEventTrack) in _hitEventTracks)
            {
                Rect trackRect = new Rect(cornerOffset, cornerOffset + yOffset, textWidth, textHeight);
                GUI.Label(trackRect, $"{trackId}", _style);
                yOffset += textHeight;

                int hitEventIndex = 0;
                foreach(HitEvent hitEvent in hitEventTrack)
                {
                    Rect hitEventRect = new Rect(cornerOffset, cornerOffset + yOffset + hitEventIndex * textHeight, textWidth, textHeight);
                    GUI.Label(hitEventRect, $"[{hitEvent.time:#.####}]: {hitEvent.hitAction} {(hitEvent.evaluated ? 'X' : 'O')}", _style);
                    hitEventIndex++;
                }
                yOffset += hitEventTrack.Count * textHeight;
            }
        }
    }
}
