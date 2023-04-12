using System;
using System.IO;
using System.Collections.Generic;

public class CPHInline
{
    public string _rawIn;
    public string _botAction;
    public string _manifestJson;
    public string _validatedInput;
    public string _paramValue;
    public string _obsValue;
    public string _engine;
    public string _obsTextPath;
    public string[] _engines;
    public Dictionary<string, string> _commandMap;
    public string _assetDir;
    public string _textDir;
    public string _manifestPath;

    public bool Validateengine(string newValue, string[] valueList, string action){
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
                _rawIn = $"{arg.Value}";
            }
            if (arg.Key == "command")
            {
                var commandRaw = $"{arg.Value}".TrimStart('!');
                _botAction = _commandMap[commandRaw];
                _obsTextPath = $"{_textDir}/{_botAction}.txt";
            }
        }

        CPH.LogInfo($"_rawIn :: {_rawIn}");
        CPH.LogInfo($"_botAction :: {_botAction}");

        //Validate inputs, set global, set param and obs values, build ventris json, then transform input if it's an int
        var inNumeric = int.TryParse(_rawIn, out _);
        CPH.LogInfo($"inNumeric :: {inNumeric}");
        if (inNumeric)
        {
            var activeengine = string.Join("", File.ReadAllLines($"{_textDir}/engine.txt"));
            CPH.LogInfo($"Active engine :: {activeengine}");
            (string obsnum, string tranNum) = Transform(_rawIn);
            _validatedInput = "True";
            _paramValue = tranNum;
            _obsValue = obsnum;
            _engine = activeengine;
            CPH.LogInfo($"Transform :: {tranNum}");
            CPH.LogInfo($"Validated input :: {_validatedInput}");
            CPH.SetGlobalVar("validated", _validatedInput);
            //Manifest is a single midi cc request to update param value
            _manifestJson = $"[{VentrisManifest(_botAction,tranNum,_engine)}]";
        }else
        {
            var validengine = Validateengine(_rawIn,_engines,_botAction);
            _validatedInput = validengine.ToString();
            CPH.LogInfo($"Input validated as engine :: {_validatedInput}");
            CPH.SetGlobalVar("validated", _validatedInput);
            if (validengine)
            {
                _paramValue = _rawIn;
                _obsValue = _rawIn;
                _engine = _rawIn;

                //Update global current engine
                CPH.SetGlobalVar("globalCurrentEngine", _engine);

                //Manifest is a set of midi cc requests that updates the engine and sets engine defaults
                _manifestJson = $"[{VentrisManifest(_botAction,_paramValue,_engine)},";
                _manifestJson += $"{VentrisManifest("mix","100",_engine)},";
                _manifestJson += $"{VentrisManifest("time","25",_engine)},";
                _manifestJson += $"{VentrisManifest("delay","25",_engine)},";
                _manifestJson += $"{VentrisManifest("control1","25",_engine)},";
                _manifestJson += $"{VentrisManifest("control2","25",_engine)}]";

                //Update obs files with default values
                WriteToFile($"{_textDir}/time.txt", "3");
                WriteToFile($"{_textDir}/delay.txt", "2");
                WriteToFile($"{_textDir}/control1.txt", "4");
                WriteToFile($"{_textDir}/control2.txt", "4");

                //Update obs popup png with path to engine png
                var enginePngFIle = $"{_assetDir}/Obs/popups/{_rawIn}.png";
                CPH.ObsSetImageSourceFile("Scene", "Info", enginePngFIle);
            }
        }

        if (_validatedInput.ToLower() == "true")
        {
            //write manifest to file
            WriteToFile(_manifestPath, _manifestJson);

            //write value to obsfile
            WriteToFile(_obsTextPath, _obsValue);

            CPH.LogInfo($"Received command :: set '{_botAction}' to '{_paramValue}' for engine '{_engine}'");
        }else{
            CPH.LogInfo($"Input invalid :: skip updating manifest and obs files");
        }

        return true;
    }

    public CPHInline(){
        _engines = new string[]{
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

        _commandMap = new Dictionary<string, string> (){
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
        _assetDir = "C:/Users/mchel/OneDrive/Documents/0_Store/Twitch";
        _textDir = $"{_assetDir}/Obs/text";
        _manifestPath = $"{_assetDir}/Manifests/manifest.json";
    }
}