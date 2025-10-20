using UnityEngine;
using BansheeGz.BGDatabase;
using System;
using System.Collections.Generic;

/// <summary>
/// 🧩 Hyper-casual meta debugger (Legacy Compatible)
/// - Works with BGDatabase 1.2.x–1.3.x
/// - Reads typed parameters (int, float, bool, string)
/// - Simulates Items, PlayerStats, Machines, Upgrades
/// - Auto-detects BGDatabase components and logs them
/// </summary>
[DisallowMultipleComponent]
public class Debugger : MonoBehaviour
{
    // 🔧 BGDatabase Components (Auto-Detected)
    private BGDatabasePreloaderGo preloader;
    private BGDataBinderDatabaseGo dbBinder;
    private BGDataBinderBatchGo batchBinder;
    private BGDataBinderFieldGo fieldBinder;
    private BGDataBinderRowGo rowBinder;
    private BGDataBinderGraphGo graphBinder;
    private BGDataBinderTemplateGo templateBinder;
    private BGEntityGo entityGo;

    void Awake()
    {
        // Auto-discover all available BG components on this GameObject
        preloader = GetComponent<BGDatabasePreloaderGo>();
        dbBinder = GetComponent<BGDataBinderDatabaseGo>();
        batchBinder = GetComponent<BGDataBinderBatchGo>();
        fieldBinder = GetComponent<BGDataBinderFieldGo>();
        rowBinder = GetComponent<BGDataBinderRowGo>();
        graphBinder = GetComponent<BGDataBinderGraphGo>();
        templateBinder = GetComponent<BGDataBinderTemplateGo>();
        entityGo = GetComponent<BGEntityGo>();
    }

    void Start()
    {
        Debug.Log("=== 🧩 HyperCasual Meta Debugger Started (Legacy Mode) ===");

        // 1️⃣ BGRepo kontrolü
        if (BGRepo.I == null)
        {
            Debug.LogError("❌ BGDatabase repository not found! Add the prefab to the scene.");
            return;
        }

        // 2️⃣ Preloader (manual trigger for older versions)
        if (preloader != null)
        {
            Debug.Log("⚙️ BGDatabasePreloaderGo detected (legacy mode).");
        }

        // 3️⃣ Tablo erişimleri
        var items = BGRepo.I["Items"];
        var machines = SafeGetTable("Machines");
        var playerStats = SafeGetTable("PlayerStats");
        var upgrades = SafeGetTable("Upgrades");
        var gameParams = SafeGetTable("GameParams");

        // 4️⃣ Parametreleri yükle
        var paramDict = LoadParameters(gameParams);

        // 5️⃣ Meta simülasyon (senin orijinal mantık)
        if (items != null) Test_Items(items);
        if (playerStats != null) Simulate_PlayerProgress(playerStats, paramDict);
        if (machines != null && upgrades != null) Simulate_MachineProduction(machines, upgrades, paramDict);

        // 6️⃣ Binder testleri
        TestAllBinders_Legacy();

        // 7️⃣ Save işlemi
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
                    default: parsed = value; break;
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

    // ======================================================
    // 🔗 AUTO BINDER TESTS (LEGACY SAFE)
    // ======================================================
    private void TestAllBinders_Legacy()
    {
        Debug.Log("=== 🔗 Testing BGDatabase Components (Legacy Safe) ===");

        if (dbBinder != null)
            Debug.Log("🧩 DB Binder found and linked to Database prefab.");

        if (batchBinder != null)
            Debug.Log("📦 Batch Binder found — will bind multiple rows automatically.");

        if (fieldBinder != null)
            Debug.Log("🎯 Field Binder found — single field binding ready.");

        if (rowBinder != null)
            Debug.Log("🧱 Row Binder found — binds one entity to GameObject.");

        if (graphBinder != null)
            Debug.Log("🔗 Graph Binder found — relational tracking available.");

        if (templateBinder != null)
            Debug.Log("🧩 Template Binder found — prefab instantiation enabled.");

        if (entityGo != null)
            Debug.Log("🎮 EntityGo found — represents one database entity in scene.");
    }
}
