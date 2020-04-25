using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public enum LevelState
    {
        Starting,
        Progress,
        End,
    }

    [SerializeField] InputManager m_inputManager = default;

    [SerializeField] Transform m_driverNode = default;
    [SerializeField] Transform m_playerNode = default;
    [SerializeField] Transform m_fireTruck = default;

    [SerializeField] Vector3 m_truckStartPosition = default;
    [SerializeField] Vector3 m_truckLevelPosition = default;

    [SerializeField] ScrollingObject[] m_backgroundParallax = default;

    public Action<LevelState> OnLevelStateChange = default;

    // Level State
    LevelState m_levelState = default;

    // Level Index
    int m_levelIndex = default;

    // Level Time
    float m_levelTime = default;

    // Character Asset
    Player m_levelPlayerCharacter = default;
    Player m_levelNonPlayerCharacter = default;

    void Awake()
    {
        OnLevelStateChange += LevelStateChange;

        // Level Details
        m_levelTime = GSTJ_Core.LevelMeta.Levels[m_levelIndex].LevelTime;

        // Character Details
        LoadCharacterAsset();

        for (int i = 0; i < m_backgroundParallax.Length; i++)
        {
            m_backgroundParallax[i].Stop();
        }
    }

    void Start()
    {
        m_fireTruck.transform.position = m_truckStartPosition;

        OnLevelStateChange?.Invoke(LevelState.Starting);
    }

    void LoadCharacterAsset()
    {
        m_levelPlayerCharacter =  Instantiate(Resources.Load<GameObject>(GSTJ_Core.CharacterMeta.Path + GSTJ_Core.SelectedPlayerCharacter.ToString())).GetComponent<Player>();
        m_levelNonPlayerCharacter = Instantiate(Resources.Load<GameObject>(GSTJ_Core.CharacterMeta.Path + GSTJ_Core.SelectedNonPlayerCharacter.ToString())).GetComponent<Player>();

        m_levelPlayerCharacter.Initialize(m_inputManager);
        m_levelPlayerCharacter.SetRole(Player.Role.Playable);
        m_levelPlayerCharacter.transform.SetParent(m_playerNode);
        m_levelPlayerCharacter.transform.localPosition = Vector3.zero;

        m_levelNonPlayerCharacter.Initialize(m_inputManager);        
        m_levelNonPlayerCharacter.SetRole(Player.Role.Driver);
        m_levelNonPlayerCharacter.transform.SetParent(m_driverNode);
        m_levelNonPlayerCharacter.transform.localPosition = Vector3.zero;
    }

    void LateUpdate()
    {
        switch(m_levelState)
        {
            case LevelState.Starting:
                m_fireTruck.position = Vector3.MoveTowards(m_fireTruck.position, m_truckLevelPosition, m_backgroundParallax[0].MoveSpeed * Time.deltaTime);

                if (Vector3.Distance(m_fireTruck.position, m_truckLevelPosition) <= 0.05f)
                {
                    m_fireTruck.position = m_truckLevelPosition;
                    OnLevelStateChange?.Invoke(LevelState.Progress);
                }
                break;
        }
    }

    void LevelStateChange(LevelState levelState)
    {
        m_levelState = levelState;

        switch (m_levelState)
        {
            case LevelState.Starting:
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Pause();
                }
                break;

            case LevelState.Progress:
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Play();
                }
                break;

            case LevelState.End:
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Pause();
                }
                break;
        }
    }

    void OnDestroy()
    {
        OnLevelStateChange -= LevelStateChange;       
    }
}
