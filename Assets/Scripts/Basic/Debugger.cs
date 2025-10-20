using UnityEngine;
using BansheeGz.BGDatabase;
using System;
using System.Collections.Generic;

/// <summary>
/// Hyper-casual meta simulation debugger with parameter type support.
/// Reads GameParams types dynamically (int, float, bool, string) and applies them.
/// </summary>
public class Debugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 🧩 HyperCasual Meta Debugger Started (with Typed Params) ===");

        if (BGRepo.I == null)
        {
            Debug.LogError("❌ BGDatabase repository not found! Add the prefab to the scene.");
            return;
        }

        // Fetch tables safely
        var items = BGRepo.I["Items"];
        var machines = SafeGetTable("Machines");
        var playerStats = SafeGetTable("PlayerStats");
        var upgrades = SafeGetTable("Upgrades");
        var gameParams = SafeGetTable("GameParams");

        // Load all parameters into memory as dictionary
        var paramDict = LoadParameters(gameParams);

        // 1️⃣ Items basic test
        if (items != null) Test_Items(items);

        // 2️⃣ Player progress test
        if (playerStats != null) Simulate_PlayerProgress(playerStats, paramDict);

        // 3️⃣ Machine production
        if (machines != null && upgrades != null) Simulate_MachineProduction(machines, upgrades, paramDict);

        // Save repo
        BGRepo.I.Save();
        Debug.Log("💾 Repository saved successfully.");
        Debug.Log("✅ Simulation finished.");
    }

    // ======================================================
    // ✅ Safe table getter
    // ======================================================
    private BGMetaEntity SafeGetTable(string name)
    {
        var table = BGRepo.I[name];
        if (table == null)
        {
            Debug.LogWarning($"⚠️ Table '{name}' not found. Skipping...");
            return null;
        }
        return table;
    }

    // ======================================================
    // ⚙️ Load typed parameters into dictionary
    // ======================================================
    private Dictionary<string, object> LoadParameters(BGMetaEntity meta)
    {
        var dict = new Dictionary<string, object>();
        if (meta == null || meta.CountEntities == 0)
        {
            Debug.LogWarning("⚠️ No GameParams found.");
            return dict;
        }

        Debug.Log("=== 🧮 Loading GameParams (Typed) ===");
        for (int i = 0; i < meta.CountEntities; i++)
        {
            var e = meta.GetEntity(i);
            string key = e.Get<string>("Key");
            string value = e.Get<string>("Value");
            string type = e.Get<string>("Type").ToLower();

            object parsed = value;
            try
            {
                switch (type)
                {
                    case "int": parsed = int.Parse(value); break;
                    case "float": parsed = float.Parse(value); break;
                    case "bool": parsed = bool.Parse(value); break;
                    default: parsed = value; break; // string fallback
                }
            }
            catch (Exception)
            {
                Debug.LogWarning($"⚠️ Could not parse parameter '{key}' as {type}, keeping as string.");
            }

            dict[key] = parsed;
            Debug.Log($"🧩 Param Loaded → {key} = {parsed} ({type})");
        }

        return dict;
    }

    // ======================================================
    // 🧱 ITEMS TEST
    // ======================================================
    private void Test_Items(BGMetaEntity meta)
    {
        Debug.Log($"=== 📦 Testing 'Items' Table ({meta.CountEntities} rows) ===");

        for (int i = 0; i < meta.CountEntities; i++)
        {
            var entity = meta.GetEntity(i);
            string name = entity.Get<string>("name");
            int value = entity.Get<int>("value");

            int newValue = value + UnityEngine.Random.Range(1, 5);
            entity.Set("value", newValue);

            Debug.Log($"🧱 Item '{name}' updated → {value} ➜ {newValue}");
        }
    }

    // ======================================================
    // 🪙 PLAYER PROGRESSION
    // ======================================================
    private void Simulate_PlayerProgress(BGMetaEntity playerStats, Dictionary<string, object> paramDict)
    {
        Debug.Log("=== 🎮 Simulating Player Progression ===");

        var player = playerStats.GetEntity(0);
        int coins = player.Get<int>("Coins");
        int xp = player.Get<int>("XP");
        int level = player.Get<int>("Level");

        int xpRequired = paramDict.ContainsKey("XPForNextLevel") ? Convert.ToInt32(paramDict["XPForNextLevel"]) : 100;
        float coinMultiplier = paramDict.ContainsKey("CoinMultiplier") ? Convert.ToSingle(paramDict["CoinMultiplier"]) : 1f;

        xp += UnityEngine.Random.Range(10, 30);
        coins += Mathf.RoundToInt(UnityEngine.Random.Range(5, 15) * coinMultiplier);

        if (xp >= xpRequired)
        {
            xp = 0;
            level++;
            Debug.Log($"⬆️ Player leveled up! Now Level {level}");
        }

        player.Set("XP", xp);
        player.Set("Coins", coins);
        player.Set("Level", level);

        Debug.Log($"🎮 Player Progress → Level={level}, XP={xp}/{xpRequired}, Coins={coins} (x{coinMultiplier})");
    }

    // ======================================================
    // ⚙️ MACHINE PRODUCTION
    // ======================================================
    private void Simulate_MachineProduction(BGMetaEntity machines, BGMetaEntity upgrades, Dictionary<string, object> paramDict)
    {
        Debug.Log("=== 🏭 Simulating Machine Output ===");

        float outputMultiplier = paramDict.ContainsKey("OutputMultiplier") ? Convert.ToSingle(paramDict["OutputMultiplier"]) : 1f;

        for (int i = 0; i < machines.CountEntities; i++)
        {
            var machine = machines.GetEntity(i);
            string name = machine.Get<string>("name");
            int baseOutput = machine.Get<int>("BaseOutput");
            int level = machine.Get<int>("Level");

            int totalBonus = 0;
            for (int j = 0; j < upgrades.CountEntities; j++)
            {
                var upgrade = upgrades.GetEntity(j);
                if (upgrade.Get<string>("TargetMachine") == name)
                    totalBonus += upgrade.Get<int>("bonus");
            }

            float finalOutput = (baseOutput + totalBonus + (level * 2)) * outputMultiplier;
            Debug.Log($"🏭 Machine '{name}' → Output={finalOutput:F1} (Base={baseOutput}, Level={level}, Bonus={totalBonus}, x{outputMultiplier})");
        }
    }
}
