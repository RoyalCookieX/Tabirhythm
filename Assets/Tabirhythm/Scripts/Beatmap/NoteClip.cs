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
        public WindowInfo Window => _window;
        public NoteAxis NoteAxis => _noteAxis;
        public int StepDistance => _stepDistance;
        public ClipCaps clipCaps => ClipCaps.None;
        
        [Header("Properties")]
        [SerializeField] private Note _prefab;
        [SerializeField] private NoteAxis _noteAxis;

        [HideInInspector] private WindowInfo _window;
        [HideInInspector] private int _stepDistance;

        public void OnInitialize(TempoInfo tempo, double duration)
        {
            if (!_prefab)
                return;

            int beatCount = Mathf.RoundToInt((float)(duration / tempo.BeatDuration));
            int actionCount = _prefab.ActionsPerBeat * beatCount;
            double holdTime = 0.0;
            double stepTime = 0.0;
            int stepDistance = 0;
            bool isHolding = false;
            WindowInfo window = new WindowInfo();

            for (int i = 0; i < actionCount; i++)
            {
                int actionIndex = i % _prefab.Actions.Length;
                NoteAction action = _prefab.Actions[actionIndex];
                double actionTime = i * tempo.BeatDuration / _prefab.ActionsPerBeat;
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
                        stepDistance++;
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
            _stepDistance = stepDistance;
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject)
        {
            if (!_prefab)
                return Playable.Null;

            var playable = ScriptPlayable<NotePlayable>.Create(graph);
            NotePlayable notePlayable = playable.GetBehaviour();
            notePlayable.prefabName = _prefab.name;
            notePlayable.noteAxis = _noteAxis;
            notePlayable.stepDistance = _stepDistance;
            return playable;
        }
    }
}
