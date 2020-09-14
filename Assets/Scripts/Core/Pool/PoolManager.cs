using System.Collections.Generic;
using UnityEngine;

namespace Core.Pool
{
    public enum PoolType
    {
        CharacterItem,
        Coin,
        EasterEgg,
        Flame,
        Smoke,
    }

    public class PoolManager : MonoBehaviour
    {
        [SerializeField] PoolConfig m_poolConfig = default;

        Dictionary<string, List<PoolItem>> m_poolItems = new Dictionary<string, List<PoolItem>>();
        Dictionary<string, Transform> m_poolParents = new Dictionary<string, Transform>();

        private void Start()
        {
            for (int i = 0; i < m_poolConfig.PoolItems.Length; i++)
            {
                if (!m_poolItems.ContainsKey(m_poolConfig.PoolItems[i].Type.ToString()))
                {
                    m_poolItems.Add(m_poolConfig.PoolItems[i].Type.ToString(), new List<PoolItem>());

                    GameObject poolParent = new GameObject(m_poolConfig.PoolItems[i].Type.ToString());
                    poolParent.transform.SetParent(transform);
                    poolParent.transform.localPosition = Vector3.zero;

                    m_poolParents.Add(m_poolConfig.PoolItems[i].Type.ToString(), poolParent.transform);
                }

                for (int j = 0; j < m_poolConfig.PoolItems[i].PreBakeCount; j++)
                {
                    GameObject newPoolObject = Instantiate(m_poolConfig.PoolItems[i].Item.Object);
                    PoolItem newPoolItem = newPoolObject.GetComponent<PoolItem>();

                    newPoolObject.transform.SetParent(m_poolParents[m_poolConfig.PoolItems[i].Type.ToString()]);
                    newPoolObject.SetActive(false);

                    m_poolItems[m_poolConfig.PoolItems[i].Type.ToString()].Add(newPoolItem);
                }
            }
        }

        PoolItem SpawnPoolItem(PoolType poolType)
        {
            for (int i = 0; i < m_poolConfig.PoolItems.Length; i++)
            {
                if (m_poolConfig.PoolItems[i].Type == poolType)
                {
                    GameObject newPoolObject = Instantiate(m_poolConfig.PoolItems[i].Item.Object);
                    PoolItem newPoolItem = newPoolObject.GetComponent<PoolItem>();

                    newPoolObject.transform.SetParent(m_poolParents[m_poolConfig.PoolItems[i].Type.ToString()]);
                    newPoolObject.SetActive(false);

                    m_poolItems[poolType.ToString()].Add(newPoolItem);

                    return newPoolItem;
                }
            }

            return null;
        }

        public PoolItem GetPoolItem(PoolType poolType)
        {
            if (!m_poolItems.ContainsKey(poolType.ToString()))
            {
                Debug.LogError("No pool item configured for type: " + poolType);
                return null;
            }

            PoolItem poolItem = null;
            for (int i = 0; i < m_poolItems[poolType.ToString()].Count; i++)
            {
                if (!m_poolItems[poolType.ToString()][i].Allocated)
                {
                    poolItem = m_poolItems[poolType.ToString()][i];
                    break;
                }
            }

            if (poolItem == null)
            {
                poolItem = SpawnPoolItem(poolType);
            }

            poolItem.OnAllocate();
            return poolItem;
        }

        public void ReturnPoolItem(PoolItem poolItem)
        {
            poolItem.OnDeallocate();
        }

        private void OnDestroy()
        {
            foreach(KeyValuePair<string, List<PoolItem>> poolItems in m_poolItems)
            {
                for (int i = 0; i < poolItems.Value.Count; i++)
                {
                    if (poolItems.Value[i].Object == null)
                        continue;

                    Destroy(poolItems.Value[i].Object);
                }
            }
        }
    }
}