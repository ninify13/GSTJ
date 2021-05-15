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
        FTUE,
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
    [SerializeField] AudioSource m_waterEmptySound = default;
    public void PlayWaterEmptyAudio()
    {
        m_waterEmptySound.Play();
    }
    [SerializeField] AudioSource m_itemCollectionSound = default;
    public void PlayItemColAudio()
    {
        m_itemCollectionSound.Play();
    }
    [SerializeField] AudioSource m_countDownSound = default;
    [SerializeField] AudioSource m_raceBeginSound = default;
    [SerializeField] InputManager m_inputManager = default;

    [SerializeField] PoolManager m_poolManager = default;

    [SerializeField] Transform m_driverNode = default;
    [SerializeField] Transform m_playerNode = default;
    [SerializeField] Transform m_fireTruck = default;
    [SerializeField] Transform m_bgPanelTransform = default;
    [SerializeField] Transform m_countUpTransform = default;
    [SerializeField] Transform m_countUpPreTextTransform = default;
    [SerializeField] Transform m_opponentSearchTextTransform = default;
    [SerializeField] GameObject m_truckDust = default;
    [SerializeField] GameObject m_bossCharacter = default;

    [SerializeField] Vector3 m_truckStartPosition = default;
    [SerializeField] Vector3 m_truckLevelPosition = default;

    [SerializeField] ScrollingObject[] m_backgroundParallax = default;
    [SerializeField] ScrollingObject[] m_foregroundParallax = default;
    [SerializeField] ScrollingObject m_groundObject = default;
    [SerializeField] ScrollingObject m_bossObject = default;

    [SerializeField] HUD m_hud = default;
    public HUD GetHUD()
    {
        return m_hud;
    }
    [SerializeField] Transform m_HUDItemFlyDestination = default;
    public Vector3 GetFlyItemDestination()
    {
        if (m_HUDItemFlyDestination != null)
            return m_HUDItemFlyDestination.position;
        else
            return Vector3.zero;
    }
    
    [SerializeField] Pause m_pause = default;
    [SerializeField] Pause m_end = default;
    [SerializeField] Pause m_endMP = default;

    [SerializeField] Text m_countUpText = default;
    [SerializeField] Text m_opponentSearchText = default;

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
    int[] spawnedEasterEggs = default;

    // Score vars
    int m_coinsThisSession = default;
    int m_fireThisSession = default;

    //for tracking player 02's progress
    int m_oppCoinsThisSession = default;
    int m_oppFireThisSession = default;
    int m_oppEasterEggCollected = default;

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
        spawnedEasterEggs = new int[3];
        spawnedEasterEggs[0] = spawnedEasterEggs[1] = spawnedEasterEggs[2] = -1;

        //initializing opponent data
        m_oppCoinsThisSession = 0;
        m_oppFireThisSession = 0;
        m_oppEasterEggCollected = 0;

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

        //if it's multiplayer mode, disable pause button
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
            m_hud.EnablePause(false);
        //and if it is single player
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
        {
            //only enable pause button if ftue is not enabled
            if (GSTJ_Core.m_ShowFTUE == false)
                m_hud.EnablePause(true);
            else
                m_hud.EnablePause(false);
        }

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
                    //update level progress simulation for player 02
                    if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
                    {
                        //set the progress in level progress bar for player 02
                        m_hud.SetProgress(HUD.PlayerHUD.Player_02, normalizedTime);
                    }
                }

                if (m_elapsedLevelTime < (m_levelTime - 5.0f))
                {
                    UpdateFires();

                    if ((Time.time - m_lastCoinTime) >= m_coinSpawnTime)
                    {
                        m_lastCoinTime = Time.time;

                        // Spawn coin at (Screen end, -2.0f, 0.0f)
                        Vector3 coinPosition = new Vector3(30.0f, -2.0f, -1.5f);
                        //if it's hard mode then move coin clusters up or down based on chance
                        //0-40 coin cluster stays where it is
                        //41-70 coin cluster moves up
                        //71-100 coin cluster moves down
                        if (m_levelIndex > 1) //i.e. hard mode is selected
                        {
                            int temp = Random.Range(0, 100);
                            if (temp > 39 && temp < 70) coinPosition.y = 4.0f;
                            if (temp > 69 && temp < 100) coinPosition.y = -6.0f;
                        }

                        int coins = GSTJ_Core.LevelMeta.Levels[m_levelIndex].GetCoinClumpAmount();
                        for (int i = 0; i < coins; i++)
                        {
                            coinPosition.x += 1.5f;
                            PoolItem poolItem = m_poolManager.GetPoolItem(PoolType.Coin);
                            Coin coin = poolItem.gameObject.GetComponent<Coin>();
                            coin.Init(coinPosition, new Vector3(-coinPosition.x, coinPosition.y, coinPosition.z), new Coin.OnCollected(CollectCoin), m_poolManager);

                            m_coinPoolItems.Add(coin);
                            m_levelPlayerCharacter.AddCoins(coin);
                            //marking the first coin transform
                            if (isFirstCoinSpawned == false)
                            {
                                isFirstCoinSpawned = true;
                                firstCoin = coin.transform;
                                //only disable the colliders if FTUE is shown
                                if (GSTJ_Core.m_ShowFTUE == true)
                                    ToggleColliders(firstCoin, false);
                            }
                        }
                    }

                    if ((Time.time - m_lastEasterEggTime) >= m_easterEggSpawnTime)
                    {
                        m_lastEasterEggTime = Time.time;

                        // Spawn coin at (Screen end, -2.0f, 0.0f)
                        Vector3 eggPosition = new Vector3(30.0f, -2.0f, 0.0f);
                        PoolItem poolItem = m_poolManager.GetPoolItem(PoolType.EasterEgg);
                        EasterEgg easterEgg = poolItem.gameObject.GetComponent<EasterEgg>();
                        int eggID = -1;
                        bool isEggIDUnique = false;
                        while (isEggIDUnique == false)
                        {
                            //generate a random egg ID
                            eggID = Random.Range(0, easterEgg.GetMaxEasterEggs());
                            //check against all the spawned eggs
                            bool isEggIDFound = false;
                            for (int i = 0; i < spawnedEasterEggs.Length; i++)
                            {
                                //if the stored egg ID is valid, 
                                //it means that an egg has spawned earlier
                                if (spawnedEasterEggs[i] >= 0)
                                {
                                    //check if we have a unique ID
                                    if (eggID == spawnedEasterEggs[i])
                                    {
                                        isEggIDFound = true;
                                        break;
                                    }
                                }
                            }
                            //if the egg ID is not found in generated eggs
                            if (isEggIDFound == false)
                            {
                                isEggIDUnique = true;
                                //add this to the spawned egg IDs
                                for (int i = 0; i < spawnedEasterEggs.Length; i++)
                                {
                                    //look for the first negative value position
                                    if (spawnedEasterEggs[i] < 0)
                                    {
                                        //add the spawned egg ID
                                        spawnedEasterEggs[i] = eggID;
                                        break;
                                    }
                                }
                            }
                        }
                        easterEgg.Init(eggID, eggPosition, new Vector3(-eggPosition.x, eggPosition.y, eggPosition.z), new EasterEgg.OnCollected(CollectEasterEgg), m_poolManager);

                        m_easterEggItems.Add(easterEgg);
                        //toggle all its colliders on
                        ToggleColliders(easterEgg.transform, true);
                        m_levelPlayerCharacter.AddEasterEgg(easterEgg);
                        //marking the first Item transform
                        if (isFirstItemSpawned == false)
                        {
                            isFirstItemSpawned = true;
                            firstItem = easterEgg.transform;
                            //only disable the colliders if FTUE is shown
                            if (GSTJ_Core.m_ShowFTUE == true)
                                ToggleColliders(firstItem, false);
                        }
                    }
                }
                break;

            case LevelState.BossFires:
                //Boss
                if (m_bossFireTotalCount < GSTJ_Core.LevelMeta.Levels[m_levelIndex].BossFires)
                {
                    if (m_bossFireCurrentCount < 8)
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
                //check if it's multiplayer mode and enable "looking for..." text
                if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
                {
                    m_opponentSearchTextTransform.gameObject.SetActive(true);
                    StartCoroutine(DelayedCountUp());
                }
                if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
                {
                    m_countUpPreTextTransform.gameObject.SetActive(true);
                    m_countUpTransform.gameObject.SetActive(true);
                    StartCoroutine(DelayedCountUp());
                }
                m_hud.EnableHUD(HUD.PlayerHUD.Player_01, false);
                break;

            case LevelState.Starting:
                StopCoroutine(DelayedCountUp());
                m_fireTruck.GetComponent<Animation>().Play();
                m_truckDust.gameObject.SetActive(true);
                //check if we need to show the FTUE first - only for single player mode
                if (GSTJ_Core.m_ShowFTUE == true && GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single) 
                {
                    StartCoroutine(ShowFTUEPrompts());
                }
                m_hud.EnableHUD(HUD.PlayerHUD.Player_01, true);
                //enable player 2's hud if multi player mode is selected
                if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
                {
                    m_hud.EnableHUD(HUD.PlayerHUD.Player_02, true);
                }
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

            //in case of FTUE also, we need to pause the parallax and level movement
            case LevelState.FTUE:
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
                for (int i = 0; i < m_foregroundParallax.Length; i++)
                {
                    m_foregroundParallax[i].Play();
                }

                break;

            case LevelState.Boss:
                //stop the ftue coroutine if it's still running somehow
                StopCoroutine(ShowFTUEPrompts());
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
        //enable the bg panel
        m_bgPanelTransform.gameObject.SetActive(true);

        //if multiplayer mode in count up state
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi &&
            State == LevelState.Countup)
        {
            //wait for a random interval between 2-5s
            yield return new WaitForSeconds(Random.Range(2.0f, 5.0f));

            //now that the waiting has finished, declare opponent found
            m_opponentSearchText.text = "Opponent found!";
            //enable the count up etc. texts
            m_countUpPreTextTransform.gameObject.SetActive(true);
            m_countUpTransform.gameObject.SetActive(true);
        }

        //start the count down process
        while (State == LevelState.Countup)
        {
            countUpIndex--;
            if (countUpIndex > 0)
            {
                //this means we are counting down
                m_countUpText.text = countUpIndex.ToString();
                //play the audio
                m_countDownSound.Play();
            }
            
            if (countUpIndex == 0)
            {
                //this means we are set to go
                m_countUpText.text = "GO";
                //play the audio
                m_raceBeginSound.Play();
            }
            //scale up and wait for 1s
            m_countUpTransform.DOScale(1.0f, 0.5f).From(0.0f);
            yield return new WaitForSeconds(1f);
            
            if (countUpIndex <= 0)
            {
                //if multiplayer mode, disable the opponent search text
                if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
                {
                    m_opponentSearchTextTransform.gameObject.SetActive(false);
                }

                m_countUpPreTextTransform.gameObject.SetActive(false);
                m_countUpTransform.gameObject.SetActive(false);
                m_bgPanelTransform.gameObject.SetActive(false);
                OnLevelStateChange?.Invoke(LevelState.Starting);
            }
        }
        
        yield return null;
    }
    [System.Serializable]
    private struct FTUEDataStructure
    {
        public Transform ftueParent;
        public Transform focusCircle;
        public Transform screenBG;
        public Transform sprayWaterText;
        public Transform collectCoinText;
        public Transform collectItemsText;
        public Transform refillWaterText;
        public Transform waterFillButton;
        public Transform tapToContinueText;
    }
    private Transform firstFire;
    private Transform firstCoin;
    private Transform firstItem;
    private bool isFirstCoinSpawned = false;
    private bool isFirstFireSpawned = false;
    private bool isFirstItemSpawned = false;
    private bool isInputAllowedInFTUE = false;
    public bool IsInputAllowedInFTUE()
    {
        return isInputAllowedInFTUE;
    } 
    private bool hasPlayerTappedinFTUE = false;
    public void IndicatePlayerInput(bool hasTapped)
    {
        hasPlayerTappedinFTUE = hasTapped;
    }

    [SerializeField] FTUEDataStructure ftueData;
    //coroutine to show the FTUE if it is enabled in the 
    //main menu, it needs access to ftud data set in the inspector
    IEnumerator ShowFTUEPrompts()
    {
        //waiting till first fire, coin, or item has spawned
        //note that this check is not required once the game has begun
        //but we just want to be on the safe side of things
        yield return new WaitUntil (() => ((isFirstFireSpawned == true) || 
                                           (isFirstCoinSpawned == true) || 
                                           (isFirstItemSpawned == true)));
        //if the first fire has spawned
        if (isFirstFireSpawned == true)
        {
            //show the FTUE for fire
            yield return new WaitForSeconds(1.5f);
            //pause the game 
            State = LevelState.FTUE;
            OnPause();
            //set the focus circle position
            Vector3 newPos = m_fireTruck.transform.position;
            newPos.x += 20.0f;
            newPos.y += 5.0f;
            firstFire.position = newPos;
            ftueData.focusCircle.position = Camera.main.WorldToScreenPoint(newPos);
            //set the focus now
            ftueData.screenBG.SetParent(ftueData.focusCircle, worldPositionStays: true);
            ftueData.focusCircle.gameObject.SetActive(true);
            ftueData.screenBG.gameObject.SetActive(true);
            //set position and scale up the text
            newPos.y += 5.0f;
            ftueData.sprayWaterText.position = Camera.main.WorldToScreenPoint(newPos);
            ftueData.sprayWaterText.gameObject.SetActive(true);
            ftueData.sprayWaterText.DOScale(1.0f, 0.5f).From(0.0f);
            //wait for the player to read the message then ask them to tap
            isInputAllowedInFTUE = false;
            yield return new WaitForSeconds(1.5f);
            ftueData.tapToContinueText.gameObject.SetActive(true);
            ftueData.tapToContinueText.DOScale(1.0f, 0.5f).From(0.0f);
            //wait until the player has tapped on screen
            isInputAllowedInFTUE = true;
            //set the colliders as on in the game object
            ToggleColliders(firstFire, true);
            hasPlayerTappedinFTUE = false;
            yield return new WaitUntil(() => (hasPlayerTappedinFTUE == true));
            ftueData.sprayWaterText.DOScale(0.0f, 0.5f).From(1.0f);
            ftueData.screenBG.gameObject.SetActive(false);
            ftueData.focusCircle.gameObject.SetActive(false);
            ftueData.sprayWaterText.gameObject.SetActive(false);
            ftueData.tapToContinueText.gameObject.SetActive(false);
            ftueData.screenBG.SetParent(ftueData.ftueParent, worldPositionStays: true);
            //resume the game
            OnResume();
        }

        //wait for 1-2s before showing the next FTUE
        yield return new WaitForSeconds(Random.Range(2.3f, 3.0f));

        //if the first coin has spawned
        if (isFirstCoinSpawned == true)
        {
            //show the FTUE for coin
            //pause the game 
            State = LevelState.FTUE;
            OnPause();
            //set the focus circle position
            Vector3 newPos = m_fireTruck.transform.position;
            newPos.x += 20.0f;
            newPos.y += 5.0f;
            firstCoin.position = newPos;
            ftueData.focusCircle.position = Camera.main.WorldToScreenPoint(newPos);
            //set the focus now
            ftueData.screenBG.SetParent(ftueData.focusCircle, worldPositionStays: true);
            ftueData.focusCircle.gameObject.SetActive(true);
            ftueData.screenBG.gameObject.SetActive(true);
            //set position and scale up the text
            newPos.y += 5.0f;
            ftueData.collectCoinText.position = Camera.main.WorldToScreenPoint(newPos);
            ftueData.collectCoinText.gameObject.SetActive(true);
            ftueData.collectCoinText.DOScale(1.0f, 0.5f).From(0.0f);
            //wait for the player to read the message then ask them to tap
            isInputAllowedInFTUE = false;
            yield return new WaitForSeconds(1.5f);
            ftueData.tapToContinueText.gameObject.SetActive(true);
            ftueData.tapToContinueText.DOScale(1.0f, 0.5f).From(0.0f);
            //wait until the player has tapped on screen
            isInputAllowedInFTUE = true;
            //set the colliders as on in the game object
            ToggleColliders(firstCoin, true);
            hasPlayerTappedinFTUE = false;
            yield return new WaitUntil(() => (hasPlayerTappedinFTUE == true));
            ftueData.collectCoinText.DOScale(0.0f, 0.5f).From(1.0f);
            ftueData.screenBG.gameObject.SetActive(false);
            ftueData.focusCircle.gameObject.SetActive(false);
            ftueData.collectCoinText.gameObject.SetActive(false);
            ftueData.tapToContinueText.gameObject.SetActive(false);
            ftueData.screenBG.SetParent(ftueData.ftueParent, worldPositionStays: true);
            //resume the game
            OnResume();
        }

        //wait for 1-2s before showing the next FTUE
        yield return new WaitForSeconds(Random.Range(2.3f, 3.0f));

        //if the first item has spawned
        if (isFirstItemSpawned == true)
        {
            //show the FTUE for item/easter egg
            //pause the game 
            State = LevelState.FTUE;
            OnPause();
            //set the focus circle position
            Vector3 newPos = m_fireTruck.transform.position;
            newPos.x += 20.0f;
            newPos.y += 5.0f;
            firstItem.position = newPos;
            ftueData.focusCircle.position = Camera.main.WorldToScreenPoint(newPos);
            //set the focus now
            ftueData.screenBG.SetParent(ftueData.focusCircle, worldPositionStays: true);
            ftueData.focusCircle.gameObject.SetActive(true);
            ftueData.screenBG.gameObject.SetActive(true);
            //set position and scale up the text
            newPos.y += 5.0f;
            ftueData.collectItemsText.position = Camera.main.WorldToScreenPoint(newPos);
            ftueData.collectItemsText.gameObject.SetActive(true);
            ftueData.collectItemsText.DOScale(1.0f, 0.5f).From(0.0f);
            //wait for the player to read the message then ask them to tap
            isInputAllowedInFTUE = false;
            yield return new WaitForSeconds(1.5f);
            ftueData.tapToContinueText.gameObject.SetActive(true);
            ftueData.tapToContinueText.DOScale(1.0f, 0.5f).From(0.0f);
            //wait until the player has tapped on screen
            isInputAllowedInFTUE = true;
            //set the colliders as on in the game object
            ToggleColliders(firstItem, true);
            hasPlayerTappedinFTUE = false;
            yield return new WaitUntil(() => (hasPlayerTappedinFTUE == true));
            ftueData.collectItemsText.DOScale(0.0f, 0.5f).From(1.0f);
            ftueData.screenBG.gameObject.SetActive(false);
            ftueData.focusCircle.gameObject.SetActive(false);
            ftueData.collectItemsText.gameObject.SetActive(false);
            ftueData.tapToContinueText.gameObject.SetActive(false);
            ftueData.screenBG.SetParent(ftueData.ftueParent, worldPositionStays: true);
            //resume the game
            OnResume();
        }
        
        //wait for some time before showing refill ftue
        yield return new WaitForSeconds(Random.Range(8.0f, 13.0f));
        
        //show the FTUE for item/easter egg
        //pause the game 
        State = LevelState.FTUE;
        OnPause();
        ftueData.focusCircle.position = ftueData.waterFillButton.position;
        //set the focus now
        ftueData.screenBG.SetParent(ftueData.focusCircle, worldPositionStays: true);
        ftueData.focusCircle.gameObject.SetActive(true);
        ftueData.screenBG.gameObject.SetActive(true);
        ftueData.refillWaterText.gameObject.SetActive(true);
        ftueData.refillWaterText.DOScale(1.0f, 0.5f).From(0.0f);
        //wait for the player to read the message then ask them to tap
        isInputAllowedInFTUE = false;
        yield return new WaitForSeconds(1.5f);
        ftueData.tapToContinueText.gameObject.SetActive(true);
        ftueData.tapToContinueText.DOScale(1.0f, 0.5f).From(0.0f);
        //wait until the player has tapped on screen
        isInputAllowedInFTUE = true;
        hasPlayerTappedinFTUE = false;
        yield return new WaitUntil(() => (hasPlayerTappedinFTUE == true));
        ftueData.refillWaterText.DOScale(0.0f, 0.5f).From(1.0f);
        ftueData.screenBG.gameObject.SetActive(false);
        ftueData.focusCircle.gameObject.SetActive(false);
        ftueData.refillWaterText.gameObject.SetActive(false);
        ftueData.tapToContinueText.gameObject.SetActive(false);
        ftueData.screenBG.SetParent(ftueData.ftueParent, worldPositionStays: true);
        //resume the game
        OnResume();

        //indicate that the player has seen ftue
        GSTJ_Core.hasPlayerSeenFTUE = true;
        //enable the pause button if it's single player mode
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
            m_hud.EnablePause(true);
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

    //this function will also make sure that player 02's progress is added
    public void AddScoreForOpponent(ScoreType scoreType, int value = 1)
    {
        //check difficulty level selected
        int difficultyID = PlayerPrefs.GetInt("LEVEL", -1);
        float chance = Random.Range(0.0f, 1.0f);
        switch (scoreType)
        {
            case ScoreType.Coin:
                //player 02 will collect this coin based on difficulty mode
                switch (difficultyID)
                {
                    case 0: //easy
                    //there's 66.66% chance player 2 will collect this coin
                    if (chance <= (2.0f/3.0f)) //collect the coin
                        m_oppCoinsThisSession += value;
                    //and 10% chance to collect an additional coin
                    chance = Random.Range(0.0f, 1.0f);
                    if (chance <= 0.1f) m_oppCoinsThisSession += value;
                    break;

                    case 1: //medium
                    //there's 90% chance player 2 will collect this coin
                    if (chance <= 0.9f) //collect the coin
                        m_oppCoinsThisSession += value;
                    //and 20% chance to collect an additional coin
                    chance = Random.Range(0.0f, 1.0f);
                    if (chance <= 0.2f) m_oppCoinsThisSession += value;
                    break;

                    case 2: //hard
                    //there's 100% chance player 2 will collect this coin
                    //since it's hard mode, the player and their opponent will collect 2x
                    m_oppCoinsThisSession += 2 * value;
                    //and 30% chance to collect an additional coin
                    chance = Random.Range(0.0f, 1.0f);
                    if (chance <= 0.3f) m_oppCoinsThisSession += value;
                    break;

                    default:
                    Debug.LogWarning("Difficulty ID was not found while checking coins");
                    break;
                }
                //set the coins for player 02
                m_hud.SetHUDCount(HUD.PlayerHUD.Player_02, scoreType, m_oppCoinsThisSession);
                break;

            case ScoreType.Fire:
                switch (difficultyID)
                {
                    case 0: //easy
                    //there's 66.66% chance player 2 will collect this fire
                    if (chance <= (2.0f/3.0f)) //collect the fire
                        m_oppFireThisSession += value;
                    //and 10% chance to collect an additional fire
                    chance = Random.Range(0.0f, 1.0f);
                    if (chance <= 0.1f) m_oppFireThisSession += value;
                    break;

                    case 1: //medium
                    //there's 90% chance player 2 will collect this fire
                    if (chance <= 0.9f) //collect the fire
                        m_oppFireThisSession += value;
                    //and 20% chance to collect an additional fire
                    chance = Random.Range(0.0f, 1.0f);
                    if (chance <= 0.2f) m_oppFireThisSession += value;
                    break;

                    case 2: //hard
                    //there's 100% chance player 2 will collect this fire
                    m_oppFireThisSession += value;
                    //and 30% chance to collect an additional fire
                    chance = Random.Range(0.0f, 1.0f);
                    if (chance <= 0.3f) m_oppFireThisSession += value;
                    break;

                    default:
                    Debug.LogWarning("Difficulty ID was not found while checking fire");
                    break;
                }
                //set the fire for player 02
                m_hud.SetHUDCount(HUD.PlayerHUD.Player_02, scoreType, m_oppFireThisSession);
                break;
        }

        //update player 02's score as well
        m_hud.SetHUDCount(HUD.PlayerHUD.Player_02, ScoreType.Score, 
                              m_oppCoinsThisSession +  m_oppFireThisSession);
    }

    //this function takes care of easter egg collection for player 02
    public void CheckEasterEggForOpponent(Sprite easterEgg, int value)
    {
        //easter egg collection % is based on difficulty chosen, so again
        int difficultyID = PlayerPrefs.GetInt("LEVEL", -1);
        float chance = Random.Range(0.0f, 1.0f);
        bool isEasterEggCollected = false;
        switch (difficultyID)
        {
            case 0: //easy
            //there's 50% chance player 2 will collect this easter egg
            if (chance <= 0.5f) isEasterEggCollected = true;
            break;

            case 1: //medium
            //there's 60% chance player 2 will collect this easter egg
            if (chance <= 0.6f) isEasterEggCollected = true;
            break;

            case 2: //hard
            //there's 75% chance player 2 will collect this easter egg
            if (chance <= 0.75f) isEasterEggCollected = true;
            break;

            default:
            Debug.LogWarning("Difficulty ID was not found while checking easter egg");
            break;
        }

        //if easter egg is collected, add it to the opponent hud and update their score
        if (isEasterEggCollected == true)
        {
            //update coin hud score for player 02
            m_oppCoinsThisSession += value;
            m_hud.SetHUDCount(HUD.PlayerHUD.Player_02, ScoreType.Coin, m_oppCoinsThisSession);
            //update overall score as well
            m_hud.SetHUDCount(HUD.PlayerHUD.Player_02, ScoreType.Score, 
                              m_oppCoinsThisSession +  m_oppFireThisSession);

            //check if easter egg image is already added to hud
            bool isAddedToHUD = IsEasterEggCollected(HUD.PlayerHUD.Player_02, easterEgg, 
                                                     m_oppEasterEggCollected);
            if (isAddedToHUD == true)
            {
                //only add score for this easter egg
                AddCollectibleScore(HUD.PlayerHUD.Player_02, m_oppEasterEggCollected);
            }
            else
            {
                m_oppEasterEggCollected += 1;
                //add image and score both
                AddCollectibletoUI(HUD.PlayerHUD.Player_02, easterEgg, m_oppEasterEggCollected);
            }
        }
    }
    public void AddScore(ScoreType scoreType, int value = 1)
    {
        //check difficulty level selected
        int difficultyID = PlayerPrefs.GetInt("LEVEL", -1);
        //for hard mode, the player will get 2x coins
        if (scoreType == ScoreType.Coin && difficultyID > 1)
            value = 2 * value;
        
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

    //for checking if an easter egg is already collected
    public bool IsEasterEggCollected(HUD.PlayerHUD playerHUD, Sprite sp, int ID)
    {
        return m_hud.IsEasterEggCollected(playerHUD, sp, ID);
    }

    //for adding the easter egg to hud/end-game ui
    public void AddCollectibletoUI(HUD.PlayerHUD playerHUD, Sprite sp, int ID)
    {
        m_hud.SetHUDCollectible(playerHUD, sp, ID);
        
        //note that collectible score is based on difficulty mode
        int difficultyID = PlayerPrefs.GetInt("LEVEL", 0);
        int colScore = 50 + 50 * (difficultyID/2);
        
        //also add collectible and score to pause screen and end-game UI
        //set appropriate data for single/multi player mode
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
        {
            //note that collectible score is based on difficulty mode
            m_pause.SetCollectibleDataForPlayer(playerHUD, sp, colScore, ID);
            m_end.SetCollectibleDataForPlayer(playerHUD, sp, colScore, ID);
        }
        
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
            m_endMP.SetCollectibleDataForPlayer(playerHUD, sp, colScore, ID);

    }

    //updating score for the easter egg in hud/end-game ui
    public void AddCollectibleScore(HUD.PlayerHUD playerHUD, int ID)
    {
        //note that collectible score is based on difficulty mode
        int difficultyID = PlayerPrefs.GetInt("LEVEL", 0);
        int colScore = 50 + 50 * (difficultyID/2);

        //set appropriate data for single/multi player mode
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
        {
            m_pause.AddCollectibleScoreForPlayer(playerHUD, colScore, ID);
            m_end.AddCollectibleScoreForPlayer(playerHUD, colScore, ID);
        }
        
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
            m_endMP.AddCollectibleScoreForPlayer(playerHUD, colScore, ID);

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

                PoolItem poolItem;
                //42% chances of spawning long flame
                if (Random.Range(0.0f, 1.0f) > 0.58f)
                {
                    poolItem = m_poolManager.GetPoolItem(PoolType.LongFlame);
                    //modify the position so it spawns on the ground only
                    firePosition.y = Random.Range(-6.0f, -2.5f);
                }
                else
                    poolItem = m_poolManager.GetPoolItem(PoolType.Flame);
                //initialize the fire
                Fire fire = poolItem.gameObject.GetComponent<Fire>();
                fire.Init(firePosition, new Vector3(-firePosition.x, firePosition.y, firePosition.z), true, new Fire.OnExtinguished(ExtinguishFire), m_poolManager);
                fire.SetSpeed(m_currentLevelSpeed);

                m_firePoolItems.Add(fire);
                m_levelPlayerCharacter.AddFire(fire);
                //marking the first fire transform
                if (isFirstFireSpawned == false)
                {
                    isFirstFireSpawned = true;
                    firstFire = fire.transform;
                    //only disable the collider if we are showing the FTUE
                    if (GSTJ_Core.m_ShowFTUE == true)
                        ToggleColliders(firstFire, false);
                }
            }
        }
    }

    //for enabling or disabling all colliders on an object and its children
    private void ToggleColliders(Transform obj, bool state)
    {
        Collider2D[] allColliders = obj.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < allColliders.Length; i++)
        {
            allColliders[i].enabled = state;
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

        m_bossFireCurrentCount--;
        m_bossFireCurrentCount = Mathf.Clamp(m_bossFireCurrentCount, 0, 100);
    }

    void StartBossMovie(ScrollingObject scrollingObject)
    {
        //pause only needs to be disabled in single player
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
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
        //we only enable the pause again, if it is single player
        if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
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
        //if it's just ftue, then 
        if (State == LevelState.FTUE)
        {
            m_prePauseLevelState = LevelState.Progress;
            OnLevelStateChange(LevelState.FTUE);
            m_fireTruck.GetComponent<Animation>().Stop();
            m_truckDust.gameObject.SetActive(false);
        }
        else if (State == LevelState.End)
        {
            //disable the in-game hud
            m_hud.gameObject.SetActive(false);
            //enable appropriate result screen
            if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Single)
            {
                m_end.gameObject.SetActive(true);
                m_end.SetData(m_fireThisSession, m_fireThisSession + m_coinsThisSession, 
                              HUD.PlayerHUD.Player_01);
            }
            //for multiplayer mode set data for both player 01 and player 02
            if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
            {
                m_endMP.gameObject.SetActive(true);
                m_endMP.SetData(m_fireThisSession, m_fireThisSession + m_coinsThisSession, 
                                HUD.PlayerHUD.Player_01);
                m_endMP.SetData(m_oppFireThisSession, m_oppFireThisSession + m_oppCoinsThisSession, 
                                HUD.PlayerHUD.Player_02);
            }
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

        //turning off fires and coins and easter eggs
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
        //stop coroutine for ftue
        StopCoroutine(ShowFTUEPrompts());

        m_mainMusic.Stop();

        m_inputManager.OnMouseUp -= OnMouseUp;

        SceneManager.LoadScene(SceneConstants.Home);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneConstants.Game);
    }
    #endregion
}