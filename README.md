# OkageLibrary Mobile
A mobile version of OkageLibrary. Ready soon for testing.

## Features of v1.0
- Send ELFs to the mast1c0re Network ELF Loader
- Send PS2 games to the mast1c0re Network Game Loader
  - Supports sending config files "GAME-ID_cli.conf" after transferring a game (requires latest Network Game Loader)
- PS2 Backup Manager
  - Shows game details and art from [PSXDatacenter](https://psxdatacenter.com) if the game ID is supported
  
## How-to send an ELF file
- Note down your console IP
- Open "Okage: Shadow King" and load the exploited save game with the "Network ELF Loader".
- Wait until you see the message "Waiting for ELF file"
- Open "OkageLibrary Mobile" on your phone and enter your console IP
- Select the ELF from your "ELF Library" and select "Send selected ELF"

## How-to send a game
- Send the "Network Game Loader" ELF to the console
- Open the "PS2 Games Library"
- Select the game on the list and select "Send selected game"
- Wait until the game has been transferred to the console
- OPTIONAL (requires latest Network Game Loader):
  - Select Yes/No on your console to send a config file
  - If you have choosen "Yes" -> Click on "Send config" and select your .conf file
- Enjoy (if not crashed... :P)