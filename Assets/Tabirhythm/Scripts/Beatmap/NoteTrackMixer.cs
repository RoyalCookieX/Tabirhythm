using UnityEngine.Playables;

namespace Tabirhythm
{
    public class NoteTrackMixer : PlayableBehaviour
    {
        public NotePool notePool;

        public override void OnPlayableDestroy(Playable playable)
        {
            if (!notePool)
                return;
            notePool.DestroyPools();
        }
    }
}
