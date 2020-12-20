using UnityEngine;
using Core.Pool;

namespace Game.Collectible
{
    public class EasterEgg : PoolItem
    {
        public delegate void OnCollected(EasterEgg egg);

        [SerializeField] GameObject[] EasterEggs = default;

        OnCollected m_onCollected = default;

        ScrollingObject m_scrollingObject = default;

        PoolManager m_poolManager = default;

        public override void OnAllocate()
        {
            base.OnAllocate();

            m_scrollingObject = gameObject.GetComponent<ScrollingObject>();

            for (int i = 0; i < EasterEggs.Length; i++)
            {
                EasterEggs[i].SetActive(false);
            }

            //allocate an easter egg
            EasterEggs[Random.Range(0, EasterEggs.Length)].SetActive(true);
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

        public void TogglePause(bool pause)
        {
            if (pause)
            {
                m_scrollingObject.Pause();
            }
            else
            {
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

        public Sprite GetSpriteResource()
        {
            //find out which easter egg is active
            int objID = -1;
            for (int i = 0; i < EasterEggs.Length; i++)
            {
                if (EasterEggs[i].gameObject.activeSelf)
                {
                    objID = i;
                    break;
                }
            }
            //now that the object ID is found return the sprite resource associated with it
            return EasterEggs[objID].GetComponent<SpriteRenderer>().sprite;
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

            base.OnDeallocate();
        }
    }
}