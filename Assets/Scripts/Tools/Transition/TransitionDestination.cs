using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionDestination : MonoBehaviour
{
    public enum DestinationTag
    {
        BirthPos,
        FrontGateIndoor,FrontGateOurdoor,
        LeftGateIndoor, LeftGateOurdoor,
        RightGateIndoor, RightGateOurdoor,
        DungonIndoor, DungonOurdoor,
        FirstFloor, SecondFloorUP, SecondFloorDOWN, ThirdFloor
    }
    public DestinationTag destinationTag;
}
