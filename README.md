# Playstation 5 Save Mounter 1.5.0
This version is using patching so save data can be mounted without launching the game.
It solves the issue of failing to mount specific game saves, as they were already mounted on game boot.
Read the original readme below.

Supports PS5 FWs:
- 3.20 up to and including 12.70.
	- Some FW versions are missing like early ones (e.g. 3.21, 4.02). It'll try to use the available offsets if missing. This might now work, there's also a warning in the program.
- Currently can only mounts PS4 game saves, no PS5 game support at the moment.

Use [ps5debug-NG](https://github.com/OpenSourcereR-dev/ps5debug-NG) v1.2.7+  
Use this elf loader: https://github.com/ps5-payload-dev/elfldr

## Instructions (mouting existing saves)
1) Load ps5debug-NG.
2) Load FTP.
3) Open the tool.
4) Enter the IP address of your PS5 and click 'Connect'.
5) Click 'Patch'.
6) Click 'Setup' and select your username.
7) Click 'Get Games' and select your game in the combobox.
8) Click 'Search'.
9) Select the save you want to mount in the combobox.
10) Click 'Mount'.
11) Your save is now mounted and accessible from FTP in `/mnt/pfs/` or `/mnt/sandbox/{title}/savedataX` (it's the same just a different dir).
12) After copying or replacing the files, be sure to click 'Unmount'.
13) Click 'Unpatch' before launching games or disconnecting from PS5 to clean applied patches. Note: patches are now automatically removed when the program closes or disconnects.

** Warning: Don't replace files in `sce_sys` directory, it is unnecessary and will probably corrupt your save**

---


[ChendoChap's PS4 Save Mounter README](https://github.com/ChendoChap/Playstation-4-Save-Mounter/blob/master/README.md)
