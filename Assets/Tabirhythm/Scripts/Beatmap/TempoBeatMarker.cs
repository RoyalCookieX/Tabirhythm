using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Tabirhythm
{
    [CustomStyle("TempoBeat")]
    public class TempoBeatMarker : Marker, INotification
    {
        public virtual PropertyName id => nameof(TempoBeatMarker);
    }
}
