const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

const stageCount = document.getElementById("stageCount");
const phaseLabel = document.getElementById("phaseLabel");
const lifeCount = document.getElementById("lifeCount");
const bombCount = document.getElementById("bombCount");
const scoreCount = document.getElementById("scoreCount");
const shotCount = document.getElementById("shotCount");
const enemyLabel = document.getElementById("enemyLabel");
const fpsCount = document.getElementById("fpsCount");
const speakerLabel = document.getElementById("speakerLabel");
const dialogueStep = document.getElementById("dialogueStep");
const dialogueText = document.getElementById("dialogueText");
const advanceButton = document.getElementById("advanceButton");
const portraitImage = document.getElementById("portraitImage");
const flowSummary = document.getElementById("flowSummary");
const flowSteps = document.getElementById("flowSteps");
const dialogueShell = document.querySelector(".dialogue-shell");

const TAU = Math.PI * 2;
const keys = new Set();
const stageId = "stage-1-prototype";

let config = null;

const state = {
  lastTimestamp: 0,
  fps: 0,
  stageNumber: 1,
  scene: "stage-intro",
  sceneTimer: 0,
  score: 0,
  shotsFired: 0,
  overlayMessage: "Stage 1",
  dialogueIndex: 0,
  playerShots: [],
  enemyShots: [],
  enemies: [],
  effects: [],
  introReady: false,
  clearReady: false,
  gameOverReady: false,
  battle: {
    spawnTimer: 0,
    waveIndex: 0,
    bossSpawned: false,
    bossDefeated: false,
    stageTime: 0,
  },
  player: {
    x: canvas.width / 2,
    y: canvas.height - 88,
    radius: 10,
    speed: 320,
    focusSpeed: 170,
    lives: 3,
    bombs: 3,
    cooldown: 0,
    bombCooldown: 0,
    invulnerability: 0,
  },
};

async function bootstrap() {
  config = await loadStageConfig();
  renderFlowSteps();
  resetForStage();
  requestAnimationFrame(render);
}

async function loadStageConfig() {
  const response = await fetch(`/api/game/stages/${stageId}`);
  if (!response.ok) {
    throw new Error(`Failed to load stage config: ${response.status}`);
  }

  return response.json();
}

function renderFlowSteps() {
  flowSteps.innerHTML = "";
  for (const [index, step] of config.flow.entries()) {
    const item = document.createElement("li");
    item.className = "flow-step-item";
    item.dataset.scene = step.scene;
    item.innerHTML = `<span>${index + 1}</span><strong>${step.label}</strong>`;
    flowSteps.appendChild(item);
  }
}

function resetForStage() {
  state.stageNumber = parseStageNumber(config.stageLabel);
  state.scene = "stage-intro";
  state.sceneTimer = 0;
  state.score = 0;
  state.shotsFired = 0;
  state.dialogueIndex = 0;
  state.playerShots = [];
  state.enemyShots = [];
  state.enemies = [];
  state.effects = [];
  state.overlayMessage = config.stageLabel;
  state.introReady = false;
  state.clearReady = false;
  state.gameOverReady = false;
  state.battle = {
    spawnTimer: 0,
    waveIndex: 0,
    bossSpawned: false,
    bossDefeated: false,
    stageTime: 0,
  };
  state.player = {
    x: canvas.width / 2,
    y: canvas.height - 88,
    radius: config.player.radius,
    speed: config.player.speed,
    focusSpeed: config.player.focusSpeed,
    lives: config.player.lives,
    bombs: config.player.bombs,
    cooldown: 0,
    bombCooldown: 0,
    invulnerability: config.player.respawnInvulnerabilitySeconds * 0.6,
  };
  syncDialogue();
  updateHud();
}

function parseStageNumber(label) {
  const matched = String(label).match(/\d+/);
  return matched ? Number(matched[0]) : 1;
}

