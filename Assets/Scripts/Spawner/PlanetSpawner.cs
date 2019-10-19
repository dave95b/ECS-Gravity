using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;

namespace Gravity.ECS.Spawner
{
    [System.Serializable]
    public struct PlanetSpawner : IComponentData
    {
        public Entity Prefab;
        public float Radius, Speed;
        public int PerFrameCount, Count, MinMass, MaxMass;
        public bool DestroyPrefab;
    }

    namespace Proxy
    {
        public class PlanetSpawner : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
        {
            [SerializeField]
            private GameObject prefab;

            [SerializeField]
            private float radius, speed;

            [SerializeField]
            private int perFrameCount, count, minMass, maxMass;

            public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
            {
                var spawner = new Spawner.PlanetSpawner
                {
                    Prefab = conversionSystem.GetPrimaryEntity(prefab),
                    Radius = radius,
                    Speed = speed,
                    MinMass = minMass,
                    MaxMass = maxMass,
                    PerFrameCount = perFrameCount,
                    Count = count,
                    DestroyPrefab = true
                };
                dstManager.AddComponentData(entity, spawner);
            }

            public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
            {
                referencedPrefabs.Add(prefab);
            }
        }
    }
}