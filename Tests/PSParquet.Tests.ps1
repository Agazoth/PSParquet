Describe "Module tests" {
    BeforeAll {
        $ModulePath = Join-Path (Join-Path $(Split-Path $PSScriptRoot) 'output') 'PSParquet'
        Write-Verbose "Test Path: $ModulePath"
        Import-Module $ModulePath -Force
        $tempFile = New-TemporaryFile
        $data = 1..100 | foreach {
            [pscustomobject]@{
                Date        = (Get-Date).AddHours($_)
                Int32       = $_
                TypedInt    = [int]$_
                IntWithNull = (($_ % 3 -eq 0) ? $null : $_)
                Text        = "Iteration $_"
            }
        }
        Export-Parquet -FilePath $tempFile.FullName -InputObject $data -Force
        $Content = Import-Parquet -FilePath $tempFile.FullName
    }
    it "Export data to file" {
        $tempFile.Length | Should -BeGreaterThan 1000
    }
    it "Import data from file" {
        $Content.Count | Should -Be 100
    }
    it "Date is a DateTime" {
        ($Content[0].Date -is [DateTime]) | Should -Be $true
    }
    it "Int32 is a double" {
        ($Content[0].Int32 -is [double]) | Should -Be $true
    }
    it "IntWithNull is a double" {
        ($Content[2].IntWithNull -is [double]) | Should -Be $true
    }
    it "IntWithNull has 0" {
        ($Content.IntWithNull -contains 0) | Should -Be $true
    }
    it "TypedInt is an Int" {
        ($Content[0].TypedInt -is [Int32]) | Should -Be $true
    }
    it "Test is a string" {
        ($Content[0].Text -is [String]) | Should -Be $true
    }

    AfterAll {
        Remove-Item $tempFile -Force
    }
}

