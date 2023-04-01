using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure; // Required in C#

public class XInputTest : MonoBehaviour
{
    bool playerIndexSet = false;
    PlayerIndex playerIndex;
    public GamePadState state;
    GamePadState prevState;
    public bool VibrateOn;
    public float stateTriggerRight;
    public float stateTriggerLeft;
    public float SetVibrationLeft;
    public float SetVibrationRight;

    public float NewScaleBase;
    public float NewScaleRandFix;
    public float NewMinScaleLimit;
    Vector3 InitObjectScale; 

    // Use this for initialization
    void Start()
    {
        // No need to initialize anything for the plugin
        VibrateOn = true;
        InitObjectScale = transform.localScale;
        NewScaleBase = 1.5f;
        NewScaleRandFix = 0.2f;
        NewMinScaleLimit = 0.2f;
    }

    void FixedUpdate()
    {
        // SetVibration should be sent in a slower rate.
        // Set vibration according to triggers

        if (!VibrateOn)
        {
            SetVibrationLeft = 0;
            SetVibrationRight = 0;
            GamePad.SetVibration(playerIndex, SetVibrationLeft, SetVibrationRight);
            return; 
        }

        if (SetVibrationLeft > 1)
        {
            SetVibrationLeft = 1;
        }
        else
        if (SetVibrationLeft < 0)
        {
            SetVibrationLeft = 0;
        }

        if (SetVibrationRight > 1)
        {
            SetVibrationRight = 1;
        }
        else
        if (SetVibrationRight < 0)
        {
            SetVibrationRight = 0;
        }

        if (SetVibrationLeft == 0) //Т.е. когда вибрируем сами левым мотором, становлясь меньше
        {
            //В целом эту часть можно вообще убрать, чтобы не перегружать мир вибрацией и не усложнять :)
            //В тоже время можно сделать игру сложнее и инетереснее, оставив. Или задать это настройками! 
            if (stateTriggerLeft < NewMinScaleLimit)
            {
                GamePad.SetVibration(playerIndex, state.Triggers.Left, state.Triggers.Right);
            }
            else
            {
                GamePad.SetVibration(playerIndex, NewMinScaleLimit, state.Triggers.Right);
            }
        }
        else //Когда вибрацию задаёт внешняя среда, объекты, вибрируем правым самостоятельно, или левым
        {
            GamePad.SetVibration(playerIndex, SetVibrationLeft, state.Triggers.Right);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Find a PlayerIndex, for a single player game
        // Will find the first controller that is connected ans use it
        if (!playerIndexSet || !prevState.IsConnected)
        {
            for (int i = 0; i < 4; ++i)
            {
                PlayerIndex testPlayerIndex = (PlayerIndex)i;
                GamePadState testState = GamePad.GetState(testPlayerIndex);
                if (testState.IsConnected)
                {
                    Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
                    playerIndex = testPlayerIndex;
                    playerIndexSet = true;
                }
            }
        }

        prevState = state;
        state = GamePad.GetState(playerIndex);
        stateTriggerRight = state.Triggers.Right;
        stateTriggerLeft = state.Triggers.Left;

        // Detect if a button was pressed this frame
        if (prevState.Buttons.A == ButtonState.Released && state.Buttons.A == ButtonState.Pressed)
        {
            GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value, 1.0f);
        }
        // Detect if a button was released this frame
        if (prevState.Buttons.A == ButtonState.Pressed && state.Buttons.A == ButtonState.Released)
        {
            //GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }

        //Увеличение
        
        if (!VibrateOn)
        {
            transform.localScale = InitObjectScale;
            return; 
        }

        float NewScale = NewScaleBase * stateTriggerRight;
        float RandTriggerRight = Random.Range(0, stateTriggerRight) * NewScaleRandFix;
        Vector3 NewScaleRand = new Vector3(RandTriggerRight, RandTriggerRight, RandTriggerRight);
        transform.localScale = InitObjectScale + new Vector3(NewScale, NewScale, NewScale) + NewScaleRand;

        //Уменьшение
        if (stateTriggerLeft < NewMinScaleLimit)
        {
            transform.localScale = transform.localScale - new Vector3(stateTriggerLeft, stateTriggerLeft, stateTriggerLeft);
        }
        else
        {
            transform.localScale = transform.localScale - new Vector3(NewMinScaleLimit, NewMinScaleLimit, NewMinScaleLimit);
        }
        
        // Make the current object turn
        //transform.localRotation *= Quaternion.Euler(0.0f, state.ThumbSticks.Left.X * 25.0f * Time.deltaTime, 0.0f);
    }

    void OnGUI()
    {
        string text = "Use left stick to turn the cube, hold A to change color\n";
        text += string.Format("IsConnected {0} Packet #{1}\n", state.IsConnected, state.PacketNumber);
        text += string.Format("\tTriggers {0} {1}\n", state.Triggers.Left, state.Triggers.Right);
        text += string.Format("\tD-Pad {0} {1} {2} {3}\n", state.DPad.Up, state.DPad.Right, state.DPad.Down, state.DPad.Left);
        text += string.Format("\tButtons Start {0} Back {1} Guide {2}\n", state.Buttons.Start, state.Buttons.Back, state.Buttons.Guide);
        text += string.Format("\tButtons LeftStick {0} RightStick {1} LeftShoulder {2} RightShoulder {3}\n", state.Buttons.LeftStick, state.Buttons.RightStick, state.Buttons.LeftShoulder, state.Buttons.RightShoulder);
        text += string.Format("\tButtons A {0} B {1} X {2} Y {3}\n", state.Buttons.A, state.Buttons.B, state.Buttons.X, state.Buttons.Y);
        text += string.Format("\tSticks Left {0} {1} Right {2} {3}\n", state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, state.ThumbSticks.Right.X, state.ThumbSticks.Right.Y);
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), text);
    }

    void OnDestroy()
    {
        GamePad.SetVibration(playerIndex, 0, 0);
    }
}
