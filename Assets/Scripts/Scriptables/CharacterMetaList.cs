using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Menu;

[CreateAssetMenu(fileName = "CharacterMeta", menuName = "Meta/Characters", order = 0)]
public class CharacterMetaList : ScriptableObject
{
    public string Path;
    public string UnlockTexts;
    
    public List<CharacterMeta> Characters;  
}

[Serializable]
public class CharacterMeta
{
    public Characters.Character CharacterName;

    public int CoinRequirement;
}
