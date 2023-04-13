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
        CPH.LogInfo($"Turn on slow mode for {_duration} seconds");
        CPH.TwitchSlowMode(true, _duration);

        return true;
    }

    public CPHInline(){
        _duration = 30;
    }
}
