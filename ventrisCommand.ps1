param(
    [switch] $Debug,
	[string] $parameter="Engine",
	[string] $value="TrueSpring",
	[string] $engine="TrueSpring",
	[string] $basePath="C:\Users\mchel\Repos\ChillSaxTools",
	[string] $manPath="C:\Program Files\Streamer.bot-x64-0.1.19\data\ChillSaxTools",
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
	$inputDevices = Get-MidiInputDeviceInformation
	$outputDevices = Get-MidiOutputDeviceInformation

	if ($Debug) {
		Write-Host "  "
		Write-Host "MIDI Input Devices ========================= " -ForegroundColor Cyan
		foreach ($device in $inputDevices)
		{
			Write-Host "  "
			Write-Host "  Name: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.Name  -ForegroundColor Red
			Write-Host "  ID: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.Id  -ForegroundColor Red
			Write-Host "  IsDefault: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.IsDefault  -ForegroundColor Red
			Write-Host "  IsEnabled: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.IsEnabled  -ForegroundColor Red
		}

		Write-Host "  "
		Write-Host "MIDI Output Devices ========================= " -ForegroundColor Cyan

		foreach ($device in $outputDevices)
		{
			Write-Host "  "
			Write-Host "  Name: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.Name  -ForegroundColor Red
			Write-Host "  ID: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.Id  -ForegroundColor Red
			Write-Host "  IsDefault: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.IsDefault  -ForegroundColor Red
			Write-Host "  IsEnabled: " -NoNewline -ForegroundColor DarkGray; Write-Host $device.IsEnabled  -ForegroundColor Red
		}
	}

	$ventrisMidi = $outputDevices | Where-Object {$_.NAME -like "*One Series Ventris Reverb*"}
	$ventrisId = $ventrisMidi.ID
	$outputPort = Get-MidiOutputPort -id $ventrisId
	Write-Host "Ventris ID: $($ventrisId)"
}
catch {
	WriteLog "Failed to retrieve ventris midi"
	WriteLog $_
	exit
}

############################################## Send CC Message
try {
	Write-Host "Send CC Message to Ventris"
	foreach ($request in $ventrisRequests) {
		Send-MidiControlChangeMessage -Port $outputPort -Channel 0 -Controller $request.CC -Value $request.VAL
	}
}
catch {
	WriteLog "Failed to send midi cc"
	WriteLog $_
	exit
}
