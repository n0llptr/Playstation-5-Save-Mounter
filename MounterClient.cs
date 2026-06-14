using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Microsoft.Data.Sqlite;

namespace PS4Saves;

public class MounterClient : IDisposable
{
    public const int DefaultPort = 9090;

    private TcpClient _tcp;
    private NetworkStream _stream;
    private byte[] _saveDbCache;
    private int _saveDbUserId;

    public bool IsConnected => _tcp?.Connected == true;

    public void Connect(string host, int port = DefaultPort, int retries = 10)
    {
        for (int i = 0; i < retries; i++)
        {
            try
            {
                _tcp = new TcpClient();
                _tcp.Connect(host, port);
                _tcp.ReceiveTimeout = 10000;
                _tcp.SendTimeout = 10000;
                _stream = _tcp.GetStream();
                return;
            }
            catch
            {
                _tcp?.Dispose();
                if (i == retries - 1) throw;
                System.Threading.Thread.Sleep(500);
            }
        }
    }

    public void Disconnect()
    {
        try
        {
            if (_stream != null)
            {
                SendCommand("EXIT");
                ReadLine();
            }
        }
        catch { }
        Dispose();
    }

    private void SendCommand(string cmd)
    {
        if (_stream == null) throw new IOException("Not connected");
        byte[] data = Encoding.UTF8.GetBytes(cmd + "\n");
        _stream.Write(data, 0, data.Length);
        _stream.Flush();
    }

    private string ReadLine()
    {
        if (_stream == null) throw new IOException("Not connected");
        var buf = new List<byte>();
        while (true)
        {
            int b = _stream.ReadByte();
            if (b < 0) return null;
            if (b == '\n') break;
            if (b == '\r') continue;
            buf.Add((byte)b);
        }
        return Encoding.UTF8.GetString(buf.ToArray());
    }

    private byte[] ReadBytes(int count)
    {
        if (_stream == null) throw new IOException("Not connected");
        byte[] buf = new byte[count];
        int pos = 0;
        while (pos < count)
        {
            int n = _stream.Read(buf, pos, count - pos);
            if (n <= 0) throw new IOException("Connection closed during binary read");
            pos += n;
        }
        return buf;
    }

    private string ExpectOk()
    {
        string line = ReadLine();
        if (line == null) throw new IOException("Connection closed");
        if (line.StartsWith("ERR ")) throw new Exception(line[4..]);
        if (line == "OK") return "";
        if (line.StartsWith("OK ")) return line[3..];
        throw new Exception("Unexpected response: " + line);
    }

    public string GetFirmwareVersion()
    {
        SendCommand("GET_FW");
        return ExpectOk();
    }

    public (int id, string name)[] GetUsers()
    {
        SendCommand("GET_USERS");
        int count = int.Parse(ExpectOk());
        var users = new List<(int, string)>();
        for (int i = 0; i < count; i++)
        {
            string line = ReadLine() ?? throw new IOException("Connection closed");
            int sp = line.IndexOf(' ');
            if (sp < 0) continue;
            int id = Convert.ToInt32(line[..sp], 16);
            string name = line[(sp + 1)..];
            users.Add((id, name));
        }
        return [.. users];
    }

    public string[] ListSaves(int userId)
    {
        SendCommand($"LIST_SAVES {userId:x}");
        int count = int.Parse(ExpectOk());
        var titles = new string[count];
        for (int i = 0; i < count; i++)
            titles[i] = ReadLine() ?? throw new IOException("Connection closed");
        Array.Sort(titles);
        return titles;
    }

