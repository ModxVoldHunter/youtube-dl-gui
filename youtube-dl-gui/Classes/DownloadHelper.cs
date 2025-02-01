﻿#nullable enable
namespace youtube_dl_gui;

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
public static class DownloadHelper {
    public static readonly string[] ProxyProtocols = [
        "https://",
        "http://",
        "socks4://",
        "socks5://"
    ];

    // lang=regex The prefix for the initial regex, encompasing the connection protocol.
    private const string RegexPrefix = @"^(http(s)?:\/\/)?";

    // From most important ... least important
    public static Regex[] CompiledRegex = [
        // lang=regex YouTube
        new(RegexPrefix + @"((www|m)\.)?(youtube\.com\/watch\?(.*?)?v=|(youtu\.be\/))[a-zA-Z0-9_-]{1,}", RegexOptions.Compiled),

        // lang=regex PornHub
        new(RegexPrefix + @"((www|m)\.)?pornhub\.com\/view_video\.php(\?viewkey=|.*?&viewkey=)ph[a-zA-Z0-9]{1,}", RegexOptions.Compiled),

        // lang=regex Reddit
        new(RegexPrefix + @"([a-zA-Z]{1,}\.)?reddit\.com\/r\/[a-zA-Z0-9-_]{1,}\/(comments\/)?[a-zA-Z0-9]{1,}|(i\.|v\.)?redd\.it\/[a-zA-Z0-9]{1,}", RegexOptions.Compiled),

        // lang=regex Twitter
        new(RegexPrefix + @"(t\.co\/[a-zA-Z0-9]{1,})|(((m|mobile)\.)?twitter\.com\/(i|[a-zA-Z0-9]{1,})\/status\/[0-9]{1,})", RegexOptions.Compiled),

        // lang=regex Twitch
        new(RegexPrefix + @"(((www|m)\.)?twitch\.tv\/((videos\/[0-9]{1,})|[a-zA-Z0-9_-]{1,}\/clip\/[a-zA-Z0-9_-]{1,})|clips\.twitch\.tv\/(clips\/)?[^clip_missing][a-zA-Z0-9_-]{1,})", RegexOptions.Compiled),
        //((www\.|m\.)?twitch.tv\/((videos\/[0-9]{1,})|[a-zA-Z0-9_-]{1,}\/clip\/[a-zA-Z0-9_-]{1,})|clips.twitch.tv\/(clips\/)?[a-zA-Z0-9_-]{1,})

        // lang=regex SoundCloud
        new(RegexPrefix + @"((www|m)\.)?soundcloud\.com\/[a-zA-Z0-9_-]{1,}\/[a-zA-Z0-9_-]{1,}", RegexOptions.Compiled),

        // lang=regex Imgur
        new(RegexPrefix + @"((www|m|i)\.)?imgur\.com(\/(a|gallery))?\/[a-zA-Z0-9]{1,}", RegexOptions.Compiled),

        // Base
        //new(RegexPrefix + "", RegexOptions.Compiled),
    ];

    private static readonly Regex BasicUrlRegex = new(@"([^\r\n\t\f\v]){1,}\.([^\r\n\t\f\v]){1,}", RegexOptions.Compiled);

    public static bool IsReddit(string Url) => CompiledRegex[2].IsMatch(Url);

    public static string GetUrlBase(string Url, bool OverrideSubdomain = false) {
        if (Url.StartsWith("https://")) {
            if (Url.StartsWith("https://www."))
                Url = Url[12..];
            else
                Url = Url[8..];
        }
        else if (Url.StartsWith("http://")) {
            if (Url.StartsWith("http://www."))
                Url = Url[11..];
            else
                Url = Url[7..];
        }
        else {
            if (Url.StartsWith("www."))
                Url = Url[4..];
        }

        Url = Url.Split('/')[0];

        if (!OverrideSubdomain && !Downloads.SubdomainFolderNames) {
            if (Url.IndexOf('.') != Url.LastIndexOf('.')) {
                Url = Url[(Url.IndexOf('.') + 1)..];
            }
        }

        return Url;
    }

    public static bool SupportedDownloadLink(string Url) => BasicUrlRegex.IsMatch(Url);

    public static string GetTransferData(string[] LineParts, ref float Percentage, ref string Eta) {
        if (LineParts[1].Contains('%')) {
            Percentage = float.Parse(LineParts[1][..LineParts[1].IndexOf('%')],
                System.Globalization.CultureInfo.InvariantCulture);

            if (LineParts[3] == "~") {
                Eta = LineParts[8];
                return $"{LineParts[1]} of ~{LineParts[4]} @ {LineParts[6]}";
            }
            else {
                Eta = LineParts[7];
                return $"{LineParts[1]} of {LineParts[3]} @ {LineParts[5]}";
            }
        }
        Eta = "Unknown";
        return "Could not parse line";
    }

    public static bool IsYoutubeKey([NotNullWhen(true)] string? key) {
        if (key.IsNullEmptyWhitespace()) {
            return false;
        }
        return Regex.IsMatch(key, "^[a-zA-Z0-9-_]{11}$");
    }

    public static bool IsYoutubeLink(string url) => Regex.IsMatch(url, @"(((http|https):\/\/)?(.){0,5}\.(youtube\.com|youtu\.be)\/(watch\?v=)?)?[a-zA-Z0-9-_]{11}");

    public static string? GetYoutubeVideoKey(string URL) {
        if (URL.StartsWith("http://",  StringComparison.InvariantCultureIgnoreCase)) {
            URL = URL[7..];
        }
        else if (URL.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase)) {
            URL = URL[8..];
        }

        if (URL.StartsWith("www.", StringComparison.InvariantCultureIgnoreCase)) {
            URL = URL[4..];
        }

        if (URL.StartsWith("youtube.com/watch?v=", StringComparison.InvariantCultureIgnoreCase)) {
            URL = URL[20..];
        }
        else if (URL.StartsWith("youtu.be/", StringComparison.InvariantCultureIgnoreCase)) {
            URL = URL[9..];
        }

        if (URL.Length >= 11) {
            URL = URL[..11];
        }

        return IsYoutubeKey(URL) ? URL : null;
    }
}