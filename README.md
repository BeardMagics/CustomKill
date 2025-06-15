# 💀 CustomKill

**CustomKill** is a fork of BestKillfeed specifically refactored to use LiteDB and for the Blood Wars server.
Credits and copyright: 
Credit for original author(s)
- Deca (retired) (https://github.com/decaprime) [OG Killfeed author]
- Sunrise (https://github.com/SunrisesvRising/BestKillfeed)
- Morphine (Phlebotomist) (https://github.com/phlebotomist/PvPDetails)

New Authors:
- BeardMagics (CustomKill) (https://github.com/BeardMagics/CustomKill)

## 🔧 Main Features
- [Clan name] + Player Name + (Level) in feed
  - Damage / Kills / Deaths / Assists / MaxStreak system
  - LiteDB persistent data storage

## ☑️ Enhanced Killfeed in Chat
- Displays **clan**, **player name**, and **max level** for each kill.
- Automatically detects player max levels on **login**, **kill**, or when executing `.lb` or `.pi` commands.


## 🛡️ Kill-Steal Protection
- If **Player A** downs **Player B**, but **Player C** finishes them, the kill is credited to **Player A**.

## 🚫 Anti-Grief Level Difference System
- Configurable level-difference protection:
  - For level **91**: max difference = **10 levels**
  - For levels **below 91**: max difference = **15 levels**
- Player levels are shown in **red** if they exceed the allowed difference.

---

## 📊 Custom Commands

#### `.top` [category] – Leaderboard (top 5 players)
- Displays an aesthetic **leaderboard** with:
  - Kills, deaths, max killstreaks
  - Pagination and ranking system
  - Categories including clan, damage, kills, deaths, assists, ms (maxstreak)
  - Future: ~~damage category~~ (DONE) , ~~top clan category~~ (DONE) [kills / deaths ]
  
#### `.stats` - Stats
- Displays users stats including:
  - Damage, Kills, Deaths, Assists, Max Killstreak
  - Future: ~~damage tracking~~ (DONE) , assist tracking (Needs testing)

#### `.rs` - Reset Stats
- Wipes database of player information, clan association, members, and stats
  - Prompt system to avoid accidental deletion ( `.y` or `.n` )
  - ~~Future TODO: Export database to file~~ (DONE) | Discord command to commit stats (webhook with pagination logic)
 
#### `.exportstats` - Export to CSV
- Fetches all database information and exports to CSV table file
  - Added with 1.1 update
  - Exports into clean CSV for easy tracking
  - Displays Playername, Clan name, Clan tag, Damage, Kills, Deaths, Assists

#### `.kd` and `.kda`
- Displays current Kill/Death ratio
  - Added kda for Kill/death/assist ratio
  - User only, no display of other users KD/KDA

#### `.pi` – Player Info
- Displays detailed **player info**:
  - Name, clan, level, clan members, and connection status
  - Name in **green** = connected  
  - Name in **red** = offline

---

## ⚙️ Easy Configuration
- Edit the `CustomKill.cfg` file to customize:
  - Text colors
  - Level difference thresholds
  - Discord webhook URL for kill notifications

---

## 🎨 Color Settings (.top / .stats)
- Customizable color settings
  - Title (change appearance when --Top 5 players-- or --Username Stats-- is read from config)
  - Kills/Deaths/Assists/Maxstreak
  - Customizable for both .top and .stats respectively

---

## ❗ Admin Flagging System
- Configurable system for adding admin only restrictions
  - Config file of CustomKill.cfg
  - Configrable to set each category as viewable by all or by admin
  - Config flag field: `RestrictKillsToAdmin` = `boolean` (true / false)

---

## 💾 Persistent Data Storage
-Uses **LiteDB** for efficient data storage, ensuring:
Better overhead performance and reliability.
Reduced load for server resources (i.e. no saving to json file)

---

### Misc Info:
All source is distributed as is. I do not own any strict rights to any material contained within.
You are free to copy, modify, change, enhance, distribute as you see fit.
I ask that you include the original authors (top of the readme) and myself for credits and contributions.
You can contact me on discord in the V-Rising Modding discord. Username BeardMagics.

## 📝 License

This mod is distributed under the **MIT License**.  
You are free to **modify** and **redistribute** it, as long as proper **credit is given** to the original author.
