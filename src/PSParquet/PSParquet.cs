using Parquet.Schema;
using Parquet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

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
    }
}
