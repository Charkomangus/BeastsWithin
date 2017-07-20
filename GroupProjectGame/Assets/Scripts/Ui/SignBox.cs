﻿using Assets.Scripts.MainManagers;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Ui
{
    //Sign object - when interacted with by the player UI textbox will appear with the stored text found here.
    public class SignBox : MonoBehaviour
    {
        private Animator _signBoxAnimator;
        private Text _signBoxText;

        
        // Use this for initialization
        private void Start ()
        {
            _signBoxAnimator = GetComponent<Animator>();
            _signBoxText = GetComponentInChildren<Text>();
        }
	
        // Update is called once per frame
        private void Update()
        {
            //Switch off when the player moves away
            if (GameManager.Instance.Player.IsMoving() && _signBoxAnimator.GetBool("Open"))
            {
                Open();
            }
        }

        //Change stroed text
        public void SetTextBoxText(string newText)
        {
            _signBoxText.text = newText;
        }

        //Change status of the animator
        public void Open()
        {
            _signBoxAnimator.SetBool("Open", !_signBoxAnimator.GetBool("Open"));
        }
      

        //Returns text box's current state
        public bool IsOpen()
        {
            return _signBoxAnimator.GetBool("Open");
        }
    }
}
