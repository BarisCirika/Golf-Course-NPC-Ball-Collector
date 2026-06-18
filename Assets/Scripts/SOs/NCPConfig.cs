using UnityEngine;

[CreateAssetMenu(fileName = "NPCPrefabRegistry", menuName = "GCNBC/NPC Prefab Registry")]
public class NPCPrefabRegistry : ScriptableObject
{
    [Header("NPC List")]
    [SerializeField] private GameObject[] _npcPrefabs;
    public GameObject GetRandomNpcPrefab()
    {
        if (_npcPrefabs == null || _npcPrefabs.Length == 0)
        {
            Debug.LogError("[PrefabRegistry] No NPC prefabs assigned!");
            return null;
        }
        return _npcPrefabs[Random.Range(0, _npcPrefabs.Length)];
    }
}