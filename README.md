# OkageLibrary Mobile
A mobile version of OkageLibrary, created with .NET MAUI in C#. </br>
Currently only for Android, an iOS version is coming later with native Swift code. </br>
This is my very first mobile app, any suggestions and/or improvements are very welcome :)

## Features of v1.1
- Send ELFs to the mast1c0re Network ELF Loader
- Send PS2 games to the mast1c0re Network Game Loader
  - Supports sending config files "GAME-ID_cli.conf" after transferring a game (requires latest Network Game Loader)
- PS2 Backup Manager
  - Shows game title and cover from [PSXDatacenter](https://psxdatacenter.com) if the game ID is supported
  
## Files location
- ISO and ELF files will be loaded from the DOWNLOAD folder on your phone (/storage/emulated/0/Download)
  
## How-to send an ELF file
- Note down your console IP
- Open "Okage: Shadow King" and load the exploited save game with the "Network ELF Loader".
- Wait until you see the message "Waiting for ELF file"
- Open "OkageLibrary Mobile" on your phone and enter your console IP on the "Home" tab
- Switch to the "ELF Library" and choose an ELF file from your "ELF Library" and select "Send selected ELF"

## How-to send a game
- Enter your console IP on the "Home" tab
- Send the "Network Game Loader" ELF to the console
- Open the "PS2 Games Library"
- Select the game on the list and select "Send selected game"
- Wait until the game has been transferred to the console
- OPTIONAL (requires latest Network Game Loader):
  - Select Yes/No on your console to send a config file
  - If you have choosen "Yes" -> Click on "Send config" and select your .conf file on your phone

## Strange sending behaviour
Sending ELFs and ISOs are working, HOWEVER the usage of "System.Net.Sockets.Socket" seems to be bugged in .NET MAUI for Android.</br>
Smaller files like the ELFs are sometimes transferred with more bytes and you end up with "Failed to download ISO / Failed on listening on port 9045" & the controller changes it's color. -> Restart the exploit and send again</br>
This problem could be device related as the Pixel 5 (emulator with A12 or A13) always succeeded while Samsung's newest A34 with Android 13 (device) only sometimes.</br>
Any suggestions or tips are very welcome.
