Describe "Module tests" {
    BeforeAll {
        $ModulePath = Join-Path (Join-Path $(Split-Path $PSScriptRoot) 'output') 'PSParquet'
        Write-Verbose $ModulePath -Verbose
        Import-Module $ModulePath -Force
        $tempFile = New-TemporaryFile
        $data = Get-ChildItem -Path $(Split-Path $PSScriptRoot) -Recurse -File
        Export-Parquet -FilePath $tempFile.FullName -InputObject $data
        $Content = Import-Parquet -FilePath $tempFile.FullName
    }
    it "Export data to file" {
        $tempFile.Length | Should -BeGreaterThan 1000
    }
    it "Import data from file" {
        $Content | Should -Not -Be $Null
    }
    it "PSParquet.dll is in the import" {
        $Content.name | Should -Contain 'PSParquet.dll'
    }
}

