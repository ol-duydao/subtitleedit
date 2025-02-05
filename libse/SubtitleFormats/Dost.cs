﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Dost : SubtitleFormat
    {
        private static readonly Regex RegexTimeCodes = new Regex(@"^\d\d\d\d\t\d\d:\d\d:\d\d:\d\d\t\d\d:\d\d:\d\d:\d\d\t", RegexOptions.Compiled);

        public override string Extension
        {
            get { return ".dost"; }
        }

        public override string Name
        {
            get { return "DOST"; }
        }

        public override bool IsTimeBased
        {
            get { return true; }
        }

        public override bool IsMine(List<string> lines, string fileName)
        {
            var subtitle = new Subtitle();

            var sb = new StringBuilder();
            foreach (string line in lines)
                sb.AppendLine(line);
            if (!sb.ToString().Contains(Environment.NewLine + "NO\tINTIME"))
                return false;
            if (!sb.ToString().Contains("$FORMAT"))
                return false;

            var oldFrameRate = Configuration.Settings.General.CurrentFrameRate;
            LoadSubtitle(subtitle, lines, fileName);
            Configuration.Settings.General.CurrentFrameRate = oldFrameRate;
            return subtitle.Paragraphs.Count > _errorCount;
        }

        public override string ToText(Subtitle subtitle, string title, bool roundSecond = false)
        {
            return "Not implemented";
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            //0001  01:25:59:21 01:26:00:20 0   0   BK02-total_0001.png 0   0
            Paragraph p = null;
            subtitle.Paragraphs.Clear();
            _errorCount = 0;
            foreach (string line in lines)
            {
                string s = line;
                if (RegexTimeCodes.IsMatch(s))
                {
                    var temp = s.Split('\t');
                    if (temp.Length > 7)
                    {
                        string start = temp[1];
                        string end = temp[2];
                        string text = temp[5];
                        try
                        {
                            p = new Paragraph(DecodeTimeCodeFramesFourParts(start.Split(':')), DecodeTimeCodeFramesFourParts(end.Split(':')), text);
                            subtitle.Paragraphs.Add(p);
                        }
                        catch (Exception exception)
                        {
                            _errorCount++;
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                        }
                    }
                }
                else if (line.StartsWith("$DROP="))
                {
                    s = s.Remove(0, "$DROP=".Length);
                    int frameRate;
                    if (int.TryParse(s, out frameRate))
                    {
                        double f = frameRate / TimeCode.BaseUnit;
                        if (f > 10 && f < 500)
                            Configuration.Settings.General.CurrentFrameRate = f;
                    }
                }
                else if (string.IsNullOrWhiteSpace(line) || line.StartsWith('$'))
                {
                    // skip empty lines or lines starting with $
                }
                else if (!string.IsNullOrWhiteSpace(line) && p != null)
                {
                    _errorCount++;
                }
            }

            subtitle.Renumber();
        }

    }
}
