using UnityEngine;
using BansheeGz.BGDatabase;
using System;

public class Debugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("=== BGDatabase Debugger Started ===");

        if (BGRepo.I == null)
        {
            Debug.LogError("❌ BGDatabase repository not found!");
            return;
        }

        Debug.Log($"Total table count: {BGRepo.I.CountMeta}");

        // Retrieve the "Items" table
        var itemsTable = BGRepo.I["Items"];

        if (itemsTable.CountEntities == 0)
        {
            Debug.LogWarning("⚠️ The 'Items' table has no rows.");
            return;
        }

        // Get the first row (entity)
        var firstItem = itemsTable.GetEntity(0);
        Debug.Log($"First item: {firstItem.Get<string>("name")}");

        // List all items
        itemsTable.ForEachEntity(entity =>
        {
            string name = entity.Get<string>("name");
            int value = entity.Get<int>("value");
            Debug.Log($"Item: {name} (Value: {value})");
        });

        // Display the entire database content
        ShowAllRepo();
    }

    // Display all tables and entities in the repository
    private void ShowAllRepo()
    {
        BGRepo.I.ForEachMeta(meta =>
        {
            Debug.Log($"--- TABLE: {meta.Name} ---");
            meta.ForEachEntity(entity =>
            {
                Debug.Log($"Entity ID: {entity.Id}");
                meta.ForEachField(field =>
                {
                    try
                    {
                        object value = GetFieldValue(entity, field);
                        Debug.Log($"{field.Name}: {value}");
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"Failed to read field: {field.Name} ({e.Message})");
                    }
                });
                Debug.Log("------------------");
            });
        });
    }

    // Automatically determines the correct data type when reading field values
    private object GetFieldValue(BGEntity entity, BGField field)
    {
        Type type = field.ValueType;

        if (type == typeof(string)) return entity.Get<string>(field.Name);
        if (type == typeof(int)) return entity.Get<int>(field.Name);
        if (type == typeof(float)) return entity.Get<float>(field.Name);
        if (type == typeof(bool)) return entity.Get<bool>(field.Name);

        return entity.Get<string>(field.Name);
    }
}
