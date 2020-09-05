using UnityEngine;
using Core.Pool;

namespace Game.Collectible
{
    public class Fire : PoolItem
    {
        public delegate void OnExtinguished(Fire fire);

        [SerializeField] SpritePlayer[] m_firePlayers = default;

        PoolManager m_poolManager = default;

        OnExtinguished m_onExtinguished = default;

        ScrollingObject m_scrollingObject = default;

        int m_firePlayerIndex = default;

        bool m_scroll = default;

        public override void OnAllocate()
        {
            base.OnAllocate();

            m_scrollingObject = gameObject.GetComponent<ScrollingObject>();

            m_firePlayerIndex = Random.Range(0, m_firePlayers.Length);
            m_firePlayers[m_firePlayerIndex].gameObject.SetActive(true);
            m_firePlayers[m_firePlayerIndex].SetClip(0);
            m_firePlayers[m_firePlayerIndex].Play();
        }

        public void Init(Vector3 startPosition, Vector3 endPosition, bool scroll, OnExtinguished onExtinguished, PoolManager poolManager)
        {
            m_scrollingObject.SetStartPoint(startPosition);
            m_scrollingObject.SetEndPoint(endPosition);

            m_scroll = scroll;

            m_onExtinguished = onExtinguished;

            if (m_scroll)
            {
                m_scrollingObject.OnScrollComplete += OnExtinguish;
                m_scrollingObject.Play();
            }

            m_poolManager = poolManager;
        }

        public void SetSpeed(float speed)
        {
            m_scrollingObject.SetMoveSpeed(speed);
        }

        public void TogglePause(bool pause)
        {
            if (pause)
            {
                m_firePlayers[m_firePlayerIndex].Pause();
                m_scrollingObject.Pause();
            }
            else
            {
                m_firePlayers[m_firePlayerIndex].Play();

                if (m_scroll)
                {
                    m_scrollingObject.Play();
                }
            }
        }

        public void Extinguish()
        {
            PoolItem poolItem = m_poolManager.GetPoolItem(PoolType.Smoke);
            poolItem.GetComponent<Smoke>().Init(transform.position);

            OnExtinguish(m_scrollingObject);
        }    

        void OnExtinguish(ScrollingObject scrollingObject)
        {
            scrollingObject.Stop();

            m_onExtinguished?.Invoke(this);

            m_poolManager.ReturnPoolItem(this);
        }

        public override void OnDeallocate()
        {
            m_onExtinguished = null;

            m_scrollingObject.Stop();
            m_scrollingObject.OnScrollComplete -= OnExtinguish;

            m_firePlayers[m_firePlayerIndex].Stop();
            m_firePlayers[m_firePlayerIndex].gameObject.SetActive(false);

            base.OnDeallocate();
        }
    }
}
