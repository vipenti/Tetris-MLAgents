using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public GameObject agent;
    //Questa classe gestisce la board di gioco

    //L'oggetto Board ha una Tilemap come oggetto figlio
    public Tilemap tilemap { get; private set; }

    //Riferimento al pezzo attualmente in movimento
    public Piece active_piece;

    public PieceGroupsData[] pieces;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public GameObject pointsViewer;
    public GameObject levelViewer;

    //variabile aggiunta per capire quando � game over e eseguire l'EndEpisode() e punire l'agente con SetReward();
    public bool game_over = false;
    public long points;
    public int level;
    public int height { get; private set; }
    public int holes { get; private set; }
    public int bumpiness { get; private set; }
    public int clearedLines { get; private set; }

    public bool gameOver { get; private set; }
    //Rettangolo che calcola l'area di gioco usato in IsValidPosition
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.active_piece = GetComponentInChildren<Piece>();

        points = 0;
        level = 1;
        height = 0;
        holes = 0;
        bumpiness = 0;
        clearedLines = 0;


        //Inizializza la pool dei pezzi da cui pescare per lo spawn casuale
        for (int i = 0; i < this.pieces.Length; i++)
        {
            this.pieces[i].Initialize();
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void LateUpdate()
    {
        UIUpdate();
        if (level != LevelUpdate())
        {
            level = LevelUpdate();
            active_piece.stepDelay = active_piece.stepDelay - 0.15f;
        }
    }

    public void StartGame()
    {
        this.tilemap.ClearAllTiles();
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        int reward = 0;

        for (int col = Bounds.xMin; col < Bounds.xMax; col++)
        {
            for (int row = Bounds.yMin; row < Bounds.yMax; row++)
            {
                if (IsCellOccupied(row, col) == 1)
                {
                    reward++;
                    break;
                }
            }
        }
        agent.GetComponent<PlayerAgent>().AddReward(reward * -reward); //punizione se crea torri di tetramini alte
        //Spawna un pezzo casuale dalla pool
        int random = Random.Range(0, this.pieces.Length);
        PieceGroupsData data = this.pieces[random];

        this.active_piece.Initialize(this, this.spawnPosition, data);

        if (IsValidPosition(this.active_piece, this.spawnPosition))
        {
            Set(this.active_piece);
        }

        else { GameOver(); }
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            //Vengono prese le singole celle e shiftate della posizione totale del pezzo
            Vector3Int tilePosition = piece.cells[i] + piece.position;

            //Vengono disegnati i tile sulla mappa
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    //La funziona restituisce true se la posizione data in input per il dato pezzo � valida
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            //Controlla se una delle celle � fuori dal rettangolo della board
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    //Chiama LineClear() se trova una riga piena
    public void ClearLines()
    {

        //Il contatore di combo conta se si sono pulite 4 o pi� righe insieme, per aumentare il punteggio
        int combo = 0;

        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                clearedLines++;
                combo++;
                if (combo == 4)
                {
                    points += 800;
                    agent.GetComponent<PlayerAgent>().AddReward(+20);
                    Debug.Log("REWARD: 4 LINES CLEARED!!! +20");
                }
                else if (combo > 4)
                {
                    points += 1200;
                    agent.GetComponent<PlayerAgent>().AddReward(+50);
                    Debug.Log("REWARD: MORE THAN 4 LINES CLEAR!!! +50");
                }
                else { points += 100; }
            }
            else
            {
                row++;
                combo = 0;
            }
        }
    }


    //Questo metodo itera sulla board di gioco dalla riga pi� in basso fino a salire, controllando se una si � riempita
    public bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;
        int count = 0;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {

            Vector3Int position = new Vector3Int(col, row, 0);

            if (this.tilemap.HasTile(position))
            {
                count++;
            }
        }
        agent.GetComponent<PlayerAgent>().AddReward(count * count);

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {

            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        agent.GetComponent<PlayerAgent>().AddReward(100f); //premio se riempie una linea
        return true;
    }

    //Itera sulla linea che gli viene passata eliminando i blocchi con SetTile(null), poi sposta gi� gli altri blocchi
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    private void GameOver()
    {
        points = 0;
        this.tilemap.ClearAllTiles();
        agent.GetComponent<PlayerAgent>().SetReward(-100f);
        agent.GetComponent<PlayerAgent>().EndEpisode();
        //active_piece.SetReward(-100f);
        //active_piece.EndEpisode();
        //game_over = true; //setto la variabile qui.
    }

    private void UIUpdate()
    {
        pointsViewer.GetComponent<Text>().text = "" + points;
        levelViewer.GetComponent<Text>().text = "" + level;
    }

    private int LevelUpdate()
    {

        int necessaryPoints = level * 1000;

        if (points >= necessaryPoints)
        {
            level++;
            agent.GetComponent<PlayerAgent>().AddReward(10f); //salgo di livello premio l'agente
        }

        return level;

    }

    public int IsCellOccupied(int row, int col)
    {
        Vector3Int position = new Vector3Int(col, row, 0);

        if (!this.tilemap.HasTile(position))
        {
            return 0;
        }
        else
            return 1;
    }

    //metodi aggiunti a cui accedere l'agente
    public void DecidedMove(int a, int i)
    {
        if (a == 0 && i == 0)
        {
            //active_piece.Move(Vector2Int.right);
            active_piece.act[0] = 0;
        }
        if (a == 0 && i == 1)
        {
            active_piece.act[0] = 1;
            //active_piece.Move(Vector2Int.left);
        }

        if (a == 0 && i == 2)
        {
            ;
        }
    }

    public void DecidedRotate(int a, int i)
    {
        if (a == 1 && i == 0)
        {
            active_piece.act[1] = 0;
            //active_piece.Rotate(-1);
        }
        if (a == 1 && i == 1)
        {
            active_piece.act[1] = 1;
            //active_piece.Rotate(1);
        }
        if (a == 1 && i == 2)
        {
            ;
        }
    }

    public void DecidedHardDrop(int a, int i)
    {
        if (a == 2 && i == 0)
        {
            active_piece.act[2] = 0;
            //active_piece.HardDrop();
        }
        if (a == 2 && i == 1)
        {
            ;
        }
    }

    public void DecidedMoveDown(int a, int i)
    {
        if (a == 3 && i == 0)
        {
            active_piece.act[3] = 0;
            //active_piece.Move(Vector2Int.down);
        }
        if (a == 3 && i == 1)
        {
            ;
        }
    }

    public int Holes()
    {
        RectInt bounds = this.Bounds;
        int holes = 0;

        for (int row = bounds.yMin + 1; row < bounds.yMax - 1; row++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {

                Vector3Int position = new Vector3Int(col, row, 0);
                if (!this.tilemap.HasTile(position))
                {
                    if (this.tilemap.HasTile(new Vector3Int(col, row + 1, 0)) && this.tilemap.HasTile(new Vector3Int(col, row - 1, 0)))
                    {
                        holes++;
                    }
                }
            }
        }

        return holes;
    }


    public int Bumpiness()
    {
        RectInt bounds = this.Bounds;
        int bumpiness = 0;
        int count = 0;
        List<int> colPieces = new List<int>();

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {

            count = 0;
            for (int row = bounds.yMax, i = 0; row >= bounds.yMin; row--, i++)
            {

                Vector3Int position = new Vector3Int(col, row, 0);
                if (this.tilemap.HasTile(position))
                {
                    count = 20 - i + 1;
                    break;
                }
            }

            colPieces.Add(count);
        }

        for (int j = 0; j < colPieces.Count - 1; j++)
        {

            bumpiness += Mathf.Abs(colPieces[j] - colPieces[j + 1]);

        }

        return bumpiness;
    }

    public int Height()
    {
        RectInt bounds = this.Bounds;

        for (int row = bounds.yMax, height = 20; row > bounds.yMin; row--, height--)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {

                Vector3Int position = new Vector3Int(col, row, 0);
                if (this.tilemap.HasTile(position))
                {

                    return height + 1;
                }
            }
        }

        return 0;
    }
    public void AgentReward()
    {
        /*float reward = agent.FitnessFunction();
        agent.AddReward(reward);

        Debug.Log("REWARD: Fitness Function: " + reward);

        holes = Holes();
        bumpiness = Bumpiness();
        height = Height();*/
        if (bumpiness >= Bumpiness())
        {
            agent.GetComponent<PlayerAgent>().AddReward(20f);
            Debug.Log("REWARD: Better bumpiness! +20");
        }
        else
        {
            agent.GetComponent<PlayerAgent>().AddReward(-30f);
            Debug.Log("REWARD: Worse bumpiness! -30");
        }

        if (Holes() <= holes)
        {
            agent.GetComponent<PlayerAgent>().AddReward(20f);
            Debug.Log("REWARD: No new holes! +20");
        }
        else
        {
            agent.GetComponent<PlayerAgent>().AddReward(-50f);
            Debug.Log("REWARD: Made a hole! -50");
        }

        if (Height() <= height)
        {
            agent.GetComponent<PlayerAgent>().AddReward(20f);
            Debug.Log("REWARD: Stable height! 20");
        }

        else
        {
            agent.GetComponent<PlayerAgent>().AddReward(-40f);
            Debug.Log("REWARD: Higher height! -40");
        }

        height = Height();
        holes = Holes();
        bumpiness = Bumpiness();
    }
}

