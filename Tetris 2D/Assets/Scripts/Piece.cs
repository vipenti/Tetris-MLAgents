using UnityEngine;

public class Piece : MonoBehaviour
{
    //Riferimento alla board di gioco
    public Board board { get; private set; }

    //Definisce la forma del pezzo
    public PieceGroupsData data { get; private set; }

    //Posizione di spawn
    public Vector3Int position { get; private set; }

    //Array di celle per poterle manipolare singolarmente
    public Vector3Int[] cells { get; private set; }

    //Valore periodico fra 1 e 4 per indicare che forma deve assumere il pezzo dopo una rotazione dalla forma attuale
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float lockDelay = 0.5f;

    //Intervallo di tempo dopo il quale il pezzo scende da solo
    private float stepTime;
    //Tempo di reazione prima che si blocchi il pezzo
    private float lockTime;

    public int[] act;

    public void Initialize(Board board, Vector3Int position, PieceGroupsData data)
    {
        act = new int[4];
        act[0] = 2;
        act[1] = 2;
        act[2] = 1;
        act[3] = 1;
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;

        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        //Se l'array è ancora null, viene inizializzato
        if (this.cells == null)
        {
            this.cells = new Vector3Int[data.cells.Length];
        }

        //Ogni cellla viene copiata nell'array per com'è nel PieceGroupsData
        for (int i = 0; i < data.cells.Length; i++)
        {
            this.cells[i] = (Vector3Int)data.cells[i];
        }

    }

    public void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;

        //qui stavo cercando di fare cose per far funzionare il codice in modalità Heuristic
        if (act[0] == 0)
        {
            Move(Vector2Int.right);
            act[0] = 2;
        }
        if (act[0] == 1)
        {
            Move(Vector2Int.left);
            act[0] = 2;
        }
        //abbiamo premuto Comma
        if (act[1] == 0)
        {
            Rotate(-1);
            act[1] = 2;
        }
        //abbiamo premuto Period
        if (act[1] == 1)
        {
            Rotate(1);
            act[1] = 2;
        }

        if (act[2] == 0)
        {
            HardDrop();
            act[2] = 1;
        }

        if (act[3] == 0)
        {
            Move(Vector2Int.down);
            act[3] = 1;
        }

        //Movimento automatico del pezzo verso il basso
        if (Time.time >= this.stepTime)
        {
            Step();
        }

        this.board.Set(this);
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;
        Move(Vector2Int.down);

        //Tempo di reazione
        if (this.lockTime >= this.lockDelay)
        {
            print("lock");
            Lock();
        }
    }

    public void Lock()
    {
        this.board.Set(this);
        this.board.ClearLines();
        this.board.SpawnPiece();
    }

    //Quando si preme la barra spaziatrice, il pezzo cade direttamente giù
    public void HardDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }

        Lock();
    }

    public bool Move(Vector2Int direction)
    {
        Vector3Int new_position = this.position;
        new_position.x += direction.x;
        new_position.y += direction.y;

        bool valid = board.IsValidPosition(this, new_position);

        if (valid)
        {
            this.position = new_position;
            this.lockTime = 0f;
        }

        return valid;
    }

    public void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        //Se la rotazione non è valida, la si risporta alla posizione originale
        if (!TestWallKicks(this.rotationIndex, direction))
        {
            this.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }


    //La rotazione è complicata e utilizza delle matrici di rotazione definite in Data.cs
    //Non è importante capire come funzionano nello specifico, sono prese direttamente dal metodo ufficiale del tetris originale
    private void ApplyRotationMatrix(int direction)
    {
        for (int i = 0; i < this.data.cells.Length; i++)
        {
            Vector3 cell = this.cells[i];

            int x, y;

            switch (this.data.piece_group)
            {
                case PieceGroups.I:
                case PieceGroups.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * Data.RotationMatrix[0] * direction) + (cell.y * Data.RotationMatrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * Data.RotationMatrix[2] * direction) + (cell.y * Data.RotationMatrix[3] * direction));
                    break;
            }

            this.cells[i] = new Vector3Int(x, y, 0);
        }
    }

    //Le funzioni per il controllo del WallKick bloccano la rotazione vicino ad un muro se questa non è valida
    //Anche queste sono complicate e sono incluse già in Data.cs
    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKicksIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < this.data.wallKicks.GetLength(1); i++)
        {

            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                return true;
            }
        }
        return false;
    }

    private int GetWallKicksIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationDirection * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }


    //Funzione di utilità che serve alla rotazione per far rientrare sempre l'input di rotazione fra 1 e 4
    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }

}
