using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.Entities;

namespace Gravity.ECS.Spawner
{
    public class InputStarSpawner : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]
        private GameObject prefab;

        private EntityManager EntityManager => World.Active?.EntityManager;

        public void OnPointerDown(PointerEventData eventData)
        {
            var position = eventData.pointerCurrentRaycast.worldPosition;
            Instantiate(prefab, position, Quaternion.identity, transform);
        }
    }
}