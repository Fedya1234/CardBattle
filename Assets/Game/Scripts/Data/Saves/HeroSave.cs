using System;
using System.Collections.Generic;
using Game.Scripts.Data.Enums;
using UnityEngine.Serialization;

namespace Game.Scripts.Data.Saves
{
  [Serializable]
  public class HeroSave
  {
    public int Level;
    public int CurrentMagicIndex;
    public int Experience;
    public List<HeroMagicSave> Magic = new List<HeroMagicSave>();
  }
}