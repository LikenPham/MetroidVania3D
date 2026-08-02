# 3D Metroidvania Game

A 3D Metroidvania game with 2D side-view gameplay developed using Unity 6.3 (URP).

## Overview

This project is a university graduation project focused on designing and implementing a modular architecture for a Metroidvania game.

The game features:

- Character movement
- Jump / Double Jump
- Dash
- Wall Jump
- Melee Combat
- Enemy AI
- Save System
- Modular Character System
- Object Pooling
- Event-driven Architecture

---

## Engine

- Unity 6.3
- Universal Render Pipeline (URP)
- C#

---

## Project Architecture

```
Assets
│
├── Core
│   ├── Data
│   ├── Events
│   ├── Interfaces
│   ├── Managers
│   ├── ScriptableObjects
│   └── Utilities
│
├── Gameplay
│   ├── Character
│   ├── Enemy
│   ├── Combat
│   ├── Camera
│   ├── Interaction
│   └── Environment
│
├── UI
│
├── Art
│
└── Audio
```

Architecture principles:

- Dependency Injection
- ScriptableObject Data
- Event Bus
- State Machine
- Object Pooling
- SOLID
- Low Coupling / High Cohesion

---

## Controls

| Action | Key |
|---------|-----|
| Move | A / D |
| Jump | Space |
| Dash | Left Shift |
| Attack | Left Mouse |
| Skill | Right Mouse |

---

## Requirements

Unity Version

```
Unity 6.3
```

Render Pipeline

```
Universal Render Pipeline (URP)
```

---

## Getting Started

Clone repository

```bash
git clone https://github.com/yourname/Metroidvania3D.git
```

Open with Unity Hub.

Open the project using Unity 6.3.

Open scene:

```
Assets/Scenes/Main.unity
```

Press Play.

---

## Current Progress

- [x] Player Movement
- [x] Camera Follow
- [x] Ground Detection
- [x] Wall Detection
- [x] Jump
- [x] Dash
- [x] Character State Machine
- [ ] Enemy AI
- [ ] Boss System
- [ ] Save System
- [ ] Inventory
- [ ] Skill Tree

---

## Screenshots

Coming Soon

---

## License

This project is for educational purposes.