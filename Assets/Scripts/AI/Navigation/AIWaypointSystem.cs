using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.AI.Navigation
{
    public class AIWaypointSystem : MonoBehaviour
    {
        [Header("Waypoint Settings")]
        public List<Waypoint> waypoints = new List<Waypoint>();
        public bool showWaypointConnections = true;
        public Color waypointColor = Color.green;
        public Color connectionColor = Color.blue;
        
        [Header("Auto Generation")]
        public bool autoGenerateGrid = false;
        public Vector2 gridSize = new Vector2(10, 10);
        public float spacing = 5f;
        
        public static AIWaypointSystem Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            if (autoGenerateGrid && waypoints.Count == 0)
            {
                GenerateWaypointGrid();
            }
            
            ConnectNearbyWaypoints();
        }
        
        void GenerateWaypointGrid()
        {
            Vector3 startPosition = transform.position - new Vector3(gridSize.x * spacing * 0.5f, 0, gridSize.y * spacing * 0.5f);
            
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int z = 0; z < gridSize.y; z++)
                {
                    Vector3 position = startPosition + new Vector3(x * spacing, 0, z * spacing);
                    CreateWaypoint(position, $"Waypoint_{x}_{z}");
                }
            }
        }
        
        public Waypoint CreateWaypoint(Vector3 position, string name = "")
        {
            GameObject waypointObj = new GameObject(string.IsNullOrEmpty(name) ? $"Waypoint_{waypoints.Count}" : name);
            waypointObj.transform.position = position;
            waypointObj.transform.SetParent(transform);
            
            Waypoint waypoint = waypointObj.AddComponent<Waypoint>();
            waypoint.Initialize(waypoints.Count);
            waypoints.Add(waypoint);
            
            return waypoint;
        }
        
        void ConnectNearbyWaypoints()
        {
            float maxConnectionDistance = spacing * 1.5f;
            
            foreach (Waypoint waypoint in waypoints)
            {
                foreach (Waypoint otherWaypoint in waypoints)
                {
                    if (waypoint == otherWaypoint) continue;
                    
                    float distance = Vector3.Distance(waypoint.transform.position, otherWaypoint.transform.position);
                    if (distance <= maxConnectionDistance)
                    {
                        waypoint.AddConnection(otherWaypoint);
                    }
                }
            }
        }
        
        public Waypoint GetNearestWaypoint(Vector3 position)
        {
            Waypoint nearest = null;
            float nearestDistance = float.MaxValue;
            
            foreach (Waypoint waypoint in waypoints)
            {
                float distance = Vector3.Distance(position, waypoint.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = waypoint;
                }
            }
            
            return nearest;
        }
        
        public List<Waypoint> FindPath(Waypoint start, Waypoint end)
        {
            // Simple A* pathfinding implementation
            List<Waypoint> openSet = new List<Waypoint>();
            HashSet<Waypoint> closedSet = new HashSet<Waypoint>();
            Dictionary<Waypoint, Waypoint> cameFrom = new Dictionary<Waypoint, Waypoint>();
            Dictionary<Waypoint, float> gScore = new Dictionary<Waypoint, float>();
            Dictionary<Waypoint, float> fScore = new Dictionary<Waypoint, float>();
            
            openSet.Add(start);
            gScore[start] = 0;
            fScore[start] = Vector3.Distance(start.transform.position, end.transform.position);
            
            while (openSet.Count > 0)
            {
                Waypoint current = GetLowestFScore(openSet, fScore);
                
                if (current == end)
                {
                    return ReconstructPath(cameFrom, current);
                }
                
                openSet.Remove(current);
                closedSet.Add(current);
                
                foreach (Waypoint neighbor in current.connections)
                {
                    if (closedSet.Contains(neighbor)) continue;
                    
                    float tentativeGScore = gScore[current] + Vector3.Distance(current.transform.position, neighbor.transform.position);
                    
                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                    else if (tentativeGScore >= gScore.GetValueOrDefault(neighbor, float.MaxValue))
                    {
                        continue;
                    }
                    
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Vector3.Distance(neighbor.transform.position, end.transform.position);
                }
            }
            
            return new List<Waypoint>(); // No path found
        }
        
        Waypoint GetLowestFScore(List<Waypoint> openSet, Dictionary<Waypoint, float> fScore)
        {
            Waypoint lowest = openSet[0];
            float lowestScore = fScore.GetValueOrDefault(lowest, float.MaxValue);
            
            foreach (Waypoint waypoint in openSet)
            {
                float score = fScore.GetValueOrDefault(waypoint, float.MaxValue);
                if (score < lowestScore)
                {
                    lowest = waypoint;
                    lowestScore = score;
                }
            }
            
            return lowest;
        }
        
        List<Waypoint> ReconstructPath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint current)
        {
            List<Waypoint> path = new List<Waypoint> { current };
            
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            
            return path;
        }
        
        void OnDrawGizmos()
        {
            if (!showWaypointConnections) return;
            
            // Draw waypoints
            Gizmos.color = waypointColor;
            foreach (Waypoint waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawWireSphere(waypoint.transform.position, 0.5f);
                }
            }
            
            // Draw connections
            Gizmos.color = connectionColor;
            foreach (Waypoint waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    foreach (Waypoint connection in waypoint.connections)
                    {
                        if (connection != null)
                        {
                            Gizmos.DrawLine(waypoint.transform.position, connection.transform.position);
                        }
                    }
                }
            }
        }
    }
    
    public class Waypoint : MonoBehaviour
    {
        public int id;
        public List<Waypoint> connections = new List<Waypoint>();
        public WaypointType waypointType = WaypointType.Normal;
        
        public void Initialize(int waypointId)
        {
            id = waypointId;
        }
        
        public void AddConnection(Waypoint waypoint)
        {
            if (!connections.Contains(waypoint))
            {
                connections.Add(waypoint);
            }
        }
        
        public void RemoveConnection(Waypoint waypoint)
        {
            connections.Remove(waypoint);
        }
    }
    
    public enum WaypointType
    {
        Normal,
        Cover,
        Vantage,
        Chokepoint,
        Spawn
    }
}