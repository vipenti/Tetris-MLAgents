using System.Collections.Generic;
using System;
using System.Linq;

public class Bag
{
    private Random rng = new Random();
    private List<Tetromino> bag = new List<Tetromino>();

    private List<Tetromino> pieces = new List<Tetromino>
    {
        Tetromino.I,
        Tetromino.J,
        Tetromino.L,
        Tetromino.O,
        Tetromino.S,
        Tetromino.T,
        Tetromino.Z
    };

    public int GetPiece()
    {
        //RefillBag();

        int piece = (int)bag[0];
        bag.RemoveAt(0);

        return piece;
    }

    public int PeekNextPiece()
    {
        //RefillBag();

        return (int)bag[0];
    }

    /*private void RefillBag()
    {
        //if (IsNullOrEmpty())
        {
            bag = new List<Tetromino>(pieces);
            Shuffle(bag);
        }
    }

    public void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static bool IsNullOrEmpty(this IList<T> enumerable)
    {
        if (enumerable == null) return true;

        return !enumerable.Any();
    }*/
}