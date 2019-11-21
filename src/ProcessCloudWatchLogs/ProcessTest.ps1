cls

$timestamp = 1574163075600

$logEvents = Get-Content ("$PSScriptRoot\sample.txt") | ForEach-Object {
    New-Object -TypeName PSObject -Property @{
        timestamp = $timestamp++
        message = $_.ToString()
    }
}


$event = @{
    logGroup = "testLogGroup"
    logStream = "testLogStream"
    logEvents = $logEvents
} 


