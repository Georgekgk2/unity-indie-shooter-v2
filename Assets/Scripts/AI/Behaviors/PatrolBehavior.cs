using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.AI.Behaviors
{
    public class PatrolBehavior : MonoBehaviour
    {
        [Header("Patrol Settings")]
        public List<Transform> patrolPoints = new List<Transform>();
        public float waitTime = 2f;
        public bool randomOrder = false;
        public bool reverseDirection = true;
        
        [Header("Auto Generate Points")]
        public bool autoGeneratePoints = true;
        public float patrolRadius = 10f;
        public int numberOfPoints = 4;
        
        private int currentPointIndex = 0;
        private bool isWaiting = false;
        private float waitTimer = 0f;
        private bool movingForward = true;
        private Vector3 originalPosition;
        
        public Vector3 CurrentPatrolTarget
        {
            get
            {
                if (patrolPoints.Count == 0) return transform.position;
                return patrolPoints[currentPointIndex].position;
            }
        }
        
        public bool HasPatrolPoints => patrolPoints.Count > 0;
        
        void Start()
        {
            originalPosition = transform.position;
            
            if (autoGeneratePoints && patrolPoints.Count == 0)
            {
                GeneratePatrolPoints();
            }
        }
        
        void GeneratePatrolPoints()
        {
            // Clear existing points
            patrolPoints.Clear();
            
            // Create patrol points in a circle around the original position
            for (int i = 0; i < numberOfPoints; i++)
            {
                float angle = (360f / numberOfPoints) * i;
                Vector3 direction = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad),
                    0,
                    Mathf.Cos(angle * Mathf.Deg2Rad)
                );
                
                Vector3 pointPosition = originalPosition + direction * patrolRadius;
                
                // Create a new GameObject for the patrol point
                GameObject patrolPoint = new GameObject($"PatrolPoint_{i}");
                patrolPoint.transform.position = pointPosition;
                patrolPoint.transform.SetParent(transform);
                
                patrolPoints.Add(patrolPoint.transform);
            }
        }
        
        public void StartPatrol()
        {
            if (patrolPoints.Count == 0) return;
            
            isWaiting = false;
            waitTimer = 0f;
        }
        
        public void UpdatePatrol()
        {
            if (patrolPoints.Count == 0) return;
            
            if (isWaiting)
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    isWaiting = false;
                    waitTimer = 0f;
                    MoveToNextPoint();
                }
            }
        }
        
        public bool HasReachedCurrentPoint(Vector3 currentPosition, float threshold = 1f)
        {
            if (patrolPoints.Count == 0) return true;
            
            float distance = Vector3.Distance(currentPosition, CurrentPatrolTarget);
            return distance <= threshold;
        }
        
        public void OnReachedPatrolPoint()
        {
            if (waitTime > 0)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
            else
            {
                MoveToNextPoint();
            }
        }
        
        void MoveToNextPoint()
        {
            if (patrolPoints.Count <= 1) return;
            
            if (randomOrder)
            {
                // Random patrol
                int newIndex;
                do
                {
                    newIndex = Random.Range(0, patrolPoints.Count);
                } while (newIndex == currentPointIndex && patrolPoints.Count > 1);
                
                currentPointIndex = newIndex;
            }
            else if (reverseDirection)
            {
                // Ping-pong patrol
                if (movingForward)
                {
                    currentPointIndex++;
                    if (currentPointIndex >= patrolPoints.Count)
                    {
                        currentPointIndex = patrolPoints.Count - 2;
                        movingForward = false;
                    }
                }
                else
                {
                    currentPointIndex--;
                    if (currentPointIndex < 0)
                    {
                        currentPointIndex = 1;
                        movingForward = true;
                    }
                }
            }
            else
            {
                // Linear patrol (loop)
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
            }
        }
        
        public Vector3 GetRandomPatrolPoint()
        {
            if (patrolPoints.Count == 0)
            {
                // Return a random point around original position
                Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
                return originalPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            }
            
            return patrolPoints[Random.Range(0, patrolPoints.Count)].position;
        }
        
        public void AddPatrolPoint(Vector3 position)
        {
            GameObject patrolPoint = new GameObject($"PatrolPoint_{patrolPoints.Count}");
            patrolPoint.transform.position = position;
            patrolPoint.transform.SetParent(transform);
            patrolPoints.Add(patrolPoint.transform);
        }
        
        public void ClearPatrolPoints()
        {
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    DestroyImmediate(point.gameObject);
                }
            }
            patrolPoints.Clear();
        }
        
        // Debug visualization
        void OnDrawGizmosSelected()
        {
            if (patrolPoints.Count == 0) return;
            
            // Draw patrol points
            Gizmos.color = Color.blue;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
            
            // Draw patrol path
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Count; i++)
            {
                if (patrolPoints[i] == null) continue;
                
                int nextIndex = reverseDirection ? 
                    (i == patrolPoints.Count - 1 ? i - 1 : i + 1) : 
                    (i + 1) % patrolPoints.Count;
                    
                if (nextIndex >= 0 && nextIndex < patrolPoints.Count && patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }
            
            // Highlight current target
            if (currentPointIndex < patrolPoints.Count && patrolPoints[currentPointIndex] != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(patrolPoints[currentPointIndex].position, 0.8f);
            }
        }
    }
}