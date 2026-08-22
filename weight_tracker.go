// weight_tracker.go
package main

import (
	"encoding/json"
	"flag"
	"fmt"
	"os"
	"time"
)

type Config struct {
	Height float64 `json:"height"`
}

type Entry struct {
	Date   string  `json:"date"`
	Weight float64 `json:"weight"`
	Note   string  `json:"note,omitempty"`
}

type Tracker struct {
	Height  float64   `json:"height"`
	Entries []Entry   `json:"entries"`
	Config  *Config   `json:"-"`
}

var configFile = "weight_config.json"
var dataFile = "weights.json"

func loadConfig() Config {
	var cfg Config
	data, err := os.ReadFile(configFile)
	if err != nil {
		return cfg
	}
	json.Unmarshal(data, &cfg)
	return cfg
}

func saveConfig(cfg Config) {
	data, _ := json.MarshalIndent(cfg, "", "  ")
	os.WriteFile(configFile, data, 0644)
}

func loadEntries() []Entry {
	var entries []Entry
	data, err := os.ReadFile(dataFile)
	if err != nil {
		return entries
	}
	json.Unmarshal(data, &entries)
	return entries
}

func saveEntries(entries []Entry) {
	data, _ := json.MarshalIndent(entries, "", "  ")
	os.WriteFile(dataFile, data, 0644)
}

func bmi(weight, height float64) float64 {
	if height <= 0 {
		return 0
	}
	h := height / 100.0
	return weight / (h * h)
}

func bmiCategory(bmi float64) string {
	if bmi < 18.5 {
		return "Underweight"
	} else if bmi < 25.0 {
		return "Normal"
	} else if bmi < 30.0 {
		return "Overweight"
	} else {
		return "Obese"
	}
}

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: weight_tracker [height|add|list|stats|export]")
		return
	}
	cfg := loadConfig()
	entries := loadEntries()
	cmd := os.Args[1]

	switch cmd {
	case "height":
		if len(os.Args) != 3 {
			fmt.Println("Usage: height <cm>")
			return
		}
		var h float64
		fmt.Sscanf(os.Args[2], "%f", &h)
		cfg.Height = h
		saveConfig(cfg)
		fmt.Printf("✅ Height set to %.1f cm\n", h)

	case "add":
		if len(os.Args) < 3 {
			fmt.Println("Usage: add <weight> [--date YYYY-MM-DD] [--note TEXT]")
			return
		}
		var w float64
		fmt.Sscanf(os.Args[2], "%f", &w)
		date := time.Now().Format("2006-01-02")
		note := ""
		for i := 3; i < len(os.Args); i++ {
			if os.Args[i] == "--date" && i+1 < len(os.Args) {
				date = os.Args[i+1]
				i++
			}
			if os.Args[i] == "--note" && i+1 < len(os.Args) {
				note = os.Args[i+1]
				i++
			}
		}
		entry := Entry{Date: date, Weight: w, Note: note}
		entries = append(entries, entry)
		saveEntries(entries)
		bmiVal := bmi(w, cfg.Height)
		if bmiVal > 0 {
			fmt.Printf("✅ Logged: %.1f kg on %s (BMI: %.1f)\n", w, date, bmiVal)
		} else {
			fmt.Printf("✅ Logged: %.1f kg on %s\n", w, date)
		}

	case "list":
		if len(entries) == 0 {
			fmt.Println("No entries.")
			return
		}
		fmt.Println("\n📋 Weight Log:")
		for _, e := range entries {
			bmiVal := bmi(e.Weight, cfg.Height)
			bmiStr := "N/A"
			if bmiVal > 0 {
				bmiStr = fmt.Sprintf("%.1f", bmiVal)
			}
			note := e.Note
			if note != "" {
				note = " | " + note
			}
			fmt.Printf("%s | %.1f kg | BMI: %s%s\n", e.Date, e.Weight, bmiStr, note)
		}

	case "stats":
		if len(entries) == 0 {
			fmt.Println("No entries.")
			return
		}
		if cfg.Height <= 0 {
			fmt.Println("❌ Please set your height first: height <cm>")
			return
		}
		total := len(entries)
		sum := 0.0
		min := entries[0].Weight
		max := entries[0].Weight
		for _, e := range entries {
			sum += e.Weight
			if e.Weight < min {
				min = e.Weight
			}
			if e.Weight > max {
				max = e.Weight
			}
		}
		avg := sum / float64(total)
		current := entries[total-1].Weight
		bmiVal := bmi(current, cfg.Height)
		cat := bmiCategory(bmiVal)
		fmt.Printf("\n⚖️ Weight Tracker\n")
		fmt.Printf("Height: %.1f cm\n", cfg.Height)
		fmt.Printf("\n📊 Statistics\n")
		fmt.Printf("Entries: %d\n", total)
		fmt.Printf("Current: %.1f kg\n", current)
		fmt.Printf("Average: %.1f kg\n", avg)
		fmt.Printf("Min: %.1f kg\n", min)
		fmt.Printf("Max: %.1f kg\n", max)
		if bmiVal > 0 {
			fmt.Printf("\n📐 BMI: %.1f (%s)\n", bmiVal, cat)
			fmt.Println("BMI Range: 18.5 - 24.9")
		}

	case "export":
		filename := "weights.csv"
		if len(os.Args) >= 3 {
			filename = os.Args[2]
		}
		f, err := os.Create(filename)
		if err != nil {
			fmt.Println("Error creating file:", err)
			return
		}
		defer f.Close()
		f.WriteString("Date,Weight (kg),BMI,Note\n")
		for _, e := range entries {
			bmiVal := bmi(e.Weight, cfg.Height)
			bmiStr := ""
			if bmiVal > 0 {
				bmiStr = fmt.Sprintf("%.1f", bmiVal)
			}
			f.WriteString(fmt.Sprintf("%s,%.1f,%s,%s\n", e.Date, e.Weight, bmiStr, e.Note))
		}
		fmt.Printf("✅ Exported %d entries to %s\n", len(entries), filename)

	default:
		fmt.Println("Unknown command")
	}
}
