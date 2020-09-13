using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Core.InputManager;
using Core.Pool;
using Game.Collectible;

public class LevelManager : MonoBehaviour
{
    public enum LevelState
    {
        Countup,
        Starting,
        Paused,
        Progress,
        Boss,
        BossMovie,
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

    [SerializeField] AudioSource m_mainMusic = default;
    [SerializeField] AudioSource m_coinSound = default;

    [SerializeField] InputManager m_inputManager = default;

    [SerializeField] PoolManager m_poolManager = default;

    [SerializeField] Transform m_driverNode = default;
    [SerializeField] Transform m_playerNode = default;
    [SerializeField] Transform m_fireTruck = default;
    [SerializeField] Transform m_countUpTransform = default;

    [SerializeField] GameObject m_truckDust = default;
    [SerializeField] GameObject m_bossCharacter = default;

    [SerializeField] Vector3 m_truckStartPosition = default;
    [SerializeField] Vector3 m_truckLevelPosition = default;

    [SerializeField] ScrollingObject[] m_backgroundParallax = default;
    [SerializeField] ScrollingObject[] m_foregroundParallax = default;
    [SerializeField] ScrollingObject m_groundObject = default;
    [SerializeField] ScrollingObject m_bossObject = default;

    [SerializeField] HUD m_hud = default;
    [SerializeField] Pause m_pause = default;
    [SerializeField] Pause m_end = default;

    [SerializeField] Text m_countUpText = default;

    public System.Action<LevelState> OnLevelStateChange = default;

    // Level State
    LevelState m_prePauseLevelState = default;

    public LevelState State { get; private set; } = default;

    // Water vars
    public float AvailableWater { get; private set; } = default;
    public bool WaterFilling { get; private set; } = false;

    // Level Index
    int m_levelIndex = default;

    // Level Time
    Vector2 m_levelSpeedRanges = default;
    float m_levelTime = default;
    float m_elapsedLevelTime = default;
    float m_currentLevelSpeed = default;

    // Level Events Time
    float m_bossTime = default;

    // Fire Logic Vars
    float m_currentMaxFires = default;
    float m_lastFireSetSpawnTime = default;
    float m_timeToNextFireSet = default;

    float m_bossFireSpawnTime = default;
    float m_bossLastFireSpawn = default;
    int m_bossFireTotalCount = default;
    int m_bossFireCurrentCount = default;

    // Coin Time
    float m_coinSpawnTime = default;
    float m_lastCoinTime = default;

    // Easter egg time
    float m_easterEggSpawnTime = default;
    float m_lastEasterEggTime = default;

    // Score vars
    int m_coinsThisSession = default;
    int m_fireThisSession = default;

    // Foreground assets
    List<ScrollingObject> m_foregroundObjects = new List<ScrollingObject>();

    // Character Asset
    Player m_levelPlayerCharacter = default;
    Player m_levelNonPlayerCharacter = default;

    // Pool Objects
    List<Fire> m_firePoolItems = new List<Fire>();
    List<Coin> m_coinPoolItems = new List<Coin>();
    List<EasterEgg> m_easterEggItems = new List<EasterEgg>();

    // Misc
    int countUpIndex = default;

