cls

$regexGuid = '[0-9a-f]{8}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{12}'
$timestamp = 1574163075599
$requestObjects = @{}


function Split-Headers {
    param (
        [string] $headers,

        [string] $prefix
    )

    # Do a raw split by comma-space.  This will actually split too much and create "orphans"
    $rawSplit = $Matches[1] -split ', '

    # Look for any headers without the equals sign -- these are the orphans.
    # If found, reunite them with the previous header
    $headersList = @()
    $lastHeader = $null
    $rawSplit | ForEach-Object {
        if ($_.Contains('=')) {
            if ($lastHeader -ne $null) { $headersList += @($lastHeader) }
            $lastHeader = $_
        }
        else {
            $lastHeader += ", $_"
        }
    }
    if ($lastHeader -ne $null) { $headersList += @($lastHeader) }

    # Return a series of objects on the pipeline
    $headersList | ForEach-Object {
        $headerSplit = $_ -split '=',2
        New-Object -TypeName PSObject -Property @{
            name = "$prefix.$($headerSplit[0].ToLowerInvariant())"
            value = $headerSplit[1]
        }
    }
}


Get-Content ("$PSScriptRoot\sample.txt") | ForEach-Object {

    $thisLogEvent = New-Object -TypeName PSObject -Property @{ message = $_ }

    # See if message matches patten:  (GUID) logMessage
    if ($thisLogEvent.message -match "^\(($regexGuid)\) (.+)`$") {
        $requestId = $Matches[1]
        $logMessage = $Matches[2]

        if ($requestObjects.ContainsKey($requestId)) {
            $requestObj = $requestObjects[ $requestId ]
        }
        else {
            $requestObj = New-Object -TypeName PSObject -Property @{
                id = $requestId
                timestamp = [DateTimeOffset]::FromUnixTimeMilliseconds($thisLogEvent.timestamp).UtcDateTime.ToString("O")
                'aws.loggroup' = $event.logGroup
                'aws.logstream' = $event.logStream
            }
        }

        # Look for log messages with specific patterns
        switch -Regex ($logMessage) {

            '^HTTP Method: (\w+), Resource Path: (.+)$' {
                $requestObj |
                    Add-Member -MemberType NoteProperty -Name method       -Value $Matches[1] -PassThru |
                    Add-Member -MemberType NoteProperty -Name resourcepath -Value $Matches[2]
            }

            '^Method request headers: \{(.*)\}$' {
                Split-Headers -headers $Matches[1] -prefix request | ForEach-Object {
                    $requestObj | Add-Member -MemberType NoteProperty -Name $_.name -Value $_.value
                }
            }

            '^Method response headers: \{(.*)\}$' {
                Split-Headers -headers $Matches[1] -prefix response | ForEach-Object {
                    $requestObj | Add-Member -MemberType NoteProperty -Name $_.name -Value $_.value
                }
            }

            '^Method completed with status: (\d+)$' {
                $requestObj |
                    Add-Member -MemberType NoteProperty -Name httpstatus -Value $Matches[1]
            }
        }

        $requestObjects[ $requestId ] = $requestObj
    }

}


$requestObjects.Values | ConvertTo-Json 