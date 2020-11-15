﻿using Constants;
using Network;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelLoader
{
    private const string WasNotMadeMessage = "Game was not made";

    static LevelLoader()
    {
        SceneManager.sceneLoaded += OnLoadLevel;
    }

    private static void OnLoadLevel(Scene _Scene, LoadSceneMode _LoadSceneMode)
    {
        if (_Scene.name != SceneNames.Level)
            return;
        LoadGame(GameClient.Instance.GameId);
    }

    public static void LoadLevel()
    {
        SoundManager.Instance.StopPlayingClips();
        SceneManager.LoadScene(SceneNames.Level);
    }
    
    private static void LoadGame(int _GameId)
    {
        switch (_GameId)
        {
            case 1:
                PointsTapper.PointsTapperManager.Instance.Init();
                break;
            case 2:
                Debug.Log(WasNotMadeMessage);
                break;
            case 3:
                Debug.Log(WasNotMadeMessage);
                break;
            case 4:
                Debug.Log(WasNotMadeMessage);
                break;
            case 5:
                Debug.Log(WasNotMadeMessage);
                break;
            default:
                throw new System.NotImplementedException();
        }
    }
}