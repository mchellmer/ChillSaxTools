using System;

public class CPHInline
{
    public string rawIn;
    public string botAction;
    public string[] engines = new string[]{
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
            if (arg.Key == "actionName")
            {
                botAction = $"{arg.Value}".TrimStart('!');
                CPH.SetGlobalVar("parameter", botAction);
            }
        }

        CPH.LogInfo($"rawIn :: {rawIn}");
        CPH.LogInfo($"botAction :: {botAction}");

        //Validate inputs and transform then set global vars
        var inNumeric = int.TryParse(rawIn, out _);
        CPH.LogInfo($"inNumeric :: {inNumeric}");

        if (inNumeric)
        {
            var validatedInput = "True";
            (string obsnum, string tranNum) = Transform(rawIn);
            CPH.LogInfo($"Transform :: {tranNum}");
            CPH.SetGlobalVar("validated", validatedInput);
            CPH.LogInfo($"Validated input :: {validatedInput}");
            CPH.SetGlobalVar("value", tranNum);
            CPH.SetGlobalVar("obsval", obsnum);
        }else
        {
            var validEngine = ValidateEngine(rawIn,engines,botAction);
            CPH.LogInfo($"Input validated as engine :: {validEngine}");
            CPH.SetGlobalVar("validated", validEngine);
            if (validEngine)
            {
                CPH.SetGlobalVar("obsval", rawIn);
                CPH.SetGlobalVar("value", rawIn);
                CPH.SetGlobalVar("engine", rawIn);
            }
        }

        return true;
    }
}