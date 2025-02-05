﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public abstract class SubtitleFormat
    {
        private static IList<SubtitleFormat> _allSubtitleFormats;

        protected static readonly char[] SplitCharColon = { ':' };

        /// <summary>
        /// Formats supported by Subtitle Edit
        /// </summary>
        public static IList<SubtitleFormat> AllSubtitleFormats
        {
            get
            {
                if (_allSubtitleFormats != null)
                    return _allSubtitleFormats;

                _allSubtitleFormats = new List<SubtitleFormat>
                {
                    new SubRip(),
                    new AbcIViewer(),
                    new AdobeAfterEffectsFTME(),
                    new AdobeEncore(),
                    new AdobeEncoreLineTabNewLine(),
                    new AdobeEncoreTabs(),
                    new AdobeEncoreWithLineNumbers(),
                    new AdobeEncoreWithLineNumbersNtsc(),
                    new AdvancedSubStationAlpha(),
                    new AQTitle(),
                    new AvidCaption(),
                    new AvidDvd(),
                    new BelleNuitSubtitler(),
                    new CaptionAssistant(),
                    new Captionate(),
                    new CaptionateMs(),
                    new CaraokeXml(),
                    new Csv(),
                    new Csv2(),
                    new Csv3(),
                    new DCSubtitle(),
                    new DCinemaSmpte2010(),
                    new DCinemaSmpte2007(),
                    new DigiBeta(),
                    new DvdStudioPro(),
                    new DvdStudioProSpaceOne(),
                    new DvdStudioProSpace(),
                    new DvdSubtitle(),
                    new DvdSubtitleSystem(),
                    new Ebu(),
                    new Edl(),
                    new Eeg708(),
                    new F4Text(),
                    new F4Rtf(),
                    new F4Xml(),
                    new FabSubtitler(),
                    new FilmEditXml(),
                    new FinalCutProXml(),
                    new FinalCutProXXml(),
                    new FinalCutProXmlGap(),
                    new FinalCutProXCM(),
                    new FinalCutProXml13(),
                    new FinalCutProXml14(),
                    new FinalCutProXml14Text(),
                    new FinalCutProTestXml(),
                    new FinalCutProTest2Xml(),
                    new FlashXml(),
                    new FLVCoreCuePoints(),
                    new Footage(),
                    new GpacTtxt(),
                    new ImageLogicAutocaption(),
                    new IssXml(),
                    new ItunesTimedText(),
                    new Json(),
                    new JsonType2(),
                    new JsonType3(),
                    new JsonType4(),
                    new JsonType5(),
                    new JsonType6(),
                    new JsonType7(),
                    new Lrc(),
                    new MicroDvd(),
                    new MidwayInscriberCGX(),
                    new MPlayer2(),
                    new NciTimedRollUpCaptions(),
                    new OpenDvt(),
                    new Oresme(),
                    new OresmeDocXDocument(),
                    new PE2(),
                    new PinnacleImpression(),
                    new PListCaption(),
                    new QubeMasterImport(),
                    new QuickTimeText(),
                    new RealTime(),
                    new RhozetHarmonic(),
                    new Sami(),
                    new SamiModern(),
                    new SamiYouTube(),
                    new Scenarist(),
                    new ScenaristClosedCaptions(),
                    new ScenaristClosedCaptionsDropFrame(),
                    new SmilTimesheetData(),
                    new SoftNiSub(),
                    new SoftNicolonSub(),
                    new SonyDVDArchitect(),
                    new SonyDVDArchitectExplicitDuration(),
                    new SonyDVDArchitectLineAndDuration(),
                    new SonyDVDArchitectLineDurationLength(),
                    new SonyDVDArchitectTabs(),
                    new SonyDVDArchitectWithLineNumbers(),
                    new Spruce(),
                    new SpruceWithSpace(),
                    new StructuredTitles(),
                    new SubStationAlpha(),
                    new SubtitleEditorProject(),
                    new SubViewer10(),
                    new SubViewer20(),
                    new SwiftInterchange2(),
                    new SwiftText(),
                    new SwiftTextLineNumber(),
                    new SwiftTextLineNOAndDur(),
                    new Tek(),
                    new TimeXml(),
                    new TimeXml2(),
                    new TimedText10(),
                    new TimedText200604(),
                    new TimedText200604CData(),
                    new TimedText(),
                    new TitleExchangePro(),
                    new Titra(),
                    new TmpegEncText(),
                    new TmpegEncAW5(),
                    new TmpegEncXml(),
                    new TMPlayer(),
                    new TranscriberXml(),
                    new Tmx14(),
                    new TurboTitler(),
                    new UniversalSubtitleFormat(),
                    new UTSubtitleXml(),
                    new Utx(),
                    new UtxFrames(),
                    new UleadSubtitleFormat(),
                    new VocapiaSplit(),
                    new WebVTT(),
                    new WebVTTFileWithLineNumber(),
                    new Xif(),
                    new YouTubeAnnotations(),
                    new YouTubeSbv(),
                    new YouTubeTranscript(),
                    new YouTubeTranscriptOneLine(),
                    new ZeroG(),
                    // new Idx(),
                    new UnknownSubtitle1(),
                    new UnknownSubtitle2(),
                    new UnknownSubtitle3(),
                    new UnknownSubtitle4(),
                    new UnknownSubtitle5(),
                    new UnknownSubtitle6(),
                    new UnknownSubtitle7(),
                    new UnknownSubtitle8(),
                    new UnknownSubtitle9(),
                    new UnknownSubtitle10(),
                    new UnknownSubtitle11(),
                    new UnknownSubtitle12(),
                    new UnknownSubtitle13(),
                    new UnknownSubtitle14(),
                    new UnknownSubtitle15(),
                    new UnknownSubtitle16(),
                    new UnknownSubtitle17(),
                    new UnknownSubtitle18(),
                    new UnknownSubtitle19(),
                    new UnknownSubtitle20(),
                    new UnknownSubtitle21(),
                    new UnknownSubtitle22(),
                    new UnknownSubtitle23(),
                    new UnknownSubtitle24(),
                    new UnknownSubtitle25(),
                    new UnknownSubtitle26(),
                    new UnknownSubtitle27(),
                    new UnknownSubtitle28(),
                    new UnknownSubtitle29(),
                    new UnknownSubtitle30(),
                    new UnknownSubtitle31(),
                    new UnknownSubtitle32(),
                    new UnknownSubtitle33(),
                    new UnknownSubtitle34(),
                    new UnknownSubtitle35(),
                    new UnknownSubtitle36(),
                    new UnknownSubtitle37(),
                    new UnknownSubtitle38(),
                    new UnknownSubtitle39(),
                    new UnknownSubtitle40(),
                    new UnknownSubtitle41(),
                    new UnknownSubtitle42(),
                    new UnknownSubtitle43(),
                    new UnknownSubtitle44(),
                    new UnknownSubtitle45(),
                    new UnknownSubtitle46(),
                    new UnknownSubtitle47(),
                    new UnknownSubtitle48(),
                    new UnknownSubtitle49(),
                    new UnknownSubtitle50(),
                    new UnknownSubtitle51(),
                    new UnknownSubtitle52(),
                    new UnknownSubtitle53(),
                    new UnknownSubtitle54(),
                    new UnknownSubtitle55(),
                    new UnknownSubtitle56(),
                    new UnknownSubtitle57(),
                    new UnknownSubtitle58(),
                    new UnknownSubtitle59(),
                    new UnknownSubtitle60(),
                    new UnknownSubtitle61(),
                    new UnknownSubtitle62(),
                    new UnknownSubtitle63(),
                    new UnknownSubtitle64(),
                    new UnknownSubtitle65(),
                    new UnknownSubtitle66(),
                    new UnknownSubtitle67(),
                    new UnknownSubtitle68(),
                    new UnknownSubtitle69(),
                    new UnknownSubtitle70(),
                    new UnknownSubtitle71(),
                    new UnknownSubtitle72(),
                    new UnknownSubtitle73(),
                    new UnknownSubtitle74(),
                    new UnknownSubtitle75(),
                    new UnknownSubtitle76(),
                    new UnknownSubtitle77(),
                    new UnknownSubtitle78(),
                    new UnknownSubtitle79(),
                    new UnknownSubtitle80(),
                    new UnknownSubtitle81(),
                    new UnknownSubtitle82(),
                };

                var path = Configuration.PluginsDirectory;
                if (!Directory.Exists(path)) return _allSubtitleFormats;
                var pluginFiles = Directory.GetFiles(path, "*.DLL");
                foreach (var pluginFileName in pluginFiles)
                {
                    try
                    {
                        var assembly = System.Reflection.Assembly.Load(FileUtil.ReadAllBytesShared(pluginFileName));
                        foreach (var exportedType in assembly.GetExportedTypes())
                        {
                            try
                            {
                                object pluginObject = Activator.CreateInstance(exportedType);
                                var po = pluginObject as SubtitleFormat;
                                if (po != null)
                                    _allSubtitleFormats.Insert(1, po);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return _allSubtitleFormats;
            }
        }

        protected int _errorCount;

        public abstract string Extension
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract bool IsTimeBased
        {
            get;
        }
        
        protected double _frameRate = 23.976d;

        public double FrameRate
        {
            get => _frameRate;

            set => _frameRate = value;
        }

        public bool IsFrameBased => !IsTimeBased;

        public string FriendlyName => $"{Name} ({Extension})";

        public int ErrorCount => _errorCount;

        public abstract bool IsMine(List<string> lines, string fileName);

        public abstract string ToText(Subtitle subtitle, string title, bool roundSecond = false);

        public abstract void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName);

        public bool IsVobSubIndexFile => string.CompareOrdinal(Extension, ".idx") == 0;

        public virtual void RemoveNativeFormatting(Subtitle subtitle, SubtitleFormat newFormat)
        {
        }

        public virtual List<string> AlternateExtensions => new List<string>();

        /// <summary>
        /// Gets the subtitle instance format by its name.
        /// </summary>
        /// <param name="name">Name of the format</param>
        /// <returns></returns>
        public static SubtitleFormat GetSubtitleFormatByName(string name)
        {
            return AllSubtitleFormats.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        public static int MillisecondsToFrames(double milliseconds)
        {
            return (int)Math.Round(milliseconds / (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate));
        }
        
        public static int MillisecondsToFrames(double milliseconds, double frameRate)
        {
            return (int)Math.Round(milliseconds / (TimeCode.BaseUnit / frameRate));
        }

        public static int MillisecondsToFramesMaxFrameRate(double milliseconds, double frameRate = 23.97d)
        {
            int frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / frameRate));
            if (frames >= frameRate)
                frames = (int)(frameRate - 0.01);
            return frames;
        }

        public static int FramesToMilliseconds(double frames)
        {
            return (int)Math.Round(frames * (TimeCode.BaseUnit / Configuration.Settings.General.CurrentFrameRate));
        }

        public static int FramesToMilliseconds(double frames, double frameRate)
        {
            return (int)Math.Round(frames * (TimeCode.BaseUnit / frameRate));
        }

        public static int FramesToMillisecondsMax999(double frames, double frameRate = 23.97d)
        {
            int ms = (int)Math.Round(frames * (TimeCode.BaseUnit / frameRate));
            return Math.Min(ms, 999);
        }

        public virtual bool HasStyleSupport => false;

        public bool BatchMode { get; set; }

        public static string ToUtf8XmlString(XmlDocument xml, bool omitXmlDeclaration = false)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = omitXmlDeclaration,
            };
            var result = new StringBuilder();

            using (var xmlWriter = XmlWriter.Create(result, settings))
            {
                xml.Save(xmlWriter);
            }

            return result.ToString().Replace(" encoding=\"utf-16\"", " encoding=\"utf-8\"").Trim();
        }

        public virtual bool IsTextBased => true;
        
        public static double GetFrameForCalculation(double frameRate)
        {
            if (Math.Abs(frameRate - 23.976) < 0.01)
            {
                return 24000.0 / 1001.0;
            }
            if (Math.Abs(frameRate - 29.97) < 0.01)
            {
                return 30000.0 / 1001.0;
            }
            if (Math.Abs(frameRate - 59.94) < 0.01)
            {
                return 60000.0 / 1001.0;
            }

            return frameRate;
        }

        protected TimeCode DecodeTimeCodeFramesTwoParts(string[] parts)
        {
            if (parts == null)
                return new TimeCode(0);
            if (parts.Length != 2)
                throw new InvalidOperationException();
            // 00:00
            return new TimeCode(0, 0, int.Parse(parts[0]), FramesToMillisecondsMax999(int.Parse(parts[1])));
        }

        protected TimeCode DecodeTimeCodeFramesThreeParts(string[] parts)
        {
            if (parts == null)
                return new TimeCode(0);
            if (parts.Length != 3)
                throw new InvalidOperationException();
            // 00:00:00
            return new TimeCode(0, int.Parse(parts[0]), int.Parse(parts[1]), FramesToMillisecondsMax999(int.Parse(parts[2])));
        }

        protected TimeCode DecodeTimeCodeFramesFourParts(string[] parts)
        {
            if (parts == null)
                return new TimeCode(0);
            if (parts.Length != 4)
                throw new InvalidOperationException();
            // 00:00:00:00
            return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), FramesToMillisecondsMax999(int.Parse(parts[3])));
        }

        protected TimeCode DecodeTimeCodeFrames(string part, char[] splitChars)
        {
            return DecodeTimeCodeFramesFourParts(part.Split(splitChars, StringSplitOptions.RemoveEmptyEntries));
        }

    }
}
