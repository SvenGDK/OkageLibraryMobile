using OkageSender;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;

namespace OkageLibraryMobile;

[SupportedOSPlatform("android")] // Only for Android atm
public partial class ELFLibrary : ContentPage
{

    long TotalBytes;
    readonly uint Magic = 0xEA6E;
    private BackgroundWorker ELFSenderWorker = new();
    private ObservableCollection<ELFFile> ELFCollection = new();

    public ELFLibrary()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //Add event handlers
        ELFSenderWorker.DoWork += ELFSenderWorker_DoWork;
        ELFSenderWorker.RunWorkerCompleted += ELFSenderWorker_RunWorkerCompleted;
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
        double DoubleBytes = Value;
        string FormattedString = $"{DoubleBytes:0.00}";
        return FormattedString;
    }

    public async Task<FileResult> PickAndShow()
    {
        FilePickerFileType ELFFileType = new(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/x-elf", "application/octet-stream" } },
                    { DevicePlatform.WinUI, new[] { ".elf" } },
                });
        PickOptions PickerOptions = new()
        {
            PickerTitle = "Please select an ELF file",
            FileTypes = ELFFileType,
        };

        var SelectedELF = await FilePicker.Default.PickAsync(PickerOptions);
        if (SelectedELF != null & SelectedELF.FileName.EndsWith(".elf", StringComparison.OrdinalIgnoreCase))
        {
            return SelectedELF;
        }
        else { return null; }
    }

    private void ELFListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {

    }

    private void LoadELFFilesButton_Clicked(object sender, EventArgs e)
    {
        // Clear previous ELFs
        ELFListView.ItemsSource = null;
        ELFCollection.Clear();

        // Show refresh state
        ELFListView.BeginRefresh();

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
                HomebrewFirmware = SplittedELFArray[1].Replace($"-", $".").Replace(".elf", "");
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

        // Hide refresh state
        ELFListView.EndRefresh();
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
                ELFSenderWorker.RunWorkerAsync(WorkArgs);
            }
        }
    }

    private void LoadSingleELFButton_Clicked(object sender, EventArgs e)
    {      
        SendELF();
    }

    public async void SendELF()
    {
        // Open a new FilePicker with .elf filter
        var SelectedELF = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select an ELF file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            { DevicePlatform.Android, new[] { "application/x-elf", "application/octet-stream" } }
        })
        });
        if (SelectedELF != null & MainPage.CurrentConsoleIP != "")
        {
            IPAddress DeviceIP = IPAddress.Parse(MainPage.CurrentConsoleIP);
            FileInfo ELFFileInfo = new(SelectedELF.FullPath);
            TotalBytes = ELFFileInfo.Length;

            // Set control properties
            SendProgresLabel.IsVisible = true;
            LoadELFFilesButton.IsEnabled = false;
            SendELFButton.IsEnabled = false;

            // Create new backgroundwoker args and start sending
            WorkerArgs WorkArgs = new() { DeviceIP = DeviceIP, FileToSend = SelectedELF.FullPath };

            ELFSenderWorker.RunWorkerAsync(WorkArgs);
        }
    }

    private void ELFSenderWorker_DoWork(object sender, DoWorkEventArgs e)
    {

        WorkerArgs args = e.Argument as WorkerArgs;
        FileInfo FileInfos = new(args.FileToSend);
        ulong FileSizeAsULong = (ulong)FileInfos.Length;

        byte[] MagicBytes = BytesConverter.ToLittleEndian(Magic);
        byte[] NewFileSizeBytes = BytesConverter.ToLittleEndian(FileSizeAsULong);

        using Socket SenderSocket = new(SocketType.Stream, ProtocolType.Tcp);
        {

            //Connect to the console
            SenderSocket.Connect(args.DeviceIP, 9045);
            //Send the magic
            SenderSocket.Send(MagicBytes);
            //Send the file size
            SenderSocket.Send(NewFileSizeBytes);

            using FileStream SenderFileStream = new(args.FileToSend, FileMode.Open, FileAccess.Read);
            {
                int BytesRead = 0;
                long SendBytes = 0;
                byte[] Buffer = new byte[4096];

                do
                {
                    BytesRead = SenderFileStream.Read(Buffer, 0, Buffer.Length);

                    if (BytesRead > 0)
                    {
                        SenderSocket.Send(Buffer, 0, BytesRead, SocketFlags.None); // This is somehow bugged in .NET MAUI and will send sometimes more/less bytes
                        SendBytes += 4096;

                        // Update status label
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            SendProgresLabel.Text = GetReadableSizeString(SendBytes) + " Bytes of " + GetReadableSizeString(TotalBytes) + " Bytes sent.";
                        });

                    }

                } while (BytesRead > 0);
                
            }

            SenderSocket.Close();
        }

    }

    private void ELFSenderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        DisplayAlert("ELF sent.", "Selected ELF has been sent.", "OK");

        SendELFButton.IsEnabled = true;
        LoadELFFilesButton.IsEnabled = true;
    }

}