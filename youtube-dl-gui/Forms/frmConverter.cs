﻿#nullable enable
namespace youtube_dl_gui;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
public partial class frmConverter : LocalizedProcessingForm {
    public ConvertInfo CurrentConversion { get; }

    private Thread? ConverterThread;    // The thread of the process for youtube-dl.
    private Process? ConverterProcess;  // The process of youtube-dl which we'll redirect.
    private bool AbortBatch;            // Determines if the rest of the batch downloads should be cancelled.

    public frmConverter(ConvertInfo Info) {
        InitializeComponent();
        LoadLanguage();
        this.CurrentConversion = Info;
    }
    private void frmConverter_Shown(object sender, EventArgs e) {
        BeginConversion();
    }
    private void frmConverter_FormClosing(object sender, FormClosingEventArgs e) {
        DialogResult Finish = DialogResult.None;
        switch (CurrentConversion.Status) {
            case ConversionStatus.Aborted:
                if (CurrentConversion.BatchConversion) {
                    if (AbortBatch) {
                        Finish = DialogResult.Abort;
                    }
                    else {
                        Finish = DialogResult.Ignore;
                    }
                } break;
            case ConversionStatus.Finished:
                if (ConverterProcess is null || ConverterProcess.ExitCode == 0) {
                    if (CurrentConversion.BatchConversion) {
                        Finish = DialogResult.Yes;
                    }
                }
                else if (CurrentConversion.BatchConversion) {
                    Finish = DialogResult.No;
                }
                break;
            case ConversionStatus.ProgramError:
            case ConversionStatus.FfmpegError:
                if (CurrentConversion.BatchConversion) {
                    Finish = DialogResult.No;
                }
                break;
            default:
                if (ConverterThread?.IsAlive == true) {
                    ConverterThread.Abort();
                    e.Cancel = true;
                }
                break;
        }
        if (!e.Cancel) {
            Converts.CloseAfterFinish = chkConverterCloseAfterConversion.Checked;

            this.DialogResult = Finish;
            this.Dispose();
        }
    }

    public override void LoadLanguage() {
        this.Text = Language.frmConverter + " ";
        if (CurrentConversion.BatchConversion) {
            this.WindowState = FormWindowState.Minimized;
            btnConverterAbortBatchConversions.Enabled = true;
            btnConverterAbortBatchConversions.Visible = true;
            btnConverterAbortBatchConversions.Text = Language.btnConverterAbortBatchConversions;
        }
        chkConverterCloseAfterConversion.Text = Language.chkConverterCloseAfterConversion;
        chkConverterCloseAfterConversion.Checked = Converts.CloseAfterFinish;

        switch (CurrentConversion?.Status) {
            case ConversionStatus.None:
            case ConversionStatus.Preparing:
            case ConversionStatus.Converting: {
                this.Text = Language.frmConverter + " ";

                if (CurrentConversion.BatchConversion) {
                    btnConverterCancelExit.Text = Language.GenericSkip;
                }
                else {
                    btnConverterCancelExit.Text = Language.GenericCancel;
                }
            } break;
            case ConversionStatus.Finished: {
                this.Text = Language.frmConverter + " ";
                btnConverterCancelExit.Text = Language.GenericExit;
            } break;
            case ConversionStatus.FfmpegError:
            case ConversionStatus.ProgramError: {
                this.Text = Language.frmConverterError;
                btnConverterAbortBatchConversions.Text = Language.GenericRetry;
                btnConverterCancelExit.Text = Language.GenericExit;
            } break;
        }
    }

