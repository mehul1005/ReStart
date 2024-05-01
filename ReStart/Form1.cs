using Krypton.Toolkit;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using WindowsInput;
using System.Xml.Linq;

namespace ReStart
{
    public partial class Form1 : Form
    {
        private string advisorExePath;
        private int timerInterval;
        private int delayBeforeKeyPress;

        private System.Windows.Forms.Timer timer;
        private InputSimulator inputSimulator;
        private int countdownTimer;

        public Form1()
        {
            InitializeComponent();

            // Load settings from XML
            LoadSettings();

            // Initialize timer
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // Set the interval to 1 second
            timer.Tick += Timer_Tick;
            timer.Start();

            // Initialize InputSimulator
            inputSimulator = new InputSimulator();

            // Initialize countdown timer
            countdownTimer = timerInterval / 1000; // Convert milliseconds to seconds

            // Update the countdown label
            UpdateCountdownLabel();

            // Check initially
            CheckAndStartAdvisor();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update countdown timer
            countdownTimer--;

            // Update the countdown label
            UpdateCountdownLabel();

            // Check if countdown is complete
            if (countdownTimer <= 0)
            {
                // Check periodically
                CheckAndStartAdvisor();

                // Reset countdown timer
                countdownTimer = timerInterval / 1000;
            }
        }

        private void UpdateCountdownLabel()
        {
            // Display the countdown in a label
            labelCountdown.Text = $"Time until Advisor launch and 'O' keypress: {countdownTimer} seconds";
        }

        private void CheckAndStartAdvisor()
        {
            if (!IsProcessRunning("Advisor"))
            {
                // Start Advisor.exe
                Process advisorProcess = StartAdvisor();

                // Wait for the specified delay before interacting with the window
                Thread.Sleep(delayBeforeKeyPress);

                // Simulate clicks on the Advisor window
                SimulateClicksOnWindow(advisorProcess.MainWindowHandle);

                // Simulate key press "O"
                inputSimulator.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.VK_O);
            }
        }

        private bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            return processes.Length > 0;
        }

        private Process StartAdvisor()
        {
            try
            {
                return Process.Start(advisorExePath);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or display an error message)
                MessageBox.Show($"Error starting Advisor.exe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void LoadSettings()
        {
            try
            {
                // Get the directory of the executable
                string executablePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string executableDirectory = Path.GetDirectoryName(executablePath);

                // Combine the executable directory with the XML file name
                string xmlFilePath = Path.Combine(executableDirectory, "Settings.xml");

                // Load settings from XML file
                XDocument doc = XDocument.Load(xmlFilePath);

                advisorExePath = doc.Element("Settings")?.Element("AdvisorExePath")?.Value ?? "";
                timerInterval = int.Parse(doc.Element("Settings")?.Element("TimerInterval")?.Value ?? "120000"); // Default: 2 minutes
                delayBeforeKeyPress = int.Parse(doc.Element("Settings")?.Element("DelayBeforeKeyPress")?.Value ?? "20000"); // Default: 20 seconds
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or display an error message)
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SimulateClicksOnWindow(IntPtr mainWindowHandle)
        {
            inputSimulator.Mouse.MoveMouseToPositionOnVirtualDesktop(100, 100); // Change the coordinates as needed
            Thread.Sleep(500); // Wait for a moment

            // First click
            inputSimulator.Mouse.LeftButtonDown();
            Thread.Sleep(100);
            inputSimulator.Mouse.LeftButtonUp();
            Thread.Sleep(500); // Wait for a moment

            // Second click
            inputSimulator.Mouse.LeftButtonDown();
            Thread.Sleep(100);
            inputSimulator.Mouse.LeftButtonUp();

            // Update the countdown label
            UpdateCountdownLabel();
        }
    }
}
