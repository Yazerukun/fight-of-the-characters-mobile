import struct
import math

def quat_to_matrix(qx, qy, qz, qw):
    # Normalize quaternion
    mag = math.sqrt(qx*qx + qy*qy + qz*qz + qw*qw)
    if mag > 1e-6:
        qx, qy, qz, qw = qx/mag, qy/mag, qz/mag, qw/mag
    else:
        qx, qy, qz, qw = 0.0, 0.0, 0.0, 1.0

    xx = qx * qx; xy = qx * qy; xz = qx * qz; xw = qx * qw
    yy = qy * qy; yz = qy * qz; yw = qy * qw
    zz = qz * qz; zw = qz * qw

    r00 = 1.0 - 2.0 * (yy + zz)
    r01 = 2.0 * (xy - zw)
    r02 = 2.0 * (xz + yw)

    r10 = 2.0 * (xy + zw)
    r11 = 1.0 - 2.0 * (xx + zz)
    r12 = 2.0 * (yz - xw)

    r20 = 2.0 * (xz - yw)
    r21 = 2.0 * (yz + xw)
    r22 = 1.0 - 2.0 * (xx + yy)

    return [
        [r00, r01, r02],
        [r10, r11, r12],
        [r20, r21, r22]
    ]

# In WC3 Z-up to Unity Y-up, swap Y and Z
# If M_wc3 transforms (x, y, z), then P * M_wc3 * P transforms (x, z, y)
def build_collada_transform_matrix(lx, ly, lz, tx, ty, tz, qx, qy, qz, qw):
    # Total translation in WC3: (lx + tx, ly + ty, lz + tz)
    # where (lx, ly, lz) is relative pivot in WC3, and (tx, ty, tz) is anim translation in WC3
    rot_wc3 = quat_to_matrix(qx, qy, qz, qw)
    tot_tx = lx + tx
    tot_ty = ly + ty
    tot_tz = lz + tz

    # Convert 3x3 rot to Y-up: swap row 1&2 and col 1&2
    # [r00, r02, r01]
    # [r20, r22, r21]
    # [r10, r12, r11]
    u_r00 = rot_wc3[0][0]; u_r01 = rot_wc3[0][2]; u_r02 = rot_wc3[0][1]
    u_r10 = rot_wc3[2][0]; u_r11 = rot_wc3[2][2]; u_r12 = rot_wc3[2][1]
    u_r20 = rot_wc3[1][0]; u_r21 = rot_wc3[1][2]; u_r22 = rot_wc3[1][1]

    # Translation in Y-up: (tot_tx, tot_tz, tot_ty)
    u_tx = tot_tx
    u_ty = tot_tz
    u_tz = tot_ty

    # Return 16-element row-major Collada matrix
    return [
        u_r00, u_r01, u_r02, u_tx,
        u_r10, u_r11, u_r12, u_ty,
        u_r20, u_r21, u_r22, u_tz,
        0.0,   0.0,   0.0,   1.0
    ]

# Test Identity
m_ident = build_collada_transform_matrix(10, 20, 30, 0, 0, 0, 0, 0, 0, 1)
print("Identity Matrix check:")
for i in range(4):
    print(" ", m_ident[i*4:(i+1)*4])
print("Translation: Expected (10, 30, 20), Got:", m_ident[3], m_ident[7], m_ident[11])