function startBattle() {
  state.scene = "battle";
  state.sceneTimer = 0;
  state.overlayMessage = "";
  state.dialogueIndex = 0;
  state.playerShots = [];
  state.enemyShots = [];
  state.effects = [];
  state.enemies = [];
  state.battle = {
    spawnTimer: config.battle.mobSpawnDelaySeconds,
    waveIndex: 0,
    bossSpawned: false,
    bossDefeated: false,
    stageTime: 0,
  };
  syncDialogue();
  updateHud();
}

function getSceneLabel(scene) {
  return config.sceneText.labels[scene] ?? scene;
}

function getSceneSummary(scene) {
  return config.flow.find((step) => step.scene === scene)?.summary ?? "";
}

function getDialogueBlock() {
  if (state.scene === "dialogue-pre") return config.dialogue.preBattle;
  if (state.scene === "dialogue-post") return config.dialogue.postBattle;
  return null;
}

function getStandbyMessage() {
  return config.sceneText.standbyMessages[state.scene] ?? "現在のフェーズでは会話表示はありません。";
}

function syncDialogue() {
  const block = getDialogueBlock();
  if (!block) {
    speakerLabel.textContent = config.dialogue.placeholderSpeaker;
    dialogueStep.textContent = "- / -";
    dialogueText.textContent = getStandbyMessage();
    portraitImage.classList.add("portrait-muted");
    dialogueShell.classList.add("is-standby");
    return;
  }

  const entry = block[state.dialogueIndex] ?? block[block.length - 1];
  speakerLabel.textContent = entry.speaker;
  dialogueStep.textContent = `${Math.min(state.dialogueIndex + 1, block.length)} / ${block.length}`;
  dialogueText.textContent = entry.text;
  portraitImage.classList.remove("portrait-muted");
  dialogueShell.classList.remove("is-standby");
}

function syncFlowUi() {
  const items = Array.from(flowSteps.querySelectorAll(".flow-step-item"));
  const currentIndex = config.flow.findIndex((step) => step.scene === state.scene);

  for (const [index, item] of items.entries()) {
    item.classList.remove("is-past", "is-current", "is-future");
    if (state.scene === "game-over") {
      item.classList.add("is-future");
      continue;
    }
    if (index < currentIndex) item.classList.add("is-past");
    else if (index === currentIndex) item.classList.add("is-current");
    else item.classList.add("is-future");
  }

  flowSummary.textContent = getSceneSummary(state.scene);
}

function syncAdvanceUi() {
  advanceButton.disabled = false;

  if (state.scene === "stage-intro") {
    advanceButton.textContent = state.introReady ? "Briefingへ進む" : "導入を待機中";
    advanceButton.disabled = !state.introReady;
    return;
  }

  if (state.scene === "dialogue-pre" || state.scene === "dialogue-post") {
    const block = getDialogueBlock();
    const isLast = block ? state.dialogueIndex >= block.length - 1 : false;
    advanceButton.textContent = isLast
      ? (state.scene === "dialogue-pre" ? "戦闘を開始" : "クリア表示へ")
      : "次のセリフへ";
    return;
  }

  if (state.scene === "battle") {
    advanceButton.textContent = "戦闘進行中";
    advanceButton.disabled = true;
    return;
  }

  if (state.scene === "stage-clear") {
    advanceButton.textContent = state.clearReady ? `${config.stageLabel}をやり直す` : "集計中";
    advanceButton.disabled = !state.clearReady;
    return;
  }

  if (state.scene === "game-over") {
    advanceButton.textContent = state.gameOverReady ? `${config.stageLabel}を再挑戦` : "復旧中";
    advanceButton.disabled = !state.gameOverReady;
  }
}

function updateHud() {
  stageCount.textContent = config.stageLabel;
  phaseLabel.textContent = getSceneLabel(state.scene);
  lifeCount.textContent = String(state.player.lives);
  bombCount.textContent = String(state.player.bombs);
  scoreCount.textContent = String(state.score);
  shotCount.textContent = String(state.shotsFired);
  enemyLabel.textContent = getEnemyLabel();
  syncFlowUi();
  syncAdvanceUi();
}

