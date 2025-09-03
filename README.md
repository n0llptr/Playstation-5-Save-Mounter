# Playstation 5 Save Mounter 1.4.7
This version is using patching so save data can be mounted without launching the game.
It solves the issue of failing to mount specific game saves, as they were already mounted on game boot.
Read the original readme below.

Supports PS5 FWs:
- 3.20, 4.03, 5.02, 5.10, 5.50, 6.02, 7.00, 7.20, 7.40, 7.60
	- If you have another FW version which is the same major version, it'll try to use the available offsets. This might now work, there's also a warning in the program.
	- The following firmware versions are recognized but currently require a ps5debug port before they are fully supported: 8.20, 8.20.02, 9.40, 9.60, 10.01.
- Currently can only mounts PS4 game saves, no PS5 game support at the moment.

Use [ps5debug](https://github.com/GoldHEN/ps5debug) at least version v1.0b5 for 3.xx-7.xx
Use this elf loader: https://github.com/ps5-payload-dev/elfldr

## Instructions (mouting existing saves)
1) Load ps5debug.
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