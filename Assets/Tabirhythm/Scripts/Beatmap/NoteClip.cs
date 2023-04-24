using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Tabirhythm
{
    public struct WindowInfo
    {
        public NoteAction action;
        public double time;
        public double holdDuration;
    }

    public class NoteClip : PlayableAsset, ITimelineClipAsset
    {
        public Note Prefab => _prefab;
        public ClipCaps clipCaps => ClipCaps.None;
        public WindowInfo Window => _window;

        [Header("Properties")]
        [SerializeField] private Note _prefab;

        [HideInInspector] private WindowInfo _window;

        public void CalculateWindow(int beatCount, double beatDuration)
        {
            if (!_prefab)
                return;

            int actionCount = _prefab.ActionsPerBeat * beatCount;
            double holdTime = 0.0;
            double stepTime = 0.0;
            bool isHolding = false;
            WindowInfo window = new WindowInfo();

            for (int i = 0; i < actionCount; i++)
            {
                int actionIndex = i % _prefab.Actions.Length;
                NoteAction action = _prefab.Actions[actionIndex];
                double actionTime = i * beatDuration / _prefab.ActionsPerBeat;
                switch (action)
                {
                    case NoteAction.Rest:
                    {
#if UNITY_EDITOR
                        if (isHolding)
                            Debug.LogWarning($"Invalid action {nameof(NoteAction.Rest)} at index {actionIndex} after {nameof(NoteAction.Hold)}");
#endif
                        break;
                    }
                    case NoteAction.Step:
                    {
                        stepTime = actionTime;
                        if (isHolding)
                        {
                            window.time = holdTime;
                            window.holdDuration = actionTime - window.time;
                            isHolding = false;
                            break;
                        }

                        window.action = NoteAction.Step;
                        window.time = actionTime;
                        window.holdDuration = 0.0;
                        break;
                    }
                    case NoteAction.Hold:
                    {
                        if (!isHolding)
                        {
                            holdTime = actionTime;
                            isHolding = true;
                        }
                        window.action = NoteAction.Hold;
                        break;
                    }
                }
            }
            _window = window;

        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject)
        {
            var playable = ScriptPlayable<NotePlayable>.Create(graph);
            return playable;
        }
    }
}
