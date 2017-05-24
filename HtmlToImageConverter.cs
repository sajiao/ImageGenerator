using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ImageGenerator
{
    public class HtmlToImageConverter
    {
        [CompilerGenerated]
        public string CustomArgs { get; set; }

        [CompilerGenerated]
        public TimeSpan? ExecutionTimeout { get; set; }

        [CompilerGenerated]
        public int Height { get; set; }

        [CompilerGenerated]
        public ProcessPriorityClass ProcessPriority { get; set; }

        [CompilerGenerated]
        public string BinPath { get; set; }

        [CompilerGenerated]
        public int Width { get; set; }

        [CompilerGenerated]
        public string WkHtmlToImageExeName { get; set; }

        [CompilerGenerated]
        public float Zoom { get; set; }
        [CompilerGenerated]
        private static object globalObj = new object();
        [CompilerGenerated]
        private static string[] ignoreWkHtmlErrLines = new string[] { "Exit with code 1 due to network error: ContentNotFoundError", "QFont::setPixelSize: Pixel size <= 0", "Exit with code 1 due to network error: ProtocolUnknownError", "Exit with code 1 due to network error: HostNotFoundError", "Exit with code 1 due to network error: ContentOperationNotPermittedError", "Exit with code 1 due to network error: UnknownContentError" };

        public HtmlToImageConverter()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (HttpContext.Current != null)
            {
                baseDirectory = HttpRuntime.AppDomainAppPath + "bin";
            }
            this.BinPath = baseDirectory;
            this.WkHtmlToImageExeName = "wkhtmltoimage.exe";
            this.Zoom = 1f;
            this.Width = 0;
            this.Height = 0;
            this.ProcessPriority = ProcessPriorityClass.Normal;
           
        }

        private void CheckExitCode(int exitCode, string lastErrLine, bool outputNotEmpty)
        {
            if ((exitCode != 0) && (((exitCode != 1) || (Array.IndexOf<string>(ignoreWkHtmlErrLines, lastErrLine.Trim()) < 0)) || !outputNotEmpty))
            {
                throw new WkHtmlToImageException(exitCode, lastErrLine);
            }
        }

        private bool CheckIsExistwkhtmltoimage()
        {
            string path = Path.Combine(this.BinPath, this.WkHtmlToImageExeName);
            return File.Exists(path);
        }

        private void DecompressWkHtmlToImage()
        {
            if (CheckIsExistwkhtmltoimage() == false)
            {
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
                string resourcePrefix = "ImageGenerator."+ WkHtmlToImageExeName;
                foreach (string str in manifestResourceNames)
                {
                    if (str.StartsWith(resourcePrefix))
                    {
                        string path = Path.Combine(this.BinPath, WkHtmlToImageExeName);
                        lock (globalObj)
                        {
                            if (!File.Exists(path) || (File.GetLastWriteTime(path) <= File.GetLastWriteTime(executingAssembly.Location)))
                            {
                                using (GZipStream stream = new GZipStream(executingAssembly.GetManifestResourceStream(str), CompressionMode.Decompress, false))
                                {
                                    using (FileStream fsream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                                    {
                                        int num;
                                        byte[] buffer = new byte[0x10000];
                                        while ((num = stream.Read(buffer, 0, buffer.Length)) > 0)
                                        {
                                            fsream.Write(buffer, 0, num);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }

        public byte[] GenerateImage(string htmlContent, string imageFormat)
        {
            MemoryStream outputStream = new MemoryStream();
            this.GenerateImage(htmlContent, imageFormat, outputStream);
            return outputStream.ToArray();
        }

        public void GenerateImage(string htmlContent, string imageFormat, Stream outputStream)
        {
            if (htmlContent == null)
            {
                throw new ArgumentNullException("htmlContent");
            }
            if (imageFormat == null)
            {
                throw new ArgumentNullException("imageFormat");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(htmlContent);
            this.GenerateImageInternal("-", bytes, "-", outputStream, imageFormat);
        }

        public byte[] GenerateImageFromFile(string htmlFilePath, string imageFormat)
        {
            if (imageFormat == null)
            {
                throw new ArgumentNullException("imageFormat");
            }
            MemoryStream outputStream = new MemoryStream();
            this.GenerateImageInternal(htmlFilePath, null, "-", outputStream, imageFormat);
            return outputStream.ToArray();
        }

        public void GenerateImageFromFile(string htmlFilePath, string imageFormat, Stream outputStream)
        {
            if (imageFormat == null)
            {
                throw new ArgumentNullException("imageFormat");
            }
            this.GenerateImageInternal(htmlFilePath, null, "-", outputStream, imageFormat);
        }

        public void GenerateImageFromFile(string htmlFilePath, string imageFormat, string outputImageFilePath)
        {
            if (File.Exists(outputImageFilePath))
            {
                File.Delete(outputImageFilePath);
            }
            this.GenerateImageInternal(htmlFilePath, null, outputImageFilePath, null, imageFormat);
        }

        private void GenerateImageInternal(string htmlFilePath, byte[] inputBytes, string outputImgFilePath, Stream outputStream, string imageFormat)
        {
            this.DecompressWkHtmlToImage();
            try
            {
                string path = Path.Combine(this.BinPath, this.WkHtmlToImageExeName);
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Cannot find WkHtmlToImage: " + path);
                }
                StringBuilder builder = new StringBuilder(200);
                builder.Append(" -q ");
                if (this.Zoom != 1f)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, " --zoom {0} ", new object[] { this.Zoom });
                }
                if (this.Width > 0)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, " --width {0} ", new object[] { this.Width });
                }
                if (this.Height > 0)
                {
                    builder.AppendFormat(CultureInfo.InvariantCulture, " --height {0} ", new object[] { this.Height });
                }
                if (!string.IsNullOrEmpty(imageFormat))
                {
                    builder.AppendFormat(" -f {0} ", imageFormat);
                }
                if (!string.IsNullOrEmpty(this.CustomArgs))
                {
                    builder.AppendFormat(" {0} ", this.CustomArgs);
                }
                builder.AppendFormat(" \"{0}\" ", htmlFilePath);
                builder.AppendFormat(" \"{0}\" ", outputImgFilePath);
                ProcessStartInfo startInfo = new ProcessStartInfo(path, builder.ToString());
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.WorkingDirectory = Path.GetDirectoryName(this.BinPath);
                startInfo.RedirectStandardInput = inputBytes != null;
                startInfo.RedirectStandardOutput = outputStream != null;
                startInfo.RedirectStandardError = true;
                Process proc = Process.Start(startInfo);
                if (this.ProcessPriority != ProcessPriorityClass.Normal)
                {
                    proc.PriorityClass = this.ProcessPriority;
                }
                string lastErrorLine = string.Empty;
                proc.ErrorDataReceived += delegate (object o, DataReceivedEventArgs args) {
                    if ((args.Data != null) && !string.IsNullOrEmpty(args.Data))
                    {
                        lastErrorLine = args.Data;
                    }
                };
                proc.BeginErrorReadLine();
                if (inputBytes != null)
                {
                    proc.StandardInput.BaseStream.Write(inputBytes, 0, inputBytes.Length);
                    proc.StandardInput.BaseStream.Flush();
                    proc.StandardInput.Close();
                }
                long length = 0L;
                if (outputStream != null)
                {
                    length = this.ReadStdOutToStream(proc, outputStream);
                }
                proc.WaitForExit();
                if ((outputStream == null) && File.Exists(outputImgFilePath))
                {
                    length = new FileInfo(outputImgFilePath).Length;
                }
                this.CheckExitCode(proc.ExitCode, lastErrorLine, length > 0L);
                proc.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Html to image generation failed: " + ex.Message, ex);
            }
        }

        private int ReadStdOutToStream(Process proc, Stream outputStream)
        {
            int num;
            byte[] buffer = new byte[0x8000];
            int num2 = 0;
            while ((num = proc.StandardOutput.BaseStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                outputStream.Write(buffer, 0, num);
                num2 += num;
            }
            return num2;
        }

        private void WaitProcessForExit(Process proc)
        {
            bool hasValue = this.ExecutionTimeout.HasValue;
            if (hasValue)
            {
                proc.WaitForExit((int)this.ExecutionTimeout.Value.TotalMilliseconds);
            }
            else
            {
                proc.WaitForExit();
            }
            if (hasValue && !proc.HasExited)
            {
                proc.Kill();
                throw new WkHtmlToImageException(-2, string.Format("WkHtmlToImage process exceeded execution timeout ({0}) and was aborted", this.ExecutionTimeout));
            }
        }
    }
}
