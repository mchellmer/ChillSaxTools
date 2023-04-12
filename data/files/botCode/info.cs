using System;
using System.IO;
using System.Threading;

public class CPHInline
{
    public string _assetDir;
    public string _sourcePng;
    public string _currentEngine;

    public bool Execute()
    {
        foreach (var arg in args)
        {
            CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
            if (arg.Key == "command")
            {
                var commandRaw = $"{arg.Value}".TrimStart('!');
                _sourcePng = $"{_assetDir}/{commandRaw}.png";
            }
        }

        var globalCurrentEngine = CPH.GetGlobalVar<string>("globalCurrentEngine");
        CPH.LogInfo($"globalCurrentEngine :: \"{globalCurrentEngine}\"");
        if (!string.IsNullOrWhiteSpace(globalCurrentEngine))
        {
            _currentEngine = globalCurrentEngine;
        }

        CPH.ObsSetImageSourceFile("Scene", "Info", _sourcePng);
        Thread.Sleep(5000);
        CPH.ObsSetImageSourceFile("Scene", "Info", $"{_assetDir}/{_currentEngine}.png");
        CPH.LogInfo($"Set obs png to :: {_sourcePng}");

        return true;
    }

    public CPHInline(){
        _currentEngine = "room";
        _assetDir = "C:/Users/mchel/OneDrive/Documents/0_Store/Twitch/Obs/popups";
    }
}
