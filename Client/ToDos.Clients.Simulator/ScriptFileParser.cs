using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ToDos.Clients.Simulator
{
    public class ScriptFileParseResult
    {
        public int? UserId { get; set; }
        public bool? SignToEvents { get; set; }
        public int NumOfClients { get; set; } = 1;
        public List<ScriptLine> ScriptLines { get; set; }
    }

    public class ScriptFileParser
    {
        public ScriptFileParseResult Parse(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var result = new List<ScriptLine>();
            int? userId = null;
            bool? signToEvents = null;
            int numOfClients = 1;
            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;
                if (line.StartsWith("userId=", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(line.Substring(7), out int uid))
                        userId = uid;
                    continue;
                }
                if (line.StartsWith("signToEvents=", StringComparison.OrdinalIgnoreCase))
                {
                    if (bool.TryParse(line.Substring(13), out bool sign))
                        signToEvents = sign;
                    continue;
                }
                if (line.StartsWith("numOfClients=", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(line.Substring(13), out int nClients))
                        numOfClients = nClients;
                    continue;
                }
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
            return new ScriptFileParseResult
            {
                UserId = userId,
                SignToEvents = signToEvents,
                NumOfClients = numOfClients,
                ScriptLines = result
            };
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