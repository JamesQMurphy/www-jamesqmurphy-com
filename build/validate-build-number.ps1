# Validates the build number against the master branch

# Read version number from build file


. "$PSScriptRoot/common-functions.ps1"

Write-Output "This branch version is $($env:VERSIONMAJOR).$($env:VERSIONMINOR).$($env:VERSIONPATCH)"

# Read version variables from master branch
$masterVersionMajor = -1
$masterVersionMinor = -1
$masterVersionPatch = -1
& git --no-pager show --no-patch origin 'master:build/azure-pipelines.yml' | Where-Object { $_ -match '\s+version(Major|Minor|Patch):\s+(\d+)' } | ForEach-Object {
    switch($Matches[1]) {
        'Major' { $masterVersionMajor = $Matches[2] }
        'Minor' { $masterVersionMinor = $Matches[2] }
        'Patch' { $masterVersionPatch = $Matches[2] }
    }
}

Write-Output "The master branch version is $($masterVersionMajor).$($masterVersionMinor).$($masterVersionPatch)"
