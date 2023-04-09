using System;
using System.IO;
using System.Threading;

public class CPHInline
{
    public string assetDir;
    public string sourcePng;
    public string obsPng;
    public string backupPng;

    public bool Execute()
    {
        foreach (var arg in args)
        {
            CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
            if (arg.Key == "command")
            {
                var commandRaw = $"{arg.Value}".TrimStart('!');
                sourcePng = $"{assetDir}/{commandRaw}.png";
            }
        }
        File.Copy(obsPng,backupPng,true);
        File.Copy(sourcePng,obsPng,true);
        Thread.Sleep(5000);
        File.Copy(backupPng,obsPng,true);
        
        CPH.LogInfo($"Set obs png to :: {sourcePng}");

        return true;
    }

    public CPHInline(){
        assetDir = "C:/Program Files/Streamer.bot-x64-0.1.19/data/ChillSaxTools";
        obsPng = $"{assetDir}/popup.png";
        backupPng = $"{assetDir}/popup_backup.png";
    }
}
