using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DinoJockey.Game;

public enum GameStatus
{
    Waiting,
    ReadyToStart,
    Counting,
    Starting,
    Playing,
    losing,
    GameOver,
    Winner
}