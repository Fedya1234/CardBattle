using System;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Saves
{
  [Serializable]
  public class HeroMagicSave
  {
    public int Level;
    public HeroMagicId Id;
  }
}