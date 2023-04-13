using System;

public class CPHInline
{
    public int _duration;

    public bool Execute()
    {
        foreach (var arg in args)
        {
            CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
        }
        CPH.LogInfo($"Remove follower only and slow mode");
        CPH.TwitchFollowMode(false,_duration);
        CPH.TwitchSlowMode(false,_duration);

        return true;
    }

    public CPHInline(){
        _duration = 0;
    }
}
