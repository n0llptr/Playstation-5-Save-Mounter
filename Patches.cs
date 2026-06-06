using System;
using System.Collections.Generic;

namespace PS4Saves;
public class Patch
{
    public ulong Offset { get; }
    public byte[] Bytes { get; }
    public byte[] OriginalBytes { get; set; }

    public Patch(ulong offset, byte[] bytes)
    {
        Offset = offset;
        Bytes = bytes;
        OriginalBytes = null;
    }
}

public static class Patches
{
    private static readonly Dictionary<string, List<Patch>> FirmwarePatches = new()
    {
        ["3.20"] =
        [
            new(0x1493043, [0x00]), // 'sce_sdmemory' patch
            new(0x148A099, [0x00]), // 'sce_sdmemory1' patch
            new(0x148A0A7, [0x00]), // 'sce_sdmemory2' patch
            new(0x15246EF, [0x00]), // 'sce_sdmemory3' patch
            new(0xB5FAF0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x0FCFB0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x181600, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x10061C, [0xCD]), // ^ (thanks to GRModSave_Username) different patch
            new(0x0FF359, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x0FD33C, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x0FCA14, [0x90, 0x90]), // nevah jump
            new(0x0FCCC3, [0x90, 0xE9]), // always jump
        ],
        ["4.03"] =
        [
            new(0x172DFBA, [0x00]), // 'sce_sdmemory' patch
            new(0x17255B6, [0x00]), // 'sce_sdmemory1' patch
            new(0x17255C4, [0x00]), // 'sce_sdmemory2' patch
            new(0x17BE5A4, [0x00]), // 'sce_sdmemory3' patch
            new(0xB04C50, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x102C80, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x18BF30, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x10625D, [0xCD]), // ^ (thanks to GRModSave_Username) different patch
            new(0x104FA9, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x10300C, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x1026E4, [0x90, 0x90]), // nevah jump
            new(0x102993, [0x90, 0xE9]), // always jump
        ],
        ["5.00"] =
        [
            new(0x183646E, [0x00]), // 'sce_sdmemory' patch
            new(0x17ED1C2, [0x00]), // 'sce_sdmemory1' patch
            new(0x17ED1D0, [0x00]), // 'sce_sdmemory2' patch
            new(0x18CE377, [0x00]), // 'sce_sdmemory3' patch
            new(0xBCE660, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x113790, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1AAE40, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x117624, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x115CE8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x113B1C, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x1131F4, [0x90, 0x90]), // nevah jump
            new(0x1134A3, [0x90, 0xE9]), // always jump
        ],
        ["5.02"] =
        [
            new(0x1836258, [0x00]), // 'sce_sdmemory' patch
            new(0x182D32F, [0x00]), // 'sce_sdmemory1' patch
            new(0x182D33D, [0x00]), // 'sce_sdmemory2' patch
            new(0x18CE44C, [0x00]), // 'sce_sdmemory3' patch
            new(0xBCE640, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x113790, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1AAE40, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x117624, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x115CE8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x113B1C, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x1131F4, [0x90, 0x90]), // nevah jump
            new(0x1134A3, [0x90, 0xE9]), // always jump
        ],
        ["5.10"] =
        [
            new(0x183A869, [0x00]), // 'sce_sdmemory' patch
            new(0x1831998, [0x00]), // 'sce_sdmemory1' patch
            new(0x18319A6, [0x00]), // 'sce_sdmemory2' patch
            new(0x18D2B23, [0x00]), // 'sce_sdmemory3' patch
            new(0xBD1320, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1138E0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1AAF90, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x117774, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x115E38, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x113C6C, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x113344, [0x90, 0x90]), // nevah jump
            new(0x1135F3, [0x90, 0xE9]), // always jump
        ],
        ["5.50"] =
        [
            new(0x183F338, [0x00]), // 'sce_sdmemory' patch
            new(0x18366E9, [0x00]), // 'sce_sdmemory1' patch
            new(0x18366F7, [0x00]), // 'sce_sdmemory2' patch
            new(0x18D792E, [0x00]), // 'sce_sdmemory3' patch
            new(0xBD1D60, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1138E0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1AAF90, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x117774, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x115E38, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x113C6C, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x113344, [0x90, 0x90]), // nevah jump
            new(0x1135F3, [0x90, 0xE9]), // always jump
        ],
        // TODO: 6.00 - no shellcore dump available, likely same as 6.02
        ["6.02"] =
        [
            new(0x18F2B0F, [0x00]), // 'sce_sdmemory' patch
            new(0x18E94BD, [0x00]), // 'sce_sdmemory1' patch
            new(0x18E94CB, [0x00]), // 'sce_sdmemory2' patch
            new(0x198F654, [0x00]), // 'sce_sdmemory3' patch
            new(0xC24780, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11FDD0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1BFB30, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x123B57, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x122228, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x12014E, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11F824, [0x90, 0x90]), // nevah jump
            new(0x11FAD2, [0x90, 0xE9]), // always jump
        ],
        ["6.50"] =
        [
            new(0x18F28E8, [0x00]), // 'sce_sdmemory' patch
            new(0x18E92B4, [0x00]), // 'sce_sdmemory1' patch
            new(0x18E92C2, [0x00]), // 'sce_sdmemory2' patch
            new(0x198F98C, [0x00]), // 'sce_sdmemory3' patch
            new(0xC24FB0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11FDD0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1BFB30, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x123B57, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x122228, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x12014E, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11F824, [0x90, 0x90]), // nevah jump
            new(0x11FAD2, [0x90, 0xE9]), // always jump
        ],
        ["7.00"] = // Contribution
        [
            new(0x1A9FFCB, [0x00]), // 'sce_sdmemory' patch
            new(0x1AFD524, [0x00]), // 'sce_sdmemory1' patch
            new(0x1AB4C8B, [0x00]), // 'sce_sdmemory2' patch
            new(0x1A5946E, [0x00]), // 'sce_sdmemory3' patch
            new(0xCF53E0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11CAB0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1C4150, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x120837, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x11EF08, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x11CE2E, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11C504, [0x90, 0x90]), // nevah jump
            new(0x11C7B2, [0x90, 0xE9]), // always jump
        ],
        ["7.01"] = // Contribution
        [
            new(0x1A9FFC9, [0x00]), // 'sce_sdmemory' patch
            new(0x1AFD8A3, [0x00]), // 'sce_sdmemory1' patch
            new(0x1AB4EE9, [0x00]), // 'sce_sdmemory2' patch
            new(0x1A5992F, [0x00]), // 'sce_sdmemory3' patch
            new(0xCF53E0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11CAB0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1C4150, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x120837, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x11EF08, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x11CE2E, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11C504, [0x90, 0x90]), // nevah jump
            new(0x11C7B2, [0x90, 0xE9]), // always jump
        ],
        ["7.20"] = // Contribution
        [
            new(0x1AA000A, [0x00]), // 'sce_sdmemory' patch
            new(0x1AFD520, [0x00]), // 'sce_sdmemory1' patch
            new(0x1AB4B9D, [0x00]), // 'sce_sdmemory2' patch
            new(0x1A596A8, [0x00]), // 'sce_sdmemory3' patch
            new(0xCF5CC0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11CAB0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1C4150, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x120837, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x11EF08, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x11CE2E, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11C504, [0x90, 0x90]), // nevah jump
            new(0x11C7B2, [0x90, 0xE9]), // always jump
        ],
        ["7.40"] =
        [
            new(0x1AAD12E, [0x00]), // 'sce_sdmemory' patch
            new(0x1B0A257, [0x00]), // 'sce_sdmemory1' patch
            new(0x1AC19ED, [0x00]), // 'sce_sdmemory2' patch
            new(0x1A66751, [0x00]), // 'sce_sdmemory3' patch
            new(0xD014D0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1204E0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1C8520, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x124267, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x122938, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x12085E, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11FF34, [0x90, 0x90]), // nevah jump
            new(0x1201E2, [0x90, 0xE9]), // always jump
        ],
        ["7.60"] =
        [
            new(0x1AB13CB, [0x00]), // 'sce_sdmemory' patch
            new(0x1B0E556, [0x00]), // 'sce_sdmemory1' patch
            new(0x1AC5B23, [0x00]), // 'sce_sdmemory2' patch
            new(0x1A6A937, [0x00]), // 'sce_sdmemory3' patch
            new(0xD049C0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1204E0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1C8520, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x124267, [0xE9, 0x0C, 0x00, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x122938, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x12085E, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11FF34, [0x90, 0x90]), // nevah jump
            new(0x1201E2, [0x90, 0xE9]), // always jump
        ],
        // TODO: 7.61 - no shellcore dump available, likely same as 7.60
        ["8.00"] =
        [
            new(0x1B14517, [0x00]), // 'sce_sdmemory' patch
            new(0x1B74DC4, [0x00]), // 'sce_sdmemory1' patch
            new(0x1B2A4F3, [0x00]), // 'sce_sdmemory2' patch
            new(0x1ACAC6A, [0x00]), // 'sce_sdmemory3' patch
            new(0xD5EDD0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11F140, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1D25F0, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x122E3A, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x121528, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x122211, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11EBB4, [0x90, 0x90]), // nevah jump
            new(0x11EE57, [0x90, 0xE9]), // always jump
        ],
        ["8.20"] =
        [
            new(0x1B21F98, [0x00]), // 'sce_sdmemory' patch
            new(0x1B8298F, [0x00]), // 'sce_sdmemory1' patch
            new(0x1B37B2D, [0x00]), // 'sce_sdmemory2' patch
            new(0x1AD8335, [0x00]), // 'sce_sdmemory3' patch
            new(0xD66930, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11F1B0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1D2660, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x122EAA, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x121598, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x122281, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11EC24, [0x90, 0x90]), // nevah jump
            new(0x11EEC7, [0x90, 0xE9]), // always jump
        ],
        ["8.40"] =
        [
            new(0x1B219EF, [0x00]), // 'sce_sdmemory' patch
            new(0x1B823EC, [0x00]), // 'sce_sdmemory1' patch
            new(0x1B3770B, [0x00]), // 'sce_sdmemory2' patch
            new(0x1AD7E63, [0x00]), // 'sce_sdmemory3' patch
            new(0xD66930, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11F1B0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1D2660, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x122EAA, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x121598, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x122281, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11EC24, [0x90, 0x90]), // nevah jump
            new(0x11EEC7, [0x90, 0xE9]), // always jump
        ],
        ["8.60"] =
        [
            new(0x1B2635B, [0x00]), // 'sce_sdmemory' patch
            new(0x1B86AFF, [0x00]), // 'sce_sdmemory1' patch
            new(0x1B3BDED, [0x00]), // 'sce_sdmemory2' patch
            new(0x1ADC881, [0x00]), // 'sce_sdmemory3' patch
            new(0xD66930, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x11F1B0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1D2660, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x122EAA, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x121598, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x122281, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x11EC24, [0x90, 0x90]), // nevah jump
            new(0x11EEC7, [0x90, 0xE9]), // always jump
        ],
        ["9.00"] =
        [
            new(0x1BE318B, [0x00]), // 'sce_sdmemory' patch
            new(0x1B70D76, [0x00]), // 'sce_sdmemory1' patch
            new(0x1BF97B5, [0x00]), // 'sce_sdmemory2' patch
            new(0x1BE34E6, [0x00]), // 'sce_sdmemory3' patch
            new(0xDD0EC0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1265D0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1E4050, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x12A2CA, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x1289B8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x1296A5, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x126044, [0x90, 0x90]), // nevah jump
            new(0x1262E7, [0x90, 0xE9]), // always jump
        ],
        // TODO: 9.05 - no shellcore dump available, likely same as 9.00
        ["9.20"] =
        [
            new(0x1BE316E, [0x00]), // 'sce_sdmemory' patch
            new(0x1B70CCE, [0x00]), // 'sce_sdmemory1' patch
            new(0x1BF98E5, [0x00]), // 'sce_sdmemory2' patch
            new(0x1BE34C9, [0x00]), // 'sce_sdmemory3' patch
            new(0xDD0C00, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1265D0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1E4050, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x12A2CA, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x1289B8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x1296A5, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x126044, [0x90, 0x90]), // nevah jump
            new(0x1262E7, [0x90, 0xE9]), // always jump
        ],
        ["9.60"] =
        [
            new(0x1C18656, [0x00]), // 'sce_sdmemory' patch
            new(0x1C873BA, [0x00]), // 'sce_sdmemory1' patch
            new(0x1C02BB0, [0x00]), // 'sce_sdmemory2' patch
            new(0x1BEC89F, [0x00]), // 'sce_sdmemory3' patch
            new(0xDD9860, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1265F0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1E4720, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x12A2EA, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x1289D8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x1296C5, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x126064, [0x90, 0x90]), // nevah jump
            new(0x126307, [0x90, 0xE9]), // always jump
        ],
        // TODO: 10.00 - no shellcore dump available, likely same as 10.01
        ["10.01"] =
        [
            new(0x1C1B5DA, [0x00]), // 'sce_sdmemory' patch
            new(0x1C8DC60, [0x00]), // 'sce_sdmemory1' patch
            new(0x1C05075, [0x00]), // 'sce_sdmemory2' patch
            new(0x1BEE286, [0x00]), // 'sce_sdmemory3' patch
            new(0xDCF260, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1242D0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1E1C50, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x127FAE, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x1266F8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x12736B, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x123D74, [0x90, 0x90]), // nevah jump
            new(0x123FF2, [0x90, 0xE9]), // always jump
        ],
        ["10.20"] =
        [
            new(0x1C200B5, [0x00]), // 'sce_sdmemory' patch
            new(0x1C9267C, [0x00]), // 'sce_sdmemory1' patch
            new(0x1C0974A, [0x00]), // 'sce_sdmemory2' patch
            new(0x1BF2AF2, [0x00]), // 'sce_sdmemory3' patch
            new(0xDD3520, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1242D0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1E1C50, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x127FAE, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x1266F8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x12736B, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x123D74, [0x90, 0x90]), // nevah jump
            new(0x123FF2, [0x90, 0xE9]) // always jump
        ],
        ["10.40"] =
        [
            new(0x1C1FF88, [0x00]), // 'sce_sdmemory' patch
            new(0x1C92759, [0x00]), // 'sce_sdmemory1' patch
            new(0x1C0999A, [0x00]), // 'sce_sdmemory2' patch
            new(0x1BF2C16, [0x00]), // 'sce_sdmemory3' patch
            new(0xDD3540, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x1242D0, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1E1C50, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x127FAE, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x1266F8, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x12736B, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x123D74, [0x90, 0x90]), // nevah jump
            new(0x123FF2, [0x90, 0xE9]) // always jump
        ],
        ["10.60"] =
        [
            new(0x1C2472A, [0x00]), // 'sce_sdmemory' patch
            new(0x1C96729, [0x00]), // 'sce_sdmemory1' patch
            new(0x1C0E1CF, [0x00]), // 'sce_sdmemory2' patch
            new(0x1BF7531, [0x00]), // 'sce_sdmemory3' patch
            new(0xDD4DE0, [0x48, 0x31, 0xC0, 0xC3]), // verify keystone patch
            new(0x124330, [0x31, 0xC0, 0xC3]), // transfer mount permission patch eg mount foreign saves with write permission
            new(0x1E1CB0, [0x31, 0xC0, 0xC3]), // patch psn check to load saves saves foreign to current account
            new(0x12800E, [0xE9, 0x05, 0x00, 0x00]), // ^ (thanks to GRModSave_Username) different patch
            new(0x126758, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // something something patches...
            new(0x1273CB, [0x90, 0x90, 0x90, 0x90, 0x90, 0x90]), // don't even remember doing this
            new(0x123DD4, [0x90, 0x90]), // nevah jump
            new(0x124052, [0x90, 0xE9]) // always jump
        ],
    };

    /// <summary>
    /// Retrieves the shellcore patches for a given firmware version.
    /// </summary>
    /// <param name="firmwareVersion">The firmware version string (e.g., "7.40").</param>
    /// <returns>A list of Patch objects for shellcore, or an empty list if no patches are defined for the version.</returns>
    public static List<Patch> GetShellcorePatches(string firmwareVersion)
    {
        if (FirmwarePatches.TryGetValue(firmwareVersion, out var patches))
            return patches;
        return [];
    }
}
