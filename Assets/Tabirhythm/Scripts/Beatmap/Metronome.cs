using UnityEngine;
using UnityEngine.Playables;

namespace Tabirhythm
{
    public class Metronome : MonoBehaviour, INotificationReceiver
    {
        [Header("Components")]
        [SerializeField] private AudioSource _audioSource;

        [Header("Properties")]
        [SerializeField] private AudioClip _beatClip;
        [SerializeField] private AudioClip _measureClip;

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            AudioClip audioClip;
            if (notification.GetType() == typeof(TempoBeatMarker))
                audioClip = _beatClip;
            else if (notification.GetType() == typeof(TempoMeasureMarker))
                audioClip = _measureClip;
            else
                return;

            _audioSource.PlayOneShot(audioClip);
        }
    }
}
