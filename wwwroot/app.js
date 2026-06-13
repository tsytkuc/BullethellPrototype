const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

const patternSelect = document.getElementById("patternSelect");
const playbackRange = document.getElementById("playbackRange");
const densityRange = document.getElementById("densityRange");
const bulletSpeedRange = document.getElementById("bulletSpeedRange");
const toggleButton = document.getElementById("toggleButton");
const restartButton = document.getElementById("restartButton");
const spawnCount = document.getElementById("spawnCount");
const fpsCount = document.getElementById("fpsCount");
const patternDescription = document.getElementById("patternDescription");
const densityValue = document.getElementById("densityValue");
const bulletSpeedValue = document.getElementById("bulletSpeedValue");

const TAU = Math.PI * 2;
const arenaPadding = 48;
const keys = new Set();

const state = {
  running: true,
  playbackSpeed: Number(playbackRange.value),
  densityPercent: Number(densityRange.value),
  bulletSpeedPercent: Number(bulletSpeedRange.value),
  lastTimestamp: 0,
  simTime: 0,
  fps: 0,
  bullets: [],
  nextSpawnIndex: 0,
  patternSummaries: [],
  sample: null,
  player: {
    x: canvas.width / 2,
    y: canvas.height - 90,
    radius: 7,
    speed: 280,
    focusSpeed: 150,
  },
};

updateSliderLabels();

async function loadPatterns() {
  const response = await fetch("/api/patterns");
  state.patternSummaries = await response.json();

  patternSelect.innerHTML = "";
  for (const pattern of state.patternSummaries) {
    const option = document.createElement("option");
    option.value = pattern.id;
    option.textContent = pattern.label;
    patternSelect.appendChild(option);
  }

  if (state.patternSummaries.length > 0) {
    patternSelect.value = state.patternSummaries[0].id;
    await loadPatternSample(patternSelect.value);
  }
}

async function loadPatternSample(id) {
  const response = await fetch(`/api/patterns/${id}`);
  if (!response.ok) {
    throw new Error(`Failed to load pattern: ${id}`);
  }

  state.sample = await response.json();
  state.running = true;
  patternDescription.textContent = state.sample.description;
  updateSpawnCount();
  toggleButton.textContent = "Pause";
  restartSimulation();
}

function restartSimulation() {
  state.simTime = 0;
  state.bullets.length = 0;
  state.nextSpawnIndex = 0;
  state.player.x = canvas.width / 2;
  state.player.y = canvas.height - 90;
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

  state.player.x = clamp(state.player.x, arenaPadding, canvas.width - arenaPadding);
  state.player.y = clamp(state.player.y, arenaPadding, canvas.height - arenaPadding);
}

function emitScheduledBullets(previousTime, currentTime) {
  if (!state.sample) {
    return;
  }

  const spawns = state.sample.spawns;

  while (state.nextSpawnIndex < spawns.length && spawns[state.nextSpawnIndex].spawnTime <= currentTime) {
    const spawn = spawns[state.nextSpawnIndex];
    if (spawn.spawnTime > previousTime && shouldEmitSpawn(state.nextSpawnIndex)) {
      state.bullets.push({
        x: spawn.x,
        y: spawn.y,
        baseVx: spawn.directionX * spawn.speed,
        baseVy: spawn.directionY * spawn.speed,
        angle: spawn.angle,
        radius: 4.5,
        hue: (spawn.angle % 360 + 360) % 360,
      });
    }
    state.nextSpawnIndex += 1;
  }
}

function updateBullets(deltaTime) {
  const speedMultiplier = state.bulletSpeedPercent / 100;
  state.bullets = state.bullets.filter((bullet) => {
    bullet.x += bullet.baseVx * speedMultiplier * deltaTime;
    bullet.y += bullet.baseVy * speedMultiplier * deltaTime;

    return (
      bullet.x > -80 &&
      bullet.x < canvas.width + 80 &&
      bullet.y > -80 &&
      bullet.y < canvas.height + 80
    );
  });
}

function drawBackground(time) {
  const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height);
  gradient.addColorStop(0, "#091321");
  gradient.addColorStop(1, "#030711");
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, canvas.width, canvas.height);

  ctx.save();
  ctx.globalAlpha = 0.12;
  for (let i = 0; i < 24; i += 1) {
    const x = (i * 97 + time * 18) % (canvas.width + 120) - 60;
    const y = (i * 53) % canvas.height;
    ctx.fillStyle = i % 2 === 0 ? "#7cf0c8" : "#ffb454";
    ctx.fillRect(x, y, 1.5, 1.5);
  }
  ctx.restore();
}

