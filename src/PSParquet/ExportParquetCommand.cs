using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Parquet.Data;
using Parquet;


namespace AxModule
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
        public string FilePath { get; set; }
        [Parameter(
        Mandatory = true,
        Position = 1,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false)]
        public dynamic InputObject { get; set; }
        [Parameter(
        Mandatory = false,
        Position = 2,
        ValueFromPipeline = false,
        ValueFromPipelineByPropertyName = false)]
        public SwitchParameter PassThru { get; set; }

        // This method gets called once for each cmdlet in the pipeline when the pipeline starts executing
        protected override void BeginProcessing()
        {
            WriteVerbose("Begin!");
        }
        // This method will be called for each input received from the pipeline to this cmdlet; if no input is received, this method is not called
        protected override void ProcessRecord()
        {
            WriteVerbose("Using: " + FilePath);
            object first = InputObject.GetValue(0);
            WriteVerbose("First: " + first.ToString());
            var props = new PSObject(first).Members.ToList();
            Dictionary<string, object> cleanprops = new Dictionary<string, object>();
            List<DataField> fields = new List<DataField>();
            List<DataColumn> col = new List<DataColumn>();
            for (int p = 0; p < props.Count; p++)
            {
                if (props[p].MemberType.ToString().Contains("Property"))
                {
                    string name = props[p].Name;
                    WriteVerbose("Doing: " + name);
                    Type type = Type.GetType(props[p].TypeNameOfValue);
                    if (type == Type.GetType("System.DateTime"))
                    {
                        type = typeof(DateTimeOffset);
                    }
                    cleanprops.Add(name, type);
                    fields.Add(new DataField(name, type));
                    DataField field = new DataField(name, type);
                    WriteVerbose("Getting Data from " + name);
                    WriteVerbose("Getting Data type " + type.ToString());
                    List<dynamic> data = new List<dynamic>();
                    Array typedArray = Array.CreateInstance(type, InputObject.Length);
                    WriteVerbose("Array type " + typedArray.GetType().ToString());
                    for (int i = 0; i < InputObject.Length; i++)
                    {
                        var ob = new PSObject(InputObject.GetValue(i)).Members.ToList();
                        var value = ob.Where(x => x.Name == name).FirstOrDefault().Value;
                        try
                        {
                            if (type == typeof(DateTimeOffset))
                            {
                                value = DateTimeOffset.Parse(value.ToString());
                            }

                            typedArray.SetValue(value, i);
                        }
                        catch (Exception e)
                        {
                            WriteVerbose("Error: " + e.Message);
                        }
                    }
                    col.Add(new DataColumn(field, typedArray));

                }
            };

            WriteVerbose("Using: " + InputObject.Length);
            //create data columns with schema metadata and the data you need
            var idColumn = new DataColumn(
               new DataField<int>("id"),
               new int[] { 1, 2 });

            var cityColumn = new DataColumn(
               new DataField<string>("city"),
               new string[] { "London", "Derby" });

            // create file schema
            var schema = new Schema(fields);

            using (Stream fileStream = System.IO.File.OpenWrite(FilePath))
            {
                using (var parquetWriter = new ParquetWriter(schema, fileStream))
                {
                    // create a new row group in the file
                    using (ParquetRowGroupWriter groupWriter = parquetWriter.CreateRowGroup())
                    {
                        foreach (var column in col)
                        {
                            // write the data for the column
                            WriteVerbose("Writing: " + column.Field.Name);
                            groupWriter.WriteColumn(column);
                        }
                    }
                }
                fileStream.Close();
                fileStream.Dispose();
            }
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
