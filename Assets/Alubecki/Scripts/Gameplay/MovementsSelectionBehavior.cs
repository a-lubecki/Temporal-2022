using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovementsSelectionBehavior : MonoBehaviour {


    public BaseMovement NextMovement { get; private set; }

    public bool IsNextMovementSelected => NextMovement != null;


    public void ValidateNextMovement(BaseMovement movement) {

        if (movement == null) {
            throw new ArgumentException();
        }

        NextMovement = movement;
    }

    public void ClearNextMovement() {
        NextMovement = null;
    }

    public void UpdateIndicationsAndActionButtons() {

        if (Game.Instance.gameManager.IsElementSelected) {
            ShowIndicationsAndActionButtons();
        } else {
            HideIndicationsAndActionButtons();
        }
    }

    public void ShowIndicationsAndActionButtons() {

        var selectedElem = Game.Instance.gameManager.SelectedElement;

        var possibleMovements = selectedElem.GetPossibleMovements();
        if (possibleMovements == null) {
            //no indication of buttons to show
            return;
        }

        //create movements which will be added in indications and in action buttons
        var movements = new List<BaseMovement>();

        foreach (var pm in possibleMovements) {

            var targets = pm.GetNextPossibleMovementTargets(selectedElem);
            if (targets == null) {
                //can't show if no target positions
                continue;
            }

            //for each possible pos, create a new movement
            foreach (var nextPos in targets) {
                movements.Add(pm.NewMovement(selectedElem, nextPos));
            }
        }

        //group infos by pos (used for displaying groups of infos pos by pos)
        var infoByPos = new Dictionary<Vector3, List<DisplayableMovementInfo>>();

        foreach (var m in movements) {

            var infos = m.NewDisplayableMovementInfos();
            if (infos == null) {
                //can't show infos
                continue;
            }

            foreach (var info in infos) {

                List<DisplayableMovementInfo> infoList;

                if (!infoByPos.ContainsKey(info.Pos)) {
                    infoList = new List<DisplayableMovementInfo>();
                    infoByPos.Add(info.Pos, infoList);
                } else {
                    infoList = infoByPos[info.Pos];
                }

                infoList.AddRange(infos);
            }
        }

        //for each pos, display an indication square/block or an action buttons group
        foreach (var e in infoByPos) {

            var pos = e.Key;

            //generate squares or blocks on the pos
            var infoIndications = e.Value.Where(info => info.Display == MovementDisplay.SQUARE || info.Display == MovementDisplay.BLOCK);
            foreach (var info in infoIndications) {

                var orientation = OrientationFunctions.FindOrientation(selectedElem.GridPos, pos);
                Game.Instance.indicationsSpawnerBehavior.SpawnMovementIndication(info);
            }

            //generate one button group with 1 or 2 buttons on the pos
            var infoButtons = e.Value.Where(info => info.Display == MovementDisplay.BUTTON &&
                info.Movement.MovementType == MovementType.ACTION_ONE_TIME || info.Movement.MovementType == MovementType.ACTION_CHANGE_STATE);

            if (infoButtons.Count() > 0) {

                Game.Instance.actionButtonsSpawnerBehavior.SpawnActionButton(
                    pos,
                    infoButtons.FirstOrDefault(info => info.Movement.MovementType == MovementType.ACTION_ONE_TIME)?.Movement,
                    infoButtons.FirstOrDefault(info => info.Movement.MovementType == MovementType.ACTION_CHANGE_STATE)?.Movement
                );
            }
        }

    }

    public void HideIndicationsAndActionButtons() {

        Game.Instance.indicationsSpawnerBehavior.DespawnAllMovementIndications();

        Game.Instance.actionButtonsSpawnerBehavior.DespawnAllActionButtons();
    }

}
