cd ..
cd Src\.nuget
NuGet.exe install Rosalia -ExcludeVersion -Prerelease
NuGet.exe restore "../Quantum.Build/packages.config" -OutputDirectory "../packages"
cd Rosalia/tools
Rosalia.exe "../../../Quantum.Build/Quantum.Build.csproj"
pause