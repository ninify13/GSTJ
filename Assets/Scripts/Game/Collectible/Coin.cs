using UnityEngine;
using Core.Pool;

namespace Game.Collectible
{
    public class Coin : PoolItem
    {
        public delegate void OnCollected(Coin fire);

        [SerializeField] SpritePlayer m_coinPlayer = default;

        OnCollected m_onCollected = default;

        ScrollingObject m_scrollingObject = default;

        PoolManager m_poolManager = default;

        public override void OnAllocate()
        {
            base.OnAllocate();

            m_scrollingObject = gameObject.GetComponent<ScrollingObject>();

            m_coinPlayer.SetClip(0);
            m_coinPlayer.Play();
        }

        public void Init(Vector3 startPosition, Vector3 endPosition, OnCollected onCollected, PoolManager poolManager)
        {
            m_scrollingObject.SetStartPoint(startPosition);
            m_scrollingObject.SetEndPoint(endPosition);

            m_scrollingObject.OnScrollComplete += OnCollect;
            m_scrollingObject.Play();

            m_onCollected = onCollected;

            m_poolManager = poolManager;
        }

        //adding a method to override endpoint position
        public void OverrideYPos(float newY)
        {
            Vector3 newPos = m_scrollingObject.StartPoint;
            newPos.y = newY;
            m_scrollingObject.SetStartPoint(newPos);
            newPos = m_scrollingObject.EndPoint;
            newPos.y = newY;
            m_scrollingObject.SetEndPoint(newPos);
        }

        public void TogglePause(bool pause)
        {
            if (pause)
            {
                m_coinPlayer.Pause();
                m_scrollingObject.Pause();
            }
            else
            {
                m_coinPlayer.Play();
                m_scrollingObject.Play();
            }
        }

        public void SetSpeed(float speed)
        {
            m_scrollingObject.SetMoveSpeed(speed);
        }

        public void Collect()
        {
            OnCollect(m_scrollingObject);
        }

        void OnCollect(ScrollingObject scrollingObject)
        {
            scrollingObject.Stop();

            m_onCollected?.Invoke(this);

            m_poolManager.ReturnPoolItem(this);
        }

        public override void OnDeallocate()
        {
            m_onCollected = null;

            m_scrollingObject.Stop();
            m_scrollingObject.OnScrollComplete -= OnCollect;

            m_coinPlayer.Stop();

            base.OnDeallocate();
        }
    }
}