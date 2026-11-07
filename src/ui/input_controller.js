/* ============================================================================
 * INPUT CONTROLLER & MOBILE TOUCH BOOTSTRAP
 * Location: src/ui/input_controller.js
 * ============================================================================ */
export function setupInputController(game) {
  // 1. Keyboard bindings
  window.addEventListener('keydown', (e) => {
    game.audio.init();
    game.keys[e.code] = true;
    if (e.code === 'KeyQ') game.castSkillQ();
    if (e.code === 'KeyW') game.castSkillW();
    if (e.code === 'KeyE') game.castSkillE();
    if (e.code === 'KeyR') game.castSkillR();
    if (e.code === 'KeyB') {
      const m = document.getElementById('shop-modal');
      if (m) m.style.display = (m.style.display === 'flex') ? 'none' : 'flex';
    }
    if (e.code === 'KeyH') {
      const hm = document.getElementById('hero-select-modal');
      if (hm) hm.style.display = (hm.style.display === 'flex') ? 'none' : 'flex';
    }
    if (e.code === 'Digit1') game.buyItem('potion');
  });

  window.addEventListener('keyup', (e) => {
    game.keys[e.code] = false;
  });

  // 2. Audio & UI Toggles
  const soundBtn = document.getElementById('btn-sound-toggle');
  if (soundBtn) {
    soundBtn.addEventListener('click', () => {
      game.audio.init();
      const on = game.audio.toggle();
      soundBtn.textContent = `Audio: ${on ? 'ON' : 'OFF'}`;
    });
  }

  const openShopBtn = document.getElementById('btn-open-shop');
  const closeShopBtn = document.getElementById('btn-close-shop');
  const shopModal = document.getElementById('shop-modal');

  if (openShopBtn && shopModal) {
    openShopBtn.addEventListener('click', () => { shopModal.style.display = 'flex'; });
  }
  if (closeShopBtn && shopModal) {
    closeShopBtn.addEventListener('click', () => { shopModal.style.display = 'none'; });
  }

  const openHeroesBtn = document.getElementById('btn-open-heroes');
  const closeHeroesBtn = document.getElementById('btn-close-heroes');
  const heroesModal = document.getElementById('hero-select-modal');

  if (openHeroesBtn && heroesModal) {
    openHeroesBtn.addEventListener('click', () => { heroesModal.style.display = 'flex'; });
  }
  if (closeHeroesBtn && heroesModal) {
    closeHeroesBtn.addEventListener('click', () => { heroesModal.style.display = 'none'; });
  }

  // 3. Touch Skill Buttons
  const bindTouchSkill = (elementId, actionFn) => {
    const btn = document.getElementById(elementId);
    if (!btn) return;
    const trigger = (e) => {
      e.preventDefault();
      game.audio.init();
      actionFn();
    };
    btn.addEventListener('touchstart', trigger, { passive: false });
    btn.addEventListener('click', trigger);
  };

  bindTouchSkill('btn-skill-q', () => game.castSkillQ());
  bindTouchSkill('btn-skill-w', () => game.castSkillW());
  bindTouchSkill('btn-skill-e', () => game.castSkillE());
  bindTouchSkill('btn-skill-r', () => game.castSkillR());

  // 4. Virtual Joystick Touch Handling
  const joyZone = document.getElementById('joystick-zone');
  const joyKnob = document.getElementById('joystick-knob');
  if (!joyZone || !joyKnob) return;

  let joyTouchId = null;
  let joyCenter = { x: 0, y: 0 };
  const joyRadius = 45;

  joyZone.addEventListener('touchstart', (e) => {
    e.preventDefault();
    game.audio.init();
    const touch = e.changedTouches[0];
    joyTouchId = touch.identifier;
    const rect = joyZone.getBoundingClientRect();
    joyCenter = { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
    handleJoyMove(touch.clientX, touch.clientY);
  }, { passive: false });

  joyZone.addEventListener('touchmove', (e) => {
    e.preventDefault();
    for (let i = 0; i < e.changedTouches.length; i++) {
      if (e.changedTouches[i].identifier === joyTouchId) {
        handleJoyMove(e.changedTouches[i].clientX, e.changedTouches[i].clientY);
        break;
      }
    }
  }, { passive: false });

  const resetJoy = () => {
    joyTouchId = null;
    game.isMoving = false;
    joyKnob.style.transform = `translate(0px, 0px)`;
  };

  joyZone.addEventListener('touchend', resetJoy);
  joyZone.addEventListener('touchcancel', resetJoy);

  function handleJoyMove(clientX, clientY) {
    const dx = clientX - joyCenter.x;
    const dy = clientY - joyCenter.y;
    const dist = Math.hypot(dx, dy);
    const angle = Math.atan2(dy, dx);
    const clampedDist = Math.min(dist, joyRadius);

    const knobX = Math.cos(angle) * clampedDist;
    const knobY = Math.sin(angle) * clampedDist;
    joyKnob.style.transform = `translate(${knobX}px, ${knobY}px)`;

    if (dist > 6) {
      game.isMoving = true;
      game.touchMoveDir.set(Math.cos(angle), Math.sin(angle));
    } else {
      game.isMoving = false;
    }
  }
}
