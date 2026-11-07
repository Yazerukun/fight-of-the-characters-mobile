/* ============================================================================
 * FOTC MOBILE APPLICATION BOOTSTRAPPER (3D WARCRAFT 3 ENGINE)
 * Location: src/main.js
 * ============================================================================ */
import { FOTC3DArena } from './engine3d/FOTC3DArena.js';

async function start() {
  const canvas = document.getElementById('gameCanvas');
  const toastContainer = document.getElementById('toast-container');

  const arena = new FOTC3DArena(canvas, toastContainer);
  window.game = arena;

  // Bridge helper for hero selection modal
  window.game.selectHero = (heroKey) => {
    arena.switchPlayerHero(heroKey);
    const modal = document.getElementById('hero-select-modal');
    if (modal) modal.style.display = 'none';
  };

  const btnCloseHeroes = document.getElementById('btn-close-heroes');
  if (btnCloseHeroes) {
    btnCloseHeroes.addEventListener('click', () => {
      const modal = document.getElementById('hero-select-modal');
      if (modal) modal.style.display = 'none';
    });
  }

  try {
    await arena.initEngine();
    console.log('[FOTC 3D] Authentic 3D Warcraft 3 Engine initialized successfully.');
  } catch (err) {
    console.error('[FOTC 3D] Failed to initialize 3D Arena Engine:', err);
  }
}

if (document.readyState === 'loading') {
  window.addEventListener('DOMContentLoaded', start);
} else {
  start();
}
