using Game.Scripts.Data.Saves;

namespace Game.Scripts.Data.Core.State
{
    public class HeroState
    {
        public event System.Action<HeroState> EventChanged;
        public int Mana { get; private set; }
        public int MaxMana { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        
        public HeroState(PlayerSave playerSave)
        {
            Mana = playerSave.Mana;
            Health = playerSave.Health;
            MaxMana = playerSave.Mana;
            MaxHealth = playerSave.Health;
        }
        
        public void ApplyChanges(HeroState changes)
        {
            Mana += changes.Mana;
            Health += changes.Health;
            EventChanged?.Invoke(this);
        }
    }
}