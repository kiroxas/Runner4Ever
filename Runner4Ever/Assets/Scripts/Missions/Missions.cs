using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Base abstract class used to define a mission the player needs to complete FOR FAME
/// Subclassed for every mission.
/// </summary>
public abstract class MissionBase
{
    // Mission type
    public enum MissionType
    {
        SINGLE_RUN,
        PICKUP,
        OBSTACLE_JUMP,
        SLIDING,
        MULTIPLIER,
        MAX
    }

    public float progress;
    public float max;
    public int reward;

    public bool isComplete { get { return (progress / max) >= 1.0f; } }

    public void Serialize(BinaryWriter w)
    {
        w.Write(progress);
        w.Write(max);
        w.Write(reward);
    } 

    public void Deserialize(BinaryReader r)
    {
        progress = r.ReadSingle();
        max = r.ReadSingle();
        reward = r.ReadInt32();
    }

	public virtual bool HaveProgressBar() { return true; }

    public abstract void Created();
    public abstract MissionType GetMissionType();
    public abstract string GetMissionDesc();
    public abstract void RunStart(TrackingManager manager);
    public abstract void Update(TrackingManager manager);

    static public MissionBase GetNewMissionFromType(MissionType type)
    {
        switch (type)
        {
            case MissionType.SINGLE_RUN:
                return new SingleRunMission();
            case MissionType.PICKUP:
                return new PickupMission();
            case MissionType.OBSTACLE_JUMP:
                return new JumpMission();
        }

        return null;
    }
}

public class SingleRunMission : MissionBase
{
    public override void Created()
    {
        float[] maxValues = { 500, 1000, 1500, 2000 };
        int choosenVal = Random.Range(0, maxValues.Length);

        reward = choosenVal + 1;
        max = maxValues[choosenVal];
        progress = 0;
    }

	public override bool HaveProgressBar()
	{
		return false;
	}

	public override string GetMissionDesc()
    {
        return LocalizationManager.GetValue("run") + ((int)max) + LocalizationManager.GetValue("distance_unit") + " " + LocalizationManager.GetValue("single_run");
    }

    public override MissionType GetMissionType()
    {
        return MissionType.SINGLE_RUN;
    }

    public override void RunStart(TrackingManager manager)
    {
        progress = 0;
    }

    public override void Update(TrackingManager manager)
    {
        progress = manager.distanceForThisRun();
    }
}

public class PickupMission : MissionBase
{
    int previousCoinAmount;

    public override void Created()
    {
        float[] maxValues = { 1000, 2000, 3000, 4000 };
        int choosen = Random.Range(0, maxValues.Length);

        max = maxValues[choosen];
        reward = choosen + 1;
        progress = 0;
    }

    public override string GetMissionDesc()
    {
        return LocalizationManager.GetValue("pickup") + max + " " + LocalizationManager.GetValue("normalObject");
    }

    public override MissionType GetMissionType()
    {
        return MissionType.PICKUP;
    }

    public override void RunStart(TrackingManager manager)
    {
        previousCoinAmount = 0;
    }

    public override void Update(TrackingManager manager)
    {
        int coins = manager.objectsCollectedForThisRun() - previousCoinAmount;
        progress += coins;

        previousCoinAmount = manager.objectsCollectedForThisRun();
    }
}

public class JumpMission : MissionBase
{
    int previousJumps;
    
    public override void Created()
    {
        float[] maxValues = { 20, 50, 75, 100 };
        int choosen = Random.Range(0, maxValues.Length);

        max = maxValues[choosen];
        reward = choosen + 1;
        progress = 0;
        previousJumps = 0;
    }

    public override string GetMissionDesc()
    {
        return "Jump over " + ((int)max) + " barriers";
    }

    public override MissionType GetMissionType()
    {
        return MissionType.OBSTACLE_JUMP;
    }

    public override void RunStart(TrackingManager manager)
    {
       previousJumps = 0;
    }

    public override void Update(TrackingManager manager)
    {
        int jumps = manager.jumpsForThisRun() - previousJumps;
        progress += jumps;

        previousJumps = manager.jumpsForThisRun();
    }
}