using System;
using System.Collections.Generic;
using Game.Scripts.Data.Core.Move;
using Game.Scripts.Data.Core.State;
using Game.Scripts.Data.Core.Units;
using Game.Scripts.Data.Enums;
using Game.Scripts.Data.Static;
using Game.Scripts.Data.Visual;
using Game.Scripts.Data.Saves;
using Game.Scripts.Helpers;
using UnityEngine;

namespace Game.Scripts.Data.Core.State
{
    [Serializable]
    public class PlayerState
    {
        public BoardState Board;
        public PlayerCardsState Cards;
        public HeroState Hero;

        public PlayerState(PlayerSave playerSave)
        {
            Cards = new PlayerCardsState(playerSave);
            Board = new BoardState();
            Hero = new HeroState(playerSave);
        }

        public void ApplyChanges(PlayerMove changes)
        {
            // Обрабатываем карты (удаляем из руки, добавляем в сброс)
            Cards.ApplyChanges(changes);
            
            // Создаем юнитов на доске из сыгранных карт
            foreach (var cardMove in changes.Cards)
            {
                // Создаем юнита из карты
                var unitData = CreateUnitFromCard(cardMove.Card);
                
                // Размещаем юнита на доске
                Board.GetPlace(cardMove.Line, cardMove.Row).PlaceUnit(unitData);
                
                // Вычитаем стоимость карты из маны
                Hero.ChangeMana(-cardMove.Card.GetManaCost());
                
                Debug.Log($"Player {cardMove.FieldIndex} placed unit {cardMove.Card.Id} at ({cardMove.Line}, {cardMove.Row})");
            }
            
            // Обрабатываем сожженную карту для маны
            if (changes.BurnedForManaCard != null)
            {
                Hero.ChangeMana(1); // Получаем 1 ману за сожженную карту
                Debug.Log($"Player burned card {changes.BurnedForManaCard.Id} for mana");
            }
        }
        
        /// <summary>
        /// Создает данные юнита из карты
        /// </summary>
        private GameUnitData CreateUnitFromCard(CardLevel card)
        {
            // Получаем статические данные карты
            
            var unitCardVisual = VisualService.GetUnitCardVisual(card.Id);
            var unitStaticData = StaticDataService.GetUnitData(unitCardVisual.UnitId, card.Level);
            var unitData = new UnitData(){Id = unitCardVisual.UnitId, Level = card.Level};
            var unitVisual = VisualService.GetUnitVisual(unitCardVisual.UnitId);
            
            return new GameUnitData(unitStaticData, unitVisual, unitData);
        }
        
        /// <summary>
        /// Возвращает базовое здоровье для карты
        /// </summary>
        private int GetCardBaseHealth(CardId cardId)
        {
            // Простая логика для определения здоровья на основе ID карты
            return cardId switch
            {
                CardId.Card_0 => 2, // Базовый юнит
                CardId.Card_1 => 1, // Слабый юнит
                CardId.Card_2 => 3, // Сильный юнит
                _ => 2 // По умолчанию
            };
        }
        
        /// <summary>
        /// Возвращает базовый урон для карты
        /// </summary>
        private int GetCardBaseDamage(CardId cardId)
        {
            // Простая логика для определения урона на основе ID карты
            return cardId switch
            {
                CardId.Card_0 => 1, // Базовый юнит
                CardId.Card_1 => 2, // Атакующий юнит
                CardId.Card_2 => 1, // Защитный юнит
                _ => 1 // По умолчанию
            };
        }
    }
}
