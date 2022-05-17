using System;

public enum NpcStates
{
    Idle,
    GoingToWork,
    Working,
    GoingHome,
    InteractingWithPlayer,
    Talking,
    Scared
}

public enum EnemyState
{
    Idle,
    Patrol,
    Chasing,
    Attacking
}

[Flags]
public enum Job
{
    Farmer = 1,
    FisherMan = 2,
    Lumberjack = 4,
    Merchant = 8,
    Guard = 16,
    Rich = 32,
    InnKeeper,
    Servant,
    Default = 64
}

[Flags]
public enum Gender
{
    Male = 1,
    Female = 2,
    Default = 4
}