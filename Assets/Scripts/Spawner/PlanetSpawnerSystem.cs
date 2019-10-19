using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using ECS.Utilities;

using Random = Unity.Mathematics.Random;
using UnityEngine.Profiling;

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
            Profiler.BeginSample("Get Spawner Entities");
            var spawnerEntities = spawnerQuery.ToEntityArray(Allocator.TempJob, out JobHandle entitiesHandle);
            var spawners = spawnerQuery.ToComponentDataArray<PlanetSpawner>(Allocator.TempJob, out JobHandle spawnerHandle);
            var spawnerPositions = spawnerQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out JobHandle positionsHandle);
            JobHandle.CompleteAll(ref entitiesHandle, ref spawnerHandle, ref positionsHandle);
            Profiler.EndSample();

            float deltaTime = Time.fixedDeltaTime;

            Profiler.BeginSample("Spawn");
            for (int i = 0; i < spawners.Length; i++)
            {
                var spawner = spawners[i];
                int spawnCount = spawner.PerFrameCount > spawner.Count ? spawner.Count : spawner.PerFrameCount;

                Profiler.BeginSample("Allocate Data Arrays");
                var planetTranslations = new NativeArray<Translation>(spawnCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var planetMasses = new NativeArray<Mass>(spawnCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                var planetVelocities = new NativeArray<Velocity>(spawnCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
                Profiler.EndSample();

                Profiler.BeginSample("Schedule Data Job");
                uint seed = (uint)UnityEngine.Random.Range(1, 500000);

                var dataJob = new PlanetDataJob
                {
                    Random = new Random(seed),
                    DeltaTime = deltaTime,
                    Spawner = spawner,
                    SpawnerPosition = spawnerPositions[i].Value,
                    PlanetPositions = planetTranslations,
                    PlanetMasses = planetMasses,
                    PlanetVelocities = planetVelocities
                };

                var handle = dataJob.Schedule(spawnCount, 128);
                Profiler.EndSample();

                Profiler.BeginSample("Instantiate Entities");
                var spawnerQuery = entitySpawner.Spawn(spawner.Prefab, spawnCount, Allocator.TempJob);
                Profiler.EndSample();

                Profiler.BeginSample("Complete Data Job");
                handle.Complete();
                Profiler.EndSample();

                Profiler.BeginSample("Set Data");
                NativeArray<Entity> spawnedEntities = spawnerQuery.SetData(planetVelocities)
                    .SetData(planetTranslations)
                    .SetData(planetMasses)
                    .Complete();
                Profiler.EndSample();

                Profiler.BeginSample("Cleanup");
                spawner.Count -= spawnCount;
                if (spawner.Count == 0)
                {
                    EntityManager.DestroyEntity(spawnerEntities[i]);
                    if (spawner.DestroyPrefab)
                        EntityManager.DestroyEntity(spawner.Prefab);
                }
                else
                    EntityManager.SetComponentData(spawnerEntities[i], spawner);


                spawnedEntities.Dispose();
                planetTranslations.Dispose();
                planetMasses.Dispose();
                planetVelocities.Dispose();
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Profiler.BeginSample("Dispose Spawner Data");
            spawners.Dispose();
            spawnerEntities.Dispose();
            spawnerPositions.Dispose();
            Profiler.EndSample();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            entitySpawner.Dispose();
        }
    }

    [BurstCompile]
    struct PlanetDataJob : IJobParallelFor
    {
        public Random Random;

        [ReadOnly]
        public float DeltaTime;
        [ReadOnly]
        public PlanetSpawner Spawner;
        [ReadOnly]
        public float3 SpawnerPosition;

        [WriteOnly]
        public NativeArray<Translation> PlanetPositions;
        [WriteOnly]
        public NativeArray<Mass> PlanetMasses;
        [WriteOnly]
        public NativeArray<Velocity> PlanetVelocities;

        public void Execute(int index)
        {
            int mass = Random.NextInt(Spawner.MinMass, Spawner.MaxMass);
            PlanetMasses[index] = new Mass(mass);

            float3 position = SpawnerPosition + RandomOnUnitSphere() * Spawner.Radius * Random.NextFloat(0.9f, 1.1f);
            float3 vector = Random.NextFloat3();
            float3 velocity = math.normalize(math.cross(position, vector)) * Spawner.Speed * Random.NextFloat(0.9f, 1.1f) * DeltaTime;
            velocity *= math.select(1f, -1f, Random.NextBool());

            PlanetVelocities[index] = new Velocity(velocity);
            PlanetPositions[index] = new Translation
            {
                Value = position
            };
        }

        private float3 RandomOnUnitSphere()
        {
            return math.normalize(Random.NextFloat3(new float3(-1f, -1f, -1f), new float3(1f, 1f, 1f)));
        }
    }
}