---
external help file: PSParquet.dll-Help.xml
Module Name: PSParquet
online version:
schema: 2.0.0
---

# Import-Parquet

## SYNOPSIS
Import objects from Parquet file

## SYNTAX

```
Import-Parquet [-FilePath] <FileInfo> [-FirstNGroups <Int32>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Imports a dataset from a Parquet file

## EXAMPLES

### Example 1
```
PS C:\> $FilesFromParquet = Import-Parquet -FilePath C:\temp\files.parquet
PS C:\> $FilesFromParquet[0]


PSPath              : Microsoft.PowerShell.Core\FileSystem::C:\dev\PSParquet\output\PSParquet\changelog.txt
PSParentPath        : Microsoft.PowerShell.Core\FileSystem::C:\dev\PSParquet\output\PSParquet
PSChildName         : changelog.txt
PSDrive             :
PSProvider          :
PSIsContainer       : False
Mode                : -a---
ModeWithoutHardLink : -a---
VersionInfo         : File:             C:\dev\PSParquet\output\PSParquet\changelog.txt
                      InternalName:
                      OriginalFilename:
                      FileVersion:
                      FileDescription:
                      Product:
                      ProductVersion:
                      Debug:            False
                      Patched:          False
                      PreRelease:       False
                      PrivateBuild:     False
                      SpecialBuild:     False
                      Language:

BaseName            : changelog
Target              :
LinkType            :
Length              : 0
DirectoryName       : C:\dev\PSParquet\output\PSParquet
Directory           : C:\dev\PSParquet\output\PSParquet
IsReadOnly          : False
FullName            : C:\dev\PSParquet\output\PSParquet\changelog.txt
Extension           : .txt
Name                : changelog.txt
Exists              : True
CreationTime        : 12-11-2021 14:37:46 +00:00
CreationTimeUtc     : 12-11-2021 13:37:46 +00:00
LastAccessTime      : 12-11-2021 18:12:59 +00:00
LastAccessTimeUtc   : 12-11-2021 17:12:59 +00:00
LastWriteTime       : 10-09-2021 07:40:41 +00:00
LastWriteTimeUtc    : 10-09-2021 05:40:41 +00:00
LinkTarget          :
Attributes          : Archive
```

Imports a Parquet file and stores the objects in the FilesFromParquet variable

## PARAMETERS

### -FilePath
Path to the Parquet file

```yaml
Type: FileInfo
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FirstNGroups
{{ Fill FirstNGroups Description }}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### System.Management.Automation.PSCustomObject
## NOTES

## RELATED LINKS
