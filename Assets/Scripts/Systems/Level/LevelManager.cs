using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public enum eLevelState
    {
        Starting,
        Progress,
        End,
    }

    [SerializeField] int[] m_levelTimes;
    int m_currentLevel = 0;
    int m_levelTime;
    float m_currentTime;

    void Start()
    {
        if (m_currentLevel < m_levelTimes.Length)
        {
            m_levelTime = m_levelTimes[m_currentLevel];
        }

        StartCoroutine(BeginCountdown());
    }

    IEnumerator BeginCountdown()
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log("3");
        yield return new WaitForSeconds(1.0f);
        Debug.Log("2");
        yield return new WaitForSeconds(1.0f);
        Debug.Log("1");
        yield return new WaitForSeconds(1.0f);
        Debug.Log("Start");
        yield return null;
    }

    private void LateUpdate()
    {
        m_currentTime += Time.deltaTime;

        if (m_currentTime >= m_levelTime)
        {
            // Trigger level end
        }
    }
}
