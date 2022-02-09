using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

[Tool]
public class LevelGenerator : Spatial
{
	#region ConfigurationVariables
	[Export(PropertyHint.Range, "1, 100")] private byte mapWidthd { get => _width; set { _width = OnlyOddNumbres(value, _width); } } //set the width of the level through inspector
	[Export(PropertyHint.Range, "1, 49")] private byte mapDepthd { get => _depth; set { _depth = OnlyOddNumbres(value, _depth); } } //set the depth of the level through inspector 
	[Export(PropertyHint.Range, "0, 1, 0.05")] private float obstacleDensity { get => _obstacleDensity; set { _obstacleDensity = value; } } //random obstacle creation factor through inspector
	[Export(PropertyHint.Range, "1, 5")] private float obstacleMaxHeight { get => _obstacleMaxHeight; set { ValidateConstraints(value, _obstacleMinHeight, true); } }
    [Export(PropertyHint.Range, "1, 5")] private float obstacleMinHeight { get => _obstacleMinHeight; set { ValidateConstraints(value, _obstacleMaxHeight, false); } }
	[Export] private uint rngSeed { get => _seed; set { _seed = value; GD.Seed(value); } }
	[Export] private Color foregroundColor { get => _foregroundColor; set { _foregroundColor = value; } }
	[Export] private Color backgroundColor { get => _backgroundColor; set { _backgroundColor = value; } }
	[Export] private bool generateLevel { get => false; set => GenerateMap(); }
	//[Export] private bool soroundLevel { get => false; set => Sorround(); }
	[Export] private bool saveLevel { get => false; set => SaveLevel(); }
	[Export] private string savedLevelName { get; set; }
	[Export] private Shader obstaclesShader { get; set; }
	[Export] private SpatialMaterial groundMaterial { get; set; }
	//[Export] private PackedScene groundScene { get; set; } //ground scene
	//[Export] private PackedScene obstacleScene { get; set; } //obstacle scene
	#endregion

	#region InternalVariables
	private byte _width = 11; //stores the width of the level
	private byte _depth = 11; //stores the depth of the level
	private float _obstacleDensity = 0.2f; //stores the random obstacle cration factor
	private uint _seed = 0u; //stores a random seed
	private float _obstacleMaxHeight = 5f;
	private float _obstacleMinHeight = 1f;
	private Color _foregroundColor = Color.Color8(255, 255, 255);
	private Color _backgroundColor = Color.Color8(0, 0 , 0);
	private CSGBox _obstacle;
	private CSGBox _ground;
	//private List<> _mapCoords; //stores the obstacle coordinates
	private Pathfinder _pathfinder;
	private List<Waypoint> _waypoints = new List<Waypoint>();
	private NavigationMeshInstance _level;
	#endregion

	private Stopwatch sw = new Stopwatch();

	public override void _Ready()
	{
		GenerateMap(); //generate map in the begining
	}
	private void ValidateConstraints(float value, float oValue, bool greaterFirst)
    {
		if (greaterFirst)
		{
			_obstacleMaxHeight = Mathf.Max(value, oValue);
		}
		else
		{
			_obstacleMinHeight = Mathf.Min(value, oValue);
		}
    }
	private void GenerateMap()
	{
		foreach (Node n in GetChildren()) { n.Free(); } //delete all the previous obstacles
		_waypoints.Clear();

		

		GenerateBaseObstacle();
		GenerateBaseGround();
		AddRootLevelNode();

		FillCoordsList();
		AddObstacles(); 
		AddGround();

		var center = from c in _waypoints
					where c.Position == Vector2Int.Zero
					select c;

		_pathfinder = new Pathfinder(_waypoints);
		GD.PushWarning("The waypoints list contains: " + _waypoints.Count);
		_pathfinder.OpenPaths(center.First());

		Sorround();
	}

	private void Sorround()
	{
		var soroundoffsetWidth = _width + 1;
		var soroundoffsetDepth = _depth + 1;


		for (int i = -soroundoffsetWidth + 2; i < soroundoffsetWidth; i+=2)
		{
			CreateObstacle(i, soroundoffsetDepth);
			CreateObstacle(i, -soroundoffsetDepth);
		}

		for (int i = -soroundoffsetDepth + 2; i < soroundoffsetDepth ; i+=2)
		{
			CreateObstacle(soroundoffsetWidth, i);
			CreateObstacle(-soroundoffsetWidth, i);
		}
	}

