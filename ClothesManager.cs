using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;

namespace TMGSSaveEditor.Core
{
    public class ClothingData
    {
        public int ClothesNo { get; set; }
        public string FileName { get; set; }
        public int Color { get; set; }
    }

    public class ClothingRoot
    {
        public List<ClothingData> Clothes { get; set; }
    }

    public class ClothesManager
    {
        private string[] _clothingNames;
        private readonly string _projectName;

        public ClothesManager(string projectName)
        {
            _projectName = projectName;
        }

        public string[] GetClothingNames()
        {
            if (_clothingNames == null)
            {
                _clothingNames = LoadClothingNames();
            }
            return _clothingNames;
        }

        private string[] LoadClothingNames()
        {
            string[] finalNames = new string[510];
            for (int i = 0; i < finalNames.Length; i++)
            {
                finalNames[i] = "Unknown Clothing";
            }

            if (string.IsNullOrEmpty(_projectName)) return finalNames;

            try
            {
                List<string> csvNamesList = new List<string>();
                try
                {
                    using var csvStream = AssetLoader.Open(new Uri($"avares://{_projectName}/Assets/Clothes Names.csv"));
                    using var csvReader = new StreamReader(csvStream);
                    string line;
                    while ((line = csvReader.ReadLine()) != null)
                    {
                        csvNamesList.Add(line);
                    }
                }
                catch { }

                string[] csvNames = csvNamesList.ToArray();

                string json = "";
                try
                {
                    using var jsonStream = AssetLoader.Open(new Uri($"avares://{_projectName}/Assets/clothes.json"));
                    using var jsonReader = new StreamReader(jsonStream);
                    json = jsonReader.ReadToEnd();
                }
                catch { }

                if (string.IsNullOrWhiteSpace(json)) return finalNames;

                ClothingRoot rootObject = JsonSerializer.Deserialize<ClothingRoot>(json);

                int maxClothesNo = -1;
                if (rootObject != null && rootObject.Clothes != null)
                {
                    foreach (var item in rootObject.Clothes)
                    {
                        if (item.ClothesNo > maxClothesNo)
                        {
                            maxClothesNo = item.ClothesNo;
                        }
                    }
                }

                int exactSize = maxClothesNo >= 0 ? maxClothesNo + 1 : 0;
                finalNames = new string[exactSize];

                for (int i = 0; i < finalNames.Length; i++)
                {
                    finalNames[i] = "Unknown Clothing"; // Default fallback for gaps
                }

                Dictionary<int, string> colors = new Dictionary<int, string>
                {
                    {0, "None"}, {1, "White"}, {2, "Black"}, {3, "Red"},
                    {4, "Blue"}, {5, "Yellow"}, {6, "Green"}, {7, "Purple"},
                    {8, "Aqua"}, {9, "Orange"}, {10, "Pink"}, {11, "Brown"}, {12, "Grey"}
                };

                if (rootObject != null && rootObject.Clothes != null)
                {
                    foreach (ClothingData item in rootObject.Clothes)
                    {
                        int id = item.ClothesNo;

                        if (id >= 0 && id < finalNames.Length)
                        {
                            string baseName = (id < csvNames.Length && !string.IsNullOrWhiteSpace(csvNames[id]))
                                ? csvNames[id].Trim()
                                : item.FileName;

                            string colorName = colors.ContainsKey(item.Color) ? colors[item.Color] : "Unknown";

                            finalNames[id] = $"{baseName} ({colorName})";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading clothes: {ex.Message}");
            }

            return finalNames;
        }
    }
}