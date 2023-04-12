﻿using System;

namespace Nikse.SubtitleEdit.Core
{
    public class Paragraph
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public TimeCode StartTime { get; set; }

        public TimeCode EndTime { get; set; }

        public TimeCode Duration => new TimeCode(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);

        public int StartFrame { get; set; }

        public int EndFrame { get; set; }

        public bool Forced { get; set; }

        public string Extra { get; set; }

        public bool IsComment { get; set; }

        public string Actor { get; set; }
        public string Region { get; set; }

        public string MarginL { get; set; }
        public string MarginR { get; set; }
        public string MarginV { get; set; }

        public string Effect { get; set; }

        public int Layer { get; set; }

        public string ID { get; }

        public string Language { get; set; }

        public string Style { get; set; }

        public bool NewSection { get; set; }
        
        public string Bookmark { get; set; }

        public bool IsDefault => Math.Abs(StartTime.TotalMilliseconds) < 0.01 && Math.Abs(EndTime.TotalMilliseconds) < 0.01 && string.IsNullOrEmpty(Text);

        private static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public Paragraph() : this(new TimeCode(), new TimeCode(), string.Empty)
        {
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
            ID = GenerateId();
        }

        public Paragraph(Paragraph paragraph, bool generateNewId = true)
        {
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TotalMilliseconds);
            EndTime = new TimeCode(paragraph.EndTime.TotalMilliseconds);
            Forced = paragraph.Forced;
            Extra = paragraph.Extra;
            IsComment = paragraph.IsComment;
            Actor = paragraph.Actor;
            Region = paragraph.Region;
            MarginL = paragraph.MarginL;
            MarginR = paragraph.MarginR;
            MarginV = paragraph.MarginV;
            Effect = paragraph.Effect;
            Layer = paragraph.Layer;
            ID = generateNewId ? GenerateId() : paragraph.ID;
            Language = paragraph.Language;
            Style = paragraph.Style;
            NewSection = paragraph.NewSection;
            Bookmark = paragraph.Bookmark;
        }

        public Paragraph(int startFrame, int endFrame, string text)
        {
            StartTime = new TimeCode(0, 0, 0, 0);
            EndTime = new TimeCode(0, 0, 0, 0);
            StartFrame = startFrame;
            EndFrame = endFrame;
            Text = text;
            ID = GenerateId();
        }

        public Paragraph(string text, double startTotalMilliseconds, double endTotalMilliseconds)
            : this(new TimeCode(startTotalMilliseconds), new TimeCode(endTotalMilliseconds), text)
        {
        }

        public void Adjust(double factor, double adjustmentInSeconds)
        {
            if (StartTime.IsMaxTime)
            {
                return;
            }

            StartTime.TotalMilliseconds = StartTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit;
            EndTime.TotalMilliseconds = EndTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit;
        }

        public void CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            StartFrame = (int)Math.Round((StartTime.TotalMilliseconds / TimeCode.BaseUnit * frameRate));
            EndFrame = (int)Math.Round((EndTime.TotalMilliseconds / TimeCode.BaseUnit * frameRate));
        }

        public void CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
            StartTime.TotalMilliseconds = StartFrame * (TimeCode.BaseUnit / frameRate);
            EndTime.TotalMilliseconds = EndFrame * (TimeCode.BaseUnit / frameRate);
        }

        public override string ToString()
        {
            return $"{StartTime} --> {EndTime} {Text}";
        }

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);

        public double WordsPerMinute
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return 0;
                var wordCount = HtmlUtil.RemoveHtmlTags(Text, true).Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '(', ')', '[', ']', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                return (60.0 / Duration.TotalSeconds) * wordCount;
            }
        }
    }
}
