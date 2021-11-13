Properties {
    # Variables for versioning (Build version is updated accordingly)
    $Script:IncrementMajorVersion = $IncrementMajorVersion
    $Script:IncrementMinorVersion = $IncrementMinorVersion
    # Basic naming variables
    $Script:BuildDir = Split-Path $psake.build_script_file
    $Script:ModuleName = $(Split-Path $psake.build_script_file -leaf) -split '\.' | Select-Object -first 1
    $Script:OutputModule = $BuildDir + '\output\' + $ModuleName
    # Variables for ps1 files
    $Script:DevModuleFolder = "$BuildDir\$ModuleName"
    $Script:PublicFiles = @()
    $Script:PrivateFiles = @()
    # Variables for primary 3. party dlls
    $Script:DevBinfolder = "$Script:DevModuleFolder\bin"
    $Script:NestedModules = @()
    $Script:NestedModuleFiles = @()
    # Variables for binary Powershell modules (not done yet)
    $Script:DevSrcFolder = "$BuildDir\src\$ModuleName"
    $Script:DevOutputFolder = "$Script:OutputModule\bin"
    $Script:CmdletsToExport = @()
    $Script:Cmdlets = @()
    # psm1 and psd1
    $Script:psm1 = "$DevModuleFolder\$ModuleName.psm1"
    $Script:psd1 = "$DevModuleFolder\$ModuleName.psd1"
}

Task Default -depends Initialize3PartyBinaries, InitializeModuleFile, UpdateHelp, InitializeManifestFile

Task Cleanup {

}

Task InitializeBinary {
    Push-Location $Script:BuildDir
    Set-Location src
    dotnet new -i Microsoft.PowerShell.Standard.Module.Template
    dotnet new psmodule -n $Script:ModuleName
}

Task BuildBinaries {
    # Do compilation of binaries, if there are any
    if (Test-Path "$Script:BuildDir\src") {
        if (!$(Test-Path $Script:DevSrcFolder)) { New-Item -ItemType Directory -Path $Script:DevSrcFolder -Force }
        dotnet build "$Script:DevSrcFolder"
        $Script:CmdletsToExport = foreach ($dll in $(Get-ChildItem "$Script:BuildDir\src\$Script:ModuleName\bin\Debug\netstandard2.0" -filter *dll)) {
            if (!$(Test-Path $Script:DevOutputFolder)) { New-Item -ItemType Directory -Path $Script:DevOutputFolder -Force }
            Copy-Item -Path $dll -Destination $Script:DevOutputFolder -force
            if ($dll.BaseName -eq $Script:ModuleName) {
                $cs = Import-Module $dll.fullname -PassThru | Select-Object -ExpandProperty ExportedCommands
                Update-ModuleManifest -Path $Script:psd1 -FunctionsToExport $cs -NestedModules "bin/$($dll.name)"
            }
        }
    }
}

Task Initialize3PartyBinaries {
    if (test-path $Script:DevBinfolder) {
        #The dlls in the root bin folder gets imported
        $Script:NestedModules = Get-ChildItem $Script:DevBinfolder -File -Filter *dll | Select-Object -ExpandProperty Name | foreach { 'bin/{0}' -f $_ }
        $Script:NestedModuleFiles = Get-ChildItem $Script:DevBinfolder -Filter *dll -Recurse
        "Found {0} 3. party dlls" -f $Script:NestedModuleFiles.count
    }
}



Task InitializeModuleFile {
    $Script:ClassFiles = Get-ChildItem $Script:DevModuleFolder\Classes\*.ps1 -ErrorAction SilentlyContinue
    "Class files found: $($Script:ClassFiles.count)"
    $Script:PublicFiles = Get-ChildItem $Script:DevModuleFolder\Public\*.ps1 -ErrorAction SilentlyContinue
    "Public files found: $($Script:PublicFiles.count)"
    $Script:PrivateFiles = Get-ChildItem $Script:DevModuleFolder\Private\*.ps1 -ErrorAction SilentlyContinue
    "Private files found: $($Script:PrivateFiles.count)"
    # Do content of ps1 files
    [string[]]$Content = @()
    foreach ($file in $($Script:PublicFiles, $Script:PrivateFiles, $Script:ClassFiles)) {
        "Adding $($file.fullname) ..."
        $Content += Get-Content $file.fullname
    }
    $Content | Out-File $Script:psm1 -force
}

