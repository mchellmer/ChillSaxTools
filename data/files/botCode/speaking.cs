using System;

public class CPHInline
{
    public string _obsScene;
    public string _obsSaxSource;
    public string _obsSpeakingSource;

    public bool Execute()
    {
        foreach (var arg in args)
        {
            CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
        }
        CPH.LogInfo($"Toggle mute state of speaking and sax");
        CPH.ObsSourceMuteToggle(_obsScene, _obsSaxSource);
        CPH.ObsSourceMuteToggle(_obsScene, _obsSpeakingSource);

        return true;
    }

    public CPHInline(){
        _obsScene = "Scene";
        _obsSaxSource = "Reaper-Sax";
        _obsSpeakingSource = "Reaper-Speech";
    }
}
