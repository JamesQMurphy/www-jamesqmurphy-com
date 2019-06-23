<#
Validates the build against a compare branch (typically master).  Ensures the following:

 1.  The current branch is equivalent (same commit) to the compare branch, or

 2.  The current branch is not behind the compare branch and has a higher SemVer version number

If neither of the conditions above are met, the script will fail the build.
 
#>

param (
    [String] $CompareBranch = 'master',

    [String] $VersionFile  = 'build/azure-pipelines.yml'
)

# Remove "refs/heads/" if in front of compare branch name
if ($CompareBranch.StartsWith('refs/heads/')) {
    $CompareBranch = $CompareBranch.Replace('refs/heads/','')
}

<#
When Azure DevOps clones our repo, it clones a shallow copy.  Then instead of checking out a branch, it does a
checkout of the specific commit ID, like this:

git checkout --progress --force <commit-id>

This leaves the repo in 'Detached HEAD' state, which means that it's not on any branch.  Furthermore, since the
clone is shallow, it doesn't have *any* knowledge of *any* of the branches (which is actually a good thing).  So
we need to fetch that branch.  Note that git will name the pointer to the fetched branch as FETCH_HEAD
#>
& git fetch origin $CompareBranch

# Get the commits of each branch
$thisBranchCommit = & git log -1 --format="%H"
$compareBranchCommit = & git log -1 FETCH_HEAD --format="%H"
Write-Output "This branch is at commit $thisBranchCommit"
Write-Output "Branch $CompareBranch is at commit $compareBranchCommit"

if ($thisBranchCommit -eq $compareBranchCommit) {
    Write-Output "This branch is equivalent to branch $CompareBranch"
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
    Write-Error "This branch must be 0 commits behind branch $CompareBranch"
    exit 1
}

$thisBranchVersionMajor = [int]::Parse($env:VERSIONMAJOR)
$thisBranchVersionMinor = [int]::Parse($env:VERSIONMINOR)
$thisBranchVersionPatch = [int]::Parse($env:VERSIONPATCH)

# Read version from other branch's file
$compareBranchVersionMajor = -1
$compareBranchVersionMinor = -1
$compareBranchVersionPatch = -1
& git --no-pager show "FETCH_HEAD:$VersionFile" | Where-Object { $_ -match '\s+version(Major|Minor|Patch):\s+(\d+)' } | ForEach-Object {
    switch($Matches[1]) {
        'Major' { $compareBranchVersionMajor = [int]::Parse($Matches[2]) }
        'Minor' { $compareBranchVersionMinor = [int]::Parse($Matches[2]) }
        'Patch' { $compareBranchVersionPatch = [int]::Parse($Matches[2]) }
    }
}

Write-Output "This branch version is $($thisBranchVersionMajor).$($thisBranchVersionMinor).$($thisBranchVersionPatch)"
Write-Output "Branch $CompareBranch version is $($compareBranchVersionMajor).$($compareBranchVersionMinor).$($compareBranchVersionPatch)"

$thisBranchVersionGreater = ($thisBranchVersionMajor -gt $compareBranchVersionMajor) -or
                            (($thisBranchVersionMajor -eq $compareBranchVersionMajor) -and ($thisBranchVersionMinor -gt $compareBranchVersionMinor)) -or
                            (($thisBranchVersionMajor -eq $compareBranchVersionMajor) -and ($thisBranchVersionMinor -eq $compareBranchVersionMinor) -and ($thisBranchVersionPatch -gt $compareBranchVersionPatch))
if (-not $thisBranchVersionGreater) {
    Write-Error "This branch must have a higher version number than branch $CompareBranch"
    exit 1
}

Write-Output "This branch successfully validated against branch $CompareBranch"
