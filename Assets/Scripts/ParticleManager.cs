using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CellState
{
    INACTIVE = 0,
    ACTIVE,
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
	public float updateTimer = 1f;
	private float chrono;
    private Cell[,] cells;
    private float size = 1.0f;
    private Vector2 scaleSize = Vector2.zero;

	private bool simulationIsOn = false;

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

		if(chrono > 0f)
			chrono -= Time.deltaTime;

		if(simulationIsOn && chrono <= 0f)
		{
			for (int j = 0; j < ratio.y; j++)
			{
				for (int i = 0; i < ratio.x; i++)
				{
					cells[i, j].CurrentState = cells[i, j].nextState;
				}
			}
			for (int j = 0; j < ratio.y; j++)
			{
				for (int i = 0; i < ratio.x; i++)
				{
					UpdateCell(i, j);
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

				GL.Color(cells[i,j].currentColor);
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
			cells[coord.x, coord.y].CurrentState = CellState.ACTIVE;
			cells[coord.x, coord.y].nextState = CellState.ACTIVE;
		}
	}

	private void UpdateCell(int _xIndex, int _yIndex)
	{
		switch (cells[_xIndex, _yIndex].CurrentState)
		{
			case CellState.INACTIVE:
			{
				int aliveCells = CountAliveCells(_xIndex, _yIndex);
				if (aliveCells == 3)
				{
					cells[_xIndex, _yIndex].nextState = CellState.ACTIVE;
					Debug.Log("Should resurect at " + _xIndex + ";" + _yIndex);
				}
				break;
			}
			case CellState.ACTIVE:
			{
				int aliveCells = CountAliveCells(_xIndex, _yIndex);
				if (aliveCells < 2 || aliveCells > 3)
				{
					cells[_xIndex, _yIndex].nextState = CellState.INACTIVE;
				}
				break;
			}
			case CellState.COUNT:
				break;
			default:
				break;
		}
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
