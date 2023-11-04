using System.Management.Automation;
using System.IO;


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
        public FileInfo FilePath { get; set; }

        protected override void BeginProcessing()
        {
            if (!FilePath.Exists)
            {
                throw new FileNotFoundException($"File not found: {FilePath}");
            }
        }

        protected override void ProcessRecord()
        {
            WriteVerbose($"File: {FilePath}");
        }


        protected override void EndProcessing()
        {
            var objs = PSParquet.GetParquetObjects(FilePath.FullName).GetAwaiter().GetResult();
            WriteObject(objs);
        }
    }
}
