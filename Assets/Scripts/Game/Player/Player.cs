using System;
using System.Collections.Generic;
using UnityEngine;
using Core.InputManager;
using Game.Collectible;
using Game.Water;

public class Player : MonoBehaviour
{
    public enum Role
    {
        Driver,
        Playable,
    }

    [SerializeField] GameObject m_hose = default;

    [SerializeField] Transform m_waterHelper = default;

    [SerializeField] WaterPlayer m_waterPlayer = default;

    Animator m_animController = default;

    Role m_role;

    InputManager m_inputManager;

    LevelManager m_levelManager;

    List<Fire> m_fires = new List<Fire>();
    List<Coin> m_coins = new List<Coin>();
    List<EasterEgg> m_easterEggs = new List<EasterEgg>();

    Vector3 m_screenEnd = default;

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

                m_waterPlayer.gameObject.SetActive(false);
                break;

            case Role.Playable:
                m_animController.SetBool(CharacterStateConstants.DriverParam, false);
                m_hose.SetActive(true);

                m_waterPlayer.Init(m_waterHelper);

                m_inputManager.OnMouseDown += OnWaterStart;
                m_inputManager.OnMouseHold += OnWaterSpray;
                m_inputManager.OnMouseUp += OnWaterRelease;

                m_screenEnd = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.nearClipPlane));
                break;
        }
    }

    public void AddFire(Fire fire)
    {
        m_fires.Add(fire);
    }

    public void AddCoins(Coin coin)
    {
        m_coins.Add(coin);
    }

    public void AddEasterEgg(EasterEgg easterEgg)
    {
        m_easterEggs.Add(easterEgg);
    }

    void OnWaterStart(Vector3 mousePostiion)
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        if (mousePostiion.x < (Screen.width / 10.0f))
            return;

        if (m_levelManager.AvailableWater <= 0.0f  || m_levelManager.WaterFilling)
            return;
    }

    void OnWaterSpray(Vector3 mousePostiion)
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        if (mousePostiion.x < (Screen.width / 10.0f))
            return;

        if (m_levelManager.AvailableWater <= 0.0f || m_levelManager.WaterFilling)
        {
            m_waterPlayer.Stop();
            return;
        }

        m_waterPlayer.Spray(mousePostiion);
        m_levelManager.ConsumeWater();

        bool dispel = false;
        float margin = 0.92f;
        if (m_levelManager.State == LevelManager.LevelState.BossFires)
        {
            margin = 0.98f;
        }
        else if (m_levelManager.State == LevelManager.LevelState.Progress)
        {
            margin = 0.92f;
        }

        for (int i = (m_fires.Count - 1); i >= 0; i--)
        {
            Transform fireTransform = m_fires[i].transform;

            float distFromHelperToScreenEnd = Mathf.Abs(m_screenEnd.x - m_waterHelper.position.x);
            Vector2 waterConfigBounds = m_waterPlayer.GetXAxisBounds();
            Vector2 bounds = new Vector2(m_waterHelper.position.x/* + (distFromHelperToScreenEnd * waterConfigBounds.x)*/, m_waterHelper.position.x + (distFromHelperToScreenEnd * waterConfigBounds.y));
            float xPos = fireTransform.position.x;
            dispel = (xPos >= Math.Min(bounds.x, bounds.y) && xPos <= Math.Max(bounds.x, bounds.y));

            if (dispel)
            {
                Vector3 dirFromAtoB = (fireTransform.position - m_waterHelper.position).normalized;
                float dotProd = Vector3.Dot(dirFromAtoB, m_waterHelper.forward);

                if (dotProd > margin)
                {
                    // Water helper is looking mostly towards fire
                    m_levelManager.AddScore(LevelManager.ScoreType.Fire);
                    m_fires[i].Extinguish();
                    m_fires.Remove(m_fires[i]);
                }
            }
        }

        for (int i = (m_coins.Count - 1); i >= 0; i--)
        {
            Transform coinTransform = m_coins[i].transform;

            float distFromHelperToScreenEnd = Mathf.Abs(m_screenEnd.x - m_waterHelper.position.x);
            Vector2 waterConfigBounds = m_waterPlayer.GetXAxisBounds();
            Vector2 bounds = new Vector2(m_waterHelper.position.x/* + (distFromHelperToScreenEnd * waterConfigBounds.x)*/, m_waterHelper.position.x + (distFromHelperToScreenEnd * waterConfigBounds.y));
            float xPos = coinTransform.position.x;
            dispel = (xPos >= Math.Min(bounds.x, bounds.y) && xPos <= Math.Max(bounds.x, bounds.y));

            if (dispel)
            {
                Vector3 dirFromAtoB = (coinTransform.position - m_waterHelper.position).normalized;
                float dotProd = Vector3.Dot(dirFromAtoB, m_waterHelper.forward);

                if (dotProd > margin)
                {
                    // Water helper is looking mostly towards coin
                    m_levelManager.AddScore(LevelManager.ScoreType.Coin);
                    m_coins[i].Collect();
                    m_coins.Remove(m_coins[i]);
                }
            }
        }

        for (int i = (m_easterEggs.Count - 1); i >= 0; i--)
        {
            Transform easterEggTransform = m_easterEggs[i].transform;

            float distFromHelperToScreenEnd = Mathf.Abs(m_screenEnd.x - m_waterHelper.position.x);
            Vector2 waterConfigBounds = m_waterPlayer.GetXAxisBounds();
            Vector2 bounds = new Vector2(m_waterHelper.position.x/* + (distFromHelperToScreenEnd * waterConfigBounds.x)*/, m_waterHelper.position.x + (distFromHelperToScreenEnd * waterConfigBounds.y));
            float xPos = easterEggTransform.position.x;
            dispel = (xPos >= Math.Min(bounds.x, bounds.y) && xPos <= Math.Max(bounds.x, bounds.y));

            if (dispel)
            {
                Vector3 dirFromAtoB = (easterEggTransform.position - m_waterHelper.position).normalized;
                float dotProd = Vector3.Dot(dirFromAtoB, m_waterHelper.forward);

                if (dotProd > margin)
                {
                    // Water helper is looking mostly towards coin
                    m_levelManager.AddScore(LevelManager.ScoreType.Coin, 50);
                    m_easterEggs[i].Collect();
                    m_easterEggs.Remove(m_easterEggs[i]);
                }
            }
        }
    }

    void OnWaterRelease(Vector3 mousePosition)
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        m_waterPlayer.Stop();
    }

    void LateUpdate()
    {
        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        m_animController.SetFloat(CharacterStateConstants.BlendParam, m_inputManager.MousePosition.y / (float)Screen.height);
    }

    void OnDestroy()
    {
        m_inputManager.OnMouseDown -= OnWaterStart;
        m_inputManager.OnMouseHold -= OnWaterSpray;
        m_inputManager.OnMouseUp -= OnWaterRelease;
    }
}
