using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Tabirhythm
{
    [Serializable]
    public record PlayerInputTrackInfo
    {
        public NoteAxis noteAxis;
        public InputAction inputAction;
        [HideInInspector] public string name;
        [HideInInspector] public bool pressed = false;
    }

    public class PlayerController : MonoBehaviour
    {
        [Header("Properties")]
        [SerializeField] private PlayerInputTrackInfo[] _inputTracks;
        [SerializeField, Min(0.00f)] private float _windowDuration = 0.15f;
        [SerializeField] private float _debugHitResultDuration = 0.5f;

        [Header("Events")]
        [SerializeField] private UnityEvent<int, HitAction, double> _onTrackInput;

        private float _debugHitResultTimer = 0.0f;
        private string _debugHitResultDisplay;

        public void OnHit(int trackId, HitResult result)
        {
            _debugHitResultDisplay = $"{trackId}: {result.status} ({result.error:#.####})";
            _debugHitResultTimer = _debugHitResultDuration;
        }

        private void InvokeTrackInput(int trackId, HitAction hitAction)
        {
            _onTrackInput?.Invoke(trackId, hitAction, _windowDuration);
        }

        private void OnEnable()
        {
            foreach (PlayerInputTrackInfo inputTrack in _inputTracks)
            {
                inputTrack.inputAction.canceled += (_) =>
                {
                    InvokeTrackInput((int)inputTrack.noteAxis, HitAction.Release);
                    inputTrack.pressed = false;
                };
                inputTrack.inputAction.started += (_) =>
                {
                    InvokeTrackInput((int)inputTrack.noteAxis, HitAction.Press);
                    inputTrack.pressed = true;
                };
                inputTrack.inputAction.Enable();
                inputTrack.name = inputTrack.inputAction.bindings[0].ToDisplayString();
            }
        }

        private void Update()
        {
            if (_debugHitResultTimer > 0.0f)
                _debugHitResultTimer -= Time.deltaTime;
        }

        private void OnDisable()
        {
            foreach (PlayerInputTrackInfo inputTrack in _inputTracks)
            {
                inputTrack.inputAction.Disable();
                inputTrack.inputAction.Dispose();
            }
        }

        private void OnGUI()
        {
            GUIStyle normalStyle = new GUIStyle();
            normalStyle.fontSize = 30;
            normalStyle.normal.textColor = Color.black;

            GUIStyle disabledStyle = new GUIStyle(normalStyle);
            disabledStyle.normal.textColor = new Color(0.0f, 0.0f, 0.0f, 0.5f);

            float cornerOffset = 30.0f;
            float textWidth;
            float textHeight = 50.0f;

            if (_debugHitResultTimer > 0.0f)
            {
                textWidth = 300.0f;
                Rect hitResultRect = new Rect(Screen.width - cornerOffset - textWidth, Screen.height - cornerOffset - textHeight, textWidth, textHeight);
                GUI.Label(hitResultRect, _debugHitResultDisplay, normalStyle);
            }

            textWidth = 50.0f;
            for (int i = 0; i < _inputTracks.Length; i++)
            {
                GUIStyle style = _inputTracks[i].pressed ? normalStyle : disabledStyle;
                Rect inputRect = new Rect(cornerOffset + i * textWidth, Screen.height - cornerOffset - textHeight, textWidth, textHeight);
                GUI.Label(inputRect, _inputTracks[i].name, style);
            }
        }
    }
}
