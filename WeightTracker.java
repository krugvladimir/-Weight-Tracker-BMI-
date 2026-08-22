// WeightTracker.java
import java.io.*;
import java.nio.file.*;
import java.time.*;
import java.util.*;
import com.google.gson.*;

class Config {
    double height = 0.0;
}

class Entry {
    String date;
    double weight;
    String note;
}

class WeightTracker {
    private static final String CONFIG_FILE = "weight_config.json";
    private static final String DATA_FILE = "weights.json";
    private static double height = 0.0;
    private static List<Entry> entries = new ArrayList<>();
    private static final Gson gson = new GsonBuilder().setPrettyPrinting().create();

    static void loadConfig() {
        try {
            Path path = Paths.get(CONFIG_FILE);
            if (Files.exists(path)) {
                String json = new String(Files.readAllBytes(path));
                Config cfg = gson.fromJson(json, Config.class);
                height = cfg.height;
            }
        } catch (Exception e) {}
    }

    static void saveConfig() {
        try {
            Config cfg = new Config();
            cfg.height = height;
            Files.write(Paths.get(CONFIG_FILE), gson.toJson(cfg).getBytes());
        } catch (Exception e) {}
    }

    static void loadEntries() {
        try {
            Path path = Paths.get(DATA_FILE);
            if (Files.exists(path)) {
                String json = new String(Files.readAllBytes(path));
                Entry[] arr = gson.fromJson(json, Entry[].class);
                entries = new ArrayList<>(Arrays.asList(arr));
            }
        } catch (Exception e) {}
    }

    static void saveEntries() {
        try {
            Files.write(Paths.get(DATA_FILE), gson.toJson(entries).getBytes());
        } catch (Exception e) {}
    }

    static Double bmi(double weight) {
        if (height <= 0) return null;
        double h = height / 100.0;
        return weight / (h * h);
    }

    static String bmiCategory(double bmiVal) {
        if (bmiVal < 18.5) return "Underweight";
        if (bmiVal < 25.0) return "Normal";
        if (bmiVal < 30.0) return "Overweight";
        return "Obese";
    }

    static void setHeight(double h) {
        height = h;
        saveConfig();
        System.out.printf("✅ Height set to %.1f cm\n", h);
    }

    static void addEntry(double weight, String date, String note) {
        if (date == null) date = LocalDate.now().toString();
        if (note == null) note = "";
        Entry e = new Entry();
        e.date = date;
        e.weight = weight;
        e.note = note;
        entries.add(e);
        saveEntries();
        Double bmiVal = bmi(weight);
        String bmiStr = bmiVal != null ? String.format(" (BMI: %.1f)", bmiVal) : "";
        System.out.printf("✅ Logged: %.1f kg on %s%s\n", weight, date, bmiStr);
    }

    static void listEntries() {
        if (entries.isEmpty()) {
            System.out.println("No entries.");
            return;
        }
        System.out.println("\n📋 Weight Log:");
        for (Entry e : entries) {
            Double bmiVal = bmi(e.weight);
            String bmiStr = bmiVal != null ? String.format("BMI: %.1f", bmiVal) : "N/A";
            String note = e.note != null && !e.note.isEmpty() ? " | " + e.note : "";
            System.out.printf("%s | %.1f kg | %s%s\n", e.date, e.weight, bmiStr, note);
        }
    }

    static void stats() {
        if (entries.isEmpty()) {
            System.out.println("No entries.");
            return;
        }
        if (height <= 0) {
            System.out.println("❌ Please set your height first: height <cm>");
            return;
        }
        int total = entries.size();
        double sum = 0, min = Double.MAX_VALUE, max = Double.MIN_VALUE;
        for (Entry e : entries) {
            sum += e.weight;
            if (e.weight < min) min = e.weight;
            if (e.weight > max) max = e.weight;
        }
        double avg = sum / total;
        double current = entries.get(total - 1).weight;
        Double bmiVal = bmi(current);
        String cat = bmiVal != null ? bmiCategory(bmiVal) : "N/A";
        System.out.printf("\n⚖️ Weight Tracker\n");
        System.out.printf("Height: %.1f cm\n", height);
        System.out.printf("\n📊 Statistics\n");
        System.out.printf("Entries: %d\n", total);
        System.out.printf("Current: %.1f kg\n", current);
        System.out.printf("Average: %.1f kg\n", avg);
        System.out.printf("Min: %.1f kg\n", min);
        System.out.printf("Max: %.1f kg\n", max);
        if (bmiVal != null) {
            System.out.printf("\n📐 BMI: %.1f (%s)\n", bmiVal, cat);
            System.out.println("BMI Range: 18.5 - 24.9");
        }
    }

    static void exportCSV(String filename) throws IOException {
        Path path = Paths.get(filename);
        try (BufferedWriter writer = Files.newBufferedWriter(path)) {
            writer.write("Date,Weight (kg),BMI,Note\n");
            for (Entry e : entries) {
                Double bmiVal = bmi(e.weight);
                String bmiStr = bmiVal != null ? String.format("%.1f", bmiVal) : "";
                writer.write(String.format("%s,%.1f,%s,%s\n", e.date, e.weight, bmiStr, e.note != null ? e.note : ""));
            }
        }
        System.out.printf("✅ Exported %d entries to %s\n", entries.size(), filename);
    }

    public static void main(String[] args) throws Exception {
        loadConfig();
        loadEntries();
        if (args.length < 1) {
            System.out.println("Usage: WeightTracker [height|add|list|stats|export]");
            return;
        }
        String cmd = args[0];
        switch (cmd) {
            case "height":
                if (args.length < 2) { System.out.println("Usage: height <cm>"); return; }
                setHeight(Double.parseDouble(args[1]));
                break;
            case "add":
                if (args.length < 2) { System.out.println("Usage: add <weight> [--date YYYY-MM-DD] [--note TEXT]"); return; }
                double weight = Double.parseDouble(args[1]);
                String date = null, note = null;
                for (int i = 2; i < args.length; i++) {
                    if (args[i].equals("--date") && i+1 < args.length) date = args[++i];
                    if (args[i].equals("--note") && i+1 < args.length) note = args[++i];
                }
                addEntry(weight, date, note);
                break;
            case "list":
                listEntries();
                break;
            case "stats":
                stats();
                break;
            case "export":
                String filename = args.length > 1 ? args[1] : "weights.csv";
                exportCSV(filename);
                break;
            default:
                System.out.println("Unknown command");
        }
    }
}
