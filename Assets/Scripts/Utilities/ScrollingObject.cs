using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingObject : MonoBehaviour
{
    public float MoveSpeed { get { return m_moveSpeed; } }

    [SerializeField] Vector3 m_startPoint = default;
    [SerializeField] Vector3 m_endPoint = default;

    [SerializeField] float m_moveSpeed = default;

    [SerializeField] bool m_localPosition = default;

    [SerializeField] Transform m_objectTransform = default;

    bool m_pause = default;

    void OnEnable()
    {
        ResetObject();
        m_pause = true;
    }

    void ResetObject()
    {
        if (m_localPosition)
        {
            m_objectTransform.localPosition = m_startPoint;
        }
        else
        {
            m_objectTransform.position = m_startPoint;
        }
    }

    void LateUpdate()
    {
        if (!m_pause)
        {
            if (m_localPosition)
            {
                m_objectTransform.localPosition = Vector3.MoveTowards(m_objectTransform.localPosition, m_endPoint, Time.deltaTime * m_moveSpeed);

                float distance = Vector3.Distance(m_objectTransform.localPosition, m_endPoint);
                if (distance < 0.05f)
                {
                    ResetObject();
                }
            }
            else
            {
                m_objectTransform.position = Vector3.MoveTowards(m_objectTransform.position, m_endPoint, Time.deltaTime * m_moveSpeed);

                float distance = Vector3.Distance(m_objectTransform.position, m_endPoint);
                if (distance < 0.05f)
                {
                    ResetObject();
                }
            }
        }
    }

    public void Stop()
    {
        ResetObject();
    }

    public void Pause()
    {
        m_pause = true;
    }

    public void Play()
    {
        m_pause = false;
    }
}
