﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Actors;
using Assets.Scripts.MainManagers;
using Assets.Scripts.Tiles;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Dialogue
{
    public class DialogueManager : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField]private Sprite[] _portraitsBunny, _portraitsBadger, _portraitsBeaver, _portraitsMouse, _portraitsInjuredMouse, _portraitsPig, _portraitsWeasel, _portraitsRats, _portraitsHedgehog;
        [SerializeField]private Animator _actor0, _actor1, _textbox, _managerAnimator;
        [SerializeField]private DialogueCreatorManager _manager;
        [SerializeField] private Line[] _lines = new Line[1000];
        [SerializeField]private int _currentPage, _maxPage;
        [SerializeField]private Text _content;
        [SerializeField] private GameObject _actorName, _choisesTransform;
        [SerializeField]private Line _currentLine;
        [SerializeField] private GameObject _choisePrefab;

        [SerializeField] private List<GameObject> _choises;
        [SerializeField] private int _currentBranch;
        private Animator _currentActor;
        private bool _specialsOpen;
          // Use this for initialization
        private void Awake()
        {
            _portraitsBunny = Resources.LoadAll<Sprite>("Portraits/Bunny/");
            _portraitsBadger = Resources.LoadAll<Sprite>("Portraits/Badger/");
            _portraitsBeaver = Resources.LoadAll<Sprite>("Portraits/Beaver/");
            _portraitsMouse = Resources.LoadAll<Sprite>("Portraits/Mouse/");
            _portraitsInjuredMouse = Resources.LoadAll<Sprite>("Portraits/InjuredMouse/");
            _portraitsPig = Resources.LoadAll<Sprite>("Portraits/Pig/");
            _portraitsWeasel = Resources.LoadAll<Sprite>("Portraits/Weasel/");
            _portraitsRats = Resources.LoadAll<Sprite>("Portraits/Rats/");
            _portraitsHedgehog = Resources.LoadAll<Sprite>("Portraits/Hedgehog/");


            _manager = GameObject.FindObjectOfType<DialogueCreatorManager>();
            _content = GameObject.FindGameObjectWithTag("Content").GetComponent<Text>();
            _actorName = GameObject.FindGameObjectWithTag("ActorName");
            _choisesTransform = GameObject.FindGameObjectWithTag("Choises");
       
        }

        //Keyboard Input
        private void Update()
        {
            if (!_managerAnimator.GetBool("Open") || !Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.KeypadEnter)) return;
            DetermineNextLine(_lines[_currentPage]);
        }

        //Mouse Input
        public void OnPointerDown(PointerEventData eventData)
        {
            DetermineNextLine(_lines[_currentPage]);
        }

        //When a dialogue is triggered disable the trigger and open the dialogue
        public void DialogueTrigger(Tile tile, string filename)
        {
            OpenDialogue(filename);
            var container = GameManager.Instance.ReturnMap();
            
            tile.SetDialogue(false);
            tile.ShowFlags();
            foreach (List<Tile> row in container)
                for (var y = 0; y < container.Count; y++)
                {
                    if (!row[y].IsDialogue() || row[y].ReturnPuzzleNumber() != tile.ReturnPuzzleNumber()) continue;
                    row[y].SetDialogue(false);
                    row[y].ShowFlags();
                }
        }

        

        //Opens the dialogue TEMP - for dialogue creator use only!
        public void OpenCreatorDialogue()
        {
            _lines = _manager.ReturnLines().ToArray();
            _maxPage = _lines.Length;
            if (_currentPage < _maxPage)
            {
                _managerAnimator.SetBool("Open", true);
                _textbox.SetBool("Open", true);
               
                _currentLine = _lines[_currentPage];
                SetActor(_currentLine);
                SetContent(_currentLine);
            }
            else
                CloseDialogue();
        }

        //Opens the dialogue TEMP - for dialogue creator use only!
        public void OpenDialogue(string filename)
        {
            //Pause movement
            Time.timeScale = 0;

            //Load map level
            var container = DialogueSaveLoad.LoadFromResources(filename);

            if (container == null) return;
            for (var x = 0; x < container.Size; x++)
            {
                var line = new Line
                {
                    Actor = container.Lines[x].ActorName,
                    ActorExpression = container.Lines[x].ActorExpression,
                    Direction = container.Lines[x].Direction,
                    Branch = container.Lines[x].Branch,
                    Condition = container.Lines[x].Condition,
                    Content = container.Lines[x].Content,
                    Special = container.Lines[x].Special,
                    Choise0 = container.Lines[x].Choise0,
                    Choise1 = container.Lines[x].Choise1,
                    Choise2 = container.Lines[x].Choise2,
                    KillOnExit = container.Lines[x].KillOnExit

                };
                _lines[x] = line;
            }
            _maxPage = _lines.Length;
            if (_currentPage < _maxPage)
            {
                _maxPage = container.Size;
                _currentPage = 0;
                Debug.Log("OPEN");
                _managerAnimator.SetBool("Open", true);
                _textbox.SetBool("Open", true);

                _currentLine = _lines[_currentPage];
                SetActor(_currentLine);
                SetContent(_currentLine);
            }
            else
                CloseDialogue();
        }



        //Close the dialogue screen
        private void CloseDialogue()
        {
            Time.timeScale = 1;
            _currentPage = 0;
           _managerAnimator.SetBool("Open", false);
            _textbox.SetBool("Open", false);
            _actor0.SetBool("Open", false);
            _actor1.SetBool("Open", false);
        }
      
        //Set the apropriate actors spite and position
        private void SetActor(Line line)
        {
            //Left or right
            var direction = line.Direction;
            //Activate correct actor
            _currentActor = direction == 0 ? _actor0 : _actor1;
            //Assign correct sprite according to actor and expression chosen
            var sprite = DetermineActor(line.Actor, line.ActorExpression);
            _currentActor.GetComponent<Image>().sprite = sprite;
            _currentActor.SetBool("Open", true);

            //Set the actors name to what the actor is. If its the injured mouse just call him mouse, its be cruel otherwise.
            _actorName.GetComponentInChildren<Text>().text = line.Actor == Actor.InjuredMouse ? "Mouse" : line.Actor.ToString();
            _actorName.transform.localPosition = direction == 0 ? new Vector3(-750, _actorName.transform.localPosition.y, _actorName.transform.localPosition.z) : new Vector3(Mathf.Abs(_actorName.transform.localPosition.x), _actorName.transform.localPosition.y, _actorName.transform.localPosition.z);
          
        }

        //Load the next line of dialogue
        private void NextLine()
        {
            //Destroy previous choises
            foreach (var choise in _choises)
            {
                Destroy(choise.gameObject);
            }
            _choises.Clear();

            //If it was set to dissapear the actor will do so. However nothing will occur if the same side is used for the next line.
            if (_lines[_currentPage].KillOnExit)
            {
                _currentActor.SetBool("Open", false);
            }

                //Increment the current page
                _currentPage++;
         
            //If the page was the last page close the dialogue otherwise carry on
            if (_currentPage < _maxPage)
            {
                //Check if the next dialogue suits the branch we are on or is main dialogue - if not skip it
                if (_lines[_currentPage].Branch == _currentBranch || _lines[_currentPage].Branch == 0)
                {
                    _currentLine = _lines[_currentPage];
                    SetActor(_currentLine);
                    SetContent(_currentLine);
                }
                else
                {
                    //This skips the line
                    NextLine();
                }
            }
            else
                CloseDialogue();
        }
   

        //Set the content
        private void SetContent(Line line)
        {
            _content.text = line.Content;
        }

       

        private void DetermineNextLine(Line line)
        {
         
            if (line.Special == 0)
            {
                NextLine();
                _specialsOpen = false;
            }
            else if (!_choisesTransform.GetComponent<Animator>().GetBool("Open") && !_specialsOpen)
            {
                _choisesTransform.GetComponent<Animator>().SetBool("Open", true);
                InstantiateChoise(line.Choise0);
                InstantiateChoise(line.Choise1);
                InstantiateChoise(line.Choise2);
                _specialsOpen = true;

            }

        }

        //Create a new choise if there is content in the string
        private void InstantiateChoise(string choise)
        {
            if (string.IsNullOrEmpty(choise)) return;
            var button = Instantiate(_choisePrefab, _choisesTransform.transform);
            button.GetComponentInChildren<Text>().text = choise;

            var temp = _choises.Count;
            button.GetComponent<Button>().onClick.AddListener(delegate { ChooseResponse(temp); });
            _choises.Add(button);
        }



        //Switch off the choises
        private void ChooseResponse(int chosenButton)
        {
            StartCoroutine(CloseSpecial(_choises[chosenButton]));
            _currentBranch = chosenButton+1;
        
        }

        //Close any remaining special items
        private IEnumerator CloseSpecial(GameObject button)
        {
            List<GameObject> otherButtons = new List<GameObject>();
            for (int i = 0; i < _choises.Count; i++)
            {
                if (_choises[i] != button)
                    otherButtons.Add(_choises[i]);

            }
        
            foreach (var otherButton in otherButtons)
            {
                otherButton.GetComponent<Button>().interactable = false;
            }
            yield return new WaitForSecondsRealtime(1);
            button.GetComponent<Button>().interactable = false;


            yield return new WaitForSecondsRealtime(0.25f);
            _choisesTransform.GetComponent<Animator>().SetBool("Open", false);
            NextLine();
        
            
        }

        //Determineswhich sprite is apropriate given the current actor and its expression
        private Sprite DetermineActor(Actor actor, int actorExpression)
        {
            switch (actor)
            {
                case Actor.Badger:
                    return _portraitsBadger[actorExpression];
                case Actor.Beaver:
                    return _portraitsBeaver[actorExpression];
                case Actor.Hedgehog:
                    return _portraitsHedgehog[actorExpression];
                case Actor.Mouse:
                    return _portraitsMouse[actorExpression];
                case Actor.InjuredMouse:
                    return _portraitsInjuredMouse[actorExpression];
                case Actor.Pig:
                    return _portraitsPig[actorExpression];
                case Actor.Bunny:
                    return _portraitsBunny[actorExpression];
                case Actor.Rats:
                    return _portraitsRats[actorExpression];
                case Actor.Weasel:
                    return _portraitsWeasel[actorExpression];
                default:
                    throw new ArgumentOutOfRangeException("actor", actor, null);
            }
        }
    }
}
