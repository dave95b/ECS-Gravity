using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Gravity.ECS.Spawner
{
    public class InputPlanetSpawner : MonoBehaviour
    {
        [SerializeField]
        private PlanetSpawner spawnerData;

        [SerializeField]
        private GameObject prefab;

        private EntityManager EntityManager => World.Active?.EntityManager;
        private EntityArchetype spawnerArchetype;
        private Entity planetPrefab;


        void Start()
        {
            planetPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
            spawnerData.Prefab = planetPrefab;
            spawnerArchetype = EntityManager.CreateArchetype(ComponentType.ReadWrite<PlanetSpawner>(), ComponentType.ReadOnly<Translation>());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Spawn();
        }


        public void Spawn()
        {
            Entity spawner = EntityManager.CreateEntity(spawnerArchetype);
            EntityManager.SetComponentData(spawner, spawnerData);
            var translation = new Translation
            {
                Value = transform.position
            };
            EntityManager.SetComponentData(spawner, translation);
        }

        private void OnDestroy()
        {
            EntityManager?.DestroyEntity(planetPrefab);
        }
    }
}