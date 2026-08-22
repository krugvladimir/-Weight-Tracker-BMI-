⚖️ Weight Tracker (BMI) — Multi‑Language Health Monitor
8 languages, one complete weight tracker – log your weight, calculate BMI, track progress, and visualize trends – right from your terminal.

✨ Features
⚖️ Log weight entries – with optional notes and date

📐 Calculate BMI – based on height (cm or m) and weight (kg)

📊 BMI categories – Underweight, Normal, Overweight, Obese

📈 Progress statistics – average, min, max, trend

📋 List all entries – with dates and BMI values

🔄 Set height once – saved in config for future use

💾 Persistent storage – all data saved in weights.json

📤 Export to CSV – for further analysis in spreadsheets

🧰 Supported Languages & Files
Language	File	Dependencies
Python	weight_tracker.py	none (stdlib)
Go	weight_tracker.go	none (stdlib)
JavaScript (Node)	weight_tracker.js	commander (optional)
Ruby	weight_tracker.rb	json, date
PHP	weight_tracker.php	none (extensions)
Java	WeightTracker.java	Java 8+
C#	WeightTracker.cs	.NET Core 3.1+
C++	weight_tracker.cpp	nlohmann/json
🚀 Quick Start
All implementations follow the same CLI pattern:

bash
# Set your height (in cm) – required before logging
<command> height 175

# Log a weight entry (kg)
<command> add 72.5

# Log with optional note
<command> add 71.8 --note "Feeling great!"

# Log with custom date
<command> add 73.0 --date 2026-08-20

# List all entries
<command> list

# Show statistics and BMI
<command> stats

# Export to CSV
<command> export weights.csv
Commands:

height <cm> – set your height (saved in config)

add <weight> [--date DATE] [--note TEXT] – log a weight

list – show all weight entries

stats – display BMI and statistics

export <filename> – export to CSV

📸 Example Output
text
⚖️ Weight Tracker
Height: 175.0 cm

📊 Statistics
Entries: 5
Current: 72.5 kg
Average: 73.2 kg
Min: 71.8 kg
Max: 74.5 kg

📐 BMI: 23.7 (Normal)
BMI Range: 18.5 - 24.9

📋 Weight Log:
2026-08-21 | 72.5 kg | BMI: 23.7 | Feeling great!
2026-08-20 | 73.0 kg | BMI: 23.8 | 
2026-08-19 | 74.0 kg | BMI: 24.2 | 
📁 Repository Structure
text
.
├── README.md
├── python/
│   └── weight_tracker.py
├── go/
│   └── weight_tracker.go
├── javascript/
│   └── weight_tracker.js
├── ruby/
│   └── weight_tracker.rb
├── php/
│   └── weight_tracker.php
├── java/
│   └── WeightTracker.java
├── csharp/
│   └── WeightTracker.cs
└── cpp/
    └── weight_tracker.cpp
