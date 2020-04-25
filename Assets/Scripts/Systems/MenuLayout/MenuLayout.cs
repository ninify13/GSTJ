using UnityEngine;
using UnityEngine.UI;

public class MenuLayout : MonoBehaviour
{
    protected MenuManager m_menuManager;

    [SerializeField] Button m_button_Back = default;

    protected virtual void Awake()
    {
        if (m_button_Back != null)
        {
            m_button_Back.onClick.AddListener(m_menuManager.BackButton);
        }
    }

    protected virtual void OnEnable()
    {
        
    }

    public void Init(MenuManager manager)
    {
        m_menuManager = manager;
    }

    protected virtual void OnDisable()
    {

    }

    protected virtual void OnDestroy()
    {
        if (m_button_Back != null)
        {
            m_button_Back.onClick.RemoveListener(m_menuManager.BackButton);
        }
    }
}
