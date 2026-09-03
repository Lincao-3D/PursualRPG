# Pursual RPG
**Pursual RPG** is a modular, AI-driven text role-playing game built using Godot 4 + C# .NET. It bridges classic tabletop mechanics with dynamic, LLM-assisted storytelling while maintaining mobile-ready performance and offline flexibility.

## Main RPG Traits
* Classic D&D-Inspired Character Attributes & Races

* Fully featured stat systems tracking core attributes (Strength, Dexterity, Constitution, Intelligence, Wisdom, Charisma) and expertises.  
 

* Racial bonuses and class factories supporting archetypes such as Warriors, Paladins, Rogues, Mages, Clerics, Bards, and Barbarians.  
 

## Dynamic Attribute Generation & Dice Systems

* Automated character creation rolls using classic tabletop rules (e.g., 4d6 drop lowest).  
 

* Toggleable Physical Dice Support: Gives players the option to input real-world dice rolls manually through the UI framework.  
 

## Tactical Turn-Based Combat System

* Dedicated encounter management supporting enemies like Skeletons, Bandits, Zombies, and Angry Villagers.  
 

* Complex action queues, active status effects (Stunned, Aiming, Reckless, Divine Smith), turn ordering, and optional flee mechanics.  
 

## AI Game Master & Tool Integration

* Powered by an asynchronous message broker and LLM client capable of parsing structured   tool payloads (initialize_combat, reward_player, take_item) dynamically.  
 

* Rich typewriter narrative text rendering with fast-forward capabilities for smooth pacing.  
 

## Modular World Builder Assistant

* Built-in context compilation tools allowing the game to read C# domain mechanics and inject them directly into AI scenarios to generate custom campaigns.  
 

## Retro Synth Audio & Localization Ready

* Custom programmatic sound effects and volume controls routed directly through Godot's audio bus framework.  
 

## Full localization architecture mapped via CSV strings to support multiple languages (such as English and Brazilian Portuguese).  
 