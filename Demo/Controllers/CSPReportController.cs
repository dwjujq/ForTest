using Demo.Models;
using Microsoft.AspNetCore.Mvc;
using SilverSoft.Utils;
using System.Text;

namespace Demo.Controllers
{
    public class CSPReportController : Controller
    {
        private readonly static object lockObj = new object();

        [HttpPost("cspreport")]
        public IActionResult CSPReport([FromBody] CspReportRequest cspReportRequest)
        {
            WriteReport(cspReportRequest);
            return Ok();
        }

        private static void WriteReport(CspReportRequest cspReportRequest)
        {
            if(!Directory.Exists(GlobalVariables.FilesOptions.Demo.CSPReport))
            {
                Directory.CreateDirectory(GlobalVariables.FilesOptions.Demo.CSPReport);
            }

            var filePath = Path.Combine(GlobalVariables.FilesOptions.Demo.CSPReport, $"CSPReport-{DateTime.Now.Date.ToString("yyyy-MM-dd")}.txt");

            var sb = new StringBuilder();
            sb.AppendLine($"====================={DateTime.Now.ToString("HH:mm:ss")}=====================");
            sb.AppendLine(@$"CSP Violation: {cspReportRequest.CspReport.DocumentUri},{cspReportRequest.CspReport.BlockedUri}");
            sb.AppendLine("Details:");
            sb.AppendLine(Newtonsoft.Json.JsonConvert.SerializeObject(cspReportRequest)).AppendLine().AppendLine();

            lock (lockObj)
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                {
                    var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
