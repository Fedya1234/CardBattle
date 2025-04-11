namespace Game.Scripts.Data.Core
{
    public class HeroState
    {
        public event System.Action<HeroState> EventChanged;
        public int Mana { get; private set; }
        public int Health { get; private set; }
        
        
        public void ApplyChanges(HeroState changes)
        {
            Mana += changes.Mana;
            Health += changes.Health;
            EventChanged?.Invoke(this);
        }
    }
}