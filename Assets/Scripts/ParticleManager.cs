using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellState
{
    INACTIVE = 0,
	ACTIVE,
	SAND,
	WATER,
	//LAVA,
	//ACID,
	FIRE,
	//STEAM,
	//SMOKE,
	WOOD,
    COUNT
}

public struct Cell
{
    private CellState currentState;
    public Color currentColor;
	public CellState nextState;

    public CellState CurrentState
	{
		get { return currentState; }
        set
		{
			switch (value)
			{
				case CellState.INACTIVE:
                    currentColor = Color.black;
					break;
				case CellState.ACTIVE:
                    currentColor = Color.white;
					break;
				case CellState.SAND:
					currentColor = Color.yellow;
					break;
				case CellState.WATER:
					currentColor = Color.cyan;
					break;
				case CellState.WOOD:
					currentColor = new Color(0.413f,0.346f,0.240f);
					break;
				case CellState.FIRE:
					currentColor = Color.red;
					break;
				default:
					break;
			}
			currentState = value;
		}
	}
}

public class ParticleManager : MonoBehaviour
{
    public Vector2Int ratio = Vector2Int.zero;
	public float updateTimer = 0f;
	[Range(0,10)]
	public int cursorSize = 0;
	private float chrono;
	private Cell[,] cells;
    private float size = 1.0f;
    private Vector2 scaleSize = Vector2.zero;

	private bool simulationIsOn = false;

	public CellState cursorState = CellState.ACTIVE;
	public Material mat;

    // Start is called before the first frame update
    void Start()
    {
		chrono = updateTimer;

		cells = new Cell[ratio.x,ratio.y];

		for (int j = 0; j < ratio.y; j++)
		{
			for (int i = 0; i < ratio.x; i++)
			{
				cells[i, j].CurrentState = CellState.INACTIVE;
				cells[i, j].nextState = cells[i, j].CurrentState;
			}
		}

		//I have to cast from int to float to have a floating value as a result for our size
		size = 1 / Mathf.Max((float)ratio.x, (float)ratio.y);
        scaleSize.x = size;
        scaleSize.y = size;
    }

    // Update is called once per frame
    void Update()
    {
		ComputeInput();

		if (chrono > 0f)
			chrono -= Time.deltaTime;

		if (simulationIsOn && chrono <= 0f)
		{
			for (int j = 0; j < ratio.y; j++)
			{
				for (int i = 0; i < ratio.x; i++)
				{
					UpdateCell(i, j);
				}
			}
			for (int j = 0; j < ratio.y; j++)
			{
				for (int i = 0; i < ratio.x; i++)
				{
					cells[i, j].CurrentState = cells[i, j].nextState;
				}
			}
			chrono = updateTimer;
		}
	}

    
    void OnPostRender()
    {
		if (!mat)
		{
			Debug.LogError("Please Assign a material on the inspector");
			return;
		}

		//GL.PushMatrix();
		//mat.SetPass(0);
		//GL.LoadOrtho();
		//GL.Begin(GL.QUADS);
		//GL.Color(Color.red);
		//GL.Vertex3(0, 0.5f, 0);
		//GL.Vertex3(0.5f, 1, 0);
		//GL.Vertex3(1, 0.5f, 0);
		//GL.Vertex3(0.5f, 0, 0);

		//GL.Color(Color.cyan);
		//GL.Vertex3(0, 0, 0);
		//GL.Vertex3(0, 0.25f, 0);
		//GL.Vertex3(0.25f, 0.25f, 0);
		//GL.Vertex3(0.25f, 0, 0);
		//GL.End();
		//GL.PopMatrix();


		GL.PushMatrix();
		mat.SetPass(0);
		GL.LoadOrtho();
		GL.Begin(GL.QUADS);
		for (int j = 0; j < ratio.y; j++)
		{
			for (int i = 0; i < ratio.x; i++)
			{
				//Color debugColor = new Color(i * size, j * size, 1);
				//if (i == 0 && j == 0)
				//{
				//	debugColor = Color.black;
				//}
				//else if (i == (ratio.x - 1) && j == (ratio.y - 1))
				//{
				//	debugColor = Color.white;
				//	Debug.Log("Last square coord" + '\n' + "X:" + i*size + " Y:" + j*size);
				//}
				//GL.Color(debugColor);

				GL.Color(cells[i, j].currentColor);
				GL.Vertex3(i*size, j*size, 0);					//0;0
				GL.Vertex3(i*size, j*size + size, 0);			//0;height
				GL.Vertex3(i*size + size, j*size + size, 0);	//width;height
				GL.Vertex3(i*size + size, j*size, 0);			//width;0
			}
		}
		GL.End();
		GL.PopMatrix();
	}

