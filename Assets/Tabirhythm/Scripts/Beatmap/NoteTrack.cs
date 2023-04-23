using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Tabirhythm
{
    [TrackClipType(typeof(NoteClip))]
    public class NoteTrack : TrackAsset
    {
        [Header("Components")]
        [SerializeField] private ExposedReference<NotePool> _notePool;

        [HideInInspector] private TempoTrack _tempoTrack;

        protected override void OnCreateClip(TimelineClip clip)
        {
            InitializeNoteClip(clip);
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var trackMixer = ScriptPlayable<NoteTrackMixer>.Create(graph, inputCount);
            NoteTrackMixer noteTrackMixer = trackMixer.GetBehaviour();
            noteTrackMixer.notePool = _notePool.Resolve(graph.GetResolver());
            return trackMixer;
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            NoteClip noteClip = InitializeNoteClip(clip);
            Note prefab = noteClip.Prefab;

            NotePool notePool = _notePool.Resolve(graph.GetResolver());
            var playable = (ScriptPlayable<NotePlayable>)base.CreatePlayable(graph, gameObject, clip);
            NotePlayable notePlayable = playable.GetBehaviour();
            notePlayable.beatsPerMinute = _tempoTrack.Tempo.beatsPerMinute;
            notePlayable.notePool = notePool;
            notePlayable.notePool.CreatePool(prefab);
            return playable;
        }

        private void InitializeTempoTrack()
        {
            if (_tempoTrack)
                return;

            foreach (TrackAsset track in timelineAsset.GetRootTracks())
            {
                if (track.GetType() != typeof(TempoTrack))
                    continue;

                _tempoTrack = (TempoTrack)track;
                break;
            }
        }

        private NoteClip InitializeNoteClip(TimelineClip clip)
        {
            InitializeTempoTrack();
            NoteClip noteClip = (NoteClip)clip.asset;
            noteClip.OnInitialize(_tempoTrack.Tempo, clip.duration);
            return noteClip;
        }

        private void OnValidate()
        {
            InitializeTempoTrack();
        }
    }
}
