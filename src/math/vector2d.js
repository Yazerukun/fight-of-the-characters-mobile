/* ============================================================================
 * 03. PRECISION MATH & VECTOR2D
 * Location: src/math/vector2d.js
 * ============================================================================ */
export class Vector2D {
  constructor(x = 0, y = 0) {
    this.x = x;
    this.y = y;
  }
  set(x, y) { this.x = x; this.y = y; return this; }
  add(v) { this.x += v.x; this.y += v.y; return this; }
  sub(v) { this.x -= v.x; this.y -= v.y; return this; }
  scale(s) { this.x *= s; this.y *= s; return this; }
  lengthSq() { return this.x * this.x + this.y * this.y; }
  length() { return Math.hypot(this.x, this.y); }
  normalize() {
    const len = this.length();
    if (len > 0.00001) { this.x /= len; this.y /= len; }
    return this;
  }
  dist(v) { return Math.hypot(this.x - v.x, this.y - v.y); }
}
