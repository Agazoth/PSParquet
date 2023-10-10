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
Export-Parquet [-FilePath] <String> [-InputObject] <Object> [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Esports an array of objects to a parquet file

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

## PARAMETERS

### -FilePath
Path to the Parquet file. Existing file will be overwritten

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
An array of objects to export to Parquet

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Passes the files to the pipeline

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
