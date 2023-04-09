# OkageLibrary Mobile
A mobile version of OkageLibrary. </br>
(Currently only for Android)

## Features of v1.0
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