Task UpdateHelp -depends InitializeModuleFile {
    Import-Module $script:psm1 -Force -Global
    if (!$(Test-Path $Script:DevModuleFolder\docs)) {
        New-MarkdownHelp -WithModulePage -Module $Script:ModuleName -OutputFolder $Script:DevModuleFolder\docs
    }
    Update-MarkdownHelpModule $Script:DevModuleFolder\docs -RefreshModulePage -Force
    New-ExternalHelp $Script:DevModuleFolder\docs -OutputPath $Script:DevModuleFolder\en-US\ -Force
}

Task InitializeManifestFile -depends InitializeModuleFile {
    $UpdateSplat = @{
        Path = $Script:psd1
    }
    if ($Public) {
        "Adding Functions: $($Script:PublicFiles.BaseName -join ', ') to Manifest"
        $UpdateSplat.Add("FunctionsToExport", $Script:PublicFiles.BaseName)
    }
    if ($CmdletsToExport) {
        "Adding Cmdlets: $($Script:CmdletsToExport.Keys -join ', ') to Manifest"
        $UpdateSplat.Add("CmdletsToExport", $Script:CmdletsToExport.Keys)
    }
    if ($NestedModules) {
        "Adding Nested Modules: $($Script:NestedModules -join ', ') to Manifest"
        $UpdateSplat.Add("NestedModules", $Script:NestedModules)
    }
    $ModuleManifestVersion = Test-ModuleManifest $Script:psd1 | Select-Object -ExpandProperty Version
    $Major = $ModuleManifestVersion.Major
    $Minor = $ModuleManifestVersion.Minor
    $Build = $ModuleManifestVersion.Build
    if ($Script:IncrementMajorVersion) { $Major++; $Minor = 0; $Build = -1 }
    elseif ($Script:IncrementMinorVersion) { $Minor++; $Build = -1 }
    $Build++
    $VersionString = $("{0}.{1}.{2}" -f $Major, $Minor, $Build)
    "Updating version to $VersionString"
    $UpdateSplat.Add("ModuleVersion", [system.version]$VersionString)
    Update-ModuleManifest @UpdateSplat
}

Task Build -depends Default {
    if (!$(Test-Path $OutputModule)) {
        "Creating Output Directory: $OutputModule"
        New-Item -Path $OutputModule -ItemType Directory -Force
    } 
    Get-ChildItem $Script:DevModuleFolder -File | Copy-Item -Destination $OutputModule -Force
    if ($Script:NestedModuleFiles) {
        "Adding {0} 3. party dlls to the module" -f $Script:NestedModuleFiles
        Copy-Item $Script:DevBinfolder -Destination $OutputModule -Recurse -Force
    }
    Get-ChildItem $Script:DevModuleFolder -Directory -Exclude docs, Private, Public, Classes | Copy-Item -Destination $OutputModule -Recurse -Force
}

Task BuildPSSecretsExtension -depends Default {
    $ExtensionFolder = Join-path $OutputModule 'ImplementingModule'
    if (!$(Test-Path $OutputModule)) {
        "Creating Output Directory: $OutputModule"
        New-Item -Path $OutputModule -ItemType Directory -Force
    } 
    if (!$(Test-Path $OutputModule)) {
        "Creating Extension Sub Directory: $ExtensionFolder"
        New-Item -Path $ExtensionFolder -ItemType Directory -Force
    } 
    Get-ChildItem $Script:DevModuleFolder -File -filter *psd1 | Copy-Item -Destination $OutputModule -Force
    Get-ChildItem $Script:DevModuleFolder -File -filter *psm1 | Copy-Item -Destination $ExtensionFolder\ImplementingModule.psm1
    $Text = "@{ ModuleVersion = '1.0';RootModule = '.\ImplementingModule.psm1';FunctionsToExport = @('Set-Secret','Get-Secret','Remove-Secret','Get-SecretInfo')}"
    $Text | Out-File $ExtensionFolder\ImplementingModule.psd1
}

Task Test -depends Build {
    If (Test-Path "C:\Program Files\WindowsPowerShell\Modules\$Script:ModuleName") {
        Remove-Item "C:\Program Files\WindowsPowerShell\Modules\$Script:ModuleName" -Recurse -force
    }
    Copy-Item -Path $OutputModule -Destination "C:\Program Files\WindowsPowerShell\Modules" -Recurse
}

Task Info {
    Write-Host $($psake | convertto-json)
}