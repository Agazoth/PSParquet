using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Parquet.Data;
using Parquet;
using IronCompress;
using Parquet.Rows;
//using static System.Runtime.InteropServices.JavaScript.JSType;
using Parquet.Schema;
using System.Dynamic;
using PSParquet;

namespace PSParquet
{
    [Cmdlet("Import", "Parquet")]
    [OutputType(typeof(PSCustomObject))]
    public class ImportParquetCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public string FilePath { get; set; }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
            WriteVerbose("File: " + FilePath);
            var fi = new FileInfo(FilePath);
            WriteVerbose("File exists: " + fi.Exists);
            if (!fi.Exists)
            {
                throw new FileNotFoundException("File not found: " + FilePath);
            }
        }
        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            WriteVerbose("File: " + FilePath);
            var fi = new FileInfo(FilePath);
            var objs = PSParquet.GetParquetObjects(FilePath).GetAwaiter().GetResult();
            WriteObject(objs);
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
