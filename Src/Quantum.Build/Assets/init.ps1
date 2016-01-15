Write-Host "Install is running"

function Q-CodeGen {
    [CmdletBinding()]
    param (
        [switch]$verbose
    )
    PROCESS {
        Write "Hello, Quantum!"
    }
}