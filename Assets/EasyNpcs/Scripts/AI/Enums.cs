using System;

public enum NpcState
{
    Idle,
    GoingToWork,
    Working,
    GoingHome,
    InteractingWithPlayer,
    Talking,
    Scared
}

public enum EnemeyState
{
    Idle,
    Patrol,
    Chase,
    Attack
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