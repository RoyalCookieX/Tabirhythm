using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Tabirhythm
{
    public enum NoteAxis
    {
        PosX,
        NegX,
        PosZ,
        NegZ,
    }

    public class NotePool : MonoBehaviour
    {
        private Transform Origin => _origin ? _origin : transform;

        [Header("Components")]
        [SerializeField] private Grid _grid;
        [SerializeField] private Transform _origin;

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
                actionOnDestroy: (instance) =>
                {
                    if (!instance)
                        return;
                    DestroyImmediate(instance.gameObject);
                });
            _pools.Add(prefabName, pool);
        }

        public Note GetNote(string prefabName, NoteAxis noteAxis, int stepDistance)
        {
            if (!_pools.ContainsKey(prefabName))
                return null;

            Vector3Int localDirection = noteAxis switch
            {
                NoteAxis.PosX => Vector3Int.right,
                NoteAxis.NegX => Vector3Int.left,
                NoteAxis.PosZ => Vector3Int.up,
                NoteAxis.NegZ => Vector3Int.down,
                _ => Vector3Int.zero,
            };
            Vector3 worldPosition;
            if (_grid)
            {
                Vector3Int originCell = _grid.WorldToCell(Origin.position);
                Vector3Int localCell = localDirection * stepDistance;
                Vector3Int worldCell = originCell + localCell;
                worldPosition = _grid.GetCellCenterWorld(worldCell);
            }
            else
            {
                worldPosition = Origin.position;
            }
            Quaternion worldRotation = Quaternion.LookRotation(-new Vector3(localDirection.x, 0.0f, localDirection.y));

            Note instance = _pools[prefabName].Get();
            instance.transform.SetPositionAndRotation(worldPosition, worldRotation);
            instance.OnGet();
            return instance;
        }

        public void ReleaseNote(Note instance)
        {
            if (!instance || !_pools.ContainsKey(instance.PrefabName))
                return;

            string prefabName = instance.PrefabName;
            instance.OnRelease();
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