function getEnemyLabel() {
  if (state.scene !== "battle") return "None";
  const boss = state.enemies.find((enemy) => enemy.kind === "boss");
  if (boss) return `Boss ${Math.max(0, boss.hp)}`;
  if (state.enemies.length > 0) return `Zako x${state.enemies.length}`;
  return "Incoming";
}

function advanceScene() {
  if (state.scene === "stage-intro" && state.introReady) {
    state.scene = "dialogue-pre";
    state.sceneTimer = 0;
    state.dialogueIndex = 0;
    syncDialogue();
    updateHud();
    return;
  }

  const block = getDialogueBlock();
  if (block) {
    if (state.dialogueIndex < block.length - 1) {
      state.dialogueIndex += 1;
      syncDialogue();
      updateHud();
      return;
    }

    if (state.scene === "dialogue-pre") {
      startBattle();
      return;
    }

    if (state.scene === "dialogue-post") {
      state.scene = "stage-clear";
      state.sceneTimer = 0;
      state.overlayMessage = `${config.stageLabel} Clear`;
      state.clearReady = false;
      syncDialogue();
      updateHud();
      return;
    }
  }

  if (state.scene === "stage-clear" && state.clearReady) {
    resetForStage();
    return;
  }

  if (state.scene === "game-over" && state.gameOverReady) {
    resetForStage();
  }
}

function updatePlayer(deltaTime) {
  let moveX = 0;
  let moveY = 0;

  if (keys.has("ArrowLeft") || keys.has("a")) moveX -= 1;
  if (keys.has("ArrowRight") || keys.has("d")) moveX += 1;
  if (keys.has("ArrowUp") || keys.has("w")) moveY -= 1;
  if (keys.has("ArrowDown") || keys.has("s")) moveY += 1;

  const length = Math.hypot(moveX, moveY) || 1;
  const speed = keys.has("Shift") ? state.player.focusSpeed : state.player.speed;
  state.player.x += (moveX / length) * speed * deltaTime;
  state.player.y += (moveY / length) * speed * deltaTime;
  state.player.x = clamp(state.player.x, 28, canvas.width - 28);
  state.player.y = clamp(state.player.y, 44, canvas.height - 28);

  state.player.cooldown = Math.max(0, state.player.cooldown - deltaTime);
  state.player.bombCooldown = Math.max(0, state.player.bombCooldown - deltaTime);
  state.player.invulnerability = Math.max(0, state.player.invulnerability - deltaTime);

  if (keys.has(" ") && state.player.cooldown <= 0) {
    firePlayerShot();
    state.player.cooldown = config.player.shotCooldown;
  }

  if ((keys.has("b") || keys.has("B")) && state.player.bombs > 0 && state.player.bombCooldown <= 0) {
    activateBomb();
  }
}

function firePlayerShot() {
  for (const shot of config.player.shots) {
    state.playerShots.push({
      x: state.player.x + shot.offsetX,
      y: state.player.y + shot.offsetY,
      vy: shot.speed,
      radius: shot.radius,
      damage: shot.damage,
    });
  }
  state.shotsFired += config.player.shots.length;
  updateHud();
}

function activateBomb() {
  state.player.bombs -= 1;
  state.player.bombCooldown = config.player.bombCooldown;
  state.player.invulnerability = config.player.bombInvulnerabilitySeconds;
  state.enemyShots = [];
  state.effects.push({ kind: "bomb", x: state.player.x, y: state.player.y, radius: 12, maxRadius: 280, life: 0.5 });

  for (const enemy of state.enemies) {
    enemy.hp -= enemy.kind === "boss" ? config.battle.bombBossDamage : config.battle.bombMobDamage;
  }

  cleanupEnemies();
  updateHud();
}

