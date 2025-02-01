﻿#nullable enable
namespace youtube_dl_gui;
using System.Windows.Forms;
using murrty.controls;
internal sealed class ExtendedConversionDetails(string InputFile) : MediaDetails(InputFile) {
    /// <summary>
    /// The input file that will be the source.
    /// </summary>
    public string InputFilePath => base.Source;
    /// <summary>
    /// The output file that will be converted to.
    /// </summary>
    public string? OutputFilePath { get; set; }

    /// <summary>
    /// Gets whether the info for this conversion instance was retrieved.
    /// </summary>
    public bool InfoRetrieved { get; private set; }
    /// <summary>
    /// Gets whether the substreams (subtitles, attachments, data) for this instance have to be extracted instead of included.
    /// <para/>
    /// Only MKV and WebM allow substreams to be embedded.
    /// </summary>
    public bool ExtractSubstreams { get; private set; }

    /// <summary>
    /// The custom arguments that the conversion will use instead of auto-generation.
    /// <para/>
    /// If this is not <see langword="NULL"/>, empty, or whitespace, it will override generated arguments.
    /// </summary>
    public string? CustomArguments { get; set; }
    /// <summary>
    /// The partial-custom arguments that will be included with the generated arguments.
    /// <para/>
    /// This will not be concated with <see cref="CustomArguments"/> if they are used.
    /// </summary>
    public string? PartialCustomArguments { get; set; }

    /// <summary>
    /// Gets the ffprobe data instance that contains ffprobe data for this instance.
    /// </summary>
    public FfprobeData? ProbeData { get; set; }
    /// <summary>
    /// Gets the QueueItem for a batch conversion relating to this instance.
    /// </summary>
    public ListViewItem? QueueItem { get; set; }

    /// <summary>
    /// Gets a list of known video <see cref="FfprobeSubdata.Stream"/> objects.
    /// </summary>
    public List<FfprobeSubdata.Stream> VideoStreams { get; } = [];
    /// <summary>
    /// Gets a list of list view items for known video streams.
    /// </summary>
    public List<ListViewItem> VideoItems { get; } = [];

    /// <summary>
    /// Gets a list of known audio <see cref="FfprobeSubdata.Stream"/> objects.
    /// </summary>
    public List<FfprobeSubdata.Stream> AudioStreams { get; } = [];
    /// <summary>
    /// Gets a list of list view items for known audio streams.
    /// </summary>
    public List<ListViewItem> AudioItems { get; } = [];

    /// <summary>
    /// Gets a list of known subtitle <see cref="FfprobeSubdata.Stream"/> objects.
    /// </summary>
    public List<FfprobeSubdata.Stream> Subtitles { get; } = [];
    /// <summary>
    /// Gets a list of list view items for known subtitles.
    /// </summary>
    public List<ListViewItem> SubtitleItems { get; } = [];

    /// <summary>
    /// Gets a list of known attachment <see cref="FfprobeSubdata.Stream"/> objects.
    /// </summary>
    public List<FfprobeSubdata.Stream> Attachments { get; } = [];
    /// <summary>
    /// Gets a list of list view items for known attachments.
    /// </summary>
    public List<ListViewItem> AttachmentItems { get; } = [];

    /// <summary>
    /// Gets a list of known data file <see cref="FfprobeSubdata.Stream"/> objects.
    /// </summary>
    public List<FfprobeSubdata.Stream> DataFiles { get; } = [];
    /// <summary>
    /// Gets a list of list view items for known data files.
    /// </summary>
    public List<ListViewItem> DataFileItems { get; } = [];

    /// <summary>
    /// Gets a list of enabled video streams for this conversion instance.
    /// </summary>
    public List<int> EnabledVideoStreams { get; } = [];
    /// <summary>
    /// Gets a list of enabled audio streams for this conversion instance.
    /// </summary>
    public List<int> EnabledAudioStreams { get; } = [];
    /// <summary>
    /// Gets a list of enabled subtitles for this conversion instance.
    /// </summary>
    public List<int> EnabledSubtitles { get; } = [];
    /// <summary>
    /// Gets a list of enabled attachments for this conversion instance.
    /// </summary>
    public List<int> EnabledAttachments { get; } = [];
    /// <summary>
    /// Gets a list of enabled data files for this conversion instance.
    /// </summary>
    public List<int> EnabledDataFiles { get; } = [];
    /// <summary>
    /// Gets a list of all enabled streams for this conversion instance.
    /// </summary>
    public List<int> TotalEnabledStreams { get; } = [];