    public void Abort() {
        switch (CurrentConversion.Status) {
            case ConversionStatus.FfmpegError:
            case ConversionStatus.ProgramError:
            case ConversionStatus.Aborted:
                btnConverterAbortBatchConversions.Visible = false;
                btnConverterAbortBatchConversions.Enabled = false;
                this.Text = Language.frmConverter + " ";
                tmrTitleActivity.Start();
                BeginConversion();
                break;
            default:
                AbortBatch = true;
                switch (CurrentConversion.Status) {
                    case ConversionStatus.Finished:
                    case ConversionStatus.Aborted:
                    case ConversionStatus.FfmpegError:
                    case ConversionStatus.ProgramError:
                        rtbConsoleOutput.AppendLine("The user requested to abort subsequent batch conversions");
                        btnConverterAbortBatchConversions.Enabled = false;
                        btnConverterAbortBatchConversions.Visible = false;
                        break;
                    default:
                        if (ConverterThread?.IsAlive == true) {
                            ConverterThread.Abort();
                        }
                        rtbConsoleOutput.AppendLine("Additionally, the batch conversion has been cancelled.");
                        CurrentConversion.Status = ConversionStatus.Aborted;
                        this.Close();
                        break;
                }
                break;
        }
    }

    private void BeginConversion() {
        if (CurrentConversion.InputFile.IsNullEmptyWhitespace()) {
            rtbConsoleOutput.AppendText("The input file is null or empty. Cannot continue converting.");
            CurrentConversion.Status = ConversionStatus.ProgramError;
            Log.Write("Conversion cannot conintue.");
            return;
        }
        if (CurrentConversion.OutputFile.IsNullEmptyWhitespace()) {
            rtbConsoleOutput.AppendText("The output file is null or empty. Cannot continue converting.");
            CurrentConversion.Status = ConversionStatus.ProgramError;
            Log.Write("Conversion cannot conintue.");
            return;
        }

        Log.Write($"Starting conversion for \"{CurrentConversion.InputFile}\" -> \"{CurrentConversion.OutputFile}\".");
        if (CurrentConversion.FullCustomArguments && CurrentConversion.CustomArguments.IsNullEmptyWhitespace()) {
            rtbConsoleOutput.AppendText("Custom arguments are required, but none were provided.");
            CurrentConversion.Status = ConversionStatus.ProgramError;
            Log.Write("Conversion cannot conintue.");
            return;
        }

        rtbConsoleOutput.AppendText("Beginning conversion, this box will output progress");
        if (CurrentConversion.BatchConversion) {
            chkConverterCloseAfterConversion.Checked = true;
        }

        CurrentConversion.Status = ConversionStatus.Preparing;

        ArgumentList ArgumentsBuffer = new(CurrentConversion.FullCustomArguments ?
            CurrentConversion.CustomArguments : "-i \"" + CurrentConversion.InputFile + "\"");

        #region ffmpeg path
        if (!Verification.FfmpegAvailable) {
            rtbConsoleOutput.AppendLine("ffmpeg has not been found.\r\nA rescan for ffmpeg was called");
            Verification.RefreshFFmpegLocation();
            if (!Verification.FfmpegAvailable) {
                CurrentConversion.Status = ConversionStatus.ProgramError;
                rtbConsoleOutput.AppendLine("still couldn't find ffmpeg.");
                Log.Write("ffmpeg could not be found.");
                return;
            }
            rtbConsoleOutput.AppendLine("Rescan finished and found, continuing");
        }
        rtbConsoleOutput.AppendLine("ffmpeg has been found and set");
        #endregion

        #region Arguments
        if (!CurrentConversion.FullCustomArguments) {
            switch (CurrentConversion.Type) {
                case ConversionType.Video:
                    if (CurrentConversion.VideoUseBitrate) {
                        ArgumentsBuffer.Add($"-b:v {CurrentConversion.VideoBitrate}k");
                    }

                    if (CurrentConversion.VideoUsePreset) {
                        ArgumentsBuffer.Add($"-preset {ConvertHelper.GetVideoPreset(CurrentConversion.VideoPreset)}");
                    }

                    if (CurrentConversion.VideoUseCRF) {
                        ArgumentsBuffer.Add($"-crf {CurrentConversion.VideoCRF}");
                    }

                    if (!CurrentConversion.OutputFile.EndsWith(".wmv") && CurrentConversion.VideoUseProfile) {
                        ArgumentsBuffer.Add($"-profile:v {ConvertHelper.GetVideoProfile(CurrentConversion.VideoProfile)}");
                    }

                    if (CurrentConversion.VideoFastStart) {
                        ArgumentsBuffer.Add("-faststart");
                    }
                    break;

                case ConversionType.Audio:
                    if (CurrentConversion.AudioUseBitrate) {
                        ArgumentsBuffer.Add($"-ab {CurrentConversion.AudioBitrate * 1000}");
                    }
                    break;

                case ConversionType.Custom:
                    rtbConsoleOutput.AppendLine("Custom conversion was specified, skipping generating arguments");
                    ArgumentsBuffer.Add(CurrentConversion.CustomArguments);
                    break;

                default:
                    rtbConsoleOutput.AppendLine("Using ffmpeg to automatically choose best settings");
                    break;
            }

            // Extra arguments not supported by the custom conversion type
            if (CurrentConversion.Type != ConversionType.Custom && CurrentConversion.HideFFmpegCompile) {
                ArgumentsBuffer.Add("-hide_banner");
            }

            ArgumentsBuffer.Add($"\"{CurrentConversion.OutputFile}\"");
            rtbConsoleOutput.AppendLine("Arguments have been generated and are readonly in the textbox.");
            CurrentConversion.Status = ConversionStatus.Converting;
        }

        txtArgumentsGenerated.Text = ArgumentsBuffer.ToString();
        #endregion

        #region Conversion thread
        Log.Write("Beginning conversion thread.");
        ConverterThread = new Thread(() => {
            try {
                ConverterProcess = new Process() {
                    StartInfo = new(Verification.FFmpegPath) {
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                        Arguments = ArgumentsBuffer.ToString()
                    },
                    EnableRaisingEvents = true
                };
                ConverterProcess.OutputDataReceived += (s, e) => {
                    if (e.Data?.Length > 0) {
                        rtbConsoleOutput.BeginInvoke(() => rtbConsoleOutput.AppendLine(e.Data));
                    }
                };
                ConverterProcess.ErrorDataReceived += (s, e) => {
                    if (e.Data?.Length > 0) {
                        rtbConsoleOutput.BeginInvoke(() => rtbConsoleOutput.AppendLine($"Error: {e.Data}"));
                    }
                };
                ConverterProcess.Exited += (s, e) => {
                    if (ConverterProcess.ExitCode == 0) {
                        CurrentConversion.Status = ConversionStatus.Finished;
                    }
                    else if (CurrentConversion.Status != ConversionStatus.Aborted) {
                        CurrentConversion.Status = ConversionStatus.FfmpegError;
                    }
                };

                if (CurrentConversion.Status != ConversionStatus.Aborted) {
                    ConverterProcess.Start();

                    ArgumentsBuffer.Clear();
                    ArgumentsBuffer = null!;

                    ConverterProcess.BeginOutputReadLine();
                    ConverterProcess.BeginErrorReadLine();

                    while (!ConverterProcess.HasExited) {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (ThreadAbortException) {
                if (ConverterProcess is not null) {
                    ConverterProcess.CancelErrorRead();
                    ConverterProcess.CancelOutputRead();
                    Program.KillProcessTree((uint)ConverterProcess.Id);
                    ConverterProcess.Kill();
                }
                this.Invoke((Action)delegate {
                    rtbConsoleOutput.AppendLine("Conversion was aborted by the user.");
                });
            }
            catch (Exception ex) {
                Log.ReportException(ex);
                CurrentConversion.Status = ConversionStatus.ProgramError;
            }
            finally {
                if ((this.IsHandleCreated && CurrentConversion.Status != ConversionStatus.Aborted) || CurrentConversion.BatchConversion) {
                    this.BeginInvoke(() => ConversionFinished());
                }
            }
        });
        rtbConsoleOutput.AppendLine("Created conversion thread, starting...");
        ConverterThread.Name = "Converting file";
        ConverterThread.Start();
        #endregion
    }

    private void ConversionFinished() {
        tmrTitleActivity.Stop();
        this.Text = this.Text.Trim('.');
        btnConverterCancelExit.Text = Language.GenericExit;

        if (CurrentConversion.BatchConversion) {
            switch (CurrentConversion.Status) {
                case ConversionStatus.Aborted:
                    if (AbortBatch)
                        this.DialogResult = DialogResult.Abort;
                    else
                        this.DialogResult = DialogResult.Ignore;
                    break;
                case ConversionStatus.FfmpegError:
                    this.Activate();
                    System.Media.SystemSounds.Hand.Play();
                    rtbConsoleOutput.AppendLine("An error occured\r\nTHIS IS A FFMPEG ERROR, NOT A ERROR WITH THIS PROGRAM!\r\nExit the form to resume batch download.");
                    this.Text = Language.frmConverterError;
                    break;
                case ConversionStatus.ProgramError:
                    rtbConsoleOutput.AppendLine("An error occured\r\nAn error log was presented, if enabled.\r\nExit the form to resume batch download.");
                    this.Text = Language.frmConverterError;
                    this.Activate();
                    System.Media.SystemSounds.Hand.Play();
                    break;
                case ConversionStatus.Finished:
                    this.DialogResult = DialogResult.Yes;
                    break;
                default:
                    this.DialogResult = DialogResult.No;
                    break;
            }
        }
        else {
            switch (CurrentConversion.Status) {
                case ConversionStatus.Aborted:
                    break;
                case ConversionStatus.FfmpegError:
                    rtbConsoleOutput.AppendLine("An error occured\r\nTHIS IS A FFMPEG ERROR, NOT A ERROR WITH THIS PROGRAM!");
                    btnConverterAbortBatchConversions.Visible = true;
                    btnConverterAbortBatchConversions.Enabled = true;
                    btnConverterAbortBatchConversions.Text = Language.GenericRetry;
                    this.Text = Language.frmConverterError;
                    this.Activate();
                    System.Media.SystemSounds.Hand.Play();
                    break;
                case ConversionStatus.ProgramError:
                    rtbConsoleOutput.AppendLine("An error occured\r\nAn error log was presented, if enabled.");
                    this.Text = Language.frmConverterError;
                    this.Activate();
                    System.Media.SystemSounds.Hand.Play();
                    break;
                case ConversionStatus.Finished:
                    this.Text = Language.frmConverterComplete;
                    btnConverterCancelExit.Text = Language.GenericExit;
                    rtbConsoleOutput.AppendLine("Conversion has finished.");

                    if (chkConverterCloseAfterConversion.Checked)
                        this.Close();
                    break;
                default:
                    this.Text = Language.frmConverterComplete;
                    btnConverterCancelExit.Text = Language.GenericExit;
                    rtbConsoleOutput.AppendLine("CurrentConversion.Status not defined (Not a batch download)\r\nAssuming success.");

                    if (chkConverterCloseAfterConversion.Checked)
                        this.Close();
                    break;
            }
        }
    }

    private void tmrTitleActivity_Tick(object sender, EventArgs e) {
        if (this.Text.EndsWith("....")) this.Text = this.Text.TrimEnd('.');
        else this.Text += ".";
    }

    private void btnConverterCancelExit_Click(object sender, EventArgs e) {
        switch (CurrentConversion.Status) {
            case ConversionStatus.Finished:
            case ConversionStatus.FfmpegError:
            case ConversionStatus.Aborted:
                this.Close();
                break;
            default:
                Log.Write("Aborting conversion finished.");
                if (ConverterThread?.IsAlive == true) {
                    ConverterThread.Abort();
                }
                break;
        }
    }

    private void btnConverterAbortBatchConversions_Click(object sender, EventArgs e) {
        Abort();
    }

    private void btnClearOutput_Click(object sender, EventArgs e) {
        rtbConsoleOutput.Clear();
    }
}
