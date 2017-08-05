using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VirusTotalNET;
using VirusTotalNET.Objects;
using VirusTotalNET.ResponseCodes;
using VirusTotalNET.Results;

namespace SecureNet.Classes
{
    class VirusTotalService
    {
        public async void startVTServicesAsyncURL(string urlText)
        {
                VirusTotal vt = new VirusTotal(ConfigurationManager.AppSettings["virusTotalAPIKey"].ToString());
                vt.UseTLS = true;
                UrlReport urlReport = await vt.GetUrlReport(urlText);

                bool hasUrlBeenScannedBefore = urlReport.ResponseCode == ReportResponseCode.Present;
                Console.WriteLine(hasUrlBeenScannedBefore);
                Console.WriteLine("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));
                MessageBox.Show("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));

                //If the url has been scanned before, the results are embedded inside the report.
                if (hasUrlBeenScannedBefore)
                {
                    PrintScan(urlReport);
                }
                else
                {
                    UrlScanResult urlResult = await vt.ScanUrl(urlText);
                    PrintScan(urlResult);
                }
        }

        public static void PrintScan(UrlScanResult scanResult)
        {
            Console.WriteLine("Scan ID: " + scanResult.ScanId);
            Console.WriteLine("Message: " + scanResult.VerboseMsg);
            Console.WriteLine();
        }

        public static void PrintScan(UrlReport urlReport)
        {
            Console.WriteLine("Scan ID: " + urlReport.ScanId);
            Console.WriteLine("Message: " + urlReport.VerboseMsg);

            if (urlReport.ResponseCode == ReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in urlReport.Scans)
                {
                    Console.WriteLine("{0,-25} Detected: {1}", scan.Key, scan.Value.Detected);
                }
            }

            Console.WriteLine();
        }


        public async void startVTSeriviceAsyncFile(string fileinfostring)
        {
                VirusTotal vt = new VirusTotal(ConfigurationManager.AppSettings["virusTotalAPIKey"].ToString());
                vt.UseTLS = true;
                FileInfo fileInfo = new FileInfo(fileinfostring);
                byte[] byteArray;
                using (StreamReader reader = new StreamReader(new FileStream(fileinfostring, FileMode.Open), new UTF8Encoding())) // do anything you want, e.g. read it
                {
                    byteArray = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                }

                File.WriteAllText(fileInfo.FullName, System.Text.Encoding.UTF8.GetString(byteArray));
                MessageBox.Show(System.Text.Encoding.UTF8.GetString(byteArray));
                FileReport fileReport = await vt.GetFileReport(fileInfo);
                bool hasFileBeenScannedBefore = fileReport.ResponseCode == ReportResponseCode.Present;

                Console.WriteLine("File has been scanned before: " + (hasFileBeenScannedBefore ? "Yes" : "No"));
                MessageBox.Show("File has been scanned before: " + (hasFileBeenScannedBefore ? "Yes" : "No"));

                //If the file has been scanned before, the results are embedded inside the report.
                if (hasFileBeenScannedBefore)
                {
                    PrintScan(fileReport);
                }
                else
                {
                    ScanResult fileResult = await vt.ScanFile(fileInfo);
                    PrintScan(fileResult);
                }
        }

        public static void PrintScan(ScanResult scanResult)
        {
            Console.WriteLine("Scan ID: " + scanResult.ScanId);
            Console.WriteLine("Message: " + scanResult.VerboseMsg);
            Console.WriteLine();
        }

        public static void PrintScan(FileReport fileReport)
        {
            Console.WriteLine("Scan ID: " + fileReport.ScanId);
            Console.WriteLine("Message: " + fileReport.VerboseMsg);

            if (fileReport.ResponseCode == ReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in fileReport.Scans)
                {
                    Console.WriteLine("{0,-25} Detected: {1}", scan.Key, scan.Value.Detected);
                }
            }

            Console.WriteLine();
        }
    }
}
