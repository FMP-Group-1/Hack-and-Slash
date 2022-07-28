using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RespawnPoint
{
    Cell,
    Hall,
    Armory,
    GuardRoom,
    Arena
}
public class RespawnManager : MonoBehaviour
{
    public static RespawnPoint currentRespawnPoint = RespawnPoint.Cell;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CompleteCorridor()
    {
        currentRespawnPoint = RespawnPoint.Hall;
    }
    public void CompleteArmory()
    {

        currentRespawnPoint = RespawnPoint.Armory;
    }
    public void CompleteGuardRoom()
    {
        currentRespawnPoint = RespawnPoint.GuardRoom;
    }
    public void SpawnInArena( int wave )
    {
        currentRespawnPoint = RespawnPoint.Arena;
        switch( wave )
		{
            default:
                break;

            case 1:
                break;
		}
    }
}