function drawEmitter() {
  if (!state.sample) {
    return;
  }

  ctx.save();
  ctx.translate(state.sample.originX, state.sample.originY);

  ctx.beginPath();
  ctx.fillStyle = "#ffd166";
  ctx.arc(0, 0, 18, 0, TAU);
  ctx.fill();

  ctx.strokeStyle = "rgba(255, 255, 255, 0.5)";
  ctx.lineWidth = 2;
  ctx.beginPath();
  ctx.moveTo(-20, -12);
  ctx.lineTo(0, -26);
  ctx.lineTo(20, -12);
  ctx.stroke();

  if (state.sample.targetX !== null && state.sample.targetY !== null) {
    ctx.setLineDash([6, 6]);
    ctx.strokeStyle = "rgba(255, 107, 122, 0.45)";
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.lineTo(state.sample.targetX - state.sample.originX, state.sample.targetY - state.sample.originY);
    ctx.stroke();
    ctx.setLineDash([]);
  }

  ctx.restore();
}

function drawBullets() {
  for (const bullet of state.bullets) {
    ctx.beginPath();
    ctx.fillStyle = `hsl(${bullet.hue} 85% 68%)`;
    ctx.arc(bullet.x, bullet.y, bullet.radius, 0, TAU);
    ctx.fill();
  }
}

function drawPlayer() {
  ctx.save();
  ctx.translate(state.player.x, state.player.y);

  ctx.beginPath();
  ctx.fillStyle = "#ffffff";
  ctx.moveTo(0, -16);
  ctx.lineTo(12, 14);
  ctx.lineTo(0, 8);
  ctx.lineTo(-12, 14);
  ctx.closePath();
  ctx.fill();

  ctx.beginPath();
  ctx.fillStyle = "#ff6b7a";
  ctx.arc(0, 0, state.player.radius, 0, TAU);
  ctx.fill();

  if (keys.has("Shift")) {
    ctx.strokeStyle = "rgba(124, 240, 200, 0.75)";
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    ctx.arc(0, 0, 18, 0, TAU);
    ctx.stroke();
  }

  ctx.restore();
}

function drawHud() {
  if (!state.sample) {
    return;
  }

  ctx.fillStyle = "rgba(255, 255, 255, 0.85)";
  ctx.font = '14px "Arial Black", sans-serif';
  ctx.fillText(`Pattern: ${state.sample.label}`, 24, canvas.height - 24);
}

function draw() {
  drawBackground(state.simTime);
  drawEmitter();
  drawBullets();
  drawPlayer();
  drawHud();
}

function tick(timestamp) {
  if (!state.lastTimestamp) {
    state.lastTimestamp = timestamp;
  }

  const deltaTime = Math.min((timestamp - state.lastTimestamp) / 1000, 0.033);
  state.lastTimestamp = timestamp;

  if (state.running && state.sample) {
    const previousTime = state.simTime;
    state.simTime += deltaTime * state.playbackSpeed;

    if (state.simTime > state.sample.duration) {
      if (state.sample.looping) {
        restartSimulation();
      } else {
        state.simTime = state.sample.duration;
        state.running = false;
        toggleButton.textContent = "Resume";
      }
    }

    emitScheduledBullets(previousTime, state.simTime);
    updatePlayer(deltaTime);
    updateBullets(deltaTime);
  }

  state.fps = Math.round(1 / Math.max(deltaTime, 0.0001));
  fpsCount.textContent = String(state.fps);

  draw();
  requestAnimationFrame(tick);
}

function clamp(value, min, max) {
  return Math.max(min, Math.min(max, value));
}

function shouldEmitSpawn(index) {
  if (state.densityPercent >= 100) {
    return true;
  }

  const hash = Math.abs(Math.sin((index + 1) * 12.9898) * 43758.5453);
  return (hash - Math.floor(hash)) * 100 < state.densityPercent;
}

function updateSpawnCount() {
  if (!state.sample) {
    spawnCount.textContent = "0";
    return;
  }

  let count = 0;
  for (let i = 0; i < state.sample.spawns.length; i += 1) {
    if (shouldEmitSpawn(i)) {
      count += 1;
    }
  }

  spawnCount.textContent = String(count);
}

function updateSliderLabels() {
  densityValue.textContent = `${state.densityPercent}%`;
  bulletSpeedValue.textContent = `${state.bulletSpeedPercent}%`;
}

patternSelect.addEventListener("change", async () => {
  await loadPatternSample(patternSelect.value);
});

playbackRange.addEventListener("input", () => {
  state.playbackSpeed = Number(playbackRange.value);
});

densityRange.addEventListener("input", () => {
  state.densityPercent = Number(densityRange.value);
  updateSliderLabels();
  updateSpawnCount();
  restartSimulation();
});

bulletSpeedRange.addEventListener("input", () => {
  state.bulletSpeedPercent = Number(bulletSpeedRange.value);
  updateSliderLabels();
});

toggleButton.addEventListener("click", () => {
  state.running = !state.running;
  toggleButton.textContent = state.running ? "Pause" : "Resume";
});

restartButton.addEventListener("click", () => {
  state.running = true;
  toggleButton.textContent = "Pause";
  restartSimulation();
});

window.addEventListener("keydown", (event) => {
  keys.add(event.key);
});

window.addEventListener("keyup", (event) => {
  keys.delete(event.key);
});

loadPatterns().catch((error) => {
  patternDescription.textContent = `API の読み込みに失敗しました: ${error.message}`;
});

requestAnimationFrame(tick);
