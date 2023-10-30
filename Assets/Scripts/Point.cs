using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace ScrapsGeometries
{
    public class ScrapPoint
    {
        public GameObject sphere;
        private int _orderPosition;
        private ScrapPoint _previousPoint;
        private ScrapPoint _nextPoint;
        private ARRaycastHit _hitPoint;
        private const float _sphereScale = 0.1f;

        public ScrapPoint(int orderPosition, ScrapPoint previousPoint, ARRaycastHit hitPoint)
        {
            _orderPosition = orderPosition;
            _previousPoint = previousPoint;
            _hitPoint = hitPoint;
            sphere = CreateSphere();
        }

        private GameObject CreateSphere()
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = new Vector3(_sphereScale, _sphereScale, _sphereScale);
            return sphere;
        }
    }
}