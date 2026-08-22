# weight_tracker.rb
#!/usr/bin/env ruby
require 'json'
require 'date'

CONFIG_FILE = 'weight_config.json'
DATA_FILE = 'weights.json'

class WeightTracker
  attr_reader :height, :entries

  def initialize
    @height = 0.0
    @entries = []
    load_config
    load_entries
  end

  def load_config
    if File.exist?(CONFIG_FILE)
      cfg = JSON.parse(File.read(CONFIG_FILE))
      @height = cfg['height'] || 0.0
    end
  end

  def save_config
    File.write(CONFIG_FILE, JSON.pretty_generate({ 'height' => @height }))
  end

  def load_entries
    if File.exist?(DATA_FILE)
      @entries = JSON.parse(File.read(DATA_FILE))
    end
  end

  def save_entries
    File.write(DATA_FILE, JSON.pretty_generate(@entries))
  end

  def bmi(weight)
    return nil if @height <= 0
    h = @height / 100.0
    weight / (h * h)
  end

  def bmi_category(bmi_val)
    return nil unless bmi_val
    if bmi_val < 18.5
      "Underweight"
    elsif bmi_val < 25.0
      "Normal"
    elsif bmi_val < 30.0
      "Overweight"
    else
      "Obese"
    end
  end

  def set_height(cm)
    @height = cm
    save_config
    puts "✅ Height set to #{cm} cm"
  end

  def add(weight, date = nil, note = '')
    date ||= Date.today.to_s
    entry = { 'date' => date, 'weight' => weight, 'note' => note }
    @entries << entry
    save_entries
    bmi_val = bmi(weight)
    bmi_str = bmi_val ? " (BMI: #{bmi_val.round(1)})" : ""
    puts "✅ Logged: #{weight} kg on #{date}#{bmi_str}"
  end

  def list
    if @entries.empty?
      puts "No entries."
      return
    end
    puts "\n📋 Weight Log:"
    @entries.each do |e|
      bmi_val = bmi(e['weight'])
      bmi_str = bmi_val ? "BMI: #{bmi_val.round(1)}" : "N/A"
      note = e['note'] && !e['note'].empty? ? " | #{e['note']}" : ""
      puts "#{e['date']} | #{e['weight']} kg | #{bmi_str}#{note}"
    end
  end

  def stats
    if @entries.empty?
      puts "No entries."
      return
    end
    if @height <= 0
      puts "❌ Please set your height first: height <cm>"
      return
    end
    total = @entries.size
    weights = @entries.map { |e| e['weight'] }
    sum = weights.sum
    avg = sum / total
    mn = weights.min
    mx = weights.max
    current = weights.last
    bmi_val = bmi(current)
    cat = bmi_category(bmi_val)
    puts "\n⚖️ Weight Tracker"
    puts "Height: #{@height} cm"
    puts "\n📊 Statistics"
    puts "Entries: #{total}"
    puts "Current: #{current.round(1)} kg"
    puts "Average: #{avg.round(1)} kg"
    puts "Min: #{mn.round(1)} kg"
    puts "Max: #{mx.round(1)} kg"
    if bmi_val
      puts "\n📐 BMI: #{bmi_val.round(1)} (#{cat})"
      puts "BMI Range: 18.5 - 24.9"
    end
  end

  def export_csv(filename)
    require 'csv'
    CSV.open(filename, 'w') do |csv|
      csv << ["Date", "Weight (kg)", "BMI", "Note"]
      @entries.each do |e|
        bmi_val = bmi(e['weight'])
        bmi_str = bmi_val ? bmi_val.round(1).to_s : ""
        csv << [e['date'], e['weight'], bmi_str, e['note']]
      end
    end
    puts "✅ Exported #{@entries.size} entries to #{filename}"
  end
end

if ARGV.empty?
  puts "Usage: weight_tracker.rb [height|add|list|stats|export]"
  exit
end

tracker = WeightTracker.new
cmd = ARGV.shift

case cmd
when 'height'
  cm = ARGV.shift.to_f
  tracker.set_height(cm)
when 'add'
  if ARGV.empty?
    puts "Usage: add <weight> [--date YYYY-MM-DD] [--note TEXT]"
    exit
  end
  weight = ARGV.shift.to_f
  date = nil
  note = ''
  if ARGV.include?('--date')
    idx = ARGV.index('--date')
    date = ARGV[idx+1] if idx
    ARGV.delete_at(idx); ARGV.delete_at(idx) if idx
  end
  if ARGV.include?('--note')
    idx = ARGV.index('--note')
    note = ARGV[idx+1] if idx
  end
  tracker.add(weight, date, note)
when 'list'
  tracker.list
when 'stats'
  tracker.stats
when 'export'
  filename = ARGV.shift || 'weights.csv'
  tracker.export_csv(filename)
else
  puts "Unknown command"
end
