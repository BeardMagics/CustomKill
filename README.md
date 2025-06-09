# 🩸 CustomKill

**CustomKill** is a fork of BestKillfeed specifically refactored to use LiteDB and for the Blood Wars server.

## 🔧 Main Features
-Clan name + player name + max level in feed
-Kills / Deaths / Assists / MaxStreak system
-LiteDB persistent data storage

### ✅ Enhanced Killfeed in Chat
- Displays **clan**, **player name**, and **max level** for each kill.
- Automatically detects player max levels on **login**, **kill**, or when executing `.lb` or `.pi` commands.


### 🛡️ Kill-Steal Protection
- If **Player A** downs **Player B**, but **Player C** finishes them, the kill is credited to **Player A**.

### 🚫 Anti-Grief Level Difference System
- Configurable level-difference protection:
  - For level **91**: max difference = **10 levels**
  - For levels **below 91**: max difference = **15 levels**
- Player levels are shown in **red** if they exceed the allowed difference.

---

### 📊 Custom Commands

#### `.top` [category] – Leaderboard (top 5 players)
- Displays an aesthetic **leaderboard** with:
  - Kills, deaths, max killstreaks
  - Pagination and ranking system
  - Categories including kills, deaths, assists, ms (maxstreak)
  - Future: damage category
 
#### `.stats` - Stats
- Displays users stats including:
- Kills, Deaths, Assists, Max Killstreak
- Future: Add damage tracking for damage

#### `.pi` – Player Info
- Displays detailed **player info**:
  - Name, clan, level, clan members, and connection status
  - Name in **green** = connected  
  - Name in **red** = offline

---

### ⚙️ Easy Configuration
- Edit the `CustomKill.cfg` file to customize:
  - Text colors
  - Level difference thresholds
  - Discord webhook URL for kill notifications

---

### 🎨 Color Settings (.top / .stats)
- Customizable color settings
  - Title (change appearance when --Top 5 players-- or --Username Stats-- is read from config)
  - Kills/Deaths/Assists/Maxstreak
  - Customizable for both .top and .stats respectively

---

### ❗ Admin Flagging System
- Configurable system for adding admin only restrictions
  - Config file of CustomKill.cfg
  - Configrable to set each category as viewable by all or by admin
  - Config flag field: `RestrictKillsToAdmin` = `boolean` (true / false)

---

### 💾 Persistent Data Storage
-Uses **LiteDB** for efficient data storage, ensuring:
Better overhead performance and reliability.
Reduced load for server resources (i.e. no saving to json file)

---

## 📝 License

This mod is distributed under the **MIT License**.  
You are free to **modify** and **redistribute** it, as long as proper **credit is given** to the original author.
