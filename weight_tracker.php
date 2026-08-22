# weight_tracker.php
#!/usr/bin/env php
<?php

define('CONFIG_FILE', 'weight_config.json');
define('DATA_FILE', 'weights.json');

function loadConfig() {
    if (file_exists(CONFIG_FILE)) {
        $data = json_decode(file_get_contents(CONFIG_FILE), true);
        return $data['height'] ?? 0.0;
    }
    return 0.0;
}

function saveConfig($height) {
    file_put_contents(CONFIG_FILE, json_encode(['height' => $height], JSON_PRETTY_PRINT));
}

function loadEntries() {
    if (file_exists(DATA_FILE)) {
        return json_decode(file_get_contents(DATA_FILE), true) ?: [];
    }
    return [];
}

function saveEntries($entries) {
    file_put_contents(DATA_FILE, json_encode($entries, JSON_PRETTY_PRINT));
}

function bmi($weight, $height) {
    if ($height <= 0) return null;
    $h = $height / 100.0;
    return $weight / ($h * $h);
}

function bmiCategory($bmiVal) {
    if ($bmiVal < 18.5) return "Underweight";
    if ($bmiVal < 25.0) return "Normal";
    if ($bmiVal < 30.0) return "Overweight";
    return "Obese";
}

if ($argc < 2) {
    die("Usage: php weight_tracker.php [height|add|list|stats|export]\n");
}

$cmd = $argv[1];
$height = loadConfig();
$entries = loadEntries();

switch ($cmd) {
    case 'height':
        if ($argc != 3) die("Usage: height <cm>\n");
        $h = (float)$argv[2];
        saveConfig($h);
        echo "✅ Height set to $h cm\n";
        break;

    case 'add':
        if ($argc < 3) die("Usage: add <weight> [--date YYYY-MM-DD] [--note TEXT]\n");
        $weight = (float)$argv[2];
        $date = date('Y-m-d');
        $note = '';
        for ($i=3; $i<$argc; $i++) {
            if ($argv[$i] == '--date' && isset($argv[$i+1])) {
                $date = $argv[++$i];
            }
            if ($argv[$i] == '--note' && isset($argv[$i+1])) {
                $note = $argv[++$i];
            }
        }
        $entries[] = ['date' => $date, 'weight' => $weight, 'note' => $note];
        saveEntries($entries);
        $bmiVal = bmi($weight, $height);
        $bmiStr = $bmiVal ? " (BMI: " . round($bmiVal, 1) . ")" : "";
        echo "✅ Logged: $weight kg on $date$bmiStr\n";
        break;

    case 'list':
        if (empty($entries)) {
            echo "No entries.\n";
            break;
        }
        echo "\n📋 Weight Log:\n";
        foreach ($entries as $e) {
            $bmiVal = bmi($e['weight'], $height);
            $bmiStr = $bmiVal ? "BMI: " . round($bmiVal, 1) : "N/A";
            $note = !empty($e['note']) ? " | " . $e['note'] : "";
            echo "{$e['date']} | {$e['weight']} kg | $bmiStr$note\n";
        }
        break;

    case 'stats':
        if (empty($entries)) {
            echo "No entries.\n";
            break;
        }
        if ($height <= 0) {
            echo "❌ Please set your height first: height <cm>\n";
            break;
        }
        $total = count($entries);
        $weights = array_column($entries, 'weight');
        $sum = array_sum($weights);
        $avg = $sum / $total;
        $mn = min($weights);
        $mx = max($weights);
        $current = end($weights);
        $bmiVal = bmi($current, $height);
        $cat = bmiCategory($bmiVal);
        echo "\n⚖️ Weight Tracker\n";
        echo "Height: $height cm\n";
        echo "\n📊 Statistics\n";
        echo "Entries: $total\n";
        echo "Current: " . round($current, 1) . " kg\n";
        echo "Average: " . round($avg, 1) . " kg\n";
        echo "Min: " . round($mn, 1) . " kg\n";
        echo "Max: " . round($mx, 1) . " kg\n";
        if ($bmiVal) {
            echo "\n📐 BMI: " . round($bmiVal, 1) . " ($cat)\n";
            echo "BMI Range: 18.5 - 24.9\n";
        }
        break;

    case 'export':
        $filename = $argv[2] ?? 'weights.csv';
        $fp = fopen($filename, 'w');
        fputcsv($fp, ['Date', 'Weight (kg)', 'BMI', 'Note']);
        foreach ($entries as $e) {
            $bmiVal = bmi($e['weight'], $height);
            $bmiStr = $bmiVal ? round($bmiVal, 1) : '';
            fputcsv($fp, [$e['date'], $e['weight'], $bmiStr, $e['note']]);
        }
        fclose($fp);
        echo "✅ Exported " . count($entries) . " entries to $filename\n";
        break;

    default:
        echo "Unknown command\n";
}
?>
