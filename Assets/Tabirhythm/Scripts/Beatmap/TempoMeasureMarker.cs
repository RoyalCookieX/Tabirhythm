using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Tabirhythm
{
    [CustomStyle("TempoMeasure")]
    public class TempoMeasureMarker : Marker, INotification
    {
        public virtual PropertyName id => nameof(TempoMeasureMarker);
    }
}
