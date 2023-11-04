using System.Management.Automation;
using System.Collections.Generic;
using System.IO;

namespace PSParquet
{
    [Cmdlet("Export", "Parquet", SupportsShouldProcess = true)]
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
        [Parameter(
        Mandatory = false,
        Position = 3,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false)]
        public SwitchParameter Force { get; set; }

        private readonly List<PSObject> inputObjects = new List<PSObject>();


        protected override void BeginProcessing()
        {
            WriteVerbose("Using: " + FilePath.FullName);
            if (FilePath.Exists && !Force)
            {
                Force = ShouldContinue(FilePath.FullName, "Overwrite the existing file?");
            }
            if (Force)
            {
                WriteVerbose($"Deleting: {FilePath}");
                FilePath.Delete();
            }
        }

        protected override void ProcessRecord()
        {
            if (!FilePath.Exists || Force)
            {
                WriteDebug("Adding to List");
                inputObjects.AddRange(InputObject);
            }
        }

        protected override async void EndProcessing()
        {
            if (!FilePath.Exists || Force)
            {
                var collectedObjects = inputObjects.ToArray();
                WriteVerbose($"Writing {collectedObjects.Length} objects to {FilePath.FullName}");
                bool result = PSParquet.WriteParquetFile(collectedObjects, FilePath.FullName).Result;
                if (!result)
                {
                    WriteWarning("InputObjects contains unsupported values. Transform the data prior to running Export-Parquet.");
                }
                else
                {
                    WriteVerbose($"InputObject has been exported to {FilePath}");
                }

                if (PassThru)
                {
                    WriteObject(collectedObjects);
                }
            }
        }
    }
}
