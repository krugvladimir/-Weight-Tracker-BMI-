// weight_tracker.js
#!/usr/bin/env node
const fs = require('fs');
const { program } = require('commander');

const CONFIG_FILE = 'weight_config.json';
const DATA_FILE = 'weights.json';

let height = 0;
let entries = [];

function loadConfig() {
    if (fs.existsSync(CONFIG_FILE)) {
        try {
            const cfg = JSON.parse(fs.readFileSync(CONFIG_FILE));
            height = cfg.height || 0;
        } catch (e) {}
    }
}

function saveConfig() {
    fs.writeFileSync(CONFIG_FILE, JSON.stringify({ height }));
}

function loadEntries() {
    if (fs.existsSync(DATA_FILE)) {
        try {
            entries = JSON.parse(fs.readFileSync(DATA_FILE));
        } catch (e) {}
    }
}

function saveEntries() {
    fs.writeFileSync(DATA_FILE, JSON.stringify(entries, null, 2));
}

function bmi(weight) {
    if (height <= 0) return null;
    const h = height / 100;
    return weight / (h * h);
}

function bmiCategory(bmiVal) {
    if (bmiVal < 18.5) return 'Underweight';
    if (bmiVal < 25) return 'Normal';
    if (bmiVal < 30) return 'Overweight';
    return 'Obese';
}

program
    .command('height <cm>')
    .action((cm) => {
        height = parseFloat(cm);
        saveConfig();
        console.log(`✅ Height set to ${height} cm`);
    });

program
    .command('add <weight>')
    .option('--date <date>', 'YYYY-MM-DD')
    .option('--note <note>', 'Optional note')
    .action((weight, options) => {
        const w = parseFloat(weight);
        const date = options.date || new Date().toISOString().slice(0,10);
        const note = options.note || '';
        entries.push({ date, weight: w, note });
        saveEntries();
        const bmiVal = bmi(w);
        const bmiStr = bmiVal !== null ? ` (BMI: ${bmiVal.toFixed(1)})` : '';
        console.log(`✅ Logged: ${w} kg on ${date}${bmiStr}`);
    });

program
    .command('list')
    .action(() => {
        if (!entries.length) {
            console.log('No entries.');
            return;
        }
        console.log('\n📋 Weight Log:');
        for (const e of entries) {
            const bmiVal = bmi(e.weight);
            const bmiStr = bmiVal !== null ? `BMI: ${bmiVal.toFixed(1)}` : 'N/A';
            const note = e.note ? ` | ${e.note}` : '';
            console.log(`${e.date} | ${e.weight} kg | ${bmiStr}${note}`);
        }
    });

program
    .command('stats')
    .action(() => {
        if (!entries.length) {
            console.log('No entries.');
            return;
        }
        if (height <= 0) {
            console.log('❌ Please set your height first: height <cm>');
            return;
        }
        const total = entries.length;
        const weights = entries.map(e => e.weight);
        const sum = weights.reduce((a, b) => a + b, 0);
        const avg = sum / total;
        const mn = Math.min(...weights);
        const mx = Math.max(...weights);
        const current = weights[weights.length - 1];
        const bmiVal = bmi(current);
        const cat = bmiCategory(bmiVal);
        console.log(`\n⚖️ Weight Tracker`);
        console.log(`Height: ${height} cm`);
        console.log(`\n📊 Statistics`);
        console.log(`Entries: ${total}`);
        console.log(`Current: ${current.toFixed(1)} kg`);
        console.log(`Average: ${avg.toFixed(1)} kg`);
        console.log(`Min: ${mn.toFixed(1)} kg`);
        console.log(`Max: ${mx.toFixed(1)} kg`);
        if (bmiVal !== null) {
            console.log(`\n📐 BMI: ${bmiVal.toFixed(1)} (${cat})`);
            console.log(`BMI Range: 18.5 - 24.9`);
        }
    });

program
    .command('export [filename]')
    .action((filename = 'weights.csv') => {
        let csv = 'Date,Weight (kg),BMI,Note\n';
        for (const e of entries) {
            const bmiVal = bmi(e.weight);
            const bmiStr = bmiVal !== null ? bmiVal.toFixed(1) : '';
            csv += `${e.date},${e.weight},${bmiStr},${e.note || ''}\n`;
        }
        fs.writeFileSync(filename, csv);
        console.log(`✅ Exported ${entries.length} entries to ${filename}`);
    });

program.parse(process.argv);
