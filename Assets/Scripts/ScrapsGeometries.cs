using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScrapsGeometries
{
    public class ScrapPoint
    {
        public Vector3 point;
        public int position;
        public Vector3 multiplier;

        public ScrapPoint(Vector3 point, int position)
        {
            this.point = point;
            this.position = position;

        }
    }

    public class ScrapPointsCollection
    {
        private List<ScrapPoint> _scrapPoints;
        private Vector3 _multiplier = new Vector3(100, 100, 100);

        public ScrapPointsCollection()
        {
            _scrapPoints = new List<ScrapPoint>();
        }
        
        public void AddScrap(Vector3 point, int position)
        {
            _scrapPoints.Add(new ScrapPoint(point, position));
        }

        public void RemoveLastScrap()
        {
            _scrapPoints.RemoveAt(_scrapPoints.Count - 1);
        }

        public override string ToString()
        {
            string message = "[";
            foreach (var scrapPoint in _scrapPoints)
            {
                message += (Vector3.Scale(_multiplier, scrapPoint.point)).ToString().Replace("(", "[").Replace(")", "]").Replace(" ", String.Empty) + ",";
            }
            message += "]";
            
            return message;
        }
    }
}