using System;
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

        [Header("Properties")]
        [SerializeField, Min(1)] private int _actionsPerBeat = 2;
        [SerializeField] private NoteAction[] _actions;

        public void OnCreate()
        {
            gameObject.SetActive(false);
        }

        public void OnGet()
        {
            gameObject.SetActive(true);
        }

        public void OnRelease()
        {
            gameObject.SetActive(false);
        }
    }
}
