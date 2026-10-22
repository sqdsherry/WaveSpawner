# 🌊 Wave Spawner — Clean Architecture & Object Pooling Showcase

![Unity Version](https://img.shields.io/badge/Unity-6000.0%20%2F%20URP-black?style=flat&logo=unity)
![Language](https://img.shields.io/badge/Language-C%23-blue?style=flat&logo=csharp)
![Architecture](https://img.shields.io/badge/Architecture-MVP%20(Passive%20View)-emerald?style=flat)
![Memory](https://img.shields.io/badge/Memory-Zero--Alloc%20UnityEngine.Pool-purple?style=flat)
![Dataflow](https://img.shields.io/badge/Data--Driven-ScriptableObjects-orange?style=flat)
![Input](https://img.shields.io/badge/Input-New%20Input%20System-red?style=flat)

An architectural, production-ready wave spawner subsystem built in **Unity**. Demonstrates strict separation of concerns, automated memory pooling (`UnityEngine.Pool`), data-driven configuration (`ScriptableObjects`), and complete decoupling via **MVP (Model-View-Presenter)** and native C# events.

---

## 🏛️ System Interaction Flow

```mermaid
flowchart LR
    subgraph Data [Data Layer]
        WC["WaveConfig / Sequence<br>(ScriptableObjects)"]
    end

    subgraph Core [Gameplay Core]
        WM["WaveController<br>(State Machine)"]
        SP["EnemySpawner<br>(ISpawner & ObjectPool)"]
        EN["Enemy<br>(IDamageable)"]
        INP["ClickRaycastController<br>(New Input System)"]
    end

    subgraph UI [UI Layer - MVP]
        PRES["WaveUIPresenter"]
        VIEW["WaveUIView<br>(Passive View)"]
    end

    WC -->|Configuration| WM
    WM -->|Spawn Command| SP
    SP -->|Pool Get| EN
    INP -->|Raycast Hit| EN
    EN -.->|OnDied Event| WM
    WM -.->|Native C# Events| PRES
    PRES -->|Update Presentation| VIEW
