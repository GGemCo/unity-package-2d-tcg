#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;

namespace GGemCo2DTcgEditor
{
    internal sealed class TcgLocalizationReport
    {
        public int Created;
        public int Updated;
        public int Skipped;

        private readonly List<string> _info = new(256);
        private readonly List<string> _warn = new(256);

        public string SummaryLine => $"Created={Created}, Updated={Updated}, Skipped={Skipped}, Warn={_warn.Count}";

        public void AddInfo(string category, string msg) => _info.Add($"[INFO:{category}] {msg}");
        public void AddWarn(string category, string msg) => _warn.Add($"[WARN:{category}] {msg}");

        public override string ToString()
        {
            var sb = new StringBuilder(4096);
            sb.AppendLine("==== TCG Localization Report ====");
            sb.AppendLine(SummaryLine);
            sb.AppendLine();

            if (_warn.Count > 0)
            {
                sb.AppendLine("---- Warnings ----");
                foreach (var w in _warn) sb.AppendLine(w);
                sb.AppendLine();
            }

            sb.AppendLine("---- Info ----");
            foreach (var i in _info) sb.AppendLine(i);

            return sb.ToString();
        }
    }
}
#endif