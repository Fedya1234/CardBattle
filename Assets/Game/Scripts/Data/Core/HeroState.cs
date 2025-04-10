namespace Game.Scripts.Data.Core
{
    public class HeroState
    {
        public event System.Action<HeroState> EventChanged;
        public int Mana { get; private set; }
        public int Health { get; private set; }
        
        public void SetMana(int mana)
        {
            Mana = mana;
            EventChanged?.Invoke(this);
        }
        
        public void SetHealth(int health)
        {
            Health = health;
            EventChanged?.Invoke(this);
        }
    }
}