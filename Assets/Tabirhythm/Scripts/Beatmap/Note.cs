using UnityEngine;

namespace Tabirhythm
{
    public class Note : MonoBehaviour
    {
        public string PrefabName { get; set; }

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
