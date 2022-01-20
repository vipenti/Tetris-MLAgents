using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using System;
using System.Collections.Generic;

public class TetrisAgent : Agent
{
    public Board board;
    public BehaviorParameters behaviourParameters;

    private int lastpressed = 0;
    public int spawn { get; private set; }
    public int rotation { get; private set; }

    public float reward { get; set; }

    public List<float> states;


    public override void Initialize()
    {
        behaviourParameters = gameObject.GetComponent<BehaviorParameters>();
        board = FindObjectOfType<Board>();
        reward = 0;

        states = new List<float>();

        Academy.Instance.AutomaticSteppingEnabled = false;

        //board.SetAgent(this);
        Debug.Log("Setted agent for " + this.board.GetInstanceID());
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("STEP: Beginning episode");
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("STEP: OnActionReceived...");

        var move = actions.DiscreteActions[0];
        var rotation = actions.DiscreteActions[1];

        Debug.Log("STEP: Decision: " + actions.DiscreteActions[0]);
        Debug.Log("STEP: Received piece: " + board.GetPiece().GetInstanceID());

        PositionPiece(move, rotation);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(states);

        string obs = "";

        foreach(float state in states)
        {
            obs += state + " ";
        }

        Debug.Log("OBSERVED: " + obs);

        Debug.Log("States size: " + states.Count);

        /*float bump = board.Bumpiness();
        float holes = board.Holes();
        float height = board.AggregateHeight();
        float lines = board.clearedLines;

        Debug.Log("Observed: lines= " + lines + " bump = " + " " + bump + " height= " + height + " holes= " + holes);

        sensor.AddObservation(lines);
        sensor.AddObservation(bump);
        sensor.AddObservation(height);
        sensor.AddObservation(holes);


        Debug.Log("Observation size: " + sensor.ObservationSize() + " shape: " + sensor.GetObservationShape());

        float rew = FitnessFunction(height, lines, holes, bump) / 100;
        /*AddReward(rew);
        reward += rew;*/

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.DiscreteActions;        

        if (Input.GetKey(KeyCode.Keypad0) && lastpressed!= 1)
        {
            actions[0] = 0;
            lastpressed = 1;
        }
        else if (Input.GetKey(KeyCode.Keypad1) && lastpressed != 2)
        {
            actions[0] = 1;
            lastpressed = 2;
        }

        else if(Input.GetKey(KeyCode.Keypad2) && lastpressed != 3)
        {
            actions[0] = 2;
            lastpressed = 3;
        }
        else if(Input.GetKey(KeyCode.Keypad3) && lastpressed != 4)
        {
            actions[0] = 3;
            lastpressed = 4;
        }
        else if(Input.GetKey(KeyCode.Keypad4) && lastpressed != 5)
        {
            actions[0] = 4;
            lastpressed = 5;
        }
        else if(Input.GetKey(KeyCode.Keypad5) && lastpressed != 6)
        {
            actions[0] = 5;
            lastpressed = 6;
        }
        else if (Input.GetKey(KeyCode.Keypad6) && lastpressed != 7)
        {
            actions[0] = 6;
            lastpressed = 7;
        }
        else if (Input.GetKey(KeyCode.Keypad7) && lastpressed != 8)
        {
            actions[0] = 7;
            lastpressed = 8;
        }


        if (Input.GetKey(KeyCode.UpArrow) && lastpressed != 9)
        {
            actions[1] = 0;
            lastpressed = 9;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && lastpressed != 10)
        {
            actions[1] = 1;
            lastpressed = 10;
        }

        else if (Input.GetKey(KeyCode.LeftArrow) && lastpressed != 11)
        {
            actions[1] = 2;
            lastpressed = 11;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && lastpressed != 12)
        {
            actions[1] = 3;
            lastpressed = 12;
        }

        else
        {
            lastpressed = 0;
        }

    }
    public void PositionPiece(int move, int rotate)
    {      

        switch(move)
        {
            case 0:
                spawn = -4;
                break;

            case 1:
                spawn = -3;
                break;

            case 2:
                spawn = -2;
                break;

            case 3:
                spawn = -1;
                break;

            case 4:
                spawn = 0;
                break;

            case 5:
                spawn = 1;
                break;

            case 6:
                spawn = 2;
                break;

            case 7:
                spawn = 3;
                break;

            default:
                break;
        }

        switch (rotate)
        {
            case 0:
                rotation = 1;
                break;

            case 1:
                rotation = 2;
                break;

            case 2:
                rotation = 3;
                break;

            case 3:
                rotation = 4;
                break;
        }

    }