function spawnMob() {
  const lane = state.battle.waveIndex % 4;
  const direction = lane % 2 === 0 ? 1 : -1;
  const prototype = config.battle.mob;
  state.enemies.push({
    kind: "mob",
    x: prototype.startX + lane * 190,
    y: prototype.startY,
    width: prototype.width,
    height: prototype.height,
    hp: prototype.hitPoints,
    speedY: prototype.speedY,
    drift: direction * prototype.drift,
    phase: Math.random() * TAU,
    fireCooldown: config.battle.mobFireCooldownSeconds,
  });
  state.battle.waveIndex += 1;
}

function spawnBoss() {
  const prototype = config.battle.boss;
  state.enemies.push({
    kind: "boss",
    x: prototype.startX,
    y: prototype.startY,
    width: prototype.width,
    height: prototype.height,
    hp: prototype.hitPoints,
    phase: 0,
    fireCooldown: config.battle.bossAimedCooldownSeconds,
    burstCooldown: config.battle.bossRadialCooldownSeconds,
  });
  state.battle.bossSpawned = true;
}

function updateBattle(deltaTime) {
  state.battle.stageTime += deltaTime;
  state.sceneTimer += deltaTime;

  updatePlayer(deltaTime);
  updatePlayerShots(deltaTime);
  updateEnemyShots(deltaTime);
  updateEffects(deltaTime);

  if (!state.battle.bossSpawned) {
    state.battle.spawnTimer -= deltaTime;
    if (state.battle.spawnTimer <= 0) {
      spawnMob();
      state.battle.spawnTimer = config.battle.mobSpawnIntervalSeconds;
    }

    if (state.battle.stageTime >= config.battle.bossSpawnAfterSeconds) {
      spawnBoss();
    }
  }

  updateEnemies(deltaTime);
  resolveCombat();
  cleanupEnemies();
  updateHud();

  if (state.player.lives <= 0) {
    state.scene = "game-over";
    state.sceneTimer = 0;
    state.overlayMessage = "Mission Failed";
    state.gameOverReady = false;
    syncDialogue();
    updateHud();
    return;
  }

  if (state.battle.bossSpawned && state.battle.bossDefeated && state.enemyShots.length === 0) {
    state.scene = "dialogue-post";
    state.sceneTimer = 0;
    state.dialogueIndex = 0;
    syncDialogue();
    updateHud();
  }
}

function updatePlayerShots(deltaTime) {
  state.playerShots = state.playerShots.filter((shot) => {
    shot.y += shot.vy * deltaTime;
    return shot.y > -24;
  });
}

function updateEnemyShots(deltaTime) {
  state.enemyShots = state.enemyShots.filter((shot) => {
    shot.x += shot.vx * deltaTime;
    shot.y += shot.vy * deltaTime;
    return shot.y < canvas.height + 30 && shot.x > -30 && shot.x < canvas.width + 30;
  });
}

function updateEffects(deltaTime) {
  state.effects = state.effects.filter((effect) => {
    effect.life -= deltaTime;
    if (effect.kind === "bomb") {
      effect.radius = lerp(effect.radius, effect.maxRadius, 0.22);
    }
    return effect.life > 0;
  });
}

