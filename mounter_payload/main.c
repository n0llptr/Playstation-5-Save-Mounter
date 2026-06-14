// PS5 Save Mounter Payload
// Runs on-console via elfldr, provides a TCP command server for the PC app.
// Based on savemounter.c PFS approach and ps5-payload-dev SDK.
// Save creation based on garlic-savemgr by earthonion.
// FS functions linked from libSceFsInternalForVsh via SDK stubs.

#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <string.h>
#include <unistd.h>
#include <fcntl.h>
#include <dirent.h>
#include <errno.h>
#include <stdint.h>
#include <sys/stat.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <netinet/in.h>
#include <ps5/kernel.h>

#define MOUNTER_PORT 9090
#define MAX_CMD_LEN  1024
#define SEND_CHUNK   (512 * 1024)

static int copy_file(const char *src, const char *dst);

// Track mounted state for copy-back on unmount
static char g_original_path[512] = {0};
static char g_local_copy[512] = {0};
static char g_mount_point[256] = {0};
static int g_mounted = 0;

// PFS save data structures (from savemounter.c)
typedef struct { int blockSize; uint8_t flags[2]; } CreateOpt;
typedef struct { uint8_t reserved; char *budgetid; } MountOpt;
typedef struct { uint8_t dummy; } UmountOpt;

// FS functions from libSceFsInternalForVsh (linked via SDK stubs)
int sceFsInitCreatePfsSaveDataOpt(CreateOpt *opt);
int sceFsCreatePfsSaveDataImage(CreateOpt *opt, const char *path, int x,
                                 uint64_t size, uint8_t *key);
int sceFsCreatePprPfsSaveDataImage(CreateOpt *opt, const char *path, int x,
                                    uint64_t size, uint8_t *key);
int sceFsInitMountSaveDataOpt(MountOpt *opt);
int sceFsMountSaveData(MountOpt *opt, const char *path,
                        const char *mount, uint8_t *key);
int sceFsInitUmountSaveDataOpt(UmountOpt *opt);
int sceFsUmountSaveData(UmountOpt *opt, const char *mount,
                          int handle, int ignore);
int sceFsUfsAllocateSaveData(int fd, uint64_t size, uint64_t flags, int ext);

// User service
int sceUserServiceInitialize(void *);
int sceUserServiceGetLoginUserIdList(int *list);
int sceUserServiceGetUserName(int userId, char *name, size_t nameSize);

// Notification
typedef struct { char unused[45]; char message[3075]; } notify_request_t;
int sceKernelSendNotificationRequest(int, notify_request_t *, size_t, int);

static void notify(const char *fmt, ...) {
    notify_request_t req;
    memset(&req, 0, sizeof(req));
    va_list ap;
    va_start(ap, fmt);
    vsnprintf(req.message, sizeof(req.message), fmt, ap);
    va_end(ap);
    sceKernelSendNotificationRequest(0, &req, sizeof(req), 0);
}

// ── TCP helpers ─────────────────────────────────────────────

static int send_all(int fd, const void *buf, size_t len) {
    const uint8_t *p = buf;
    while (len > 0) {
        ssize_t n = send(fd, p, len, 0);
        if (n <= 0) return -1;
        p += n;
        len -= n;
    }
    return 0;
}

// ── param.sfo parser ────────────────────────────────────────

typedef struct {
    char title[128];
    char subtitle[128];
    char detail[256];
} sfo_data_t;

