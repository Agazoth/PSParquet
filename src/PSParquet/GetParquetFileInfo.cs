using System.Management.Automation;
using System.IO;


namespace PSParquet
{
    [Cmdlet("Get", "ParquetFileInfo")]
    [OutputType(typeof(PSCustomObject))]
    public class GetParquetFileInfoCommand : PSCmdlet
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
            var fileInfo = PSParquet.GetParquetFileInfo(FilePath.FullName).GetAwaiter().GetResult();
            WriteObject(fileInfo);
        }
        protected override void EndProcessing()
        {

        }
    }
}