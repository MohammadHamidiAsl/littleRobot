﻿using System.Collections;
using System.Collections.Generic;
using MHamidi;
using UnityEngine;
using System;
using Utils.Singlton;

public class CommandManger : MonoBehaviour
{
    public static event Action<Action, string> UpdatePlay;
    public static event Action ChangePlayButtonInteractivityStatus;
    public static event Action ResetPlayerPosition;
    public static event Action<int, ICommand> AddToBuffer;
    public static event Action<int, int> RemoveAtIndexofBuffer;


    private int curentBufferIndex = 0;
    public GameObject subjectOFCommand;
    public static CommandManger current;

    //A Pointer To Correc Command Buffer

    public List<ICommand> MainCommand = new List<ICommand>();
    public List<ICommand> P1Command = new List<ICommand>();
    public List<ICommand> p2Command = new List<ICommand>();
    public Stack<ICommand> RewindStack = new Stack<ICommand>();

    public List<ICommand> RunTimeCommands = new List<ICommand>();

    public void StopPlay()
    {
        Dipendency.Instance.StopAllCoroutines();
        Dipendency.Instance.LevelManger.ResetLevel();
        UpdatePlay?.Invoke(() => { Play(); }, "Play");
    }


    private void Awake()
    {
        current = this;
        Util.ShowMessag($"Command Factory Initlized");
        CommandFactory.GetCommand($"");
        var commandA = new Move();
        var commandB = new Move();
    }

    public void SetSubjectOfCommand(GameObject subjectRefrence)
    {
        subjectOFCommand = subjectRefrence;
        ClearAllBuffers();
        SetCurrentBuffer(0);

        UpdatePlay?.Invoke(() => { Play(); }, "PlayGame");
    }

    private void ClearAllBuffers()
    {
        MainCommand.Clear();
        P1Command.Clear();
        p2Command.Clear();
    }

    //Can Be Loaded From a json File


    public void Play()
    {
        StartCoroutine(PlayWithDelay());
    }

    private IEnumerator PlayWithDelay()
    {
        Util.ShowMessag($" the Current MainCommand has {MainCommand.Count} item in it");
        foreach (var item in MainCommand)
        {
            yield return StartCoroutine(item.Execute(subjectOFCommand));
            RewindStack.Push(item);
        }
    }

    //Redo This part 

    public void AddToCurrentBuffer(ICommand command)
    {
        if (curentBufferIndex == 0)
        {
            if (MainCommand.Count < Dipendency.Instance.LevelManger.currentLevel.maxBufferSize)
            {
                MainCommand.Add(command);
                AddToBuffer?.Invoke(curentBufferIndex, command);
            }
        }
        else if (curentBufferIndex == 1)
        {
            if (P1Command.Count < Dipendency.Instance.LevelManger.currentLevel.maxP1Size)
            {
                P1Command.Add(command);
                AddToBuffer?.Invoke(curentBufferIndex, command);
            }
        }
        else if (curentBufferIndex == 2)
        {
            if (p2Command.Count < Dipendency.Instance.LevelManger.currentLevel.maxP2Size)
            {
                p2Command.Add(command);
                AddToBuffer?.Invoke(curentBufferIndex, command);
            }
        }
    }

    public void RemoveFromBuffer(int bufferIndex, ICommand command)
    {
        if (bufferIndex == 0)
        {
            var index=MainCommand.IndexOf(command);
            RemoveAtIndexofBuffer?.Invoke(bufferIndex, index);
            MainCommand.Remove(command);
        }

        if (bufferIndex == 1)
        {
            var index=P1Command.IndexOf(command);
            RemoveAtIndexofBuffer?.Invoke(bufferIndex, index);
            P1Command.Remove(command);
        }

        if (bufferIndex == 2)
        {
            var index=p2Command.IndexOf(command);
            RemoveAtIndexofBuffer?.Invoke(bufferIndex, index);
            p2Command.Remove(command);
        }
    }

    private void ChangeBuffer(int index)
    {
        curentBufferIndex = index;
        
    }

    public void SetCurrentBuffer(int index)
    {
        ChangeBuffer(index);
    }
}