function updateEnemies(deltaTime) {
  for (const enemy of state.enemies) {
    if (enemy.kind === "mob") {
      enemy.y += enemy.speedY * deltaTime;
      enemy.phase += deltaTime * 2.1;
      enemy.x += Math.sin(enemy.phase) * enemy.drift * deltaTime;
      enemy.fireCooldown -= deltaTime;
      if (enemy.fireCooldown <= 0) {
        fireAimedSpread(enemy.x, enemy.y + 8, config.battle.mobBulletCount, config.battle.mobBulletSpeed, config.battle.mobSpreadAngleRadians);
        enemy.fireCooldown = config.battle.mobFireCooldownSeconds;
      }
    } else if (enemy.kind === "boss") {
      const bossConfig = config.battle.boss;
      if (enemy.y < bossConfig.entranceY) {
        enemy.y += bossConfig.speedY * deltaTime;
      } else {
        enemy.phase += deltaTime;
        enemy.x = bossConfig.startX + Math.sin(enemy.phase * 0.9) * bossConfig.drift;
      }

      enemy.fireCooldown -= deltaTime;
      enemy.burstCooldown -= deltaTime;

      if (enemy.y >= bossConfig.entranceY && enemy.fireCooldown <= 0) {
        fireAimedSpread(enemy.x, enemy.y + 24, config.battle.bossAimedBulletCount, config.battle.bossAimedBulletSpeed, config.battle.bossAimedSpreadRadians);
        enemy.fireCooldown = config.battle.bossAimedCooldownSeconds;
      }

      if (enemy.y >= bossConfig.entranceY && enemy.burstCooldown <= 0) {
        fireRadialBurst(enemy.x, enemy.y + 12, config.battle.bossRadialBulletCount, config.battle.bossRadialBulletSpeed);
        enemy.burstCooldown = config.battle.bossRadialCooldownSeconds;
      }
    }
  }
}

function fireAimedSpread(originX, originY, count, speed, spread) {
  const baseAngle = Math.atan2(state.player.y - originY, state.player.x - originX);
  const middle = (count - 1) / 2;
  for (let i = 0; i < count; i += 1) {
    const angle = baseAngle + (i - middle) * spread;
    state.enemyShots.push({
      x: originX,
      y: originY,
      vx: Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed,
      radius: 6,
      color: i === Math.round(middle) ? "#ffd166" : "#ff7b91",
    });
  }
}

function fireRadialBurst(originX, originY, count, speed) {
  for (let i = 0; i < count; i += 1) {
    const angle = (TAU * i) / count + state.sceneTimer * 0.7;
    state.enemyShots.push({
      x: originX,
      y: originY,
      vx: Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed,
      radius: 5,
      color: "#6ee7ff",
    });
  }
}

function resolveCombat() {
  for (const enemy of state.enemies) {
    state.playerShots = state.playerShots.filter((shot) => {
      const hit =
        shot.x >= enemy.x - enemy.width / 2 &&
        shot.x <= enemy.x + enemy.width / 2 &&
        shot.y >= enemy.y - enemy.height / 2 &&
        shot.y <= enemy.y + enemy.height / 2;

      if (hit) {
        enemy.hp -= shot.damage;
        state.score += enemy.kind === "boss" ? 5 : 12;
      }

      return !hit;
    });
  }

  for (const shot of state.enemyShots) {
    const hitPlayer = distance(shot.x, shot.y, state.player.x, state.player.y) <= shot.radius + state.player.radius;
    if (hitPlayer && state.player.invulnerability <= 0) {
      onPlayerHit();
      break;
    }
  }

  for (const enemy of state.enemies) {
    const hitPlayer =
      Math.abs(enemy.x - state.player.x) <= enemy.width / 2 + state.player.radius &&
      Math.abs(enemy.y - state.player.y) <= enemy.height / 2 + state.player.radius;
    if (hitPlayer && state.player.invulnerability <= 0) {
      onPlayerHit();
      break;
    }
  }
}

function onPlayerHit() {
  state.player.lives -= 1;
  state.player.invulnerability = config.player.respawnInvulnerabilitySeconds;
  state.player.x = canvas.width / 2;
  state.player.y = canvas.height - 88;
  state.enemyShots = [];
  state.playerShots = [];
  state.effects.push({ kind: "hit", x: state.player.x, y: state.player.y, radius: 14, maxRadius: 48, life: 0.32 });
  updateHud();
}

