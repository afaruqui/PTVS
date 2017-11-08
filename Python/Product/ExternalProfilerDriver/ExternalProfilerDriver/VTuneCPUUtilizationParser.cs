﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace ExternalProfilerDriver
{ 
    public class CPUUtilRecord
    {
        // timeBin,Bin Start Time,Bin End Time,CPU Time:Self
        long _timeBin;
        double _binStartTime;
        double _binEndTime;
        double _cpuTimeSelf;
        public CPUUtilRecord(long bin, double start, double end, double cputime)
        {
            _timeBin = bin;
            _binStartTime = start;
            _binEndTime = end;
            _cpuTimeSelf = cputime;
        }
        public long Bin { get { return _timeBin; } }
        public double Start { get { return _binStartTime; } }
        public double End { get { return _binEndTime; } }
        public double CPUUtil { get { return _cpuTimeSelf; } }
    }

    public static class VTuneCPUUtilizationParser
    {
        public static IEnumerable<CPUUtilRecord> ParseCPURecords(this IEnumerable<string> s)
        {
            foreach (string _s in s)
            {
                var ss = _s.Split(',');
                if (ss.Length != 4) { break; }
                yield return new CPUUtilRecord(Int64.Parse(ss[0]), Single.Parse(ss[1]) * 1000, Single.Parse(ss[2]) * 1000, Single.Parse(ss[3]));
            }
        }

        public static void CPURecordsFromFilename(string filename)
        {
            var cpuRecords = VTuneStackParser.ReadFromFile(filename)
                .Skip(2)
                .ParseCPURecords();
            CPUUtilRecord first = cpuRecords.First();
            CPUUtilRecord last = cpuRecords.Last();

            CPUUtilTrace trace = new CPUUtilTrace();
            trace.beginTime = new LongInt(0, (long)(first.Start));
            trace.duration = new LongInt(0, (long)(last.End - first.Start));
            trace.counters = new List<ValueTrace> { new ValueTrace(cpuRecords.Select(r => new CPUSample(new LongInt(0, (long)r.Start), (float)r.CPUUtil)).ToList()) };

            // drop to file
            string json = JsonConvert.SerializeObject(trace, Formatting.Indented);
            Console.WriteLine($"JSON serialization: << {json} >>");
        }
    }
}