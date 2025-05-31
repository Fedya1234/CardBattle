using System;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Saves;
using Game.Scripts.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Scripts.Data.Core.State
{
    [Serializable]
    public class HeroState
    {
        [SerializeField][ReadOnly] private int _mana;
        [SerializeField][ReadOnly] private int _health;
        public event System.Action<HeroState> EventChanged;

        public int Mana
        {
            get => _mana;
            private set => _mana = value;
        }

        public int Health
        {
            get => _health;
            private set => _health = value;
        }

        public int HeroMagicUseCount { get; set; }
        public HeroMagicId HeroMagicId { get; set; }
        public int HeroMagicLevel { get; set; }
        public HeroId HeroId { get; set; }
        
        /// <summary>
        /// Constructor for initializing from player save data
        /// </summary>
        public HeroState(PlayerSave playerSave)
        {
            if (playerSave != null)
            {
                // Get hero data based on playerSave.HeroId
                var heroData = StaticDataService.GetHeroData(playerSave.HeroId);
                
                // Set initial values from hero data
                Health = heroData.Health;
                Mana = 0; // Start with 0 mana
            }
            else
            {
                // Fallback values if data couldn't be loaded
                Health = 20;
                Mana = 0;
                
                Debug.LogError("Player save is null in HeroState constructor");
            }
        }

        /// <summary>
        /// Constructor for creating state changes
        /// </summary>
        public HeroState(int mana = 0, int health = 0)
        {
            Mana = mana;
            Health = health;
        }
        
        /// <summary>
        /// Apply changes from another HeroState
        /// </summary>
        public void ApplyChanges(HeroState changes)
        {
            Mana += changes.Mana;
            Health += changes.Health;
            EventChanged?.Invoke(this);
        }
        
        /// <summary>
        /// Apply changes directly with mana and health values
        /// </summary>
        public void ApplyChanges(int manaChange = 0, int healthChange = 0)
        {
            Mana += manaChange;
            Health += healthChange;
            EventChanged?.Invoke(this);
        }
        
        /// <summary>
        /// Change mana value
        /// </summary>
        public void ChangeMana(int manaChange)
        {
            Mana += manaChange;
            EventChanged?.Invoke(this);
        }
        
        /// <summary>
        /// Change health value
        /// </summary>
        public void ChangeHealth(int healthChange)
        {
            Health += healthChange;
            EventChanged?.Invoke(this);
        }
    }
}