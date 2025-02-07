﻿using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Views.Utils
{
    public static class SortingOrders
    {
        public const int
            BackgroundTexture            = Path - 350,
            BackgroundItem               = Path - 300,
            AdditionalBackgroundTexture2 = Path - 250,
            AdditionalBackgroundTexture  = Path - 200,
            AdditionalBackgroundBorder   = Path - 150,
            AdditionalBackgroundCorner   = Path - 100,
            PathBackground               = Path - 50,
            Path                         = -3000,
            PathLine                     = Path + 50,
            PathJoint                    = Path + 100,
            MoneyItem                    = Path + 150,
            GameUI                       = Path + 1000,
            Character                    = Path + 1500,
            GameLogoBackground           = Path + 3500,
            GameLogoForeground           = Path + 3501;

        public static int GetBlockSortingOrder(EMazeItemType _Type)
        {
            switch (_Type)
            {
                case EMazeItemType.GravityBlock:
                case EMazeItemType.GravityTrap:
                    return Path + 151;
                case EMazeItemType.Block:
                    return Path + 152;
                case EMazeItemType.Diode:
                    return Path + 153;
                case EMazeItemType.Portal:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlockFree:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.KeyLock:
                    return Path + 154;
                case EMazeItemType.Turret:
                    return Path + 155;
                case EMazeItemType.Springboard:
                    return Path + 156;
                case EMazeItemType.TrapReact:
                    return Path + 300;
                case EMazeItemType.Hammer:
                    return Path + 350;
                case EMazeItemType.Spear:
                    return Path + 450;
                default: return Path + 500;
            }
        }
    }
}