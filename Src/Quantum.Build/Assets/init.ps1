param($installPath, $toolsPath, $package)
 
foreach ($_ in Get-Module | ?{$_.Name -eq 'VSExtensionsModule'})
{
    Remove-Module 'QuantumCodeGenModule'
}
 
Import-Module (Join-Path $toolsPath QuantumCodeGenModule.psm1)