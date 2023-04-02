$basePath="C:\Users\mchel\Repos\ChillSaxTools"
$ventrisJsonPath = "$basePath\data\ventrisMap.json"
$ventrisMap = Get-Content -Raw -Path $ventrisJsonPath | ConvertFrom-Json
$parameter = "HallSize"
$value = "20"
$transformValue = $ventrisMap.ParameterCC.$parameter.MAX -lt 127;
$transformMax = $ventrisMap.ParameterCC.$parameter.MAX
$ventrisValue = [int]($value/127*$transformMax)
Write-Host "$value to $($ventrisValue) out of $transformMax"