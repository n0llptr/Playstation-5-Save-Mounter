# Playstation 5 Save Mounter 2.0.0
Supports all jailbreakable PS5 FWs.  
Mounts PS4 game saves as well PS5 game saves.

Use this elf loader: https://github.com/ps5-payload-dev/elfldr

## Instructions (mouting existing saves)
1) Load elfldr.
2) Load FTP.
3) Open the tool.
4) Enter the IP address of your PS5 and click 'Connect'.
5) Click 'Setup' and select your username.
6) Click 'Get Games' and select your game in the combobox.
7) Click 'Search'.
8) Select the save you want to mount in the combobox.
9) Click 'Mount'.
10) Your save is now mounted and accessible from FTP in `/mnt/pfs/`.
11) After copying or replacing the files, be sure to click 'Unmount'.

** Warning: Don't replace files in `sce_sys` directory, it is unnecessary and will probably corrupt your save**

---

# Credits:
* cow - based on his save mounting research using `sceFs*` internal PS5 functions
* earthonion - for his loading game titles code and save creation from [garlic-savemgr](https://git.etawen.dev/earthonion/garlic-savemgr)
