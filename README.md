# CardBattle

> **Note**: This project was entirely written by an AI assistant (Claude) under my minimal supervision. This project is
> my experiment and first successful example of using "vibe coding" - development through natural dialogue with AI.

## 🎮 Description

CardBattle is a turn-based card strategy game created in Unity, inspired by Hearthstone. Players place unit cards on a
3x3 battlefield, manage resources, and engage in tactical battles with various heroes and magical abilities.

### ✨ Key Features

- **Tactical 3x3 Battlefield** - strategic unit placement
- **Hero System** - 6 different hero classes (Warrior, Mage, Rogue, Paladin, Hunter, Druid)
- **Advanced Combat System** - turn-based battles with animations and effects
- **Card System** - units and spells with various abilities
- **AI Opponent** - smart bot with game state analysis
- **Save System** - player progress and card collection
- **Modular Architecture** - event-driven system

## 🎯 Game Mechanics

- **Turn-based Gameplay**: Players move simultaneously, then resolution phase occurs
- **Resource Management**: Mana system and ability to burn cards for additional mana
- **Strategic Positioning**: Placement on 3x3 grid affects combat interactions
- **Card Variety**: Units with unique abilities and spells
- **Effect System**: Armor, vampirism, first strike, anti-magic, and more

## 🛠 Technologies

- **Unity 2022.3+** - Game engine
- **C#** - Primary programming language
- **UniTask** - Asynchronous programming
- **DOTween** - Animation system
- **ScriptableObjects** - Data configuration
- **PlayerPrefs** - Save system

## 📁 Project Structure

```
Assets/
├── Game/
│   ├── Scripts/
│   │   ├── Core/           # Core game logic
│   │   ├── Data/           # Data structures and models
│   │   ├── UI/             # User interface components
│   │   ├── Helpers/        # Helper classes
│   │   └── Save/           # Save system
│   ├── Prefabs/            # Unity prefabs
│   ├── SO/                 # ScriptableObjects
│   └── Textures/           # Textures and visual resources
├── Scenes/                 # Unity scenes
└── Documentation/          # Project documentation
```

## 🚀 Getting Started

1. **Requirements**:
    - Unity 2022.3 or newer
    - Git

2. **Installation**:
   ```bash
   git clone https://github.com/yourusername/CardBattle.git
   cd CardBattle
   ```

3. **Open project in Unity Hub**

4. **Run scene**: `Assets/Scenes/SampleScene.unity`

## 🎮 Controls

- **Card Selection**: Click to select card from hand
- **Unit Placement**: Drag and drop onto battlefield
- **End Turn**: "End Turn" button
- **Card Burning**: Option to trade card for mana

## 🏗 Architecture

The project uses modular architecture with clear separation of concerns:

- **MVC-like separation**: Game logic, data state, and presentation
- **Event-driven architecture** for system communication
- **Factory Pattern** for card effects
- **Service Locator** for game service access
- **Command Pattern** for move execution

### Key Components

- `GameSession` - Central game controller
- `BattleResolver` - Combat resolution system
- `CardEffectSystem` - Card effect processing
- `StaticDataService` - Centralized data access
- `SaveService` - Save system

## 📊 Current Status

### ✅ Implemented

- [x] Complete game state system
- [x] Combat system with phased execution
- [x] Card effect system
- [x] AI opponent with state analysis
- [x] Save system
- [x] Basic unit animations
- [x] Hero and magic system

### 🚧 In Development

- [ ] Full user interface
- [ ] Spell effect animations
- [ ] Deck building system
- [ ] Enhanced AI

### 📋 Planned

- [ ] Multiplayer
- [ ] Additional heroes and cards
- [ ] Achievement system
- [ ] Enhanced graphics

## 🤖 AI Development Features

This project demonstrates the capabilities of modern AI assistants in game development:

- **Architectural Planning**: AI designed modular, extensible architecture
- **Complex Logic**: Implemented sophisticated combat interaction system
- **Asynchronous Programming**: Using UniTask for smooth gameplay
- **Event System**: Decentralized architecture with events
- **Performance**: Caching and data access optimization

## 📝 Documentation

Detailed documentation is located in the `Assets/Documentation/` folder:

- `CardBattleGameOverview.txt` - Game mechanics overview
- `GameContext.txt` - Development context
- `GameStructure.txt` - Game structure
- `ProjectArchitecture.txt` - Project architecture

## 🤝 Contributing

Since this is an experimental project demonstrating AI development capabilities, external contributions are welcome for:

- User interface improvements
- Adding new cards and abilities
- Performance optimization
- Bug fixes

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by Hearthstone by Blizzard Entertainment
- Uses UniTask, DOTween libraries
- Created with AI assistant Claude (Anthropic)

---

*This project serves as a demonstration of how AI can help create complex game systems with minimal human oversight.*
