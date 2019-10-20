using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;

namespace Gravity.ECS
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(VelocitySystem))]
    public class GravitySystem : JobComponentSystem
    {
        private EntityQuery planetsQuery, starsQuery;

        float lastSimulationTime;

        protected override void OnCreate()
        {
            base.OnCreate();

            planetsQuery = GetEntityQuery(ComponentType.ReadWrite<Velocity>(), ComponentType.ReadOnly<Mass>(), ComponentType.ReadOnly<Translation>());
            starsQuery = GetEntityQuery(ComponentType.ReadOnly<Mass>(), ComponentType.ReadOnly<LocalToWorld>(), ComponentType.ReadOnly<StarTag>());
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //float currentTime = Time.realtimeSinceStartup;
            //float difference = currentTime - lastSimulationTime;

            //float deltaTime = Time.fixedDeltaTime;
            //int simulationCount = (int)(difference / deltaTime);

            //for (int i = 0; i < simulationCount; i++)
            //{
            //    var job = new GravityJob
            //    {
            //        DeltaTime = Time.fixedDeltaTime,
            //        StarsPositions = starsPositions,
            //        StarsMasses = starsMasses
            //    };
            //}

            //lastSimulationTime += simulationCount * deltaTime;

            NativeArray<LocalToWorld> starsPositions = starsQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            NativeArray<Mass> starsMasses = starsQuery.ToComponentDataArray<Mass>(Allocator.TempJob);

            var job = new GravityJob
            {
                DeltaTime = Time.fixedDeltaTime,
                StarsPositions = starsPositions,
                StarsMasses = starsMasses,
                VelocityType = GetArchetypeChunkComponentType<Velocity>(isReadOnly: false),
                MassType = GetArchetypeChunkComponentType<Mass>(isReadOnly: true),
                TranslationType = GetArchetypeChunkComponentType<Translation>(isReadOnly: true)
            };

            return job.Schedule(planetsQuery, inputDeps);
        }
    }

    [BurstCompile(FloatPrecision.Low, FloatMode.Fast)]
    struct GravityJob : IJobChunk
    {
        [ReadOnly]
        public float DeltaTime;

        [ReadOnly, DeallocateOnJobCompletion]
        public NativeArray<LocalToWorld> StarsPositions;
        [ReadOnly, DeallocateOnJobCompletion]
        public NativeArray<Mass> StarsMasses;

        public ArchetypeChunkComponentType<Velocity> VelocityType;
        [ReadOnly]
        public ArchetypeChunkComponentType<Mass> MassType;
        [ReadOnly]
        public ArchetypeChunkComponentType<Translation> TranslationType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var planetVelocities = chunk.GetNativeArray(VelocityType);
            var planetMasses = chunk.GetNativeArray(MassType);
            var planetPositions = chunk.GetNativeArray(TranslationType);

            int chunkCount = chunk.Count;
            for (int i = 0; i < chunkCount; i++)
            {
                float3 force = float3.zero;
                float3 planetPosition = planetPositions[i].Value;
                int planetMass = planetMasses[i];

                for (int j = 0; j < StarsPositions.Length; j++)
                {
                    float3 direction = StarsPositions[j].Position - planetPosition;
                    float distance = math.length(direction);
                    direction = math.normalize(direction);

                    force += (StarsMasses[j] * planetMass) / (distance * distance) * direction;
                }

                float3 velocity = planetVelocities[i] + force * DeltaTime;
                planetVelocities[i] = new Velocity(velocity);
            }
        }
    }
}