    void Awake()
    {
        m_levelIndex = PlayerPrefs.GetInt("LEVEL", 0);

        OnLevelStateChange += LevelStateChange;

        // Level Details
        m_levelSpeedRanges = GSTJ_Core.LevelMeta.Levels[m_levelIndex].LevelSpeedMinMax;
        m_levelTime = GSTJ_Core.LevelMeta.Levels[m_levelIndex].LevelTime;
        m_elapsedLevelTime = 0.0f;
        m_currentLevelSpeed = m_levelSpeedRanges.x;

        m_bossTime = GSTJ_Core.LevelMeta.Levels[m_levelIndex].BossTime;
        m_bossFireSpawnTime = (m_bossTime) / GSTJ_Core.LevelMeta.Levels[m_levelIndex].BossFires;

        m_coinSpawnTime = (m_levelTime - 5.0f) / GSTJ_Core.LevelMeta.Levels[m_levelIndex].Coins;
        m_lastCoinTime = Time.time;

        m_easterEggSpawnTime = (m_levelTime - 5.0f) / 3;
        m_lastEasterEggTime = Time.time;

        // Input handlers
        m_inputManager.OnMouseUp += OnMouseUp;

        // Character Details
        LoadCharacterAsset();

        m_bossCharacter.SetActive(false);

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
        AvailableWater = 1.0f;
        OnWaterFill();
        OnLevelStateChange?.Invoke(LevelState.Countup);

        m_pause.gameObject.SetActive(false);
        m_end.gameObject.SetActive(false);

        m_hud.EnableHUD(HUD.PlayerHUD.Player_01, false);
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

    void OnMouseUp(Vector3 mousePostiion)
    {
        WaterFilling = false;
    }

    public void OnWaterFill()
    {
        AvailableWater += ((GSTJ_Core.LevelMeta.Levels[m_levelIndex].WaterFillRate / 10f) * Time.deltaTime);
        AvailableWater = Mathf.Clamp(AvailableWater, 0.0f, 1.0f);

        m_hud.SetWaterValue(AvailableWater);

        WaterFilling = true;
    }

    public void ConsumeWater()
    {
        AvailableWater -= ((GSTJ_Core.LevelMeta.Levels[m_levelIndex].WaterReduceRate / 10f) * Time.deltaTime);
        AvailableWater = Mathf.Clamp(AvailableWater, 0.0f, 1.0f);

        m_hud.SetWaterValue(AvailableWater);
    }

    void LateUpdate()
    {
        switch (State)
        {
            case LevelState.Starting:
                m_fireTruck.position = Vector3.MoveTowards(m_fireTruck.position, m_truckLevelPosition, m_currentLevelSpeed * Time.deltaTime);
                if (Vector3.Distance(m_fireTruck.position, m_truckLevelPosition) <= 0.05f)
                {
                    m_fireTruck.position = m_truckLevelPosition;
                    OnLevelStateChange?.Invoke(LevelState.Progress);
                    StartCoroutine(SpawnRandomForeground());
                }
                break;

            case LevelState.Progress:
                m_elapsedLevelTime += Time.deltaTime;

                float speedDiff = m_levelSpeedRanges.y - m_levelSpeedRanges.x;
                float minSpeed = m_levelSpeedRanges.x;
                float normalizedTime = (m_elapsedLevelTime / m_levelTime);
                m_currentLevelSpeed = minSpeed + (normalizedTime * speedDiff);

                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].SetMoveSpeed(m_currentLevelSpeed / (i + 1));
                }

                for (int i = 0; i < m_foregroundParallax.Length; i++)
                {
                    m_foregroundParallax[i].SetMoveSpeed(m_currentLevelSpeed);
                }

                for (int i = 0; i < m_firePoolItems.Count; i++)
                {
                    m_firePoolItems[i].SetSpeed(m_currentLevelSpeed);
                }

                for (int i = 0; i < m_coinPoolItems.Count; i++)
                {
                    m_coinPoolItems[i].SetSpeed(m_currentLevelSpeed);
                }

                for (int i = 0; i < m_easterEggItems.Count; i++)
                {
                    m_easterEggItems[i].SetSpeed(m_currentLevelSpeed);
                }

                m_groundObject.SetMoveSpeed(m_currentLevelSpeed);


                if (m_elapsedLevelTime > m_levelTime)
                {
                    OnLevelStateChange?.Invoke(LevelState.Boss);    
                }
                else
                {
                    m_hud.SetProgress(HUD.PlayerHUD.Player_01, normalizedTime);
                }

                if (m_elapsedLevelTime < (m_levelTime - 5.0f))
                {
                    UpdateFires();

                    if ((Time.time - m_lastCoinTime) >= m_coinSpawnTime)
                    {
                        m_lastCoinTime = Time.time;

                        // Spawn coin at (Screen end, -2.0f, 0.0f)
                        Vector3 coinPosition = new Vector3(30.0f, -2.0f, 0.0f);
                        int coins = GSTJ_Core.LevelMeta.Levels[m_levelIndex].GetCoinClumpAmount();
                        for (int i = 0; i < coins; i++)
                        {
                            coinPosition.x += 1.5f;
                            PoolItem poolItem = m_poolManager.GetPoolItem(PoolType.Coin);
                            Coin coin = poolItem.gameObject.GetComponent<Coin>();
                            coin.Init(coinPosition, new Vector3(-coinPosition.x, coinPosition.y, coinPosition.z), new Coin.OnCollected(CollectCoin), m_poolManager);

                            m_coinPoolItems.Add(coin);
                            m_levelPlayerCharacter.AddCoins(coin);
                        }
                    }

                    if ((Time.time - m_lastEasterEggTime) >= m_easterEggSpawnTime)
                    {
                        m_lastEasterEggTime = Time.time;

                        // Spawn coin at (Screen end, -2.0f, 0.0f)
                        Vector3 eggPosition = new Vector3(30.0f, -2.0f, 0.0f);
                        PoolItem poolItem = m_poolManager.GetPoolItem(PoolType.EasterEgg);
                        EasterEgg easterEgg = poolItem.gameObject.GetComponent<EasterEgg>();
                        easterEgg.Init(eggPosition, new Vector3(-eggPosition.x, eggPosition.y, eggPosition.z), new EasterEgg.OnCollected(CollectEasterEgg), m_poolManager);

                        m_easterEggItems.Add(easterEgg);
                        m_levelPlayerCharacter.AddEasterEgg(easterEgg);
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
                            Vector2 randomPos = Random.insideUnitCircle;
                            Vector3 bossFirePosition = new Vector3(randomPos.x * 4f, randomPos.y * 4f, 0.0f);

                            PoolItem poolItem = m_poolManager.GetPoolItem(PoolType.Flame);
                            Fire fire = poolItem.gameObject.GetComponent<Fire>();
                            fire.Init(m_bossObject.transform.position + bossFirePosition, Vector3.zero, false, new Fire.OnExtinguished(ExtinguishFire), m_poolManager);
                            fire.SetSpeed(0.0f);

                            m_firePoolItems.Add(fire);
                            m_levelPlayerCharacter.AddFire(fire);

                            m_bossFireTotalCount++;
                            m_bossFireCurrentCount++;
                        }
                    }
                }
                else
                {
                    if (m_firePoolItems.Count <= 0)
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
            case LevelState.Countup:
                countUpIndex = 4;
                m_countUpTransform.gameObject.SetActive(true);
                StartCoroutine(DelayedCountUp());
                m_hud.EnableHUD(HUD.PlayerHUD.Player_01, false);
                break;

            case LevelState.Starting:
                StopCoroutine(DelayedCountUp());
                m_fireTruck.GetComponent<Animation>().Play();
                m_truckDust.gameObject.SetActive(true);
                m_hud.EnableHUD(HUD.PlayerHUD.Player_01, true);
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Pause();
                }

