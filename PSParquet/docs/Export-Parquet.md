---
external help file: PSParquet.dll-Help.xml
Module Name: PSParquet
online version:
schema: 2.0.0
---

# Export-Parquet

## SYNOPSIS
Export objects to Parquet file

## SYNTAX

```
Export-Parquet [-FilePath] <FileInfo> [-InputObject] <PSObject[]> [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Exports an array of objects to a parquet file. Make sure your objects are formatted correctly. Array, List and HashTable parameters are not supported. Convert these types to basic types before running Export-Parquet.

## EXAMPLES

### Example 1
```
PS C:\> $files = Get-ChildItem -File -Recurse
PS C:\> Export-Parquet -InputObject $files -FilePath C:\temp\files.parquet
```

Get a list of files recursively from the current directory and export them tp a Parquet file

### Example 2
```
PS C:\> $files = Get-ChildItem -File -Recurse
PS C:\> $files | Export-Parquet -FilePath C:\temp\files.parquet
```

Get a list of files recursively from the current directory and export them tp a Parquet file

### Example 3
```
$File = C:\Temp\Test.parquet
$data = 1..100 | foreach {
    [pscustomobject]@{
        Date        = (Get-Date).AddHours($_)
        Int32       = $_
        TypedInt    = [int]$_
        IntWithNull = (($_ % 3 -eq 0) ? $null : $_)
        Text        = "Iteration $_"
    }
}
$data | Export-Parquet -FilePath $File -Force
```

Exports the objects to C:\Temp\Test.parquet and overwrites the file if it already exists.

### Example 4
```
$File = C:\Temp\Test.parquet
$data = 1..10 | foreach {
    [pscustomobject]@{
        name = $_ 
        nested=@{
            nest="blah $_"
        }
    }
}
$data | Export-Parquet -FilePath $File -Force

WARNING: InputObjects contains unsupported values. Transform the data prior to running Export-Parquet.
```

The data contains nested object and returns a warning. No data will be exported.

## PARAMETERS

### -FilePath
Path to the Parquet file. Existing file will be overwritten

```yaml
Type: FileInfo
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
An array of objects to export to Parquet.

```yaml
Type: PSObject[]
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -PassThru
Passes the objects to the pipeline

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: 2
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Management.Automation.PSCustomObject
## NOTES

## RELATED LINKS
