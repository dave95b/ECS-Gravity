using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Entities;
using Gravity.ECS;
using Unity.Transforms;

namespace ECS.Utilities
{
    public class DataPrinter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI planetsText, starsText;

        private EntityQuery planetsQuery, starsQuery;
        private int planetCount, starCount;

        private EntityManager EntityManager => World.Active?.EntityManager;
        private int PlanetCount => planetsQuery.CalculateEntityCountWithoutFiltering();
        private int StarCount => starsQuery.CalculateEntityCountWithoutFiltering();


        private void Start()
        {
            Application.targetFrameRate = 600;
            planetsQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Velocity>(), ComponentType.ReadOnly<Mass>(), ComponentType.ReadOnly<Translation>());
            starsQuery = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<StarTag>());

            planetCount = PlanetCount;
            starCount = StarCount;

            planetsText.text = $"Planets: {planetCount}";
            starsText.text = $"Stars: {starCount}";
        }

        private void Update()
        {
            int _planetCount = PlanetCount;
            if (_planetCount != planetCount)
            {
                planetCount = _planetCount;
                planetsText.text = $"Planets: {planetCount}";
            }

            int _starCount = StarCount;
            if (_starCount != starCount)
            {
                starCount = _starCount;
                starsText.text = $"Stars: {starCount}";
            }
        }
    }
}