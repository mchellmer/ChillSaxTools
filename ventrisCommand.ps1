param(
    [switch] $Debug,
	[string] $command="Set",
	[string] $parameter="Engine",
	[string] $value="TrueSpring",
	[string] $engine="TrueSpring"
  )

$psMidiPath = ".\lib\Windows-10-PowerShell-MIDI-master\PeteBrown.PowerShellMidi\bin\Debug\PeteBrown.PowerShellMidi.dll"
$ventrisJsonPath = ".\data\ventrisMap.json"
Import-Module $psMidiPath

############################################## Ventris Parameter Mapping
$ventrisMap = Get-Content -Raw -Path $ventrisJsonPath | ConvertFrom-Json
$ventrisValue = $value
if ($parameter -eq "Engine") {
	$ventrisValue = $ventrisMap.ParameterValue.$value.Value
} elseif ($parameter -in @("Control1", "Control2" )) {
	$parameter = $ventrisMap.ParameterValue.$engine.$parameter
}
$ventrisCC = $ventrisMap.ParameterCC.$parameter.CC

Write-Host "Received request: $command $parameter to $value - Sending CC: $ventrisCC,$ventrisValue"


############################################## Retrieve Ventris Midi Object
Write-Host "Retrieve midi devices"
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

############################################## Send CC Message
Write-Host "Send CC Message to Ventris"
Send-MidiControlChangeMessage -Port $outputPort -Channel 0 -Controller $ventrisCC -Value $ventrisValue
