#Requires -Modules @{ModuleName='AWSPowerShell.NetCore';ModuleVersion='3.3.335.0'}

<#
$LambdaInput = New-Object -TypeName PSCustomObject -Property @{
    awslogs = @{
        data = 'H4sIAAAAAAAAAHWPwQqCQBCGX0Xm7EFtK+smZBEUgXoLCdMhFtKV3akI8d0bLYmibvPPN3wz00CJxmQnTO41whwWQRIctmEcB6sQbFC3CjW3XW8kxpOpP+OC22d1Wml1qZkQGtoMsScxaczKN3plG8zlaHIta5KqWsozoTYw3/djzwhpLwivWFGHGpAFe7DL68JlBUk+l7KSN7tCOEJ4M3/qOI49vMHj+zCKdlFqLaU2ZHV2a4Ct/an0/ivdX8oYc1UVX860fQDQiMdxRQEAAA=='
    }
}
#>

# Decode the event
$ZipBytes = [System.Convert]::FromBase64String($LambdaInput.awslogs.data)
$ZipStream = New-Object System.IO.Memorystream
$ZipStream.Write($ZipBytes, 0, $ZipBytes.Length)
$ZipStream.Seek(0, [System.IO.SeekOrigin]::Begin) | Out-Null
$GZipStream = New-Object System.IO.Compression.GZipStream($ZipStream, [System.IO.Compression.CompressionMode]([System.IO.Compression.CompressionMode]::Decompress), $true)
$streamReader = New-Object System.IO.StreamReader($GZipStream, [System.Text.Encoding]::UTF8)
$event = $streamReader.ReadToEnd() | ConvertFrom-Json


$event.logEvents | ForEach-Object {

    #Write-Host "$($_.id):$($_.timestamp): $($_.message)"

    switch -regex ($_.message) {
    
        '^\([0-9a-f]{8}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{4}[-][0-9a-f]{12}\)HTTP Method: (\w+), Resource Path: (.+)$' {
            Write-Host "-->HTTP $($Matches[1]) $($Matches[2])"
        }
    
        default {
        }
    }

}
