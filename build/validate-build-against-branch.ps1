<#

Validates the build against a compare branch (typically master).  Does the following

 1.  If this branch is the compare branch, then succeed
 2.  If this branch is not the compare branch, but is equivalent to the compare branch (i.e.,
     same commit), then cancel the build
 3.  If this branch is one or more commits behind the compare branch, fail the build
 4.  If this branch has a SemVer version number less than or equal to the compare branch,
     fail the build. 

See https://www.jamesqmurphy.com/blog/brewing-the-blog-6 for an explanation
 
#>

param (
    [String] $CompareBranch = 'master',

    [String] $VersionFile  = 'build/azure-pipelines.yml'
)

. "$PSScriptRoot/common-functions.ps1"

# Remove "refs/heads/" in front of compare branch name (if present)
if ($CompareBranch.StartsWith('refs/heads/')) {
    $CompareBranch = $CompareBranch.Replace('refs/heads/','')
}

# Check if this is, in fact, the compare branch itself
if ($env:BUILD_SOURCEBRANCH -eq "refs/heads/$CompareBranch") {
    Write-Output "This branch is the compare branch ($CompareBranch); validation successful"
    exit 0
}

<#
When Azure DevOps clones our repo, it clones a shallow copy.  Then instead of checking out a branch, it does a
checkout of the specific commit ID, like this:

git checkout --progress --force <commit-id>

This leaves the repo in 'Detached HEAD' state, which means that it's not on any branch.  Furthermore, since the
clone is shallow, it doesn't have any knowledge of any of the other branches (which is actually a good thing).  So
we need to explicitly fetch the compare branch.  Note that git will name the pointer to the fetched branch as FETCH_HEAD
#>
& git fetch origin $CompareBranch

# Get the commit IDs of this branch and the compare branch
$thisBranchCommit = & git log -1 --format="%H"
$compareBranchCommit = & git log -1 FETCH_HEAD --format="%H"
Write-Output "This branch is at commit $thisBranchCommit"
Write-Output "Branch $CompareBranch is at commit $compareBranchCommit"

# If this branch has the same commit as the compare branch, then cancel the build
if ($thisBranchCommit -eq $compareBranchCommit) {
    Write-Output "This branch is equivalent to branch $CompareBranch; canceling build"
    Invoke-AzureDevOpsWebApi -Api "_apis/build/builds/$($env:BUILD_BUILDID)" -Method PATCH -Version '4.1'
    exit 0
}

# Get the number of commits behind and ahead
$behindAhead = & git rev-list --left-right --count "FETCH_HEAD...$thisBranchCommit"
if ($behindAhead -match '(\d+)\s+(\d+)') {
    $commitsBehind = [int]::Parse($Matches[1])
    $commitsAhead = [int]::Parse($Matches[2])
}
else {
    Write-AzureDevOpsBuildError "Could not parse `$behindAhead ($behindAhead)"
    exit 1
}
Write-Output "This branch is $commitsBehind commits behind and $commitsAhead commits ahead of branch $CompareBranch"
if ($commitsBehind -gt 0) {
    Write-AzureDevOpsBuildError "This branch must be 0 commits behind branch $CompareBranch.  Do a git merge from branch $CompareBranch into this branch."
    exit 1
}

# Define function to read version number from file contents
# This function will be called for this branch and for the compare branch
function Get-VersionInfoFromFileContents {
    param(
        [string[]] $FileContents
    )

    $versionMajor = -1
    $versionMinor = -1
    $versionPatch = -1
    $FileContents | Where-Object { $_ -match '\s+version(Major|Minor|Patch):\s+(\d+)' } | ForEach-Object {
        switch($Matches[1]) {
            'Major' { $versionMajor = [int]::Parse($Matches[2]) }
            'Minor' { $versionMinor = [int]::Parse($Matches[2]) }
            'Patch' { $versionPatch = [int]::Parse($Matches[2]) }
        }
    }

    return New-Object PSObject -Property @{Major=$versionMajor; Minor=$versionMinor; Patch=$versionPatch}
}

# Get this branch's version
$contents = Get-Content $VersionFile
$thisBranchVersion = Get-VersionInfoFromFileContents $contents
Write-Output "This branch version is $($thisBranchVersion.Major).$($thisBranchVersion.Minor).$($thisBranchVersion.Patch)"

# Get compare branch's version
$contents = & git --no-pager show "FETCH_HEAD:$VersionFile"
$compareBranchVersion = Get-VersionInfoFromFileContents $contents
Write-Output "Branch $CompareBranch version is $($compareBranchVersion.Major).$($compareBranchVersion.Minor).$($compareBranchVersion.Patch)"


# Check if this branch's version is greater than the compare branch's version
$thisBranchVersionGreater = ($thisBranchVersion.Major -gt $compareBranchVersion.Major) -or
                            (($thisBranchVersion.Major -eq $compareBranchVersion.Major) -and ($thisBranchVersion.Minor -gt $compareBranchVersion.Minor)) -or
                            (($thisBranchVersion.Major -eq $compareBranchVersion.Major) -and ($thisBranchVersion.Minor -eq $compareBranchVersion.Minor) -and ($thisBranchVersion.Patch -gt $compareBranchVersion.Patch))
if (-not $thisBranchVersionGreater) {
    Write-AzureDevOpsBuildError "This branch must have a higher version number than branch $CompareBranch"
    exit 1
}

Write-Output "This branch successfully validated against branch $CompareBranch"


Write-Output "For fun, try canceling the build"
Invoke-AzureDevOpsWebApi -Api "_apis/build/builds/$($env:BUILD_BUILDID)" -Method PATCH -Version '4.1'


exit 0
