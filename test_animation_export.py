import struct
import math

def quat_to_matrix(qx, qy, qz, qw):
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

def build_collada_transform_matrix(lx, ly, lz, tx, ty, tz, qx, qy, qz, qw):
    rot_wc3 = quat_to_matrix(qx, qy, qz, qw)
    tot_tx = lx + tx
    tot_ty = ly + ty
    tot_tz = lz + tz

    u_r00 = rot_wc3[0][0]; u_r01 = rot_wc3[0][2]; u_r02 = rot_wc3[0][1]
    u_r10 = rot_wc3[2][0]; u_r11 = rot_wc3[2][2]; u_r12 = rot_wc3[2][1]
    u_r20 = rot_wc3[1][0]; u_r21 = rot_wc3[1][2]; u_r22 = rot_wc3[1][1]

    return [
        u_r00, u_r01, u_r02, tot_tx,
        u_r10, u_r11, u_r12, tot_tz,
        u_r20, u_r21, u_r22, tot_ty,
        0.0,   0.0,   0.0,   1.0
    ]

def sample_vec3(keys, t):
    if not keys: return (0.0, 0.0, 0.0)
    if t <= keys[0][0]: return keys[0][1]
    if t >= keys[-1][0]: return keys[-1][1]
    for i in range(len(keys)-1):
        t0, v0 = keys[i]
        t1, v1 = keys[i+1]
        if t0 <= t <= t1:
            alpha = (t - t0) / (t1 - t0) if t1 > t0 else 0.0
            return (
                v0[0] + alpha * (v1[0] - v0[0]),
                v0[1] + alpha * (v1[1] - v0[1]),
                v0[2] + alpha * (v1[2] - v0[2])
            )
    return keys[-1][1]

def sample_quat(keys, t):
    if not keys: return (0.0, 0.0, 0.0, 1.0)
    if t <= keys[0][0]: return keys[0][1]
    if t >= keys[-1][0]: return keys[-1][1]
    for i in range(len(keys)-1):
        t0, q0 = keys[i]
        t1, q1 = keys[i+1]
        if t0 <= t <= t1:
            alpha = (t - t0) / (t1 - t0) if t1 > t0 else 0.0
            # Lerp and normalize for fast robust interpolation
            dot = q0[0]*q1[0] + q0[1]*q1[1] + q0[2]*q1[2] + q0[3]*q1[3]
            sign = 1.0 if dot >= 0 else -1.0
            qx = (1.0 - alpha) * q0[0] + alpha * sign * q1[0]
            qy = (1.0 - alpha) * q0[1] + alpha * sign * q1[1]
            qz = (1.0 - alpha) * q0[2] + alpha * sign * q1[2]
            qw = (1.0 - alpha) * q0[3] + alpha * sign * q1[3]
            mag = math.sqrt(qx*qx + qy*qy + qz*qz + qw*qw)
            if mag > 1e-6:
                return (qx/mag, qy/mag, qz/mag, qw/mag)
            return (0.0, 0.0, 0.0, 1.0)
    return keys[-1][1]

print("Sampler functions compiled successfully.")
