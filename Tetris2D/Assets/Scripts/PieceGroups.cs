using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum PieceGroups
{
    I,
    O,
    T,
    J,
    L,
    S,
    Z,
}

//Questa classe definisce una struct per i pezzi del tetris.
[System.Serializable]
public struct PieceGroupsData
{
    //Questo è l'enum definito sopra che stabilisce il tipo di pezzo
    public PieceGroups piece_group;

    //Definisce il colore del pezzo
    public Tile tile;

    //Il vettore di celle che definisce la forma del pezzo
    public Vector2Int[] cells { get; private set; }
    public Vector2Int[,] wallKicks { get; private set; }

    public void Initialize()
    {
        //Istanzia un pezzo del tipo specificato da piece_group
        this.cells = Data.Cells[this.piece_group];
        this.wallKicks = Data.WallKicks[this.piece_group];

    }
}