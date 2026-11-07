/* ============================================================================
 * 02. SECURITY & ANTI-CHEAT UTILITIES
 * Location: src/security/anti_cheat.js
 * ============================================================================ */
export class AntiCheatNumber {
  constructor(initialValue = 0) {
    this.salt = (Math.random() * 0xFFFF) | 0;
    this.maskedValue = (initialValue ^ this.salt);
    this.shadowValue = initialValue;
  }

  get() {
    const decoded = this.maskedValue ^ this.salt;
    if (decoded !== this.shadowValue) {
      console.warn("[ANTI-CHEAT DETECTED] Memory tampering detected! Enforcing shadow rollback.");
      this.maskedValue = this.shadowValue ^ this.salt;
    }
    return this.shadowValue;
  }

  set(val) {
    const sanitized = Math.max(0, Math.floor(val));
    this.shadowValue = sanitized;
    this.maskedValue = (sanitized ^ this.salt);
  }

  add(val) {
    this.set(this.get() + val);
  }
}

export function escapeHTML(str) {
  return String(str).replace(/[&<>'"]/g, 
    tag => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[tag] || tag));
}
