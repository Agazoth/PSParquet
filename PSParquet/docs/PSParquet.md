---
Module Name: PSParquet
Module Guid: {{ Update Module Guid }}
Download Help Link: {{ Update Download Link }}
Help Version: {{ Update Help Version }}
Locale: {{ Update Locale }}
---

# PSParquet Module
## Description
This module contains modules to import and export data from and to Parquet files. Export objects must be "flat" objects. Nested objects are not (currently) supported. Strong typed parameters are preserved. PSObject based on Int like values are converted to doubles. This is due to the often large amount of objects in the arrays exported to Parquet.

## PSParquet Cmdlets
### [Export-Parquet](Export-Parquet.md)
Export objects to Parquet file

### [Import-Parquet](Import-Parquet.md)
Import objects from Parquet file

