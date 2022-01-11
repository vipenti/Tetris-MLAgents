using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Piece : Agent
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

    //variabili aggiunte
    private ActionSegment<int> act; //salvo i dicreteActionOut in Heuristic per usarli al di fuori della funzione
    private int last_move = 2;
    public void Initialize(Board board, Vector3Int position, PieceGroupsData data)
    {
        this.board = board;
        this.position = position;
        this.data = data;
        this.rotationIndex = 0;

        this.stepTime = Time.time + this.stepDelay;
        this.lockTime = 0f;

        //Se l'array è ancora null, viene inizializzato
        if(this.cells == null) {
            this.cells = new Vector3Int[data.cells.Length];
        }

        //Ogni cellla viene copiata nell'array per com'è nel PieceGroupsData
        for(int i = 0; i < data.cells.Length; i++) {
            this.cells[i] = (Vector3Int)data.cells[i];
        }
    }

    public override void CollectObservations(VectorSensor sensor) //osservo se le celle sono occupate
    {
        for (int col = board.Bounds.xMin; col < board.Bounds.xMax; col++)
        {
            for (int row = board.Bounds.yMin; row < board.Bounds.yMax; row++)
            {
                sensor.AddObservation(board.IsCellOccupied(row, col));
            }
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //spostamento destra, sinistra o non mi muovo affatto
        if (actionBuffers.DiscreteActions[0] == 0)
        {
            //Move(Vector2Int.left);
        }
        else if (actionBuffers.DiscreteActions[0] == 1)
        {
            //Move(Vector2Int.right);
        }
        else
        {
           ;
        }


        //rotazione
        /*if (actionBuffers.DiscreteActions[1] == 0)
        {
            Rotate(1);
        }
        else if (actionBuffers.DiscreteActions[1] == 1)
        {
            Rotate(-1);
        }
        else 
        {
            ;
        }


        //ArrowDown
        if (actionBuffers.DiscreteActions[2] == 0)
        {
            Move(Vector2Int.down);
        }
        else if (actionBuffers.DiscreteActions[2] == 1)
        {
            ;
        }
        else {; }


        //HardDrop
        if (actionBuffers.DiscreteActions[3] == 0)
        {
            HardDrop();
        }
        else if(actionBuffers.DiscreteActions[3] == 1)
        {
            ; 
        }
        else
        {
            ;
        }*/

        //print("boad.IsGameOver = " + board.IsGameOver());
        if (board.IsGameOver() == true)
        {
            //print("game over");
            SetReward(-1f);
            EndEpisode();
        }


       /*if (actionBuffers.DiscreteActions[0] == 0)
       {
           Move(Vector2Int.left);
       }
       else if (actionBuffers.DiscreteActions[0] == 1)
       {
           Move(Vector2Int.right);
       }
       else if (actionBuffers.DiscreteActions[0] == 2)
       {
           Rotate(1);
       }
       else if (actionBuffers.DiscreteActions[0] == 3)
       {
           Rotate(-1);
       }
       else if (actionBuffers.DiscreteActions[0] == 4)
       {
           Move(Vector2Int.down);
       }
       else if (actionBuffers.DiscreteActions[0] == 5)
       {
           HardDrop();
       }
       else if(actionBuffers.DiscreteActions[0] == 6)
       {
           ; 
       }
       else
       {
           ;
       }*/

       //print("boad.IsGameOver = " + board.IsGameOver());
  
    }


    public void Update()
    {
        this.board.Clear(this);

        this.lockTime += Time.deltaTime;
        //qui stavo cercando di fare cose per far funzionare il codice in modalità Heuristic
        if (act[0] == 1)
        {
            Move(Vector2Int.right);
            act[0] = 2;
        }
        /*if (Input.GetKeyDown(KeyCode.Comma)) {
            Rotate(-1);
        }
        else if (Input.GetKeyDown(KeyCode.Period)) {
            Rotate(1);
        }*/

        /*if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            Move(Vector2Int.left);
        }*/
        /*else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            Move(Vector2Int.right);
        }*/
        /*else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            Move(Vector2Int.down);
        }
        else if(Input.GetKeyDown(KeyCode.Space)) {
            HardDrop();
        }*/

        //Movimento automatico del pezzo verso il basso
        if (Time.time >= this.stepTime) {
            Step();
        }

        this.board.Set(this);
    }

    //questa funzione serve a convertire i comandi Input.GetKey in numeri perchè se l'assegnazione di discreteActionOut[0] = Input.GetKeyDown(...)
    //in Heuristic non si può fare.
    public int getActionMoveLeftandRight() 
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            //print("RIGHT ARROW");
            return 1;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            return 0;
        }
        else
            return 2;  
    }

    //stavo cercando di usare destra e sinistra senza gli altri comandi. Last_move = 2 significa che l'agente non deve far nulla e quindi non
    //spamma gli altri tasti che mi è sembrato si accavallano tra loro e succedono cose strane.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
        if (last_move != getActionMoveLeftandRight()) //quindi mi voglio muovere a destra o a sinistra
        {
            discreteActionsOut[0] = getActionMoveLeftandRight();
            print("last move = " + last_move);
            last_move = getActionMoveLeftandRight();
        }
        else
        {
            discreteActionsOut[0] = 2; //non fare niente
        }
        act = discreteActionsOut;
        print("act = " + act[0]);
        /*discreteActionsOut[1] = -1;
        discreteActionsOut[2] = -1;
        discreteActionsOut[3] = -1;*/
    }

    private void Step()
    {
        this.stepTime = Time.time + this.stepDelay;
        Move(Vector2Int.down);

        //Tempo di reazione
        if (this.lockTime >= this.lockDelay) {
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
    private void HardDrop()
    {
        while(Move(Vector2Int.down)) {
            continue;
        }

        Lock();
    }

    private bool Move(Vector2Int direction)
    {
        Vector3Int new_position = this.position;
        new_position.x += direction.x;
        new_position.y += direction.y;
        
        bool valid = board.IsValidPosition(this, new_position);

       if (valid) {
            this.position = new_position;
            this.lockTime = 0f;
       }
        
        return valid;
    }

    
    private void Rotate(int direction)
    {
        int originalRotation = this.rotationIndex;
        this.rotationIndex = Wrap(this.rotationIndex + direction, 0, 4);

        ApplyRotationMatrix(direction);

        //Se la rotazione non è valida, la si risporta alla posizione originale
        if(!TestWallKicks(this.rotationIndex, direction)) {
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

        for(int i = 0; i < this.data.wallKicks.GetLength(1); i++) {

            Vector2Int translation = this.data.wallKicks[wallKickIndex, i];

            if (Move(translation)) {
                return true;
            }
        }
        return false;
    }

    private int GetWallKicksIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationDirection * 2;

        if(rotationDirection < 0) {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, this.data.wallKicks.GetLength(0));
    }


    //Funzione di utilità che serve alla rotazione per far rientrare sempre l'input di rotazione fra 1 e 4
    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        }
        else {
            return min + (input - min) % (max - min);
        }
    }

    public void Reward(float i)
    {
        AddReward(i);
    }
    
}