                for (int i = 0; i < m_foregroundParallax.Length; i++)
                {
                    m_foregroundParallax[i].Pause();
                }

                m_mainMusic.Play();
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
                m_bossObject.OnScrollComplete = StartBossMovie;
                break;

            case LevelState.BossMovie:
                m_fireTruck.GetComponent<Animation>().Stop();
                m_truckDust.gameObject.SetActive(false);
                for (int i = 0; i < m_backgroundParallax.Length; i++)
                {
                    m_backgroundParallax[i].Pause();
                }

                for (int i = 0; i < m_foregroundParallax.Length; i++)
                {
                    m_foregroundParallax[i].Pause();
                }

                m_bossCharacter.SetActive(true);
                m_bossCharacter.GetComponent<Animator>().SetInteger(CharacterStateConstants.BossRunCycle, 0);
                m_bossCharacter.GetComponent<ScrollingObject>().Play();
                m_bossCharacter.GetComponent<ScrollingObject>().OnScrollComplete = ShowBossDialog;
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

    IEnumerator DelayedCountUp()
    {
        while (State == LevelState.Countup)
        {
            countUpIndex--;
            m_countUpText.text = (countUpIndex == 0) ? "GO" : countUpIndex.ToString();
            m_countUpTransform.DOScale(2.0f, 0.5f).From(0.0f);

            yield return new WaitForSeconds(1f);
            
            if (countUpIndex <= 0)
            {
                m_countUpTransform.gameObject.SetActive(false);
                OnLevelStateChange?.Invoke(LevelState.Starting);
            }
        }
        
        yield return null;
    }

