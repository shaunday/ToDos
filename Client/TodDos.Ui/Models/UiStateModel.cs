using System;

namespace Todos.Ui.Models
{
    public class UiStateModel
    {
        // Window State
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 800;
        public double WindowTop { get; set; } = 100;
        public double WindowLeft { get; set; } = 100;
        public string WindowState { get; set; } = "Normal";

        // Task Selection
        public int? LastSelectedTaskId { get; set; }

        // Filter State
        public string FilterSelectedPriority { get; set; } = "All";
        public string FilterTag { get; set; } = string.Empty;
        public string FilterCompletedStatus { get; set; } = "All";
    }
} 