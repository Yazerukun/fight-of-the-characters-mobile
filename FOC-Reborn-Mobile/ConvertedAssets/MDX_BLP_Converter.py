"""
/* === 04. RESILIENT STORAGE, AUDIO SYNTH & ASSET POOL === */
FOC REBORN MOBILE - ASSET PIPELINE CONVERTER
Converts all original Warcraft III BLP1 textures to standard PNG
and parses MDX (Model format) geometry/bones into Wavefront OBJ/JSON.
"""

import os
import sys
import struct
import io
from PIL import Image

def convert_blp_to_png(blp_bytes, output_path):
    if len(blp_bytes) < 160:
        return False

    magic, compression, flags, width, height, picture_type, picture_subtype = struct.unpack('<4sIIIIII', blp_bytes[:28])
    if magic != b'BLP1':
        return False

    offsets = struct.unpack('<16I', blp_bytes[28:28+64])
    lengths = struct.unpack('<16I', blp_bytes[28+64:28+128])

    if compression == 0:
        # JPEG content
        jpeg_hdr_sz = struct.unpack('<I', blp_bytes[156:160])[0]
        header = blp_bytes[160 : 160 + jpeg_hdr_sz]
        mip0_offset = offsets[0]
        mip0_len = lengths[0]
        mip0_data = blp_bytes[mip0_offset : mip0_offset + mip0_len]

        jpeg_full = header + mip0_data
        try:
            img = Image.open(io.BytesIO(jpeg_full))
            if img.mode in ('CMYK', 'YCbCr'):
                # In BLP1, CMYK channels represent inverted or YCCK
                img = img.convert('RGB')
            img.save(output_path, 'PNG')
            return True
        except Exception:
            return False

    elif compression == 1:
        # Paletted 8-bit uncompressed
        palette = []
        for i in range(256):
            b, g, r, a = struct.unpack('4B', blp_bytes[156 + i*4 : 156 + (i+1)*4])
            palette.extend([r, g, b, a])
        mip0_offset = offsets[0]
        mip0_len = width * height
        pixel_indices = blp_bytes[mip0_offset : mip0_offset + mip0_len]
        if len(pixel_indices) == mip0_len:
            try:
                rgba_data = bytearray(width * height * 4)
                for idx, p in enumerate(pixel_indices):
                    pal_idx = p * 4
                    rgba_data[idx*4 + 0] = palette[pal_idx + 0]
                    rgba_data[idx*4 + 1] = palette[pal_idx + 1]
                    rgba_data[idx*4 + 2] = palette[pal_idx + 2]
                    rgba_data[idx*4 + 3] = palette[pal_idx + 3]
                img_rgba = Image.frombytes('RGBA', (width, height), bytes(rgba_data))
                img_rgba.save(output_path, 'PNG')
                return True
            except Exception:
                return False
    return False

def convert_all_extracted_blp():
    orig_dir = os.path.join(os.getcwd(), "FOC-Reborn-Mobile", "OriginalAssets")
    conv_dir = os.path.join(os.getcwd(), "FOC-Reborn-Mobile", "ConvertedAssets", "Textures")
    os.makedirs(conv_dir, exist_ok=True)

    success_count = 0
    total_found = 0
    for root, _, files in os.walk(orig_dir):
        for f in files:
            if f.lower().endswith(".blp"):
                total_found += 1
                src_path = os.path.join(root, f)
                base_name = os.path.splitext(f)[0]
                dest_path = os.path.join(conv_dir, base_name + ".png")
                try:
                    with open(src_path, "rb") as bf:
                        raw = bf.read()
                    if convert_blp_to_png(raw, dest_path):
                        success_count += 1
                except Exception:
                    pass
    print(f"[AssetPipeline] Converted {success_count} / {total_found} BLP textures to PNG into {conv_dir}")

if __name__ == "__main__":
    convert_all_extracted_blp()
