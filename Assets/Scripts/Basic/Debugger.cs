using UnityEngine;
using BansheeGz.BGDatabase;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 🧩 Hyper-casual meta debugger (BGDatabase Integrated)
/// - Works with BGDatabase 1.2.x–1.3.x
/// - Reads typed parameters (int, float, bool, string)
/// - Connects GameObjects to DB rows dynamically
/// - Demonstrates auto-binding via BGEntityGo and BGDataBinderFieldGo
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

    // 🔗 Example GameObjects for DB connections
    [Header("DB Connection Targets")]
    public GameObject playerObject;
    public GameObject machineObject;
    public GameObject uiCoinText;

    void Awake()
    {
        // Auto-discover BG components on this GameObject
        preloader = GetComponent<BGDatabasePreloaderGo>();
        dbBinder = GetComponent<BGDataBinderDatabaseGo>();
        batchBinder = GetComponent<BGDataBinderBatchGo>();
        fieldBinder = GetComponent<BGDataBinderFieldGo>();
        rowBinder = GetComponent<BGDataBinderRowGo>();
        graphBinder = GetComponent<BGDataBinderGraphGo>();
        templateBinder = GetComponent<BGDataBinderTemplateGo>();
        entityGo = GetComponent<BGEntityGo>();

        var tmp = uiCoinText.GetComponent<TextMeshPro>();
        if (tmp == null)
        {
            Debug.LogWarning("⚠️ UI object does not have TextMeshProUGUI component!");
            return;
        }
    }

    void Start()
    {
        DebugListFields(BGRepo.I["Items"]);
        Debug.Log("=== 🧩 HyperCasual Meta Debugger Started (Full Integration) ===");

        if (BGRepo.I == null)
        {
            Debug.LogError("❌ BGDatabase repository not found! Add the prefab to the scene.");
            return;
        }

        // 1️⃣ Safe table loading
        var items = SafeGetTable("Items");
        var machines = SafeGetTable("Machines");
        var playerStats = SafeGetTable("PlayerStats");
        var upgrades = SafeGetTable("Upgrades");
        var gameParams = SafeGetTable("GameParams");

        // 2️⃣ Load typed parameters
        var paramDict = LoadParameters(gameParams);

        // 3️⃣ Simulations
        if (items != null) Test_Items(items);
        if (playerStats != null) Simulate_PlayerProgress(playerStats, paramDict);
        if (machines != null && upgrades != null) Simulate_MachineProduction(machines, upgrades, paramDict);

        // 4️⃣ Connect GameObjects to DB rows
        ConnectGameObjectsToDatabase(playerStats, machines);

        

        // 5️⃣ Auto-bind UI fields (e.g., coins)
        BindUIFieldToDatabase(playerStats, "Coins");

        // 6️⃣ Test all detected binders
        TestAllBinders_Legacy();

        // 7️⃣ Save database
        BGRepo.I.Save();
        Debug.Log("💾 Repository saved successfully.");
        Debug.Log("✅ Simulation finished.");
    }

    // ======================================================
    // 🔗 Connect GameObjects to DB Rows (BGEntityGo)
    // ======================================================
    private void ConnectGameObjectsToDatabase(BGMetaEntity playerStats, BGMetaEntity machines)
    {
        Debug.Log("=== 🔗 Connecting GameObjects to Database Rows ===");

        if (playerObject != null && playerStats != null)
        {
            var entityGo = playerObject.AddComponent<BGEntityGo>();
            entityGo.Entity = playerStats.GetEntity(0);
            Debug.Log($"🎮 Linked {playerObject.name} → PlayerStats[0]");
        }

        if (machineObject != null && machines != null)
        {
            var entityGo = machineObject.AddComponent<BGEntityGo>();
            entityGo.Entity = machines.GetEntity(0);
            Debug.Log($"🏭 Linked {machineObject.name} → Machines[0]");
        }
    }

    // ======================================================
    // 🎯 Bind a specific field to a UI GameObject (LEGACY SAFE)
    // ======================================================
    private void BindUIFieldToDatabase(BGMetaEntity playerStats, string fieldName)
    {
        if (uiCoinText == null || playerStats == null)
        {
            Debug.LogWarning("⚠️ UI or PlayerStats not assigned.");
            return;
        }

        var entity = playerStats.GetEntity(0);
        if (entity == null)
        {
            Debug.LogWarning("⚠️ No entity found in PlayerStats table!");
            return;
        }

        // 🔍 Field adını normalize et (case-insensitive karşılaştırma)
        string matchedField = null;
        for (int i = 0; i < playerStats.CountFields; i++)
        {
            var field = playerStats.GetField(i);
            if (string.Equals(field.Name.Trim(), fieldName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                matchedField = field.Name;
                break;
            }
        }

        if (matchedField == null)
        {
            Debug.LogWarning($"⚠️ Field '{fieldName}' not found in PlayerStats!");
            return;
        }

        // 🧠 Değeri al
        object value = "N/A";
        
        try
        {
            // En sık kullanılan tipleri sırayla dene
            try { value = entity.Get<int>(matchedField); }
            catch
            {
                try { value = entity.Get<float>(matchedField); }
                catch
                {
                    try { value = entity.Get<string>(matchedField); }
                    catch
                    {
                        try { value = entity.Get<bool>(matchedField); }
                        catch
                        {
                            value = "(unknown type)";
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ Could not read value for '{matchedField}': {ex.Message}");
            value = "(error)";
        }

        // 💬 UI Text güncelle
        var tmp = uiCoinText.GetComponent<TextMeshProUGUI>();
        if (tmp == null)
        {
            Debug.LogError($"❌ {uiCoinText.name} has no TextMeshProUGUI component!");
            return;
        }
        tmp.text = $"{matchedField}: {value}";
        Debug.Log($"💬 Bound UI '{uiCoinText.name}' → PlayerStats[{matchedField}] = {value}");
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

    private void DebugListFields(BGMetaEntity table)
    {
        if (table == null)
        {
            Debug.LogWarning("⚠️ Table is null!");
            return;
        }

        Debug.Log($"=== 🧩 Listing fields for table: {table.Name} ===");

        try
        {
            int fieldCount = table.CountFields;
            for (int i = 0; i < fieldCount; i++)
            {
                var field = table.GetField(i);
                string fieldName = field.Name;

                // 🎯 Tipi runtime’dan tahmin et (tablodaki ilk entity’den oku)
                string fieldType = "Unknown";
                if (table.CountEntities > 0)
                {
                    try
                    {
                        var entity = table.GetEntity(0);
                        var value = entity.Get<object>(fieldName);
                        fieldType = value != null ? value.GetType().Name : "null";
                    }
                    catch { /* ignore */ }
                }

                Debug.Log($"🧠 Field[{i}] → {fieldName} (RuntimeType={fieldType})");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Could not read fields: {ex.Message}");
        }
    }



}
