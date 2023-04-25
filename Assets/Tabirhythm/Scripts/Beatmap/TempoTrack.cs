using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace Tabirhythm
{
    public struct TempoInfo
    {
        public double BeatDuration => 60.0 / beatsPerMinute;
        public double DivisionDuration => BeatDuration / divisionsPerBeat;

        public int divisionsPerBeat;
        public float beatsPerMinute;
        public int beatsPerMeasure;
    }

    [Serializable]
    [TrackBindingType(typeof(Metronome))]
    public class TempoTrack : TrackAsset
    {
        private enum TempoMarkerType
        {
            Division,
            Beat,
            Measure,
        }

        public TempoInfo Tempo => new TempoInfo
        {
            divisionsPerBeat = _divisionsPerBeat,
            beatsPerMinute = _beatsPerMinute,
            beatsPerMeasure = _beatsPerMeasure,
        };

        [Header("Properties")]
        [SerializeField, Min(1)] private int _divisionsPerBeat = 2;
        [SerializeField, Min(0.001f)] private float _beatsPerMinute = 120.0f;
        [SerializeField, Range(1, 4)] private int _beatsPerMeasure = 4;

        [ContextMenu("Destroy Markers")]
        public void DestroyMarkers()
        {
            int markerCount = GetMarkerCount();
            for (int i = markerCount - 1; i >= 0; i--)
                DeleteMarker(GetMarker(i));
        }

        [ContextMenu("Create Markers")]
        public void CreateMarkers()
        {
            DestroyMarkers();

            float musicDuration = (float)timelineAsset.duration;

            // exit when there is no duration
            if (Mathf.Approximately(musicDuration, 0.0f))
                return;

            // create new markers
            int divisionCount = Mathf.RoundToInt(musicDuration / (float)Tempo.DivisionDuration);
            for (int division = GetMarkerCount(); division < divisionCount; division++)
            {
                double time = division * Tempo.DivisionDuration;
                TempoMarkerType type = GetTempoMarkerType(division);
                Marker marker = type switch
                {
                    TempoMarkerType.Division => CreateMarker<TempoDivisionMarker>(time),
                    TempoMarkerType.Beat => CreateMarker<TempoBeatMarker>(time),
                    TempoMarkerType.Measure => CreateMarker<TempoMeasureMarker>(time),
                    _ => null,
                };
                marker.time = time;
            }
        }
        private TempoMarkerType GetTempoMarkerType(int division)
        {
            if (division % (Tempo.divisionsPerBeat * Tempo.beatsPerMeasure) == 0)
                return TempoMarkerType.Measure;
            else if (division % Tempo.divisionsPerBeat == 0)
                return TempoMarkerType.Beat;
            else
                return TempoMarkerType.Division;
        }

        private void OnValidate()
        {
            timelineAsset.editorSettings.frameRate = _beatsPerMinute / 60.0 * _divisionsPerBeat;
        }
    }
}
