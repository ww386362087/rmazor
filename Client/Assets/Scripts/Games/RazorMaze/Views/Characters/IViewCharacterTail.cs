﻿using Games.RazorMaze.Models;
using SpawnPools;

namespace Games.RazorMaze.Views.Characters
{
    public interface IViewCharacterTail : IInit, IActivated
    {
        void ShowTail(CharacterMovingEventArgs _Args);
        void HideTail(CharacterMovingEventArgs _Args = null);
    }
}