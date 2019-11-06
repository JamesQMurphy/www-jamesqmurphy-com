<#

Validates the build against a compare branch (typically master).  Evaluates the following:

 1.  If this branch *is* the compare branch, then succeed
 2.  If this branch is not the compare branch, but is equivalent to the compare branch (i.e.,
     same commit), then succeed
 3.  If this branch is one or more commits behind the compare branch, then fail the build
 4.  If this branch has a SemVer version number less than or equal to the compare branch,
     fail the build. 

See https://www.jamesqmurphy.com/blog/brewing-the-blog-6 for an explanation
 
#>

param (
    [String] $CompareBranch = 'master',

    [String] $VersionFile  = 'azure-pipelines.yml'
)

. "$PSScriptRoot/common-functions.ps1"

# Remove "refs/heads/" in front of compare branch name (if present)
if ($CompareBranch.StartsWith('refs/heads/')) {
    $CompareBranch = $CompareBranch.Replace('refs/heads/','')
}

# Check if this is, in fact, the compare branch itself
if ($env:GITHUB_REF -eq "refs/heads/$CompareBranch") {
    Write-Output "This branch is the compare branch ($CompareBranch); validation successful."
    exit 0
}

# Fetch the compare branch
& git fetch origin $CompareBranch

# Get the commit IDs of this branch and the compare branch
$thisBranchCommit = & git log -1 --format="%H"
$compareBranchCommit = & git log -1 FETCH_HEAD --format="%H"
Write-Output "This branch is at commit $thisBranchCommit"
Write-Output "Branch $CompareBranch is at commit $compareBranchCommit"

# If this branch has the same commit as the compare branch, then succeed
if ($thisBranchCommit -eq $compareBranchCommit) {
    Write-Output "This branch is equivalent to branch $CompareBranch; validation successful."
    exit 0
}

# Get the number of commits behind and ahead
$behindAhead = & git rev-list --left-right --count "FETCH_HEAD...$thisBranchCommit"
if ($behindAhead -match '(\d+)\s+(\d+)') {
    $commitsBehind = [int]::Parse($Matches[1])
    $commitsAhead = [int]::Parse($Matches[2])
}
else {
    Write-Error "Could not parse `$behindAhead ($behindAhead)"
    exit 1
}
Write-Output "This branch is $commitsBehind commits behind and $commitsAhead commits ahead of branch $CompareBranch"
if ($commitsBehind -gt 0) {
    Write-Error "This branch must be 0 commits behind branch $CompareBranch; validation failed."
    Write-Error "Perform a git merge from branch $CompareBranch into this branch."
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
    $FileContents | Where-Object { $_ -match '\s+VERSION_(MAJOR|MINOR|PATCH):\s+(\d+)' } | ForEach-Object {
        switch($Matches[1]) {
            'MAJOR' { $versionMajor = [int]::Parse($Matches[2]) }
            'MINOR' { $versionMinor = [int]::Parse($Matches[2]) }
            'PATCH' { $versionPatch = [int]::Parse($Matches[2]) }
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
    Write-Error "This branch must have a higher version number than branch $CompareBranch; validation failed."
    Write-Error "Increase the version number on this branch to something higher than $($compareBranchVersion.Major).$($compareBranchVersion.Minor).$($compareBranchVersion.Patch)."
    exit 1
}

Write-Output "This branch successfully validated against branch $CompareBranch."
exit 0