static int parse_sfo(const char *sfo_path, sfo_data_t *out) {
    memset(out, 0, sizeof(*out));
    int fd = open(sfo_path, O_RDONLY);
    if (fd < 0) return -1;

    uint8_t hdr[20];
    if (read(fd, hdr, 20) != 20) { close(fd); return -1; }
    if (hdr[0] != 0 || hdr[1] != 'P' || hdr[2] != 'S' || hdr[3] != 'F') {
        close(fd);
        return -1;
    }

    uint32_t key_tbl  = *(uint32_t *)(hdr + 8);
    uint32_t data_tbl = *(uint32_t *)(hdr + 12);
    uint32_t n_ent    = *(uint32_t *)(hdr + 16);

    // Clamp the (untrusted) entry count so a malformed SFO can't drive a
    // multi-billion iteration loop with several preads each.
    if (n_ent > 1024) n_ent = 1024;

    for (uint32_t i = 0; i < n_ent; i++) {
        uint8_t ent[16];
        if (pread(fd, ent, 16, 20 + i * 16) != 16) break;

        uint16_t koff  = *(uint16_t *)(ent + 0);
        uint16_t fmt   = *(uint16_t *)(ent + 2);
        uint32_t dused = *(uint32_t *)(ent + 4);
        uint32_t doff  = *(uint32_t *)(ent + 12);

        char kname[32] = {0};
        if (pread(fd, kname, 31, key_tbl + koff) <= 0) continue;
        kname[31] = 0;

        if (fmt != 0x0204) continue; // only read UTF-8 strings

        char val[256] = {0};
        size_t rd = dused < sizeof(val) ? dused : sizeof(val) - 1;
        if (pread(fd, val, rd, data_tbl + doff) <= 0) continue;

        if (strcmp(kname, "MAINTITLE") == 0 || strcmp(kname, "TITLE") == 0)
            strncpy(out->title, val, sizeof(out->title) - 1);
        else if (strcmp(kname, "SUBTITLE") == 0)
            strncpy(out->subtitle, val, sizeof(out->subtitle) - 1);
        else if (strcmp(kname, "DETAIL") == 0)
            strncpy(out->detail, val, sizeof(out->detail) - 1);
    }
    close(fd);
    return 0;
}

static void sendf(int fd, const char *fmt, ...) {
    char buf[4096];
    va_list ap;
    va_start(ap, fmt);
    int len = vsnprintf(buf, sizeof(buf), fmt, ap);
    va_end(ap);
    if (len > 0) send_all(fd, buf, len);
}

static int recv_line(int fd, char *buf, int maxlen) {
    int pos = 0;
    while (pos < maxlen - 1) {
        char c;
        ssize_t n = recv(fd, &c, 1, 0);
        if (n <= 0) return -1;
        if (c == '\n') break;
        if (c == '\r') continue;
        buf[pos++] = c;
    }
    buf[pos] = 0;
    return pos;
}

// Parse a hex user id from a command argument. Returns 0 on success.
static int parse_uid(const char *uid_hex, unsigned int *uid) {
    return (uid_hex && sscanf(uid_hex, "%x", uid) == 1) ? 0 : -1;
}

// ── Command handlers ────────────────────────────────────────

static void cmd_get_fw(int c) {
    uint32_t fw = kernel_get_fw_version();
    sendf(c, "OK %x.%02x\n", (fw >> 24) & 0xFF, (fw >> 16) & 0xFF);
}

static void cmd_get_users(int c) {
    int ids[4];
    memset(ids, -1, sizeof(ids));
    sceUserServiceGetLoginUserIdList(ids);

    int cnt = 0;
    for (int i = 0; i < 4; i++)
        if (ids[i] != -1) cnt++;

    sendf(c, "OK %d\n", cnt);
    for (int i = 0; i < 4; i++) {
        if (ids[i] == -1) continue;
        char name[17] = {0};
        sceUserServiceGetUserName(ids[i], name, sizeof(name));
        sendf(c, "%x %s\n", ids[i], name);
    }
}

static void scan_save_dir(const char *path, char titles[][16], int *cnt) {
    DIR *d = opendir(path);
    if (!d) return;
    struct dirent *ent;
    while ((ent = readdir(d)) != NULL && *cnt < 512) {
        if (ent->d_name[0] == '.') continue;
        if (strncmp(ent->d_name, "CUSA", 4) == 0 ||
            strncmp(ent->d_name, "PPSA", 4) == 0) {
            strncpy(titles[*cnt], ent->d_name, 15);
            titles[*cnt][15] = 0;
            (*cnt)++;
        }
    }
    closedir(d);
}

static void cmd_list_saves(int c, const char *uid_hex) {
    unsigned int uid;
    if (parse_uid(uid_hex, &uid) != 0) { sendf(c, "ERR bad uid\n"); return; }

    char (*titles)[16] = calloc(512, 16);
    if (!titles) { sendf(c, "ERR out of memory\n"); return; }
    int cnt = 0;

    char path[256];
    snprintf(path, sizeof(path), "/user/home/%x/savedata/", uid);
    scan_save_dir(path, titles, &cnt);

    snprintf(path, sizeof(path), "/user/home/%x/savedata_prospero/", uid);
    scan_save_dir(path, titles, &cnt);

    sendf(c, "OK %d\n", cnt);
    for (int i = 0; i < cnt; i++)
        sendf(c, "%s\n", titles[i]);
    free(titles);
}

