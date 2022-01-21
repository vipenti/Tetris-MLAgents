using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    //Questa classe gestisce la board di gioco

    //L'oggetto Board ha una Tilemap come oggetto figlio
    public Tilemap tilemap { get; private set; }

    //Riferimento al pezzo attualmente in movimento
    public Piece active_piece; //{ get; private set; }
    public TetrisAgent agent { get; private set; }

    public PieceGroupsData[] pieces;
    public List<int> indeces;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public GameObject pointsViewer;
    public GameObject linesViewer;
    public GameObject levelViewer;
    public GameObject agentController;

    public long points;
    public int level;
    public int lines;
    public int height { get; private set; }
    public int holes { get; private set; }
    public int bumpiness { get; private set; }
    public int clearedLines { get; private set; }

    public bool gameOver { get; private set; }

    private int xspawn;


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
        this.agent = agentController.GetComponent<TetrisAgent>();
        this.indeces = new List<int>();

        Debug.Log("Agent is null? " + (this.agent == null));

        points = 0;
        level = 1;
        height = 0;
        holes = 0;
        bumpiness = 0;
        clearedLines = 0;

        //Inizializza la pool dei pezzi da cui pescare per lo spawn casuale
        for (int i = 0; i < this.pieces.Length; i++) {
            this.pieces[i].Initialize();
        }
    }

    private void Start()
    {
        Debug.Log("Starting board " + this.name + "   " + this.GetInstanceID());
        SpawnPiece();
    }

    private void LateUpdate()
    {
        UIUpdate();
        if (level != LevelUpdate()) {
            level = LevelUpdate();
            active_piece.stepDelay -= 0.15f;
        }
    }

    public void SpawnPiece()
    {
        if (MaxHeight() >= 19)
        {
            GameOver();
            this.tilemap.ClearAllTiles();
            return;
        }

        //Spawna un pezzo casuale dalla pool
        else
        {
            int random = GenerateIndex();

            xspawn = 0;

            Debug.Log("Generated piece " + random + "for board " + this.GetInstanceID());

            NextStep(random);


            if (random == 0 && xspawn == 3)
            {
                this.spawnPosition = new Vector3Int(2, 8, 0);
            }

            else
            {
                this.spawnPosition = new Vector3Int(xspawn, 8, 0);
            }

            clearedLines = 0;

            PieceGroupsData data = this.pieces[random];

            this.active_piece.Initialize(this, this.spawnPosition, data);

            DecidedRotation(active_piece, agent.rotation, agent.spawn);

            Debug.Log("Valid position spawn: " + spawnPosition + " = " + IsValidPosition(this.active_piece, this.spawnPosition));

            Debug.Log("HOLES: " + Holes());

            Set(this.active_piece);

            agent.AddReward(+1f);
            agent.reward += 1f;
            Debug.Log("Reward: piece placed +1");
        }        
    }


    public void Set(Piece piece)
    {
        for(int i = 0; i < piece.cells.Length; i++) {

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

    //La funziona restituisce true se la posizione data in input per il dato pezzo è valida
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for(int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + position;

            //Controlla se una delle celle è fuori dal rettangolo della board
            if(!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            if(this.tilemap.HasTile(tilePosition)) {
                return false;
            }
        }
        return true;
    }

    //Chiama LineClear() se trova una riga piena
    public void ClearLines() {
        
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;
        List<int> rows = new List<int>();

        while(row < bounds.yMax) {

            if(IsLineFull(row)) {
                LineClear(row);
                rows.Add(row);
                clearedLines++;

                if(clearedLines == 2)
                {
                    points += 200;
                }

                if (clearedLines == 4) {
                    points += 800;
                }

                else if (clearedLines > 4) {
                    points += 1200;
                }

                else { 
                    points += 100; 
                }
            }
            else { 
                row++; 
            }
        }
        if(rows.Count > 0) {
            agent.AddLineReward(rows);
        }
    }


    //Questo metodo itera sulla board di gioco dalla riga più in basso fino a salire, controllando se una si è riempita
    public bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for(int col = bounds.xMin; col < bounds.xMax; col++) {

            Vector3Int position = new Vector3Int(col, row, 0);

            if(!this.tilemap.HasTile(position)) {
                return false;
            }
        }

        Debug.Log("Line " + row + " is full");

        return true;
    }

    //Itera sulla linea che gli viene passata eliminando i blocchi con SetTile(null), poi sposta giù gli altri blocchi
    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++) {
            Vector3Int position = new Vector3Int(col, row, 0);
            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax) {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
        lines++;
    }

    private void GameOver()
    {
        points = 0;
        lines = 0;
        level = 1;
        clearedLines = 0;
        this.tilemap.ClearAllTiles();
        indeces = new List<int>();

        agent.AddReward(-20f);
        agent.reward -= 20f;
        Debug.Log("REWARD: Game over! -20");

        agent.UpdateStats();
        agent.EndEpisode();

    }

    private void UIUpdate()
    {
       pointsViewer.GetComponent<Text>().text = "" + points;
       levelViewer.GetComponent<Text>().text = "" + level;
       linesViewer.GetComponent<Text>().text = "" + lines + "/10";

        if(lines == 10)
        {
            lines = 0;
            linesViewer.GetComponent<Text>().text = "" + lines + "/10";
        }
    }

    private int LevelUpdate() {

        int necessaryPoints = level * 1000;

        if(points >= necessaryPoints) {
            level++;
            agent.AddReward(+10.0f);
            agent.reward += 10f;
            Debug.Log("REWARD: Level up! +10");
        }

        return level;

    }

    public ref Piece GetPiece()
    {
        return ref this.active_piece;
    }

    public int MaxHeight()
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

    public int AggregateHeight()
    {
        RectInt bounds = this.Bounds;
        int sum = 0;
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

        for (int j = 0; j < colPieces.Count; j++)
        {
            sum += colPieces[j];
        }

        return sum;
    }

    public int Holes()
    {
        RectInt bounds = this.Bounds;
        int holes = 0;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            if (!this.tilemap.HasTile(new Vector3Int(col, bounds.yMin, 0)))
            {
                if (this.tilemap.HasTile(new Vector3Int(col, bounds.yMin + 1, 0)))
                {
                    holes++;
                }
            }
        }

            for (int row = bounds.yMin + 1; row < bounds.yMax - 1; row++) {
            for (int col = bounds.xMin; col < bounds.xMax; col++) {

                Vector3Int position = new Vector3Int(col, row, 0);
                if (!this.tilemap.HasTile(position))
                {
                    if(this.tilemap.HasTile(new Vector3Int(col, row+1, 0)) && this.tilemap.HasTile(new Vector3Int(col, row-1, 0))) {
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

        for (int col = bounds.xMin; col < bounds.xMax; col++) {

            count = 0;
            for (int row = bounds.yMax, i = 0; row >= bounds.yMin; row--, i++) {

                Vector3Int position = new Vector3Int(col, row, 0);
                if (this.tilemap.HasTile(position)) {
                    count = 20 - i + 1;
                    break;
                }
            }

            colPieces.Add(count);
        }

        for (int j = 0; j < colPieces.Count - 1; j++) {            

            bumpiness += Mathf.Abs(colPieces[j] - colPieces[j + 1]);
        }

        return bumpiness;
    }

    public int FullLines()
    {
        RectInt bounds = this.Bounds;
        int lines = 0;

        for (int row = bounds.yMin; row <= bounds.yMax; row++)
        {
            if (IsLineFull(row))
            {
                lines++;
            }
        }

        return lines;
    }

    private void DecidedRotation(Piece piece, int rotation, int spawn)
    {
        switch (rotation)
        {
            case 1:
                break;

            case 2:
                piece.Rotate(1);
                break;

            case 3:
                piece.Rotate(1);
                piece.Rotate(1);
                break;

            case 4:
                piece.Rotate(-1);
                break;
        }

        if (spawn == -4)
        {
            while (piece.Move(Vector2Int.left))
            {
                continue;
            }
        }

        if (spawn == 3)
        {
            while (piece.Move(Vector2Int.right))
            {
                continue;
            }
        }

    }

    public void SetAgent(TetrisAgent agent)
    {
        this.agent = agent;
        Debug.Log("SET AGENT = " + this.agent.name + "   " + this.agent.GetInstanceID() + " = " + (this.agent == null));


    }

    private void NextStep(int pieceIndex)
    {
        Debug.Log("Populating... ");
        agent.states = new List<float>();

        PopulateStates(pieceIndex);

        Debug.Log("Populated ");

        agent.AgentStep();
        agent.RequestDecision();

        xspawn = agent.spawn;
    }

    private void PopulateStates(int pieceIndex)
    {
        for(int spawn = -4; spawn <= 3; spawn++) {
            for(int rot = 1; rot <= 4; rot++) {

                float[] obs = GetStates(pieceIndex, spawn, rot);
                agent.states.AddRange(obs);
            }
        }
    }

    private float[] GetStates(int pieceIndex, int spawn, int rot)
    {
        float[] states = new float[4];

        PieceGroupsData data = this.pieces[pieceIndex];

        if(pieceIndex == 0 && spawn == 3) {
            active_piece.Initialize(this, new Vector3Int(spawn - 1, 8, 0), data);
        }

        else { 
            active_piece.Initialize(this, new Vector3Int(spawn, 8, 0), data); 
        }

        Debug.Log("GETSTATES: Initialized piece " + active_piece.data.piece_group);
        
        for(int j = 1; j <= rot; j++) {
            active_piece.Rotate(+1);
        }
        int k = 0;
        Debug.Log("GETSTATES: Rotated piece " + rot + " times");

        while (active_piece.Move(Vector2Int.down)) {
            k++;
            continue;            
        }

        Debug.Log("GETSTATES: Moved piece down " + k + " times");

        Set(active_piece);

        Debug.Log("GETSTATES: Setted piece " + active_piece.data.piece_group);

        states[0] = MaxHeight() / 4f;
        states[1] = AggregateHeight() / 200f;
        states[2] = Bumpiness() / 200f;
        states[3] = Holes() / 200f;

        Clear(active_piece);

        Debug.Log("GETSTATES: Cleared piece " + active_piece.data.piece_group);

        return states;
    }

    private int GenerateIndex()
    {
        if(indeces.Count == 0) { 
            for (int i = 0; i < this.pieces.Length; i++)
            {
                indeces.Add(i);
            }
        }

        int random = Random.Range(0, this.indeces.Count);

        int index = indeces[random];
        indeces.RemoveAt(random);

        return index;
    }
}
