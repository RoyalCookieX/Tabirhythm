using UnityEngine.Playables;

namespace Tabirhythm
{
    public class NotePlayable : PlayableBehaviour
    {
        public string prefabName;
        public NoteAxis noteAxis;
        public int stepDistance;
        public float beatsPerMinute;
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
            _instance = null;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!_instance)
                return;

            double beatTime = playable.GetTime() / 60.0 * beatsPerMinute;
            _instance.OnSetBeatTime(beatTime);
        }
    }
}
