using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    [SerializeField] Text m_fireCount = default;
    [SerializeField] Text m_score = default;

    public void SetData(int fireCount, int score)
    {
        m_fireCount.text = fireCount.ToString();
        m_score.text = score.ToString();
    }
}
