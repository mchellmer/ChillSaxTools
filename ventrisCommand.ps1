param(
    [switch] $Debug,
	[string] $parameter="Engine",
	[string] $value="TrueSpring",
	[string] $engine="TrueSpring",
	[string] $basePath="C:\Users\mchel\Repos\ChillSaxTools",
	[string] $manPath="C:\Users\mchel\OneDrive\Documents\0_Store\Twitch\Manifests",
	[string] $manFile="manifest.json"
)

############################################## Log setup
$Logfile = "$basePath\logs\ventrisCommand.log"
function WriteLog
{
	Param ([string]$LogString)
	$Stamp = (Get-Date).toString("yyyy/MM/dd HH:mm:ss")
	$LogMessage = "$Stamp $LogString"
	Add-content $LogFile -value $LogMessage
    Start-Sleep .02
}

############################################## Import Modules
$psMidiPath = "$basePath\lib\Windows-10-PowerShell-MIDI-master\PeteBrown.PowerShellMidi\bin\Debug\PeteBrown.PowerShellMidi.dll"
$ventrisJsonPath = "$basePath\data\ventrisMap.json"
try {
	Import-Module $psMidiPath
}
catch {
	WriteLog "Failed to import module at $psMidiPath"
	WriteLog $_
	exit
}

############################################## Ventris Parameter Mapping
try {
	$ventrisManifest = Get-Content -Raw -Path "$manPath/$manFile" | ConvertFrom-Json
	$ventrisMap = Get-Content -Raw -Path $ventrisJsonPath | ConvertFrom-Json
	function SetVentris {
		param (
			[string] $parameter,
			[string] $value,
			[string] $engine,
			[object] $ventrisMap
		)
		try {
			$ventrisValue = $value
			if ($parameter -eq "engine") {
				$ventrisValue = $ventrisMap.ParameterValue.$value.Value
			} elseif ($parameter -in @("control1", "control2" )) {
				$parameter = $ventrisMap.ParameterValue.$engine.$parameter
				$transformValue = $ventrisMap.ParameterCC.$parameter.MAX -lt 127;
				if ($transformValue) {
					$transformMax = $ventrisMap.ParameterCC.$parameter.MAX
					$ventrisValue = [int]($value/127*$transformMax)
					WriteLog "Transforming $value out of 127 to $ventrisValue out of $transformMax"
				}
			}
			$ventrisCC = $ventrisMap.ParameterCC.$parameter.CC
			
			WriteLog "Received request: $parameter to $value - Sending CC: $ventrisCC,$ventrisValue"
			return $ventrisCC, $ventrisValue
		}
		catch {
			WriteLog "Failed to generate CC command"
			WriteLog $_
			exit
		}
		
	}

	$ventrisRequests = @()
	foreach ($request in $ventrisManifest) {
		$requestCC, $requestValue = SetVentris -parameter $request.parameter -value $request.value -engine $request.engine -ventrisMap $ventrisMap
		$request = @{CC=$requestCC;VAL=$requestValue}
		$ventrisRequests += $request
	}
}
catch {
	WriteLog "Failed to generate requests from manifest file"
	WriteLog $_
	exit
}

############################################## Retrieve Ventris Midi Object
Write-Host "Retrieve midi devices"
try {
	$outputDevices = Get-MidiOutputDeviceInformation
	$inputDevices = Get-MidiInputDeviceInformation

	if ($Debug) {
		WriteLog "MIDI Output Devices ========================= "
		foreach ($device in $outputDevices)
		{
			WriteLog "Name: $($device.Name), ID: $($device.Id)"
		}
		WriteLog "MIDI Input Devices ========================= "
		foreach ($device in $inputDevices)
		{
			WriteLog "Name: $($device.Name), ID: $($device.Id)"
		}
	}

	$ventrisMidi = $outputDevices | Where-Object {$_.Name -like "*One Series Ventris Reverb*"}
	$ventrisId = $ventrisMidi.Id
	$outputPort = Get-MidiOutputPort -id $ventrisId
	Write-Host "Ventris ID: $($ventrisId)"
}
catch {
	WriteLog "Failed to retrieve ventris midi"
	WriteLog $_
	WriteLog "Ventris ID: $($ventrisId)"
	exit
}

############################################## Send CC Message
try {
	Write-Host "Send CC Message to Ventris"
	foreach ($request in $ventrisRequests) {
		if ($request.CC -eq 1) {
			$timeoutSec = 1
		} else {
			$timeoutSec = .1
		}
		Send-MidiControlChangeMessage -Port $outputPort -Channel 0 -Controller $request.CC -Value $request.VAL
		Start-Sleep $timeoutSec
	}
}
catch {
	WriteLog "Failed to send midi cc"
	WriteLog $_
	exit
}
