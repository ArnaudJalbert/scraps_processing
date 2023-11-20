using System.Collections.Generic;
using UnityEngine;

namespace ScrapsGeometries
{
    public class ScrapPoint
    {
        public Vector3 point;
        public int position;

        public ScrapPoint(Vector3 point, int position)
        {
            this.point = point;
            this.position = position;
        }
    }

    public class ScrapPointsCollection
    {
        private List<ScrapPoint> _scrapPoints;

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
            string message = "";
            foreach (var scrapPoint in _scrapPoints)
            {
                message += scrapPoint.position + ": " + scrapPoint.point + "\n";
            }

            return message;
        }
    }
}