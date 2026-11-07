import os
import re
import struct
import zlib
import bz2
import mpyq

def patched_read_header(self):
    self.file.seek(512)
    data = self.file.read(32)
    header = mpyq.MPQFileHeader._make(struct.unpack(mpyq.MPQFileHeader.struct_format, data))._asdict()
    header['offset'] = 512
    return header

mpyq.MPQArchive.read_header = patched_read_header

def patched_init(self, file, listfile=False):
    self.file = open(file, 'rb') if isinstance(file, str) else file
    self.header = self.read_header()
    self.hash_table = self.read_table('hash')
    self.block_table = self.read_table('block')
    self.files = []

mpyq.MPQArchive.__init__ = patched_init

def extract_file(archive, filename, target_path):
    entry = archive.get_hash_table_entry(filename)
    if not entry:
        # Try alternate path separators
        alt1 = filename.replace('/', '\\')
        entry = archive.get_hash_table_entry(alt1)
        if not entry:
            alt2 = filename.replace('\\', '/')
            entry = archive.get_hash_table_entry(alt2)
            if not entry:
                return False
    b = archive.block_table[entry.block_table_index]
    if b.archived_size == 0:
        return False

    archive.file.seek(b.offset + 512)
    raw = archive.file.read(b.archived_size)

    if not (b.flags & mpyq.MPQ_FILE_SINGLE_UNIT):
        sector_size = 512 << archive.header['sector_size_shift']
        sectors = b.size // sector_size + 1
        positions = struct.unpack('<%dI' % (sectors + 1), raw[:4*(sectors+1)])
        out = bytearray()
        for i in range(len(positions) - 1):
            sector = raw[positions[i]:positions[i+1]]
            if not sector:
                continue
            if len(sector) == sector_size:
                out.extend(sector)
            else:
                flag = sector[0]
                if flag == 2:
                    out.extend(zlib.decompress(sector[1:]))
                elif flag == 16:
                    out.extend(bz2.decompress(sector[1:]))
                else:
                    out.extend(sector)
        data = bytes(out)
    else:
        if b.flags & mpyq.MPQ_FILE_COMPRESS:
            flag = raw[0]
            if flag == 2:
                data = zlib.decompress(raw[1:])
            elif flag == 16:
                data = bz2.decompress(raw[1:])
            else:
                data = raw
        else:
            data = raw

    os.makedirs(os.path.dirname(os.path.abspath(target_path)), exist_ok=True)
    with open(target_path, 'wb') as f:
        f.write(data)
    print(f"Extracted: {filename} -> {target_path} ({len(data):,} bytes)")
    return data

def main():
    map_path = r'c:\Users\Technical\Desktop\FOCS3 Another 9.7 C03.w3x'
    archive = mpyq.MPQArchive(map_path, listfile=False)
    print("Opened MPQ archive successfully.")

    hero_candidates = [
        # (W3X Internal Path, Local Target Name)
        ("war3mapImported\\Gokuss32.mdx", "public/models/goku.mdx"),
        ("Gokuss32.mdx", "public/models/goku.mdx"),
        ("Sasuke.mdx", "public/models/sasuke.mdx"),
        ("war3mapImported\\Sasuke.mdx", "public/models/sasuke.mdx"),
        ("narutoSunin.mdx", "public/models/naruto.mdx"),
        ("war3mapImported\\narutoSunin.mdx", "public/models/naruto.mdx"),
        ("rocklee2.mdx", "public/models/rocklee.mdx"),
        ("war3mapImported\\rocklee2.mdx", "public/models/rocklee.mdx"),
        ("ace.mdx", "public/models/ace.mdx"),
        ("war3mapImported\\ace.mdx", "public/models/ace.mdx"),
        ("Inuyasha.mdx", "public/models/inuyasha.mdx"),
        ("war3mapImported\\Inuyasha.mdx", "public/models/inuyasha.mdx"),
        ("Toushirou.mdx", "public/models/toshiro.mdx"),
        ("war3mapImported\\Toushirou.mdx", "public/models/toshiro.mdx"),
        ("Gilgamesh46.mdx", "public/models/gilgamesh.mdx"),
        ("war3mapImported\\Gilgamesh46.mdx", "public/models/gilgamesh.mdx"),
    ]

    extracted_models = []
    for internal_path, target_path in hero_candidates:
        if os.path.exists(target_path):
            extracted_models.append(target_path)
            continue
        data = extract_file(archive, internal_path, target_path)
        if data:
            extracted_models.append(target_path)

    # Now inspect models for required .blp textures
    for model_path in extracted_models:
        with open(model_path, 'rb') as f:
            content = f.read()
        blps = set(re.findall(rb'[\w\-\\\/.]+\.blp', content, re.IGNORECASE))
        for raw_blp in blps:
            blp_str = raw_blp.decode('ascii', errors='ignore')
            clean_blp_name = os.path.basename(blp_str)
            target_blp = os.path.join("public/models", clean_blp_name)
            if not os.path.exists(target_blp):
                # Try internal variations
                for prefix in ["", "war3mapImported\\", "Textures\\"]:
                    try_name = prefix + clean_blp_name
                    if extract_file(archive, try_name, target_blp):
                        break

if __name__ == '__main__':
    main()
