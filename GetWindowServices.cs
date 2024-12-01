using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using System.Management;

namespace GetWindowsServiceDetails
{
    partial class GetWindowServices : ServiceBase
    {
        private Timer _timer;
        public GetWindowServices()
        {
            InitializeComponent();
            this.ServiceName = "ServiceDetailsFetcher";
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            Log("Service started.");
            FetchAndLogServiceDetails();
            _timer = new Timer(6000); // 1 minute interval
            _timer.Elapsed += TimerElapsed;
            _timer.Start();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            _timer?.Stop();
            Log("Service stopped.");
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            FetchAndLogServiceDetails();
        }
        private void FetchAndLogServiceDetails()
        {
            try
            {
                var services = ServiceController.GetServices();
                var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServiceDetails.log");

                using (var writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"--- Service Details (Fetched at {DateTime.Now}) ---");
                    foreach (var service in services)
                    {
                        writer.WriteLine($"Service Name: {service.ServiceName}");
                        writer.WriteLine($"Display Name: {service.DisplayName}");
                        writer.WriteLine($"Status: {service.Status}");
                        writer.WriteLine($"Service Type: {service.ServiceType}");
                        writer.WriteLine($"Service Path: {GetServiceExecutablePath(service.ServiceName)}");
                        writer.WriteLine(new string('-', 50));
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Error fetching service details: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServiceDetails.log");
            using (var writer = new StreamWriter(logFilePath, true))
            {
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
        }

        static string GetServiceExecutablePath(string serviceName)
        {
            try
            {
                string query = $"SELECT * FROM Win32_Service WHERE Name = '{serviceName}'";
                using (var searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject service in searcher.Get())
                    {
                        return service["PathName"]?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving service path: {ex.Message}");
            }

            return null;
        }
    }
}
