using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AITeamBehavior : MonoBehaviour {


    List<AINPCBehavior> orderedNPCs;


    [SerializeField] Team team;
    [SerializeField] float maxMovementsPerTurn = 1;//if 0.5f => 1 movement every 2 turn
    [SerializeField] int maxMovementsPerCharacter = 1;

    int turnsCount;
    int lastTurnWithMovement;


    public void InitAITeam(DataAITeam aiTeam) {

        if (aiTeam == null) {
            return;
        }

        maxMovementsPerTurn = aiTeam.MaxMovementsPerTurn;
        maxMovementsPerCharacter = aiTeam.MaxMovementsPerCharacter;
    }

    public void ComputeNextNPCsMovements() {

        if (maxMovementsPerTurn <= 0) {
            throw new InvalidOperationException("Can't prepare 0 movements for turns");
        }
        if (maxMovementsPerCharacter < 1) {
            throw new InvalidOperationException("Can't prepare less than 1 movement for characters");
        }

        turnsCount++;

        orderedNPCs = Game.Instance.boardBehavior.GetElements()
            .OfType<CharacterBehavior>()
            .Where(c => c.Team == team && c.TryGetComponent<AINPCBehavior>(out _))
            .Select(c => c.GetComponent<AINPCBehavior>())
            .OrderBy(npc => npc.GetPriority())
            .ToList();

        foreach (var npc in orderedNPCs) {
            npc.ResetNextMovements();
        }

        if (orderedNPCs.Count() <= 0) {
            return;
        }

        //manage movements every X turns
        if (maxMovementsPerTurn <= 1) {

            //if max per turn is 0.5 : 1/0.5 = 2 : 1 movement every 2 turns
            var turnsFrequency = 1 / maxMovementsPerTurn;
            if (turnsCount < lastTurnWithMovement + turnsFrequency) {
                //wait next turn
                return;
            }
        }

        var movementCount = 0;
        var loopCount = 0;
        var cursor = 0;

        var maxMovementForThisTurn = (maxMovementsPerTurn >= 1) ? (int)Mathf.Floor(maxMovementsPerTurn) : 1;

        //distribute movements equally for each npc
        while (movementCount < maxMovementForThisTurn) {

            orderedNPCs[cursor].PrepareNextBestMovement();
            movementCount++;
            lastTurnWithMovement = turnsCount;

            cursor++;

            //loop when the cursor reched the end of the npc list
            if (cursor >= orderedNPCs.Count) {

                loopCount++;

                if (loopCount >= maxMovementsPerCharacter) {
                    //finished
                    break;
                }

                cursor = 0;
            }
        }
    }

    public BaseMovement AdvanceToNextAvailableNPCMovement() {

        if (orderedNPCs == null) {
            //not intialized yet
            return null;
        }

        BaseMovement movement = null;

        while (orderedNPCs.Count > 0 && movement == null) {

            var npc = orderedNPCs.First();
            movement = npc.ExtractNextPreparedMovement();

            if (movement != null) {
                //found 1 movement
                return movement;
            }

            //no movement found for this npc, advance
            orderedNPCs.RemoveAt(0);
        }

        //no movement found
        return null;
    }

    public void ResetNPCAttackFlags() {

        foreach (var e in Game.Instance.boardBehavior.GetElements()) {

            if (e.TryGetComponent<AINPCBehavior>(out var npc)) {
                npc.ResetAttackFlag();
            }
        }
    }

    public AINPCBehavior GetRemainingNPCWithAttack() {

        return Game.Instance.boardBehavior.GetElements()
            .OfType<CharacterBehavior>()
            .Where(c => c.Team == team && c.TryGetComponent<AINPCBehavior>(out var npc) && npc.CanAttack())
            .Select(c => c.GetComponent<AINPCBehavior>())
            .FirstOrDefault();
    }

    public void DisplayNPCMovementIndications() {

        //TODO
    }

}
