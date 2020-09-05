using UnityEngine;
using Core.Pool;

namespace Game.Collectible
{
    public class Smoke : PoolItem
    {
        [SerializeField] SpritePlayer m_smokePlayer = default;

        public override void OnAllocate()
        {
            base.OnAllocate();
        }

        public void Init(Vector3 position)
        {
            transform.position = position;

            m_smokePlayer.SetClip(0);
            m_smokePlayer.Play();
        }

        public override void OnDeallocate()
        {
            base.OnDeallocate();
        }
    }
}