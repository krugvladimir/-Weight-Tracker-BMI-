// weight_tracker.cpp
#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <ctime>
#include <iomanip>
#include <nlohmann/json.hpp>

using namespace std;
using json = nlohmann::json;

struct Entry {
    string date;
    double weight;
    string note;
};

double height = 0.0;
vector<Entry> entries;
const string CONFIG_FILE = "weight_config.json";
const string DATA_FILE = "weights.json";

void loadConfig() {
    ifstream f(CONFIG_FILE);
    if (f.is_open()) {
        json j;
        f >> j;
        if (j.contains("height")) height = j["height"];
        f.close();
    }
}

void saveConfig() {
    json j = {{"height", height}};
    ofstream f(CONFIG_FILE);
    f << setw(2) << j << endl;
}

void loadEntries() {
    ifstream f(DATA_FILE);
    if (f.is_open()) {
        json j;
        f >> j;
        for (auto& item : j) {
            Entry e;
            e.date = item["date"];
            e.weight = item["weight"];
            e.note = item["note"];
            entries.push_back(e);
        }
        f.close();
    }
}

void saveEntries() {
    json j = json::array();
    for (auto& e : entries) {
        j.push_back({{"date", e.date}, {"weight", e.weight}, {"note", e.note}});
    }
    ofstream f(DATA_FILE);
    f << setw(2) << j << endl;
}

string currentDate() {
    time_t t = time(nullptr);
    char buf[11];
    strftime(buf, sizeof(buf), "%Y-%m-%d", localtime(&t));
    return string(buf);
}

double bmi(double weight) {
    if (height <= 0) return 0;
    double h = height / 100.0;
    return weight / (h * h);
}

string bmiCategory(double bmiVal) {
    if (bmiVal < 18.5) return "Underweight";
    if (bmiVal < 25.0) return "Normal";
    if (bmiVal < 30.0) return "Overweight";
    return "Obese";
}

void setHeight(double h) {
    height = h;
    saveConfig();
    cout << "✅ Height set to " << h << " cm\n";
}

void addEntry(double weight, const string& date, const string& note) {
    string d = date.empty() ? currentDate() : date;
    string n = note;
    entries.push_back({d, weight, n});
    saveEntries();
    double bmiVal = bmi(weight);
    if (bmiVal > 0) {
        cout << "✅ Logged: " << weight << " kg on " << d << " (BMI: " << bmiVal << ")\n";
    } else {
        cout << "✅ Logged: " << weight << " kg on " << d << "\n";
    }
}

void listEntries() {
    if (entries.empty()) {
        cout << "No entries.\n";
        return;
    }
    cout << "\n📋 Weight Log:\n";
    for (auto& e : entries) {
        double bmiVal = bmi(e.weight);
        string bmiStr = bmiVal > 0 ? "BMI: " + to_string(bmiVal).substr(0,4) : "N/A";
        string note = !e.note.empty() ? " | " + e.note : "";
        cout << e.date << " | " << e.weight << " kg | " << bmiStr << note << "\n";
    }
}

void stats() {
    if (entries.empty()) {
        cout << "No entries.\n";
        return;
    }
    if (height <= 0) {
        cout << "❌ Please set your height first: height <cm>\n";
        return;
    }
    double sum = 0, mn = 1e9, mx = -1e9;
    for (auto& e : entries) {
        sum += e.weight;
        if (e.weight < mn) mn = e.weight;
        if (e.weight > mx) mx = e.weight;
    }
    double avg = sum / entries.size();
    double current = entries.back().weight;
    double bmiVal = bmi(current);
    cout << "\n⚖️ Weight Tracker\n";
    cout << "Height: " << height << " cm\n";
    cout << "\n📊 Statistics\n";
    cout << "Entries: " << entries.size() << "\n";
    cout << "Current: " << current << " kg\n";
    cout << "Average: " << avg << " kg\n";
    cout << "Min: " << mn << " kg\n";
    cout << "Max: " << mx << " kg\n";
    if (bmiVal > 0) {
        cout << "\n📐 BMI: " << bmiVal << " (" << bmiCategory(bmiVal) << ")\n";
        cout << "BMI Range: 18.5 - 24.9\n";
    }
}

void exportCSV(const string& filename) {
    ofstream f(filename);
    f << "Date,Weight (kg),BMI,Note\n";
    for (auto& e : entries) {
        double bmiVal = bmi(e.weight);
        string bmiStr = bmiVal > 0 ? to_string(bmiVal).substr(0,4) : "";
        f << e.date << "," << e.weight << "," << bmiStr << "," << e.note << "\n";
    }
    f.close();
    cout << "✅ Exported " << entries.size() << " entries to " << filename << "\n";
}

int main(int argc, char* argv[]) {
    loadConfig();
    loadEntries();
    if (argc < 2) {
        cerr << "Usage: weight_tracker [height|add|list|stats|export]\n";
        return 1;
    }
    string cmd = argv[1];
    if (cmd == "height") {
        if (argc < 3) { cerr << "Usage: height <cm>\n"; return 1; }
        setHeight(stod(argv[2]));
    } else if (cmd == "add") {
        if (argc < 3) { cerr << "Usage: add <weight> [--date YYYY-MM-DD] [--note TEXT]\n"; return 1; }
        double w = stod(argv[2]);
        string date, note;
        for (int i=3; i<argc; i++) {
            if (string(argv[i]) == "--date" && i+1 < argc) date = argv[++i];
            if (string(argv[i]) == "--note" && i+1 < argc) note = argv[++i];
        }
        addEntry(w, date, note);
    } else if (cmd == "list") {
        listEntries();
    } else if (cmd == "stats") {
        stats();
    } else if (cmd == "export") {
        string filename = argc > 2 ? argv[2] : "weights.csv";
        exportCSV(filename);
    } else {
        cerr << "Unknown command\n";
        return 1;
    }
    return 0;
}
