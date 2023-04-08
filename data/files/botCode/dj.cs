using System;

public class CPHInline
{
    string newDj;
    string userInput = null;
    public bool Execute()
    {
        foreach (var arg in args)
        {
            CPH.LogInfo($"LogVars :: {arg.Key} = {arg.Value}");
            if (arg.Key == "input0") {
            	userInput = $"{arg.Value}";
                CPH.LogInfo($"Set User to Input :: {userInput}");
                CPH.LogInfo($"input is null or empty :: {String.IsNullOrEmpty(userInput)}");
            }
        }

		if(String.IsNullOrEmpty(userInput)){
                CPH.LogInfo($"Default newDj");
                newDj = "8-BitSaxBot";
                CPH.AddUserToGroup(newDj, "DeeJay");
		}else{
                newDj = userInput;
        }
        CPH.SetGlobalVar("newDj", newDj);
        CPH.LogInfo($"Set newDJ to :: {newDj}");

        return true;
    }
}
