﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// Export from Subtitle Edit OCR window
    /// </summary>
    public class SeImageHtmlIndex : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^#\d+:\d+", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".html"; }
        }

        public override string Name
        {
            get { return "SE image HTML index"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            if (fileName != null && !fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                return false;

            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title, bool roundSecond = false)
        {
            throw new NotImplementedException();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            _errorCount = 0;
            foreach (string line in lines)
            {
                if (line.Contains(".png") && RegexTimeCodes.IsMatch(line))
                {
                    int idx = line.IndexOf("<div");
                    if (idx > 0)
                    {
                        try
                        {
                            var s = line.Replace("&gt;", ">").Substring(0, idx);
                            s = s.Remove(0, s.IndexOf(':') + 1);
                            var arr = s.Split(new[] { '-', '>' }, StringSplitOptions.RemoveEmptyEntries);
                            var p = new Paragraph();
                            p.StartTime = DecodeTimeCode(arr[0]);
                            p.EndTime = DecodeTimeCode(arr[1]);
                            int start = line.IndexOf("<img src=") + 9;
                            int end = line.IndexOf(".png") + 4;
                            p.Text = line.Substring(start, end - start).Trim(new[] { '"', '\'' });

                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
            }
            subtitle.Renumber();
        }

        private TimeCode DecodeTimeCode(string timeCode)
        {
            var parts = timeCode.Split(new[] { ':', '-', '>', ',', '.' }, StringSplitOptions.RemoveEmptyEntries);
            int milliseconds = int.Parse(parts[parts.Length - 1]);
            int seconds = int.Parse(parts[parts.Length - 2]);
            int minutes = 0;
            if (parts.Length > 2)
                minutes = int.Parse(parts[parts.Length - 3]);
            int hour = 0;
            if (parts.Length > 3)
                hour = int.Parse(parts[parts.Length - 4]);

            return new TimeCode(hour, minutes, seconds, milliseconds);
        }

    }
}
