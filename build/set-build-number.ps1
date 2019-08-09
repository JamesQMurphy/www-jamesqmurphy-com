<#

Sets the build number based on the following rules, where N = 1, 2, 3

1.  If the build is for the master branch, set it to Major.Minor.Patch[.N]
2.  If the build is for a release branch, set it to Major.Minor.Patch-rc[.N]
3.  If the build is for a pull request, set it to PR-xxxx[.N]
4.  Otherwise, set it to Major.Minor.Patch-<branchname>[.N]

See https://www.jamesqmurphy.com/blog/brewing-the-blog-6 for an explanation
 
#>

. "$PSScriptRoot/common-functions.ps1"

$baseBuildNumber = switch -regex ($env:BUILD_SOURCEBRANCH) {
    
    'refs/heads/master'
        { "$($env:VERSIONMAJOR).$($env:VERSIONMINOR).$($env:VERSIONPATCH)" }
    
    'refs/heads/releases/*'
        { "$($env:VERSIONMAJOR).$($env:VERSIONMINOR).$($env:VERSIONPATCH)-rc" }

    'refs/pull/(\d+)/merge'
        { "PR-$($Matches[1])" }

    default
        { "$($env:VERSIONMAJOR).$($env:VERSIONMINOR).$($env:VERSIONPATCH)-$($env:BUILD_SOURCEBRANCHNAME)" }

}

Write-Output "Base build number: $baseBuildNumber"

#Determine value of N to tack on the end
$N = 0

# Retrieve builds for this definition that match the pattern
$previousBuildNumbers = Invoke-AzureDevOpsWebApi 'build/builds' -Version '5.0' -QueryString "definitions=$($env:SYSTEM_DEFINITIONID)&buildNumber=$baseBuildNumber*" | Select-Object -ExpandProperty Value | Select-Object -ExpandProperty buildNumber

# Find the highest build number in the previous builds
if (($previousBuildNumbers -ne $null) -and (@($previousBuildNumbers).Count -gt 0)) {
    
    Write-Output "Previous builds found that match $($baseBuildNumber): "
    @($previousBuildNumbers) | ForEach-Object {
        Write-Output " $_"
        if ($_ -eq $baseBuildNumber) {
            $N = 1
        }
    }

    @($previousBuildNumbers) | Where-Object {$_ -match "$baseBuildNumber\.\d+`$" } | ForEach-Object {
        $split = $_ -split '\.'
        $previousN = [Int32]::Parse($split[($split.Length - 1)])
        if ($previousN -ge $N) {
            $N = $previousN + 1
        }
    }
}
else {
    Write-Output "No previous builds found beginning with $baseBuildNumber"
}

# Set actual build number        
if ($N -eq 0) {
    $newBuildNumber = $baseBuildNumber
}
else {
    $newBuildNumber = "$baseBuildNumber.$N"
}

Write-AzureDevOpsLoggingCommand -Area build -Action updatebuildnumber -Message $newBuildNumber
