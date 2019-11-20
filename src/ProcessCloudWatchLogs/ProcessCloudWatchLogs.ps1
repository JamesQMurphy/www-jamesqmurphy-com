#Requires -Modules @{ModuleName='AWSPowerShell.NetCore';ModuleVersion='3.3.335.0'}

<#
    Publish with this pwsh command:
    Publish-AWSPowerShellLambda -ScriptPath .\ProcessCloudWatchLogs.ps1 -Name lambdaPowerShell -Region us-east-1
#>

<#
$LambdaInput = New-Object -TypeName PSCustomObject -Property @{
    awslogs = @{
        data = 'H4sIAAAAAAAAAHWPwQqCQBCGX0Xm7EFtK+smZBEUgXoLCdMhFtKV3akI8d0bLYmibvPPN3wz00CJxmQnTO41whwWQRIctmEcB6sQbFC3CjW3XW8kxpOpP+OC22d1Wml1qZkQGtoMsScxaczKN3plG8zlaHIta5KqWsozoTYw3/djzwhpLwivWFGHGpAFe7DL68JlBUk+l7KSN7tCOEJ4M3/qOI49vMHj+zCKdlFqLaU2ZHV2a4Ct/an0/ivdX8oYc1UVX860fQDQiMdxRQEAAA=='
    }
}
#>

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



# Decode the event
$ZipBytes = [System.Convert]::FromBase64String($LambdaInput.awslogs.data)
$ZipStream = New-Object System.IO.Memorystream
$ZipStream.Write($ZipBytes, 0, $ZipBytes.Length)
$ZipStream.Seek(0, [System.IO.SeekOrigin]::Begin) | Out-Null
$GZipStream = New-Object System.IO.Compression.GZipStream($ZipStream, [System.IO.Compression.CompressionMode]([System.IO.Compression.CompressionMode]::Decompress), $true)
$streamReader = New-Object System.IO.StreamReader($GZipStream, [System.Text.Encoding]::UTF8)
$event = $streamReader.ReadToEnd() | ConvertFrom-Json

# Process the log events and create request objects
$regexGuid = '[0-9a-f]{8}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{12}'
$requestObjects = @{}
$event.logEvents | ForEach-Object {

    $thisLogEvent = $_

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

# Write out each request object
$requestObjects.Values.GetEnumerator() | ForEach-Object {
    Write-Host ($_ | ConvertTo-Json -Compress)
}