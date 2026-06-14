// Kernel dynlib walk and NID encoding based on ps5-payload-dev/sdk by John Tornblom
// https://github.com/ps5-payload-dev/sdk

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using libdebug;

namespace PS4Saves;

public static class OffsetResolver
{
    private const string CacheFile = "libs_offsets_cache.json";

    // Shellcode slot offset → (module, function) for GetSaveDirectories
    public static readonly (int SlotOffset, string Module, string Function)[] DirShellcodeSlots =
    [
        (0x12, "libSceLibcInternal", "opendir"),
        (0x20, "libSceLibcInternal", "readdir"),
        (0x2E, "libSceLibcInternal", "closedir"),
        (0x3C, "libSceLibcInternal", "strcpy"),
    ];

    // Shellcode slot offset → (module, function) for ReadFile
    public static readonly (int SlotOffset, string Module, string Function)[] FileShellcodeSlots =
    [
        (0x12, "libSceLibcInternal", "fopen"),
        (0x20, "libSceLibcInternal", "fread"),
        (0x2E, "libSceLibcInternal", "fclose"),
    ];

    // All modules and functions needed
    private static readonly Dictionary<string, string[]> RequiredFunctions = new()
    {
        ["libSceLibcInternal"] = ["opendir", "readdir", "closedir", "strcpy", "fopen", "fread", "fclose"],
        ["libSceUserService"] = ["sceUserServiceGetInitialUser", "sceUserServiceGetLoginUserIdList", "sceUserServiceGetUserName"],
        ["libSceSaveData"] = ["sceSaveDataMount", "sceSaveDataUmount", "sceSaveDataDirNameSearch", "sceSaveDataTransferringMount", "sceSaveDataInitialize3"],
    };

    // From ps5-payload-dev/sdk kernel.c
    private static readonly Dictionary<int, uint> AllprocOffsets = new()
    {
        [0x0100] = 0x26D1BF8, [0x0101] = 0x26D1BF8, [0x0102] = 0x26D1BF8,
        [0x0105] = 0x26D1C18, [0x0107] = 0x26D1C18, [0x0110] = 0x26D1C18,
        [0x0111] = 0x26D1C18, [0x0112] = 0x26D1C18, [0x0113] = 0x26D1C18,
        [0x0114] = 0x26D1C18,
        [0x0200] = 0x2701C28,
        [0x0220] = 0x2701C28, [0x0225] = 0x2701C28, [0x0226] = 0x2701C28,
        [0x0230] = 0x2701C28, [0x0250] = 0x2701C28, [0x0270] = 0x2701C28,
        [0x0300] = 0x276DC58, [0x0310] = 0x276DC58, [0x0320] = 0x276DC58,
        [0x0321] = 0x276DC58,
        [0x0402] = 0x27EDCB8,
        [0x0400] = 0x27EDCB8, [0x0403] = 0x27EDCB8, [0x0450] = 0x27EDCB8,
        [0x0451] = 0x27EDCB8,
        [0x0500] = 0x291DD00, [0x0502] = 0x291DD00, [0x0510] = 0x291DD00,
        [0x0550] = 0x291DD00,
        [0x0600] = 0x2869D20, [0x0602] = 0x2869D20, [0x0650] = 0x2869D20,
        [0x0700] = 0x2859D50, [0x0720] = 0x2859D50, [0x0740] = 0x2859D50,
        [0x0760] = 0x2859D50, [0x0761] = 0x2859D50,
        [0x0800] = 0x2875D50, [0x0820] = 0x2875D50, [0x0840] = 0x2875D50,
        [0x0860] = 0x2875D50,
        [0x0900] = 0x2755D50,
        [0x0905] = 0x2755D50, [0x0920] = 0x2755D50, [0x0940] = 0x2755D50,
        [0x0960] = 0x2755D50,
        [0x1000] = 0x2765D70, [0x1001] = 0x2765D70, [0x1020] = 0x2765D70,
        [0x1040] = 0x2765D70, [0x1060] = 0x2765D70,
        [0x1100] = 0x2875D70, [0x1120] = 0x2875D70, [0x1140] = 0x2875D70,
        [0x1160] = 0x2875D70,
        [0x1200] = 0x2885E00, [0x1202] = 0x2885E00, [0x1220] = 0x2885E00,
        [0x1240] = 0x2885E00, [0x1260] = 0x2885E00, [0x1270] = 0x2885E00,
        [0x1300] = 0x28C5E00, [0x1320] = 0x28C5E00,
    };

