using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BoardBehavior : MonoBehaviour, IMementoOriginator {

    /// <summary>
    /// Where are the ground blocks on y axis
    /// </summary>
    public const int POS_Y_GROUND = -1;

    public LevelBehavior CurrentLevel { get; private set; }


    public void Show() {

        gameObject.SetActive(true);

        //hide elements outside the board
        foreach (var elem in GetElements()) {
            elem.SetMeshesVisible(IsInsideBoardHorizontalLimits(elem.GridPos));
        }
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Try to load a level with it's number and return true if it was well loaded
    /// </summary>
    public bool LoadNewLevel(DataChapter dataChapter, int levelNumber) {

        if (dataChapter == null) {
            throw new ArgumentException("The board need a chapter to load a level of a chapter");
        }

        if (!dataChapter.HasLevel(levelNumber)) {
            return false;
        }

        var levelPrefab = dataChapter.GetLevelPrefab(levelNumber);
        if (levelPrefab == null) {
            throw new ArgumentException("No level prefab for level " + levelNumber);
        }

        var goLevel = GameObject.Instantiate(levelPrefab, transform);

        CurrentLevel = goLevel.AddComponent<LevelBehavior>();
        CurrentLevel.InitLevel(dataChapter, levelNumber);

        //instanciate characters from pawns
        foreach (Transform tr in goLevel.transform) {

            if (tr.TryGetComponent<CharacterPawnBehavior>(out var pawn)) {
                pawn.InstantiateCharacterGameObject();
            }
        }

        //check if the level can be finished by player
        var elems = GetElements();
        var nbGoals = elems.Count(e => e.TryGetComponent<GoalBehavior>(out _));
        var nbPlayerCharacters = elems.Count(e => e.TryGetComponent<CharacterBehavior>(out var character) && character.IsPlayable);

        if (nbPlayerCharacters <= 0) {
            throw new InvalidOperationException("The level can't be finished because there are no characters");
        }

        if (nbGoals < nbPlayerCharacters) {
            throw new InvalidOperationException("The level can't be finished because there are not enough goals (" + nbGoals + ") for player characters (" + nbPlayerCharacters + ")");
        }

        return true;
    }

    public void UnloadCurrentLevel() {

        if (CurrentLevel == null) {
            return;
        }

        //avoid seeing the level for some millis while it's destroying
        CurrentLevel.gameObject.SetActive(false);
        GameObject.Destroy(CurrentLevel.gameObject);

        CurrentLevel = null;
    }

    public IEnumerable<BaseElementBehavior> GetElements() {
        return CurrentLevel.GetComponentsInChildren<BaseElementBehavior>();
    }

    /// <summary>
    /// Get pile of element at position, sorted from the bottom to the top.
    /// </summary>
    public IEnumerable<BaseElementBehavior> GetSortedPileOfElements(Vector2 horizontalPos) {

        return GetElements().Where(e => e.IsOnGridPile(horizontalPos))
            .OrderBy(e => e.GridPosY + e.ColliderHeight);
    }

    public bool IsInsideBoardHorizontalLimits(Vector3 pos) {
        //if can't fall => is part of the board
        return GetSortedPileOfElements(new Vector2(pos.x, pos.z))
            .Any(e => e.IsPhysicallyWalkableOverBlock && pos.y >= e.GridPosY);
    }

    public bool IsAnyWalkableElementUnderPos(Vector3 pos, bool canWalkOverInvisibleBlocks = false) {
        return GetElemsUnderPos(pos).Any(e => canWalkOverInvisibleBlocks ? e.IsTheoricallyWalkableOverBlock : e.IsPhysicallyWalkableOverBlock);
    }

    public bool IsWalkablePos(Vector3 pos, bool canWalkOnInvisibleBlocks = false) {

        if (GetElemsOnPos(pos).Any(e => !e.IsPhysicallyWalkableInBlock && !e.IsInvisible)) {
            //must not have something on pos to walk on it
            return false;
        }

        //must have something under pos to walk on it
        return IsAnyWalkableElementUnderPos(pos, canWalkOnInvisibleBlocks);
    }

    public bool HasElemOnPos(Vector3 pos) {
        return GetElemsOnPos(pos).Count() > 0;
    }

    public IEnumerable<BaseElementBehavior> GetElemsOnPos(Vector3 pos) {

        var y = (int)pos.y;
        return GetElements()
            .Where(e => e.IsOnGridPile(pos) && e.GridPosY <= y && (e.ColliderHeight <= 0 || y < e.GridPosY + e.ColliderHeight));
    }

    public bool HasElemUnderPos(Vector3 pos) {
        return GetElemsUnderPos(pos).Count() > 0;
    }

    public IEnumerable<BaseElementBehavior> GetElemsUnderPos(Vector3 pos) {
        return GetElemsOnPos(pos + Vector3.down);
    }

    public IEnumerable<BaseElementBehavior> GetSortedElementsAbove(BaseElementBehavior current) {

        var pile = GetSortedPileOfElements(new Vector2(current.GridPosX, current.GridPosZ));

        var y = current.GridPosY;
        return pile.Where(e => e.GridPosY > y);
    }

    public int GetFallHeight(BaseElementBehavior elem, bool canWalkOnInvisibleBlocks = false) {

        var height = 0;
        var y = elem.GridPosY;

        while (y > POS_Y_GROUND && !IsAnyWalkableElementUnderPos(new Vector3(elem.GridPosX, y, elem.GridPosZ), canWalkOnInvisibleBlocks)) {

            y--;
            height++;
        }

        return height;
    }


    public IMementoSnapshot NewSnapshot() {

        var elemsSnapshots = GetElements().Select(e => (MementoSnapshotElement)e.NewSnapshot());
        return new MementoSnapshotBoard(
            elemsSnapshots,
            Game.Instance.aiTeamEnemy.TurnsCount,
            Game.Instance.aiTeamEnemy.LastTurnWithMovement,
            Game.Instance.aiTeamNeutral.TurnsCount,
            Game.Instance.aiTeamNeutral.LastTurnWithMovement
        );
    }

    public void Restore(IMementoSnapshot snapshot) {

        if (snapshot is not MementoSnapshotBoard) {
            throw new ArgumentException("Wrong snapshot type, waiting for a MementoSnapshotBoard");
        }

        var s = ((MementoSnapshotBoard)snapshot);
        s.ProcessElements(
            GetElements(),
            (elem, elemSnapshot) => elem.Restore(elemSnapshot)
        );

        Game.Instance.aiTeamEnemy.RestoreTurnCount(s.aiTeamEnemyTurnCount, s.aiTeamEnemyLastTurnWithMovement);
        Game.Instance.aiTeamNeutral.RestoreTurnCount(s.aiTeamNeutralTurnCount, s.aiTeamNeutralLastTurnWithMovement);
    }

}