static void cmd_search(int c, const char *uid_hex, const char *title_id) {
    unsigned int uid;
    if (parse_uid(uid_hex, &uid) != 0) { sendf(c, "ERR bad uid\n"); return; }

    typedef struct {
        char name[64];
        char title[128];
        char subtitle[128];
        char detail[256];
        int64_t mtime;
    } entry_t;

    entry_t *entries = calloc(256, sizeof(entry_t));
    if (!entries) { sendf(c, "ERR out of memory\n"); return; }
    int cnt = 0;

    // Scan PS4 savedata (sdimg_ files)
    char base[256];
    snprintf(base, sizeof(base), "/user/home/%x/savedata/%s/", uid, title_id);
    DIR *d = opendir(base);
    if (d) {
        struct dirent *ent;
        while ((ent = readdir(d)) != NULL && cnt < 256) {
            if (ent->d_name[0] == '.') continue;
            int nlen = strlen(ent->d_name);
            if (nlen > 4 && strcmp(ent->d_name + nlen - 4, ".bin") == 0) continue;
            if (strncmp(ent->d_name, "sdimg_sce_bu_", 13) == 0) continue;

            const char *save_name = ent->d_name;
            if (strncmp(save_name, "sdimg_", 6) == 0)
                save_name += 6;
            if (strncmp(save_name, "sce_bu_", 7) == 0) continue;
            strncpy(entries[cnt].name, save_name, 63);
            entries[cnt].name[63] = 0;

            char filepath[512];
            snprintf(filepath, sizeof(filepath), "%s%s", base, ent->d_name);
            struct stat st;
            if (stat(filepath, &st) == 0)
                entries[cnt].mtime = st.st_mtime;
            cnt++;
        }
        closedir(d);
    }

    // PS4 metadata is enriched on the C# side via savedata.db

    // Scan PS5 savedata_prospero — with SFO metadata from savedata_prospero_meta
    snprintf(base, sizeof(base), "/user/home/%x/savedata_prospero/%s/", uid, title_id);
    d = opendir(base);
    if (d) {
        struct dirent *ent;
        while ((ent = readdir(d)) != NULL && cnt < 256) {
            if (ent->d_name[0] == '.') continue;
            int nlen = strlen(ent->d_name);
            if (nlen > 4 && strcmp(ent->d_name + nlen - 4, ".bin") == 0) continue;
            if (strncmp(ent->d_name, "sdimg_sce_bu_", 13) == 0) continue;

            // Strip sdimg_ prefix for display name
            const char *save_name = ent->d_name;
            if (strncmp(save_name, "sdimg_", 6) == 0)
                save_name += 6;
            strncpy(entries[cnt].name, save_name, 63);
            entries[cnt].name[63] = 0;

            // Read metadata from SFO: <saveName>.sfo in savedata_prospero_meta
            char sfo_path[512];
            snprintf(sfo_path, sizeof(sfo_path),
                     "/user/home/%x/savedata_prospero_meta/user/%s/%s.sfo",
                     uid, title_id, save_name);
            sfo_data_t sfo;
            if (parse_sfo(sfo_path, &sfo) != 0) {
                // Fallback: try sce_bu_ prefixed SFO
                snprintf(sfo_path, sizeof(sfo_path),
                         "/user/home/%x/savedata_prospero_meta/user/%s/sce_bu_%s.sfo",
                         uid, title_id, save_name);
                parse_sfo(sfo_path, &sfo);
            }
            // If SFO has both title+subtitle, use both.
            // If only title (no subtitle), move it to subtitle so
            // the UI keeps the game name from GET_NAME as the title.
            if (sfo.title[0]) {
                if (sfo.subtitle[0]) {
                    strncpy(entries[cnt].title, sfo.title, 127);
                    strncpy(entries[cnt].subtitle, sfo.subtitle, 127);
                } else {
                    strncpy(entries[cnt].subtitle, sfo.title, 127);
                }
                strncpy(entries[cnt].detail, sfo.detail, 255);
            }

            char filepath[512];
            snprintf(filepath, sizeof(filepath), "%s%s", base, ent->d_name);
            struct stat st;
            if (stat(filepath, &st) == 0)
                entries[cnt].mtime = st.st_mtime;
            cnt++;
        }
        closedir(d);
    }

    if (cnt == 0) {
        sendf(c, "OK 0\n");
        free(entries);
        return;
    }

    sendf(c, "OK %d\n", cnt);
    for (int i = 0; i < cnt; i++) {
        sendf(c, "%s\t%s\t%s\t%s\t%lld\n",
              entries[i].name, entries[i].title,
              entries[i].subtitle, entries[i].detail,
              (long long)entries[i].mtime);
    }
    free(entries);
}

