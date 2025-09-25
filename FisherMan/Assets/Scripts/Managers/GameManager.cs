using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public enum GameStates
{
    Title,
    Playing,
    Pause
}

public enum PlayerStates
{
    Idle,
    Walk,
    Fishing,
    Typing
}
