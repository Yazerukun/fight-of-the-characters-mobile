import os
import struct

def get_mdx_info(path):
    with open(path, 'rb') as f:
        d = f.read()
    if not d.startswith(b'MDLX'):
        return None
    pos = 4
    internal_name = ''
    textures = []
    seqs = []
    attachments = []
    particles = []
    ribbons = []

    while pos + 8 <= len(d):
        tag, sz = struct.unpack('<4sI', d[pos:pos+8])
        pos += 8
        cdata = d[pos:pos+sz]
        pos += sz
        tstr = tag.decode('latin1', errors='ignore')

        if tstr == 'MODL':
            internal_name = cdata[:80].split(b'\x00')[0].decode('latin1', errors='ignore')
        elif tstr == 'TEXS':
            cnt = sz // 268
            for i in range(cnt):
                tname = cdata[i*268 + 4 : (i+1)*268].split(b'\x00')[0].decode('latin1', errors='ignore')
                if tname:
                    textures.append(tname)
        elif tstr == 'SEQS':
            cnt = sz // 132
            for s in range(cnt):
                sdata = cdata[s*132 : (s+1)*132]
                sname = sdata[:80].split(b'\x00')[0].decode('latin1', errors='ignore')
                sstart, send, spd = struct.unpack('<IIf', sdata[80:92])
                seqs.append((sname, sstart, send, spd))
        elif tstr == 'ATCH':
            # Attachment chunk
            # Each attachment starts with an inclusive size
            p_atch = 0
            while p_atch < len(cdata):
                atch_sz = struct.unpack('<I', cdata[p_atch:p_atch+4])[0]
                if atch_sz == 0: break
                node_name = cdata[p_atch+4 : p_atch+84].split(b'\x00')[0].decode('latin1', errors='ignore')
                attachments.append(node_name)
                p_atch += atch_sz
        elif tstr in ('PREM', 'PRE2'):
            particles.append((tstr, sz))
        elif tstr == 'RIBB':
            ribbons.append(sz)

    return {
        'internal_name': internal_name,
        'textures': textures,
        'sequences': seqs,
        'attachments': attachments,
        'particles': particles,
        'ribbons': ribbons,
        'file_size': len(d)
    }

def main():
    root = os.path.join(os.getcwd(), 'FOC-Reborn-Mobile', 'OriginalAssets')
    hero_models = []

    for r, _, files in os.walk(root):
        for f in files:
            if f.lower().endswith('.mdx'):
                p = os.path.join(r, f)
                info = get_mdx_info(p)
                if info and len(info['sequences']) >= 5:
                    hero_models.append((f, info))

    print(f"Total hero/character candidates (>= 5 animations): {len(hero_models)}")
    hero_models.sort(key=lambda x: -len(x[1]['sequences']))

    for idx, (fname, info) in enumerate(hero_models[:10]):
        print(f"\n[{idx+1}] File: {fname} (Size: {info['file_size']:,} bytes)")
        print(f"    Internal Name: '{info['internal_name']}'")
        print(f"    Textures: {info['textures'][:4]}")
        print(f"    Attachments ({len(info['attachments'])}): {info['attachments'][:6]}")
        print(f"    Particle Emitters: {len(info['particles'])}, Ribbon Emitters: {len(info['ribbons'])}")
        print(f"    Animation Sequences ({len(info['sequences'])}):")
        for s in info['sequences']:
            print(f"      - '{s[0]}': {s[1]}-{s[2]} ms (Speed: {s[3]:.1f})")

if __name__ == '__main__':
    main()
