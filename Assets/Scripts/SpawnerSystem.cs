using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;

// spawner with jobs
[BurstCompile]
public partial struct SpawnerSystemOptimized : ISystem
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
        EntityCommandBuffer.ParallelWriter commandBuffer = GetEntityCommandBuffer(ref state);
        new ProcessSpawnerJob
        {
            elapsedTime = SystemAPI.Time.ElapsedTime,
            Ecb = commandBuffer
        }.ScheduleParallel();
    }

    private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        return ecb.AsParallelWriter();
    }
}

[BurstCompile]
public partial struct ProcessSpawnerJob : IJobEntity
{
    public EntityCommandBuffer.ParallelWriter Ecb;
    public double elapsedTime;

    private void Execute([ChunkIndexInQuery] int chuckIndex, ref Spawner spawner)
    {
        bool canSpawn = elapsedTime > spawner.NextSpawnTime;
        
        if (canSpawn)
        {
            // spawn entity
            var newEntity = Ecb.Instantiate(chuckIndex, spawner.spawnPrefab);

            var transform = LocalTransform.FromPosition(spawner.SpawnPosition);
            Ecb.SetComponent(chuckIndex, newEntity, transform);
            spawner.NextSpawnTime = (float) elapsedTime + spawner.SpawnRate;
        }
    }
}


/*
// spawner system without jobs
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

            spawner.ValueRW.NextSpawnTime = (float) SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
        }
    }
}*/