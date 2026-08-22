// WeightTracker.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

class Config
{
    [JsonPropertyName("height")]
    public double Height { get; set; }
}

class Entry
{
    [JsonPropertyName("date")]
    public string Date { get; set; }
    [JsonPropertyName("weight")]
    public double Weight { get; set; }
    [JsonPropertyName("note")]
    public string Note { get; set; }
}

class WeightTracker
{
    private static readonly string ConfigFile = "weight_config.json";
    private static readonly string DataFile = "weights.json";
    private static double height = 0.0;
    private static List<Entry> entries = new List<Entry>();
    private static readonly JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

    static void LoadConfig()
    {
        if (!File.Exists(ConfigFile)) return;
        string json = File.ReadAllText(ConfigFile);
        Config cfg = JsonSerializer.Deserialize<Config>(json);
        if (cfg != null) height = cfg.Height;
    }

    static void SaveConfig()
    {
        Config cfg = new Config { Height = height };
        string json = JsonSerializer.Serialize(cfg, options);
        File.WriteAllText(ConfigFile, json);
    }

    static void LoadEntries()
    {
        if (!File.Exists(DataFile)) return;
        string json = File.ReadAllText(DataFile);
        entries = JsonSerializer.Deserialize<List<Entry>>(json) ?? new List<Entry>();
    }

    static void SaveEntries()
    {
        string json = JsonSerializer.Serialize(entries, options);
        File.WriteAllText(DataFile, json);
    }

    static double? BMI(double weight)
    {
        if (height <= 0) return null;
        double h = height / 100.0;
        return weight / (h * h);
    }

    static string BMICategory(double bmiVal)
    {
        if (bmiVal < 18.5) return "Underweight";
        if (bmiVal < 25.0) return "Normal";
        if (bmiVal < 30.0) return "Overweight";
        return "Obese";
    }

    static void SetHeight(double h)
    {
        height = h;
        SaveConfig();
        Console.WriteLine($"✅ Height set to {h} cm");
    }

    static void AddEntry(double weight, string date, string note)
    {
        if (string.IsNullOrEmpty(date)) date = DateTime.Now.ToString("yyyy-MM-dd");
        if (note == null) note = "";
        entries.Add(new Entry { Date = date, Weight = weight, Note = note });
        SaveEntries();
        double? bmiVal = BMI(weight);
        string bmiStr = bmiVal.HasValue ? $" (BMI: {bmiVal.Value:F1})" : "";
        Console.WriteLine($"✅ Logged: {weight} kg on {date}{bmiStr}");
    }

    static void ListEntries()
    {
        if (!entries.Any())
        {
            Console.WriteLine("No entries.");
            return;
        }
        Console.WriteLine("\n📋 Weight Log:");
        foreach (var e in entries)
        {
            double? bmiVal = BMI(e.Weight);
            string bmiStr = bmiVal.HasValue ? $"BMI: {bmiVal.Value:F1}" : "N/A";
            string note = !string.IsNullOrEmpty(e.Note) ? $" | {e.Note}" : "";
            Console.WriteLine($"{e.Date} | {e.Weight} kg | {bmiStr}{note}");
        }
    }

    static void Stats()
    {
        if (!entries.Any())
        {
            Console.WriteLine("No entries.");
            return;
        }
        if (height <= 0)
        {
            Console.WriteLine("❌ Please set your height first: height <cm>");
            return;
        }
        var weights = entries.Select(e => e.Weight).ToList();
        double avg = weights.Average();
        double mn = weights.Min();
        double mx = weights.Max();
        double current = weights.Last();
        double? bmiVal = BMI(current);
        string cat = bmiVal.HasValue ? BMICategory(bmiVal.Value) : "N/A";
        Console.WriteLine($"\n⚖️ Weight Tracker");
        Console.WriteLine($"Height: {height} cm");
        Console.WriteLine($"\n📊 Statistics");
        Console.WriteLine($"Entries: {entries.Count}");
        Console.WriteLine($"Current: {current:F1} kg");
        Console.WriteLine($"Average: {avg:F1} kg");
        Console.WriteLine($"Min: {mn:F1} kg");
        Console.WriteLine($"Max: {mx:F1} kg");
        if (bmiVal.HasValue)
        {
            Console.WriteLine($"\n📐 BMI: {bmiVal.Value:F1} ({cat})");
            Console.WriteLine($"BMI Range: 18.5 - 24.9");
        }
    }

    static void ExportCSV(string filename)
    {
        using var writer = new StreamWriter(filename);
        writer.WriteLine("Date,Weight (kg),BMI,Note");
        foreach (var e in entries)
        {
            double? bmiVal = BMI(e.Weight);
            string bmiStr = bmiVal.HasValue ? bmiVal.Value.ToString("F1") : "";
            writer.WriteLine($"{e.Date},{e.Weight},{bmiStr},{e.Note ?? ""}");
        }
        Console.WriteLine($"✅ Exported {entries.Count} entries to {filename}");
    }

    static void Main(string[] args)
    {
        LoadConfig();
        LoadEntries();
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: WeightTracker [height|add|list|stats|export]");
            return;
        }
        string cmd = args[0];
        switch (cmd)
        {
            case "height":
                if (args.Length < 2) { Console.WriteLine("Usage: height <cm>"); return; }
                SetHeight(double.Parse(args[1]));
                break;
            case "add":
                if (args.Length < 2) { Console.WriteLine("Usage: add <weight> [--date YYYY-MM-DD] [--note TEXT]"); return; }
                double w = double.Parse(args[1]);
                string date = null, note = null;
                for (int i = 2; i < args.Length; i++)
                {
                    if (args[i] == "--date" && i+1 < args.Length) date = args[++i];
                    if (args[i] == "--note" && i+1 < args.Length) note = args[++i];
                }
                AddEntry(w, date, note);
                break;
            case "list":
                ListEntries();
                break;
            case "stats":
                Stats();
                break;
            case "export":
                string filename = args.Length > 1 ? args[1] : "weights.csv";
                ExportCSV(filename);
                break;
            default:
                Console.WriteLine("Unknown command");
                break;
        }
    }
}