// Send file contents in chunks
static int send_file_chunked(int c, FILE *f, long sz) {
    uint8_t *buf = malloc(SEND_CHUNK);
    if (!buf) return -1;
    long remaining = sz;
    while (remaining > 0) {
        size_t to_read = remaining < SEND_CHUNK ? remaining : SEND_CHUNK;
        size_t nr = fread(buf, 1, to_read, f);
        if (nr == 0) break;
        if (send_all(c, buf, nr) < 0) { free(buf); return -1; }
        remaining -= nr;
    }
    free(buf);
    return 0;
}

static void cmd_read_file(int c, const char *path) {
    FILE *f = fopen(path, "rb");
    if (!f) { sendf(c, "ERR %s\n", strerror(errno)); return; }

    fseek(f, 0, SEEK_END);
    long sz = ftell(f);
    fseek(f, 0, SEEK_SET);

    if (sz <= 0) { fclose(f); sendf(c, "ERR bad size\n"); return; }

    sendf(c, "OK %ld\n", sz);
    if (send_file_chunked(c, f, sz) < 0)
        printf("[mounter] read_file send failed\n");
    fclose(f);
}

static int copy_file(const char *src, const char *dst) {
    int sfd = open(src, O_RDONLY);
    if (sfd < 0) return -1;
    int dfd = open(dst, O_CREAT | O_WRONLY | O_TRUNC, 0755);
    if (dfd < 0) { close(sfd); return -2; }
    char *buf = malloc(512 * 1024);
    if (!buf) { close(sfd); close(dfd); return -3; }
    int rc = 0;
    ssize_t n;
    while ((n = read(sfd, buf, 512 * 1024)) > 0) {
        // Loop on partial writes; bail out on any write error (e.g. disk full)
        // so we never silently produce a truncated copy.
        ssize_t off = 0;
        while (off < n) {
            ssize_t w = write(dfd, buf + off, n - off);
            if (w <= 0) { rc = -4; goto done; }
            off += w;
        }
    }
    if (n < 0) rc = -5; // read error
done:
    free(buf);
    close(sfd);
    close(dfd);
    return rc;
}

// Shared unmount + copy-back + cleanup. Returns 0 on success.
//   force == 0: an unmount failure aborts and leaves the mount state intact
//               (so the caller can report it and the user can retry).
//   force != 0: tear down unconditionally (best-effort) and always clear state.
// On copy-back failure returns -1, keeps g_local_copy, and still tears down.
// If errbuf is non-NULL it receives the error text; otherwise errors are
// surfaced via notify() (used by the forced/background path).
static int do_unmount(int force, char *errbuf, size_t errlen) {
    if (!g_mounted) return 0;

    UmountOpt uopt;
    memset(&uopt, 0, sizeof(uopt));
    sceFsInitUmountSaveDataOpt(&uopt);
    int ret = sceFsUmountSaveData(&uopt, g_mount_point, 0, 0);
    if (ret < 0 && !force) {
        if (errbuf) snprintf(errbuf, errlen, "umount 0x%08x", ret);
        return ret;
    }
    sync();

    int rc = 0;
    if (g_local_copy[0] && g_original_path[0]) {
        printf("[mounter] Copying back %s -> %s\n", g_local_copy, g_original_path);
        int cr = copy_file(g_local_copy, g_original_path);
        if (cr < 0) {
            // Keep the local copy so the save isn't lost on a failed copy-back.
            printf("[mounter] copy-back failed (%d), keeping %s\n", cr, g_local_copy);
            if (errbuf) snprintf(errbuf, errlen, "copy-back failed (%d), kept %s",
                                 cr, g_local_copy);
            else notify("Save copy-back FAILED (%d)\nKept: %s", cr, g_local_copy);
            rc = -1;
        } else {
            unlink(g_local_copy);
        }
    }

    if (rmdir(g_mount_point) < 0)
        printf("[mounter] rmdir %s: %s\n", g_mount_point, strerror(errno));

    g_mounted = 0;
    g_mount_point[0] = 0;
    g_original_path[0] = 0;
    g_local_copy[0] = 0;
    return rc;
}

