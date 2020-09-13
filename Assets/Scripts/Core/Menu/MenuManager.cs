using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Menu
{
    [Serializable]
    public class MenuItem
    {
        public enum Menus
        {
            Home,
            Characters,
            Scores,
            Game,
            Difficulty,
        }

        [SerializeField] Menus m_key = default;
        [SerializeField] MenuLayout m_layout = default;

        public MenuLayout Layout { get { return m_layout; } }
        public Menus Key { get { return m_key; } }
    }

    public class MenuManager : MonoBehaviour
    {
        [SerializeField] List<MenuItem> m_menuItems = new List<MenuItem>();

        MenuItem m_currentMenu = default;

        Stack<MenuItem> m_menuStack = new Stack<MenuItem>();


        void Awake()
        {
            for (int i = 0; i < m_menuItems.Count; i++)
            {
                m_menuItems[i].Layout.Init(this);
                m_menuItems[i].Layout.gameObject.SetActive(false);
            }

            SwitchToScreen(0);
        }

        public void SwitchToScreen(MenuItem.Menus screen, bool addToStack = true)
        {
            DisableCurrentScreen(addToStack);

            m_currentMenu = m_menuItems.Find(m => m.Key == screen);
            m_currentMenu.Layout.gameObject.SetActive(true);

            CheckMenuStack();
        }

        public void SwitchToScreen(int index)
        {
            DisableCurrentScreen();

            m_currentMenu = m_menuItems[index];
            m_currentMenu.Layout.gameObject.SetActive(true);

            CheckMenuStack();
        }

        public void BackButton()
        {
            if (m_menuStack.Count > 0)
            {
                Debug.Log(m_menuStack.Count);
                MenuItem item = m_menuStack.Peek();
                m_menuStack.Pop();
                SwitchToScreen(item.Key, false);
            }
        }

        void DisableCurrentScreen(bool addToStack = true)
        {
            if (m_currentMenu != null)
            {
                m_currentMenu.Layout.gameObject.SetActive(false);

                if (addToStack)
                    m_menuStack.Push(m_currentMenu);
            }
        }

        void CheckMenuStack()
        {
            if (m_currentMenu.Key == MenuItem.Menus.Home || m_currentMenu.Key == MenuItem.Menus.Game)
            {
                m_menuStack.Clear();
            }
        }

        void OnDestroy()
        {
            m_menuStack.Clear();
            m_menuStack = null;
        }
    }
}