# CartGame

A reusable **2D turn-based card game platform built with Unity 6 and C#**.

CartGame is designed so that gameplay logic stays independent from Unity presentation code. New card games should be buildable primarily by adding game-specific cards, deck composition, rules, AI and presentation.

> **Current scope:** offline client only. Online multiplayer/backend is intentionally out of scope for the current phase.

## Table of Contents

- [Project Goals](#project-goals)
- [Current Game](#current-game)
- [Gameplay](#gameplay)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Core Game Flow](#core-game-flow)
- [Card System](#card-system)
- [Game Rules](#game-rules)
- [Economy and IAP](#economy-and-iap)
- [Presentation](#presentation)
- [AI](#ai)
- [Extensibility](#extensibility)
- [Current Status](#current-status)
- [Roadmap](#roadmap)
- [Development Principles](#development-principles)

## Project Goals

- Keep gameplay logic independent from Unity.
- Keep cards and rules modular.
- Make game state easy to inspect and test.
- Separate rules from presentation/UI.
- Reuse the same Core for future card games.
- Provide an offline economy layer.
- Keep the architecture open to future AI improvements and possible online functionality.

## Current Game

The first sample game is a simple turn-based card game:

- 1 human player.
- 3 basic AI opponents.
- 4 players total.
- Each player starts with 12 HP.
- Each player receives 3 cards at match start.
- The deck contains 8 Attack, 8 Heal and 8 Guard cards.
- The deck is shuffled before dealing.
- The human player starts the first turn.
- A match ends when the human player is defeated or only one living player remains.

These values belong to the sample game under `Games/SimpleCardGame`, not to the generic card model.

## Gameplay

### Turn

The active player can:

1. Draw a card.
2. Play a card.
3. End the turn.

Only the active living player can submit gameplay actions.

### Attack

- Effect: `Damage`
- Value: 3
- Targets another living player.
- If no target is supplied, the rules can select the next living opponent.
- Damage is applied through `PlayerState.Damage()`.

### Heal

- Effect: `Heal`
- Value: 2
- Restores the acting player's health.
- Health cannot exceed `MaxHealth`.

### Guard

- Effect: `Guard`
- Activates a defensive state.
- Incoming damage is reduced by half, rounded up.
- Guard is consumed when damage is received.

### Card Consumption

A played card is removed from the player's hand only after its effect has been successfully resolved. Invalid plays therefore do not consume cards.

## Architecture

The project uses a layered architecture:

~~~text
┌─────────────────────────────────────────────┐
│              Unity / Presentation           │
│      GameUi / GamePresenter / Card UI       │
└──────────────────────┬──────────────────────┘
                       │ GameAction
                       ▼
┌─────────────────────────────────────────────┐
│                    Core                     │
│                                             │
│  GameEngine → GameRules → GameState         │
│       │            │                        │
│       │            ├── Cards                │
│       │            └── Players              │
│       │                                     │
│       └── GameEvent                         │
└──────────────────────┬──────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────┐
│             Game-specific layer             │
│          Games/SimpleCardGame               │
│   Card definitions / deck / game settings   │
└─────────────────────────────────────────────┘

Platform services remain separate:
Economy → Wallet / LocalPlayerData
IAP     → IIapService / Mock implementation
~~~

### Responsibility boundaries

- **GameState:** current game state.
- **GameAction:** requested player/AI action.
- **GameRules:** validation and gameplay resolution.
- **GameEngine:** orchestration, turns, lifecycle and events.
- **GameEvent:** observable gameplay results.
- **GamePresenter:** connects Unity-facing code to Core.
- **GameUi:** displays state and collects player input.
- **Game-specific layer:** defines the actual game's cards and deck.

The key dependency direction is:

~~~text
Presentation
     ↓
Core
     ↑
Game-specific configuration
~~~

Core gameplay code should not depend on Unity UI classes.

## Project Structure

~~~text
CartGame/
│
├── Assets/
│   └── Scripts/
│       └── CardGame/
│
├── Core/
│   ├── Game/
│   │   ├── GameState.cs
│   │   ├── GamePhase.cs
│   │   ├── GameAction.cs
│   │   ├── GameEvent.cs
│   │   ├── GameEngine.cs
│   │   └── GameRules.cs
│   │
│   ├── Cards/
│   │   ├── Card.cs
│   │   ├── CardDefinition.cs
│   │   ├── Deck.cs
│   │   └── Hand.cs
│   │
│   ├── Players/
│   │   └── PlayerState.cs
│   │
│   └── AI/
│       └── AI strategies
│
├── Games/
│   └── SimpleCardGame/
│       └── SimpleCardGameRules.cs
│
├── Platform/
│   ├── Economy/
│   │   ├── Wallet.cs
│   │   └── EconomyService.cs
│   │
│   ├── Storage/
│   │   └── LocalPlayerData.cs
│   │
│   └── Iap/
│       ├── IIapService.cs
│       └── Mock/Unity implementation
│
├── Presentation/
│   ├── Game/
│   │   ├── GamePresenter.cs
│   │   └── GameUi.cs
│   ├── Cards/
│   └── UI/
│
└── Bootstrap/
~~~

## Core Game Flow

~~~text
StartMatch
    │
    ├── Economy check
    ├── Create game-specific deck
    ├── Shuffle deck
    ├── Create players
    ├── Deal opening hands
    ├── Set current player
    └── Start turn
             │
             ▼
        SubmitAction
             │
       ┌─────┼──────────┐
       ▼     ▼          ▼
      Draw  Play      EndTurn
       │     │          │
       │     │          └── Next living player
       │     │
       │     └── Resolve card effect
       │             │
       │       ┌─────┼─────┐
       │       ▼     ▼     ▼
       │    Damage  Heal  Guard
       │
       └── Card → Hand

After relevant actions:
        ↓
   CheckGameOver
        ↓
    MatchEnded
~~~

## Card System

### CardDefinition

Defines a card type:

~~~text
CardDefinition
├── Id
├── Name
├── Effect
└── Value
~~~

Supported sample effects:

- `Damage`
- `Heal`
- `Guard`

### Card

Represents a concrete card instance:

~~~text
Card
├── InstanceId
└── Definition
~~~

This allows many physical instances of the same card definition while keeping every card uniquely addressable.

### Deck

Owns cards that have not been drawn.

Responsibilities:

- Add cards.
- Add ranges of cards.
- Shuffle.
- Draw.
- Clear.
- Check membership.

### Hand

Owns cards held by a player.

Responsibilities:

- Add.
- Remove by instance ID.
- Find by instance ID.
- Enumerate cards.

## Game Rules

`GameRules` is responsible for gameplay validation and resolution:

- Validate actions.
- Validate the active player.
- Draw cards.
- Play cards.
- Validate targets.
- Resolve Damage, Heal and Guard.
- Consume successfully played cards.

`GameEngine` is responsible for orchestration:

- Match lifecycle.
- Turn progression.
- Event emission.
- Turn ownership.
- Game-over detection.

This separation is intentional:

~~~text
GameEngine = orchestration
GameRules  = gameplay rules
GameState  = current state
GameAction = requested action
GameEvent  = observed result
~~~

## Economy and IAP

The project currently contains an offline economy layer.

Starting a match consumes coins:

~~~text
Player
  ↓
EconomyService.TryStartMatch()
  ↓
Enough coins?
  ├── Yes → start match
  └── No  → show insufficient coins
~~~

When coins are exhausted, the application is designed to request a purchase through `IIapService`.

A mock IAP implementation is currently used so the gameplay architecture can be developed without a real store integration.

## Presentation

### GamePresenter

Connects Unity-facing code with:

- `GameEngine`
- Economy
- IAP

It exposes operations such as:

- Start match.
- Read the player's hand.
- Draw a card.
- Play a specific card.
- End a turn.
- Purchase coins.

### GameUi

The Unity UI is responsible for:

- Displaying the player's hand.
- Creating card buttons.
- Selecting cards.
- Selecting Attack targets.
- Drawing.
- Ending turns.
- Showing status.

The UI does not resolve gameplay rules. It requests actions and lets Core decide whether they are legal.

Example:

~~~csharp
presenter.PlayCard(cardId, targetPlayerId);
~~~

## AI

The sample game is intended to use basic AI opponents.

AI should submit normal `GameAction` objects rather than modifying `GameState` directly.

Planned progression:

1. Random legal actions.
2. Basic rule-based decisions.
3. Card-aware decisions.
4. Difficulty levels.
5. More advanced strategies.

This keeps human and AI players on the same action pipeline.

## Extensibility

A future game should primarily provide its own content under `Games/`:

~~~text
Games/
└── NewCardGame/
    ├── Card definitions
    ├── Deck composition
    ├── Game-specific rules
    ├── AI strategy
    └── Presentation assets
~~~

**** GameUI To GameState
GameUi
   ↓
GamePresenter
   ↓
GameFlowController
   ↓
BasicAiStrategy
   ↓
GameAction
   ↓
GameEngine
   ↓
GameRules
   ↓
GameState
*******

The reusable Core should continue to provide:

- State management.
- Actions and events.
- Players.
- Cards.
- Deck and hand mechanics.
- Turn management.
- Generic validation infrastructure.

New game-specific content should not require rewriting the generic engine.

## Current Status

### Implemented

- [x] Unity 6 client-oriented architecture.
- [x] Offline game model.
- [x] GameState.
- [x] GameAction.
- [x] GameEvent.
- [x] GameEngine.
- [x] GameRules.
- [x] Player health and guard state.
- [x] Card definitions and instances.
- [x] Deck and Hand.
- [x] Real card drawing.
- [x] Real card playing.
- [x] Damage, Heal and Guard.
- [x] Deck shuffle.
- [x] Opening hand dealing.
- [x] Sample game deck.
- [x] Offline economy abstraction.
- [x] Mock IAP abstraction.
- [x] GamePresenter integration.
- [x] Card-oriented UI flow.

### In progress

- [ ] Complete AI turn execution.
- [ ] Final card UI polish.
- [ ] Better game result presentation.
- [ ] More detailed gameplay events.
- [ ] Automated Core tests.
- [ ] Unity scene/bootstrap integration.

## Roadmap

### Phase 1 — Core Gameplay

- [x] Core state model.
- [x] Card/deck/hand model.
- [x] Turn system.
- [x] Action validation.
- [x] Card effects.
- [x] Opening hand.
- [ ] Complete AI turn execution.
- [ ] Automated tests.

### Phase 2 — Presentation

- [ ] Final card visuals.
- [ ] Player/target selection UI.
- [ ] Health and guard indicators.
- [ ] Turn indicator.
- [ ] Match result screen.
- [ ] Action feedback and animations.

### Phase 3 — Game Content

- [ ] Additional card definitions.
- [ ] More card effects.
- [ ] Additional game modes.
- [ ] Configurable deck composition.
- [ ] Difficulty levels.

### Phase 4 — Economy

- [x] Wallet abstraction.
- [x] Local player data.
- [x] Economy service.
- [x] IAP abstraction.
- [x] Mock IAP.
- [ ] Unity store implementation.
- [ ] Purchase restoration.
- [ ] Economy persistence hardening.

### Phase 5 — Future Architecture

Online/backend functionality is **not part of the current implementation**.

If online play is added later, networking should be introduced behind explicit boundaries rather than putting networking concerns into the Core gameplay model.

## Development Principles

### 1. Core first

Gameplay rules belong in Core, not in Unity UI scripts.

### 2. Actions over direct mutation

Players and AI request actions through `GameAction`.

### 3. State is the source of truth

Presentation reads game state; it should not maintain a second gameplay state.

### 4. Events describe results

`GameEvent` provides a mechanism for presentation and other systems to react to gameplay changes.

### 5. Game-specific content stays outside generic Core

The sample deck and card definitions belong under `Games/SimpleCardGame`.

### 6. Services stay behind interfaces

Economy and IAP integrations should remain replaceable.

### 7. Design for reuse

The goal is not only one card game. The goal is a reusable foundation for future turn-based card games.

## Technology

- **Engine:** Unity 6
- **Language:** C#
- **Network model:** Offline
- **IAP:** Interface-based, mock implementation currently
- **Architecture:** Modular layered client architecture

## Project Status

This repository is an active development project. The immediate priority is completing the first end-to-end offline gameplay loop before expanding content or introducing online infrastructure.
