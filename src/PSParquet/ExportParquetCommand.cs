using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Parquet.Data;
using Parquet;
using Parquet.Schema;


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
                    var valueType = props[p].TypeNameOfValue;
                    //WriteVerbose("Doing: " + name);
                    //WriteVerbose("Type: " + valueType);
                    Type type = Type.GetType(valueType) ?? typeof(string);
                    bool convert = false;
                    switch (type.ToString())
                    {
                        case "System.DateTime":
                            type = typeof(DateTimeOffset);
                            break;
                        case "System.Int32":
                            type = typeof(int);
                            break;
                        case "System.Int64":
                            type = typeof(long);
                            break;
                        case "System.Double":
                            type = typeof(double);
                            break;
                        case "System.Decimal":
                            type = typeof(decimal);
                            break;
                        case "System.String":
                            type = typeof(string);
                            break;
                        case "System.Boolean":
                            type = typeof(bool);
                            break;
                        default:
                            type = typeof(string);
                            convert = true;
                            break;
                    }
                    //WriteVerbose("CalculatedType: " + type);
                    cleanprops.Add(name, type);
                    fields.Add(new DataField(name, type));
                    DataField field = new DataField(name, type);
                    //WriteVerbose("Getting Data from " + name);
                    //WriteVerbose("Getting Data type " + type.ToString());
                    List<dynamic> data = new List<dynamic>();
                    Array typedArray = Array.CreateInstance(type, InputObject.Length);
                    //WriteVerbose("Array type " + typedArray.GetType().ToString());
                    for (int i = 0; i < InputObject.Length; i++)
                    {
                        var ob = new PSObject(InputObject.GetValue(i)).Members.ToList();
                        var value = ob.Where(x => x.Name == name).FirstOrDefault().Value;
                        if (convert)
                        {
                            value = value.ToString();
                        }
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

            WriteVerbose("Objects: " + InputObject.Length);

            ParquetConvert.SerializeAsync(cleanprops, FilePath);


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