static void auto_unmount(void) {
    do_unmount(1, NULL, 0);
}

static void cmd_mount(int c, const char *uid_hex,
                       const char *title_id, const char *dir_name) {
    unsigned int uid;
    if (parse_uid(uid_hex, &uid) != 0) { sendf(c, "ERR bad uid\n"); return; }

    auto_unmount();

    // Detect PS4 vs PS5 save location
    // PS4: /user/home/<uid>/savedata/<titleId>/sdimg_<name>
    // PS5: /user/home/<uid>/savedata_prospero/<titleId>/<name>
    char image[512];
    int is_ps4 = 0;

    // Try PS4 path first, then PS5
    snprintf(image, sizeof(image),
             "/user/home/%x/savedata/%s/sdimg_%s",
             uid, title_id, dir_name);
    int probe = open(image, O_RDONLY);
    if (probe >= 0) {
        close(probe);
        is_ps4 = 1;
    } else {
        // PS5 saves also use sdimg_ prefix
        snprintf(image, sizeof(image),
                 "/user/home/%x/savedata_prospero/%s/sdimg_%s",
                 uid, title_id, dir_name);
        probe = open(image, O_RDONLY);
        if (probe >= 0) {
            close(probe);
        } else {
            snprintf(image, sizeof(image),
                     "/user/home/%x/savedata_prospero/%s/%s",
                     uid, title_id, dir_name);
        }
    }

    printf("[mounter] MOUNT: image=%s is_ps4=%d\n", image, is_ps4);

    const char *bname = strrchr(image, '/');
    bname = bname ? bname + 1 : image;

    // Read key from original location (no copy needed for key reading)
    uint8_t *data = malloc(0x100);
    if (!data) { sendf(c, "ERR out of memory\n"); return; }
    memset(data, 0, 0x100);

    uint8_t decrypted_key[0x20];
    memset(decrypted_key, 0, sizeof(decrypted_key));

    if (is_ps4) {
        // PS4: read sealed key from companion .bin file
        const char *savename = (strncmp(bname, "sdimg_", 6) == 0) ? bname + 6 : bname;
        char bin_path[512];
        snprintf(bin_path, sizeof(bin_path),
                 "/user/home/%x/savedata/%s/%s.bin",
                 uid, title_id, savename);

        int fd = open(bin_path, O_RDONLY);
        if (fd < 0) {
            sendf(c, "ERR open sealed key %s: %s\n", bin_path, strerror(errno));
            free(data);
            return;
        }
        int r = read(fd, data, 0x60);
        close(fd);
        if (r != 0x60) {
            sendf(c, "ERR short read .bin (%d)\n", r);
            free(data);
            return;
        }
        printf("[mounter] PS4 sealed key read OK\n");
    } else {
        // PS5: read key from offset 0x800 in image
        int fd = open(image, O_RDONLY);
        if (fd < 0) {
            sendf(c, "ERR open %s: %s\n", image, strerror(errno));
            free(data);
            return;
        }
        int r = pread(fd, data, 0x60, 0x800);
        close(fd);
        if (r != 0x60) {
            sendf(c, "ERR read key (got %d): %s\n", r, strerror(errno));
            free(data);
            return;
        }
        printf("[mounter] PS5 key read OK\n");
    }

    // Decrypt via pfsmgr
    int pfsmgr = open("/dev/pfsmgr", O_RDWR);
    if (pfsmgr < 0) {
        sendf(c, "ERR open pfsmgr: %s\n", strerror(errno));
        free(data);
        return;
    }
    int ret = ioctl(pfsmgr, 0xc0845302, data);
    close(pfsmgr);
    if (ret < 0) {
        printf("[mounter] key decrypt failed (ret=%d)\n", ret);
        free(data);
        sendf(c, "ERR key decryption failed (ret=%d)\n", ret);
        return;
    }
    memcpy(decrypted_key, data + 0x60, 0x20);
    printf("[mounter] key decrypted OK\n");
    free(data);

    // Mount directly from original path
    if (mkdir("/mnt/pfs", 0777) != 0 && errno != EEXIST) {
        sendf(c, "ERR mkdir /mnt/pfs: %s\n", strerror(errno));
        return;
    }
    char mp[256];
    snprintf(mp, sizeof(mp), "/mnt/pfs/savedata_%x_%s_%s", uid, title_id, dir_name);
    mkdir(mp, 0777);

    MountOpt mopt;
    memset(&mopt, 0, sizeof(mopt));
    sceFsInitMountSaveDataOpt(&mopt);
    mopt.budgetid = "system";

    printf("[mounter] MOUNT: %s -> %s\n", image, mp);
    ret = sceFsMountSaveData(&mopt, image, mp, decrypted_key);
    printf("[mounter] MOUNT: ret=0x%08x\n", ret);

    if (ret < 0) {
        notify("MOUNT failed: 0x%08x", ret);
        rmdir(mp);
        sendf(c, "ERR mount 0x%08x\n", ret);
        return;
    }

    strncpy(g_mount_point, mp, sizeof(g_mount_point) - 1);
    g_mounted = 1;

    notify("MOUNT OK: %s", mp);
    sendf(c, "OK %s\n", mp);
}

