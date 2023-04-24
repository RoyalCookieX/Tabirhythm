using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Tabirhythm
{
    public class NotePool : MonoBehaviour
    {
        private Dictionary<string, ObjectPool<Note>> _pools = new Dictionary<string, ObjectPool<Note>>();

        public void CreatePool(Note prefab)
        {
            string prefabName = prefab.name;
            if (_pools.ContainsKey(prefabName))
                return;

            var pool = new ObjectPool<Note>(
                () =>
                {
                    Note instance = Instantiate(prefab, transform.position, transform.rotation);
                    instance.PrefabName = prefabName;
                    instance.OnCreate();
                    return instance;
                },
                actionOnGet: (instance) =>
                {
                    if (!instance)
                        return;
                    instance.OnGet();
                },
                actionOnRelease: (instance) =>
                {
                    if (!instance)
                        return;
                    instance.OnRelease();
                },
                actionOnDestroy: (instance) =>
                {
                    if (!instance)
                        return;
                    DestroyImmediate(instance.gameObject);
                });
            _pools.Add(prefabName, pool);
        }

        public Note GetNote(string prefabName)
        {
            if (!_pools.ContainsKey(prefabName))
                return null;

            Note instance = _pools[prefabName].Get();
            return instance;
        }

        public void ReleaseNote(Note instance)
        {
            if (!instance || !_pools.ContainsKey(instance.PrefabName))
                return;

            string prefabName = instance.PrefabName;
            _pools[prefabName].Release(instance);
        }

        public void DestroyPools()
        {
            foreach (var (_, pool) in _pools)
                pool.Dispose();
            _pools.Clear();
        }
    }
}
