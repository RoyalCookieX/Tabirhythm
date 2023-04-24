using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Tabirhythm
{
    [CustomStyle("TempoDivision")]
    public class TempoDivisionMarker : Marker, INotification
    {
        public virtual PropertyName id => nameof(TempoDivisionMarker);
    }
}