static void cmd_umount(int c) {
    if (!g_mounted) {
        sendf(c, "ERR nothing mounted\n");
        return;
    }

    char err[256] = {0};
    if (do_unmount(0, err, sizeof(err)) < 0) {
        sendf(c, "ERR %s\n", err[0] ? err : "umount failed");
        return;
    }

    sendf(c, "OK\n");
}

// Save creation flow based on garlic-savemgr by earthonion
static void cmd_create(int c, const char *uid_hex, const char *title_id,
                        const char *dir_name, const char *blocks_str) {
    unsigned int uid;
    uint64_t blocks;
    if (parse_uid(uid_hex, &uid) != 0) { sendf(c, "ERR bad uid\n"); return; }
    blocks = strtoull(blocks_str, NULL, 10);

    // Bound the (network-supplied) block count so the size math below can't
    // overflow uint64_t and so we don't try to allocate an absurd image.
    // 8 GiB of data == 262144 blocks of 32 KiB, far above any real save.
    if (blocks == 0 || blocks > 262144) {
        sendf(c, "ERR bad block count\n");
        return;
    }

    // Calculate image size: blocks * 32KB + 25% overhead + 4MB, min 32MB
    uint64_t data_size = blocks * 32768ULL;
    uint64_t img_size = data_size + (data_size / 4) + (4 * 1024 * 1024);
    if (img_size < 32 * 1024 * 1024)
        img_size = 32 * 1024 * 1024;
    img_size = ((img_size + 32767) / 32768) * 32768;

    auto_unmount();

    mkdir("/data/save_files", 0777);
    const char *tmp = "/data/save_files/_tmp_enc";
    unlink(tmp);

    // Detect PS4 (CUSA) vs PS5 (PPSA)
    int is_ps4 = (strncmp(title_id, "CUSA", 4) == 0);
    uint8_t ckey[0x20] = {0};
    char bin_path[512] = {0};

    if (is_ps4) {
        // PS4: generate sealed key via sbl_srv/pfsmgr, then decrypt
        uint8_t *kbuf = malloc(0x100);
        if (!kbuf) { sendf(c, "ERR out of memory\n"); return; }
        memset(kbuf, 0, 0x100);

        int gen_ok = 0;
        int kfd = open("/dev/sbl_srv", O_RDWR);
        if (kfd >= 0) {
            if (ioctl(kfd, 0x40845303, kbuf) >= 0) gen_ok = 1;
            close(kfd);
        }
        if (!gen_ok) {
            memset(kbuf, 0, 0x100);
            kfd = open("/dev/pfsmgr", O_RDWR);
            if (kfd >= 0) {
                if (ioctl(kfd, 0x40845303, kbuf) >= 0) gen_ok = 1;
                close(kfd);
            }
        }
        if (!gen_ok) {
            free(kbuf);
            sendf(c, "ERR cannot generate sealed key\n");
            return;
        }

        // Decrypt sealed key via pfsmgr
        uint8_t sealed_copy[96];
        memcpy(sealed_copy, kbuf, 96);
        memset(kbuf, 0, 0x100);
        memcpy(kbuf, sealed_copy, 96);
        int pfsmgr = open("/dev/pfsmgr", O_RDWR);
        if (pfsmgr < 0) { free(kbuf); sendf(c, "ERR open pfsmgr\n"); return; }
        int ret = ioctl(pfsmgr, 0xc0845302, kbuf);
        close(pfsmgr);
        if (ret < 0) { free(kbuf); sendf(c, "ERR decrypt key\n"); return; }
        memcpy(ckey, kbuf + 0x60, 0x20);

        // Save sealed key as .bin companion
        char dir[512];
        snprintf(dir, sizeof(dir), "/user/home/%x/savedata/%s", uid, title_id);
        mkdir(dir, 0777);
        snprintf(bin_path, sizeof(bin_path), "%s/%s.bin", dir, dir_name);
        int bfd = open(bin_path, O_CREAT | O_WRONLY | O_TRUNC, 0755);
        if (bfd >= 0) { write(bfd, sealed_copy, 96); close(bfd); }

        free(kbuf);
        printf("[mounter] CREATE: PS4 key generated\n");
    }

    // Create and allocate image file
    int imgfd = open(tmp, O_CREAT | O_TRUNC | O_RDWR, 0777);
    if (imgfd < 0) {
        if (bin_path[0]) unlink(bin_path);
        sendf(c, "ERR create temp: %s\n", strerror(errno));
        return;
    }
    int ret = sceFsUfsAllocateSaveData(imgfd, img_size, 0, 0);
    if (ret < 0) {
        if (ftruncate(imgfd, img_size) < 0) {
            close(imgfd); unlink(tmp);
            if (bin_path[0]) unlink(bin_path);
            sendf(c, "ERR allocate image\n");
            return;
        }
    }
    close(imgfd);

    // Format PFS image
    CreateOpt copt;
    memset(&copt, 0, sizeof(copt));
    sceFsInitCreatePfsSaveDataOpt(&copt);

    if (is_ps4) {
        ret = sceFsCreatePfsSaveDataImage(&copt, tmp, 0, img_size, ckey);
    } else {
        copt.flags[1] = 0x02;
        ret = sceFsCreatePprPfsSaveDataImage(&copt, tmp, 0, img_size, ckey);
    }
    if (ret < 0) {
        unlink(tmp);
        if (bin_path[0]) unlink(bin_path);
        sendf(c, "ERR format PFS 0x%08x\n", ret);
        return;
    }

    // Mount the new image
    if (mkdir("/mnt/pfs", 0777) != 0 && errno != EEXIST) {
        unlink(tmp);
        if (bin_path[0]) unlink(bin_path);
        sendf(c, "ERR mkdir /mnt/pfs: %s\n", strerror(errno));
        return;
    }
    snprintf(g_mount_point, sizeof(g_mount_point),
             "/mnt/pfs/savedata_%x_%s_%s", uid, title_id, dir_name);
    mkdir(g_mount_point, 0777);

    MountOpt mopt;
    memset(&mopt, 0, sizeof(mopt));
    sceFsInitMountSaveDataOpt(&mopt);
    mopt.budgetid = "system";

    ret = sceFsMountSaveData(&mopt, tmp, g_mount_point, ckey);
    if (ret < 0) {
        unlink(tmp);
        if (bin_path[0]) unlink(bin_path);
        rmdir(g_mount_point);
        sendf(c, "ERR mount 0x%08x\n", ret);
        return;
    }

    // Copy image to final location after mount succeeds
    char final_path[512];
    if (is_ps4) {
        snprintf(final_path, sizeof(final_path),
                 "/user/home/%x/savedata/%s/sdimg_%s", uid, title_id, dir_name);
    } else {
        char dir[512];
        snprintf(dir, sizeof(dir), "/user/home/%x/savedata_prospero/%s",
                 uid, title_id);
        mkdir(dir, 0777);
        snprintf(final_path, sizeof(final_path), "%s/%s", dir, dir_name);
    }

    strncpy(g_original_path, final_path, sizeof(g_original_path) - 1);
    strncpy(g_local_copy, tmp, sizeof(g_local_copy) - 1);
    g_mounted = 1;

    printf("[mounter] CREATE: mounted at %s\n", g_mount_point);
    sendf(c, "OK %s\n", g_mount_point);
}

