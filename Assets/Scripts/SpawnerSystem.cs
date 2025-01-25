using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
public partial struct SpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        
    }

    public void OnDestroy(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // invocations of SystemAPI.Querry need to be inside a foreach loop? throw everything you knew out the window
        // var query = SystemAPI.Query<RefRW<Spawner>>();
        foreach (var spawner in SystemAPI.Query<RefRW<Spawner>>())
        {
            ProcessSpawner(ref state, spawner);
        }
    }

    private void ProcessSpawner(ref SystemState state, RefRW<Spawner> spawner)
    {
        var nextSpawnTime = spawner.ValueRO.NextSpawnTime;
        bool canSpawn = SystemAPI.Time.ElapsedTime > nextSpawnTime;
        if (canSpawn)
        {
            // spawn entity
            var newEntity = state.EntityManager.Instantiate(spawner.ValueRO.spawnPrefab);
            
            // set position
            Unity.Transforms.LocalTransform spawnTransform = LocalTransform.FromPosition(spawner.ValueRO.SpawnPosition);
            state.EntityManager.SetComponentData(newEntity, spawnTransform);

            spawner.ValueRW.NextSpawnTime = (float)SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
        }
    }
}