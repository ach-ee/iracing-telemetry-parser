using CsvHelper;

Console.WriteLine("Starting iRacing Telemetry Parser");

const int SkipUpToIndex = 9;
const int MaxQueueLength = 30;
const double SpeedThreshold = 1.0;
const int MaxEndRecordCount = 30;
const double DecelThreshold = -5.0;

List<Record> records = new();

using (var reader = new StreamReader($"../test2.csv"))
using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
{
    // Skip first header rows
    foreach (var value in Enumerable.Range(0,SkipUpToIndex))
    {
        csv.Read();
    }

    // Read header and units on following line
    csv.ReadHeader();
    csv.Read();

    RecordUnits units = new();

    units.Brake = csv.GetField("Brake");
    units.Clutch = csv.GetField("Clutch");
    units.Speed = csv.GetField("Speed");
    units.Throttle = csv.GetField("Throttle");

    Queue<Record> recordQueue = new();

    // Store a rolling queue of records until the speed jumps beyond a threshold
    while (csv.Read())
    {
        Record rec = ReadRecord(csv);
        recordQueue.Enqueue(rec);

        if (recordQueue.Count > MaxQueueLength)
        {
            recordQueue.Dequeue();
        }

        if (rec.Speed > SpeedThreshold)
        {
            break;
        }
    }

    // Add the rolling queue to this list of records
    records.AddRange(recordQueue);

    double previousSpeed = 0;
    int decelCount = 0;
    bool decelDetected = false;

    // Read and store records until deceleration is detected, then save a few more records after
    while (csv.Read() && decelCount < MaxEndRecordCount)
    {
        Record rec = ReadRecord(csv);
        records.Add(rec);

        if (rec.Speed - previousSpeed <= DecelThreshold)
        {
            decelDetected = true;
        }   

        if (decelDetected) 
        {
            decelCount++;
        }

        previousSpeed = rec.Speed;
    }
}

using (var writer = new StreamWriter($"../test2_out.csv"))
using (var csv = new CsvWriter(writer, System.Globalization.CultureInfo.InvariantCulture))
{
    csv.WriteRecords(records);
}

Console.WriteLine("Done.");

Record ReadRecord(CsvReader csv)
{
    Record rec = new();

    rec.Brake = double.Parse(csv.GetField("Brake"));
    rec.Clutch = double.Parse(csv.GetField("Clutch"));
    rec.Speed = double.Parse(csv.GetField("Speed"));
    rec.Throttle = double.Parse(csv.GetField("Throttle"));

    return rec;
}