    IEnumerator SpawnRandomForeground()
    {
        while (State == LevelState.Progress)
        {
            ScrollingObject scrollingObject = m_foregroundParallax[Random.Range(0, m_foregroundParallax.Length)];
            if (scrollingObject != null && !scrollingObject.IsPlaying)
            {
                m_foregroundObjects.Add(scrollingObject);
                scrollingObject.Play();
                scrollingObject.OnScrollComplete += CleanForegroundScroll;
            }

            yield return new WaitForSeconds(Random.Range(1f, 3f));
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
                m_coinSound.Play();
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

    public void CleanForegroundScroll(ScrollingObject scrollingObject)
    {
        scrollingObject.Stop();
        m_foregroundObjects.Remove(scrollingObject);
        scrollingObject.OnScrollComplete -= CleanForegroundScroll;
        scrollingObject.OnScrollComplete = null;
    }

    #region Fires
    void UpdateFires()
    {
        if ((Time.time - m_lastFireSetSpawnTime) >= m_timeToNextFireSet)
        {
            float fireTime = (m_levelTime - 5.0f);
            float normalizedFireTime = m_elapsedLevelTime / fireTime;
            float diff = GSTJ_Core.LevelMeta.Levels[m_levelIndex].FireMinMaxPerSet.y - GSTJ_Core.LevelMeta.Levels[m_levelIndex].FireMinMaxPerSet.x;
            m_currentMaxFires = GSTJ_Core.LevelMeta.Levels[m_levelIndex].FireMinMaxPerSet.x + (normalizedFireTime * diff);
            m_currentMaxFires = Mathf.Clamp(m_currentMaxFires, GSTJ_Core.LevelMeta.Levels[m_levelIndex].FireMinMaxPerSet.x, GSTJ_Core.LevelMeta.Levels[m_levelIndex].FireMinMaxPerSet.y);

            m_lastFireSetSpawnTime = Time.time;
            m_timeToNextFireSet = Random.Range(GSTJ_Core.LevelMeta.Levels[m_levelIndex].TimeBetweenFireSets.x, GSTJ_Core.LevelMeta.Levels[m_levelIndex].TimeBetweenFireSets.y);

            for (int i = 0; i < Mathf.RoundToInt(m_currentMaxFires); i++)
            {
                // Spawn fire at (Screen end, Random y, 0.0f)
                Vector2 gaps = GSTJ_Core.LevelMeta.Levels[m_levelIndex].GapBetweenFiresInSet;
                Vector3 firePosition = new Vector3(30.0f + (i * (Random.Range(gaps.x, gaps.y))), Random.Range(0f, 6f), 0.0f);

                PoolItem poolItem = m_poolManager.GetPoolItem(PoolType.Flame);
                Fire fire = poolItem.gameObject.GetComponent<Fire>();
                fire.Init(firePosition, new Vector3(-firePosition.x, firePosition.y, firePosition.z), true, new Fire.OnExtinguished(ExtinguishFire), m_poolManager);
                fire.SetSpeed(m_currentLevelSpeed);

                m_firePoolItems.Add(fire);
                m_levelPlayerCharacter.AddFire(fire);
            }
        }
    }

    public void CollectCoin(Coin coin)
    {
        m_coinPoolItems.Remove(coin);
    }

    public void CollectEasterEgg(EasterEgg easterEgg)
    {
        m_easterEggItems.Remove(easterEgg);
    }

    public void ExtinguishFire(Fire fire)
    {
        m_firePoolItems.Remove(fire);
    }

    public void CleanFire(ScrollingObject scrollingObject)
    {
        m_bossFireCurrentCount--;
        m_bossFireCurrentCount = Mathf.Clamp(m_bossFireCurrentCount, 0, 100);
        scrollingObject.Pause();
        scrollingObject.OnScrollComplete -= CleanFire;
        scrollingObject.OnScrollComplete = null;
        if (scrollingObject.gameObject.GetComponent<SpritePlayer>().ClipCount > 1)
        {
            scrollingObject.gameObject.GetComponent<SpritePlayer>().SetClip(1);
            scrollingObject.gameObject.GetComponent<SpritePlayer>().Play();
        }
        else
        {
            scrollingObject.gameObject.GetComponent<SpritePlayer>().Stop();
        }
    }

    void StartBossMovie(ScrollingObject scrollingObject)
    {
        m_hud.EnablePause(false);
        scrollingObject.Pause();
        scrollingObject.OnScrollComplete = null;
        OnLevelStateChange(LevelState.BossMovie);
    }

    void ShowBossDialog(ScrollingObject scrollingObject)
    {
        m_bossCharacter.GetComponent<Animator>().SetInteger(CharacterStateConstants.BossRunCycle, 1);
        m_hud.EnableBossDialog(true);
    }

    public void RetractBoss()
    {
        m_hud.EnableBossDialog(false);
        ScrollingObject scrollingObject = m_bossCharacter.GetComponent<ScrollingObject>();
        scrollingObject.Pause();
        Vector3 start = scrollingObject.EndPoint;
        Vector3 end = scrollingObject.StartPoint;
        scrollingObject.SetStartPoint(start);
        scrollingObject.SetEndPoint(end);
        scrollingObject.Stop();
        scrollingObject.Play();
        scrollingObject.OnScrollComplete = StartBossFires;
        m_bossCharacter.GetComponent<Animator>().SetInteger(CharacterStateConstants.BossRunCycle, 2);
        m_bossCharacter.transform.eulerAngles = new Vector3(m_bossCharacter.transform.eulerAngles.x,
                                                            m_bossCharacter.transform.eulerAngles.y * -1,
                                                            m_bossCharacter.transform.eulerAngles.z);
    }

    void StartBossFires(ScrollingObject scrollingObject)
    {
        m_hud.EnablePause(true);
        m_bossCharacter.SetActive(false);
        scrollingObject.Pause();
        scrollingObject.OnScrollComplete = null;
        OnLevelStateChange(LevelState.BossFires);
    }
    #endregion

    void OnDestroy()
    {
        m_inputManager.OnMouseUp -= OnMouseUp;

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

            m_fireTruck.GetComponent<Animation>().Stop();
            m_truckDust.gameObject.SetActive(false);
        }
        for (int i = 0; i < m_firePoolItems.Count; i++)
        {
            m_firePoolItems[i].TogglePause(true);
        }

        for (int i = 0; i < m_coinPoolItems.Count; i++)
        {
            m_coinPoolItems[i].TogglePause(true);
        }

        for (int i = 0; i < m_easterEggItems.Count; i++)
        {
            m_easterEggItems[i].TogglePause(true);
        }

        m_mainMusic.Pause();
    }

    public void OnResume()
    {
        if ((int)m_prePauseLevelState < (int)LevelState.BossMovie)
        {
            m_fireTruck.GetComponent<Animation>().Play();
            m_truckDust.gameObject.SetActive(true);
        }
        
        OnLevelStateChange(m_prePauseLevelState);
        m_pause.gameObject.SetActive(false);
        m_hud.gameObject.SetActive(true);

        for (int i = 0; i < m_firePoolItems.Count; i++)
        {
            m_firePoolItems[i].TogglePause(false);
        }

        for (int i = 0; i < m_coinPoolItems.Count; i++)
        {
            m_coinPoolItems[i].TogglePause(false);
        }

        for (int i = 0; i < m_easterEggItems.Count; i++)
        {
            m_easterEggItems[i].TogglePause(false);
        }

        m_mainMusic.Play();
    }

    public void OnExit()
    {
        m_mainMusic.Stop();

        m_inputManager.OnMouseUp -= OnMouseUp;

        SceneManager.LoadScene(SceneConstants.Home);
    }
    #endregion
}