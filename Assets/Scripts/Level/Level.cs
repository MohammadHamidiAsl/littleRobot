﻿using System;
using System.Collections.Generic;
using MHamidi;
using Newtonsoft.Json.Linq;
using UnityEngine;


public enum PlayerDirection
{
    Up,Down,Right,Left
    
}

[System.Serializable]
public class Level
{

    public int width=8;
    public int height=8;
    public int number;
    public int startX=0;
    public int startY=0;
    public List<string> AvailableCommand = new List<string>();
    public int maxBufferSize;
    public int maxP1Size;
    public int maxP2Size;
    public Vector2Int Start;
    public PlayerDirection direction;
    public CellLayout[,] LevelLayout;


    public Level()
    {
    }
    

    public Level(JToken token)
    {
        var castedType = Util.NullabelCaster.CastInt(token["direction"]);
        direction = (PlayerDirection)castedType;
        number = Util.NullabelCaster.CastInt(token["number"]);
        startX = Util.NullabelCaster.CastInt(token["startX"]);
        startY = Util.NullabelCaster.CastInt(token["startY"]);
        maxBufferSize = Util.NullabelCaster.CastInt(token["maxBufferSize"]);
        maxP1Size = Util.NullabelCaster.CastInt(token["maxP1Size"]);
        maxP2Size = Util.NullabelCaster.CastInt(token["maxP2Size"]);
        var availebel = (JArray)token["AvailableCommand"];
        AvailableCommand = new List<string>();
        foreach (var item in availebel)
        {
            AvailableCommand.Add(Util.NullabelCaster.CastString(item));
        }
        var first = (JArray)token["LevelLayout"];
        var second = (JArray)first[0];
        LevelLayout = new CellLayout[first.Count, second.Count];
        for (var i = 0; i < first.Count; i++)
        {
            for (int j = 0; j < second.Count; j++)
            {
                var s = (JArray)first[i];
                LevelLayout[i, j] = new CellLayout(s[j]);
              
            }
        }

       
    }



    public Level(int number,List<string> availableCommand,PlayerDirection direction,CellLayout[,] levelLayout, int bufferSizeCurrentValue, int p1SizeCurrentValue, int p2SizeCurrentValue,int StartX,int StartY)
    {
        this.direction = direction;
        this.number = number;
        startX = StartX;
        startY = StartY;
        this.AvailableCommand = availableCommand;
        this.LevelLayout = levelLayout;
        this.maxBufferSize = bufferSizeCurrentValue;
        this.maxP1Size = p1SizeCurrentValue;
        this.maxP2Size = p2SizeCurrentValue;
    }
}
