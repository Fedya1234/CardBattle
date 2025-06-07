using System;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Core.State
{
  [Serializable]
  public class HeroMagicState
  {
    public int UsesRemaining;
    public HeroMagicId Id;
    public int Level;
    
    public HeroMagicState(HeroMagicId id, int level, int usesRemaining)
    {
      Id = id;
      Level = level;
      UsesRemaining = usesRemaining;
    }
  }
}