    // NID encoder (matches ps5-payload-sdk/crt/nid.c)
    private static string NidEncode(string sym)
    {
        byte[] salt = [0x51, 0x8D, 0x64, 0xA6, 0x35, 0xDE, 0xD8, 0xC1,
                       0xE6, 0xB0, 0x39, 0xB1, 0xC3, 0xE5, 0x52, 0x30];
        const string charset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+-";

        byte[] input = Encoding.ASCII.GetBytes(sym).Concat(salt).ToArray();
        byte[] digest = SHA1.HashData(input);

        Array.Reverse(digest, 0, 8);
        Array.Clear(digest, 8, 8);

        var chars = new char[12];
        int ci = 0, di = 0;
        while (ci < 12)
        {
            int a = digest[di++], b = digest[di++], c = digest[di++];
            int t = (a << 16) | (b << 8) | c;
            chars[ci++] = charset[(t >> 18) & 0x3F];
            chars[ci++] = charset[(t >> 12) & 0x3F];
            chars[ci++] = charset[(t >> 6) & 0x3F];
            chars[ci++] = charset[t & 0x3F];
        }
        return new string(chars, 0, 11);
    }

    // Kernel memory helpers
    private static ulong Kr8(PS4DBG ps4, ulong addr) =>
        BitConverter.ToUInt64(ps4.KernelReadMemory(addr, 8), 0);

    private static uint Kr4(PS4DBG ps4, ulong addr) =>
        BitConverter.ToUInt32(ps4.KernelReadMemory(addr, 4), 0);

    private static int FwRawToHex(int fwRaw) =>
        int.Parse(fwRaw.ToString("D4"), System.Globalization.NumberStyles.HexNumber);

    /// <summary>
    /// Resolve function offsets for all required modules via kernel dynlib walk.
    /// Returns a dictionary: module name → (function name → offset).
    /// </summary>
    public static Dictionary<string, Dictionary<string, ulong>> ResolveFromKernel(
        PS4DBG ps4, int pid, int fwRaw)
    {
        int fwHex = FwRawToHex(fwRaw);
        if (!AllprocOffsets.TryGetValue(fwHex, out uint allprocOff))
            throw new Exception($"Unsupported firmware 0x{fwHex:X4} for kernel walk");

        ulong kdata = ps4.KernelBase();
        ulong allproc = kdata + allprocOff;

        // Walk allproc to find kproc for our PID
        ulong cur = Kr8(ps4, allproc);
        ulong kproc = 0;
        while (cur != 0)
        {
            if (Kr4(ps4, cur + 0xBC) == (uint)pid)
            {
                kproc = cur;
                break;
            }
            cur = Kr8(ps4, cur);
        }
        if (kproc == 0)
            throw new Exception($"PID {pid} not found in allproc");

        // Cache the full dynlib list (walk once)
        ulong dynlibHead = Kr8(ps4, kproc + 0x3E8);
        var dynlibs = new List<(ulong objAddr, ulong mapbase)>();
        cur = Kr8(ps4, dynlibHead);
        while (cur != 0)
        {
            ulong mb = Kr8(ps4, cur + 0x30);
            dynlibs.Add((cur, mb));
            cur = Kr8(ps4, cur);
        }

        // Get process maps to match module names to mapbase
        var maps = ps4.GetProcessMaps(pid);

        var result = new Dictionary<string, Dictionary<string, ulong>>();

        foreach (var (moduleName, funcNames) in RequiredFunctions)
        {
            var entry = maps.FindEntry(moduleName + ".sprx");
            if (entry == null) continue;

            ulong modBase = entry.start;

            // Find dynlib_obj by mapbase
            ulong foundObj = 0;
            foreach (var (objAddr, mb) in dynlibs)
            {
                if (mb == modBase)
                {
                    foundObj = objAddr;
                    break;
                }
            }
            if (foundObj == 0) continue;

            // Read dynsec
            ulong dynsecPtr = Kr8(ps4, foundObj + 0x148);
            ulong symKaddr = Kr8(ps4, dynsecPtr + 0x28);
            ulong symSize = Kr8(ps4, dynsecPtr + 0x30);
            ulong strKaddr = Kr8(ps4, dynsecPtr + 0x38);
            ulong strSize = Kr8(ps4, dynsecPtr + 0x40);

            if (symSize > 0x200000 || strSize > 0x200000 || symSize == 0)
                continue;

            byte[] symtab = ps4.KernelReadMemory(symKaddr, (int)symSize);
            byte[] strtab = ps4.KernelReadMemory(strKaddr, (int)strSize);

            // Build NID lookup
            var nidMap = new Dictionary<string, string>();
            foreach (string fn in funcNames)
                nidMap[NidEncode(fn)] = fn;

            var found = new Dictionary<string, ulong>();
            int nSyms = (int)(symSize / 24);
            for (int i = 0; i < nSyms; i++)
            {
                int off = i * 24;
                uint stName = BitConverter.ToUInt32(symtab, off);
                ulong stValue = BitConverter.ToUInt64(symtab, off + 8);
                if (stName == 0 || stValue == 0 || stName >= strtab.Length)
                    continue;

                int end = Array.IndexOf(strtab, (byte)0, (int)stName);
                if (end < 0) continue;

                string symName = Encoding.ASCII.GetString(strtab, (int)stName, end - (int)stName);
                string nidPart = symName.Length >= 11 ? symName[..11] : symName;

                if (nidMap.TryGetValue(nidPart, out string funcName))
                    found[funcName] = stValue; // offset from module base
            }

            result[moduleName] = found;
        }

        return result;
    }

