using DiscUtils.Iso9660;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;

namespace OkageSender;

public partial class GamesLibrary : ContentPage

{
    BackgroundWorker SenderWorker = new();
    ObservableCollection<PS2Game> PS2GamesCollection = new();
    ObservableCollection<PS2Game> PS2GamesList { get { return PS2GamesCollection; } }

    readonly string CoverJS = "document.getElementById('table2').getElementsByClassName('sectional')[1].querySelector('img').src";
    readonly string TitleJS = "document.getElementById('table4').rows[0].cells[1].innerText";
    readonly string DescriptionJS = "document.getElementById('table16').rows[0].cells[0].innerText";

    readonly uint Magic = 0xEA6E;
    long TotalBytes;
    bool SendConfig = false;

    public GamesLibrary()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Clear previous title
        TitleLabel.Text = "";

        //Add event handlers
        PSXBrowser.Navigated += PSXBrowser_Navigated;
        SenderWorker.DoWork += SenderWorker_DoWork;
        SenderWorker.RunWorkerCompleted += SenderWorker_RunWorkerCompleted;
    }

    public static string GetReadableSizeString(long Value)
    {
        double DoubleBytes;
        DoubleBytes = Convert.ToDouble(Value / 1048576);
        string FormattedString = $"{DoubleBytes:0.00}";
        return FormattedString;
    }

    private async void SenderWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {

        if (SendConfig == true)
        {
            await DisplayAlert("Config sent.", "Selected .conf file has been sent.", "OK");
        }
        else
        {
            await DisplayAlert("Game sent.", "Selected game has been sent. You can send now an additional .conf file.", "OK");
        }
        
        SendConfigButton.IsEnabled = true;
        SendGameButton.IsEnabled = true;
        LoadGamesButton.IsEnabled = true;
        SendConfig = false;
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        //Remove event handlers
        PSXBrowser.Navigated -= PSXBrowser_Navigated;
        SenderWorker.DoWork -= SenderWorker_DoWork;
        SenderWorker.RunWorkerCompleted -= SenderWorker_RunWorkerCompleted;

        //Clear ListView
        GamesListView.ItemsSource = null;
    }

    public async void PSXBrowser_Navigated(object sender, WebNavigatedEventArgs e)
    {
        // Evaluate javascripts
        string CoverSource = await PSXBrowser.EvaluateJavaScriptAsync(CoverJS);
        string GameTitleString = await PSXBrowser.EvaluateJavaScriptAsync(TitleJS);

        // Set cover image
        if (CoverSource != null)
        {
            GameCoverImage.Source = CoverSource;
        }

        // Set the game title and add it to GamesListView.SelectedItem if not set/null
        if (GameTitleString != null)
        {
            // Set the game title
            TitleLabel.Text = GameTitleString.Trim();

            if (GamesListView.SelectedItem != null)
            {              
                PS2Game SelectedGame = GamesListView.SelectedItem as PS2Game;

                // Show refresh state and set the game title if empty
                if (SelectedGame.Title == "")
                {
                    // Show refresh state
                    GamesListView.BeginRefresh();
                    // Add the title to the game
                    SelectedGame.Title = GameTitleString;
                    // Empty the ListView
                    GamesListView.ItemsSource = null;
                    // Refill the ListView
                    GamesListView.ItemsSource = PS2GamesCollection;
                    // Hide refresh state
                    GamesListView.EndRefresh();
                }
        
            }
        }

    }

    public void GamesListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        // Update the cover and game title displayed in the app
        if (e.SelectedItem != null)
        {
            PS2Game SelectedGame = GamesListView.SelectedItem as PS2Game;
            PSXBrowser.Source = "https://psxdatacenter.com/psx2/games2/" + SelectedGame.GameID + ".html";
            TitleLabel.Text = SelectedGame.GameID;
        }

    }

    public static List<string> GetGames()
    {
        List<string> GamesList = new();

        //string AndroidDownloads = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);

        foreach (string GameISO in Directory.GetFiles("/storage/emulated/0/Download", "*.iso"))
        {
            GamesList.Add(GameISO);       
        }

        return GamesList;
    }

    public static string GetGameIDFromISO(string GameISO)
    {

        string GameID = null;

        using FileStream ISOFileStream = File.OpenRead(GameISO);
        {

            CDReader ISOFileReader = new(ISOFileStream, true);

            foreach (string FileInISO in ISOFileReader.GetFiles(""))
            {
                //Search for the file that contains the game ID
                if (FileInISO.Contains("SLES_") | FileInISO.Contains("SLUS_") | FileInISO.Contains("SCES_") | FileInISO.Contains("SCUS_"))             
                {
                    GameID = FileInISO.Replace(@"\", "").Replace(".", "").Replace("_", "-").Replace(";1", "").Trim();
                }

            }
   
            if (GameID != null)
            {
                return GameID;
            }
            else
            {
                return null;
            }

        }

    }

    public static string GetGameRegionFromID(string GameID)
    {

        if (GameID.StartsWith("SLES"))
        {
            return "Europe";
        }
        else if (GameID.StartsWith("SCES"))
        {
            return "Europe";
        }
        else if (GameID.StartsWith("SLUS"))
        {
            return "USA";
        }
        else if (GameID.StartsWith("SCUS"))
        {
            return "USA";
        }
        else
        {
            return null;
        }

    }

    private void LoadGamesButton_Clicked(object sender, EventArgs e)
    {
        // Clear previous games
        GamesListView.ItemsSource = null;
        PS2GamesCollection.Clear();

        // Create a new game list
        List<string> GamesFromDownloadsFolder = GetGames();

        foreach (string game in GamesFromDownloadsFolder)
        {
            // Get game infos
            string GameID = GetGameIDFromISO(game);
            string GameRegion = GetGameRegionFromID(GameID);

            // Create a new PS2Game
            PS2Game newps2game = new() { Title = "", Region = GameRegion, GameID = GameID, FilePath = game };

            // Add to the GamesListView's ObservableCollection
            PS2GamesCollection.Add(newps2game);
        }

        // Set the ItemsSource
        GamesListView.ItemsSource = PS2GamesCollection;

        if (PS2GamesCollection.Count > 0)
        {
            SendGameButton.IsEnabled = true;
        }
    }

    private void SendGameButton_Clicked(object sender, EventArgs e)
    {
        // Check first if a game is selected
        if (GamesListView.SelectedItem != null)
        {
            // Check if an IP address has been entered before
            if (MainPage.CurrentConsoleIP != null)
            {
                PS2Game SelectedGame = GamesListView.SelectedItem as PS2Game;
                IPAddress DeviceIP = IPAddress.Parse(MainPage.CurrentConsoleIP);
                FileInfo GameFileInfo = new(SelectedGame.FilePath);
                TotalBytes = GameFileInfo.Length;

                // Set control properties
                SendProgresLabel.IsVisible = true;
                LoadGamesButton.IsEnabled = false;
                SendGameButton.IsEnabled = false;
                SendConfigButton.IsEnabled = false;

                // Create new backgroundwoker args and start sending
                WorkerArgs WorkArgs = new() { DeviceIP = DeviceIP, FileToSend = SelectedGame.FilePath, ChunkSize = 63488 };
                SenderWorker.RunWorkerAsync(WorkArgs);
            }      

        }
    }

    public async Task<FileResult> PickAndShow(PickOptions options)
    {
        var SelectedConfig = await FilePicker.Default.PickAsync(options);
        if (SelectedConfig != null)
        {
            if (SelectedConfig.FileName.EndsWith(".conf", StringComparison.OrdinalIgnoreCase))
            {
                return SelectedConfig;
            }
            else
            {
                return null;
            }
        }
        else { return null; }
    }

    private async void SendConfigButton_Clicked(object sender, EventArgs e)
    {
        // Open a new FilePicker and send the selected .conf file
        FileResult SelectedConfig = await PickAndShow(PickOptions.Default);
        if (SelectedConfig != null)
        {
            SendConfig = true;
            IPAddress DeviceIP = IPAddress.Parse(MainPage.CurrentConsoleIP);
            WorkerArgs WorkArgs = new() { DeviceIP = DeviceIP, FileToSend = SelectedConfig.FullPath, ChunkSize = 10 };
            SenderWorker.RunWorkerAsync(WorkArgs);
        }
    }
}