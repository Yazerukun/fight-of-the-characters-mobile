/* ============================================================================
 * 01. ENGINE CONFIG & CONSTANTS
 * Location: src/config/constants.js
 * ============================================================================ */
export const CONFIG = Object.freeze({
  WORLD_WIDTH: 2400,
  WORLD_HEIGHT: 2400,
  DUEL_INTERVAL_SEC: 180,
  DUEL_DURATION_SEC: 45,
  FIXED_TIMESTEP_MS: 1000 / 60,
  MAX_FRAME_DELTA_MS: 250,
  GRID_CELL_SIZE: 160,
  AUDIO_ENABLED: true
});

export const TEAMS = Object.freeze({
  SENTINEL: 1,
  SCOURGE: 2,
  NEUTRAL: 3
});