    public SearchResult[] Search(int userId, string titleId)
    {
        SendCommand($"SEARCH {userId:x} {titleId}");
        int count = int.Parse(ExpectOk());
        var results = new SearchResult[count];
        for (int i = 0; i < count; i++)
        {
            string line = ReadLine() ?? throw new IOException("Connection closed");
            string[] parts = line.Split('\t');
            results[i] = new SearchResult
            {
                DirName = parts.Length > 0 ? parts[0] : "",
                Title = parts.Length > 1 ? parts[1] : "",
                Subtitle = parts.Length > 2 ? parts[2] : "",
                Detail = parts.Length > 3 ? parts[3] : "",
                Time = parts.Length > 4 && long.TryParse(parts[4], out long mt) && mt > 0
                    ? DateTimeOffset.FromUnixTimeSeconds(mt).LocalDateTime.ToString()
                    : "",
            };
        }

        // Enrich entries missing subtitle/detail from savedata.db (mainly PS4 saves, PS5 uses SFO)
        bool needsEnrich = false;
        foreach (var r in results)
            if (string.IsNullOrEmpty(r.Subtitle) && string.IsNullOrEmpty(r.Detail))
            { needsEnrich = true; break; }

        if (needsEnrich)
            EnrichFromSaveDb(userId, titleId, results);

        return results;
    }

    private void EnrichFromSaveDb(int userId, string titleId, SearchResult[] results)
    {
        byte[] dbData;
        try
        {
            if (_saveDbCache != null && _saveDbUserId == userId)
            {
                dbData = _saveDbCache;
            }
            else
            {
                string dbPath = $"/system_data/savedata/{userId:x}/db/user/savedata.db";
                dbData = ReadFile(dbPath);
                _saveDbCache = dbData;
                _saveDbUserId = userId;
            }
        }
        catch { return; }

        string tmpPath = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tmpPath, dbData);
            using var conn = new SqliteConnection($"Data Source={tmpPath};Mode=ReadOnly");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT dir_name, main_title, sub_title, detail FROM savedata WHERE title_id = @tid";
            cmd.Parameters.AddWithValue("@tid", titleId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string dirName = reader.GetString(0);
                string mainTitle = reader.IsDBNull(1) ? "" : reader.GetString(1);
                string subTitle = reader.IsDBNull(2) ? "" : reader.GetString(2);
                string detail = reader.IsDBNull(3) ? "" : reader.GetString(3);

                foreach (var r in results)
                {
                    if (r.DirName != dirName) continue;
                    // Game title from app.db; fallback to savedata.db for uninstalled games
                    if (string.IsNullOrEmpty(r.Title) && !string.IsNullOrEmpty(mainTitle))
                        r.Title = mainTitle;
                    if (string.IsNullOrEmpty(r.Subtitle)) r.Subtitle = subTitle;
                    if (string.IsNullOrEmpty(r.Detail)) r.Detail = detail;
                    break;
                }
            }
        }
        catch { }
        finally
        {
            try { File.Delete(tmpPath); } catch { }
        }
    }

    public Image GetGameIcon(string titleId)
    {
        try
        {
            byte[] data = ReadFile($"/user/appmeta/{titleId}/icon0.png");
            return Image.FromStream(new MemoryStream(data));
        }
        catch
        {
            return null;
        }
    }

    public string Mount(int userId, string titleId, string dirName)
    {
        SendCommand($"MOUNT {userId:x} {titleId} {dirName}");
        return ExpectOk();
    }

    public void Unmount()
    {
        SendCommand("UMOUNT");
        ExpectOk();
    }

    public byte[] ReadFile(string path, int maxSize = 16 * 1024 * 1024)
    {
        SendCommand($"READ_FILE {path}");
        int size = int.Parse(ExpectOk());
        if (size > maxSize) throw new Exception($"File too large ({size} bytes, max {maxSize})");
        return ReadBytes(size);
    }

    public string CreateSave(int userId, string titleId, string dirName, ulong blocks)
    {
        SendCommand($"CREATE {userId:x} {titleId} {dirName} {blocks}");
        string result = ExpectOk();
        _saveDbCache = null;
        return result;
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _stream = null;
        _tcp?.Dispose();
        _tcp = null;
    }
}

public class SearchResult
{
    public string DirName;
    public string Title;
    public string Subtitle;
    public string Detail;
    public string Time;

    public override string ToString() => DirName;
}
