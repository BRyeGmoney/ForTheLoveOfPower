using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gamelogic.Grids.Examples
{
	/**
		A demo showing how a wrapped flat hex map can be mapped to a 
		hemisphere, that supports rotation so that it gives the illusion 
		that it is a full sphere.
	*/
	public class SphereIllusion : MonoBehaviour
	{
		#region Constants

		private static readonly float Sqrt3 = Mathf.Sqrt(3);
		private static readonly float SqrSphereRadius = Sqr(600*2);
		private const int GridWidth = 35*2;
		private const int GridHeight = 40*2;

		private const float CellWidth = 69;
		private const float CellHeight = 80;
		private const float CellHeightAndAHalf = CellHeight*1.5f;
		private const float ScrollSpeed = 100;

		private static readonly Vector3[] VertexDirections =
		{
			new Vector3(0, 0, 1f/2),
			new Vector3(Sqrt3/4, 0, 1f/4),
			new Vector3(Sqrt3/4, 0, -1f/4),
			new Vector3(0, 0, -1f/2),
			new Vector3(-Sqrt3/4, 0, -1f/4),
			new Vector3(-Sqrt3/4, 0, 1f/4),
			Vector3.zero
		};

		private static readonly Vector2[] UVDirections =
			VertexDirections.Select(v => new Vector2(v.x, v.z) + Vector2.one*0.5f).ToArray();

		private static readonly int[] Triangles =
		{
			6, 0, 1,
			6, 1, 2,
			6, 2, 3,
			6, 3, 4,
			6, 4, 5,
			6, 5, 0
		};

		private static readonly Vector3[] Normals =
		{
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up,
			Vector3.up
		};

		//Tweakables
		//Assumes the texture you have on you renderer is divided 
		//in 5x4 rectangles. A texture like this: http://www.andrethiel.de/ContentImages/2D/Fullsize/Textures_Terrain.jpg
		private const int TextureGridWidth = 4;
		private const int TextureGridHeight = 3;

		private const float TextureCellWidth = 1f/TextureGridWidth;
		private const float TextureCellHeight = 1f/TextureGridHeight;

		#endregion

		#region Public Fields

		public GameObject plane;

		public Camera topCamera;
		public Camera sideCamera;

		#endregion

		#region Private Fields

		private float offsetX;
		private float offsetY;
		private MeshFilter mesh;
		private WrappedGrid<int, PointyHexPoint> grid;
		private Vector2 dimensions;
		private IMap3D<PointyHexPoint> map;
		private PointyHexPoint gridOffset;
		private bool planeActive;
		private bool topCameraActive;

		#endregion

		#region Menu commands

		[ContextMenu("Generate Mesh")]
		public void GenerateMesh()
		{
			var grid = PointyHexGrid<int>.WrappedParallelogram(GridWidth, GridHeight);

			foreach (var point in grid)
			{
				grid[point] = point.GetColor(3, 1, 4); //Random.Range(0, textureCount);
			}

			var dimensions = new Vector2(CellWidth, 80);

			var map = new PointyHexMap(dimensions)
				.WithWindow(new Rect(0, 0, 0, 0))
				.AlignMiddleCenter(grid)
				.To3DXZ();

			var mesh = new Mesh();

			GetComponent<MeshFilter>().mesh = mesh;
			GenerateMesh(mesh, grid, map, dimensions);

			Debug.Log("Done");
		}

		#endregion

		#region Unity Callbacks

		public void Awake()
		{
			planeActive = true;
			topCameraActive = true;
			topCamera.gameObject.SetActive(true);

			gridOffset = PointyHexPoint.Zero;
			mesh = GetComponent<MeshFilter>();
			grid = PointyHexGrid<int>.WrappedParallelogram(GridWidth, GridHeight);

			foreach (var point in grid)
			{
				grid[point] = point.GetColor(3, 1, 4); //Random.Range(0, textureCount);
			}

			dimensions = new Vector2(CellWidth, CellHeight);

			map = new PointyHexMap(dimensions)
				.WithWindow(new Rect(0, 0, 0, 0))
				.AlignMiddleCenter(grid)
				.To3DXZ();
		}

		public void Update()
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				ScrollLeft();
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				ScrollRight();
			}

			if (Input.GetKey(KeyCode.UpArrow))
			{
				ScrollUp();
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				ScrollDown();
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				TogglePlaneActive();
			}

			if (Input.GetKeyDown(KeyCode.C))
			{
				ToggleActiveCamera();
			}

			Debug.Log(offsetX);
		}

		#endregion

		#region Implementation

		private void TogglePlaneActive()
		{
			planeActive = !planeActive;
			plane.SetActive(planeActive);
		}

		private void ToggleActiveCamera()
		{
			topCameraActive = !topCameraActive;

			if (topCameraActive)
			{
				topCamera.gameObject.SetActive(true);
				sideCamera.gameObject.SetActive(false);
			}
			else
			{
				sideCamera.gameObject.SetActive(true);
				topCamera.gameObject.SetActive(false);
			}
		}

		private void ScrollDown()
		{
			offsetY -= ScrollSpeed*Time.deltaTime;

			if (offsetY < -CellHeightAndAHalf)
			{
				offsetY += CellHeightAndAHalf;
				gridOffset += PointyHexPoint.NorthEast + PointyHexPoint.NorthWest;

				UpdateUVs();
			}

			UpdateVerts();
		}

		private void ScrollUp()
		{
			offsetY += ScrollSpeed*Time.deltaTime;

			if (offsetY > CellHeightAndAHalf)
			{
				offsetY -= CellHeightAndAHalf;
				gridOffset -= PointyHexPoint.NorthEast + PointyHexPoint.NorthWest;

				UpdateUVs();
			}

			UpdateVerts();
		}

		private void ScrollRight()
		{
			offsetX += ScrollSpeed*Time.deltaTime;

			if (offsetX > -CellWidth)
			{
				offsetX -= CellWidth;
				gridOffset -= PointyHexPoint.East;
				UpdateUVs();
			}

			UpdateVerts();
		}

		private void ScrollLeft()
		{
			offsetX -= ScrollSpeed*Time.deltaTime;

			if (offsetX < -CellWidth)
			{
				offsetX += CellWidth;
				gridOffset += PointyHexPoint.East;
				UpdateUVs();
			}

			UpdateVerts();
		}

		private static void GenerateMesh(Mesh mesh, IGrid<int, PointyHexPoint> grid, IMap3D<PointyHexPoint> map,
			Vector2 dimensions)
		{
			mesh.Clear();
			mesh.vertices = MakeVertices(grid, map, dimensions, 0, 0);
			mesh.uv = MakeUVs(grid, PointyHexPoint.Zero);
			mesh.triangles = MakeTriangles(grid);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			mesh.MarkDynamic();
			mesh.name = "HexSphere";
		}

		private static Vector3[] MakeNormals(IEnumerable<PointyHexPoint> grid)
		{
			return grid.SelectMany(p => Normals).ToArray();
		}

		private static int[] MakeTriangles(IEnumerable<PointyHexPoint> grid)
		{
			var vertexIndices = Enumerable.Range(0, grid.Count());

			return vertexIndices
				.SelectMany(i => Triangles.Select(j => i*7 + j))
				.ToArray();
		}

		private static Vector2[] MakeUVs(IGrid<int, PointyHexPoint> grid, PointyHexPoint gridOffset)
		{
			return grid
				.SelectMany(p => UVDirections.Select(uv => CalcUV(uv, grid[p + gridOffset])))
				.ToArray();
		}

		private static Vector3[] MakeVertices(IEnumerable<PointyHexPoint> grid, IMap3D<PointyHexPoint> map, Vector2 dimensions,
			float offsetX, float offsetY)
		{
			return grid
				.SelectMany(p => VertexDirections.Select(v => v*dimensions.y + map[p] + new Vector3(offsetX, 0, offsetY)))
				.Select<Vector3, Vector3>(ToSphere)
				.ToArray();
		}

		private static Vector3 ToSphere(Vector3 flatPoint)
		{
			//Assumes y = 0

			float sqrDistanceFromOrigin = flatPoint.sqrMagnitude;

			if (sqrDistanceFromOrigin < SqrSphereRadius)
			{
				float ratio = 2 - Mathf.Sqrt(sqrDistanceFromOrigin)/Mathf.Sqrt(SqrSphereRadius);
				var newPos = new Vector3(flatPoint.x*ratio, 0, flatPoint.z*ratio);
				var sqrHeight = SqrSphereRadius - newPos.sqrMagnitude;

				if (sqrHeight > 0)
				{
					return new Vector3(newPos.x, Mathf.Sqrt(sqrHeight), newPos.z);
				}
			}

			return flatPoint;
		}

		private static Vector2 CalcUV(Vector2 fullUV, int textureIndex)
		{
			int textureIndexX = textureIndex%TextureGridWidth;
			int textureIndexY = textureIndex/TextureGridHeight;

			float u = fullUV.x/TextureGridWidth + textureIndexX*TextureCellWidth;
			float v = fullUV.y/TextureGridHeight + textureIndexY*TextureCellHeight;

			return new Vector2(u, v);
		}

		private static float Sqr(float f)
		{
			return f*f;
		}

		private void UpdateVerts()
		{
			mesh.mesh.vertices = MakeVertices(grid, map, dimensions, offsetX, offsetY);
			mesh.mesh.RecalculateNormals();
			mesh.mesh.UploadMeshData(false);
		}

		private void UpdateUVs()
		{
			mesh.mesh.uv = MakeUVs(grid, gridOffset);
		}

		#endregion
	}
}