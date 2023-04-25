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
            hitQueue.EnqueueNote((int)noteAxis, window, (noteAction, hitAction, hitStatus) =>
            {
                switch (noteAction)
                {
                    case NoteAction.Step:
                    {
                        ReleaseNote();
                        break;
                    }
                    case NoteAction.Hold:
                    {
                        if (hitAction == HitAction.Release)
                            ReleaseNote();
                        break;
                    }
                }
            });
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (!notePool || !hitQueue)
                return;

            hitQueue.DequeueNote((int)noteAxis);
            ReleaseNote();
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!_instance)
                return;

            double beatTime = playable.GetTime() / 60.0 * beatsPerMinute;
            _instance.OnSetBeatTime(beatTime);
        }

        private void ReleaseNote()
        {
            notePool.ReleaseNote(_instance);
            _instance = null;
        }
    }
}
