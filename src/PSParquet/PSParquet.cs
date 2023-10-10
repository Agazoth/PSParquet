using Parquet.Schema;
using Parquet;
using Parquet.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using PSParquet.Classes;
using System.Collections;

namespace PSParquet
{
    public class PSParquet
    {
        string FilePath { get; set; }

        public static async Task<List<dynamic>> GetParquetObjects(string FilePath)
        {
            List<dynamic> objects = new List<dynamic>();
            Stream fileStream = File.OpenRead(FilePath);
            using (ParquetReader reader = await ParquetReader.CreateAsync(fileStream, leaveStreamOpen: false))
            {
                var all = await reader.ReadEntireRowGroupAsync();
                string[] headers = new string[all.Length];
                for (int i = 0; i < all.Length; i++)
                {
                    headers[i] = all[i].Field.Name.ToString();
                }

                for (int i = 0; i < all[0].Data.Length; i++)
                {
                    PSObject pso = new();
                    for (int j = 0; j < headers.Length; j++)
                    {
                        pso.Properties.Add(new PSNoteProperty(headers[j], all[j].Data.GetValue(i)));
                    }
                    objects.Add(pso);

                }
            }
            return objects;
        }
        public static object GetTypedNull(Type type)
        {
            dynamic val;
            switch (type.ToString())
            {
                case ("System.String"):
                    val = "";
                    break;
                case ("System.DateTime"):
                    val = DateTime.MinValue;
                    break;
                default:
                    val = 0;
                    break;

            }
            return Convert.ChangeType(val, type);
        }
        public static async Task WriteParquetFile(PSObject[] inputObject, string filePath)
        {
            var properties = inputObject[0].Members.Where(w => w.GetType() == typeof(PSNoteProperty)).ToList();

            List<ParquetData> parquetData = properties.Select(s => new ParquetData
            {
                Parameter = s.Name,
                Type = Type.GetType(s.TypeNameOfValue),
                Data = (from o in inputObject select o.Properties[s.Name].Value).ToArray()
            }).ToList();

            var schema = new ParquetSchema(
                parquetData.Select(s => new DataField(s.Parameter, s.Type, false))
            );


            using (Stream fileStream = File.OpenWrite(filePath))
            {
                using (ParquetWriter parquetWriter = await ParquetWriter.CreateAsync(schema, fileStream))
                {
                    parquetWriter.CompressionMethod = CompressionMethod.Gzip;
                    parquetWriter.CompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    // create a new row group in the file
                    using (ParquetRowGroupWriter groupWriter = parquetWriter.CreateRowGroup())
                    {
                        for (int i = 0; i < parquetData.Count; i++)
                        {
                            Array arr = Array.CreateInstance(parquetData[i].Type, parquetData[i].Data.Count());
                            var data = parquetData[i].Data.Select(s => s ?? GetTypedNull(parquetData[i].Type)).ToArray();
                            Array.Copy(data, arr, parquetData[i].Data.Count());
                            await groupWriter.WriteColumnAsync(new DataColumn(schema.DataFields[i], arr));
                        }
                    }
                }
            }
        }
    }
}
