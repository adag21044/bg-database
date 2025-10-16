using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using BansheeGz.BGDatabase;

/// <summary>
/// Example class demonstrating how to export and import BGDatabase data in JSON format.
/// This version is compatible with the Basic edition of BGDatabase (no Save/Load addon).
/// </summary>
public class BGDatabaseBasicExporter : MonoBehaviour
{
    private string savePath;

    void Start()
    {
        savePath = Path.Combine(Application.dataPath, "BGDatabaseExport.json");
        ExportDatabase();
    }

    // ✅ Export (Save database to JSON)
    public void ExportDatabase()
    {
        if (BGRepo.I == null)
        {
            Debug.LogError("❌ BGDatabase is not loaded!");
            return;
        }

        var dbDump = new Dictionary<string, List<Dictionary<string, object>>>();

        BGRepo.I.ForEachMeta(meta =>
        {
            var tableData = new List<Dictionary<string, object>>();

            meta.ForEachEntity(entity =>
            {
                var row = new Dictionary<string, object>();

                meta.ForEachField(field =>
                {
                    row[field.Name] = GetFieldValue(entity, field);
                });

                row["Id"] = entity.Id.ToString();
                tableData.Add(row);
            });

            dbDump[meta.Name] = tableData;
        });

        string json = JsonUtility.ToJson(new Wrapper { tables = dbDump }, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"✅ Database exported to JSON file: {savePath}");
    }

    // ✅ Import (Load database from JSON)
    public void ImportDatabase()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("❌ JSON file not found!");
            return;
        }

        string json = File.ReadAllText(savePath);
        var data = JsonUtility.FromJson<Wrapper>(json);

        foreach (var tablePair in data.tables)
        {
            var meta = BGRepo.I[tablePair.Key];
            foreach (var rowData in tablePair.Value)
            {
                var entity = meta.NewEntity();
                foreach (var kvp in rowData)
                {
                    if (kvp.Key == "Id") continue; // ID is auto-generated
                    entity.Set(kvp.Key, kvp.Value);
                }
            }
        }

        Debug.Log("✅ Database successfully imported from JSON.");
    }

    // Detects the correct field type and retrieves its value
    private object GetFieldValue(BGEntity entity, BGField field)
    {
        Type type = field.ValueType;

        if (type == typeof(string)) return entity.Get<string>(field.Name);
        if (type == typeof(int)) return entity.Get<int>(field.Name);
        if (type == typeof(float)) return entity.Get<float>(field.Name);
        if (type == typeof(bool)) return entity.Get<bool>(field.Name);
        if (type == typeof(double)) return entity.Get<double>(field.Name);

        return entity.Get<string>(field.Name);
    }

    // Serializable wrapper for JSONUtility (since it doesn't handle nested dictionaries by default)
    [Serializable]
    public class Wrapper
    {
        public Dictionary<string, List<Dictionary<string, object>>> tables;
    }
}
