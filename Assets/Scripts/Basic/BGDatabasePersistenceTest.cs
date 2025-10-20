using UnityEngine;
using BansheeGz.BGDatabase;

/// <summary>
/// 🔍 BGDatabase Runtime Save Test
/// Bu script BGDatabase’in runtime’da yapılan değişiklikleri gerçekten kaydedip kaydetmediğini test eder.
/// 1. İlk çalıştırmada tabloyu okur.
/// 2. Rastgele bir değeri değiştirir.
/// 3. BGRepo.I.Save() çağırır.
/// 4. Oyunu yeniden başlattığında değişiklik korunmuş mu diye kontrol eder.
/// </summary>
public class BGDatabasePersistenceTest : MonoBehaviour
{
    private const string TEST_TABLE = "Items"; // test edeceğin tablo ismi
    private const string TEST_FIELD = "value"; // değiştirilecek sütun
    private const string TEST_NAME = "Items0"; // değiştirilecek satırın ismi

    void Start()
    {
        if (BGRepo.I == null)
        {
            Debug.LogError("❌ BGDatabase repository not found! Scene'e BGDatabase prefab ekle.");
            return;
        }

        var table = BGRepo.I[TEST_TABLE];
        if (table == null)
        {
            Debug.LogError($"❌ Table '{TEST_TABLE}' not found!");
            return;
        }

        // Entity bul
        BGEntity target = null;
        for (int i = 0; i < table.CountEntities; i++)
        {
            var e = table.GetEntity(i);
            if (e.Get<string>("name") == TEST_NAME)
            {
                target = e;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning($"⚠️ Entity '{TEST_NAME}' bulunamadı, test iptal edildi.");
            return;
        }

        // Eski değeri oku
        int oldValue = target.Get<int>(TEST_FIELD);
        Debug.Log($"📦 Eski Değer: {oldValue}");

        // Yeni değer ata
        int newValue = oldValue + Random.Range(5, 20);
        target.Set(TEST_FIELD, newValue);
        Debug.Log($"✏️ Yeni Değer Atandı: {newValue}");

        // Veritabanını kaydet
        BGRepo.I.Save();
        Debug.Log("💾 BGRepo.I.Save() çağrıldı.");

        // Doğrulama için tekrar oku
        int confirmValue = target.Get<int>(TEST_FIELD);
        Debug.Log($"✅ Hafızadaki (RAM) güncel değer: {confirmValue}");

        // Kullanıcıya bilgi
        Debug.Log("🔁 Şimdi oyunu kapatıp yeniden başlat. Ardından aynı log'lara bak:");
        Debug.Log("Eğer değer aynıysa SAVE başarılıdır. Eski haline döndüyse, Auto Reset aktif olabilir.");
    }
}
