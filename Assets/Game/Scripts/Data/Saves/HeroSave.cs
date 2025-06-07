using System;
using System.Collections.Generic;
using System.Linq;
using Game.Scripts.Data.Enums;

namespace Game.Scripts.Data.Saves
{
  [Serializable]
  public class HeroSave
  {
    public HeroId Id;
    public int Level;
    public HeroMagicId CurrentMagicId;
    public int Experience;
    public List<HeroMagicSave> Magic = new List<HeroMagicSave>();
    
    public HeroMagicSave GetMagicSave(HeroMagicId magicId) => 
      Magic.FirstOrDefault(magic => magic.Id == magicId);

    public HeroMagicSave GetMagicSave() => 
      GetMagicSave(CurrentMagicId);
  }
}