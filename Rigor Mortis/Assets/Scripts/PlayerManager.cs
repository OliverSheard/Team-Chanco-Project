﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Character selectedPlayer, selectedEnemy;
    public BlockScript[] walkTiles, sprintTiles;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private UIManager uiManager;

    public List<Character> unitList;
    public List<Character> activeEnemyNecromancers;
    public List<Character> activePlayerNecromancers;

    // Start is called before the first frame update
    void Start()
    {
        unitList = new List<Character>();
        activeEnemyNecromancers = new List<Character>();
        activePlayerNecromancers = new List<Character>();

        GridManager.unitSpawned += (s, e) => { e.characterClicked += (sender, character) => PlayerUnitChosen(e); };
        GridManager.unitSpawned += (s, e) => { e.moveComplete += (sender, character) => gridManager.CycleTurns(); };
        GridManager.unitSpawned += (s, e) => { e.attackComplete += (sender, character) => gridManager.CycleTurns(); };
        GridManager.enemySpawned += (s, e) => { e.unit.characterClicked += (sender, character) => EnemyUnitChosen(e.unit); };
        //BlockScript.blockClicked += (s, e) => BlockClicked(e);
        ChooseAttackButton.pointerExit += (s, e) =>
        {
            //if (selectedPlayer != null)
            //    if(selectedPlayer.selectedAttack == null)
            //    {
            //        HighlightMovementTiles(selectedPlayer);
            //    }
            //    else
            //    {
            //        gridManager.ColourTiles(selectedPlayer.pathfinder.GetTilesInRange(selectedPlayer.floor, selectedPlayer.selectedAttack.Range, true), false);
            //    }
        };
    }

    public void AddUnit(Character unit)
    {
        unitList.Add(unit);
        uiManager.InstantiateUIBars(unit);
        uiManager.InstantiateMarker(unit);
    }

    public void AddNecromancer(Character unit)
    {
        if (unit.tag == "Enemy") {
            activeEnemyNecromancers.Add(unit);
        } else if(unit.tag == "Player") {
            activePlayerNecromancers.Add(unit);
        }
    }

    public void RemoveNecromancer(Character unit)
    {
        if (unit.tag == "Enemy")
        {
            activeEnemyNecromancers.Remove(unit);
        } else if (unit.tag == "Player") {
            activePlayerNecromancers.Remove(unit);
        }
    }

    private void BlockClicked(BlockScript tile)
    {
        //bool unitCanMove = selectedPlayer != null && gridManager.playerTurn;

        //if (unitCanMove && tile.occupier == null && uiManager.attacking == false)
        //{
        //    bool playerCanMove = selectedPlayer.ActionPoints >= 0;
        //    if(playerCanMove)
        //    {
        //        MovePlayerToBlock(tile);
        //    }
        //}
        //else
        //{
        //    if (selectedPlayer != null)
        //    {
        //        //Debug.Log(selectedPlayer.attackManager.attackerAssigned);
        //    }
        //}
    }

    private void MovePlayerToBlock(BlockScript tile)
    {
        bool sprinting = walkTiles.Contains(tile) == false && sprintTiles.Contains(tile);
        if (sprinting)
        {
            selectedPlayer.MoveUnit(selectedPlayer.pathfinder.GetPath(selectedPlayer.floor, (b) => b == tile, selectedPlayer.isFlying == false));
            gridManager.ClearMap();
            if(selectedPlayer.tag =="Player")
            {
                gridManager.nextUnit();
            }
        }
        else if (walkTiles.Contains(tile))
        {
            selectedPlayer.MoveUnit(selectedPlayer.pathfinder.GetPath(selectedPlayer.floor, (b) => b == tile, selectedPlayer.isFlying == false));
            gridManager.ClearMap();
        }
        else
        {
            Debug.Log("Clicked invalid block");
        }
    }

    public void PlayerUnitChosen(Character unit)
    {
        
        if (gridManager.playerTurn && unit.ActionPoints >= 0)
        {
            uiManager.DisplayActionButtons(unit.attacks, unit);            

            if (selectedPlayer != null)
            {
                selectedPlayer.GetComponentInChildren<Renderer>().material.color = Color.white;
                gridManager.ClearMap();
            }

            selectedPlayer = unit;
            selectedPlayer.GetComponentInChildren<Renderer>().material.color = Color.yellow;
            HighlightMovementTiles(unit);
            GetComponent<PlayerCharacterMover>().SetMovement(unit, walkTiles, sprintTiles);
        }
        else
            selectedPlayer = null;
    }

    private void HighlightMovementTiles(Character unit)
    {
        if (unit.MaxAP)
        {
            walkTiles = unit.pathfinder.GetTilesInRange(unit.floor, unit.movementSpeed, unit.isFlying == false).Where(t => t.Occupied == false).ToArray();
            sprintTiles = unit.pathfinder.GetTilesInRange(unit.floor, unit.movementSpeed + unit.movemenSprint, unit.isFlying == false).Where(t => t.Occupied == false).ToArray();
        }
        else if(unit.CanMove)
        {
            sprintTiles = unit.pathfinder.GetTilesInRange(unit.floor, unit.movemenSprint, unit.isFlying == false).Where(t => t.Occupied == false).ToArray();
            walkTiles = new BlockScript[0];
        }
        else
        {
            walkTiles = sprintTiles = new BlockScript[0];
        }

        gridManager.ColourTiles(sprintTiles, false);
        gridManager.ColourTiles(walkTiles, true);
    }

    public void EnemyUnitChosen(Character unit)
    {
        Debug.Log("Enemy clicked");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
