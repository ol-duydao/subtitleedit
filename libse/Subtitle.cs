using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Nikse.SubtitleEdit.Core
{
    public class Subtitle : CaptionTrackContent
    {
        [JsonIgnore]
        public string Header { get; set; }
        [JsonIgnore]
        public string Footer { get; set; }
        [JsonIgnore]
        public string FileName { get; set; }

        public const int MaximumHistoryItems = 100;
        [JsonIgnore]
        public SubtitleFormat OriginalFormat { get; private set; }
        
        public string Label { get; set; }
        
        string format;
        /// <summary>
        /// Gets or sets the string format of the subttile
        /// </summary>
        /// <value>
        /// The format.
        /// </value>
        public override string Format
        {
            get => format ?? OriginalFormat?.Name ?? WebVTT.Const.WEB_VTT_NAME;

            set => format = value;
        }

        private string content;
        /// <summary>
        /// Gets or sets the content of the subtitle
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public override string Content
        {
            get => ToText(new WebVTT());

            set => content = value;
        }

        [JsonIgnore]
        public List<HistoryItem> HistoryItems { get; }

        public Subtitle()
        {
            Paragraphs = new List<Paragraph>();
            HistoryItems = new List<HistoryItem>();
            FileName = "Untitled";
        }

        public Subtitle(List<HistoryItem> historyItems)
            : this()
        {
            HistoryItems = historyItems;
        }

        /// <summary>
        /// Copy constructor (only paragraphs)
        /// </summary>
        /// <param name="subtitle">Subtitle to copy</param>
        /// <param name="generateNewId">Generate new ID (guid) for paragraphs</param>
        public Subtitle(Subtitle subtitle, bool generateNewId = true)
            : this()
        {
            if (subtitle == null)
                return;

            foreach (var p in subtitle.Paragraphs)
            {
                Paragraphs.Add(new Paragraph(p, generateNewId));
            }
            WasLoadedWithFrameNumbers = subtitle.WasLoadedWithFrameNumbers;
            Header = subtitle.Header;
            Footer = subtitle.Footer;
            FileName = subtitle.FileName;
        }

        [JsonIgnore]
        public List<Paragraph> Paragraphs { get; private set; }

        /// <summary>
        /// Get the paragraph of index, null if out of bounds
        /// </summary>
        /// <param name="index">Index of wanted paragraph</param>
        /// <returns>Paragraph, null if index is index is out of bounds</returns>
        public Paragraph GetParagraphOrDefault(int index)
        {
            if (Paragraphs == null || Paragraphs.Count <= index || index < 0)
                return null;

            return Paragraphs[index];
        }

        public Paragraph GetParagraphOrDefaultById(string id)
        {
            return Paragraphs.FirstOrDefault(p => p.ID == id);
        }

        public SubtitleFormat ReloadLoadSubtitle(List<string> lines, string fileName, SubtitleFormat format)
        {
            Paragraphs.Clear();
            if (format != null && format.IsMine(lines, fileName))
            {
                format.LoadSubtitle(this, lines, fileName);
                OriginalFormat = format;
                return format;
            }
            foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    OriginalFormat = subtitleFormat;
                    return subtitleFormat;
                }
            }
            return null;
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding)
        {
            return LoadSubtitle(fileName, out encoding, useThisEncoding, false);
        }

        public SubtitleFormat LoadSubtitle(string fileName, out Encoding encoding, Encoding useThisEncoding, bool batchMode)
        {
            FileName = fileName;

            Paragraphs = new List<Paragraph>();

            var lines = new List<string>();
            StreamReader sr;
            if (useThisEncoding != null)
            {
                try
                {
                    sr = new StreamReader(fileName, useThisEncoding);
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    encoding = Encoding.UTF8;
                    return null;
                }
            }
            else
            {
                try
                {
                    sr = new StreamReader(fileName, LanguageAutoDetect.GetEncodingFromFile(fileName), true);
                }
                catch
                {
                    try
                    {
                        Stream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        sr = new StreamReader(fs);
                    }
                    catch (Exception exception)
                    {
                        System.Diagnostics.Debug.WriteLine(exception.Message);
                        encoding = Encoding.UTF8;
                        return null;
                    }
                }
            }

            encoding = sr.CurrentEncoding;
            while (!sr.EndOfStream)
                lines.Add(sr.ReadLine());
            sr.Close();

            foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, fileName))
                {
                    Header = null;
                    subtitleFormat.BatchMode = batchMode;
                    subtitleFormat.LoadSubtitle(this, lines, fileName);
                    OriginalFormat = subtitleFormat;
                    WasLoadedWithFrameNumbers = OriginalFormat.IsFrameBased;
                    if (WasLoadedWithFrameNumbers)
                        CalculateTimeCodesFromFrameNumbers(Configuration.Settings.General.CurrentFrameRate);
                    return subtitleFormat;
                }
            }

            if (useThisEncoding == null)
                return LoadSubtitle(fileName, out encoding, Encoding.Unicode);

            return null;
        }
        
        /// <summary>
        /// Loads the subtile using the stream reader
        /// </summary>
        /// <param name="_oStreamReader">The stream reader.</param>
        /// <param name="_oEncoding">The encoding.</param>
        /// <param name="_bBatchMode">if set to <c>true</c> [_b batch mode].</param>
        /// <returns></returns>
        protected SubtitleFormat LoadSubtile(StreamReader _oStreamReader, out Encoding _oEncoding, bool _bBatchMode)
        {
            var lines = new List<string>();
            Paragraphs = Paragraphs ?? new List<Paragraph>();
            _oEncoding = _oStreamReader.CurrentEncoding;
            while (!_oStreamReader.EndOfStream)
                lines.Add(_oStreamReader.ReadLine());
            _oStreamReader.Close();

            foreach (SubtitleFormat subtitleFormat in SubtitleFormat.AllSubtitleFormats)
            {
                if (subtitleFormat.IsMine(lines, null))
                {
                    Header = null;
                    subtitleFormat.BatchMode = _bBatchMode;
                    subtitleFormat.LoadSubtitle(this, lines, null);
                    OriginalFormat = subtitleFormat;
                    WasLoadedWithFrameNumbers = OriginalFormat.IsFrameBased;
                    return subtitleFormat;
                }
            }
            return null;
        }

        public void MakeHistoryForUndo(string description, SubtitleFormat subtitleFormat, DateTime fileModified, Subtitle original, string originalSubtitleFileName, int lineNumber, int linePosition, int linePositionAlternate)
        {
            // don't fill memory with history - use a max rollback points
            if (HistoryItems.Count > MaximumHistoryItems)
                HistoryItems.RemoveAt(0);

            HistoryItems.Add(new HistoryItem(HistoryItems.Count, this, description, FileName, fileModified, subtitleFormat.FriendlyName, original, originalSubtitleFileName, lineNumber, linePosition, linePositionAlternate));
        }

        [JsonIgnore]
        public bool CanUndo => HistoryItems.Count > 0;

        public string UndoHistory(int index, out string subtitleFormatFriendlyName, out DateTime fileModified, out Subtitle originalSubtitle, out string originalSubtitleFileName)
        {
            Paragraphs.Clear();
            foreach (Paragraph p in HistoryItems[index].Subtitle.Paragraphs)
                Paragraphs.Add(new Paragraph(p));

            subtitleFormatFriendlyName = HistoryItems[index].SubtitleFormatFriendlyName;
            FileName = HistoryItems[index].FileName;
            fileModified = HistoryItems[index].FileModified;
            originalSubtitle = new Subtitle(HistoryItems[index].OriginalSubtitle);
            originalSubtitleFileName = HistoryItems[index].OriginalSubtitleFileName;

            return FileName;
        }

        /// <summary>
        /// Creates subtitle as text in it's native format
        /// </summary>
        /// <param name="format">Format to output</param>
        /// <param name="roundSecond">Round to seconds</param>
        /// <returns>Native format as text string</returns>
        public string ToText(SubtitleFormat format, bool roundSecond = false)
        {
            return format.ToText(this, Path.GetFileNameWithoutExtension(FileName), roundSecond);
        }

        public void AddTimeToAllParagraphs(TimeSpan time)
        {
            var milliseconds = time.TotalMilliseconds;
            foreach (var p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds += milliseconds;
                p.EndTime.TotalMilliseconds += milliseconds;
            }
        }

        /// <summary>
        /// Calculate the time codes from frame number/frame rate
        /// </summary>
        /// <param name="frameRate">Number of frames per second</param>
        /// <returns>True if times could be calculated</returns>
        public bool CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
            if (OriginalFormat == null || OriginalFormat.IsTimeBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateTimeCodesFromFrameNumbers(frameRate);
            }
            return true;
        }

        /// <summary>
        /// Calculate the frame numbers from time codes/frame rate
        /// </summary>
        /// <param name="frameRate"></param>
        /// <returns></returns>
        public bool CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            if (OriginalFormat == null || OriginalFormat.IsFrameBased)
                return false;

            foreach (Paragraph p in Paragraphs)
            {
                p.CalculateFrameNumbersFromTimeCodes(frameRate);
            }

            FixEqualOrJustOverlappingFrameNumbers();

            return true;
        }

        public void CalculateFrameNumbersFromTimeCodesNoCheck(double frameRate)
        {
            foreach (Paragraph p in Paragraphs)
                p.CalculateFrameNumbersFromTimeCodes(frameRate);

            FixEqualOrJustOverlappingFrameNumbers();
        }

        private void FixEqualOrJustOverlappingFrameNumbers()
        {
            for (int i = 0; i < Paragraphs.Count - 1; i++)
            {
                Paragraph p = Paragraphs[i];
                Paragraph next = GetParagraphOrDefault(i + 1);
                if (next != null && (p.EndFrame == next.StartFrame || p.EndFrame == next.StartFrame + 1))
                    p.EndFrame = next.StartFrame - 1;
            }
        }

        public void ChangeFrameRate(double oldFrameRate, double newFrameRate)
        {
            var factor = SubtitleFormat.GetFrameForCalculation(oldFrameRate) / SubtitleFormat.GetFrameForCalculation(newFrameRate);
            foreach (var p in Paragraphs)
            {
                p.StartTime.TotalMilliseconds *= factor;
                p.EndTime.TotalMilliseconds *= factor;
            }
        }

        [JsonIgnore]
        public bool WasLoadedWithFrameNumbers { get; set; }

        public void AdjustDisplayTimeUsingPercent(double percent, List<int> selectedIndexes)
        {
            for (var i = 0; i < Paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = double.MaxValue;
                    if (i + 1 < Paragraphs.Count)
                        nextStartMilliseconds = Paragraphs[i + 1].StartTime.TotalMilliseconds;

                    var newEndMilliseconds = Paragraphs[i].EndTime.TotalMilliseconds;
                    newEndMilliseconds = Paragraphs[i].StartTime.TotalMilliseconds + (((newEndMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) * percent) / 100);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;
                    Paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                }
            }
        }

        public void AdjustDisplayTimeUsingSeconds(double seconds, List<int> selectedIndexes)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    double nextStartMilliseconds = double.MaxValue;
                    if (i + 1 < Paragraphs.Count)
                        nextStartMilliseconds = Paragraphs[i + 1].StartTime.TotalMilliseconds;

                    double newEndMilliseconds = Paragraphs[i].EndTime.TotalMilliseconds + (seconds * TimeCode.BaseUnit);
                    if (newEndMilliseconds > nextStartMilliseconds)
                        newEndMilliseconds = nextStartMilliseconds - 1;

                    if (seconds < 0)
                    {
                        if (Paragraphs[i].StartTime.TotalMilliseconds + 100 > newEndMilliseconds)
                            Paragraphs[i].EndTime.TotalMilliseconds = Paragraphs[i].StartTime.TotalMilliseconds + 100;
                        else
                            Paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                    else
                    {
                        Paragraphs[i].EndTime.TotalMilliseconds = newEndMilliseconds;
                    }
                }
            }
        }
        
        private void AdjustDisplayTimeUsingMilliseconds(int idx, double ms)
        {
            var p = Paragraphs[idx];
            var nextStartTimeInMs = double.MaxValue;
            if (idx + 1 < Paragraphs.Count)
            {
                nextStartTimeInMs = Paragraphs[idx + 1].StartTime.TotalMilliseconds;
            }
            var newEndTimeInMs = p.EndTime.TotalMilliseconds + ms;

            // handle overlap with next
            if (newEndTimeInMs > nextStartTimeInMs)
            {
                newEndTimeInMs = nextStartTimeInMs - 1; // MinimumMillisecondsBetweenLines = 1
            }

            // fix too short duration
            var minDur = Math.Max(1, 100); // SubtitleMinimumDisplayMilliseconds = 1
            if (p.StartTime.TotalMilliseconds + minDur > newEndTimeInMs)
            {
                newEndTimeInMs = p.StartTime.TotalMilliseconds + minDur;
            }

            if (ms > 0 && newEndTimeInMs < p.EndTime.TotalMilliseconds || ms < 0 && newEndTimeInMs > p.EndTime.TotalMilliseconds)
            {
                return; // do not adjust wrong way
            }

            p.EndTime.TotalMilliseconds = newEndTimeInMs;
        }

        public void RecalculateDisplayTimes(double maxCharactersPerSecond, List<int> selectedIndexes)
        {
            for (int i = 0; i < Paragraphs.Count; i++)
            {
                if (selectedIndexes == null || selectedIndexes.Contains(i))
                {
                    Paragraph p = Paragraphs[i];
                    double duration = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                    p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
                    while (Utilities.GetCharactersPerSecond(p) > maxCharactersPerSecond)
                    {
                        duration++;
                        p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + duration;
                    }

                    Paragraph next = GetParagraphOrDefault(i + 1);
                    if (next != null && p.StartTime.TotalMilliseconds + duration + Configuration.Settings.General.MinimumMillisecondsBetweenLines > next.StartTime.TotalMilliseconds)
                    {
                        p.EndTime.TotalMilliseconds = next.StartTime.TotalMilliseconds - Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                        if (p.Duration.TotalMilliseconds <= 0)
                            p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 1;
                    }
                }
            }
        }

        public void Renumber(int startNumber = 1)
        {
            var number = startNumber;
            var l = Paragraphs.Count + number;
            while (number < l)
            {
                var p = Paragraphs[number - startNumber];
                p.Number = number++;
            }
        }

        public int GetIndex(Paragraph p)
        {
            if (p == null)
            {
                return -1;
            }

            var index = Paragraphs.IndexOf(p);
            if (index >= 0)
            {
                return index;
            }

            for (int i = 0; i < Paragraphs.Count; i++)
            {
                if (p.ID == Paragraphs[i].ID)
                {
                    return i;
                }

                if (i < Paragraphs.Count - 1 && p.ID == Paragraphs[i + 1].ID)
                {
                    return i + 1;
                }

                if (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 &&
                    Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1)
                {
                    return i;
                }

                if (p.Number == Paragraphs[i].Number && (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                                                         Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                {
                    return i;
                }

                if (p.Text == Paragraphs[i].Text && (Math.Abs(p.StartTime.TotalMilliseconds - Paragraphs[i].StartTime.TotalMilliseconds) < 0.1 ||
                                                     Math.Abs(p.EndTime.TotalMilliseconds - Paragraphs[i].EndTime.TotalMilliseconds) < 0.1))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get paragraph index by time in seconds
        /// </summary>
        public int GetIndex(double seconds)
        {
            var totalMilliseconds = seconds * TimeCode.BaseUnit;
            for (var i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                if (totalMilliseconds >= p.StartTime.TotalMilliseconds && totalMilliseconds <= p.EndTime.TotalMilliseconds)
                {
                    return i;
                }
            }
            return -1;
        }

        public Paragraph GetFirstAlike(Paragraph p)
        {
            return Paragraphs.FirstOrDefault(item => 
                Math.Abs(p.StartTime.TotalMilliseconds - item.StartTime.TotalMilliseconds) < 0.1 
                && Math.Abs(p.EndTime.TotalMilliseconds - item.EndTime.TotalMilliseconds) < 0.1 
                && p.Text == item.Text);
        }

        public int RemoveEmptyLines()
        {
            var count = Paragraphs.Count;
            if (count <= 0)
            {
                return 0;
            }

            int firstNumber = Paragraphs[0].Number;
            for (int i = Paragraphs.Count - 1; i >= 0; i--)
            {
                var p = Paragraphs[i];
                if (p.Text.IsOnlyControlCharactersOrWhiteSpace())
                {
                    Paragraphs.RemoveAt(i);
                }
            }
            if (count != Paragraphs.Count)
            {
                Renumber(firstNumber);
            }
            return count - Paragraphs.Count;
        }

        /// <summary>
        /// Removes paragrahs by a list of indices
        /// </summary>
        /// <param name="indices">Indices of pargraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIndices(IEnumerable<int> indices)
        {
            var count = 0;
            foreach (var index in indices.OrderByDescending(p => p))
            {
                if (index < 0 || index >= Paragraphs.Count) continue;
                Paragraphs.RemoveAt(index);
                count++;
            }
            return count;
        }

        /// <summary>
        /// Removes paragrahs by a list of IDs
        /// </summary>
        /// <param name="ids">IDs of pargraphs/lines to delete</param>
        /// <returns>Number of lines deleted</returns>
        public int RemoveParagraphsByIds(IEnumerable<string> ids)
        {
            var beforeCount = Paragraphs.Count;
            Paragraphs = Paragraphs.Where(p => !ids.Contains(p.ID)).ToList();
            return beforeCount - Paragraphs.Count;
        }

        /// <summary>
        /// Sort subtitle paragraphs
        /// </summary>
        /// <param name="sortCriteria">Paragraph sort criteria</param>
        public void Sort(SubtitleSortCriteria sortCriteria)
        {
            switch (sortCriteria)
            {
                case SubtitleSortCriteria.Number:
                    Paragraphs.Sort((p1, p2) => p1.Number.CompareTo(p2.Number));
                    break;
                case SubtitleSortCriteria.StartTime:
                    Paragraphs.Sort((p1, p2) => p1.StartTime.TotalMilliseconds.CompareTo(p2.StartTime.TotalMilliseconds));
                    break;
                case SubtitleSortCriteria.EndTime:
                    Paragraphs.Sort((p1, p2) => p1.EndTime.TotalMilliseconds.CompareTo(p2.EndTime.TotalMilliseconds));
                    break;
                case SubtitleSortCriteria.Duration:
                    Paragraphs.Sort((p1, p2) => p1.Duration.TotalMilliseconds.CompareTo(p2.Duration.TotalMilliseconds));
                    break;
                case SubtitleSortCriteria.Text:
                    Paragraphs.Sort((p1, p2) => string.Compare(p1.Text, p2.Text, StringComparison.Ordinal));
                    break;
                case SubtitleSortCriteria.TextMaxLineLength:
                    Paragraphs.Sort((p1, p2) => Utilities.GetMaxLineLength(p1.Text).CompareTo(Utilities.GetMaxLineLength(p2.Text)));
                    break;
                case SubtitleSortCriteria.TextTotalLength:
                    Paragraphs.Sort((p1, p2) => p1.Text.Length.CompareTo(p2.Text.Length));
                    break;
                case SubtitleSortCriteria.TextNumberOfLines:
                    Paragraphs.Sort((p1, p2) => p1.NumberOfLines.CompareTo(p2.NumberOfLines));
                    break;
                case SubtitleSortCriteria.TextCharactersPerSeconds:
                    Paragraphs.Sort((p1, p2) => Utilities.GetCharactersPerSecond(p1).CompareTo(Utilities.GetCharactersPerSecond(p2)));
                    break;
                case SubtitleSortCriteria.WordsPerMinute:
                    Paragraphs.Sort((p1, p2) => p1.WordsPerMinute.CompareTo(p2.WordsPerMinute));
                    break;
                case SubtitleSortCriteria.Style:
                    Paragraphs.Sort((p1, p2) => string.Compare(p1.Extra, p2.Extra, StringComparison.Ordinal));
                    break;
            }
        }

        public int InsertParagraphInCorrectTimeOrder(Paragraph newParagraph)
        {
            for (var i = 0; i < Paragraphs.Count; i++)
            {
                var p = Paragraphs[i];
                if (!(newParagraph.StartTime.TotalMilliseconds < p.StartTime.TotalMilliseconds)) continue;
                Paragraphs.Insert(i, newParagraph);
                return i;
            }
            Paragraphs.Add(newParagraph);
            return Paragraphs.Count - 1;
        }
        
        /// <summary>
        /// Returns the first paragraph that within the given time
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public Paragraph GetFirstParagraphOrDefaultByTime(double milliseconds)
        {
            return Paragraphs.FirstOrDefault(
                p => p.StartTime.TotalMilliseconds < milliseconds && milliseconds < p.EndTime.TotalMilliseconds);
        }

        /// <summary>
        /// Fast hash code for subtitle (only includes start + end + text)
        /// </summary>
        /// <returns>Hash value that can be used for quick compare</returns>
        public string GetFastHashCode()
        {
            var sb = new StringBuilder(Paragraphs.Count * 50);
            foreach (var p in Paragraphs)
            {
                sb.Append(p.StartTime.TotalMilliseconds.GetHashCode());
                sb.Append(p.EndTime.TotalMilliseconds.GetHashCode());
                sb.Append(p.Text);
            }
            return sb.ToString().TrimEnd();
        }
        
        /// <summary>
        /// Fast hash code for subtitle - includes pre (encoding atm) + header + number + start + end + text + style + actor + extra.
        /// </summary>
        /// <returns>Hash value that can be used for quick compare</returns>
        public int GetFastHashCode(string pre)
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                if (pre != null)
                {
                    hash = hash * 23 + pre.GetHashCode();
                }
                if (Header != null)
                {
                    hash = hash * 23 + Header.Trim().GetHashCode();
                }
                var max = Paragraphs.Count;
                for (int i = 0; i < max; i++)
                {
                    var p = Paragraphs[i];
                    hash = hash * 23 + p.Number.GetHashCode();
                    hash = hash * 23 + p.StartTime.TotalMilliseconds.GetHashCode();
                    hash = hash * 23 + p.EndTime.TotalMilliseconds.GetHashCode();
                    hash = hash * 23 + p.Text.GetHashCode();
                    if (p.Style != null)
                    {
                        hash = hash * 23 + p.Style.GetHashCode();
                    }
                    if (p.Extra != null)
                    {
                        hash = hash * 23 + p.Extra.GetHashCode();
                    }
                    if (p.Actor != null)
                    {
                        hash = hash * 23 + p.Actor.GetHashCode();
                    }
                }
                return hash;
            }
        }

        /// <summary>
        /// Concatenates all the Paragraph its Text property from Paragraphs, using the default line terminator between each Text.
        /// </summary>
        /// <returns>Contatenated Text property of all Paragraph present in Paragraphs property.</returns>
        public string GetAllTexts()
        {
            var max = Paragraphs.Count;
            const int averageLength = 40;
            var sb = new StringBuilder(max * averageLength);
            for (var index = 0; index < max; index++)
            {
                sb.AppendLine(Paragraphs[index].Text);
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// Concatenates all Paragraphs Text property, using the default NewLine string between each Text.
        /// </summary>
        /// <returns>Concatenated Text property of all Paragraphs.</returns>
        public string GetAllTexts(int stopAfterBytes)
        {
            var max = Paragraphs.Count;
            const int averageLength = 40;
            var sb = new StringBuilder(max * averageLength);
            for (var index = 0; index < max; index++)
            {
                sb.AppendLine(Paragraphs[index].Text);
                if (sb.Length > stopAfterBytes)
                {
                    return sb.ToString();
                }
            }
            return sb.ToString();
        }
    }
}
