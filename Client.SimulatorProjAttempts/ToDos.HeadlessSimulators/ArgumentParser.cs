using System;

namespace ToDos.HeadlessSimulators
{
    public class ArgumentParser
    {
        public SimulatorArgs Parse(string[] args)
        {
            if (args.Length < 2)
                throw new ArgumentException("Usage: ToDos.Clients.Simulator <userid> <filepath>");

            if (!int.TryParse(args[0], out int userId))
                throw new ArgumentException("First argument must be an integer userId.");

            string filePath = args[1];
            return new SimulatorArgs { UserId = userId, FilePath = filePath };
        }
    }

    public class SimulatorArgs
    {
        public int UserId { get; set; }
        public string FilePath { get; set; }
    }
} 