// ── Main ────────────────────────────────────────────────────

int main() {
    printf("[mounter] starting\n");

    // Privilege escalation
    pid_t me = getpid();
    kernel_set_ucred_authid(me, 0x4800000000000010);

    uint8_t caps[16];
    memset(caps, 0xFF, sizeof(caps));
    kernel_set_ucred_caps(me, caps);

    // Escape sandbox
    intptr_t rvnode = kernel_get_root_vnode();
    kernel_set_proc_rootdir(me, rvnode);
    kernel_set_proc_jaildir(me, 0);

    sceUserServiceInitialize(NULL);

    // Notification
    uint32_t fw = kernel_get_fw_version();
    notify("Save Mounter ready\nFW %x.%02x | port %d",
           (fw >> 24) & 0xFF, (fw >> 16) & 0xFF, MOUNTER_PORT);

    // TCP server
    int srv = socket(AF_INET, SOCK_STREAM, 0);
    if (srv < 0) { printf("[mounter] socket: %s\n", strerror(errno)); return -1; }

    int opt = 1;
    setsockopt(srv, SOL_SOCKET, SO_REUSEADDR, &opt, sizeof(opt));

    struct sockaddr_in addr = {0};
    addr.sin_family = AF_INET;
    addr.sin_port = htons(MOUNTER_PORT);
    addr.sin_addr.s_addr = INADDR_ANY;

    if (bind(srv, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        printf("[mounter] bind: %s\n", strerror(errno));
        close(srv);
        return -1;
    }
    listen(srv, 2);
    printf("[mounter] listening on port %d\n", MOUNTER_PORT);

    while (1) {
        int cl = accept(srv, NULL, NULL);
        if (cl < 0) continue;
        printf("[mounter] client connected\n");

        char line[MAX_CMD_LEN];
        while (recv_line(cl, line, sizeof(line)) >= 0) {
            printf("[mounter] < %s\n", line);

            char *a[8] = {0};
            int ac = 0;
            char *t = strtok(line, " ");
            while (t && ac < 8) { a[ac++] = t; t = strtok(NULL, " "); }
            if (ac == 0) continue;

            if      (strcmp(a[0], "PING") == 0)
                sendf(cl, "OK\n");
            else if (strcmp(a[0], "GET_FW") == 0)
                cmd_get_fw(cl);
            else if (strcmp(a[0], "GET_USERS") == 0)
                cmd_get_users(cl);
            else if (strcmp(a[0], "LIST_SAVES") == 0 && ac >= 2)
                cmd_list_saves(cl, a[1]);
            else if (strcmp(a[0], "SEARCH") == 0 && ac >= 3)
                cmd_search(cl, a[1], a[2]);
            else if (strcmp(a[0], "MOUNT") == 0 && ac >= 4)
                cmd_mount(cl, a[1], a[2], a[3]);
            else if (strcmp(a[0], "UMOUNT") == 0)
                cmd_umount(cl);
            else if (strcmp(a[0], "READ_FILE") == 0 && ac >= 2)
                cmd_read_file(cl, a[1]);
            else if (strcmp(a[0], "CREATE") == 0 && ac >= 5)
                cmd_create(cl, a[1], a[2], a[3], a[4]);
            else if (strcmp(a[0], "EXIT") == 0) {
                auto_unmount();
                sendf(cl, "OK\n");
                close(cl);
                close(srv);
                printf("[mounter] exit\n");
                return 0;
            }
            else
                sendf(cl, "ERR unknown: %s\n", a[0]);
        }

        printf("[mounter] client disconnected\n");
        auto_unmount();
        close(cl);
    }

    close(srv);
    return 0;
}
