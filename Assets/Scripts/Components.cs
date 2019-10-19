using UnityEngine;
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Gravity.ECS
{
    [Serializable]
    public struct Mass : IComponentData
    {
        public int Value;

        public Mass(int value)
        {
            Value = value;
        }

        public static implicit operator int(in Mass mass) => mass.Value;
    }

    [Serializable]
    public struct Velocity : IComponentData
    {
        public float3 Value;

        public Velocity(in float3 value)
        {
            Value = value;
        }
        
        public static implicit operator float3(in Velocity velocity) => velocity.Value;
    }

    public struct StarTag : IComponentData { }
}