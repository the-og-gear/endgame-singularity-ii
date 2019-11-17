using UnityEditor;

public class ReserializeAllMenuItem
{
    [MenuItem("Development/Reserialize all items")]
    static void ReserializeAll()
    {
        AssetDatabase.ForceReserializeAssets();
    }
}
