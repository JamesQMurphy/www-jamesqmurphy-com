# Validates the build number against the master branch

# Read version number from build file


. "$PSScriptRoot/common-functions.ps1"

Write-Output "This branch version is $($env:VERSIONMAJOR).$($env:VERSIONMINOR).$($env:VERSIONPATCH)"

# Read version variables from master branch
$masterVersionMajor = -1
$masterVersionMinor = -1
$masterVersionPatch = -1

<#
When Azure DevOps clones our repo, it clones a shallow copy.  Then instead of checking out a branch, it does a
checkout of the specific commit ID, like this:

git checkout --progress --force <commit-id>

This leaves the repo in 'Detached HEAD' state, which means that it's not on any branch.  Furthermore, since the
clone is shallow, it doesn't have *any* knowledge of *any* of the branches (which is actually a good thing).  But
since we need to compare against the version numbers in the master branch, we need to fetch that branch.
#>
& git fetch origin master

# Now we've fetched the master branch, but git names the pointer FETCH_HEAD, not master
& git --no-pager show FETCH_HEAD:build/azure-pipelines.yml | Where-Object { $_ -match '\s+version(Major|Minor|Patch):\s+(\d+)' } | ForEach-Object {
    switch($Matches[1]) {
        'Major' { $masterVersionMajor = $Matches[2] }
        'Minor' { $masterVersionMinor = $Matches[2] }
        'Patch' { $masterVersionPatch = $Matches[2] }
    }
}

Write-Output "The master branch version is $($masterVersionMajor).$($masterVersionMinor).$($masterVersionPatch)"
