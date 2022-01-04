using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        //Inizializza la pool dei pezzi da cui pescare per lo spawn casuale
        for (int i = 0; i < this.pieces.Length; i++) {
            this.pieces[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
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

        while(row < bounds.yMax) {
            if(IsLineFull(row)) {
                LineClear(row);
            }
            else { row++; }
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
    }

    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
    }
}
