using UnityEngine.Playables;

namespace Tabirhythm
{
    public class NotePlayable : PlayableBehaviour
    {
        public string prefabName;
        public NoteAxis noteAxis;
        public int stepDistance;
        public NotePool notePool;

        private Note _instance;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!notePool)
                return;
            _instance = notePool.GetNote(prefabName, noteAxis, stepDistance);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!notePool)
                return;
            notePool.ReleaseNote(_instance);
        }
    }
}
