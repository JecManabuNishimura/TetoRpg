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

    private float timer;
    private float maxTimer = 0.05f;

    public void HandleInput()
    {
        timer += Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
        {
            if(timer > maxTimer)
            {
                MoveLeft?.Invoke();
                timer=0;
            }
        }

        if (Input.GetKey(KeyCode.D))
        {
            if (timer > maxTimer)
            {
                MoveRight?.Invoke();
                timer=0;
            }
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

        if (Input.GetKey(KeyCode.S))
        {
            if (timer > maxTimer)
            {
                MoveDown?.Invoke();
                timer=0;
            }
        }
    }
}
