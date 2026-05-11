using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor importer: CSV -> FishDatabase ScriptableObject.
/// Usage: __Tools > Import > Fish CSV to FishDatabase__
/// Select a CSV file when prompted. The importer will create or update
/// `Assets/Data/FishDatabase.asset`.
/// 
/// CSV expected columns (names are case-sensitive to match parsing below):
/// FishType, Habitat, Attractor, Rarity, Difficulty, ID, Name, Description,
/// CatchTier, SpecialLootID, Experience, Movement, CatchMissionCount1,
/// CatchMissionCount2, CatchMissionCount3, BoonBonus
/// </summary>
public static class FishCsvImporter
{
    private const string DefaultOutputPath = "Assets/Data/FishDatabase.asset";

#if UNITY_EDITOR
    [MenuItem("Tools/Import/Fish CSV to FishDatabase")]
    public static void ImportCsvToSO()
    {
        string csvPath = EditorUtility.OpenFilePanel("Select Fish CSV", Application.dataPath, "csv");
        if (string.IsNullOrEmpty(csvPath))
            return;

        string csvText;
        try
        {
            csvText = File.ReadAllText(csvPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"FishCsvImporter: Failed to read CSV file: {e}");
            return;
        }

        var lines = SplitLines(csvText).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        if (lines.Count < 2)
        {
            Debug.LogError("FishCsvImporter: CSV contains no data or only header.");
            return;
        }

        var header = ParseCsvLine(lines[0]);
        var rows = lines.Skip(1).Select(ParseCsvLine).ToList();

        // Load or create FishDatabase asset
        Directory.CreateDirectory(Path.GetDirectoryName(DefaultOutputPath));
        var db = AssetDatabase.LoadAssetAtPath<FishDatabase>(DefaultOutputPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<FishDatabase>();
            AssetDatabase.CreateAsset(db, DefaultOutputPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"FishCsvImporter: Created new FishDatabase at {DefaultOutputPath}");
        }

        var fishes = new List<FishData>(rows.Count);
        for (int r = 0; r < rows.Count; r++)
        {
            var cols = rows[r];
            try
            {
                var fd = CreateFishDataFromRow(header, cols);
                fishes.Add(fd);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"FishCsvImporter: Failed parsing row {r + 2} : {ex.Message}");
            }
        }

        db.fishes = fishes;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = db;
        Debug.Log($"FishCsvImporter: Imported {fishes.Count} fish into {DefaultOutputPath}");
    }

    private static FishData CreateFishDataFromRow(string[] header, string[] cols)
    {
        string Get(string name)
        {
            for (int i = 0; i < header.Length; i++)
            {
                if (string.Equals(header[i].Trim(), name, StringComparison.OrdinalIgnoreCase))
                {
                    if (i < cols.Length) return cols[i].Trim();
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        FishData fd = new FishData();

        // Parse enums (case-insensitive). Default fallbacks used where appropriate.
        Enum.TryParse(Get("FishType"), true, out FishType ft);
        Enum.TryParse(Get("Habitat"), true, out Habitat hab);
        Enum.TryParse(Get("Attractor"), true, out Attractor att);
        Enum.TryParse(Get("Rarity"), true, out Rarity rar);
        Enum.TryParse(Get("Difficulty"), true, out Difficulty diff);

        int.TryParse(Get("ID"), out int id);
        int.TryParse(Get("CatchTier"), out int catchTier);
        int.TryParse(Get("SpecialLootID"), out int spid);
        int.TryParse(Get("Experience"), out int exp);
        int.TryParse(Get("CatchMissionCount1"), out int cm1);
        int.TryParse(Get("CatchMissionCount2"), out int cm2);
        int.TryParse(Get("CatchMissionCount3"), out int cm3);
        int.TryParse(Get("BoonBonus"), out int boon);

        fd.Id = id;
        fd.Name = Get("Name");
        fd.Description = Get("Description");
        fd.Type = ft;
        fd.Habitat = hab;
        fd.CatchTier = catchTier;
        fd.Attractor = att;
        fd.Rarity = rar;
        fd.SpecialLootId = spid;
        fd.Difficulty = diff;
        fd.WeightClassMultiplier = GenerateFishClassMultByFishType(ft);
        fd.Experience = exp;
        fd.MovementDescription = Get("Movement");
        fd.CatchMissionCount1 = cm1;
        fd.CatchMissionCount2 = cm2;
        fd.CatchMissionCount3 = cm3;
        fd.BoonBonus = boon;

        // Populate runtime defaults for Weight/Speed (these can be randomized later at runtime)
        fd.Weight = 1f;
        fd.Speed = 1f;
        fd.NumberCaught = 0;
        fd.Discovered = false;

        return fd;
    }

    // Mirror of previous runtime mapping — keeps importer deterministic for WeightClassMultiplier
    private static float GenerateFishClassMultByFishType(FishType type)
    {
        switch (type)
        {
            case FishType.Small: return 2f;
            case FishType.Medium: return 1.8f;
            case FishType.Large: return 1.6f;
            case FishType.Leviathan: return 1.4f;
            default: return 2f;
        }
    }

    /// <summary>
    /// Parse a CSV line into fields. Handles quoted fields containing commas and escaped quotes.
    /// </summary>
    private static string[] ParseCsvLine(string line)
    {
        if (string.IsNullOrEmpty(line))
            return Array.Empty<string>();

        var fields = new List<string>();
        bool inQuotes = false;
        var field = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Handle escaped quotes
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    field.Append('"');
                    i++; // skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(field.ToString());
                field.Length = 0;
            }
            else
            {
                field.Append(c);
            }
        }

        fields.Add(field.ToString());
        return fields.ToArray();
    }

    private static IEnumerable<string> SplitLines(string text)
    {
        using (StringReader reader = new StringReader(text))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
        }
    }
#endif
}