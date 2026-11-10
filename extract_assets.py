import io
import os
import re
import struct
import zlib
import bz2
import mpyq

def main():
    map_path = 'Fight_of_Characters9.1a(Asia).w3x'
    output_dir = os.path.join(os.getcwd(), 'Extracted_Assets')
    os.makedirs(output_dir, exist_ok=True)

    print(f"Opening {map_path}...")
    with open(map_path, 'rb') as f:
        f.seek(512)
        mpq_data = io.BytesIO(f.read())

    archive = mpyq.MPQArchive(mpq_data)
    print(f"Archive loaded. Header: {archive.header}")
    print(f"Block table size: {len(archive.block_table)}")
    print(f"Hash table size: {len(archive.hash_table)}")

    hash_to_block = {}
    for h in archive.hash_table:
        if h.block_table_index < len(archive.block_table):
            hash_to_block[(h.hash_a, h.hash_b)] = h.block_table_index

    # Collect candidate names
    candidates = set([
        'war3map.doo', 'war3map.j', 'war3map.mmp', 'war3map.shd',
        'war3map.w3a', 'war3map.w3b', 'war3map.w3d', 'war3map.w3e',
        'war3map.w3i', 'war3map.wpm', 'war3map.wts', 'war3mapExtra.txt',
        'war3mapMisc.txt', 'war3mapSkin.txt', '(listfile)', '(attributes)',
        'war3mapMap.blp', 'war3mapPreview.tga', 'war3mapPreview.blp',
        'war3map.w3u', 'war3map.w3t', 'war3map.w3c', 'war3map.w3r', 'war3map.w3s'
    ])

    if os.path.exists('war3map.j'):
        with open('war3map.j', 'r', encoding='utf-8', errors='ignore') as f:
            jass_content = f.read()
        quoted = re.findall(r'"([^"]+)"', jass_content)
        for q in quoted:
            q_clean = q.strip()
            if not q_clean:
                continue
            candidates.add(q_clean)
            candidates.add(q_clean.replace('/', '\\'))
            candidates.add(q_clean.replace('\\', '/'))
            base = os.path.basename(q_clean.replace('/', '\\'))
            candidates.add(base)
            candidates.add('war3mapImported\\' + base)
            candidates.add('war3mapimported\\' + base)

