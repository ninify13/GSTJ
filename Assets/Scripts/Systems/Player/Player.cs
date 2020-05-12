using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public enum Role
    {
        Driver,
        Playable,
    }

    [SerializeField] GameObject m_hose = default;

    Animator m_animController = default;

    Role m_role;

    InputManager m_inputManager;


    public void Initialize(InputManager inputManager)
    {
        m_inputManager = inputManager;

        if (m_animController == null)
        {
            m_animController = GetComponent<Animator>();
        }
    }

    public void SetRole(Role role)
    {
        m_role = role;

        switch(m_role)
        {
            case Role.Driver:
                m_animController.SetBool(CharacterStateConstants.DriverParam, true);
                m_hose.SetActive(false);
                break;

            case Role.Playable:
                m_animController.SetBool(CharacterStateConstants.DriverParam, false);
                m_hose.SetActive(true);
                m_inputManager.OnMouseHold += OnWaterSpray;
                m_inputManager.OnMouseUp += OnWaterRelease;
                break;
        }
    }

    void OnWaterSpray(Vector3 mousePostiion)
    {
        m_animController.SetBool(CharacterStateConstants.MouseDownParam, true);
        m_animController.SetFloat(CharacterStateConstants.BlendParam, mousePostiion.y / (float)Screen.height);
        if (mousePostiion.y < ((float)Screen.height / 3.0f))
        {

        }
        else
        if (mousePostiion.y > ((float)Screen.height / 3.0f))
        {

        }
        else
        {

        }
    }

    void OnWaterRelease(Vector3 mousePosition)
    {
        m_animController.SetBool(CharacterStateConstants.MouseDownParam, false);
    }

    void OnDestroy()
    {
        m_inputManager.OnMouseHold -= OnWaterSpray;
    }
}
