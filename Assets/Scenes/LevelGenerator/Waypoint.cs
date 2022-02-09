using Godot;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Waypoint
{
    public bool IsExplored { get; set; }
    public bool IsBlocked => Obstacle != null; 
    public Waypoint ExploredFrom { get; set; }
    public List<Waypoint> Neighbours { get; set; } = new List<Waypoint>();
    public Vector2Int Position { get; private set; }
    public CSGBox Obstacle { get; set; }

    public Waypoint(Vector2Int position)
    {
        Position = position;
    }

    public override string ToString()
    {
        return string.Format($"[{ Position.x }, { Position.y }]");
    }
}

public class Pathfinder
{
    private List<Waypoint> _waypoints = new List<Waypoint>();
    private Queue<Waypoint> _queue = new Queue<Waypoint>();
    //private List<Waypoint> _path = new List<Waypoint>();
    private Waypoint _startWaypoint;
    private bool _isRunning = true;
    
    private readonly Vector2Int[] _directions = 
    {
        Vector2Int.Up,
        Vector2Int.Right,
        Vector2Int.Down,
        Vector2Int.Left
    };

    public Pathfinder(List<Waypoint> waypoints)
    {
        _waypoints = waypoints;
    }

    public void OpenPaths(Waypoint start)
    {
        _startWaypoint = start;
        CalculatePath();
    }

    private void CalculatePath()
    {
        LoadBlocks();
        BreadthFirstSearch();

        RemoveIsolatedZones();
    }

    private List<Waypoint> MakeDeletionPath(Waypoint origin, Waypoint target)
    {
        List<Waypoint> path = new List<Waypoint>();

        GD.Print(target.ExploredFrom);
        path.Add(target.ExploredFrom);
        
        while (path.Last().ExploredFrom != null && path.Last().ExploredFrom != origin)
        {
            path.Add(path.Last().ExploredFrom);
        }
        return path;
    }

    private void RemoveIsolatedZones()
    {
        IEnumerable<Waypoint> isolatedWaypoints = from way in _waypoints
                                                    where !way.IsExplored
                                                    where !way.IsBlocked
                                                    select way;

        IEnumerable<Waypoint> exploredWaypoints = from dway in _waypoints
                                                    where !dway.IsBlocked
                                                    where dway.IsExplored
                                                    select dway;

        while (isolatedWaypoints.Count() > 0)
        {
            GD.PrintT(isolatedWaypoints.Count(), "isolated waypoints");
            Waypoint centerClosest = isolatedWaypoints.First();
            Waypoint discoveredTarget = isolatedWaypoints.First();
            float distance = float.MaxValue;

            _isRunning = true;

            foreach (Waypoint w in isolatedWaypoints)
            {
                centerClosest = w.Position.LenghtSquared() < centerClosest.Position.LenghtSquared() ? w : centerClosest;
            }

            foreach (Waypoint wa in exploredWaypoints)
            {
                float tmpDis = (wa.Position - centerClosest.Position).LenghtSquared();
                
                if (tmpDis < distance)
                {
                    discoveredTarget = wa;
                    distance = tmpDis;
                }
            }

            GD.Print("      ", centerClosest, "     origin from deletion path");
            GD.Print("      ", discoveredTarget, "       target from deletion path");

            Parallel.ForEach(_waypoints, w => w.ExploredFrom = null);
            Parallel.ForEach(_waypoints, w => w.IsExplored = false);

            BreadthFirstSearch(centerClosest, discoveredTarget);
            
            foreach (Waypoint wp in MakeDeletionPath(centerClosest, discoveredTarget))
            {
                
                if(wp.IsBlocked)
                {
                    GD.Print("deletion: ", wp);
                    wp.Obstacle.Free();
                    wp.Obstacle = null;
                }
                
            }

            Parallel.ForEach(_waypoints, w => w.IsExplored = false);
            BreadthFirstSearch();

            GD.Print(isolatedWaypoints.Count());
        }
    }

    private void BreadthFirstSearch(Waypoint start, Waypoint end)
    {
        _queue.Clear();
        _queue.Enqueue(start);

        while (_queue.Count > 0 && _isRunning)
        {
            var searchCenter = _queue.Dequeue();
            GD.PrintT("Searching from:", searchCenter);
            
            Explore(searchCenter, end, out bool found);
            if (found) break;
            searchCenter.IsExplored = true;
        }
        _queue.Clear();
    }

    private bool VerifyEnd(Waypoint searchCenter, Waypoint end)
    {
        if (searchCenter.Position == end.Position)
        {
            _isRunning = false;
            GD.PrintS("End Explored from:", end.ExploredFrom);
            return true;
        }
        return false;
    }

    private void Explore(Waypoint from, Waypoint end, out bool found)
    {
        foreach (Waypoint neighbour in from.Neighbours)
        { 
            GD.PrintS("     ", neighbour, "reached.");
            if (neighbour.IsExplored/* || !neighbour.IsBlocked*/ || _queue.Contains(neighbour)) continue;
            
            _queue.Enqueue(neighbour);
            neighbour.ExploredFrom = from;
            GD.PrintS("     ", neighbour, "explored.");

            if (VerifyEnd(neighbour, end)) 
            {
                found = true;
                return;
            }

        }
        found = false;
    }

    private void BreadthFirstSearch()
    {
        _queue.Enqueue(_startWaypoint);

        while (_queue.Count > 0)
        {
            var searchCenter = _queue.Dequeue();
            ExploreNeighbours(searchCenter);
            searchCenter.IsExplored = true;
        }
    }

    private void ExploreNeighbours(Waypoint center)
    {
        foreach (var neighbour in center.Neighbours)
        {
            if (neighbour.IsExplored || neighbour.IsBlocked || _queue.Contains(neighbour)) continue;

            _queue.Enqueue(neighbour);
        }
    }

    private void ConnectWaypoints(Waypoint center)
    {
        var w = from way in _waypoints
                where way.Position == center.Position + Vector2Int.Up * 2
                || way.Position == center.Position + Vector2Int.Right * 2
                || way.Position == center.Position + Vector2Int.Down * 2
                || way.Position == center.Position + Vector2Int.Left * 2
                select way;

        
        foreach (Waypoint way in w)
        {   
            center.Neighbours.Add(way);
        }
    }

    private void LoadBlocks()
    {
        foreach (Waypoint w in _waypoints)
        {
            ConnectWaypoints(w);
        }
    }
}