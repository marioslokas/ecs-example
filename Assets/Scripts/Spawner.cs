using Unity.Entities;
using Unity.Mathematics;

public struct Spawner : IComponentData
{
    public Entity spawnPrefab;
    public float3 SpawnPosition;
    public float NextSpawnTime;
    public float SpawnRate;
}
