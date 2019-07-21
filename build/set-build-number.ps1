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
$previousBuilds = Invoke-AzureDevOpsWebApi '/_apis/build/builds' -Version '5.0' -QueryString "definitions=$($env:SYSTEM_DEFINITIONID)&buildNumber=$baseBuildNumber*" -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Value

# Find the highest build number in the previous builds
if ($previousBuilds -ne $null) {
    $N = 1
    $previousBuilds | Where-Object {$_ -like "$baseBuildNumber.*" } | ForEach-Object {
        $previousN = [Int32]::Parse(($_ -split '.')[1])
        if ($previousN -ge $N) {
            $N = $previousN + 1
        }
    }
}

# Set actual build number        
if ($N -eq 0) {
    $newBuildNumber = $baseBuildNumber
}
else {
    $newBuildNumber = "$baseBuildNumber.$N"
}

Write-AzureDevOpsLoggingCommand -Area build -Action updatebuildnumber -Message $newBuildNumber
