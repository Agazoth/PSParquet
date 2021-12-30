using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Parquet.Data;
using Parquet;

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
        }
        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            ProviderInfo provider;
            PSDriveInfo drive;
            var boundFile = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(FilePath, out provider, out drive);
            var fi = new FileInfo(boundFile);
            if (!fi.Exists)
            {
                throw new FileNotFoundException("File not found: " + boundFile);
            }
            WriteVerbose("Using: " + boundFile);
            WriteVerbose("Provider: " + provider);
            WriteVerbose("Drive: " + drive);
            Stream fileStream = fi.OpenRead();
            var parquetReader = new ParquetReader(fileStream);
            DataField[] dataFields = parquetReader.Schema.GetDataFields();
            var dictionary = new Dictionary<string, object>();
            foreach (DataField d in dataFields)
            {
                string s = d.Name;
                WriteVerbose(d.ToString());
                dictionary.Add(d.Name, d.Name);
            }

            PSObject obj = new PSObject(dictionary);
            var options = new ParquetOptions { TreatByteArrayAsString = true };
            //var pReader = ParquetReader.OpenFromFile(FilePath, options);
            string[] header = dataFields.Select(f => f.Name).ToArray();
            var dict = parquetReader.ReadAsTable();
            WriteVerbose("Adding rows");
            foreach (var row in dict)
            {
                //WriteVerbose("Doing " + row.ToString());
                object[] strarr = row.Values.Select(v => v ?? "").ToArray();
                ///object[] strarr = row.Values.Select(v => v).ToArray();
                //WriteVerbose("Zipping");
                var resdict = header.Zip(strarr, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
                var pso = new PSObject();
                foreach (var item in resdict)
                {
                    //WriteVerbose("Adding " + item.Key + " " + item.Value);
                    pso.Properties.Add(new PSNoteProperty(item.Key, item.Value));
                };
                pso.TypeNames.Add("ParquetObject");
                WriteObject(pso);
            };
            fileStream.Close();
            fileStream.Dispose();

        }

        // This method will be called once at the end of pipeline execution; if no input is received, this method is not called
        protected override void EndProcessing()
        {
            WriteVerbose("End!");
        }
    }
}
