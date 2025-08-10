using libdebug;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using String = System.String;

struct GameMetadata
{
    public string Name;
    public Image Image;
    public bool Installed;
    public GameMetadata()
    {
        Name = null;
        Image = null;
        Installed = false;
    }
}

namespace PS4Saves
{
    public partial class Main : Form
    {
        PS4DBG ps4 = new PS4DBG();
        private int pid;
        private ulong stub;
        private ulong libSceUserServiceBase = 0x0;
        private ulong libSceSaveDataBase = 0x0;
        private ulong executableBase = 0x0;
        private ulong libSceLibcInternalBase = 0x0;
        private ulong GetSaveDirectoriesAddr = 0;
        private ulong ReadFileAddr = 0;
        private bool isPatched = false;
        private int user = 0x0;
        private string selectedGame = null;
        Dictionary<string, GameMetadata> GamesMetadata = [];
        string mp = "";
        bool log = true;

        public Main()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2 && args[1] == "-log")
            {
                log = true;
            }

            if (File.Exists("ip"))
            {
                ipTextBox.Text = File.ReadAllText("ip");
            }
        }
        public static string FormatSize(double size)
        {
            const long BytesInKilobytes = 1024;
            const long BytesInMegabytes = BytesInKilobytes * 1024;
            const long BytesInGigabytes = BytesInMegabytes * 1024;
            double value;
            string str;
            if (size < BytesInGigabytes)
            {
                value = size / BytesInMegabytes;
                str = "MB";
            }
            else
            {
                value = size / BytesInGigabytes;
                str = "GB";
            }
            return String.Format("{0:0.##} {1}", value, str);
        }
        public void snapSize()
        {
            if (!sizeSnapCheckbox.Checked) return;

            int v = sizeTrackBar.Value;
            sizeTrackBar.Value = (int)Math.Round((double)v / 32, 2) * 32;
        }
        private void sizeSnapCheckbox_CheckStateChanged(object sender, EventArgs e)
        {
            snapSize();
            string size = FormatSize((double)(sizeTrackBar.Value * 32768));
            sizeToolTip.SetToolTip(sizeTrackBar, size);
        }

        private void sizeTrackBar_Scroll(object sender, EventArgs e)
        {
            snapSize();
            string size = FormatSize((double)(sizeTrackBar.Value * 32768));
            sizeToolTip.SetToolTip(sizeTrackBar, size);
        }
        private void SetStatus(string msg)
        {
            statusLabel.Text = $"Status: {msg}";
        }
        private void WriteLog(string msg)
        {
            if (log)
            {

                msg = $"|{msg}|";
                var a = msg.Length / 2;
                for (var i = 0; i < 48 - a; i++)
                {
                    msg = msg.Insert(0, " ");
                }

                for (var i = msg.Length; i < 96; i++)
                {
                    msg += " ";
                }

                var dateAndTime = DateTime.Now;
                var logStr = $"|{dateAndTime:MM/dd/yyyy} {dateAndTime:hh:mm:ss tt}| |{msg}|";

                if (File.Exists(@"log.txt"))
                {
                    File.AppendAllText(@"log.txt",
                        $"{logStr}{Environment.NewLine}");
                }
                else
                {
                    using (var sw = File.CreateText(@"log.txt"))
                    {
                        sw.WriteLine(logStr);
                    }
                }

                Console.WriteLine(logStr);
            }
        }
        private void matchExactFWVersion(int fwVersion)
        {
            String detectedFirmware = ((double)fwVersion / 100).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            Offsets.SelectedFirmwareLibraries = detectedFirmware;
            Offsets.SelectedFirmwareShellcore = detectedFirmware;
            label2.Text += " " + detectedFirmware;
        }
        private void matchLooseFWVersion(int fwVersion, String relatedFwVersion, bool offsetsWarning = true, bool differentShellcorePatches = false)
        {
            String detectedFirmware = ((double)fwVersion / 100).ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            Offsets.SelectedFirmwareLibraries = relatedFwVersion;

            if (differentShellcorePatches)
            {
                Offsets.SelectedFirmwareShellcore = detectedFirmware;
            }
            else
            {
                Offsets.SelectedFirmwareShellcore = relatedFwVersion;
            }

            if (offsetsWarning)
            {
                label2.Text += " " + detectedFirmware + ". Using FW " + relatedFwVersion + " offsets that might not work";
                label2.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                label2.Text += " " + detectedFirmware;
            }
        }
        private void connectButton_Click(object sender, EventArgs e)
        {
            if (connectButton.Text == "Disconnect")
            {
                bool wasPatched = isPatched;
                if (isPatched)
                {
                    unpatch();
                }

                ps4.Disconnect();
                if (ps4.IsConnected)
                {
                    SetStatus("Failed To Disconnect");
                    return;
                }

                SetStatus(wasPatched ? "Disconnected and unpatched Shellcore" : "Disconnected");
                connectButton.Text = "Connect";
                label2.Text = "Detected firmware version:";

                patchButton.Enabled = false;
                unpatchButton.Enabled = false;

                setupButton.Enabled = false;
                userComboBox.Enabled = false;

                gamesButton.Enabled = false;
                gamesComboBox.Enabled = false;

                searchButton.Enabled = false;
                dirsComboBox.Enabled = false;

                mountButton.Enabled = false;
                unmountButton.Enabled = false;

                nameTextBox.Enabled = false;
                createButton.Enabled = false;

                return;
            }

            try
            {
                ps4 = new PS4DBG(ipTextBox.Text);
                ps4.Connect();
                if (!ps4.IsConnected)
                {
                    SetStatus("Failed To Connect");
                }
                SetStatus("Connected");
                if (!File.Exists("ip"))
                {
                    File.WriteAllText("ip", ipTextBox.Text);
                }
                else
                {
                    using (var sw = File.CreateText(@"log.txt"))
                    {
                        sw.Write(ipTextBox.Text);
                    }
                }

                int version = ps4.GetExtFWVersion();
                switch(version)
                {
                    case 320:
                    case 403:
                    case 502:
                    case 602:
                    case 740:
                    case 820:
                    case 960:
                    case 1001:
                        matchExactFWVersion(version);
                        break;
                    case 550:
                        matchLooseFWVersion(version, "5.02", false, true); // same as 5.02, different shellcore patches
                        break;
                    case 700:
                    case 701:
                    case 720:
                    case 760:
                        matchLooseFWVersion(version, "7.40", false, true);  // same as 7.40, different shellcore patches
                        break;
                    case 940:
                        matchLooseFWVersion(version, "9.60", false);  // same as 9.60
                        break;
                    case >= 300 and < 400:
                        matchLooseFWVersion(version, "3.20");
                        break;
                    case >= 400 and < 500:
                        matchLooseFWVersion(version, "4.03");
                        break;
                    case >= 500 and < 600:
                        matchLooseFWVersion(version, "5.02");
                        break;
                    case >= 600 and < 700:
                        matchLooseFWVersion(version, "6.02");
                        break;
                    case >= 700 and < 800:
                        matchLooseFWVersion(version, "7.40");
                        break;
                    case >= 800 and < 900:
                        matchLooseFWVersion(version, "8.20");
                        break;
                    case >= 900 and < 1000:
                        matchLooseFWVersion(version, "9.60");
                        break;
                    case >= 1000 and < 1100:
                        matchLooseFWVersion(version, "10.01");
                        break;
                    default:
                        MessageBox.Show("Error! Unsupported firmware version detected: " + ((double)version / 100).ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "\nExiting.");
                        System.Windows.Forms.Application.Exit();
                        break;
                }

                connectButton.Text = "Disconnect";
                patchButton.Enabled = true;
                unpatchButton.Enabled = true;
            }
            catch
            {
                SetStatus("Failed To Connect");
            }
        }
        private string[] GetSaveDirectories()
        {
            var dirs = new List<string>();
            var mem = ps4.AllocateMemory(pid, 0x8000);
            var path = mem;
            var buffer = mem + 0x101;

            ps4.WriteMemory(pid, path, $"/user/home/{GetUser():x}/savedata/");
            var ret = (int)ps4.Call(pid, stub, GetSaveDirectoriesAddr, path, buffer);
            if (ret != -1 && ret != 0)
            {
                var bDirs = ps4.ReadMemory(pid, buffer, ret * 0x10);
                for (var i = 0; i < ret; i++)
                {
                    var sDir = System.Text.Encoding.UTF8.GetString(PS4DBG.SubArray(bDirs, i * 10, 9));
                    if (sDir.StartsWith("CUSA"))
                    {
                        dirs.Add(sDir);
                    }
                }
            }

            ps4.FreeMemory(pid, mem, 0x8000);
            return [.. dirs.Order()];
        }
        private Image GetGameImage(string game)
        {
            var mem_size = 0x100000; // 1MB should be enough memory for the image
            var mem = ps4.AllocateMemory(pid, mem_size);
            var path = mem;
            var buffer = mem + 0x201;

            ps4.WriteMemory(pid, path, $"/user/appmeta/{game}/icon0.png");
            var ret = (int)ps4.Call(pid, stub, ReadFileAddr, path, buffer);
            // ReadFile returns 1 when file is found
            if (ret == 1)
            {
                var image = ps4.ReadMemory(pid, buffer, mem_size);
                if (image == null || image.All(singleByte => singleByte == 0))
                {
                    ps4.FreeMemory(pid, mem, mem_size);
                    return null;
                }
                MemoryStream mStream = new();
                mStream.Write(image, 0, image.Length);
                ps4.FreeMemory(pid, mem, mem_size);
                return Image.FromStream(mStream);
            }
            ps4.FreeMemory(pid, mem, mem_size);
            return null;
        }
        private string GetGameName(string game)
        {
            var mem_size = 0x100000; // Due to shellcode, it'll be 1MB (lower amounts are unreliable, otherwise 32KB should be plenty)
            var mem = ps4.AllocateMemory(pid, mem_size);
            var path = mem;
            var buffer = mem + 0x101;

            ps4.WriteMemory(pid, path, $"/user/appmeta/{game}/pronunciation.xml");
            var ret = (int)ps4.Call(pid, stub, ReadFileAddr, path, buffer);
            if (ret == 1)
            {
                var xmlBytes = ps4.ReadMemory(pid, buffer, mem_size);
                if (xmlBytes == null || xmlBytes.All(singleByte => singleByte == 0))
                {
                    ps4.FreeMemory(pid, mem, mem_size);
                    return null;
                }
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(new MemoryStream(xmlBytes));
                // Based on some games which have language id = 00 with Japanese names (ex: The Playroom)
                // Language ID = 01 seems to be English consistently, but some Lua games don't have id = 01
                var englishNode = xmlDocument.DocumentElement.SelectSingleNode("/gamePackage/language[@id=01]/speechRecognitionWords/text");
                var anyNode = xmlDocument.DocumentElement.SelectSingleNode("/gamePackage/language/speechRecognitionWords/text");
                if (englishNode == null)
                {
                    return anyNode.InnerText;
                }
                ps4.FreeMemory(pid, mem, mem_size);
                return englishNode.InnerText;
            }
            ps4.FreeMemory(pid, mem, mem_size);
            return null;
        }
        private GameMetadata GetGameMetadata(string game)
        {
            if (GamesMetadata.ContainsKey(game))
            {
                return GamesMetadata[game];
            }
            var metadata = new GameMetadata();
            metadata.Image = GetGameImage(game);
            metadata.Name = GetGameName(game);
            // Either one is good enough to identify the game, hopefully, so no warning/error.
            if (metadata.Image != null || metadata.Name != null)
            {
                metadata.Installed = true;
            }
            GamesMetadata.Add(game, metadata);
            return metadata;
        }
        private void gamesButton_Click(object sender, EventArgs e)
        {
            if (!ps4.IsConnected)
            {
                SetStatus("Not Connected");
                return;
            }
            var dirs = GetSaveDirectories();
            gamesComboBox.DataSource = dirs;
            SetStatus("Refreshed Games");
        }

        private void setupButton_Click(object sender, EventArgs e)
        {
            var pm = ps4.GetProcessMaps(pid);
            var tmp = pm.FindEntry("libSceSaveData.sprx")?.start;
            if (tmp == null)
            {
                MessageBox.Show("savedata lib not found", "Error");
                return;
            }
            libSceSaveDataBase = (ulong)tmp;

            tmp = pm.FindEntry("libSceUserService.sprx")?.start;
            if (tmp == null)
            {
                MessageBox.Show("user service lib not found", "Error");
                return;
            }
            libSceUserServiceBase = (ulong)tmp;

            tmp = pm.FindEntry("executable")?.start;
            if (tmp == null)
            {
                MessageBox.Show("executable not found", "Error");
                return;
            }
            executableBase = (ulong)tmp;

            stub = ps4.InstallRPC(pid); // dummy in ps5debug

            var ids = GetLoginList();
            List<User> users = new List<User>();
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == -1)
                {
                    continue;
                }
                users.Add(new User { id = ids[i], name = GetUserName(ids[i]) });
            }
            userComboBox.DataSource = users.ToArray();

            var ret = ps4.Call(pid, stub, libSceSaveDataBase + Offsets.sceSaveDataInitialize3);
            WriteLog($"sceSaveDataInitialize3 ret = 0x{ret:X}");

            SetStatus("Setup Done :)");
        }

        public bool ApplyShellcorePatches(ProcessList pl)
        {
            var shellcore = pl.FindProcess("SceShellCore");
            var shellcore_maps = ps4.GetProcessMaps(shellcore.pid);
            var ex = shellcore_maps.FindEntry("executable");

            ps4.ChangeProtection(shellcore.pid, ex.start, (uint)(ex.end - ex.start), PS4DBG.VM_PROTECTIONS.VM_PROT_ALL);

            List<Patch> patchesToApply = Patches.GetShellcorePatches(Offsets.SelectedFirmwareShellcore);

            if (patchesToApply.Count > 0) // Check if there are any patches to apply
            {
                // Verify if first patch is already applied and set status accordingly
                var firstPatch = patchesToApply.FirstOrDefault();
                ulong firstPatchAddress = ex.start + firstPatch.Offset;

                byte[] originalBytes = ps4.ReadMemory(shellcore.pid, firstPatchAddress, firstPatch.Bytes.Length);
                if (originalBytes.SequenceEqual(firstPatch.Bytes))
                {
                    SetStatus("Shellcore is already patched, so skipped applying patches again");
                    ps4.ChangeProtection(shellcore.pid, ex.start, (uint)(ex.end - ex.start), PS4DBG.VM_PROTECTIONS.VM_PROT_EXECUTE);
                    return false;
                }

                // Loop through each patch and apply it
                foreach (var patch in patchesToApply)
                {
                    ulong targetAddress = ex.start + patch.Offset;
                    patch.OriginalBytes = ps4.ReadMemory(shellcore.pid, targetAddress, patch.Bytes.Length);

                    ps4.WriteMemory(shellcore.pid, targetAddress, patch.Bytes);
                }
            }
            else
            {
                SetStatus("Patching failed as no patches were found");
                ps4.ChangeProtection(shellcore.pid, ex.start, (uint)(ex.end - ex.start), PS4DBG.VM_PROTECTIONS.VM_PROT_EXECUTE);
                return false;
            }

            ps4.ChangeProtection(shellcore.pid, ex.start, (uint)(ex.end - ex.start), PS4DBG.VM_PROTECTIONS.VM_PROT_EXECUTE);
            return true;
        }

        private void ApplyCustomFunctions()
        {
            // Allocate memory for custom functions
            GetSaveDirectoriesAddr = ps4.AllocateMemory(pid, 0x8000);
            ReadFileAddr = ps4.AllocateMemory(pid, 0x8000);

            ps4.WriteMemory(pid, GetSaveDirectoriesAddr, functions.GetSaveDirectories);
            ps4.WriteMemory(pid, ReadFileAddr, functions.ReadFile);

            List<Patch> patchesToApply = Patches.GetLibcPatches(Offsets.SelectedFirmwareShellcore);
            List<Patch> imagePatchesToApply = Patches.GetLibcPatches(Offsets.SelectedFirmwareShellcore, true);

            if (patchesToApply.Count > 0) // Check if there are any patches to apply
            {
                // Loop through each patch and apply it
                foreach (var patch in patchesToApply)
                {
                    ulong targetAddress = GetSaveDirectoriesAddr + patch.Offset;
                    ps4.WriteMemory(pid, targetAddress, libSceLibcInternalBase + patch.FunctionOffset);
                }
            }
            if (imagePatchesToApply.Count > 0)
            {
                foreach (var patch in imagePatchesToApply)
                {
                    ulong targetAddress = ReadFileAddr + patch.Offset;
                    ps4.WriteMemory(pid, targetAddress, libSceLibcInternalBase + patch.FunctionOffset);
                }
            }
        }
        private void searchButton_Click(object sender, EventArgs e)
        {
            var dirNameAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataDirName)) * 1024 + 0x10 + Marshal.SizeOf(typeof(SceSaveDataParam)) * 1024);
            var titleIdAddr = dirNameAddr + (uint)Marshal.SizeOf(typeof(SceSaveDataDirName)) * 1024;
            var paramAddr = titleIdAddr + 0x10;
            SceSaveDataDirNameSearchCond searchCond = new SceSaveDataDirNameSearchCond
            {
                userId = GetUser(),
                titleId = titleIdAddr
            };
            SceSaveDataDirNameSearchResult searchResult = new SceSaveDataDirNameSearchResult
            {
                dirNames = dirNameAddr,
                dirNamesNum = 1024,
                param = paramAddr,
            };
            ps4.WriteMemory(pid, titleIdAddr, selectedGame);
            dirsComboBox.DataSource = Find(searchCond, searchResult);
            ps4.FreeMemory(pid, dirNameAddr, Marshal.SizeOf(typeof(SceSaveDataDirName)) * 1024);
            ps4.FreeMemory(pid, paramAddr, Marshal.SizeOf(typeof(SceSaveDataParam)) * 1024);
            if (dirsComboBox.Items.Count > 0)
            {
                SetStatus($"Found {dirsComboBox.Items.Count} Save Directories :D");
            }
            else
            {
                SetStatus("Found 0 Save Directories :(");
            }
        }

        private void mountButton_Click(object sender, EventArgs e)
        {
            if (dirsComboBox.Text.Length == 0)
            {
                return;
            }
            var dirNameAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataDirName)) + 0x10 + 0x41);
            var titleIdAddr = dirNameAddr + (uint)Marshal.SizeOf(typeof(SceSaveDataDirName));
            var fingerprintAddr = titleIdAddr + 0x10;
            ps4.WriteMemory(pid, titleIdAddr, selectedGame);
            ps4.WriteMemory(pid, fingerprintAddr, "0000000000000000000000000000000000000000000000000000000000000000");
            SceSaveDataDirName dirName = new SceSaveDataDirName
            {
                data = dirsComboBox.Text
            };

            SceSaveDataMount mount = new SceSaveDataMount
            {
                userId = GetUser(),
                dirName = dirNameAddr,
                blocks = 32768,
                mountMode = 0x8 | 0x2,
                titleId = titleIdAddr,
                fingerprint = fingerprintAddr
            };
            SceSaveDataMountResult mountResult = new SceSaveDataMountResult
            {

            };
            ps4.WriteMemory(pid, dirNameAddr, dirName);
            mp = Mount(mount, mountResult);

            ps4.FreeMemory(pid, dirNameAddr, Marshal.SizeOf(typeof(SceSaveDataDirName)));
            if (mp != "")
            {
                SetStatus($"Save Mounted in /mnt/pfs/savedata_{user:x}_{selectedGame}_{dirName.data}/");
            }
            else
            {
                SetStatus("Mounting Failed");
            }
        }

        private void unmountButton_Click(object sender, EventArgs e)
        {
            if (mp == "")
            {
                SetStatus("No save mounted");
                return;
            }
            SceSaveDataMountPoint mountPoint = new SceSaveDataMountPoint
            {
                data = mp,
            };

            Unmount(mountPoint);
            mp = null;
            SetStatus("Save Unmounted");
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            if (nameTextBox.Text == "")
            {
                SetStatus("No Save Name");
                return;
            }
            var dirNameAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataDirName)));
            SceSaveDataDirName dirName = new SceSaveDataDirName
            {
                data = nameTextBox.Text
            };

            SceSaveDataMount mount = new SceSaveDataMount
            {
                userId = GetUser(),
                dirName = dirNameAddr,
                blocks = (ulong)sizeTrackBar.Value,
                mountMode = 4 | 2 | 8,

            };
            SceSaveDataMountResult mountResult = new SceSaveDataMountResult
            {

            };
            ps4.WriteMemory(pid, dirNameAddr, dirName);
            var mp = Mount(mount, mountResult);
            ps4.FreeMemory(pid, dirNameAddr, Marshal.SizeOf(typeof(SceSaveDataDirName)));
            if (mp != "")
            {
                SetStatus("Save Created");
                SceSaveDataMountPoint mountPoint = new SceSaveDataMountPoint
                {
                    data = mp,
                };
                Unmount(mountPoint);
            }
            else
            {
                SetStatus("Save Creation Failed");
            }
        }

        private int GetUser()
        {
            if (user != 0)
            {
                return user;
            }
            else
            {
                return InitialUser();
            }
        }

        private int InitialUser()
        {
            var bufferAddr = ps4.AllocateMemory(pid, sizeof(int));

            ps4.Call(pid, stub, libSceUserServiceBase + Offsets.sceUserServiceGetInitialUser, bufferAddr);

            var id = ps4.ReadMemory<int>(pid, bufferAddr);

            ps4.FreeMemory(pid, bufferAddr, sizeof(int));

            return id;
        }

        private int[] GetLoginList()
        {
            var bufferAddr = ps4.AllocateMemory(pid, sizeof(int) * 4);
            ps4.Call(pid, stub, libSceUserServiceBase + Offsets.sceUserServiceGetLoginUserIdList, bufferAddr);

            var id = ps4.ReadMemory(pid, bufferAddr, sizeof(int) * 4);
            var size = id.Length / sizeof(int);
            var ints = new int[size];
            for (var index = 0; index < size; index++)
            {
                ints[index] = BitConverter.ToInt32(id, index * sizeof(int));
            }
            ps4.FreeMemory(pid, bufferAddr, sizeof(int));

            return ints;
        }

        private string GetUserName(int userid)
        {
            var bufferAddr = ps4.AllocateMemory(pid, 17);
            ps4.Call(pid, stub, libSceUserServiceBase + Offsets.sceUserServiceGetUserName, userid, bufferAddr, 17);
            var str = ps4.ReadMemory<string>(pid, bufferAddr);
            ps4.FreeMemory(pid, bufferAddr, 17);
            return str;
        }

        private SearchEntry[] Find(SceSaveDataDirNameSearchCond searchCond, SceSaveDataDirNameSearchResult searchResult)
        {
            var searchCondAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataDirNameSearchCond)));
            var searchResultAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataDirNameSearchResult)));

            ps4.WriteMemory(pid, searchCondAddr, searchCond);
            ps4.WriteMemory(pid, searchResultAddr, searchResult);

            var ret = ps4.Call(pid, stub, libSceSaveDataBase + Offsets.sceSaveDataDirNameSearch, searchCondAddr, searchResultAddr);
            WriteLog($"sceSaveDataDirNameSearch ret = 0x{ret:X}");
            if (ret == 0)
            {
                searchResult = ps4.ReadMemory<SceSaveDataDirNameSearchResult>(pid, searchResultAddr);
                SearchEntry[] sEntries = new SearchEntry[searchResult.hitNum];
                for (uint i = 0; i < searchResult.hitNum; i++)
                {
                    SceSaveDataParam tmp = ps4.ReadMemory<SceSaveDataParam>(pid, searchResult.param + i * (uint)Marshal.SizeOf(typeof(SceSaveDataParam)));
                    sEntries[i] = new SearchEntry
                    {
                        dirName = ps4.ReadMemory<string>(pid, searchResult.dirNames + i * 32),
                        title = tmp.Title,
                        subtitle = tmp.SubTitle,
                        detail = tmp.Detail,
                        time = new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(tmp.mtime).ToString(),
                    };
                }
                ps4.FreeMemory(pid, searchCondAddr, Marshal.SizeOf(typeof(SceSaveDataDirNameSearchCond)));
                ps4.FreeMemory(pid, searchResultAddr, Marshal.SizeOf(typeof(SceSaveDataDirNameSearchResult)));
                return sEntries;
            }

            ps4.FreeMemory(pid, searchCondAddr, Marshal.SizeOf(typeof(SceSaveDataDirNameSearchCond)));
            ps4.FreeMemory(pid, searchResultAddr, Marshal.SizeOf(typeof(SceSaveDataDirNameSearchResult)));

            return Array.Empty<SearchEntry>();

        }

        private string Mount(SceSaveDataMount mount, SceSaveDataMountResult mountResult)
        {
            var mountAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataMount)));
            var mountResultAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataMountResult)));

            ps4.WriteMemory(pid, mountAddr, mount);
            ps4.WriteMemory(pid, mountResultAddr, mountResult);
            var ret = ps4.Call(pid, stub, libSceSaveDataBase + Offsets.sceSaveDataMount, mountAddr, mountResultAddr);
            WriteLog($"sceSaveDataMount ret = 0x{ret:X}");
            WriteLog($"mountResultAddr ret = 0x{mountResultAddr:X}");
            if (ret == 0)
            {
                mountResult = ps4.ReadMemory<SceSaveDataMountResult>(pid, mountResultAddr);

                ps4.FreeMemory(pid, mountAddr, Marshal.SizeOf(typeof(SceSaveDataMount)));
                ps4.FreeMemory(pid, mountResultAddr, Marshal.SizeOf(typeof(SceSaveDataMountResult)));
                return mountResult.mountPoint.data;
            }

            ps4.FreeMemory(pid, mountAddr, Marshal.SizeOf(typeof(SceSaveDataMount)));
            ps4.FreeMemory(pid, mountResultAddr, Marshal.SizeOf(typeof(SceSaveDataMountResult)));

            return "";
        }

        private void Unmount(SceSaveDataMountPoint mountPoint)
        {
            var mountPointAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataMountPoint)));

            ps4.WriteMemory(pid, mountPointAddr, mountPoint);
            var ret = ps4.Call(pid, stub, libSceSaveDataBase + Offsets.sceSaveDataUmount, mountPointAddr);
            WriteLog($"sceSaveDataUmount ret = 0x{ret:X}");
            ps4.FreeMemory(pid, mountPointAddr, Marshal.SizeOf(typeof(SceSaveDataMountPoint)));
            mp = null;
        }

        private string TransferMount(SceSaveDataTransferringMount mount, SceSaveDataMountResult mountResult)
        {
            var mountAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataTransferringMount)));
            var mountResultAddr = ps4.AllocateMemory(pid, Marshal.SizeOf(typeof(SceSaveDataMountResult)));

            ps4.WriteMemory(pid, mountAddr, mount);
            ps4.WriteMemory(pid, mountResultAddr, mountResult);
            var ret = ps4.Call(pid, stub, libSceSaveDataBase + Offsets.sceSaveDataTransferringMount, mountAddr, mountResultAddr);
            WriteLog($"sceSaveDataTransferringMount ret = 0x{ret:X}");
            if (ret == 0)
            {
                mountResult = ps4.ReadMemory<SceSaveDataMountResult>(pid, mountResultAddr);

                ps4.FreeMemory(pid, mountAddr, Marshal.SizeOf(typeof(SceSaveDataTransferringMount)));
                ps4.FreeMemory(pid, mountResultAddr, Marshal.SizeOf(typeof(SceSaveDataMountResult)));

                return mountResult.mountPoint.data;
            }

            ps4.FreeMemory(pid, mountAddr, Marshal.SizeOf(typeof(SceSaveDataTransferringMount)));
            ps4.FreeMemory(pid, mountResultAddr, Marshal.SizeOf(typeof(SceSaveDataMountResult)));

            return "";
        }

        class SearchEntry
        {
            public string dirName;
            public string title;
            public string subtitle;
            public string detail;
            public string time;
            public override string ToString()
            {
                return dirName;
            }
        }

        private void dirsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dirsComboBox.SelectedItem != null)
            {
                titleTextBox.Text = ((SearchEntry)dirsComboBox.SelectedItem).title;
                subtitleTextBox.Text = ((SearchEntry)dirsComboBox.SelectedItem).subtitle;
                detailsTextBox.Text = ((SearchEntry)dirsComboBox.SelectedItem).detail;
                detailsTextBox.ForeColor = SystemColors.ControlText;
                dateTextBox.Text = ((SearchEntry)dirsComboBox.SelectedItem).time;
            }
            else
            {
                // Clear all textboxes when no item is selected
                titleTextBox.Text = "";
                subtitleTextBox.Text = "";
                detailsTextBox.Text = "";
                dateTextBox.Text = "";
            }
        }

        private void clearSavedataInfo()
        {
            dirsComboBox.DataSource = null;
            titleTextBox.Text = "";
            subtitleTextBox.Text = "";
            detailsTextBox.Text = "";
            detailsTextBox.ForeColor = SystemColors.ControlText;
            dateTextBox.Text = "";
        }

        private void gamesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gamesComboBox.SelectedItem != null)
            {
                selectedGame = (string)gamesComboBox.SelectedItem;
                clearSavedataInfo();
                var metadata = GetGameMetadata(selectedGame);
                if (!metadata.Installed)
                {
                    detailsTextBox.Text += "Details for " + (metadata.Name ?? selectedGame) + " were not found. Game might be uninstalled." + Environment.NewLine + "You can mount a save to get more information.";
                    detailsTextBox.ForeColor = System.Drawing.Color.Red;
                }
                gameImageBox.Image = metadata.Image;
                titleTextBox.Text = metadata.Name;
            }
            else
            {
                selectedGame = "";
                clearSavedataInfo();
                gameImageBox.Image = null;
            }
        }

        private void userComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (userComboBox.SelectedItem != null)
            {
                user = ((User)userComboBox.SelectedItem).id;
            }
            else
            {
                user = 0;
            }
        }

        class User
        {
            public int id;
            public string name;

            public override string ToString()
            {
                return name;
            }
        }

        private void patchButton_Click(object sender, EventArgs e)
        {
            if (isPatched)
            {
                SetStatus("Already patched Shellcore!");
                return;
            }

            var pl = ps4.GetProcessList();
            var shellcoreui = pl.FindProcess("SceShellUI");
            if (shellcoreui == null)
            {
                SetStatus("Couldn't find SceShellUI");
                return;
            }
            pid = shellcoreui.pid;

            var pm = ps4.GetProcessMaps(pid);
            var tmp = pm.FindEntry("libSceSaveData.sprx")?.start;
            if (tmp == null)
            {
                MessageBox.Show("savedata lib not found", "Error");
                return;
            }
            libSceSaveDataBase = (ulong)tmp;

            tmp = pm.FindEntry("libSceUserService.sprx")?.start;
            if (tmp == null)
            {
                MessageBox.Show("user service lib not found", "Error");
                return;
            }
            libSceUserServiceBase = (ulong)tmp;

            tmp = pm.FindEntry("executable")?.start;
            if (tmp == null)
            {
                MessageBox.Show("executable not found", "Error");
                return;
            }
            executableBase = (ulong)tmp;

            tmp = pm.FindEntry("libSceLibcInternal.sprx")?.start;
            if (tmp == null)
            {
                MessageBox.Show("libc not found", "Error");
                return;
            }
            libSceLibcInternalBase = (ulong)tmp;

            //SHELLCORE PATCHES (SceShellCore)
            if (ApplyShellcorePatches(pl))
                SetStatus("Patched Shellcore");

            // Write libSceLibcInternal function addresses to custom function shellcode
            ApplyCustomFunctions();

            isPatched = true;

            setupButton.Enabled = true;
            userComboBox.Enabled = true;

            gamesButton.Enabled = true;
            gamesComboBox.Enabled = true;
            gameImageBox.Enabled = true;

            searchButton.Enabled = true;
            dirsComboBox.Enabled = true;

            mountButton.Enabled = true;
            unmountButton.Enabled = true;

            nameTextBox.Enabled = true;
            createButton.Enabled = true;

        }

        private void unpatch()
        {
            var pl = ps4.GetProcessList();
            var shellcore = pl.FindProcess("SceShellCore");
            var shellcore_maps = ps4.GetProcessMaps(shellcore.pid);
            var ex = shellcore_maps.FindEntry("executable");

            // Change memory mapping to RWX
            ps4.ChangeProtection(shellcore.pid, ex.start, (uint)(ex.end - ex.start), PS4DBG.VM_PROTECTIONS.VM_PROT_ALL);

            List<Patch> patchesToApply = Patches.GetShellcorePatches(Offsets.SelectedFirmwareShellcore);

            if (patchesToApply.Count > 0) // Check if there are any patches to apply
            {
                // Loop through each patch and apply it
                foreach (var patch in patchesToApply)
                {
                    ulong targetAddress = ex.start + patch.Offset;
                    // edge case for when originalBytes aren't set properly (usually while debugging or if the app crashes)
                    if (patch.OriginalBytes != null)
                    {
                        ps4.WriteMemory(shellcore.pid, targetAddress, patch.OriginalBytes);
                    }
                    patch.OriginalBytes = [];
                }
            }

            // Return memory mapping to execute-only
            ps4.ChangeProtection(shellcore.pid, ex.start, (uint)(ex.end - ex.start), PS4DBG.VM_PROTECTIONS.VM_PROT_EXECUTE);

            // Free custom function shellcode
            ps4.FreeMemory(pid, GetSaveDirectoriesAddr, 0x8000);
            ps4.FreeMemory(pid, ReadFileAddr, 0x8000);
            GetSaveDirectoriesAddr = 0;
            ReadFileAddr = 0;

            isPatched = false;

            //dirsComboBox.SelectedItem = null;
            dirsComboBox.DataSource = null;
            //gamesComboBox.SelectedItem = null;
            gamesComboBox.DataSource = null;
            //userComboBox.SelectedItem = null;
            userComboBox.DataSource = null;
            gameImageBox.Image = null;

            setupButton.Enabled = false;
            userComboBox.Enabled = false;

            gamesButton.Enabled = false;
            gamesComboBox.Enabled = false;
            gameImageBox.Enabled = false;

            searchButton.Enabled = false;
            dirsComboBox.Enabled = false;

            mountButton.Enabled = false;
            unmountButton.Enabled = false;

            nameTextBox.Enabled = false;
            createButton.Enabled = false;
            SetStatus("Unpatched Shellcore");
        }
        private void unpatchButton_Click(object sender, EventArgs e)
        {
            if (!isPatched)
            {
                SetStatus("Already unpatched Shellcore!");
                return;
            }

            unpatch();
        }

        private void Main_Closing(object sender, CancelEventArgs e)
        {
            if (isPatched)
            {
                unpatch();
            }
        }
    }
}
