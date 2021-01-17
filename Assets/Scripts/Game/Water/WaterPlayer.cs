using System;
using UnityEngine;
using System.Collections.Generic;

namespace Game.Water
{
    [Serializable]
    public class WaterConfig
    {
        public Vector2 WaterXAxisBounds = default;
        public SpritePlayer WaterPlayer = default;
        //for getting information about water tip
        public Transform WaterTip = default;
        //for rotating and scaling operations
        public Transform hinge = default;
    }

    public class WaterPlayer : MonoBehaviour
    {
        [SerializeField] WaterConfig[] m_waterConfigs = default;

        WaterConfig m_currentWaterConfig = null;

        //for maintaining information on which obj tip is colliding with 
        private List<Transform> colObjList = default;

        //for scaling the water stream
        float baseLen = 0.0f;

        public void Init(Transform parent)
        {
            transform.SetParent(parent);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            m_currentWaterConfig = m_waterConfigs[0];
            //resetting the collision list
            if (colObjList != null)
                colObjList.Clear();
            StopAll();

            //calculate base length 
            baseLen = (m_currentWaterConfig.WaterTip.position - m_currentWaterConfig.hinge.position).magnitude;
        }

        //for getting information about the collision of water tip
        public bool CheckCollisionWith(Transform obj)
        {
            //only if some list has been initialized
            if (colObjList == null)
                return false;
            //only if tip is colliding with anything
            if (colObjList.Count <= 0)
                return false;
            
            bool isColliding = false;
            for (int i = 0; i < colObjList.Count; i++)
            {
                if (colObjList[i] == obj)
                {
                    isColliding = true;
                    break;
                }
            }
            //return the result
            return isColliding;
        }
        //for use when we want to remove an obj after dipelling or collecting it
        public void RemoveFromList(Transform obj)
        {
            if (colObjList != null)
            {
                colObjList.Remove(obj);
            }
        }

        void OnTriggerEnter2D(Collider2D col)
        {
            //initialize if not done so already
            if (colObjList == null)
                colObjList = new List<Transform>();
            
            //check if we collided with fire
            //we have different trigger sizes for different fire types
            //so we need to get the parent's transform
            if (col.gameObject.name.Contains("Fire"))
                colObjList.Add(col.transform.parent);
            else
                colObjList.Add(col.transform);
        }

        public void Spray(Vector3 toPosition)
        {
            WaterConfig waterConfig = GetWaterConfig(toPosition.x);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(toPosition);
            //scale it according to input position
            float newLen, newScale = 0.0f;
            //get required scale (due to input)
            newLen = (worldPos - waterConfig.hinge.position).magnitude;
            //min and max scale mentioned below are determined by trial and error
            newScale = Mathf.Clamp(newLen/baseLen, 0.13f, 1.3f);
            //apply new scale 
            waterConfig.hinge.localScale = Vector3.one * newScale;

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
