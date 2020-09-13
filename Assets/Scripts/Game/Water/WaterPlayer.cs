using System;
using UnityEngine;

namespace Game.Water
{
    [Serializable]
    public class WaterConfig
    {
        public Vector2 WaterXAxisBounds = default;
        public SpritePlayer WaterPlayer = default;
    }

    public class WaterPlayer : MonoBehaviour
    {
        [SerializeField] WaterConfig[] m_waterConfigs = default;

        WaterConfig m_currentWaterConfig = null;

        public void Init(Transform parent)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            m_currentWaterConfig = m_waterConfigs[0];
            StopAll();
        }

        public void Spray(Vector3 toPosition)
        {
            WaterConfig waterConfig = GetWaterConfig(toPosition.x);

            if (!waterConfig.WaterPlayer.IsPlaying)
            {
                waterConfig.WaterPlayer.SetClip(0);
                waterConfig.WaterPlayer.Play();
            }
        }

        public Vector2 GetXAxisBounds()
        {
            return m_currentWaterConfig.WaterXAxisBounds;
        }

        public void Stop()
        {
            m_currentWaterConfig.WaterPlayer.Stop();
        }

        void StopAll()
        {
            for (int i = 0; i < m_waterConfigs.Length; i++)
            {
                m_waterConfigs[i].WaterPlayer.Stop();
            }
        }

        WaterConfig GetWaterConfig(float xPos)
        {
            Vector2 bounds = m_currentWaterConfig.WaterXAxisBounds * Screen.width;
            if (xPos >= Math.Min(bounds.x, bounds.y) && xPos <= Math.Max(bounds.x, bounds.y))
            {
                return m_currentWaterConfig;
            }
            else
            {
                for (int i = 0; i < m_waterConfigs.Length; i++)
                {
                    bounds = m_waterConfigs[i].WaterXAxisBounds * Screen.width;
                    if (xPos >= Math.Min(bounds.x, bounds.y) && xPos <= Math.Max(bounds.x, bounds.y))
                    {
                        m_currentWaterConfig.WaterPlayer.Stop();
                        m_currentWaterConfig = m_waterConfigs[i];
                        break;
                    }
                }
            }

            return m_currentWaterConfig;
        }
    }
}
