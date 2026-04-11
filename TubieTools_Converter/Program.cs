using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


// i convert all  json files to a csv to use in ML.NET
class Program
{
    static void Main(string[] args)
    {
        try
        {
            string jsonFilePath = "reviews.json"; // Path to your JSON file
            string csvFilePath = "output.csv";  // Path for the CSV output

            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine($"Error: File '{jsonFilePath}' not found.");
                return;
            }

            // Read JSON file
            string jsonContent = File.ReadAllText(jsonFilePath);

            // Parse JSON into a JArray (works for arrays or single objects)
            JArray jsonArray;
            try
            {
                jsonArray = JArray.Parse(jsonContent);
            }
            catch (JsonReaderException)
            {
                // If it's a single object, wrap it in an array
                JObject singleObject = JObject.Parse(jsonContent);
                jsonArray = new JArray { singleObject };
            }

            // Convert JArray to a list of dictionaries for CSVHelper
            var records = new List<Dictionary<string, object>>();
            foreach (JObject obj in jsonArray)
            {
                var dict = new Dictionary<string, object>();
                foreach (var prop in obj.Properties())
                {
                    dict[prop.Name] = prop.Value?.ToString();
                }
                records.Add(dict);
            }

            // Write to CSV
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                if (records.Count > 0)
                {
                    // Write header
                    foreach (var header in records[0].Keys)
                    {
                        csv.WriteField(header);
                    }
                    csv.NextRecord();

                    // Write rows
                    foreach (var record in records)
                    {
                        foreach (var value in record.Values)
                        {
                            csv.WriteField(value);
                        }
                        csv.NextRecord();
                    }
                }
            }

            Console.WriteLine($"✅ Conversion complete! CSV saved to '{csvFilePath}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }

}