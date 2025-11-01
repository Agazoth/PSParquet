using Parquet.Schema;
using Parquet;
using Parquet.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using PSParquet.Classes;
using System.Collections;

namespace PSParquet
{
    public class PSParquet
    {
        string FilePath { get; set; }

        public static async Task<List<PSObject>> GetParquetObjects(string FilePath, int? FirstNGroups)
        {
            List<PSObject> objects = new List<PSObject>();
            Stream fileStream = File.OpenRead(FilePath);
            using (ParquetReader reader = await ParquetReader.CreateAsync(fileStream, leaveStreamOpen: false))
            {

                int iterator;
                if (!FirstNGroups.HasValue || FirstNGroups.Value ==0 || FirstNGroups.Value > reader.RowGroupCount)
                {
                    iterator = reader.RowGroupCount;
                }
                else
                {
                    iterator = FirstNGroups.Value;
                }

                for (int rg =0; rg < iterator; rg++)
                {
                    var all = await reader.ReadEntireRowGroupAsync(rg);

                    string[] headers = new string[all.Length];
                    for (int i =0; i < all.Length; i++)
                    {
                        headers[i] = all[i].Field.Name.ToString();
                    }

                    for (int rowIndex =0; rowIndex < all[0].Data.Length; rowIndex++)
                    {
                        PSObject pso = new();
                        for (int j =0; j < headers.Length; j++)
                        {
                            pso.Properties.Add(new PSNoteProperty(headers[j], all[j].Data.GetValue(rowIndex)));
                        }
                        objects.Add(pso);
                    }
                }
            }
            return objects;
        }

        public static object GetTypedValue(Type type, dynamic value = null)
        {
            dynamic valueResult = value;
            if (valueResult is null)
            {
                switch (type.ToString())
                {
                    case ("System.String"):
                        valueResult = "";
                        break;
                    case ("System.DateTime"):
                        valueResult = DateTime.MinValue;
                        break;
                    default:
                        valueResult =0;
                        break;
                }
            }
            if (valueResult is PSObject)
            {
                valueResult = ((PSObject)value).BaseObject;
            }
            return Convert.ChangeType(valueResult, type);
        }

        public static async Task<bool> WriteParquetFile(PSObject[] inputObject, string filePath)
        {
            var properties = inputObject[0].Members.Where(w => w.GetType() == typeof(PSNoteProperty)).ToList();
            bool dataIsValid = true;
            List<ParquetData> parquetData = properties.Select(s => new ParquetData
            {
                Parameter = s.Name,
                // Making sure ps-typed int32s have sufficient size for all objects
                Type = inputObject[0].Properties[s.Name].Value is PSObject && s.TypeNameOfValue == "System.Int32" ?
                    Type.GetType("System.Double") : 
                    Type.GetType(s.TypeNameOfValue),
                Data = (from o in inputObject select o.Properties[s.Name].Value).ToArray()
            }).ToList();

            ParquetSchema schema = new(new DataField("Column1", typeof(string)));

            try
            {
                schema = new ParquetSchema(
                    parquetData.Select(s => new DataField(s.Parameter, s.Type, false))
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Operation failed: {ex.Message}");
                dataIsValid = false;
                return dataIsValid;
            }


            using (Stream fileStream = File.OpenWrite(filePath))
            {
                using (ParquetWriter parquetWriter = await ParquetWriter.CreateAsync(schema, fileStream))
                {
                    parquetWriter.CompressionMethod = CompressionMethod.Gzip;
                    parquetWriter.CompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
                    // create a new row group in the file
                    using (ParquetRowGroupWriter groupWriter = parquetWriter.CreateRowGroup())
                    {
                        try
                        {
                            for (int i =0; i < parquetData.Count; i++)
                            {
                                Type type = parquetData[i].Type;
                                Int64 count = parquetData[i].Data.Count();
                                var rawData = parquetData[i].Data;
                                Array arr = Array.CreateInstance(type, count);
                                var data = rawData.Select(s => GetTypedValue(type, s)).ToArray();
                                Array.Copy(data, arr, parquetData[i].Data.Count());
                                await groupWriter.WriteColumnAsync(new DataColumn(schema.DataFields[i], arr));
                            }
                            return true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Operation failed: {ex.Message}");
                            dataIsValid = false;
                            return dataIsValid;
                        }
                    }
                }
            }
        }
    }
}
