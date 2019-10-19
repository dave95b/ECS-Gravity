using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;
using ECS.Utilities;

namespace Gravity.ECS
{
    public class Star : MonoBehaviour, IConvertGameObjectToEntity
    {
        [SerializeField]
        private int mass;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new Mass(mass));
            dstManager.AddComponent<StarTag>(entity);
            dstManager.AddComponent<InstantiatedTag>(entity);
        }
    }
}