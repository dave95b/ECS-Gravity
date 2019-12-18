using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

using Random = Unity.Mathematics.Random;

namespace Gravity.ECS.Spawner
{
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