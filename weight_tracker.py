# weight_tracker.py
import json
import os
import argparse
from datetime import datetime

CONFIG_FILE = "weight_config.json"
DATA_FILE = "weights.json"

class WeightTracker:
    def __init__(self):
        self.height = 0.0  # in cm
        self.entries = []
        self.load_config()
        self.load_entries()

    def load_config(self):
        if os.path.exists(CONFIG_FILE):
            with open(CONFIG_FILE, "r") as f:
                cfg = json.load(f)
                self.height = cfg.get("height", 0.0)

    def save_config(self):
        with open(CONFIG_FILE, "w") as f:
            json.dump({"height": self.height}, f)

    def load_entries(self):
        if os.path.exists(DATA_FILE):
            with open(DATA_FILE, "r") as f:
                self.entries = json.load(f)

    def save_entries(self):
        with open(DATA_FILE, "w") as f:
            json.dump(self.entries, f, indent=2)

    def set_height(self, cm):
        self.height = cm
        self.save_config()
        print(f"✅ Height set to {cm} cm")

    def bmi(self, weight):
        if self.height <= 0:
            return None
        height_m = self.height / 100.0
        return weight / (height_m * height_m)

    def bmi_category(self, bmi):
        if bmi < 18.5:
            return "Underweight"
        elif bmi < 25.0:
            return "Normal"
        elif bmi < 30.0:
            return "Overweight"
        else:
            return "Obese"

    def add(self, weight, date=None, note=""):
        if date is None:
            date = datetime.now().strftime("%Y-%m-%d")
        entry = {"date": date, "weight": weight, "note": note}
        self.entries.append(entry)
        self.save_entries()
        bmi = self.bmi(weight)
        bmi_str = f"{bmi:.1f}" if bmi else "N/A"
        print(f"✅ Logged: {weight} kg on {date} (BMI: {bmi_str})")

    def list(self):
        if not self.entries:
            print("No entries.")
            return
        print("\n📋 Weight Log:")
        for e in self.entries:
            bmi = self.bmi(e["weight"])
            bmi_str = f"{bmi:.1f}" if bmi else "N/A"
            note = f" | {e['note']}" if e.get("note") else ""
            print(f"{e['date']} | {e['weight']} kg | BMI: {bmi_str}{note}")

    def stats(self):
        if not self.entries:
            print("No entries.")
            return
        if self.height <= 0:
            print("❌ Please set your height first: height <cm>")
            return

        weights = [e["weight"] for e in self.entries]
        total = len(weights)
        current = weights[-1]
        avg = sum(weights) / total
        mn = min(weights)
        mx = max(weights)

        bmi = self.bmi(current)
        cat = self.bmi_category(bmi) if bmi else "N/A"

        print(f"\n⚖️ Weight Tracker")
        print(f"Height: {self.height} cm")
        print(f"\n📊 Statistics")
        print(f"Entries: {total}")
        print(f"Current: {current:.1f} kg")
        print(f"Average: {avg:.1f} kg")
        print(f"Min: {mn:.1f} kg")
        print(f"Max: {mx:.1f} kg")
        if bmi:
            print(f"\n📐 BMI: {bmi:.1f} ({cat})")
            print(f"BMI Range: 18.5 - 24.9")

    def export_csv(self, filename):
        import csv
        with open(filename, 'w', newline='') as f:
            writer = csv.writer(f)
            writer.writerow(["Date", "Weight (kg)", "BMI", "Note"])
            for e in self.entries:
                bmi = self.bmi(e["weight"])
                bmi_str = f"{bmi:.1f}" if bmi else ""
                writer.writerow([e["date"], e["weight"], bmi_str, e.get("note", "")])
        print(f"✅ Exported {len(self.entries)} entries to {filename}")

def main():
    parser = argparse.ArgumentParser(description="Weight Tracker (BMI)")
    subparsers = parser.add_subparsers(dest="cmd", required=True)

    height_parser = subparsers.add_parser("height")
    height_parser.add_argument("cm", type=float)

    add_parser = subparsers.add_parser("add")
    add_parser.add_argument("weight", type=float)
    add_parser.add_argument("--date", help="YYYY-MM-DD")
    add_parser.add_argument("--note", default="")

    subparsers.add_parser("list")
    subparsers.add_parser("stats")

    export_parser = subparsers.add_parser("export")
    export_parser.add_argument("filename", default="weights.csv", nargs="?")

    args = parser.parse_args()
    tracker = WeightTracker()

    if args.cmd == "height":
        tracker.set_height(args.cm)
    elif args.cmd == "add":
        tracker.add(args.weight, args.date, args.note)
    elif args.cmd == "list":
        tracker.list()
    elif args.cmd == "stats":
        tracker.stats()
    elif args.cmd == "export":
        tracker.export_csv(args.filename)

if __name__ == "__main__":
    main()
