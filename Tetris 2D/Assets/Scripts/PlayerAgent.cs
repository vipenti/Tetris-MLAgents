using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    private ActionSegment<int> actions;
    public Board board;
    public Piece piece { get; private set; }
    private ActionSegment<int> act; //salvo i dicreteActionOut in Heuristic per usarli al di fuori della funzione
    private int last_move = 2;
    private int last_rotate = 2;
    private int last_hardt = 1;
    private int last_down = 1;

    private Vector3Int new_position;

    // Start is called before the first frame update
    void Start()
    {
        ;
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
        sensor.AddObservation(board.clearedLines);
        sensor.AddObservation(board.holes);
        sensor.AddObservation(board.bumpiness);
        sensor.AddObservation(board.height);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        if (act[0] == 1)
        {
            board.DecidedMove(0, 1);
            act[0] = 2;
        }
        if (act[0] == 0)
        {
            board.DecidedMove(0, 0);
            act[0] = 2;
        }
        if (act[0] == 2)
        {
            board.DecidedMove(0, 2);
        }
        //abbiamo premuto Comma
        if (act[1] == 0)
        {
            board.DecidedRotate(1, 0);
            act[1] = 2;
        }
        //abbiamo premuto Period
        if (act[1] == 1)
        {
            board.DecidedRotate(1, 1);
            act[1] = 2;
        }
        if (act[1] == 2)
        {
            board.DecidedRotate(1, 2);
        }

        if (act[2] == 0)
        {
            board.DecidedHardDrop(2, 0);
            act[2] = 1;
        }
        if (act[2] == 1)
        {
            board.DecidedHardDrop(2, 1);
        }

        if (act[3] == 0)
        {
            board.DecidedMoveDown(3, 0);
            act[3] = 1;
        }
        if (act[3] == 1)
        {
            board.DecidedMoveDown(3, 1);
        }

    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    //questa funzione serve a convertire i comandi Input.GetKey in numeri perchè se l'assegnazione di discreteActionOut[0] = Input.GetKeyDown(...)
    //in Heuristic non si può fare.
    public int getActionMoveLeftandRight()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            return 0;
        }
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            return 1;
        }
        else
            return 2;
    }

    public int getActionRotate()
    {
        if (Input.GetKey(KeyCode.Comma))
        {
            return 0;
        }
        else if (Input.GetKey(KeyCode.Period))
        {
            return 1;
        }
        else
            return 2;
    }

    public int getActionHardTrop()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            return 0;
        }
        else
            return 1;
    }

    public int getActionDown()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            return 0;
        }
        else
            return 1;
    }

    //stavo cercando di usare destra e sinistra senza gli altri comandi. Last_move = 2 significa che l'agente non deve far nulla e quindi non
    //spamma gli altri tasti che mi è sembrato si accavallano tra loro e succedono cose strane.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
        if (last_move != getActionMoveLeftandRight()) //mi voglio muovere a destra o a sinistra
        {
            discreteActionsOut[0] = getActionMoveLeftandRight();
            last_move = getActionMoveLeftandRight();
        }
        else
        {
            discreteActionsOut[0] = 2; //non fare niente
        }

        if (last_rotate != getActionRotate())
        {
            discreteActionsOut[1] = getActionRotate();
            last_rotate = getActionRotate();
        }
        else
        {
            discreteActionsOut[1] = 2; //non fare niente
        }

        if (last_hardt != getActionHardTrop())
        {
            discreteActionsOut[2] = getActionHardTrop();
            last_hardt = getActionHardTrop();
        }
        else
        {
            discreteActionsOut[2] = 1; //non fare niente
        }

        if (last_down != getActionDown())
        {
            discreteActionsOut[3] = getActionDown();
            last_down = getActionDown();
        }
        else
        {
            discreteActionsOut[3] = 1; //non fare niente
        }

        //act = discreteActionsOut; //salviamo l'array degli actionbuffer per poterla utilizzare anche nel metodo Update per eseguire i comandi che corrispondono al contenuto del buffer

    }

    // Update is called once per frame
    void Update()
    {
        ;
    }


    

}
