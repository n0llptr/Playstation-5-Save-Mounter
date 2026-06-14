using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private MounterClient mounter;
        private int user = 0;
        private string selectedGame = null;
        private string mountPoint = "";
        Dictionary<string, GameMetadata> GamesMetadata = [];
        Dictionary<string, string> gameTitles = [];

        private const string ElfFileName = "mounter.elf";

        public Main()
        {
            InitializeComponent();

            if (File.Exists("ip"))
                ipTextBox.Text = File.ReadAllText("ip");
        }

        private static string FormatSize(double size)
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

        private void snapSize()
        {
            if (!sizeSnapCheckbox.Checked) return;
            int v = sizeTrackBar.Value;
            sizeTrackBar.Value = (int)Math.Round((double)v / 32, 2) * 32;
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

        private int GetUser() => user;

        private async void connectButton_Click(object sender, EventArgs e)
        {
            if (connectButton.Text == "Disconnect")
            {
                connectButton.Enabled = false;
                SetStatus("Disconnecting...");

                await Task.Run(() =>
                {
                    try
                    {
                        if (mountPoint != "")
                        {
                            mounter.Unmount();
                            mountPoint = "";
                        }
                    }
                    catch { }
                    mounter?.Disconnect();
                    mounter = null;
                });

                SetStatus("Disconnected");
                connectButton.Text = "Connect";
                connectButton.Enabled = true;
                label2.Text = "Detected firmware version:";

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

            connectButton.Enabled = false;
            SetStatus("Deploying payload...");

            try
            {
                var ip = ipTextBox.Text;

                if (!File.Exists(ElfFileName))
                {
                    MessageBox.Show($"{ElfFileName} not found.\nPlace it next to the executable.", "Error");
                    SetStatus("Payload not found");
                    connectButton.Enabled = true;
                    return;
                }

                byte[] elfData = File.ReadAllBytes(ElfFileName);

                var fw = await Task.Run(() =>
                {
                    // Save IP for next launch
                    File.WriteAllText("ip", ip);

                    // Send payload to elfldr
                    ElfldrClient.SendElf(ip, elfData);

                    // Connect to the payload's TCP server (with retries)
                    mounter = new MounterClient();
                    mounter.Connect(ip);

                    return mounter.GetFirmwareVersion();
                });

                label2.Text = "Detected firmware version: " + fw;
                SetStatus("Connected");
                connectButton.Text = "Disconnect";

                setupButton.Enabled = true;
                userComboBox.Enabled = true;
            }
            catch (Exception ex)
            {
                SetStatus("Failed: " + ex.Message);
                mounter?.Dispose();
                mounter = null;
            }
            finally
            {
                connectButton.Enabled = true;
            }
        }

        private async void setupButton_Click(object sender, EventArgs e)
        {
            if (mounter == null || !mounter.IsConnected) { SetStatus("Not connected"); return; }
            setupButton.Enabled = false;
            SetStatus("Getting users...");

            try
            {
                var (users, titles) = await Task.Run(() =>
                {
                    var u = mounter.GetUsers();
                    var t = LoadGameTitles();
                    return (u, t);
                });

                gameTitles = titles;
                var userList = users.Select(u => new User { id = u.id, name = u.name }).ToArray();
                userComboBox.DataSource = userList;

                gamesButton.Enabled = true;
                gamesComboBox.Enabled = true;
                gameImageBox.Enabled = true;
                searchButton.Enabled = true;
                dirsComboBox.Enabled = true;
                mountButton.Enabled = true;
                unmountButton.Enabled = true;

                SetStatus("Setup done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                SetStatus("Setup failed");
            }
            finally
            {
                setupButton.Enabled = true;
            }
        }

        private async void gamesButton_Click(object sender, EventArgs e)
        {
            if (mounter == null || !mounter.IsConnected) { SetStatus("Not connected"); return; }

            gamesButton.Enabled = false;
            SetStatus("Getting save directories...");

            var dirs = await Task.Run(() => mounter.ListSaves(GetUser()));
            gamesComboBox.DataSource = dirs;
            gamesButton.Enabled = true;
            SetStatus("Refreshed games");
        }

        private async void searchButton_Click(object sender, EventArgs e)
        {
            if (mounter == null || !mounter.IsConnected) { SetStatus("Not connected"); return; }
            if (selectedGame == null) return;

            searchButton.Enabled = false;
            SetStatus("Searching save directories...");

            var results = await Task.Run(() => mounter.Search(GetUser(), selectedGame));
            dirsComboBox.DataSource = results;
            searchButton.Enabled = true;

            if (results.Length > 0)
                SetStatus($"Found {results.Length} save directories");
            else
                SetStatus("Found 0 save directories");
        }

        private async void mountButton_Click(object sender, EventArgs e)
        {
            if (mounter == null || !mounter.IsConnected) { SetStatus("Not connected"); return; }
            if (dirsComboBox.SelectedItem == null) return;

            mountButton.Enabled = false;
            SetStatus("Mounting save...");

            string dirName = dirsComboBox.SelectedItem is SearchResult sr ? sr.DirName : dirsComboBox.Text;

            try
            {
                var mp = await Task.Run(() => mounter.Mount(GetUser(), selectedGame, dirName));
                mountPoint = mp ?? "";
                SetStatus($"Save mounted at {mp}/");
            }
            catch (Exception ex)
            {
                SetStatus("Mount failed: " + ex.Message);
            }
            finally
            {
                mountButton.Enabled = true;
            }
        }

        private async void unmountButton_Click(object sender, EventArgs e)
        {
            if (mounter == null || !mounter.IsConnected) { SetStatus("Not connected"); return; }
            if (mountPoint == "")
            {
                SetStatus("No save mounted");
                return;
            }

            SetStatus("Unmounting save...");

            try
            {
                await Task.Run(() => mounter.Unmount());
                mountPoint = "";
                SetStatus("Save unmounted");
            }
            catch (Exception ex)
            {
                SetStatus("Unmount failed: " + ex.Message);
            }
        }

        private async void createButton_Click(object sender, EventArgs e)
        {
            if (mounter == null || !mounter.IsConnected) { SetStatus("Not connected"); return; }
            if (nameTextBox.Text == "" || string.IsNullOrEmpty(selectedGame))
            {
                SetStatus("No save name or game selected");
                return;
            }

            createButton.Enabled = false;
            SetStatus("Creating save...");

            var saveName = nameTextBox.Text;
            var saveBlocks = (ulong)sizeTrackBar.Value;
            var saveGame = selectedGame;
            var saveUser = GetUser();

            try
            {
                var mp = await Task.Run(() => mounter.CreateSave(
                    saveUser, saveGame, saveName, saveBlocks));
                mountPoint = mp;

                // Add new entry to the list directly
                var existing = dirsComboBox.DataSource as SearchResult[] ?? [];
                var newEntry = new SearchResult { DirName = saveName };
                var updated = existing.Append(newEntry).ToArray();
                dirsComboBox.DataSource = updated;
                dirsComboBox.SelectedIndex = updated.Length - 1;

                SetStatus($"Save created and mounted at {mp}/");
            }
            catch (Exception ex)
            {
                SetStatus("Create failed: " + ex.Message);
            }
            finally
            {
                createButton.Enabled = true;
            }
        }

        // ported from garlic-savemgr by earthonion
        private Dictionary<string, string> LoadGameTitles()
        {
            var titles = new Dictionary<string, string>();
            string tmpPath = Path.Combine(Path.GetTempPath(), "app.db");
            try
            {
                byte[] dbData = mounter.ReadFile("/system_data/priv/mms/app.db");
                File.WriteAllBytes(tmpPath, dbData);
                using var conn = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={tmpPath};Mode=ReadOnly");
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT titleId, titleName FROM tbl_contentinfo WHERE titleName IS NOT NULL";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string tid = reader.GetString(0);
                    string name = reader.GetString(1);
                    titles[tid] = name;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadGameTitles failed: {ex.Message}");
            }
            finally
            {
                try { File.Delete(tmpPath); } catch { }
            }
            return titles;
        }

        private GameMetadata GetGameMetadata(string game)
        {
            if (GamesMetadata.ContainsKey(game))
                return GamesMetadata[game];

            var metadata = new GameMetadata();
            if (gameTitles.TryGetValue(game, out string title))
                metadata.Name = title;
            try { metadata.Image = mounter.GetGameIcon(game); } catch { }

            if (metadata.Image != null || metadata.Name != null)
                metadata.Installed = true;

            GamesMetadata.Add(game, metadata);
            return metadata;
        }

        private void dirsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dirsComboBox.SelectedItem is SearchResult sr)
            {
                if (!string.IsNullOrEmpty(sr.Title))
                    titleTextBox.Text = sr.Title;
                subtitleTextBox.Text = sr.Subtitle;
                detailsTextBox.Text = sr.Detail;
                detailsTextBox.ForeColor = SystemColors.ControlText;
                dateTextBox.Text = sr.Time;
            }
            else
            {
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

        private async void gamesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gamesComboBox.SelectedItem != null)
            {
                selectedGame = (string)gamesComboBox.SelectedItem;
                clearSavedataInfo();
                var game = selectedGame;
                var metadata = await Task.Run(() => GetGameMetadata(game));
                if (selectedGame != game) return;
                if (!metadata.Installed)
                {
                    detailsTextBox.Text += "Details for " + (metadata.Name ?? selectedGame) + " were not found. Game might be uninstalled." +
                        Environment.NewLine + "You can mount a save to get more information.";
                    detailsTextBox.ForeColor = System.Drawing.Color.Red;
                }
                gameImageBox.Image = metadata.Image;
                titleTextBox.Text = metadata.Name ?? "";
                nameTextBox.Enabled = true;
                createButton.Enabled = true;
            }
            else
            {
                selectedGame = "";
                clearSavedataInfo();
                nameTextBox.Enabled = false;
                createButton.Enabled = false;
                gameImageBox.Image = null;
            }
        }

        private void userComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (userComboBox.SelectedItem != null)
                user = ((User)userComboBox.SelectedItem).id;
            else
                user = 0;
        }

        class User
        {
            public int id;
            public string name;
            public override string ToString() => name;
        }

        private bool _closing = false;

        private async void Main_Closing(object sender, CancelEventArgs e)
        {
            if (_closing || mounter == null) return;

            _closing = true;
            e.Cancel = true;
            Enabled = false;
            SetStatus("Cleaning up...");

            await Task.Run(() =>
            {
                try
                {
                    if (mountPoint != "")
                        mounter.Unmount();
                }
                catch { }
                try { mounter.Disconnect(); } catch { }
                mounter.Dispose();
            });

            mounter = null;
            Close();
        }
    }
}
