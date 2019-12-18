using ECS.Utilities;
using Gravity.ECS.Spawner.Model;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

using Random = Unity.Mathematics.Random;

namespace Gravity.ECS.Spawner
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class PlanetSpawnerSystem : ComponentSystem
    {
        private EntityQuery spawnerQuery;

        private EntitySpawner entitySpawner;

        protected override void OnCreate()
        {
            base.OnCreate();
            spawnerQuery = GetEntityQuery(ComponentType.ReadWrite<PlanetSpawner>(), ComponentType.ReadOnly<Translation>());
            entitySpawner = new EntitySpawner(this);
        }

        protected override void OnUpdate()
        {
            var systemData = CreateSystemData();
            float deltaTime = Time.fixedDeltaTime;

            for (int i = 0; i < systemData.Spawners.Length; i++)
            {
                var spawnData = CreateSpawnData(systemData, i);
                Spawn(spawnData, deltaTime);
                spawnData.Dispose();
            }

            systemData.Dispose();
        }

        private void Spawn(in SpawnData data, float deltaTime)
        {
            var handle = ScheduleDataJob(data, deltaTime, data.Spawner);
            var spawnerQuery = entitySpawner.Spawn(data.Spawner.Prefab, data.SpawnCount, Allocator.TempJob);
            handle.Complete();

            NativeArray<Entity> spawnedEntities = spawnerQuery
                .SetData(data.Velocities)
                .SetData(data.Translations)
                .SetData(data.Masses)
                .Complete();

            UpdateOrDestroySpawner(data);

            spawnedEntities.Dispose();
        }

        private PlanetSpawnerSystemData CreateSystemData()
        {
            var entities = spawnerQuery.ToEntityArray(Allocator.TempJob, out JobHandle entitiesHandle);
            var spawners = spawnerQuery.ToComponentDataArray<PlanetSpawner>(Allocator.TempJob, out JobHandle spawnerHandle);
            var positions = spawnerQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle positionsHandle);
            JobHandle.CompleteAll(ref entitiesHandle, ref spawnerHandle, ref positionsHandle);

            return new PlanetSpawnerSystemData(entities, spawners, positions);
        }

        private SpawnData CreateSpawnData(in PlanetSpawnerSystemData data, int index)
        {
            var entity = data.Entities[index];
            var spawner = data.Spawners[index];
            var position = data.Positions[index].Value;
            int spawnCount = spawner.PerFrameCount > spawner.Count ? spawner.Count : spawner.PerFrameCount;

            var planetTranslations = new NativeArray<Translation>(spawnCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var planetMasses = new NativeArray<Mass>(spawnCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            var planetVelocities = new NativeArray<Velocity>(spawnCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            return new SpawnData(entity, spawner, position, planetTranslations, planetMasses, planetVelocities);
        }
        
        private JobHandle ScheduleDataJob(in SpawnData data, float deltaTime, PlanetSpawner spawner)
        {
            uint seed = (uint)UnityEngine.Random.Range(1, 500000);

            var dataJob = new PlanetDataJob
            {
                Random = new Random(seed),
                DeltaTime = deltaTime,
                Spawner = spawner,
                SpawnerPosition = data.Position,
                PlanetPositions = data.Translations,
                PlanetMasses = data.Masses,
                PlanetVelocities = data.Velocities
            };

            return dataJob.Schedule(data.SpawnCount, 128);
        }

        private void UpdateOrDestroySpawner(in SpawnData data)
        {
            var spawner = data.Spawner;
            spawner.Count -= data.SpawnCount;

            if (spawner.Count > 0)
                EntityManager.SetComponentData(data.Entity, spawner);
            else
            {
                if (spawner.DestroyPrefab)
                    EntityManager.DestroyEntity(spawner.Prefab);

                EntityManager.DestroyEntity(data.Entity);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            entitySpawner.Dispose();
        }
    }
}