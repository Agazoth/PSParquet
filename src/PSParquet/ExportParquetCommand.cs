using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.IO;
using System;
using Parquet.Data;
using System.Linq;
using Parquet.Rows;
using Parquet.File;
using Parquet.Schema;
using Parquet.Serialization;
using Parquet;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Dynamic;
using PSParquet;
using System.ComponentModel.Design;
using System.Threading.Tasks;

namespace PSParquet
{
    [Cmdlet("Export", "Parquet")]
    [OutputType(typeof(PSCustomObject))]
    public class ExportParquetCommand : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = false,
            ValueFromPipelineByPropertyName = false)]
        public FileInfo FilePath { get; set; }
        [Parameter(
        Mandatory = true,
        Position = 1,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = false)]
        public PSObject[] InputObject { get; set; }
        [Parameter(
        Mandatory = false,
        Position = 2,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false)]
        public SwitchParameter PassThru { get; set; }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin block");
        }
        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override async void ProcessRecord()
        {
            WriteVerbose("Using: " + FilePath.FullName);
            PSObject[] io = InputObject;
            var writeParquet = PSParquet.WriteParquetFile(io, FilePath.FullName);
            WriteVerbose($"Write concluded with status {(writeParquet.IsCompletedSuccessfully ? "Failed" : "Succeded")}");


            if (PassThru)
            {
                WriteObject(InputObject);
            }
        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