    /// <summary>
    /// Load cached offsets for a firmware version, or null if not cached.
    /// Values are stored as hex strings ("0x72BF0") in JSON for readability.
    /// </summary>
    public static Dictionary<string, Dictionary<string, ulong>> LoadCache(string fwVersion)
    {
        if (!File.Exists(CacheFile))
            return null;

        try
        {
            string json = File.ReadAllText(CacheFile);
            var allCache = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(json);
            if (allCache != null && allCache.TryGetValue(fwVersion, out var cached))
            {
                var result = new Dictionary<string, Dictionary<string, ulong>>();
                foreach (var (module, funcs) in cached)
                {
                    var moduleDict = new Dictionary<string, ulong>();
                    foreach (var (func, hexVal) in funcs)
                        moduleDict[func] = Convert.ToUInt64(hexVal, 16);
                    result[module] = moduleDict;
                }
                return result;
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Save offsets to the cache file for a firmware version.
    /// Values are stored as hex strings ("0x72bf0") in JSON for readability.
    /// </summary>
    public static void SaveCache(string fwVersion, Dictionary<string, Dictionary<string, ulong>> offsets)
    {
        Dictionary<string, Dictionary<string, Dictionary<string, string>>> allCache;

        if (File.Exists(CacheFile))
        {
            try
            {
                string existing = File.ReadAllText(CacheFile);
                allCache = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(existing) ?? [];
            }
            catch
            {
                allCache = [];
            }
        }
        else
        {
            allCache = [];
        }

        var hexOffsets = new Dictionary<string, Dictionary<string, string>>();
        foreach (var (module, funcs) in offsets)
        {
            var hexFuncs = new Dictionary<string, string>();
            foreach (var (func, val) in funcs)
                hexFuncs[func] = $"0x{val:x}";
            hexOffsets[module] = hexFuncs;
        }

        allCache[fwVersion] = hexOffsets;

        string json = JsonSerializer.Serialize(allCache, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(CacheFile, json);
    }

    /// <summary>
    /// Get offsets for a firmware version — from cache if available, otherwise resolve via kernel walk and cache.
    /// </summary>
    public static Dictionary<string, Dictionary<string, ulong>> GetOffsets(
        PS4DBG ps4, int pid, int fwRaw, string fwVersion)
    {
        var cached = LoadCache(fwVersion);
        if (cached != null)
            return cached;

        var resolved = ResolveFromKernel(ps4, pid, fwRaw);
        SaveCache(fwVersion, resolved);
        return resolved;
    }

    /// <summary>
    /// Look up a single function offset. Returns 0 if not found.
    /// </summary>
    public static ulong GetFunctionOffset(
        Dictionary<string, Dictionary<string, ulong>> offsets,
        string moduleName, string functionName)
    {
        if (offsets.TryGetValue(moduleName, out var funcs))
            if (funcs.TryGetValue(functionName, out ulong offset))
                return offset;
        return 0;
    }
}