def compute_mpq_hash(encryption_table, string_or_bytes, hash_type):
    hash_types = {
        'TABLE_OFFSET': 0,
        'HASH_A': 1,
        'HASH_B': 2,
        'TABLE': 3
    }
    seed1 = 0x7FED7FED
    seed2 = 0xEEEEEEEE
    type_offset = hash_types[hash_type] << 8

    if isinstance(string_or_bytes, str):
        # MPQ filenames are uppercase ASCII or bytes
        raw_bytes = string_or_bytes.upper().encode('cp949', errors='ignore')
    else:
        raw_bytes = bytes(string_or_bytes).upper()

    for b in raw_bytes:
        val = encryption_table[type_offset + b]
        seed1 = (val ^ (seed1 + seed2)) & 0xFFFFFFFF
        seed2 = (b + seed1 + seed2 + (seed2 << 5) + 3) & 0xFFFFFFFF

    # Match candidates
    block_to_name = {}
    for cand in candidates:
        try:
            ha = compute_mpq_hash(archive.encryption_table, cand, 'HASH_A')
            hb = compute_mpq_hash(archive.encryption_table, cand, 'HASH_B')
            if (ha, hb) in hash_to_block:
                b_idx = hash_to_block[(ha, hb)]
                block_to_name[b_idx] = cand
        except Exception:
            pass

    print(f"Identified names for {len(block_to_name)} / {len(archive.block_table)} blocks using initial candidates.")

    def decrypt_data(data, k):
        res = bytearray(archive._decrypt(data[:len(data)//4*4], k))
        if len(data) % 4 != 0:
            res.extend(data[len(data)//4*4:])
        return bytes(res)

    def decompress(data):
        ctype = data[0]
        if ctype == 0:
            return data
        elif ctype == 2:
            return zlib.decompress(data[1:], 15)
        elif ctype == 16:
            return bz2.decompress(data[1:])
        return data

    def extract_block(b_idx, filename=None):
        b = archive.block_table[b_idx]
        if not (b.flags & mpyq.MPQ_FILE_EXISTS) or b.archived_size == 0:
            return None
        
        archive.file.seek(b.offset + archive.header['offset'])
        file_data = archive.file.read(b.archived_size)

        key = 0
        if b.flags & mpyq.MPQ_FILE_ENCRYPTED:
            if not filename:
                raise ValueError(f"Block {b_idx} is encrypted but no filename provided!")
            key = compute_mpq_hash(archive.encryption_table, os.path.basename(filename.replace('/', '\\')), 'TABLE')
            if b.flags & mpyq.MPQ_FILE_FIX_KEY:
                key = ((key + b.offset) ^ b.size) & 0xFFFFFFFF

        if b.flags & mpyq.MPQ_FILE_SINGLE_UNIT:
            if b.flags & mpyq.MPQ_FILE_ENCRYPTED:
                file_data = decrypt_data(file_data, key)
            if b.flags & mpyq.MPQ_FILE_COMPRESS and b.size > b.archived_size:
                file_data = decompress(file_data)
            return file_data
        else:
            sector_size = 512 << archive.header['sector_size_shift']
            sectors = b.size // sector_size + 1
            crc = bool(b.flags & mpyq.MPQ_FILE_SECTOR_CRC)
            if crc:
                sectors += 1
            pos_len = 4 * (sectors + 1)
            pos_data = file_data[:pos_len]
            if b.flags & mpyq.MPQ_FILE_ENCRYPTED:
                pos_data = decrypt_data(pos_data, (key - 1) & 0xFFFFFFFF)
            positions = struct.unpack('<%dI' % (sectors + 1), pos_data)

            res = io.BytesIO()
            bytes_left = b.size
            num_sectors = len(positions) - (2 if crc else 1)
            for s in range(num_sectors):
                sec = file_data[positions[s]:positions[s+1]]
                if b.flags & mpyq.MPQ_FILE_ENCRYPTED:
                    sec = decrypt_data(sec, (key + s) & 0xFFFFFFFF)
                expected = min(sector_size, bytes_left)
                if len(sec) < expected:
                    sec = decompress(sec)
                bytes_left -= len(sec)
                res.write(sec)
            return res.getvalue()

    # Extract all
    extracted_manifest = []
    for idx, b in enumerate(archive.block_table):
        name = block_to_name.get(idx)
        try:
            raw = extract_block(idx, name)
            if raw is None:
                continue

            magic = raw[:4]
            ext = '.bin'
            if magic.startswith(b'MDLX'):
                ext = '.mdx'
            elif magic.startswith(b'BLP1'):
                ext = '.blp'
            elif magic.startswith(b'RIFF'):
                ext = '.wav'
            elif magic.startswith(b'ID3') or (len(raw) > 2 and raw[:2] in (b'\xff\xfb', b'\xff\xf3', b'\xff\xf2')):
                ext = '.mp3'
            elif magic.startswith(b'DDS '):
                ext = '.dds'
            elif all(c < 128 for c in raw[:min(len(raw), 64)]):
                ext = '.txt'

            if not name:
                name = f"unknown_{idx:04d}{ext}"

            # Clean name for filesystem
            clean_rel_path = name.replace('\\', '/').lstrip('/')
            dest_path = os.path.join(output_dir, clean_rel_path)
            os.makedirs(os.path.dirname(dest_path), exist_ok=True)
            with open(dest_path, 'wb') as out_f:
                out_f.write(raw)

            extracted_manifest.append((idx, name, len(raw), ext))
        except Exception as e:
            print(f"Error extracting block {idx} ({name}): {e}")

    print(f"Extraction complete! Extracted {len(extracted_manifest)} files to {output_dir}")

    # Write manifest
    with open(os.path.join(output_dir, 'manifest.txt'), 'w', encoding='utf-8') as mf:
        for idx, name, sz, ext in extracted_manifest:
            mf.write(f"Block {idx:04d} | Size: {sz:8d} | Ext: {ext:5s} | Name: {name}\n")

if __name__ == '__main__':
    main()
