using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    //Questa classe gestisce la board di gioco

    //L'oggetto Board ha una Tilemap come oggetto figlio
    public Tilemap tilemap { get; private set; }

    //Riferimento al pezzo attualmente in movimento
    public Piece active_piece { get; private set; }

    public PieceGroupsData[] pieces;
    public Vector3Int spawnPosition;
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public GameObject pointsViewer;
    public GameObject levelViewer;

    public long points;
    public int level; 
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

        //Inizializza la pool dei pezzi da cui pescare per lo spawn casuale
        for (int i = 0; i < this.pieces.Length; i++) {
            this.pieces[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    private void LateUpdate()
    {
        UIUpdate();
        if (level != LevelUpdate()) {
            level = LevelUpdate();
            active_piece.stepDelay = active_piece.stepDelay - 0.15f;
        }
    }

    public void SpawnPiece()
    {
        //Spawna un pezzo casuale dalla pool
        int random = Random.Range(0, this.pieces.Length);
        PieceGroupsData data = this.pieces[random];

        this.active_piece.Initialize(this, this.spawnPosition, data);

        if(IsValidPosition(this.active_piece, this.spawnPosition)) {
            Set(this.active_piece);
        }

        else { GameOver();  }
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

    //La funziona restituisce true se la posizione data in input per il dato pezzo � valida
    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for(int i = 0; i < piece.cells.Length; i++) {
            Vector3Int tilePosition = piece.cells[i] + position;

            //Controlla se una delle celle � fuori dal rettangolo della board
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
        
        //Il contatore di combo conta se si sono pulite 4 o pi� righe insieme, per aumentare il punteggio
        int combo = 0;

        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while(row < bounds.yMax) {
            if(IsLineFull(row)) {
                LineClear(row);
                combo++;
                if (combo == 4) {
                    points += 800;
                }
                else if (combo > 4) {
                    points += 1200;
                }
                else { points += 100; }
            }
            else { 
                row++; 
                combo = 0; 
            }
        }
    }


    //Questo metodo itera sulla board di gioco dalla riga pi� in basso fino a salire, controllando se una si � riempita
    public bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for(int col = bounds.xMin; col < bounds.xMax; col++) {

            Vector3Int position = new Vector3Int(col, row, 0);

            if(!this.tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    //Itera sulla linea che gli viene passata eliminando i blocchi con SetTile(null), poi sposta gi� gli altri blocchi
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
    }

    private void GameOver()
    {
        points = 0;
        this.tilemap.ClearAllTiles();
    }

    private void UIUpdate()
    {
       pointsViewer.GetComponent<Text>().text = "" + points;
       levelViewer.GetComponent<Text>().text = "" + level;
    }

    private int LevelUpdate() {

        int necessaryPoints = level * 1000;

        if(points >= necessaryPoints) {
            level++;
        }

        return level;

    }
}
