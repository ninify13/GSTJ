using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public enum LevelState
    {
        Starting,
        Paused,
        Progress,
        Boss,
        BossFires,
        End,
    }

    public enum ScoreType
    {
        Min = -1,
        Coin,
        Fire,
        Score,
        Max,
    }

    [SerializeField] InputManager m_inputManager = default;

    [SerializeField] Transform m_driverNode = default;
    [SerializeField] Transform m_playerNode = default;
    [SerializeField] Transform m_fireTruck = default;

    [SerializeField] Vector3 m_truckStartPosition = default;
    [SerializeField] Vector3 m_truckLevelPosition = default;

    [SerializeField] ScrollingObject[] m_backgroundParallax = default;
    [SerializeField] ScrollingObject[] m_foregroundParallax = default;
    [SerializeField] ScrollingObject m_bossObject = default;

    [SerializeField] SpritePlayer[] m_fireSprites_Top = default;
    [SerializeField] SpritePlayer[] m_fireSprites_Bottom = default;
    [SerializeField] SpritePlayer[] m_coinSprites = default;

    [SerializeField] HUD m_hud = default;
    [SerializeField] Pause m_pause = default;
    [SerializeField] Pause m_end = default;

    public Action<LevelState> OnLevelStateChange = default;

    // Level State
    LevelState m_prePauseLevelState = default;

    public LevelState State { get; private set; } = default;

    // Level Index
    int m_levelIndex = default;

    // Level Time
    Vector2 m_levelSpeedRanges = default;
    float m_levelTime = default;
    float m_elapsedLevelTime = default;
    float m_currentLevelSpeed = default;

    // Level Events Time
    float m_bossTime = default;

    // Fire Time
    float m_fireSpawnTime = default;
    float m_bossFireSpawnTime = default;
    float m_lastFireSpawn = default;
    float m_bossLastFireSpawn = default;
    int m_bossFireTotalCount = default;
    int m_bossFireCurrentCount = default;

    // Coin Time
    float m_coinSpawnTime = default;
    float m_lastCoinTime = default;

    // Score vars
    int m_coinsThisSession = default;
    int m_fireThisSession = default;

    // Foreground assets
    List<ScrollingObject> m_foregroundObjects = new List<ScrollingObject>();

    // Character Asset
    Player m_levelPlayerCharacter = default;
    Player m_levelNonPlayerCharacter = default;

    void Awake()
    {
        OnLevelStateChange += LevelStateChange;

        // Level Details
        m_levelSpeedRanges = GSTJ_Core.LevelMeta.Levels[m_levelIndex].LevelSpeedMinMax;
        m_levelTime = GSTJ_Core.LevelMeta.Levels[m_levelIndex].LevelTime;
        m_bossTime = GSTJ_Core.LevelMeta.Levels[m_levelIndex].BossTime;

        m_fireSpawnTime = (m_levelTime - 5.0f) / GSTJ_Core.LevelMeta.Levels[m_levelIndex].Fires;
        m_bossFireSpawnTime = (m_bossTime) / GSTJ_Core.LevelMeta.Levels[m_levelIndex].BossFires;
        m_lastFireSpawn = Time.time;

        m_coinSpawnTime = (m_levelTime - 5.0f) / GSTJ_Core.LevelMeta.Levels[m_levelIndex].Coins;
        m_lastCoinTime = Time.time;

        // Input handlers
        m_inputManager.OnMouseDown += OnMouseDown;

        // Character Details
        LoadCharacterAsset();

        for (int i = 0; i < m_backgroundParallax.Length; i++)
        {
            m_backgroundParallax[i].Stop();
        }

        for (int i = 0; i < m_foregroundParallax.Length; i++)
        {
            m_foregroundParallax[i].Stop();
        }

        m_bossObject.gameObject.SetActive(false);
    }

    void Start()
    {
        m_fireTruck.transform.position = m_truckStartPosition;

        OnLevelStateChange?.Invoke(LevelState.Starting);

        m_pause.gameObject.SetActive(false);
        m_end.gameObject.SetActive(false);

        m_hud.EnableHUD(HUD.PlayerHUD.Player_01, true);
        m_hud.EnableHUD(HUD.PlayerHUD.Player_02, false);

        m_hud.SetProgress(HUD.PlayerHUD.Player_01, 0);
        m_hud.SetProgress(HUD.PlayerHUD.Player_02, 0);

        for (int i = 0; i < (int)ScoreType.Max; i++)
        {
            AddScore((ScoreType)i, 0);
        }
    }

    void LoadCharacterAsset()
    {
        m_levelPlayerCharacter =  Instantiate(Resources.Load<GameObject>(GSTJ_Core.CharacterMeta.Path + GSTJ_Core.SelectedPlayerCharacter.ToString())).GetComponent<Player>();
        m_levelNonPlayerCharacter = Instantiate(Resources.Load<GameObject>(GSTJ_Core.CharacterMeta.Path + GSTJ_Core.SelectedNonPlayerCharacter.ToString())).GetComponent<Player>();

        m_levelPlayerCharacter.Initialize(m_inputManager, this);
        m_levelPlayerCharacter.SetRole(Player.Role.Playable);
        m_levelPlayerCharacter.transform.SetParent(m_playerNode);
        m_levelPlayerCharacter.transform.localPosition = Vector3.zero;

        m_levelNonPlayerCharacter.Initialize(m_inputManager, this);        
        m_levelNonPlayerCharacter.SetRole(Player.Role.Driver);
        m_levelNonPlayerCharacter.transform.SetParent(m_driverNode);
        m_levelNonPlayerCharacter.transform.localPosition = Vector3.zero;
    }


    void OnMouseDown(Vector3 mousePostiion)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.tag == "Coin")
            {
                hit.transform.gameObject.GetComponent<SpritePlayer>().Stop();
                hit.transform.gameObject.SetActive(false);
                AddScore(ScoreType.Coin);
            }
        }
    }

    void LateUpdate()
    {
        switch(State)
        {
            case LevelState.Starting:
                m_fireTruck.position = Vector3.MoveTowards(m_fireTruck.position, m_truckLevelPosition, m_backgroundParallax[0].MoveSpeed * Time.deltaTime);
                if (Vector3.Distance(m_fireTruck.position, m_truckLevelPosition) <= 0.05f)
                {
                    m_fireTruck.position = m_truckLevelPosition;
                    OnLevelStateChange?.Invoke(LevelState.Progress);
                    m_elapsedLevelTime = 0.0f;
                    m_currentLevelSpeed = m_levelSpeedRanges.x;
                    StartCoroutine(SpawnRandomForeground());
                }
                break;

            case LevelState.Progress:
                m_elapsedLevelTime += Time.deltaTime;
                if (m_elapsedLevelTime > m_levelTime)
                {
                    OnLevelStateChange?.Invoke(LevelState.Boss);    
                }
                else
                {
                    float normalizedTime = m_elapsedLevelTime / m_levelTime;
                    m_hud.SetProgress(HUD.PlayerHUD.Player_01, normalizedTime);
                }

                if (m_elapsedLevelTime < (m_levelTime - 5.0f))
                {
                    if ((Time.time - m_lastFireSpawn) >= m_fireSpawnTime)
                    {
                        m_lastFireSpawn = Time.time;

                        // Spawn fire at (Screen end, Random y, 0.0f)
                        SpritePlayer fireSprite = null;
                        int position = UnityEngine.Random.Range(0, 4);
                        Vector3 firePosition = Vector3.zero;
                        int index = 0;
                        switch (position)
                        {
                            case 0:
                            case 1:
                                //Top
                                firePosition = new Vector3(30.0f, UnityEngine.Random.Range(0f, 6f), 0.0f);
                                do
                                {
                                    index = UnityEngine.Random.Range(0, m_fireSprites_Top.Length);
                                }
                                while (m_fireSprites_Top[index].IsPlaying);
                                m_fireSprites_Top[index].gameObject.SetActive(true);
                                m_fireSprites_Top[index].Play();
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().SetStartPoint(firePosition);
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().SetEndPoint(new Vector3(-firePosition.x, firePosition.y, firePosition.z));
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().Play();
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().OnScrollComplete += CleanFire;
                                fireSprite = m_fireSprites_Top[index];
                                break;

                            case 2:
                            case 3:
                                //Bottom
                                firePosition = new Vector3(30.0f, UnityEngine.Random.Range(-1f, -6f), 0.0f);
                                do
                                {
                                    index = UnityEngine.Random.Range(0, m_fireSprites_Bottom.Length);
                                }
                                while (m_fireSprites_Bottom[index].IsPlaying);
                                m_fireSprites_Bottom[index].gameObject.SetActive(true);
                                m_fireSprites_Bottom[index].Play();
                                m_fireSprites_Bottom[index].GetComponent<ScrollingObject>().SetStartPoint(firePosition);
                                m_fireSprites_Bottom[index].GetComponent<ScrollingObject>().SetEndPoint(new Vector3(-firePosition.x, firePosition.y, firePosition.z));
                                m_fireSprites_Bottom[index].GetComponent<ScrollingObject>().Play();
                                m_fireSprites_Bottom[index].GetComponent<ScrollingObject>().OnScrollComplete += CleanFire;
                                fireSprite = m_fireSprites_Bottom[index];
                                break;

                            default:
                                //Top
                                firePosition = new Vector3(30.0f, UnityEngine.Random.Range(0f, 6f), 0.0f);
                                do
                                {
                                    index = UnityEngine.Random.Range(0, m_fireSprites_Top.Length);
                                }
                                while (m_fireSprites_Top[index].IsPlaying);
                                m_fireSprites_Top[index].gameObject.SetActive(true);
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().SetStartPoint(firePosition);
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().SetEndPoint(new Vector3(-firePosition.x, firePosition.y, firePosition.z));
                                m_fireSprites_Top[index].Play();
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().Play();
                                m_fireSprites_Top[index].GetComponent<ScrollingObject>().OnScrollComplete += CleanFire;
                                fireSprite = m_fireSprites_Top[index];
                                break;
                        }

                        m_levelPlayerCharacter.AddFire(fireSprite);
                    }

                    if ((Time.time - m_lastCoinTime) >= m_coinSpawnTime)
                    {
                        m_lastCoinTime = Time.time;

                        // Spawn coin at (Screen end, Random y, 0.0f)
                        Vector3 coinPosition = Vector3.zero;
                        int index = 0;
                        coinPosition = new Vector3(30.0f, UnityEngine.Random.Range(-2f, 8f), 0.0f);
                        do
                        {
                            index = UnityEngine.Random.Range(0, m_coinSprites.Length);
                        }
                        while (m_coinSprites[index].IsPlaying);
                        m_coinSprites[index].gameObject.SetActive(true);
                        m_coinSprites[index].Play();
                        m_coinSprites[index].GetComponent<ScrollingObject>().SetStartPoint(coinPosition);
                        m_coinSprites[index].GetComponent<ScrollingObject>().SetEndPoint(new Vector3(-coinPosition.x, coinPosition.y, coinPosition.z));
                        m_coinSprites[index].GetComponent<ScrollingObject>().Play();
                        m_coinSprites[index].GetComponent<ScrollingObject>().OnScrollComplete += CleanFire;
                    }
                }
                break;

            case LevelState.BossFires:
                //Boss
                if (m_bossFireTotalCount < GSTJ_Core.LevelMeta.Levels[m_levelIndex].BossFires)
                {
                    if (m_bossFireCurrentCount < 2)
                    {
                        if ((Time.time - m_bossLastFireSpawn) >= m_bossFireSpawnTime)
                        {
                            m_bossLastFireSpawn = Time.time;
                            Vector2 randomPos = UnityEngine.Random.insideUnitCircle;
                            Vector3 bossFirePosition = new Vector3(randomPos.x * 4f, randomPos.y * 4f, 0.0f);
                            int bossFireIndex = 0;

                            if (m_bossFireCurrentCount < m_fireSprites_Top.Length)
                            {
                                do
                                {
                                    bossFireIndex = UnityEngine.Random.Range(0, m_fireSprites_Top.Length);
                                }
                                while (m_fireSprites_Top[bossFireIndex].IsPlaying);
                            }

                            m_fireSprites_Top[bossFireIndex].gameObject.SetActive(true);
                            m_fireSprites_Top[bossFireIndex].GetComponent<ScrollingObject>().enabled = false;
                            m_fireSprites_Top[bossFireIndex].Play();
                            m_fireSprites_Top[bossFireIndex].transform.position = m_bossObject.transform.position + bossFirePosition;
                            SpritePlayer bossFireSprite = m_fireSprites_Top[bossFireIndex];
                            m_levelPlayerCharacter.AddFire(bossFireSprite);
                            m_bossFireTotalCount++;
                            m_bossFireCurrentCount++;
                        }
                    }
                }
                else
                {
                    bool levelFinished = true;
                    for (int i = 0; i < m_fireSprites_Top.Length; i++)
                    {
                        if (m_fireSprites_Top[i].gameObject.activeSelf)
                        {
                            levelFinished = false;
                        }
                    }

                    if (levelFinished)
                    {
                        OnLevelStateChange(LevelState.End);
                    }
                }
                break;
        }
    }

    void LevelStateChange(LevelState levelState)
    {
        State = levelState;

        switch (State)
        {
            case LevelState.Starting:
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Pause();
                }

                for (int i = 0; i < m_foregroundParallax.Length; i++)
                {
                    m_foregroundParallax[i].Pause();
                }
                break;

            case LevelState.Paused:
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Pause();
                }

                for (int i = 0; i < m_foregroundParallax.Length; i++)
                {
                    m_foregroundParallax[i].Pause();
                }
                break;

            case LevelState.Progress:
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Play();
                }

                break;

            case LevelState.Boss:
                m_bossObject.gameObject.SetActive(true);
                m_bossObject.Play();
                m_bossObject.OnScrollComplete = StartBossFires;
                break;

            case LevelState.BossFires:
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Pause();
                }

                for (int i = 0; i < m_foregroundParallax.Length; i++)
                {
                    m_foregroundParallax[i].Pause();
                }
                break;

            case LevelState.End:
                m_bossObject.gameObject.SetActive(false);
                OnPause();
                break;
        }
    }

    IEnumerator SpawnRandomForeground()
    {
        while (State == LevelState.Progress)
        {
            ScrollingObject scrollingObject = m_foregroundParallax[UnityEngine.Random.Range(0, m_foregroundParallax.Length)];
            if (scrollingObject != null && !scrollingObject.IsPlaying)
            {
                m_foregroundObjects.Add(scrollingObject);
                scrollingObject.Play();
                scrollingObject.OnScrollComplete += CleanForegroundScroll;
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 3f));
        }

        yield return null;
    }

    public void AddScore(ScoreType scoreType, int value = 1)
    {
        int score = PlayerPrefs.GetInt(scoreType.ToString(), 0);
        score += value;
        PlayerPrefs.SetInt(scoreType.ToString(), score);

        switch (scoreType)
        {
            case ScoreType.Coin:
                m_coinsThisSession += value;
                m_hud.SetHUDCount(HUD.PlayerHUD.Player_01, scoreType, m_coinsThisSession);
                break;

            case ScoreType.Fire:
                m_fireThisSession += value;
                m_hud.SetHUDCount(HUD.PlayerHUD.Player_01, scoreType, m_fireThisSession);
                break;
        }

        int overallScore = PlayerPrefs.GetInt(ScoreType.Score.ToString(), 0);
        overallScore += value;
        PlayerPrefs.SetInt(ScoreType.Score.ToString(), overallScore);

        m_hud.SetHUDCount(HUD.PlayerHUD.Player_01, ScoreType.Score, m_coinsThisSession +  m_fireThisSession);
    }

    void CleanForegroundScroll(ScrollingObject scrollingObject)
    {
        scrollingObject.Stop();
        m_foregroundObjects.Remove(scrollingObject);
        scrollingObject.OnScrollComplete -= CleanForegroundScroll;
        scrollingObject.OnScrollComplete = null;
    }

    public void CleanFire(ScrollingObject scrollingObject)
    {
        m_bossFireCurrentCount--;
        m_bossFireCurrentCount = Mathf.Clamp(m_bossFireCurrentCount, 0, 100);
        scrollingObject.Stop();
        scrollingObject.OnScrollComplete -= CleanFire;
        scrollingObject.OnScrollComplete = null;
        scrollingObject.gameObject.GetComponent<SpritePlayer>().Stop();
        scrollingObject.gameObject.SetActive(false);
    }

    void StartBossFires(ScrollingObject scrollingObject)
    {
        scrollingObject.Pause();
        scrollingObject.OnScrollComplete = null;
        OnLevelStateChange(LevelState.BossFires);
    }

    void OnDestroy()
    {
        OnLevelStateChange -= LevelStateChange;       
    }

    #region Buttons
    public void OnPause()
    {
        if (State == LevelState.End)
        {
            m_end.gameObject.SetActive(true);
            m_hud.gameObject.SetActive(false);
            m_end.SetData(m_fireThisSession, m_fireThisSession + m_coinsThisSession);
        }
        else
        {
            m_prePauseLevelState = State;
            OnLevelStateChange(LevelState.Paused);
            m_pause.gameObject.SetActive(true);
            m_hud.gameObject.SetActive(false);
            m_pause.SetData(m_fireThisSession, m_fireThisSession + m_coinsThisSession);
        }
        for (int i = 0; i < m_fireSprites_Top.Length; i++)
        {
            if (m_fireSprites_Top[i].IsPlaying)
            {
                m_fireSprites_Top[i].Pause();
                m_fireSprites_Top[i].GetComponent<ScrollingObject>().Pause();
            }
        }

        for (int i = 0; i < m_fireSprites_Bottom.Length; i++)
        {
            if (m_fireSprites_Bottom[i].IsPlaying)
            {
                m_fireSprites_Bottom[i].Pause();
                m_fireSprites_Bottom[i].GetComponent<ScrollingObject>().Pause();
            }
        }

        for (int i = 0; i < m_coinSprites.Length; i++)
        {
            if (m_coinSprites[i].IsPlaying)
            {
                m_coinSprites[i].Pause();
                m_coinSprites[i].GetComponent<ScrollingObject>().Pause();
            }
        }
    }

    public void OnResume()
    {
        OnLevelStateChange(m_prePauseLevelState);
        m_pause.gameObject.SetActive(false);
        m_hud.gameObject.SetActive(true);

        for (int i = 0; i < m_fireSprites_Top.Length; i++)
        {
            if (m_fireSprites_Top[i].gameObject.activeSelf)
            {
                m_fireSprites_Top[i].Play();
                m_fireSprites_Top[i].GetComponent<ScrollingObject>().Play();
            }
        }

        for (int i = 0; i < m_fireSprites_Bottom.Length; i++)
        {
            if (m_fireSprites_Bottom[i].gameObject.activeSelf)
            {
                m_fireSprites_Bottom[i].Play();
                m_fireSprites_Bottom[i].GetComponent<ScrollingObject>().Play();
            }
        }

        for (int i = 0; i < m_coinSprites.Length; i++)
        {
            if (m_coinSprites[i].gameObject.activeSelf)
            {
                m_coinSprites[i].Play();
                m_coinSprites[i].GetComponent<ScrollingObject>().Play();
            }
        }
    }

    public void OnExit()
    {
        SceneManager.LoadScene(SceneConstants.Home);
    }
    #endregion
}