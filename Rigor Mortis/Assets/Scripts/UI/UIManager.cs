﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField]Pathfinder pathFinder;

    [SerializeField] GridManager gridManager;

    [SerializeField]Canvas battleCanvas;
    [SerializeField]Canvas prepCanvas;
    [SerializeField]Canvas fixedCanvas;
    [SerializeField]Canvas pauseCanvas;

    [SerializeField]Text turnDisplay;

    public bool attacking = false;

    [SerializeField] GameObject attackButton, targetCharacterButton, popupArea;
    [SerializeField] private Vector3 baseAttackPosition, targetCharacterOffset;
    public List<GameObject> popUpButtons;

    [SerializeField] Slider healthBar;
    List<Slider> healthBars;
    List<Character> unitList;
    public Vector3 healthBarOffset;

    public BlockScript[] blocksInRange;
    public static EventHandler<GameStates> gameStateChange;

    

    public enum GameStates
    {
        placementPhase, playerTurn, enemyTurn, paused
    }

    // Start is called before the first frame update
    void Start()
    {
        //pathFinder = GetComponent<Pathfinder>();
        //attackManager = GetComponent<AttackManager>();
        //gridManager = GetComponent<GridManager>();
        //attackButton = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/UI/AttackButton.prefab", typeof(GameObject));
        unitList = new List<Character>();
        healthBars = new List<Slider>();
        popUpButtons = new List<GameObject>();

        BuildUnits();

        gameStateChange += GameStateChanged;
        gameStateChange?.Invoke(this, UIManager.GameStates.placementPhase);

        ChooseAttackButton.attackChosen += DisplayTargets;
        Character.attackEvent += ClearAttackUI;
    }

    private void ClearAttackUI(object sender, AttackEventArgs e)
    {
        foreach (var btn in popUpButtons)
        {
            Destroy(btn);
        }
        popUpButtons = new List<GameObject>();
    }    

    private void DisplayTargets(object sender, ChooseAttackButton.CharacterAttack e)
    {
        //Clear the previous target buttons
        var buttonsToRemove = popUpButtons.Where(s => s.GetComponent<ChooseAttackButton>() == null);
        foreach (var btn in buttonsToRemove)
        {
            Destroy(btn);
        }

        popUpButtons.RemoveAll(s => s.GetComponent<ChooseAttackButton>() == null);       

        var targetsInRange = e.attacker.pathfinder.GetTilesInRange(e.attacker.floor, e.attackChosen.Range, true).Where(b => b.Occupied ? b.occupier.tag == "Enemy" : false).Select(t => t.occupier.GetComponent<Character>());
        if(targetsInRange.Count() == 0)
        {
            //Say no people to hit
            var btn = Instantiate(targetCharacterButton, battleCanvas.transform);
            popUpButtons.Add(btn);
            btn.GetComponentInChildren<Text>().text = "No Targets";
        }
        else
        {
            for (int i = 0; i < targetsInRange.Count(); i++)
            {                
                GameObject button = Instantiate(targetCharacterButton, battleCanvas.transform);
                popUpButtons.Add(button);
                button.GetComponentInChildren<Text>().text = targetsInRange.ElementAt(i).name;
                var enemySelect = button.GetComponent<EnemySelectButton>();
                enemySelect.AssignData(e.attacker, targetsInRange.ElementAt(i));
            }
        }
    }

    public void UpdateTurnNumber(int turn)
    {
        turnDisplay.text = "Turn " + turn;
    }

    public void wait()
    {
        gridManager.CycleTurns();
    }

    public void DisplayAttacks(IEnumerable<Attack> _attacks, Character character)
    {
        foreach (GameObject button in popUpButtons)
        {
            Destroy(button);            
        }
        popUpButtons = new List<GameObject>();
        
        if(character.CanAttack)
        {
            for (int i = 0; i < _attacks.Count(); i++)
            {
                Vector3 popUpOffset = new Vector3(0, 30 * i, 0);

                GameObject button = Instantiate(attackButton, popupArea.transform);
                //button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                //button.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
                button.transform.localPosition = baseAttackPosition + popUpOffset;
                popUpButtons.Add(button);

                button.GetComponent<ChooseAttackButton>().character = character;
                button.GetComponent<ChooseAttackButton>().gridManager = gridManager;
                button.GetComponent<ChooseAttackButton>().attack = _attacks.ElementAt(i);
                button.GetComponentInChildren<Text>().text = _attacks.ElementAt(i).Name;
            }

            var moveOffset = new Vector3(0, 30 * (_attacks.Count() + 1), 0);
            var moveBtn = Instantiate(attackButton, popupArea.transform);
            moveBtn.transform.localPosition = baseAttackPosition + moveOffset;
        }  
        else if(character.CanMove)
        {
            var moveBtn = Instantiate(attackButton, popupArea.transform);
            moveBtn.transform.localPosition = baseAttackPosition;
        }
        else
        {
            //No actions can be taken add button to represent this
        }
    }

    public void ClearRangeBlocks()
    {
        foreach (var block in blocksInRange)
        {
            block.GetComponent<Renderer>().material.color = block.Normal;
        }
    }


    //Health Bars
    void BuildUnits() {
        foreach (Character unit in FindObjectsOfType<Character>())
        {
            unitList.Add(unit);
            InstantiateHealthBar(unit);
        }
    }

    void InstantiateHealthBar(Character unit) {
        Slider newSlider = Instantiate(healthBar, unit.transform.position + healthBarOffset, fixedCanvas.transform.rotation, fixedCanvas.transform);
        healthBars.Add(newSlider);
        unit.gameObject.AddComponent<HealthBar>().unit = unit;
        unit.gameObject.GetComponent<HealthBar>().slider = newSlider;
        unit.gameObject.GetComponent<HealthBar>().offset = healthBarOffset;
    }

    public void AddUnit(Character newUnit) {
        unitList.Add(newUnit);
        InstantiateHealthBar(newUnit);
    }


    // Unit Assignment
    public void setUnit(Character unit) {
        gridManager.SetSelectedUnit(unit);
    }


    //Canvas
    private void GameStateChanged(object sender, GameStates state) {
        switch(state)
        {
            case GameStates.placementPhase:
                SetPrepCanvas(true);
                SetBattleCanvas(false);
                SetFixedCanvas(false);
                SetPauseCanvas(false);
                break;

            case GameStates.playerTurn:
                SetPrepCanvas(false);
                SetBattleCanvas(true);
                SetFixedCanvas(true);
                SetPauseCanvas(false);
                break;

            case GameStates.enemyTurn:
                SetPrepCanvas(false);
                SetBattleCanvas(false);
                SetFixedCanvas(true);
                SetPauseCanvas(false);
                break;

            case GameStates.paused:
                SetPrepCanvas(false);
                SetBattleCanvas(false);
                SetFixedCanvas(false);
                SetPauseCanvas(true);
                break;
        }
    }


    public void SetPrepCanvas(bool enabled) {
        prepCanvas.enabled = enabled;
    }

    public void SetBattleCanvas(bool enabled) {
        battleCanvas.enabled = enabled;
    }

    public void SetFixedCanvas(bool enabled) {
        fixedCanvas.enabled = enabled;
    }

    public void SetPauseCanvas(bool enabled) {
        pauseCanvas.enabled = enabled;
    }
}
