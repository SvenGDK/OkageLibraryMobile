namespace OkageSender;

public partial class MainPage : ContentPage
{

	public static string CurrentConsoleIP;

	public MainPage()
	{
		InitializeComponent();
	}

	private void OpenLibrary(object sender, EventArgs e)
	{
		//await Navigation.PushAsync(new GamesLibrary());
		((AppShell)Application.Current.MainPage).SwitchToTab(1);
    }

    private void OpenSender(object sender, EventArgs e)
    {
        //await Navigation.PushAsync(new GamesLibrary());
        ((AppShell)Application.Current.MainPage).SwitchToTab(2);
    }

    private void IPTextBox_Completed(object sender, EventArgs e)
    {
        // Set the shared console IP string after the IP address has been entered
        if (IPTextBox.Text != null)
        {
            CurrentConsoleIP = IPTextBox.Text;
        }
    }

    protected override async void OnAppearing()
    {
        await CheckAndRequestStorageReadPermission();
    }

    public async Task<PermissionStatus> CheckAndRequestStorageReadPermission()
    {
        PermissionStatus PermStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

        if (PermStatus == PermissionStatus.Granted)
            return PermStatus;

        PermStatus = await Permissions.RequestAsync<Permissions.StorageRead>();

        return PermStatus;
    }

}

