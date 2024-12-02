using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler 
{
    public Action RotateMino;
    public Action MoveLeft;
    public Action MoveRight;
    public Action MoveDown;
    public Action Fall;
    public Action ChangeColor;
    public Action ResetMinoDataTable;
    public Action HoldMino;

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveLeft?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveRight?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Fall?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            HoldMino?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            //ToggleMinoVisibility();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ResetMinoDataTable?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RotateMino?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            MoveDown?.Invoke();
        }
    }
}
