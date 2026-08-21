# Architect of Mind

An interactive, therapeutic mindfulness game concept designed for mobile platforms. The game features 5 distinct cognitive modules represented as floating islands in a serene sky, allowing players to perform mental health exercises, balance thoughts, gather positive resource memories, and rebuild mental ruins.

---

## 🌟 Core Concept

**Architect of Mind** translates psychological cognitive exercises into engaging 3D gameplay mechanics. Players explore a set of floating islands representing different aspects of the mind:

```
                  ┌──────────────────────┐
                  │  ARCHITECT OF MIND   │
                  │   Core Island Hub    │
                  └──────────┬───────────┘
                             │
       ┌──────────────┬──────┴───────┬──────────────┐
       ▼              ▼              ▼              ▼
┌───────────┐  ┌───────────┐  ┌───────────┐  ┌───────────┐
│  MINDFUL  │  │   INNER   │  │ RESOURCE  │  │  HEALING  │
│  GROWING  │  │ DIALOGUE  │  │   PATH    │  │ TOGETHER  │
└───────────┘  └───────────┘  └───────────┘  └───────────┘
```

---

## 🏝️ The 5 Cognitive Modules

### 1. 🌱 Mindful Growing (*Achtsames Wachsen*)
* **Objective:** Cultivate gratitude and practice mindfulness.
* **Gameplay:** Plant seeds of gratitude in fertile soil and water them. Perform slow-breathing exercises to help the plants grow into beautiful, blooming flowers.
* **Key Metric:** Gratitude Counter (e.g., `2/5` daily goals).

### 2. ⚖️ Inner Dialogue (*Innerer Dialog*)
* **Objective:** Balance and strengthen positive inner voices.
* **Gameplay:** Twin islands (one bright and sunny, one dark and thorny) are linked by a fragile rope bridge. Speech bubbles representing thoughts appear. The player balances a thought seesaw (*Gedanken-Wippe*) by filtering out negative statements ("You will fail") and reinforcing positive ones ("I can do this").

### 3. 🗺️ Resource Path (*Ressourcen-Pfad*)
* **Objective:** Reactivate social contacts, hobbies, and positive memories.
* **Gameplay:** Navigate a winding path across floating stepping stones. Collect positive memory crystals (*Erinnerungs-Kristalle*) while navigating around falling stress fragments to clear the mind path.

### 4. 🌳 Stress Distraction (*Stress-Ablenkung*)
* **Objective:** Focus on constructive building and ignore negative triggers.
* **Gameplay:** Plant protective trees and gardens to shield your island from incoming stress triggers shown on the local map radar, channeling focus away from anxieties.

### 5. 🏛️ Healing Together (*Gemeinsames Heilen*)
* **Objective:** Rebuild mental strength through collaborative restoration.
* **Gameplay:** Restoring a ruined floating temple. Players clean pillars, water vegetation, and rebuild stone structures to raise a progress bar towards complete healing.

---

## 🛠️ Tech Stack & Controls

* **Game Engine:** Unity 6 (URP - Universal Render Pipeline)
* **Input System:** Modern Unity Input System package.
* **Mobile Ready:** Programmatic on-screen virtual joystick, swipe-to-look touch controls, action buttons, and color palette.
* **Editor Support:** Seamless Keyboard/Mouse fallback for fast testing in the editor.

---

## 🚀 Quick Start & Setup

1. Open the project in the **Unity Editor**.
2. Go to the top main menu bar and select:
   **Architect of Mind** $\rightarrow$ **Setup Demo Scene**
3. The editor script will automatically:
   * Construct the 3D Ground and setup the Player.
   * Spawn a stack of interactive physics blocks.
   * Setup the mobile touchscreen UI Canvas overlay.
4. Press **Play** in Unity:
   * **Walk:** `WASD` / Drag Left Virtual Joystick
   * **Look:** Mouse Move / Swipe Right Side of screen
   * **Spawn Block:** Left Click / Tap `BUILD`
   * **Destroy Block:** Right Click / Tap `DEL`
   * **Grab Block (Telekinesis):** Hold `E` / Hold `MIND HOLD` button
   * **Change Color:** Scroll wheel / Tap color circles