	private void SaveLevel()
	{
		var packedScene = new PackedScene();
		var root = GetNode<Navigation>("Navigation");

		_level.Owner = root;
	
		foreach (var child in _level.GetChildren())
		{
			(child as Node).Owner = root;
		}

		packedScene.Pack(root);
		string savePath = "res://Assets/Scenes/GeneratedLevels/" + savedLevelName + ".tscn";
		ResourceSaver.Save(savePath, packedScene);
	}

	private void AddRootLevelNode()
	{
		var root = new Navigation();
		root.Name = "Navigation";

		_level = new NavigationMeshInstance();
		_level.Name = "NavigationMeshInstance";

		_level.Navmesh = new NavigationMesh();

		root.AddChild(_level);
		
		AddChild(root);
		_level.Owner = this;
		root.Owner = this;
	}

	private void GenerateBaseGround()
	{
		//if (_ground != null) return;

		_ground = new CSGBox();
		_ground.Height = 1.5f;
		_ground.Material = groundMaterial;
		_ground.Translation = new Vector3(0, -0.75f, 0);
		_ground.UseCollision = true;
	}

	private void GenerateBaseObstacle()
	{
		//if (_obstacle != null) return;

		_obstacle = new CSGBox();
		_obstacle.Width = 2;
		_obstacle.Depth = 2;
		var mat = new ShaderMaterial();
		mat.Shader = obstaclesShader;
		_obstacle.Material = mat;
		_obstacle.UseCollision = true;
	}

	private void FillCoordsList()
	{
		for (int i = 0; i < _width; i++)
		{
			for (int j = 0; j < _depth; j++)
			{
				Vector2Int obsPos = new Vector2Int(i * 2, j * 2);
				Vector2Int offset = new Vector2Int(_width - 1, _depth - 1);

				Vector2Int fOffset = obsPos - offset;


				_waypoints.Add(new Waypoint(new Vector2Int(fOffset.x, fOffset.y)));
			}
		}
	}

	private void AddGround()
	{  
		var ground = _ground.Duplicate(8) as CSGBox;
		ground.Width = _width * 2;
		ground.Depth = _depth * 2;

		_level.AddChild(ground);
		ground.Owner = this;
	}

	private void AddObstacles() //Add a dictionary?, because nested for-loops are squared complexity
	{
		uint it = 0;
		sw.Start();
		
		foreach (var tuple in SelectRandomObstacleSeed())
		{
			GD.Print($"Call { it } started	:	{ sw.Elapsed.TotalMilliseconds } ms");
			if (tuple.Position == Vector2Int.Zero) continue;

			tuple.Obstacle = CreateObstacle(tuple.Position.x, tuple.Position.y);

			GD.Print($"Call { it } completed	:	{ sw.Elapsed.TotalMilliseconds } ms");
			it++;
		}
		sw.Stop();
	}

	private IEnumerable<Waypoint> SelectRandomObstacleSeed()
	{
		return _waypoints.OrderBy(a => GD.Randi().ToString()).Take((ushort)(_waypoints.Count * _obstacleDensity));
	}

	private CSGBox CreateObstacle(int x, int z)
	{
		//CSGBox newObstacle = obstacleScene.Instance<CSGBox>();
		CSGBox newObstacle = _obstacle.Duplicate(8) as CSGBox;
		
		newObstacle.Height = (float)GD.RandRange(_obstacleMinHeight, _obstacleMaxHeight);
		newObstacle.Translation = new Vector3(x, newObstacle.Height / 2, z);

		//New material and set it's color
		ShaderMaterial mat = newObstacle.Material as ShaderMaterial;
		mat.SetShaderParam("background", _backgroundColor);
		mat.SetShaderParam("foreground", _foregroundColor);
		mat.SetShaderParam("level_depth", _depth * 2);
		
		_level.AddChild(newObstacle);
		newObstacle.Owner = this;
		return newObstacle;
	}

    private byte OnlyOddNumbres(byte parameterValue, byte localDimention)
	{   
		if (parameterValue % 2 != 1)
		{
			if (parameterValue > localDimention)
			{
				return checked( ++parameterValue );
			}
			else 
			{
				return checked( --parameterValue );
			}
		}
		else
		{
			return parameterValue;
		}
	}
} 