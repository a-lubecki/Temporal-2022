using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// https://refactoring.guru/fr/design-patterns/memento
/// </summary>
public class MementoCaretaker : MonoBehaviour {


    [SerializeField] GameObject goOriginator;

    IMementoOriginator originator;
    List<IMementoSnapshot> history = new List<IMementoSnapshot>();
    int cursor = -1;


    void Awake() {

        if (goOriginator != null) {
            originator = goOriginator.GetComponent<IMementoOriginator>();
        }

        if (originator == null) {
            throw new InvalidOperationException("Missing memento originator in caretaker");
        }
    }

    public void Reset() {

        history.Clear();
        cursor = -1;
    }

    void ClearHistoryAfterCursor() {

        if (history.Count > 0 && cursor < history.Count - 1) {
            history.RemoveRange(cursor + 1, history.Count - 1 - cursor);
        }
    }

    public void SaveCurrentState() {

        //clear history after cursor (if player did undo, snapshots after cursor are cast)
        ClearHistoryAfterCursor();

        history.Add(originator.NewSnapshot());
        cursor++;

        Debug.Log("save : " + (cursor+1) + "/" + history.Count);
    }

    public bool RestoreExistingState(IMementoSnapshot snapshot) {

        var pos = history.IndexOf(snapshot);
        if (pos < 0) {
            Debug.LogWarning("Can't restore state");
            return false;
        }

        cursor = pos;

        ClearHistoryAfterCursor();

        originator.Restore(history[cursor]);

        return true;
    }

    public bool Undo() {

        if (history.Count <= 0) {
            return false;
        }

        cursor--;

        if (cursor <= 0) {
            cursor = 0;
            return false;
        }

        originator.Restore(history[cursor]);

        return true;
    }

    public bool Redo() {

        if (history.Count <= 0) {
            return false;
        }

        cursor++;

        if (cursor >= history.Count) {
            cursor = history.Count - 1;
            return false;
        }

        originator.Restore(history[cursor]);

        return true;
    }

    public IMementoSnapshot GetCurrentSnapshot() {

        if (history.Count <= 0) {
            return null;
        }

        return history[cursor];
    }

}
