using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Tabirhythm
{
    public class NoteClip : PlayableAsset, ITimelineClipAsset
    {
        public Note Prefab => _prefab;
        public ClipCaps clipCaps => ClipCaps.None;

        [SerializeField] private Note _prefab;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject)
        {
            return ScriptPlayable<NotePlayable>.Create(graph);
        }
    }
}
