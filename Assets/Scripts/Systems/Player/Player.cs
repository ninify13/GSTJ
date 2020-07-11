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

    [SerializeField] Transform m_waterHelper = default;

    [SerializeField] SpritePlayer m_waterSpriteTemplate = default;

    SpritePlayer m_waterSpritePlayer = default;

    Animator m_animController = default;

    Role m_role;

    InputManager m_inputManager;

    LevelManager m_levelManager;

    List<SpritePlayer> m_fire = new List<SpritePlayer>();


    public void Initialize(InputManager inputManager, LevelManager levelManager)
    {
        m_inputManager = inputManager;
        m_levelManager = levelManager;

        if (m_animController == null)
        {
            m_animController = GetComponent<Animator>();
        }

        m_animController.SetBool(CharacterStateConstants.MouseDownParam, true);
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

                GameObject waterSprite = Instantiate(m_waterSpriteTemplate.gameObject);
                m_waterSpritePlayer = waterSprite.GetComponent<SpritePlayer>();
                m_waterSpritePlayer.transform.SetParent(m_waterHelper);
                m_waterSpritePlayer.transform.localPosition = Vector3.zero;
                m_waterSpritePlayer.transform.localRotation = Quaternion.identity;
                m_waterSpritePlayer.gameObject.SetActive(false);

                m_inputManager.OnMouseDown += OnWaterStart;
                m_inputManager.OnMouseHold += OnWaterSpray;
                m_inputManager.OnMouseUp += OnWaterRelease;
                break;
        }
    }

    public void AddFire(SpritePlayer firePlayer)
    {
        m_fire.Add(firePlayer);
    }

    public void RemoveFire(SpritePlayer firePlayer)
    {
        m_fire.Remove(firePlayer);
    }

    void OnWaterStart(Vector3 mousePostiion)
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        if (m_levelManager.AvailableWater <= 0.0f  || m_levelManager.WaterFilling)
            return;

        m_waterSpritePlayer.gameObject.SetActive(true);

        m_waterSpritePlayer.SetClip(0);
        m_waterSpritePlayer.Play();
    }

    void OnWaterSpray(Vector3 mousePostiion)
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        if (m_levelManager.AvailableWater <= 0.0f || m_levelManager.WaterFilling)
        {
            m_waterSpritePlayer.gameObject.SetActive(false);
            return;
        }

        m_levelManager.ConsumeWater();

        for (int i = (m_fire.Count - 1); i >= 0; i--)
        {
            Transform fireTransform = m_fire[i].transform;

            bool dispel = false;
            float margin = 0.92f;
            if (m_levelManager.State == LevelManager.LevelState.BossFires)
            {
                dispel = true;
                margin = 0.98f;
            }
            else if (m_levelManager.State == LevelManager.LevelState.Progress)
            {
                if (fireTransform.position.x < 6.5f && fireTransform.position.x > -6.5f)
                {
                    dispel = true;
                    margin = 0.92f;
                }
            }

            if (dispel)
            {
                Vector3 dirFromAtoB = (fireTransform.position - m_waterHelper.position).normalized;
                float dotProd = Vector3.Dot(dirFromAtoB, m_waterHelper.forward);

                if (dotProd > margin)
                {
                    // Water helper is looking mostly towards fire
                    m_levelManager.CleanFire(m_fire[i].GetComponent<ScrollingObject>());
                    m_levelManager.AddScore(LevelManager.ScoreType.Fire);
                }
            }
        }
    }

    void OnWaterRelease(Vector3 mousePosition)
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        m_waterSpritePlayer.SetClip(1);
        m_waterSpritePlayer.Play();

        if (m_levelManager.AvailableWater <= 0.0f || m_levelManager.WaterFilling)
        {
            m_waterSpritePlayer.gameObject.SetActive(false);
            return;
        }
    }

    void LateUpdate()
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        m_animController.SetFloat(CharacterStateConstants.BlendParam, m_inputManager.MousePosition.y / (float)Screen.height);
    }

    void OnDestroy()
    {
        if (m_waterSpritePlayer != null)
        {
            Destroy(m_waterSpritePlayer.gameObject);
        }

        m_inputManager.OnMouseDown -= OnWaterStart;
        m_inputManager.OnMouseHold -= OnWaterSpray;
        m_inputManager.OnMouseUp -= OnWaterRelease;
    }
}
