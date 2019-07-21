# Common PowerShell functions

function Write-AzureDevOpsLoggingCommand {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateSet("task","artifact","build")]
        [string]$Area,

        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$Action,

        [Parameter(Mandatory=$false)]
        [string]$Message = '',

        [Parameter(Mandatory=$false)]
        [HashTable]$Properties = @{}
    )

    $Area = $Area.ToLowerInvariant()
    $Action = $Action.ToLowerInvariant()

    $sb = New-Object 'System.Text.StringBuilder' -ArgumentList ' '
    $Properties.Keys.GetEnumerator() | ForEach-Object {
        $thisValue = $Properties[$_]
        $sb.Append("$_=$thisValue;") | Out-Null
    }
    $propString = $sb.ToString()
    if ([String]::IsNullOrWhiteSpace($propString)) {
        $propString = ''
    }

    Write-Output "##vso[$Area.$Action$propString]$Message"
}


function Write-AzureDevOpsBuildError {
    param(
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string] $Message
    )

    Write-AzureDevOpsLoggingCommand -Area task -Action logissue -Message $Message -Properties @{type='error'}
}


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
        [string] $Body,
 
        [Parameter(Mandatory=$false)]
        [string] $ContentType,
 
        [Parameter(Mandatory=$false)]
        [switch] $Raw
     )
 
    $azureUrl = $env:SYSTEM_TEAMFOUNDATIONCOLLECTIONURI
    $teamProjectId = $env:SYSTEM_TEAMPROJECTID
    $headers = @{Authorization = "Bearer $($env:SYSTEM_ACCESSTOKEN)"}
    if ( -not ([String]::IsNullOrWhiteSpace($ContentType)) ) {
        $headers.Add('Content-Type',$ContentType)
    }
 
    $url = "$($azureUrl)$($teamProjectId)/_apis/$($Api)?api-version=$Version"
    if (![String]::IsNullOrEmpty($QueryString)) {
        $url = "$url&$QueryString"
    }
    Write-Host "Attempting to $Method $url"
    if ([String]::IsNullOrWhiteSpace($Body)) {
        $results = Invoke-WebRequest -Method $Method -UseBasicParsing -Uri $url -Headers $headers 
    }
    else {
        $results = Invoke-WebRequest -Method $Method -UseBasicParsing -Uri $url -Headers $headers -Body $Body
    }
    if ($Raw) {
        return $results
    }
    else {
        $retVal = $results.Content | ConvertFrom-Json
        return $retVal
    }
}

