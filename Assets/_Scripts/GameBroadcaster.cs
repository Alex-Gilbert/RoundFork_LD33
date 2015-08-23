﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public class GameBroadcaster 
{
    public delegate void UnityAction(GameObject sender, float flag);

    private static GameBroadcaster instance;
    private GameBroadcaster() { }

    public static GameBroadcaster Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameBroadcaster();
            }
            return instance;
        }
    }

    public event UnityAction PlayerMadeNoise;
    public void OnPlayerMadeNoise(GameObject player, float noiseRange)
    {


        if (PlayerMadeNoise != null)
            PlayerMadeNoise(player, noiseRange);

        Debug.Log("Player made noise: " + noiseRange.ToString());
    }

    public event Action PlayerSwipeAttacked;
    public void OnPlayerSwipeAttacked()
    {
        if (PlayerSwipeAttacked != null)
            PlayerSwipeAttacked();

        Debug.Log("PlayerAttacked");
    }
}