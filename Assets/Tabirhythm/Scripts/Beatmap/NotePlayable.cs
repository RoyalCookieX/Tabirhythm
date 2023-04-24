using UnityEngine.Playables;

namespace Tabirhythm
{
    public class NotePlayable : PlayableBehaviour
    {
        public string prefabName;
        public WindowInfo window;
        public NoteAxis noteAxis;
        public int stepDistance;
        public float beatsPerMinute;
        public NotePool notePool;
        public HitQueue hitQueue;

        private Note _instance;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!notePool || !hitQueue)
                return;

            _instance = notePool.GetNote(prefabName, noteAxis, stepDistance);
            hitQueue.EnqueueNote((int)noteAxis, window);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!notePool || !hitQueue)
                return;

            notePool.ReleaseNote(_instance);
            hitQueue.DequeueNote((int)noteAxis);
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
