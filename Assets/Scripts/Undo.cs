using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UndoEntry
{
    public SliceType Face;
    public SliceType SliceType;
    public int SliceID;
    public FaceEdgeTypeDetails SwipDir;
    public bool IsRandomMove;
    public bool Clockwise;
}

public struct UndoState
{
    public List<UndoEntry> entries;
    public int CurrentPlayIndex;
}

public static class Undo
{

    public static UndoState Undos;

    public static void InitUndo()
    {
        Undo.Undos = new UndoState();
        Undo.Undos.entries = new List<UndoEntry>();
    }
    public static void RecordMove(UndoEntry entry)
    {
        Undos.entries.Insert(Undos.CurrentPlayIndex, entry);
        Undos.CurrentPlayIndex++;
    }

    public static SliceData RewindMove()
    {
        SliceData results = new SliceData();
        if (Undos.entries.Count > 0 && Undos.CurrentPlayIndex >= 1)
        {
            UndoEntry e = Undos.entries[Undos.CurrentPlayIndex - 1];
            results = Game.GetSlice(e.SliceType, e.SliceID,Cube.CubeDim);
            results.Clockwise = !e.Clockwise;
            Undos.CurrentPlayIndex--;
            if (Undos.CurrentPlayIndex < 0) { Undos.CurrentPlayIndex = 0; }
        }
        return results;
    }

}
