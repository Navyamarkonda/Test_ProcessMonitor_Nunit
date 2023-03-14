using McMaster.Extensions.CommandLineUtils;
using Moq;
using Veeam_Window_Monitoring;
using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Test_ProcessMonitor
{
    public class Tests
    {
        private Process process;

        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }

        //Create instance of the class
        Process_Monitor monitor = new Process_Monitor();

        [Test]
        //After Maxlifetime, process should be killed
        public void Execute_processgetskilled_morethanMaxlifetime()
        {
            // Arrange
            var monitor = new Process_Monitor();
            string processName = "Notepad";
            string maxLifetimeInMinutes = "1";
            string monitoringFrequencyInMinutes = "invalidmonitoringfrequency";
            //Open the process 
            process = new Process();
            process.StartInfo.FileName = "notepad.exe";
            process.Start();
            //Wait
            Thread.Sleep(Convert.ToInt32(maxLifetimeInMinutes) * 60 * 1000);

            //Act
            string result = monitor.Execute(processName, maxLifetimeInMinutes, monitoringFrequencyInMinutes);
            //Assert
            Assert.That(result, Is.EqualTo("processkilled"));
        }

        [Test]
        //Before threshold time, process should not be killed
        public void Execute_processnotkilled_lessthanMaxlifetime()
        {
            //Open the process 
            process = new Process();
            process.StartInfo.FileName = "notepad.exe";
            process.Start();
            //Act
            string result = monitor.Execute("Notepad", "1", "1");
            //Assert
            Assert.That(result, Is.EqualTo("processNotkilled"));

        }
        [Test]
        //When there is no process running, monitoring should be terminated as user terminates
        public void Execute_noprocessrunning()
        {
            //Cancellationtoken to cancel the method if needed
            CancellationTokenSource cts = new CancellationTokenSource();
            //Act
            string result = "";
            ThreadPool.QueueUserWorkItem(_ => {
                result = monitor.Execute("Notepad", "1", "1", cts.Token);
                //Assert
                Assert.That(result, Is.EqualTo("noprocess"));
            });

        }

        [Test]

        //Throw format exception when user gives invalid MaxLifetime
        public void Execute_WithInvalidMaxLifetime_ReturnsFormatException()
        {
            // Arrange
            var monitor = new Process_Monitor();
            string processName = "Notepad";
            string maxLifetimeInMinutes = "invalidmaxlifetime";
            string monitoringFrequencyInMinutes = "0.1";

            // Act and Assert
            Assert.Throws<FormatException>(() => monitor.Execute(processName, maxLifetimeInMinutes, monitoringFrequencyInMinutes));
        }

        [Test]
        ////Throw format exception when user gives invalid MonitoringFrequency
        public void Execute_WithInvalidMonitoringFrequency_ReturnsFormatException()
        {
            // Arrange
            var monitor = new Process_Monitor();
            string processName = "Notepad";
            string maxLifetimeInMinutes = "60";
            string monitoringFrequencyInMinutes = "invalidmonitoringfrequency";

            // Act and Assert
            Assert.Throws<FormatException>(() => monitor.Execute(processName, maxLifetimeInMinutes, monitoringFrequencyInMinutes));
        }

        [Test]
        //When process is not running, the monitoring should continue untill the user interupts
        public async Task Execute_NoProcessRunning_MonitorsAtExpectedIntervals()
        {
            // Arrange
            //Cancellationtoken to cancel the method if needed
            var monitor = new Process_Monitor();
            var processName = "NonexistentProcess";
            var maxLifetime = "1";
            var monitoringFrequency = "1";
            var cts = new CancellationTokenSource();

            // Act
            var task = Task.Run(() => monitor.Execute(processName, maxLifetime, monitoringFrequency, cts.Token));

            // Wait for several monitoring intervals
            await Task.Delay(Convert.ToInt32(monitoringFrequency) * 2 * 60 * 1000);

            // Assert
            Assert.IsFalse(task.IsCompleted); // Verify that the method is still running after the first monitoring interval
        }
    }

}