using UnityEngine;
using System.Collections;
using Unity.Entities;
using ECS.Utilities;

namespace Gravity.ECS
{
    public class Planet : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Mass>(entity);
            dstManager.AddComponent<Velocity>(entity);
            dstManager.AddComponent<InstantiatedTag>(entity);
        }
    }
}