    /// <summary>
    /// Removes any non-critical metadata information from the output.
    /// </summary>
    public bool RemoveMetadata { get; set; }
    /// <summary>
    /// Does not embed ffmpeg-related metadata to the output.
    /// </summary>
    public bool HideFfmpegMetadata { get; set; }
    /// <summary>
    /// Copies the codec from the input to the output.
    /// <para/>
    /// May not allow conversion if the codec cannot be stored in the container.
    /// </summary>
    public bool CopyCodecs { get; set; }
    /// <summary>
    /// Specifies a starting time to start the conversion from. Anything before this time will not be included in the output.
    /// <para/>
    /// If <see cref="EndTime"/> is not specified it will only skip the parts of the media before <see cref="StartTime"/>.
    /// </summary>
    public Time StartTime { get; set; } = Time.Empty;
    /// <summary>
    /// Specifies a ending time to end the conversion at. Anything after this time will not be included in the output.
    /// <para/>
    /// If <see cref="StartTime"/> is not specified, it will only skip the parts of the media after <see cref="EndTime"/>.
    /// </summary>
    public Time EndTime { get; set; } = Time.Empty;
    /// <summary>
    /// Determines whether to add Video or Audio arguments.
    /// </summary>
    public ConversionType Type { get; set; } = ConversionType.Unspecified;

    /// <summary>
    /// The bitrate (in KBs) that the video streams will be encoded to.
    /// <para/>
    /// If <see cref="CopyCodecs"/> is true, it will re-encode to the new bitrate.
    /// </summary>
    public decimal VideoBitrate { get; set; }
    /// <summary>
    /// "The preset that will be used during video encoding.
    /// </summary>
    public VideoPresets VideoPreset { get; set; } = VideoPresets.none;
    /// <summary>
    /// The profile that will be used during video encoding.
    /// </summary>
    public VideoProfiles VideoProfile { get; set; } = VideoProfiles.none;
    /// <summary>
    /// Whether to use Constant Rate Factor, which is used for higher qualities with less regard for file size.
    /// <para/>
    /// If <see cref="CopyCodecs"/> is true, it will re-encode for the new CRF value.
    /// </summary>
    public bool VideoUseCRF { get; set; }
    /// <summary>
    /// The Constant Rate Factor to use for encoding.
    /// </summary>
    public int VideoCRF { get; set; }
    /// <summary>
    /// Moves the MOOV atom to the start of the video.
    /// </summary>
    public bool VideoFastStart { get; set; }

    /// <summary>
    /// The bitrate (in KBs) that the audio streams will be encoded to.
    /// <para/>
    /// If <see cref="CopyCodecs"/> is true, it will re-encode to the new bitrate.
    /// </summary>
    public decimal AudioBitrate { get; set; }
    /// <summary>
    /// Whether to use Variable Bitrate (as apposed to Constant Bitrate), which is a more storage-saving encoding method that adjusts the bitrate over time.
    /// </summary>
    public bool AudioUseVBR { get; set; }
    /// <summary>
    /// The Variable Bitrate value to use during encoding, with a valid range between 0-10.
    /// <para/>
    /// 0 is conisdered the best quality, while 10 is considered the worst.
    /// </summary>
    public int AudioVBR { get; set; }
    /// <summary>
    /// The sample rate that will be encoded into audio streams.
    /// <para/>
    /// If <see cref="CopyCodecs"/> is true, it will re-encode for the new sample rate.
    /// </summary>
    public AudioSampleRates AudioSampleRate { get; set; } = AudioSampleRates.none;

    private bool ShouldExtractSubstreams() {
        return OutputFilePath!.ToLowerInvariant()[OutputFilePath.LastIndexOf('.')..] switch {
            ".webm" => false,
            ".mkv" => false,
            _ => true,
        };
    }

