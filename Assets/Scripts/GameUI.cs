using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Main ReferenceToMain;
    bool FirstStart = true;

    void ButtonStartGameCallBack()
    {
        ReferenceToMain.UIParent.SetActive(false);
        ReferenceToMain.GameUIParent.SetActive(true);
        ReferenceToMain.UndoButton.gameObject.SetActive(true);
        ReferenceToMain.GameOverText.gameObject.SetActive(false);
        Game.GameStarted = true;
        Debug.Log("Button PUshed to start game");
    }

    void SliderValueChanged(float value)
    {
        //TODO(Ray):Also creates Garbage fix..
        ReferenceToMain.SliderCount.text = value.ToString();
    }

    void InitUI()
    {
        ReferenceToMain.ContinueGameButton.gameObject.SetActive(false);
        ReferenceToMain.GameUIParent.SetActive(false);
        ReferenceToMain.SliderCount.text = ReferenceToMain.CubeDimSlider.value.ToString();
        ReferenceToMain.GameOverText.gameObject.SetActive(false);

        ReferenceToMain.NewGameButton.onClick.AddListener(() =>
        {
            ReferenceToMain.NewGameStart = true;
            Cube.CubeDim = (int)ReferenceToMain.CubeDimSlider.value;
            ReferenceToMain.GameUIParent.SetActive(true);
            if (FirstStart)
            {
                FirstStart = false;
                ReferenceToMain.ContinueGameButton.gameObject.SetActive(true);
                Cube.InitCube();
            }
            else
            {
                Game.ResetState();
                Cube.InitCube();
            }
        });

        ReferenceToMain.NewGameButton.onClick.AddListener(ButtonStartGameCallBack);
        ReferenceToMain.ContinueGameButton.onClick.AddListener(ButtonStartGameCallBack);

        ReferenceToMain.MenuButton.onClick.AddListener(() =>
        {
            if (Game.GameOver == true)
            {
                ReferenceToMain.ContinueGameButton.gameObject.SetActive(false);
            }
            else
            {
                ReferenceToMain.ContinueGameButton.gameObject.SetActive(true);
            }
            
            ReferenceToMain.GameUIParent.SetActive(false);
            ReferenceToMain.UIParent.SetActive(true);
            Game.GameStarted = false;
        });

        ReferenceToMain.CubeDimSlider.onValueChanged.AddListener(SliderValueChanged);

        ReferenceToMain.UndoButton.onClick.AddListener(() =>
        {
            if (Game.MoveInProgress == false)
            {
                Game.CurrentSliceData = Undo.RewindMove();
                if (Game.CurrentSliceData.entities != null)
                {
                    Game.StartMove(Game.CurrentSliceData.entities);
                }
            }
        });
    }

    // Start is called before the first frame update
    void Start()
    {
        ReferenceToMain = GetComponent<Main>();
        if (ReferenceToMain == null)
        {
            //Tell the user the game is not going to work
        }
        InitUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
