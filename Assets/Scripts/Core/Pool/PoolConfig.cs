using System;
using UnityEngine;

namespace Core.Pool
{
    [CreateAssetMenu(fileName = "PoolConfig", menuName = "Meta/PoolConfig", order = 0)]
    public class PoolConfig : ScriptableObject
    {
        [SerializeField] public PoolItemConfig[] PoolItems;
    }

    [Serializable]
    public class PoolItemConfig
    {
        public PoolType Type;
        public PoolItem Item;
        public int PreBakeCount = 5;
    }
}