    /*private void CalculateMove(int decision)
    {
        switch (decision)
        {
            case 0:
                spawn = -4;
                rotation = 1;
                break;

            case 1:
                spawn = -4;
                rotation = 2;
                break;

            case 2:
                spawn = -4;
                rotation = 3;
                break;

            case 3:
                spawn = -4;
                rotation = 4;
                break;

            case 4:
                spawn = -3;
                rotation = 1;
                break;

            case 5:
                spawn = -3;
                rotation = 2;
                break;

            case 6:
                spawn = -3;
                rotation = 3;
                break;

            case 7:
                spawn = -3;
                rotation = 4;
                break;

            case 8:
                spawn = -2;
                rotation = 1;
                break;

            case 9:
                spawn = -2;
                rotation = 2;
                break;

            case 10:
                spawn = -2;
                rotation = 3;
                break;

            case 11:
                spawn = -2;
                rotation = 4;
                break;

            case 12:
                spawn = -1;
                rotation = 1;
                break;

            case 13:
                spawn = -1;
                rotation = 2;
                break;

            case 14:
                spawn = -1;
                rotation = 3;
                break;

            case 15:
                spawn = -1;
                rotation = 4;
                break;

            case 16:
                spawn = 0;
                rotation = 1;
                break;

            case 17:
                spawn = 0;
                rotation = 2;
                break;

            case 18:
                spawn = 0;
                rotation = 3;
                break;

            case 19:
                spawn = 0;
                rotation = 4;
                break;

            case 20:
                spawn = 1;
                rotation = 1;
                break;

            case 21:
                spawn = 1;
                rotation = 2;
                break;

            case 22:
                spawn = 1;
                rotation = 3;
                break;

            case 23:
                spawn = 1;
                rotation = 4;
                break;
            case 24:
                spawn = 2;
                rotation = 1;
                break;

            case 25:
                spawn = 2;
                rotation = 2;
                break;

            case 26:
                spawn = 2;
                rotation = 3;
                break;

            case 27:
                spawn = 2;
                rotation = 4;
                break;
            case 28:
                spawn = 3;
                rotation = 1;
                break;

            case 29:
                spawn = 3;
                rotation = 2;
                break;

            case 30:
                spawn = 3;
                rotation = 3;
                break;

            case 31:
                spawn = 3;
                rotation = 4;
                break;

        }


    }*/

    public float FitnessFunction(float height, float lines, float holes, float bumpiness)
    {
        const float a = -0.51f;
        const float b = 0.76f;
        const float c = -0.36f;
        const float d = -0.18f;

        return (a * height) + (b * lines) + (c * holes) + (d * bumpiness);
    }

    public void AgentStep()
    {
        Academy.Instance.EnvironmentStep();
        Debug.Log("STEP: Stepping...");
    }

    public void UpdateStats()
    {
        Academy.Instance.StatsRecorder.Add("Score", board.points);
        Academy.Instance.StatsRecorder.Add("Lines", board.clearedLines);
        Academy.Instance.StatsRecorder.Add("Bumpiness", board.bumpiness);
        Academy.Instance.StatsRecorder.Add("Holes", board.holes);
        Academy.Instance.StatsRecorder.Add("Height", board.height);
        
    }

    public void AddLineReward(List<int> row)
    {
        row.Sort();
        float multiplier = (20 - row[0]) / 5f;
        float reward = Mathf.Pow(row.Count, 2) * 10 * multiplier;
        AddReward(reward);

        this.reward += reward;
        Debug.Log("Reward: cleared" + row.Count + " lines: "+ reward);
    }


}
