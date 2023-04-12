using System;
using System.IO;

public class CPHInline
{
    string _newDj;
    string _userInput;
    string _djFilePath;

    public void WriteToFile(string filePath, string value){
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine(value);
        }
    }

    public bool Execute()
    {
        foreach (var arg in args)
        {
            CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
            if (arg.Key == "input0") {
            	_userInput = $"{arg.Value}";
                CPH.LogInfo($"Set User to Input :: {_userInput}");
                CPH.LogInfo($"input is null or empty :: {String.IsNullOrEmpty(_userInput)}");
            }
        }

		if(String.IsNullOrEmpty(_userInput)){
                CPH.LogInfo($"Default newDj");
                _newDj = "8-BitSaxBot";
		}else{
                _newDj = _userInput;
        }
        CPH.AddUserToGroup(_newDj, "DeeJay");
        CPH.LogInfo($"Set newDJ to :: {_newDj}");
        WriteToFile(_djFilePath, _newDj);

        return true;
    }

    public CPHInline(){
        _userInput = null;
        _djFilePath = "C:/Users/mchel/OneDrive/Documents/0_Store/Twitch/Obs/text/deejay.txt";
    }
}
