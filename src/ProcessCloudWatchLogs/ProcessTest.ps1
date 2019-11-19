cls

$regexGuid = '[0-9a-f]{8}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{12}'
$timestamp = 1574163075599
$requestObjects = @{}

Get-Content ("$PSScriptRoot\sample.txt") | ForEach-Object {

    if ($_ -match "^\(($regexGuid)\) (.+)`$") {
        $requestId = $Matches[1]
        $logMessage = $Matches[2]

        if ($requestObjects.ContainsKey($requestId)) {
            $requestObj = $requestObjects[ $requestId ]
        }
        else {
            $requestObj = New-Object -TypeName PSObject
        }


        switch -Regex ($logMessage) {

            '^HTTP Method: (\w+), Resource Path: (.+)$' {
                $requestObj |
                    Add-Member -MemberType NoteProperty -Name Method       -Value $Matches[1] -PassThru |
                    Add-Member -MemberType NoteProperty -Name ResourcePath -Value $Matches[2] -PassThru |
                    Add-Member -MemberType NoteProperty -Name Timestamp    -Value ([DateTimeOffset]::FromUnixTimeMilliseconds($timestamp).UtcDateTime.ToString("O"))
            }

            'Method request headers: \{(.*)\}$' {
                $rawSplit = $Matches[1] -split ', '
                $headers = @()
                $lastHeader = $null
                $rawSplit | ForEach-Object {
                    if ($_.Contains('=')) {
                        if ($lastHeader -ne $null) { $headers += @($lastHeader) }
                        $lastHeader = $_
                    }
                    else {
                        $lastHeader += ", $_"
                    }
                }
                if ($lastHeader -ne $null) { $headers += @($lastHeader) }

                $headers | ForEach-Object {
                    $headerSplit = $_ -split '=',2
                    $requestObj | Add-Member -MemberType NoteProperty -Name "Header.$($headerSplit[0])" -Value $headerSplit[1]
                }
            }
        }

        $requestObjects[ $requestId ] = $requestObj
    }

}


$requestObjects.Values | ConvertTo-Json -Compress