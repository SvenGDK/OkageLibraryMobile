namespace OkageLibraryMobile
{
    class PermissionHelper
    {
        public static async Task<PermissionStatus> StorageReadPermission()
        {
            // Check and request StorageRead permissions
            PermissionStatus StorageReadStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (StorageReadStatus == PermissionStatus.Granted)
            {
                return StorageReadStatus;
            }
            else
            {
                await Permissions.RequestAsync<Permissions.StorageRead>();
                return StorageReadStatus;
            }
        }

        public static async Task<PermissionStatus> MediaPermission()
        {
            // Check and request Media permissions
            PermissionStatus MediaStatus = await Permissions.CheckStatusAsync<Permissions.Media>();

            if (MediaStatus == PermissionStatus.Granted)
            {
                return MediaStatus;
            }
            else
            {
                await Permissions.RequestAsync<Permissions.Media>();
                return MediaStatus;
            }
        }
    }
}
