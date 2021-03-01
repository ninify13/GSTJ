using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Core.InputManager;
using Game.Collectible;
using Game.Water;
using DG.Tweening;

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
    int m_easterEggCollectedCount = 0;

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
        //set collectible count back to 0
        m_easterEggCollectedCount = 0;

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
        //if it's the FTUE state, send out a signal that player has tapped on screen
        if (m_levelManager.State == LevelManager.LevelState.FTUE)
        {
            //return if the input is not allowed
            if (m_levelManager.IsInputAllowedInFTUE() == false)
                return;
            m_levelManager.IndicatePlayerInput(true);
        }

        if (m_levelManager.State == LevelManager.LevelState.End || m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        if (mousePostiion.x < (Screen.width / 10.0f))
            return;

        if (m_levelManager.AvailableWater <= 0.0f  || m_levelManager.WaterFilling)
        {
            //play the audio sfx for water empty
            m_levelManager.PlayWaterEmptyAudio();
            //pop the bucket
            Transform btn = m_levelManager.GetHUD().GetWaterButton();
            btn.DOPunchScale(Vector3.one * 0.15f, 0.25f, 1);
            return;
        }
    }
    void OnWaterSpray(Vector3 mousePostiion)
    {
        if (m_levelManager.State == LevelManager.LevelState.End || 
            m_levelManager.State == LevelManager.LevelState.Paused ||
            m_levelManager.State == LevelManager.LevelState.FTUE)
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

        for (int i = (m_fires.Count - 1); i >= 0; i--)
        {
            //to determine if the fire is dispelled, we check if it collided with tip
            dispel = m_waterPlayer.CheckCollisionWith(m_fires[i].transform);
            //let's dispel this fire!
            if (dispel == true)
            {
                // Water tip is mostly on fire
                m_levelManager.AddScore(LevelManager.ScoreType.Fire);
                //check if player 2 also collected something
                //note that this is simulated play for player 2
                if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
                    m_levelManager.AddScoreForOpponent(LevelManager.ScoreType.Fire);
                m_fires[i].Extinguish();
                //remove it from the water player collision list
                m_waterPlayer.RemoveFromList(m_fires[i].transform);
                m_fires.Remove(m_fires[i]);
            }
        }

        for (int i = (m_coins.Count - 1); i >= 0; i--)
        {
            //to determine if the coin can be collected, we check if it collided with tip
            dispel = m_waterPlayer.CheckCollisionWith(m_coins[i].transform);
            //let's collect this coin!
            if (dispel == true)
            {
                //start a co-routine to take care of flying coin image to the counter
                FlyItemToCounter(m_coins[i].transform, dur: 1.0f, scaleDown: false);
                // Water tip is mostly on coin
                m_levelManager.AddScore(LevelManager.ScoreType.Coin);
                
                //check if player 2 also collected something
                if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
                    m_levelManager.AddScoreForOpponent(LevelManager.ScoreType.Coin);
                
                m_coins[i].Collect();
                //first remove it from the water player collision list
                m_waterPlayer.RemoveFromList(m_coins[i].transform);
                m_coins.Remove(m_coins[i]);
            }
        }

        for (int i = (m_easterEggs.Count - 1); i >= 0; i--)
        {
            //to determine if the easter egg can be collected, we check if it collided with tip
            dispel = m_waterPlayer.CheckCollisionWith(m_easterEggs[i].transform);
            //let's collect this easter egg!
            if (dispel == true)
            {
                //check if easter egg is already collected
                bool isEasEggCol = m_levelManager.IsEasterEggCollected(HUD.PlayerHUD.Player_01, 
                                                        m_easterEggs[i].GetSpriteResource(), 
                                                        m_easterEggCollectedCount);
                if (isEasEggCol == true)
                {
                    //only add score for this collectible
                    m_levelManager.AddCollectibleScore(HUD.PlayerHUD.Player_01, m_easterEggCollectedCount);
                }
                else
                {
                    //add the easter egg image to the in-game HUD and end-game UI
                    m_easterEggCollectedCount += 1;
                    m_levelManager.AddCollectibletoUI(HUD.PlayerHUD.Player_01, 
                                                    m_easterEggs[i].GetSpriteResource(), 
                                                    m_easterEggCollectedCount);
                }

                // Water helper is looking mostly towards easter egg
                m_levelManager.AddScore(LevelManager.ScoreType.Coin, 50);
                //play the easter egg (item) collection sound
                m_levelManager.PlayItemColAudio();
                //check if easter egg is also collected by the opponent
                if (GSTJ_Core.SelectedMode == GSTJ_Core.GameMode.Multi)
                    m_levelManager.CheckEasterEggForOpponent(m_easterEggs[i].GetSpriteResource(), 50);

                m_easterEggs[i].Collect();
                //first remove it from the water player collision list
                m_waterPlayer.RemoveFromList(m_easterEggs[i].transform);
                m_easterEggs.Remove(m_easterEggs[i]);
            }
        }
    }

    //for flying and collecting coins/collectibles etc. in counter
    private void FlyItemToCounter(Transform item, float dur = 1.0f, bool scaleDown = true)
    {
        //create a copy of this item
        GameObject go = Instantiate(item.gameObject, item.position, item.rotation);
        //strip components from this 
        Destroy(go.GetComponent<Coin>());
        Destroy(go.GetComponent<ScrollingObject>());
        Destroy(go.GetComponent<CircleCollider2D>());
        go.transform.position = item.position;
        //start a new routine for flying this item
        IEnumerator flyItem = FlyingItemToCounter(go.transform, dur, scaleDown);
        StartCoroutine(flyItem);
    }
    private IEnumerator FlyingItemToCounter(Transform item, float dur, bool scaleDown)
    {
        Vector3 startPos = item.position;
        Vector3 finalPos = m_levelManager.GetFlyItemDestination();
        Vector3 startScale = item.localScale;
        float timeLapsed = 0.0f;
        float percent = 0.0f;
        //let's begin moving the object
        while (percent <=1.0f)
        {
            //if for whatever reason the game ends, end this loop immediately
            if (m_levelManager.State == LevelManager.LevelState.End)
                break;
            
            //only fly an item if the game is running (and not paused, etc.)
            if (m_levelManager.State != LevelManager.LevelState.Paused)
            {
                //update the position and scale (if specified)
                item.position = Vector3.Lerp(startPos, finalPos, percent);
                Vector3 newScale = Vector3.zero;
                if (scaleDown == true)
                {
                    newScale = Vector3.Lerp(startScale, Vector3.zero, percent);
                    item.localScale = newScale;
                }
                //update counters and wait
                timeLapsed += Time.deltaTime;
                percent = (timeLapsed)/dur;
                yield return new WaitForEndOfFrame();
            }
            else //skip this frame and do nothing
                yield return new WaitForEndOfFrame();
        }

        //item has reached destination, destroy it
        Destroy(item.gameObject);
        yield return null;
    }

    void OnWaterRelease(Vector3 mousePosition)
    {
        if (m_levelManager.State == LevelManager.LevelState.Paused)
            return;

        //if (m_levelManager.State == LevelManager.LevelState.End)
          //  return;

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
