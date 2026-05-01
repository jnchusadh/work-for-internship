using System;
using System.Collections.Generic;
public delegate void MovementDelegate(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle, bool isCarrying,
    ToolEffect toolEffect,
    bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
    bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
    bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown);

public class EventHandler 
{
    public static event Action<int, Season, int, string, int, int, int> gameTimeMinuteEvent;
    public static event Action<int, Season, int, string, int, int, int> gameTimeHourEvent;
    public static event Action<int, Season, int, string, int, int, int> gameTimeDayEvent;
    public static event Action<int, Season, int, string, int, int, int> gameTimeSeasonEvent;
    public static event Action<int, Season, int, string, int, int, int> gameTimeYearEvent;
    public static event Action<InventoryLocation, List<InventoryItem>> InventoryUpdateEvents;
    public static event Action BeforeSceneUnloadFadeOutEvent;
    public static event Action BeforeSceneUnloadEvent;
    public static event Action AfterSceneLoadEvent;
    public static event Action AfterSceneLoadFadeInEvent;
    public static event Action DropSelectedItemEvent;
    public static event MovementDelegate MovementEvent;
    #region 物品栏变化事件
    public static void CallInventoryUpdateEvents(InventoryLocation inventoryLocation,
        List<InventoryItem> inventoryItemList)
    {
        if (InventoryUpdateEvents != null)
        {
            InventoryUpdateEvents(inventoryLocation, inventoryItemList);
        }
    }
    #endregion
    #region 玩家移动事件
    public static void CallMovementEvent(float xInput, float yInput, bool isWalking, bool isRunning, bool isIdle, bool isCarrying,
    ToolEffect toolEffect,
    bool isUsingToolRight, bool isUsingToolLeft, bool isUsingToolUp, bool isUsingToolDown,
    bool isLiftingToolRight, bool isLiftingToolLeft, bool isLiftingToolUp, bool isLiftingToolDown,
    bool isPickingRight, bool isPickingLeft, bool isPickingUp, bool isPickingDown)
    {
        if (MovementEvent != null)
        {
            MovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying,
    toolEffect,
    isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
    isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
    isPickingRight, isPickingLeft, isPickingUp, isPickingDown);
        }
    }
    #endregion
    #region 游戏时间事件
    public static void CallgameTimeMinuteEvent(int gameYear,Season gameSeason,int gameDay,string dayOfWeek,int gameHour,int gameMinute,
        int gameSecond)
    {
        if (gameTimeMinuteEvent != null)
        {
            gameTimeMinuteEvent(gameYear, gameSeason, gameDay, dayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }
    public static void CallgameTimeHourEvent(int gameYear, Season gameSeason, int gameDay, string dayOfWeek, int gameHour, int gameMinute,
        int gameSecond)
    {
        if (gameTimeHourEvent != null)
        {
            gameTimeHourEvent(gameYear, gameSeason, gameDay, dayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }
    public static void CallgameTimeDayEvent(int gameYear, Season gameSeason, int gameDay, string dayOfWeek, int gameHour, int gameMinute,
        int gameSecond)
    {
        if (gameTimeDayEvent != null)
        {
            gameTimeDayEvent(gameYear, gameSeason, gameDay, dayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }
    public static void CallgameTimeSeasonEvent(int gameYear, Season gameSeason, int gameDay, string dayOfWeek, int gameHour, int gameMinute,
        int gameSecond)
    {
        if (gameTimeSeasonEvent != null)
        {
            gameTimeSeasonEvent(gameYear, gameSeason, gameDay, dayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }
    public static void CallgameTimeYearEvent(int gameYear, Season gameSeason, int gameDay, string dayOfWeek, int gameHour, int gameMinute,
        int gameSecond)
    {
        if (gameTimeYearEvent != null)
        {
            gameTimeYearEvent(gameYear, gameSeason, gameDay, dayOfWeek, gameHour, gameMinute, gameSecond);
        }
    }
    #endregion
    #region 场景切换事件
    public static void CallBeforeSceneUnloadFadeOutEvent()
    {
        if (BeforeSceneUnloadFadeOutEvent != null)
        {
            BeforeSceneUnloadFadeOutEvent();
        }
    }
    public static void CallBeforeSceneUnloadEvent()
    {
        if (BeforeSceneUnloadEvent != null)
        {
            BeforeSceneUnloadEvent();
        }
    }
    public static void CallAfterSceneLoadEvent()
    {
        if (AfterSceneLoadEvent != null)
        {
            AfterSceneLoadEvent();
        }
    }
    public static void CallAfterSceneLoadFadeInEvent()
    {
        if (AfterSceneLoadFadeInEvent != null)
        {
            AfterSceneLoadFadeInEvent();
        }
    }
    #endregion
    #region 点击丢弃事件
    public static void CallDropSelectedItemEvent()
    {
        if (DropSelectedItemEvent != null)
        {
            DropSelectedItemEvent();
        }
    }
    #endregion

}