	private void ComputeInput()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();

		if (Input.GetKeyDown(KeyCode.Space))
		{
			simulationIsOn = !simulationIsOn;
			Debug.Log("Simulation is now " + (simulationIsOn ? "ON" : "OFF"));
		}

		if (Input.GetMouseButton(0))
		{
			Vector2 mousePos = Input.mousePosition;
			mousePos = Camera.main.ScreenToViewportPoint(mousePos);
			if (mousePos.x < 0f || mousePos.y < 0f || mousePos.x > 1f || mousePos.y > 1f)
				return;

			Vector2Int coord = new Vector2Int(Mathf.FloorToInt(mousePos.x / size), Mathf.FloorToInt(mousePos.y / size));

			int minX = coord.x - cursorSize >= 0 ? coord.x - cursorSize : 0;
			int maxX = coord.x + cursorSize < ratio.x - 1 ? coord.x + cursorSize : ratio.x - 1;

			int minY = coord.y - cursorSize >= 0 ? coord.y - cursorSize : 0;
			int maxY = coord.y + cursorSize < ratio.y - 1 ? coord.y + cursorSize : ratio.y - 1;

			for (int j = minY; j <= maxY; j++)
			{
				for (int i = minX; i <= maxX; i++)
				{
					cells[i, j].CurrentState = cursorState;
					cells[i, j].nextState = cursorState;
				}
			}
			//cells[coord.x, coord.y].CurrentState = cursorState;
			//cells[coord.x, coord.y].nextState = cursorState;
		}
	}

	private void UpdateCell(int _xIndex, int _yIndex)
	{
		switch (cells[_xIndex, _yIndex].CurrentState)
		{
			//case CellState.INACTIVE:
			//{
			//		//int aliveCells = CountAliveCells(_xIndex, _yIndex);
			//		//if (aliveCells == 3)
			//		//{
			//		//	cells[_xIndex, _yIndex].nextState = CellState.ACTIVE;
			//		//	Debug.Log("Should resurect at " + _xIndex + ";" + _yIndex);
			//		//}
			//	break;
			//}
			//case CellState.ACTIVE:
			//{
			//		//int aliveCells = CountAliveCells(_xIndex, _yIndex);
			//		//if (aliveCells < 2 || aliveCells > 3)
			//		//{
			//		//	cells[_xIndex, _yIndex].nextState = CellState.INACTIVE;
			//		//}
			//	break;
			//}
			case CellState.SAND:
			{
					System.Tuple<int, int> coordinate = new System.Tuple<int,int>(_xIndex,_yIndex);
					coordinate = FindFreeCellSand(_xIndex,_yIndex);

					cells[_xIndex, _yIndex].nextState = CellState.INACTIVE;
					if(cells[coordinate.Item1, coordinate.Item2].nextState == CellState.WATER)
						cells[_xIndex, _yIndex].nextState = CellState.WATER;

					cells[coordinate.Item1,coordinate.Item2].nextState = CellState.SAND;
					break;
			}
			case CellState.WATER:
			{
				System.Tuple<int, int> coordinate = new System.Tuple<int, int>(_xIndex, _yIndex);

				coordinate = FindFreeCellWater(_xIndex, _yIndex);
				cells[_xIndex, _yIndex].nextState = CellState.INACTIVE;
				cells[coordinate.Item1, coordinate.Item2].nextState = CellState.WATER;
				break;
			}
			case CellState.FIRE:
			{
				List<System.Tuple<int, int>> canBurnCells = FindFreeCellFire(_xIndex, _yIndex);

				cells[_xIndex, _yIndex].nextState = CellState.INACTIVE;
				foreach (System.Tuple<int, int> burnCoord in canBurnCells)
				{
					if(cells[burnCoord.Item1,burnCoord.Item2].nextState == CellState.WOOD)
					{
						cells[burnCoord.Item1, burnCoord.Item2].nextState = CellState.FIRE;
					}
				}

				break;
			}
			case CellState.ACTIVE:
			case CellState.INACTIVE:
			case CellState.WOOD:
			case CellState.COUNT:
			default:
				break;
		}
	}

	private System.Tuple<int, int> FindFreeCellSand(int _xIndex, int _yIndex)
	{
		System.Tuple<int, int> toReturn = new System.Tuple<int, int>(_xIndex, _yIndex);
		if (_yIndex != 0)
		{
			toReturn = new System.Tuple<int, int>(_xIndex, _yIndex - 1);
			if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE
				|| cells[toReturn.Item1, toReturn.Item2].nextState == CellState.WATER)
			{
				return toReturn;
			}

			if (_xIndex > 0)
			{
				toReturn = new System.Tuple<int, int>(_xIndex - 1, _yIndex - 1);
				if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE
					|| cells[toReturn.Item1, toReturn.Item2].nextState == CellState.WATER)
				{
					return toReturn;
				}
			}

			if(_xIndex < ratio.x - 1)
			{
				toReturn = new System.Tuple<int, int>(_xIndex + 1, _yIndex - 1);
				if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE
					|| cells[toReturn.Item1, toReturn.Item2].nextState == CellState.WATER)
				{
					return toReturn;
				}
			}

			toReturn = new System.Tuple<int, int>(_xIndex, _yIndex);
		}
		return toReturn;
	}

	private System.Tuple<int, int> FindFreeCellWater(int _xIndex, int _yIndex)
	{

		System.Tuple<int, int> toReturn = new System.Tuple<int, int>(_xIndex, _yIndex);
		if(toReturn.Item1 == _xIndex && toReturn.Item2 == _yIndex)
		{
			if (_yIndex != 0)
			{
				toReturn = new System.Tuple<int, int>(_xIndex, _yIndex - 1);
				if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE)
				{
					return toReturn;
				}

				if (_xIndex > 0)
				{
					toReturn = new System.Tuple<int, int>(_xIndex - 1, _yIndex - 1);
					if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE)
					{
						return toReturn;
					}
				}

				if (_xIndex < ratio.x - 1)
				{
					toReturn = new System.Tuple<int, int>(_xIndex + 1, _yIndex - 1);
					if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE)
					{
						return toReturn;
					}
				}

				toReturn = new System.Tuple<int, int>(_xIndex, _yIndex);
			}

			if (_xIndex > 0)
			{
				toReturn = new System.Tuple<int, int>(_xIndex - 1, _yIndex);
				if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE)
				{
					return toReturn;
				}
			}
			if (_xIndex < ratio.x - 1)
			{
				toReturn = new System.Tuple<int, int>(_xIndex + 1, _yIndex);
				if (cells[toReturn.Item1, toReturn.Item2].nextState == CellState.INACTIVE)
				{
					return toReturn;
				}
			}
			toReturn = new System.Tuple<int, int>(_xIndex, _yIndex);
		}
		return toReturn;
	}

	private List<System.Tuple<int, int>> FindFreeCellFire(int _xIndex, int _yIndex)
	{
		List <System.Tuple<int, int>> toReturn = new List<System.Tuple<int, int>>();
		
		int minX = _xIndex - 1 >= 0 ? _xIndex - 1 : _xIndex;
		int maxX = _xIndex + 1 < ratio.x - 1 ? _xIndex + 1 : _xIndex;

		int minY = _yIndex - 1 >= 0 ? _yIndex - 1 : _yIndex;
		int maxY = _yIndex + 1 < ratio.y - 1 ? _yIndex + 1 : _yIndex;

		for(int j = minY; j <= maxY; j++)
		{
			for(int i = minX; i <= maxX; i++)
			{
				if(cells[i, j].nextState == CellState.WOOD)
				{
					toReturn.Add(new System.Tuple<int,int>(i,j));
				}
			}
		}
		return toReturn;
	}

	private int CountAliveCells(int _xIndex, int _yIndex)
	{
		int count = 0;
		for(int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				//Ignore my current cell
				if(i != 0 && j != 0)
				{
					//Counting alive cells
					Vector2Int currentCoord = new Vector2Int(_xIndex + i, _yIndex + j);
					if(currentCoord.x >= 0 && currentCoord.y >= 0 && currentCoord.x < ratio.x && currentCoord.y < ratio.y)
					{
						if (cells[currentCoord.x, currentCoord.y].CurrentState == CellState.ACTIVE)
							count++;
					}
				}
			}
		}
		return count;
	}
}
