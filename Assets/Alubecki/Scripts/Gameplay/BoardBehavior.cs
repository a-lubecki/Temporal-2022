﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BoardBehavior : MonoBehaviour {

    /// <summary>
    /// Where are the ground blocks on y axis
    /// </summary>
    public const int POS_Y_GROUND = -1;

    public LevelBehavior CurrentLevel { get; private set; }


    public void Show() {
        gameObject.SetActive(true);
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

        var goLevel = GameObject.Instantiate(dataChapter.GetLevelPrefab(levelNumber), transform);

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

        GameObject.Destroy(CurrentLevel);

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
            .OrderBy(e => e.GridPosY);
    }

    public bool IsInsideBoardHorizontalLimits(Vector2 horizontalPos) {
        return GetSortedPileOfElements(horizontalPos).Count() > 0;
    }

    public bool IsInsideBoardHorizontalLimits(Vector3 pos) {
        return IsInsideBoardHorizontalLimits(new Vector2(pos.x, pos.z));
    }

    public bool IsWalkablePos(Vector3 pos) {

        if (GetElemsOnPos(pos).Any(e => e.ColliderHeight > 0)) {
            //must not have something on pos to walk on it
            return false;
        }

        //must have something under pos to walk on it
        return GetElemsUnderPos(pos).Any(e => e.IsWalkableOn);
    }

    public bool HasElemOnPos(Vector3 pos) {
        return GetElemsOnPos(pos).Count() > 0;
    }

    public IEnumerable<BaseElementBehavior> GetElemsOnPos(Vector3 pos) {

        var y = (int)pos.y;
        return GetElements().Where(e => e.IsOnGridPile(pos) && e.GridPosY <= y && y < e.GridPosY + e.ColliderHeight);
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

    public int GetFallHeight(BaseElementBehavior elem) {

        var height = 0;
        var y = elem.GridPosY;

        while (y > POS_Y_GROUND && !HasElemUnderPos(new Vector3(elem.GridPosX, y, elem.GridPosZ))) {

            y--;
            height++;
        }

        return height;
    }

    public IEnumerable<BaseElementBehavior> GetAboutToFallElements() {
        return GetElements().Where(e => e.CanFall && !HasElemUnderPos(e.GridPos));
    }
}