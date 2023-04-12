using System;
using Newtonsoft.Json;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Structure for representing the content of a caption (format, content etc)
    /// </summary>
    ///
    public class CaptionTrackContent
    {

        [JsonProperty(PropertyName = "id")]
        public virtual string Id { get; set; } = GetShortUuid();

        [JsonProperty(PropertyName = "format")]
        public virtual string Format { get; set; }

        [JsonProperty(PropertyName = "content")]
        public virtual string Content { get; set; }
        
        public CaptionTrackContent()
        {
            Id = GetShortUuid();
            Format = WebVTT.Const.WEB_VTT_NAME;
        }

        /// <summary>
        /// Gets the short UUID. It also replace the invalid path characters with path-safe characters (/ + for example)
        /// </summary>
        /// <returns></returns>
        public static string GetShortUuid()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8).Replace("/", "_").Replace("+", "-");
        }
    }
}