# 🎉 Party Tricks
[![Build](https://github.com/jadeharlev/PartyTricks/actions/workflows/build.yml/badge.svg)](https://github.com/jadeharlev/PartyTricks/actions/workflows/build.yml) [![Test Swinging.Core](https://github.com/jadeharlev/PartyTricks/actions/workflows/test-swinging-core.yml/badge.svg)](https://github.com/jadeharlev/PartyTricks/actions/workflows/test-swinging-core.yml) [![Test Results.Core](https://github.com/jadeharlev/PartyTricks/actions/workflows/test-results-core.yml/badge.svg)](https://github.com/jadeharlev/PartyTricks/actions/workflows/test-results-core.yml)

Party Tricks is a four-player minigame rush with strategic elements currently in development using Unity 6.  

**Just interested in trying it? Jump to [How to Play](#how-to-play).**

## Overview

Party Tricks is a party game that incorporates strategy so that anyone can win, not just those who have mastered the game's controls! Spend funds on power-ups from the shop for unique advantages in each minigame type; being the best at a particular minigame is good, but overall strategy is equally important to winning in this game. 

The game supports both macOS & Windows, and may be played with up to four controllers (though a controller is not required).

### Authors

- Jade Harlev (Producer, Programmer)
- Kamron Swingle (Programmer, UX, Level Design)
- Ren Peng (Theme and Narrative, Gamefeel, 3D Artist)
- Ryn Reid (2D Artist)

### Minigame Types

The game is balanced around three minigame types: **Combat**, **Gambling**, and **Movement**. For this vertical slice, we're solely focusing on **Combat** and **Movement** minigames. 

In **Combat** minigames, eliminate the other players by directly fighting them. In **Gambling** minigames, make strategic wagers and hope to win it big. And, finally, in **Movement** minigames, show off your skills in tough challenges without directly attacking your opponents.

### Minigames

#### Blackjack
Minigame Type: Gambling

Bet as much as you'd like and try to win it all!

![Blackjack Gameplay](https://github.com/user-attachments/assets/1216029d-e2a8-4a65-880f-42745cf5600e)

#### Dire Dodging
Minigame Type: Combat

2D battle minigame; shoot your opponents with quick regular attacks or powerful charged attacks and rack up the most eliminations.

![Dire Dodging Gameplay](https://github.com/user-attachments/assets/693b054e-2c23-45bb-ba14-40df5d9989b0)

#### Coin Tilt
Minigame Type: Movement

Traverse a floating platform and collect coins. Be careful, though, because it's easy to fall off!

![Coin Tilt Gameplay](https://github.com/user-attachments/assets/3ea0d420-e18c-4f88-8003-259a296d50e3)

#### Vine Swinging
Minigame Type: Movement

Swing from vine to vine while collecting coins. Make it as far as you can while maximizing coin collection to win!

![Vine Swinging Gameplay](https://github.com/user-attachments/assets/4c8312ec-5277-4743-837d-14b1515650ff)


### Shop
The shop displays between minigames and includes power-ups for the player to purchase. 

![Shop Gameplay](https://github.com/user-attachments/assets/ebc8525d-9a43-4fdc-a45a-f15f542360bc)

### Power-Ups
Power-ups are generally minigame-type dependent, and last for the duration of an entire game (multiple rounds / until somebody wins).

For example, the **Magnet** powerup will magnetize coins to you in **Movement** minigames, allowing you to rack up points more quickly and take fewer risks.

Meanwhile, the **Increased HP** powerup will make you more resilient in **Combat** minigames, allowing you to take more hits without dying.

![Magnet Powerup Demonstration](https://github.com/user-attachments/assets/33df0785-57bf-4e65-8d53-64cb3a50fd8e)
_Pictured: Player 1 (top left) attracts coins using the Magnet powerup in the Coin Tilt minigame._

### Gameplay Video
(last updated Feb 17, 2026)

Most Recent Footage: [Sprint 2 Trailer](https://youtu.be/fwe8L3UIld4)

Voiced Trailer: [Sprint 1 Trailer](https://www.youtube.com/watch?v=D4D7D5y370I)

_Not fully representative of work as of the end of Sprint 2. Will be updated around the end of Sprint 3._

## How to Play

### Running the Game
Try running our [latest build](https://github.com/jadeharlev/PartyTricks/releases) from our GitHub releases! 

_If there's no recent build, follow the [Development Guide](#development-guide) to build the project yourself._

### Controls

| Action                                                   | Keyboard/Mouse                | Controller             |
|----------------------------------------------------------|-------------------------------|------------------------|
| Move / Navigate Menus                                    | W/A/S/D, arrow keys, or mouse | Left joystick or D-pad |
| Perform Action / Select (hold to charge in Dire Dodging) | Enter or Spacebar             | A / Cross              |
| Secondary Action / Go Back / Unlock Shop                 | Esc                           | B / Circle             |
| Pause                                                    | P                             | + / Start              |
| Debug Menu (P1 only)                                     | Ctrl + Esc                    | - / Select             |

## Roadmap
_Last updated Mar 5, 2026_

### Completed Sprints
#### Sprint 1 (Ended February 19)

Planned and Complete: Dire Dodging Minigame Rework, Vine Swinging minigame

Deferred: title screen art, 3D character models for Coin Tilt

Also completed: sound effects and music for all minigames, shop tweaks (QOL, audio), manual board configuration (dev tool), simple tutorial system, controller improvements, visual change to Coin Tilt

#### Sprint 2 (Ended March 5)

Planned and complete: Title screen art (not yet integrated), refinement for all minigames (visual overhauls for Dire Dodging and Vine Swinging, gameplay refinement for Vine Swinging)

Deferred: Dire Dodging character art, 3D character models for Coin Tilt (started)

Nixed: air hockey minigame

Completed (but not explicitly planned): miscellaneous menu improvements, bug fixes, shop feedback, debug view options, various sound effects, music in more places, shop rigging in Unity, powerup buffs, options menu
 
### Current Sprint

**Sprint 3 (Ends March 19)** 

Planned: Minigames further refined, Vine Swinging art (initial), Dire Dodging character art, Title Screen integration, Coin Tilt Model integration and refinement, more sound effects, visual overhauls for powerups, board display visual overhaul, Shockwave combat powerup, additional visually interesting powerup

### Upcoming Sprints

**Sprint 4 (Ends April 2)** Victory screen rework complete, IEEE trailer outline and V1, final art for Vine Swinging

**Sprint 5 (Ends April 16):** Game (vertical slice) is done! Any other work is polish.

## Development Guide
### Requirements

**Unity Version**: 6000.2.7f2

**FMOD Studio (installed separately)**: Version 2.03

### How to Build or Tweak:

1. Clone the project
2. Use "Add project from disk" in the Unity Hub
3. Run the project!
 
Builds can be created using the Unity build menu (file -> build profiles -> build).

### Contributing
- This project is currently not open to external contributions, but we're open to feedback!
