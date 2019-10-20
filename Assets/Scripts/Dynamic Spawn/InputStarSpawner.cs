using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using ECS.Utilities;

namespace Gravity.ECS.Spawner
{
    public class InputStarSpawner : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private GameObject prefab;

        private EntityManager EntityManager => World.Active?.EntityManager;
        private Entity starPrefab;

        void Start()
        {
            starPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
            EntityManager.RemoveComponent(starPrefab, ComponentType.ReadOnly<NonUniformScale>());
            EntityManager.RemoveComponent(starPrefab, ComponentType.ReadOnly<Rotation>());
            EntityManager.RemoveComponent(starPrefab, ComponentType.ReadOnly<Translation>());
            EntityManager.SetName(starPrefab, "Spawner star prefab");
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var position = eventData.pointerCurrentRaycast.worldPosition;
            var translation = new LocalToWorld
            {
                Value = float4x4.TRS(position, quaternion.identity, prefab.transform.localScale)
            };
            var created = EntityManager.Instantiate(starPrefab);
            EntityManager.SetComponentData(created, translation);
        }
    }
}