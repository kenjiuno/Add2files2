using Add2files2.Models;
using Add2files2.Properties;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Add2files2
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetCurrentClassLogger();

            var conf = new YamlDotNet.Serialization.DeserializerBuilder()
                .Build()
                .Deserialize<Conf>(
                    File.ReadAllText("Conf.yml")
                );

            if (args.Length >= 1 && args[0] == "build")
            {
                using (var writer = new StreamWriter("Files.txt", false, Encoding.UTF8, 10 * 1024 * 1024))
                {
                    foreach (var dir in conf.dirs)
                    {
                        log.Info(dir);
                        Walk(writer, dir);
                    }
                }
                log.Info("Done");
                return;
            }
            else if (args.Length >= 2 && args[0] == "find")
            {
                var rex = new Regex(string.Join("|", args.Skip(1).Select(arg => "^" + Regex.Escape(arg).Replace("\\*", ".*").Replace("\\?", ".?"))), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

                using (var reader = new StreamReader("Files.txt", Encoding.UTF8, true, 10 * 1024 * 1024))
                {
                    while (true)
                    {
                        var row = reader.ReadLine();
                        if (row == null)
                        {
                            break;
                        }
                        int pos = row.LastIndexOf('\\');
                        if (pos < 0)
                        {
                            pos = 0;
                        }
                        else
                        {
                            pos++;
                        }
                        var target = row.Substring(pos);
                        if (rex.IsMatch(target))
                        {
                            Console.WriteLine(row);
                        }
                    }
                }
                return;
            }

            Console.Error.WriteLine(Resources.Usage);
            Environment.ExitCode = 1;
        }

        private static void Walk(TextWriter lines, string baseDir)
        {
            try
            {
                foreach (var filePath in Directory.GetFiles(baseDir))
                {
                    lines.WriteLine(filePath);
                }
            }
            catch
            {
                // なにもしない
            }

            try
            {
                foreach (var dir in Directory.GetDirectories(baseDir))
                {
                    Walk(lines, dir);
                }
            }
            catch
            {
                // なにもしない
            }

        }
    }
}
