using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToDos.Clients.Simulator
{
    public class ScriptFileParser
    {
        public List<ScriptLine> Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var result = new List<ScriptLine>();
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                var parts = line.Split(',');
                if (parts.Length < 3)
                    throw new FormatException($"Invalid script line: {line}");
                var opList = parts.Take(parts.Length - 3).ToArray();
                int count = int.Parse(parts[parts.Length - 3]);
                int delayEach = int.Parse(parts[parts.Length - 2]);
                int delayEnd = int.Parse(parts[parts.Length - 1]);
                result.Add(new ScriptLine
                {
                    Operations = opList,
                    Count = count,
                    DelayEach = delayEach,
                    DelayEnd = delayEnd
                });
            }
            return result;
        }
    }

    public class ScriptLine
    {
        public string[] Operations { get; set; }
        public int Count { get; set; }
        public int DelayEach { get; set; }
        public int DelayEnd { get; set; }
    }
} 