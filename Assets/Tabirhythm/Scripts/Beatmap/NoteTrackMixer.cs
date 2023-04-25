using UnityEngine.Playables;

namespace Tabirhythm
{
    public class NoteTrackMixer : PlayableBehaviour
    {
        public NotePool notePool;
        public HitQueue hitQueue;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!hitQueue)
                return;

            hitQueue.Time = playable.GetTime();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if (!notePool)
                return;
            notePool.DestroyPools();
        }
    }
}