function cleanupEnemies() {
  const survivors = [];
  for (const enemy of state.enemies) {
    const escaped = enemy.kind === "mob" && enemy.y > canvas.height + 48;
    const defeated = enemy.hp <= 0;

    if (defeated) {
      state.effects.push({
        kind: "burst",
        x: enemy.x,
        y: enemy.y,
        radius: enemy.kind === "boss" ? 26 : 14,
        maxRadius: enemy.kind === "boss" ? 120 : 46,
        life: enemy.kind === "boss" ? 0.85 : 0.38,
      });
      state.score += enemy.kind === "boss" ? 2000 : 120;
      if (enemy.kind === "boss") {
        state.battle.bossDefeated = true;
      }
      continue;
    }

    if (!escaped) {
      survivors.push(enemy);
    }
  }

  state.enemies = survivors;
}

function updateScene(deltaTime) {
  state.sceneTimer += deltaTime;

  if (state.scene === "stage-intro") {
    if (!state.introReady && state.sceneTimer >= 1.4) {
      state.introReady = true;
      updateHud();
    }
  } else if (state.scene === "battle") {
    updateBattle(deltaTime);
  } else if (state.scene === "stage-clear") {
    if (!state.clearReady && state.sceneTimer >= 1.4) {
      state.clearReady = true;
      updateHud();
    }
  } else if (state.scene === "game-over") {
    if (!state.gameOverReady && state.sceneTimer >= 1.2) {
      state.gameOverReady = true;
      updateHud();
    }
  }
}

function drawBackground(time) {
  const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height);
  gradient.addColorStop(0, "#08111a");
  gradient.addColorStop(0.55, "#09121d");
  gradient.addColorStop(1, "#03060b");
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.save();
  ctx.globalAlpha = 0.16;
  for (let i = 0; i < 22; i += 1) {
    const y = (i * 56 + time * 42) % (canvas.height + 80) - 40;
    ctx.fillStyle = i % 2 === 0 ? "#173149" : "#0c1c2d";
    ctx.fillRect(0, y, canvas.width, 1);
  }
  ctx.restore();

  ctx.save();
  ctx.strokeStyle = "rgba(110, 231, 255, 0.08)";
  ctx.lineWidth = 1;
  for (let x = 40; x < canvas.width; x += 80) {
    ctx.beginPath();
    ctx.moveTo(x, 0);
    ctx.lineTo(x, canvas.height);
    ctx.stroke();
  }
  ctx.restore();
}

function drawPlayer() {
  if (state.scene !== "battle") return;

  ctx.save();
  ctx.translate(state.player.x, state.player.y);
  if (state.player.invulnerability > 0 && Math.floor(state.player.invulnerability * 12) % 2 === 0) {
    ctx.globalAlpha = 0.45;
  }
  ctx.beginPath();
  ctx.fillStyle = "#eef4ff";
  ctx.moveTo(0, -20);
  ctx.lineTo(14, 16);
  ctx.lineTo(0, 8);
  ctx.lineTo(-14, 16);
  ctx.closePath();
  ctx.fill();

  ctx.beginPath();
  ctx.fillStyle = "#6ee7ff";
  ctx.arc(0, 0, 4, 0, TAU);
  ctx.fill();

  if (keys.has("Shift")) {
    ctx.strokeStyle = "rgba(110, 231, 255, 0.72)";
    ctx.beginPath();
    ctx.arc(0, 0, 18, 0, TAU);
    ctx.stroke();
  }
  ctx.restore();
}

function drawPlayerShots() {
  ctx.fillStyle = "#ffd166";
  for (const shot of state.playerShots) {
    ctx.beginPath();
    ctx.arc(shot.x, shot.y, shot.radius, 0, TAU);
    ctx.fill();
  }
}

function drawEnemyShots() {
  for (const shot of state.enemyShots) {
    ctx.beginPath();
    ctx.fillStyle = shot.color;
    ctx.arc(shot.x, shot.y, shot.radius, 0, TAU);
    ctx.fill();
  }
}

