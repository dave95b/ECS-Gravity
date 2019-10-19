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
    public class VelocitySystem : JobComponentSystem
    {

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new VelocityJob();

            return job.Schedule(this, inputDeps);
        }
    }

    [BurstCompile]
    struct VelocityJob : IJobForEach_CC<Translation, Velocity>
    {
        public void Execute(ref Translation translation, [ReadOnly] ref Velocity velocity)
        {
            translation.Value += velocity;
        }
    }

    [BurstCompile]
    struct VelocityChunkJob : IJobChunk
    {
        [ReadOnly]
        public float DeltaTime;

        [ReadOnly]
        public ArchetypeChunkComponentType<Velocity> VelocityType;

        public ArchetypeChunkComponentType<Translation> TranslationType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var velocities = chunk.GetNativeArray(VelocityType);
            var translations = chunk.GetNativeArray(TranslationType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var velocity = velocities[i].Value * DeltaTime;

                translations[i] = new Translation
                {
                    Value = translations[i].Value + velocity
                };
            }
        }
    }
}