using UnityEngine;
using BansheeGz.BGDatabase;
using System;

/// <summary>
/// Final legacy-safe BGDatabase debugger.
/// Works fully with read + update on old BGDatabase versions (no Add support).
/// Focused on "Items" table.
/// </summary>
public class Debugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== 🧩 BGDatabase: Items Table Debugger Started (Read/Write Mode) ===");

        if (BGRepo.I == null)
        {
            Debug.LogError("❌ BGDatabase repository not found! Make sure the BGDatabase prefab exists in the scene.");
            return;
        }

        // Get the Items table
        var meta = BGRepo.I["Items"];
        if (meta == null)
        {
            Debug.LogError("❌ Table 'Items' not found in BGDatabase!");
            return;
        }

        // If table is empty
        if (meta.CountEntities == 0)
        {
            Debug.LogWarning("⚠️ 'Items' table has no rows. Please add rows manually in BGDatabase Editor.");
            return;
        }

        // 1️⃣ Dump all current data
        DumpItems(meta);

        // 2️⃣ Modify a random one
        ModifyRandomItem(meta);

        // 3️⃣ Save database
        BGRepo.I.Save();
        Debug.Log("💾 Repository saved successfully.");

        // 4️⃣ Dump again to confirm
        DumpItems(meta);

        Debug.Log("✅ BGDatabase runtime debugging complete.");
    }

    // ======================================================
    // 🔍 Dump Items Table
    // ======================================================
    private void DumpItems(BGMetaEntity meta)
    {
        Debug.Log($"=== 📦 Dumping 'Items' Table ({meta.CountEntities} rows) ===");

        for (int i = 0; i < meta.CountEntities; i++)
        {
            var entity = meta.GetEntity(i);
            string name = entity.Get<string>("name");
            int value = entity.Get<int>("value");
            Debug.Log($"🧱 Item #{i}: Name={name}, Value={value}");
        }
    }

    // ======================================================
    // ✏️ Modify Random Entity
    // ======================================================
    private void ModifyRandomItem(BGMetaEntity meta)
    {
        if (meta.CountEntities == 0) return;

        int randomIndex = UnityEngine.Random.Range(0, meta.CountEntities);
        var entity = meta.GetEntity(randomIndex);

        string name = entity.Get<string>("name");
        int oldValue = entity.Get<int>("value");

        int newValue = oldValue + UnityEngine.Random.Range(1, 5);
        entity.Set("value", newValue);

        Debug.Log($"✏️ Updated item '{name}' (old value={oldValue} → new value={newValue})");
    }
}
