﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class TurboTitler : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d:\d\d:\d\d\.\d\d,\d:\d\d:\d\d\.\d\d,NTP ", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".tts"; }
        }

        public override string Name
        {
            get { return "TurboTitler"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();
            LoadSubtitle(subtitle, lines, fileName);
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title, bool roundSecond = false)
        {
            //0:01:37.89,0:01:40.52,NTP You should come to the Drama Club, too.
            //0:01:40.52,0:01:43.77,NTP Yeah. The Drama Club is worried|that you haven't been coming.
            //0:01:44.13,0:01:47.00,NTP I see. Sorry, I'll drop by next time.

            const string paragraphWriteFormat = "{0},{1},NTP {2}";

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
            {
                var text = p.Text.Replace(Environment.NewLine, "|");
                sb.AppendLine(string.Format(paragraphWriteFormat, EncodeTimeCode(p.StartTime), EncodeTimeCode(p.EndTime), text));
            }
            return sb.ToString().Trim();
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0:01:37.89,0:01:40.52,NTP You...|Line2!
            _errorCount = 0;

            subtitle.Paragraphs.Clear();
            char[] splitChars = { ':', '.' };
            foreach (string line in lines)
            {
                if (RegexTimeCodes.IsMatch(line))
                {
                    string[] parts = line.Substring(0, 10).Trim().Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4)
                    {
                        try
                        {
                            var start = DecodeTimeCode(parts);
                            parts = line.Substring(11, 10).Trim().Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                            var end = DecodeTimeCode(parts);
                            string text = line.Substring(25).Trim();
                            var p = new Paragraph();
                            p.Text = text.Replace("|", Environment.NewLine);
                            p.StartTime = start;
                            p.EndTime = end;
                            subtitle.Paragraphs.Add(p);
                        }
                        catch
                        {
                            _errorCount++;
                        }
                    }
                }
                else
                {
                    _errorCount++;
                }
            }
            subtitle.Renumber();
        }

        private static string EncodeTimeCode(TimeCode time)
        {
            //0:01:37.89
            return string.Format("{0:0}:{1:00}:{2:00}.{3:00}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 10);
        }

        private static TimeCode DecodeTimeCode(string[] parts)
        {
            var hour = int.Parse(parts[0]);
            var minutes = int.Parse(parts[1]);
            var seconds = int.Parse(parts[2]);
            var ms = int.Parse(parts[3]);
            return new TimeCode(hour, minutes, seconds, ms * 10);
        }

    }
}
