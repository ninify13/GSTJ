using System;
using UnityEngine;

namespace Core.Pool
{
    [Serializable]
    public class PoolItem : MonoBehaviour
    {
        public GameObject Object => this.gameObject;

        public bool Allocated => Object.activeSelf;

        public virtual void OnAllocate()
        {
            Object.SetActive(true);
        }

        public virtual void OnDeallocate()
        {
            if (Object)
                Object.SetActive(false);
        }
    }
}