function drawEnemies() {
  for (const enemy of state.enemies) {
    ctx.save();
    ctx.translate(enemy.x, enemy.y);
    if (enemy.kind === "mob") {
      ctx.fillStyle = "#ff7b91";
      ctx.fillRect(-enemy.width / 2, -enemy.height / 2, enemy.width, enemy.height);
      ctx.strokeStyle = "rgba(255, 255, 255, 0.16)";
      ctx.strokeRect(-enemy.width / 2, -enemy.height / 2, enemy.width, enemy.height);
    } else {
      ctx.fillStyle = "#732eff";
      roundRect(ctx, -enemy.width / 2, -enemy.height / 2, enemy.width, enemy.height, 22);
      ctx.fill();
      ctx.fillStyle = "#ffd166";
      ctx.fillRect(-40, -8, 80, 16);
      ctx.strokeStyle = "rgba(255, 255, 255, 0.22)";
      ctx.strokeRect(-enemy.width / 2, -enemy.height / 2, enemy.width, enemy.height);

      const hpRatio = clamp(enemy.hp / config.battle.boss.hitPoints, 0, 1);
      ctx.fillStyle = "rgba(8, 13, 20, 0.9)";
      ctx.fillRect(-70, -74, 140, 10);
      ctx.fillStyle = "#ff5c8a";
      ctx.fillRect(-70, -74, 140 * hpRatio, 10);
    }
    ctx.restore();
  }
}

function drawEffects() {
  for (const effect of state.effects) {
    ctx.save();
    ctx.translate(effect.x, effect.y);
    if (effect.kind === "bomb") {
      ctx.strokeStyle = "rgba(110, 231, 255, 0.75)";
      ctx.lineWidth = 4;
      ctx.beginPath();
      ctx.arc(0, 0, effect.radius, 0, TAU);
      ctx.stroke();
    } else if (effect.kind === "burst") {
      ctx.fillStyle = "rgba(255, 209, 102, 0.25)";
      ctx.beginPath();
      ctx.arc(0, 0, lerp(effect.radius, effect.maxRadius, 0.5), 0, TAU);
      ctx.fill();
    } else if (effect.kind === "hit") {
      ctx.strokeStyle = "rgba(255, 107, 107, 0.85)";
      ctx.lineWidth = 3;
      ctx.beginPath();
      ctx.arc(0, 0, lerp(effect.radius, effect.maxRadius, 0.55), 0, TAU);
      ctx.stroke();
    }
    ctx.restore();
  }
}

function drawOverlay() {
  if (state.scene === "battle") {
    ctx.fillStyle = "rgba(255, 255, 255, 0.88)";
    ctx.font = '14px "Arial Black", sans-serif';
    ctx.fillText(`${config.stageLabel} Prototype Battle`, 24, canvas.height - 24);
    return;
  }

  ctx.save();
  ctx.fillStyle = "rgba(4, 8, 14, 0.62)";
  ctx.fillRect(0, 0, canvas.width, canvas.height);
  ctx.fillStyle = "#eef4ff";
  ctx.textAlign = "center";
  ctx.font = '18px "Arial Black", sans-serif';
  ctx.fillText("BullethellPrototype", canvas.width / 2, 178);
  ctx.font = '58px "Arial Black", sans-serif';
  ctx.fillText(state.overlayMessage, canvas.width / 2, canvas.height / 2 - 12);
  ctx.font = '18px "Hiragino Sans", sans-serif';

  if (state.scene === "stage-intro") {
    ctx.fillText(state.introReady ? "Press Enter to start the briefing" : "Synchronizing stage feed...", canvas.width / 2, canvas.height / 2 + 54);
  } else if (state.scene === "dialogue-pre") {
    ctx.fillText("Briefing in progress", canvas.width / 2, canvas.height / 2 + 54);
  } else if (state.scene === "dialogue-post") {
    ctx.fillText("Debriefing in progress", canvas.width / 2, canvas.height / 2 + 54);
  } else if (state.scene === "stage-clear") {
    ctx.fillText(state.clearReady ? `Press Enter to replay ${config.stageLabel}` : "Mission data updating...", canvas.width / 2, canvas.height / 2 + 54);
  } else if (state.scene === "game-over") {
    ctx.fillText(state.gameOverReady ? `Press Enter to retry ${config.stageLabel}` : "Recovering support systems...", canvas.width / 2, canvas.height / 2 + 54);
  }
  ctx.restore();
}

