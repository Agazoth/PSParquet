Properties {
    # Variables for versioning (Build version is updated accordingly)
    $Script:IncrementMajorVersion = $IncrementMajorVersion
    $Script:IncrementMinorVersion = $IncrementMinorVersion
    # Basic naming variables
    $Script:BuildDir = Split-Path $psake.build_script_file
    $Script:ModuleName = $(Split-Path $psake.build_script_file -leaf) -split '\.' | Select-Object -first 1
    $Script:OutputModule = Join-Path (Join-Path $Script:BuildDir 'output') $Script:ModuleName
    # Variables for ps1 files
    $Script:DevModuleFolder = Join-Path $Script:BuildDir $Script:ModuleName
    $Script:PublicFiles = @()
    $Script:PrivateFiles = @()
    # Variables for primary 3. party dlls
    $Script:DevBinfolder = Join-String $Script:DevModuleFolder "bin"
    $Script:NestedModules = @()
    $Script:NestedModuleFiles = @()
    # Variables for binary Powershell modules (not done yet)
    $Script:SrcDir = (Join-Path $Script:BuildDir 'src')
    $Script:DevSrcFolder = Join-Path $Script:SrcDir $Script:ModuleName
    $Script:DevOutputFolder = Join-Path $Script:DevSrcFolder "bin"
    $Script:DebugDllFolder = Join-Path (Join-Path $Script:DevOutputFolder "Debug") "net7.0" 
    $Script:OutputModuleBin = Join-Path $Script:OutputModule 'bin'
    $Script:CmdletsToExport = @()
    $Script:Cmdlets = @()
    # psm1 and psd1
    $Script:psm1 = Join-Path $Script:DevModuleFolder $($ModuleName + ".psm1")
    $Script:psd1 = Join-Path $Script:DevModuleFolder $($ModuleName + ".psd1")
}

Task Default -depends BuildBinaries, Initialize3PartyBinaries, InitializeModuleFile, InitializeManifestFile, UpdateHelp

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
    if (Test-Path $Script:DevSrcFolder) {
        dotnet build "$Script:DevSrcFolder"
        if (!$(Test-Path $Script:OutputModuleBin)) {
            "Creating $Script:OutputModuleBin"
            $null = New-Item -ItemType Directory -Path $Script:OutputModuleBin -Force
        }
        $Script:CmdletsToExport = foreach ($dll in $(Get-ChildItem $Script:DebugDllFolder -filter *dll)) {
            Write-Verbose "Copying $dll to $Script:OutputModuleBin" 
            $PlacedDll = Copy-Item -Path $dll -Destination $Script:OutputModuleBin -force -PassThru
            if ($PlacedDll.BaseName -eq $Script:ModuleName) {
                $PlacedDll.FullName | foreach { Write-Verbose $_ }
                $cs = Start-Job -ScriptBlock { Import-Module $args  -PassThru | Select-Object -ExpandProperty ExportedCommands } -ArgumentList $dll.FullName | Wait-Job | Receive-Job
                Update-ModuleManifest -Path $Script:psd1 -FunctionsToExport $cs.Keys -NestedModules "bin/$($PlacedDll.name)"
            }
        }
    }
}

Task Initialize3PartyBinaries {
    if (test-path $Script:DevBinfolder) {
        #The dlls in the root bin folder gets imported
        $Script:NestedModules = Get-ChildItem $Script:DevBinfolder -File -Filter *$($Script:ModuleName).dll | Select-Object -ExpandProperty Name | foreach { 'bin/{0}' -f $_ }
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
    $ScriptBlock = {
        $psm1File, $DevModuleFolder, $Modulename = $args
        $Docs = Join-Path $DevModuleFolder 'docs'
        $EnUs = Join-Path $DevModuleFolder 'en-US'
        Import-Module $psm1File -Force -Global
        if (!$(Test-Path $Docs)) {
            New-MarkdownHelp -WithModulePage -Module $Modulename -OutputFolder $Docs
        }
        Update-MarkdownHelpModule $Docs -RefreshModulePage -Force
        if (!$(Test-Path $EnUs)) {
            New-Item -ItemType Directory -Path $EnUs
        }
        New-ExternalHelp $Docs -OutputPath $EnUs -Force
    }
    Start-Job -ScriptBlock $ScriptBlock -ArgumentList $Script:psm1, $Script:DevModuleFolder, $Script:ModuleName | Wait-Job | Receive-Job
    Get-Job | Remove-Job

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
    if (!$env:psakeDeploy) {
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
    }
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
    start-job -scriptblock {
        $VerbosePreference = $args
        $pesterConfig = @{Path = './Tests/' }
        $c = New-PesterContainer @pesterConfig
        Invoke-Pester -Container $c -Output Detailed
    } -ArgumentList $VerbosePreference | Wait-Job | Receive-job
    Get-Job | Remove-Job
}

Task Info {
    Write-Host $($psake | convertto-json)
}