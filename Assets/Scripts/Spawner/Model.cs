using Unity.Collections;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

namespace Gravity.ECS.Spawner.Model
{
    readonly struct PlanetSpawnerSystemData
    {
        public readonly NativeArray<Entity> Entities;
        public readonly NativeArray<PlanetSpawner> Spawners;
        public readonly NativeArray<Translation> Positions;

        public PlanetSpawnerSystemData(NativeArray<Entity> entities, NativeArray<PlanetSpawner> spawners, NativeArray<Translation> positions)
        {
            Entities = entities;
            Spawners = spawners;
            Positions = positions;
        }

        public int GetSpawnCount(int index)
        {
            var spawner = Spawners[index];
            return spawner.PerFrameCount > spawner.Count ? spawner.Count : spawner.PerFrameCount;
        }

        public void Dispose()
        {
            Entities.Dispose();
            Spawners.Dispose();
            Positions.Dispose();
        }
    }

    readonly struct SpawnData
    {
        public readonly Entity Entity;
        public readonly PlanetSpawner Spawner;
        public readonly float3 Position;

        public readonly NativeArray<Translation> Translations;
        public readonly NativeArray<Mass> Masses;
        public readonly NativeArray<Velocity> Velocities;

        public int SpawnCount => Translations.Length;

        public SpawnData(Entity entity, PlanetSpawner spawner, float3 position,
            NativeArray<Translation> translations, NativeArray<Mass> masses, NativeArray<Velocity> velocities)
        {
            Entity = entity;
            Spawner = spawner;
            Position = position;
            Translations = translations;
            Masses = masses;
            Velocities = velocities;
        }

        public void Dispose()
        {
            Translations.Dispose();
            Masses.Dispose();
            Velocities.Dispose();
        }
    }
}