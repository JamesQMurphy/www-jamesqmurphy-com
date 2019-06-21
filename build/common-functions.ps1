# Common PowerShell functions

function Invoke-AzureDevOpsWebApi {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string] $Api,
 
        [Parameter(Mandatory=$false)]
        [string] $Method = 'Get',
 
        [Parameter(Mandatory=$false)]
        [string] $Version = '1.0',
 
        [Parameter(Mandatory=$false)]
        [string] $QueryString,
 
        [Parameter(Mandatory=$false)]
        [switch] $Raw
     )
 
    $azureUrl = $env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
    $teamProjectId = $env:SYSTEM_TEAMPROJECTID
    $authHeaders = @{Authorization = "Bearer $($env:SYSTEM_ACCESSTOKEN)"}
 
    $url = "$($azureUrl)$($teamProjectId)$($Api)?api-version=$Version"
    if (![String]::IsNullOrEmpty($QueryString)) {
        $url = "$url&$QueryString"
    }
    $results = Invoke-WebRequest -Method $Method -UseBasicParsing -Uri $Url -Headers $authHeaders 
    if ($Raw) {
        return $results
    }
    else {
        $retVal = $results.Content | ConvertFrom-Json
        return $retVal
    }
}

