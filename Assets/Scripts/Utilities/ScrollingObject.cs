using System;
using UnityEngine;

public class ScrollingObject : MonoBehaviour
{
    public Action<ScrollingObject> OnScrollComplete = default;

    public float MoveSpeed { get { return m_moveSpeed; } }

    [SerializeField] Vector3 m_startPoint = default;
    [SerializeField] Vector3 m_endPoint = default;

    [SerializeField] float m_moveSpeed = default;

    [SerializeField] bool m_localPosition = default;

    [SerializeField] Transform m_objectTransform = default;

    [SerializeField] bool m_loop = true;

    public bool IsPlaying { get; private set; } = default;

    void OnEnable()
    {
        ResetObject();
        IsPlaying = false;
    }

    void ResetObject()
    {
        if (m_objectTransform == null)
        {
            m_objectTransform = transform;
        }

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
        if (IsPlaying)
        {
            if (m_localPosition)
            {
                m_objectTransform.localPosition = Vector3.MoveTowards(m_objectTransform.localPosition, m_endPoint, Time.deltaTime * m_moveSpeed);

                float distance = Vector3.Distance(m_objectTransform.localPosition, m_endPoint);
                if (distance < 0.05f)
                {
                    OnScrollComplete?.Invoke(this);

                    if (m_loop)
                    {
                        ResetObject();
                    }
                    else
                    {
                        Pause();
                    }
                }
            }
            else
            {
                m_objectTransform.position = Vector3.MoveTowards(m_objectTransform.position, m_endPoint, Time.deltaTime * m_moveSpeed);

                float distance = Vector3.Distance(m_objectTransform.position, m_endPoint);
                if (distance < 0.05f)
                {
                    OnScrollComplete?.Invoke(this);

                    if (m_loop)
                    {
                        ResetObject();
                    }
                    else
                    {
                        Pause();
                    }
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
        IsPlaying = false;
    }

    public void Play()
    {
        IsPlaying = true;
    }

    public void SetStartPoint(Vector3 start)
    {
        m_startPoint = start;
        ResetObject();
    }

    public void SetEndPoint(Vector3 end)
    {
        m_endPoint = end;
    }

    public void SetMoveSpeed(float speed)
    {
        m_moveSpeed = speed;
    }
}
