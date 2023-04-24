using UnityEngine;

namespace Tabirhythm
{
    public enum NoteAction
    {
        Rest,
        Step,
        Hold,
    }

    public class Note : MonoBehaviour
    {
        public int ActionsPerBeat => _actionsPerBeat;
        public NoteAction[] Actions => _actions;
        public string PrefabName { get; set; }
        public float StepSize { get; set; } = 1.0f;
        private Transform Root => _root ? _root : transform;

        [Header("Components")]
        [SerializeField] private Transform _root;

        [Header("Properties")]
        [SerializeField, Min(1)] private int _actionsPerBeat = 2;
        [SerializeField] private NoteAction[] _actions;
        [SerializeField] private AnimationCurve _stepCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
        [SerializeField] private AnimationCurve _holdCurve = AnimationCurve.Constant(0.0f, 1.0f, 1.0f);

        private Vector3 _startPosition;

        public void OnCreate()
        {
            gameObject.SetActive(false);
        }

        public void OnGet()
        {
            gameObject.SetActive(true);
            _startPosition = transform.position;
        }

        public void OnRelease()
        {
            gameObject.SetActive(false);
        }

        public void OnSetBeatTime(double beatTime)
        {
            double actionTime = beatTime * _actionsPerBeat;
            int flooredActionTime = Mathf.FloorToInt((float)actionTime);
            float normalizedActionTime = (float)(actionTime - flooredActionTime);
            NoteAction action = NoteAction.Rest;

            float distance = 0.0f;
            for (int i = 0; i <= flooredActionTime; i++)
            {
                bool interpolate = i == flooredActionTime;
                int actionIndex = i % _actions.Length;
                action = _actions[actionIndex];
                distance += action == NoteAction.Step
                    ? interpolate
                        ? _stepCurve.Evaluate(normalizedActionTime) * StepSize
                        : StepSize
                    : 0.0f;
            }
            float holdScale = action == NoteAction.Hold ? _holdCurve.Evaluate(normalizedActionTime) : 1.0f;

            transform.position = _startPosition + transform.forward * distance;
            Root.localScale = new Vector3(1.0f, holdScale, 1.0f);
        }
    }
}
