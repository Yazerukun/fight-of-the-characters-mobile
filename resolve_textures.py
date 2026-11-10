import io
import os
import mpyq

def main():
    map_path = 'Fight_of_Characters9.1a(Asia).w3x'
    with open(map_path, 'rb') as f:
        f.seek(512)
        mpq_data = io.BytesIO(f.read())
    archive = mpyq.MPQArchive(mpq_data)

    hash_to_block = {}
    for h in archive.hash_table:
        if h.block_table_index < len(archive.block_table):
            hash_to_block[(h.hash_a, h.hash_b)] = h.block_table_index

    def compute_mpq_hash(tbl, string_or_bytes, hash_type):
        htypes = {'TABLE_OFFSET': 0, 'HASH_A': 1, 'HASH_B': 2, 'TABLE': 3}
        s1, s2 = 0x7FED7FED, 0xEEEEEEEE
        t_off = htypes[hash_type] << 8
        if isinstance(string_or_bytes, str):
            raw = string_or_bytes.upper().encode('cp949', errors='ignore')
        else:
            raw = bytes(string_or_bytes).upper()
        for b in raw:
            v = tbl[t_off + b]
            s1 = (v ^ (s1 + s2)) & 0xFFFFFFFF
            s2 = (b + s1 + s2 + (s2 << 5) + 3) & 0xFFFFFFFF
        return s1

    # Extract all textures listed in MDX models
    import struct
    mdx_textures = set()
    root = os.path.join(os.getcwd(), 'FOC-Reborn-Mobile', 'OriginalAssets')
    for r, _, files in os.walk(root):
        for f in files:
            if f.endswith('.mdx'):
                with open(os.path.join(r, f), 'rb') as fp:
                    d = fp.read()
                pos = 4
                while pos + 8 <= len(d):
                    tag, sz = struct.unpack('<4sI', d[pos:pos+8])
                    pos += 8
                    cdata = d[pos:pos+sz]
                    pos += sz
                    if tag == b'TEXS':
                        cnt = sz // 268
                        for i in range(cnt):
                            tname = cdata[i*268 + 4 : (i+1)*268].split(b'\x00')[0].decode('latin1', errors='ignore')
                            if tname:
                                mdx_textures.add(tname)

    print(f"Total unique texture paths found inside MDX files: {len(mdx_textures)}")

    resolved = {}
    for tex in mdx_textures:
        candidates = [
            tex,
            os.path.basename(tex),
            'war3mapImported\\' + os.path.basename(tex),
            'Textures\\' + os.path.basename(tex),
            tex.replace('/', '\\')
        ]
        for c in candidates:
            ha = compute_mpq_hash(archive.encryption_table, c, 'HASH_A')
            hb = compute_mpq_hash(archive.encryption_table, c, 'HASH_B')
            if (ha, hb) in hash_to_block:
                b_idx = hash_to_block[(ha, hb)]
                resolved[tex] = (b_idx, c)
                break

    print(f"Successfully resolved {len(resolved)} / {len(mdx_textures)} textures to their exact MPQ block!")
    for tex, (b_idx, c) in sorted(resolved.items(), key=lambda x: x[1][0])[:25]:
        print(f"  Block {b_idx:04d} -> '{tex}' (matched as '{c}')")

if __name__ == '__main__':
    main()
