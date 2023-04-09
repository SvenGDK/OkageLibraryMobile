using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace OkageSender;

public partial class ELFLibrary : ContentPage
{

    readonly uint Magic = 0xEA6E;
    long TotalBytes;

    ObservableCollection<ELFFile> ELFCollection = new();
    BackgroundWorker SenderWorker = new();

    public ELFLibrary()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //Add event handlers
        SenderWorker.DoWork += SenderWorker_DoWork;
        SenderWorker.RunWorkerCompleted += SenderWorker_RunWorkerCompleted;
    }

    private void ELFListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {

    }

    public static List<string> GetELFs()
    {
        List<string> ELFList = new();

        foreach (string ELF in Directory.GetFiles("/storage/emulated/0/Download", "*.elf"))
        {
            ELFList.Add(ELF);
        }

        return ELFList;
    }

    public static string GetReadableSizeString(long Value)
    {
        double DoubleBytes;
        DoubleBytes = Convert.ToDouble(Value / 1048576);
        string FormattedString = $"{DoubleBytes:0.00}";
        return FormattedString;
    }

    public async Task<FileResult> PickAndShow(PickOptions options)
    {
        var SelectedELF = await FilePicker.Default.PickAsync(options);
        if (SelectedELF != null)
        {
            if (SelectedELF.FileName.EndsWith(".elf", StringComparison.OrdinalIgnoreCase))
            {
                return SelectedELF;
            }
            else
            {
                return null;
            }
        }
        else { return null; }
    }

    private void LoadELFFilesButton_Clicked(object sender, EventArgs e)
    {
        // Clear previous ELFs
        ELFListView.ItemsSource = null;
        ELFCollection.Clear();

        //Create a new ELF list
        List<string> ELFsFromDownloadsFolder = GetELFs();

        string HomebrewName;
        string HomebrewConsole;
        string HomebrewFirmware;

        foreach (string ELF in ELFsFromDownloadsFolder)
        {
            FileInfo FoundELFFile = new(ELF);
            // Set ELF infos
            if (ELF.Contains("PS4"))
            {
                HomebrewConsole = "PS4";
                string[] SplittedELFArray = FoundELFFile.Name.Split(new string[] { "-PS4-" }, StringSplitOptions.None);
                HomebrewName = SplittedELFArray[0].Replace($"-", $" ");
                HomebrewFirmware = SplittedELFArray[1].Replace($"-", $".").Replace(".elf", "");
            }
            else if (ELF.Contains("PS5"))
            {
                HomebrewConsole = "PS5";
                string[] SplittedELFArray = FoundELFFile.Name.Split(new string[] { "-PS5-" }, StringSplitOptions.None);
                HomebrewName = SplittedELFArray[0].Replace($"-", $" ");
                HomebrewFirmware = SplittedELFArray[1].Replace($"-", $".").Replace(".elf","");
            }
            else
            {
                HomebrewConsole = "Unknown";
                HomebrewName = "Unknown";
                HomebrewFirmware = "Unknown";
            }

            // Create a new ELFFile
            ELFFile NewELFFile = new() { FileName = HomebrewName, Console = HomebrewConsole, Firmware = HomebrewFirmware, FilePath = ELF };

            // Add to the GamesListView's ObservableCollection
            ELFCollection.Add(NewELFFile);
        }

        // Set the ItemsSource
        ELFListView.ItemsSource = ELFCollection;

        if (ELFCollection.Count > 0)
        {
            SendELFButton.IsEnabled = true;
        }
    }

    private void SendELFButton_Clicked(object sender, EventArgs e)
    {
        // Check first if an ELF is selected
        if (ELFListView.SelectedItem != null)
        {
            // Check if an IP address has been entered before
            if (MainPage.CurrentConsoleIP != null)
            {
                ELFFile SelectedELF = ELFListView.SelectedItem as ELFFile;
                IPAddress DeviceIP = IPAddress.Parse(MainPage.CurrentConsoleIP);
                FileInfo ELFFileInfo = new(SelectedELF.FilePath);
                TotalBytes = ELFFileInfo.Length;

                // Set control properties
                SendProgresLabel.IsVisible = true;
                LoadELFFilesButton.IsEnabled = false;
                SendELFButton.IsEnabled = false;

                // Create new backgroundwoker args and start sending
                WorkerArgs WorkArgs = new() { DeviceIP = DeviceIP, FileToSend = SelectedELF.FilePath, ChunkSize = 4096 };
                SenderWorker.RunWorkerAsync(WorkArgs);
            }

        }
    }

    private void SenderWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        WorkerArgs CurrentWorkerArgs = e.Argument as WorkerArgs;

        FileInfo FileInfos = new(CurrentWorkerArgs.FileToSend);
        ulong FileSizeAsULong = (ulong)FileInfos.Length;

        byte[] MagicBytes = BytesConverter.ToLittleEndian(Magic);
        byte[] NewFileSizeBytes = BytesConverter.ToLittleEndian(FileSizeAsULong);
        int ChunkSize = CurrentWorkerArgs.ChunkSize;

        using Socket SenderSocket = new(SocketType.Stream, ProtocolType.Tcp) { ReceiveTimeout = 3000 };
        {

            //Connect to the console
            SenderSocket.Connect(CurrentWorkerArgs.DeviceIP, 9045);
            //Send the magic
            SenderSocket.Send(MagicBytes);
            //Send the file size
            SenderSocket.Send(NewFileSizeBytes);

            int BytesRead;
            long SendBytes = 0;
            byte[] Buffer = new byte[ChunkSize];

            using FileStream SenderFileStream = new(CurrentWorkerArgs.FileToSend, FileMode.Open, FileAccess.Read);
            {

                do
                {
                    BytesRead = SenderFileStream.Read(Buffer, 0, Buffer.Length);

                    if (BytesRead > 0)
                    {
                        SendBytes += SenderSocket.Send(Buffer, 0, BytesRead, SocketFlags.None);

                        // Update status label
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            SendProgresLabel.Text = GetReadableSizeString(SendBytes) + "MB of " + GetReadableSizeString(TotalBytes) + "MB sent.";
                        });

                    }

                } while (BytesRead > 0);

            }

            SenderSocket.Close();

        }
    }

    private async void SenderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        await DisplayAlert("ELF sent.", "Selected ELF has been sent.", "OK");

        SendELFButton.IsEnabled = true;
        LoadELFFilesButton.IsEnabled = true;
        SendProgresLabel.IsVisible = false;
    }

    private async void LoadSingleELFButton_Clicked(object sender, EventArgs e)
    {
        // Open a new FilePicker and send the selected .elf file
        FileResult SelectedELF = await PickAndShow(PickOptions.Default);
        if (SelectedELF != null)
        {
            IPAddress DeviceIP = IPAddress.Parse(MainPage.CurrentConsoleIP);
            WorkerArgs WorkArgs = new() { DeviceIP = DeviceIP, FileToSend = SelectedELF.FullPath, ChunkSize = 4096 };
            SenderWorker.RunWorkerAsync(WorkArgs);
        }
    }
}