    protected override void Parse() {
        if (InputFilePath.IsNullEmptyWhitespace()) {
            throw new ArgumentNullException(nameof(InputFilePath));
        }

        ProbeData = FfprobeData.GenerateData(InputFilePath, out _);
        if (ProbeData is null || ProbeData.MediaStreams is null || ProbeData.MediaStreams.Length < 1) {
            throw new ArgumentException(nameof(InputFilePath));
        }

        FfprobeSubdata.Stream Stream;
        StringBuilder Display = new();
        string[] Framerate;
        for (int i = 0; i < ProbeData.MediaStreams.Length; i++) {
            Stream = ProbeData.MediaStreams[i];

            if (Stream.codec_type.IsNullEmptyWhitespace()) {
                continue;
            }

            switch (Stream.codec_type.ToLowerInvariant()) {
                case "video": {
                    VideoStreams.Add(Stream);
                    ListViewItem NewStream = new((i + 1).ToString()) {
                        Tag = Stream,
                        Checked = true,
                    };
                    NewStream.SubItems.Add(Stream.tags is null ? Stream.codec_name : Stream.tags.title ?? Stream.codec_name ?? "-");
                    NewStream.SubItems.Add(Stream.codec_long_name ?? "-");
                    Display.Clear();

                    if (Stream.width > 0) {
                        Display.Append(Stream.width);
                    }
                    else if (Stream.coded_width > 0) {
                        Display.Append(Stream.coded_width);
                    }
                    else {
                        Display.Append('?');
                    }

                    Display.Append('x');

                    if (Stream.height > 0) {
                        Display.Append(Stream.height);
                    }
                    else if (Stream.coded_height > 0) {
                        Display.Append(Stream.coded_height);
                    }
                    else {
                        Display.Append('?');
                    }

                    if (!Stream.display_aspect_ratio.IsNullEmptyWhitespace()) {
                        if (Display.Length > 0) {
                            Display.Append(" / ");
                        }
                        Display.Append(Stream.display_aspect_ratio);
                    }

                    if (!Stream.avg_frame_rate.IsNullEmptyWhitespace()
                    && (Framerate = Stream.avg_frame_rate.Split('/')).Length >= 2
                    && int.TryParse(Framerate[0], out int A) && int.TryParse(Framerate[1], out int B)) {
                        if (Display.Length > 0) {
                            Display.Append(" / ");
                        }
                        Display.Append((decimal)A / B).Append(" fps");
                    }
                    else if (!Stream.r_frame_rate.IsNullEmptyWhitespace()
                    && (Framerate = Stream.r_frame_rate.Split('/')).Length >= 2
                    && int.TryParse(Framerate[0], out A) && int.TryParse(Framerate[1], out B)) {
                        if (Display.Length > 0) {
                            Display.Append(" / ");
                        }
                        Display.Append((decimal)A / B).Append(" fps");
                    }

                    NewStream.SubItems.Add(Display.Length > 0 ? Display.ToString() : "-");
                    VideoItems.Add(NewStream);
                } break;

                case "audio": {
                    AudioStreams.Add(Stream);
                    ListViewItem NewStream = new((i + 1).ToString()) {
                        Tag = Stream,
                        Checked = true,
                    };
                    NewStream.SubItems.Add(Stream.tags is null ? Stream.codec_name : Stream.tags.title ?? Stream.codec_name ?? "-");
                    NewStream.SubItems.Add(Stream.codec_long_name ?? "-");
                    NewStream.SubItems.Add(Stream.bit_rate);
                    NewStream.SubItems.Add(Stream.sample_rate);
                    AudioItems.Add(NewStream);
                } break;

                case "subtitle":
                case "subtitles":
                case "text": {
                    Subtitles.Add(Stream);
                    ListViewItem NewStream = new((i + 1).ToString()) {
                        Tag = Stream,
                        Checked = true,
                    };
                    if (Stream.tags is null) {
                        NewStream.SubItems.Add(Stream.codec_name);
                        NewStream.SubItems.Add("-");
                    }
                    else {
                        NewStream.SubItems.Add(Stream.tags.title ?? Stream.codec_name ?? "-");
                        NewStream.SubItems.Add(Stream.tags.language ?? "-");
                    }
                    NewStream.SubItems.Add(Stream.codec_long_name);
                    SubtitleItems.Add(NewStream);
                } break;

                case "attachments":
                case "attachment": {
                    Attachments.Add(Stream);
                    ListViewItem NewStream = new((i + 1).ToString()) {
                        Tag = Stream,
                        Checked = true,
                    };
                    NewStream.SubItems.Add(Stream.tags is null ? Stream.codec_name : Stream.tags.filename ?? Stream.codec_name ?? "-");
                    NewStream.SubItems.Add(Stream.codec_long_name ?? "-");
                    AttachmentItems.Add(NewStream);
                } break;

                case "data": {
                    DataFiles.Add(Stream);
                    ListViewItem NewStream = new((i + 1).ToString()) {
                        Tag = Stream,
                        Checked = true,
                    };
                    NewStream.SubItems.Add(Stream.tags is null ? Stream.codec_name : Stream.tags.title ?? Stream.codec_name ?? "-");
                    DataFileItems.Add(NewStream);
                } break;
            }
        }

        InfoRetrieved = true;
    }
    public override bool GenerateArguments() {
        if (this.OutputFilePath.IsNullEmptyWhitespace()) {
            throw new ArgumentException("Please specify an output file path before generating arguments.");
        }

        if (!this.InfoRetrieved) {
            throw new ArgumentException(nameof(FfprobeData));
        }

        ArgumentList Args = new($"-i \"{this.InputFilePath}\"");

        bool VideoStreams = false;
        bool AudioStreams = false;

        // TODO: Implement subtitle extraction instead of mapping into output if output is MP4.

        #region Stream removal
        this.EnabledVideoStreams.Clear();
        this.EnabledAudioStreams.Clear();
        this.EnabledSubtitles.Clear();
        this.EnabledSubtitles.Clear();
        this.EnabledDataFiles.Clear();
        this.TotalEnabledStreams.Clear();

        if (this.VideoItems.Count > 0) {
            for (int i = 0; i < this.VideoItems.Count; i++) {
                if (this.VideoItems[i].Checked && this.VideoItems[i].Tag is FfprobeSubdata.Stream stream) {
                    this.EnabledVideoStreams.Add(stream.index);
                    this.TotalEnabledStreams.Add(stream.index);
                }
            }
            VideoStreams = EnabledVideoStreams.Count > 0;
        }
        if (this.AudioItems.Count > 0) {
            for (int i = 0; i < this.AudioItems.Count; i++) {
                if (this.AudioItems[i].Checked && this.AudioItems[i].Tag is FfprobeSubdata.Stream stream) {
                    this.EnabledAudioStreams.Add(stream.index);
                    this.TotalEnabledStreams.Add(stream.index);
                }
            }
            AudioStreams = EnabledAudioStreams.Count > 0;
        }

        if (this.SubtitleItems.Count > 0) {
            for (int i = 0; i < this.SubtitleItems.Count; i++) {
                if (this.SubtitleItems[i].Checked && this.SubtitleItems[i].Tag is FfprobeSubdata.Stream stream) {
                    this.EnabledSubtitles.Add(stream.index);
                    this.TotalEnabledStreams.Add(stream.index);
                }
            }
        }
        if (this.AttachmentItems.Count > 0) {
            for (int i = 0; i < this.AttachmentItems.Count; i++) {
                if (this.AttachmentItems[i].Checked && this.AttachmentItems[i].Tag is FfprobeSubdata.Stream stream) {
                    EnabledAttachments.Add(stream.index);
                    TotalEnabledStreams.Add(stream.index);
                }
            }
        }
        if (this.DataFileItems.Count > 0) {
            for (int i = 0; i < this.DataFileItems.Count; i++) {
                if (this.DataFileItems[i].Checked && this.DataFileItems[i].Tag is FfprobeSubdata.Stream stream) {
                    this.EnabledDataFiles.Add(stream.index);
                    this.TotalEnabledStreams.Add(stream.index);
                }
            }
        }

        this.ExtractSubstreams = ShouldExtractSubstreams()
            && this.EnabledSubtitles.Count > 0
            && this.EnabledAttachments.Count > 0
            && this.EnabledDataFiles.Count > 0;

        if (this.EnabledVideoStreams.Count < this.VideoStreams.Count
        || this.EnabledAudioStreams.Count < this.AudioStreams.Count
        || this.EnabledSubtitles.Count < this.Subtitles.Count
        || this.EnabledAttachments.Count < this.Attachments.Count
        || this.EnabledDataFiles.Count < this.DataFiles.Count) {
            if (TotalEnabledStreams.Count < 1) {
                throw new InvalidProgramException($"Not enough streams enabled in {nameof(this.TotalEnabledStreams)}.");
            }

            if (this.EnabledVideoStreams.Count > 0) {
                this.EnabledVideoStreams.For((Stream) => Args.Add($"-map 0:{Stream}"));
            }
            if (this.EnabledAudioStreams.Count > 0) {
                this.EnabledAudioStreams.For((Stream) => Args.Add($"-map 0:{Stream}"));
            }

            if (!this.ExtractSubstreams) {
                if (this.EnabledVideoStreams.Count < 1 && this.EnabledAudioStreams.Count < 1) {
                    throw ExtractOnlyException.Default;
                }

                if (this.EnabledSubtitles.Count > 0) {
                    this.EnabledSubtitles.For((Stream) => Args.Add($"-map 0:{Stream}"));
                }
                if (this.EnabledAttachments.Count > 0) {
                    this.EnabledAttachments.For((Stream) => Args.Add($"-map 0:{Stream}"));
                }
                if (this.EnabledDataFiles.Count > 0) {
                    this.EnabledDataFiles.For((Stream) => Args.Add($"-map 0:{Stream}"));
                }
            }
        }
        #endregion

        #region Video options
        if (VideoStreams) {
            if (this.VideoUseCRF) {
                Args.Add("-crf " + this.VideoCRF);
            }
            else if (this.VideoBitrate > 0) {
                Args.Add($"-b:v {this.VideoBitrate}k");
            }

            if (this.VideoPreset != VideoPresets.none) {
                Args.Add("-preset " + this.VideoPreset.ToString());
            }

            if (this.VideoProfile != VideoProfiles.none && !this.OutputFilePath.EndsWith(".wmv", StringComparison.InvariantCultureIgnoreCase)) {
                Args.Add("-profile:v " + this.VideoProfile.ToString());
            }

            // TODO: MP4 + MOV?
            if (this.VideoFastStart) {
                Args.Add("-movflags +faststart"); // or `-faststart` ?
            }
        }
        #endregion

        #region Audio options
        if (AudioStreams) {
            if (this.AudioUseVBR) {
                Args.Add($"-q:a {this.AudioVBR}");
            }
            else if (this.AudioBitrate > 0) {
                Args.Add($"-b:a {this.AudioBitrate * 1000}");
            }

            if (this.AudioSampleRate != AudioSampleRates.none) {
                Args.Add(this.AudioSampleRate switch {
                    AudioSampleRates.hz8000 =>  "-ar 8000",
                    AudioSampleRates.hz11025 => "-ar 11025",
                    AudioSampleRates.hz22050 => "-ar 22050",
                    AudioSampleRates.hz32000 => "-ar 32000",
                    AudioSampleRates.hz44100 => "-ar 44100",
                    AudioSampleRates.hz48000 => "-ar 48000",
                    _ => ""
                });
            }
        }
        #endregion

        #region Misc options
        if (this.StartTime.HasValue) {
            Args.Add($"-ss {this.StartTime}");

            if (this.EndTime.HasValue) {
                Args.Add($"-to {this.EndTime}");
            }
        }
        else if (this.EndTime.HasValue) {
            Args.Add($"-ss 0:00 -to {this.EndTime}");
        }

        if (this.CopyCodecs) {
            Args.Add("-c copy");
        }

        if (this.RemoveMetadata) {
            Args.Add("-map_metadata -1");
        }

        if (this.HideFfmpegMetadata) {
            Args.Add("-hide_banner -fflags +bitexact -flags:v +bitexact -flags:a +bitexact");
        }
        #endregion

        if (!this.PartialCustomArguments.IsNullEmptyWhitespace()) {
            Args.Add(this.PartialCustomArguments.TrimEnd().Replace("\n", " "));
        }

        Args.Add('\"' + this.OutputFilePath + '\"');
        this.Arguments = Args.ToString();
        return true;
    }
    public bool GenerateArgumentsTest() {
        if (InputFilePath.IsNullEmptyWhitespace() || OutputFilePath.IsNullEmptyWhitespace()) {
            return false;
        }

        bool CopyCodecs = this.CopyCodecs && InputFilePath[InputFilePath.LastIndexOf('.')..].Equals(
            OutputFilePath[OutputFilePath.LastIndexOf('.')..], StringComparison.InvariantCultureIgnoreCase);

        ArgumentList Args = new($"-i \"{InputFilePath}\"");
        List<int> DisabledVideoStreams = [];
        List<int> DisabledAudioStreams = [];
        List<int> DisabledSubtitles = [];
        List<int> DisabledAttachments = [];
        List<int> DisabledData = [];

        #region Options
        if (StartTime.HasValue) {
            Args.Add($"-ss {StartTime}");

            if (EndTime.HasValue) {
                Args.Add($"-to {EndTime}");
            }
        }
        else if (EndTime.HasValue) {
            Args.Add($"-ss 0:00 -to {EndTime}");
        }

        Args.Add("-map 0");

        if (CopyCodecs) {
            Args.Add("-c copy");

            if (VideoItems.Count > 0) {
                for (int i = 0; i < VideoItems.Count; i++) {
                    if (!VideoItems[i].Checked) {
                        DisabledVideoStreams.Add(((FfprobeSubdata.Stream)VideoItems[i].Tag).index);
                    }
                }

                if (DisabledVideoStreams.Count > 0) {
                    if (DisabledVideoStreams.Count == VideoItems.Count) {
                        Args.Add("-map -0:v");
                    }
                    else {
                        DisabledVideoStreams.For((Stream) => Args.Add($"-map -0:v:{Stream}"));
                    }
                }
            }

            if (AudioItems.Count > 0) {
                for (int i = 0; i < AudioItems.Count; i++) {
                    if (!AudioItems[i].Checked) {
                        DisabledAudioStreams.Add(((FfprobeSubdata.Stream)AudioItems[i].Tag).index);
                    }
                }

                if (DisabledAudioStreams.Count > 0) {
                    if (DisabledAudioStreams.Count == AudioItems.Count) {
                        Args.Add("-map -0:a");
                    }
                    else {
                        DisabledAudioStreams.For((Stream) => Args.Add($"-map -0:a:{Stream}"));
                    }
                }
            }

            if (SubtitleItems.Count > 0) {
                for (int i = 0; i < SubtitleItems.Count; i++) {
                    if (!SubtitleItems[i].Checked) {
                        DisabledSubtitles.Add(((FfprobeSubdata.Stream)SubtitleItems[i].Tag).index);
                    }
                }

                if (DisabledSubtitles.Count > 0) {
                    if (DisabledSubtitles.Count == SubtitleItems.Count) {
                        Args.Add("-map -0:a");
                    }
                    else {
                        DisabledSubtitles.For((Stream) => Args.Add($"-map -0:s:{Stream}"));
                    }
                }
            }

            if (AttachmentItems.Count > 0) {
                for (int i = 0; i < AttachmentItems.Count; i++) {
                    if (!AttachmentItems[i].Checked) {
                        DisabledAttachments.Add(((FfprobeSubdata.Stream)AttachmentItems[i].Tag).index);
                    }
                }

                if (DisabledAttachments.Count > 0) {
                    if (DisabledAttachments.Count == AttachmentItems.Count) {
                        Args.Add("-map -0:a");
                    }
                    else {
                        DisabledAttachments.For((Stream) => Args.Add($"-map -0:t:{Stream}"));
                    }
                }
            }

            if (DataFileItems.Count > 0) {
                for (int i = 0; i < DataFileItems.Count; i++) {
                    if (!DataFileItems[i].Checked) {
                        DisabledData.Add(((FfprobeSubdata.Stream)DataFileItems[i].Tag).index);
                    }
                }

                if (DisabledData.Count > 0) {
                    if (DisabledData.Count == DataFileItems.Count) {
                        Args.Add("-map -0:a");
                    }
                    else {
                        DisabledData.For((Stream) => Args.Add($"-map -0:d:{Stream}"));
                    }
                }
            }
        }

        if (RemoveMetadata) {
            Args.Add("-map_metadata -1");
        }

        if (HideFfmpegMetadata) {
            Args.Add("-hide_banner -fflags +bitexact -flags:v +bitexact -flags:a +bitexact");
        }
        #endregion

        Args.Add($"\"{OutputFilePath}\"");
        Arguments = Args.ToString();
        Args.Clear();
        return true;
    }

    /// <summary>
    /// Generates an array of strings that will be ran to extract files from the input file.
    /// </summary>
    /// <returns></returns>
    public string[]? GenerateExtractionArguments() {
        if (this.OutputFilePath.IsNullEmptyWhitespace()) {
            throw new ArgumentException("Please specify an output file path before generating arguments.");
        }

        if (!this.ExtractSubstreams) {
            return null;
        }

        List<string> Arguments = [];

        string OutputDir = this.OutputFilePath[..this.OutputFilePath.LastIndexOf('.')];

        this.EnabledSubtitles.For((Stream) => Arguments.Add($"-i \"{this.InputFilePath}\" -map 0:{Stream} \"{OutputDir}\\subs\\\""));

        if (Arguments.Count < 1) {
            throw new ArgumentException("No arguments available to run.");
        }

        return [.. Arguments];
    }
}