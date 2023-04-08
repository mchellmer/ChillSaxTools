using System;
using System.IO;
using System.Collections.Generic;

public class CPHInline
{
    public string rawIn;
    public string botAction;
    public string manifestJson;
    public string validatedInput;
    public string paramValue;
    public string obsValue;
    public string engine;
    public string obsPath;
    public string[] engines;
    public Dictionary<string, string> commandMap;
    public string assetDir;
    public string obsPng;
    public string  manifestPath;

    public bool ValidateEngine(string newValue, string[] valueList, string action){
        var validated = false;
        int pos = Array.IndexOf(valueList, newValue);
        if (pos > -1 & action == "engine")
        {
            validated = true;
        }
		return validated;
    }
    public Tuple<string,string> Transform(string newValue){
        int transVal = Int32.Parse(newValue);
        if (transVal > 11)
        {
            return Tuple.Create("11","127");
        }
        if(transVal < 0){
            return Tuple.Create("0","0");
        }
        double tranRatio = 127.0/11.0;
		var trannum = transVal*tranRatio;
		var tranint = Convert.ToInt32(trannum);
        return Tuple.Create(transVal.ToString(),tranint.ToString());
    }
    public string VentrisManifest(string param, string value, string engine){
        return $"{{\"parameter\": \"{param}\",\"value\": \"{value}\",\"engine\": \"{engine}\"}}";
    }

    public void WriteToFile(string filePath, string value){
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(value);
        }
    }
    public bool Execute()
    {
        //Log arguments and pull input/action arguments to local variables
        foreach (var arg in args)
        {
            CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
            if (arg.Key == "input0")
            {
                rawIn = $"{arg.Value}";
            }
            if (arg.Key == "command")
            {
                var commandRaw = $"{arg.Value}".TrimStart('!');
                botAction = commandMap[commandRaw];
                obsPath = $"{assetDir}/{botAction}.txt";
            }
        }

        CPH.LogInfo($"rawIn :: {rawIn}");
        CPH.LogInfo($"botAction :: {botAction}");

        //Validate inputs, set global, set param and obs values, build ventris json, then transform input if it's an int
        var inNumeric = int.TryParse(rawIn, out _);
        CPH.LogInfo($"inNumeric :: {inNumeric}");
        if (inNumeric)
        {
            var activeEngine = string.Join("", File.ReadAllLines($"{assetDir}/engine.txt"));
            CPH.LogInfo($"Active engine :: {activeEngine}");
            (string obsnum, string tranNum) = Transform(rawIn);
            validatedInput = "True";
            paramValue = tranNum;
            obsValue = obsnum;
            engine = activeEngine;
            CPH.LogInfo($"Transform :: {tranNum}");
            CPH.LogInfo($"Validated input :: {validatedInput}");
            CPH.SetGlobalVar("validated", validatedInput);
            //Manifest is a single midi cc request to update param value
            manifestJson = $"[{VentrisManifest(botAction,tranNum,engine)}]";
        }else
        {
            var validEngine = ValidateEngine(rawIn,engines,botAction);
            validatedInput = validEngine.ToString();
            CPH.LogInfo($"Input validated as engine :: {validatedInput}");
            CPH.SetGlobalVar("validated", validatedInput);
            if (validEngine)
            {
                paramValue = rawIn;
                obsValue = rawIn;
                engine = rawIn;
                //Manifest is a set of midi cc requests that updates the engine and sets engine defaults
                manifestJson = $"[{VentrisManifest(botAction,paramValue,engine)},";
                manifestJson += $"{VentrisManifest("mix","100",engine)},";
                manifestJson += $"{VentrisManifest("time","25",engine)},";
                manifestJson += $"{VentrisManifest("delay","25",engine)},";
                manifestJson += $"{VentrisManifest("control1","25",engine)},";
                manifestJson += $"{VentrisManifest("control2","25",engine)}]";

                //Update obs files with default values
                WriteToFile($"{assetDir}/time.txt", "3");
                WriteToFile($"{assetDir}/delay.txt", "2");
                WriteToFile($"{assetDir}/control1.txt", "4");
                WriteToFile($"{assetDir}/control2.txt", "4");

                //Update obs popup png with path to engine png
                var enginePngFIle = $"{assetDir}/{rawIn}.png";
                System.IO.File.Copy(enginePngFIle,obsPng,true);
            }
        }

        if (validatedInput.ToLower() == "true")
        {
            //write manifest to file
            WriteToFile(manifestPath, manifestJson);

            //write value to obsfile
            WriteToFile(obsPath, obsValue);

            CPH.LogInfo($"Received command :: set '{botAction}' to '{paramValue}' for engine '{engine}'");
        }else{
            CPH.LogInfo($"Input invalid :: skip updating manifest and obs files");
        }

        return true;
    }

    public CPHInline(){
        engines = new string[]{
            "room",
            "hall",
            "dome",
            "spring",
            "plate",
            "lofi",
            "modverb",
            "shimmer",
            "echoverb",
            "swell",
            "offspring",
            "reverse"
        };

        commandMap = new Dictionary<string, string> (){
            {"control1","control1"},
            {"c1","control1"},
            {"dial1","control1"},
            {"d1","control1"},
            {"control2","control2"},
            {"c2","control2"},
            {"dial2","control2"},
            {"d2","control2"},
            {"delay","delay"},
            {"d","delay"},
            {"engine","engine"},
            {"e","engine"},
            {"time","time"},
            {"t","time"},
        };
        assetDir = "C:/Program Files/Streamer.bot-x64-0.1.19/data/ChillSaxTools";
        obsPng = $"{assetDir}/popup.png";
        manifestPath = $"{assetDir}/manifest.json";
    }
}