function drawDialogueStatus() {
  const block = getDialogueBlock();
  if (!block) return;

  ctx.save();
  ctx.fillStyle = "rgba(6, 11, 17, 0.74)";
  ctx.fillRect(24, canvas.height - 116, canvas.width - 48, 80);
  ctx.strokeStyle = "rgba(110, 231, 255, 0.18)";
  ctx.strokeRect(24, canvas.height - 116, canvas.width - 48, 80);
  ctx.fillStyle = "#6ee7ff";
  ctx.font = '16px "Arial Black", sans-serif';
  ctx.fillText(speakerLabel.textContent, 42, canvas.height - 84);
  ctx.fillStyle = "#eef4ff";
  ctx.font = '15px "Hiragino Sans", sans-serif';
  wrapText(dialogueText.textContent, 42, canvas.height - 58, canvas.width - 84, 22);
  ctx.restore();
}

function render(timestamp) {
  if (!config) return;
  if (!state.lastTimestamp) state.lastTimestamp = timestamp;
  const deltaTime = Math.min((timestamp - state.lastTimestamp) / 1000, 0.033);
  state.lastTimestamp = timestamp;

  updateScene(deltaTime);
  drawBackground(timestamp / 1000);
  drawEnemies();
  drawEnemyShots();
  drawPlayerShots();
  drawEffects();
  drawPlayer();
  drawDialogueStatus();
  drawOverlay();

  state.fps = Math.round(1 / Math.max(deltaTime, 0.0001));
  fpsCount.textContent = String(state.fps);
  requestAnimationFrame(render);
}

function wrapText(text, x, y, maxWidth, lineHeight) {
  const words = text.split(" ");
  let line = "";
  let offsetY = 0;

  for (const word of words) {
    const testLine = line ? `${line} ${word}` : word;
    if (ctx.measureText(testLine).width > maxWidth && line) {
      ctx.fillText(line, x, y + offsetY);
      line = word;
      offsetY += lineHeight;
    } else {
      line = testLine;
    }
  }

  if (line) {
    ctx.fillText(line, x, y + offsetY);
  }
}

function roundRect(context, x, y, width, height, radius) {
  context.beginPath();
  context.moveTo(x + radius, y);
  context.arcTo(x + width, y, x + width, y + height, radius);
  context.arcTo(x + width, y + height, x, y + height, radius);
  context.arcTo(x, y + height, x, y, radius);
  context.arcTo(x, y, x + width, y, radius);
  context.closePath();
}

function lerp(a, b, t) {
  return a + (b - a) * t;
}

function distance(ax, ay, bx, by) {
  return Math.hypot(ax - bx, ay - by);
}

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}

function onKeyDown(event) {
  keys.add(event.key);
  if (event.key === "Enter" || event.key === "z" || event.key === "Z") {
    advanceScene();
  }
}

function onKeyUp(event) {
  keys.delete(event.key);
}

advanceButton.addEventListener("click", advanceScene);
window.addEventListener("keydown", onKeyDown);
window.addEventListener("keyup", onKeyUp);

bootstrap().catch((error) => {
  console.error(error);
  dialogueText.textContent = "ステージ定義の読み込みに失敗しました。";
  flowSummary.textContent = "C# 側の共有定義を確認してください。";
  advanceButton.disabled = true;
});
