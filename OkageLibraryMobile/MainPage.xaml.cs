namespace OkageLibraryMobile;

public partial class MainPage : ContentPage
{
    public static string CurrentConsoleIP;

    public MainPage()
	{
		InitializeComponent();
	}

    private void OpenLibrary(object sender, EventArgs e)
    {
        ((AppShell)Application.Current.MainPage).SwitchToTab(1);
    }

    private void OpenSender(object sender, EventArgs e)
    {
        ((AppShell)Application.Current.MainPage).SwitchToTab(2);
    }

    private void IPTextBox_Completed(object sender, EventArgs e)
    {
        // Set the shared console IP string after the IP address has been entered
        if (IPTextBox.Text != null)
        {
            CurrentConsoleIP = IPTextBox.Text;
            // Store the entered IP in the app preferences
            Preferences.Default.Set("LastIP", IPTextBox.Text);
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Request file permissions to load ISOs and ELFs from the Downloads folder
        await PermissionHelper.StorageReadPermission();
        await PermissionHelper.MediaPermission();

        // Load the last entered IP from the app preferences
        bool IPSaved = Preferences.Default.ContainsKey("LastIP");
        if (IPSaved)
        {
            IPTextBox.Focus();
            IPTextBox.Text = Preferences.Default.Get("LastIP", "");
            IPTextBox.Unfocus();
        }
    }

}

