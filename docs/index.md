---
layout: home
title: Home
description: Device-aware keyboard layout manager for Windows. Automatically switches layouts when you switch keyboards.
---

<!-- Hero Section -->
<section class="hero">
  <div class="hero-content" data-reveal="up">
    <h1>SwitchcraftKeys</h1>
    <p class="tagline">Device-aware keyboard layout manager for Windows.<br>Switch keyboards, layouts follow automatically.</p>
    <div class="hero-buttons">
      <a href="https://github.com/marcelofrau/switchcraft-keys/releases/latest" class="btn btn-primary">
        ⬇️ Download Latest
      </a>
      <a href="{{ '/docs' | relative_url }}" class="btn btn-secondary">
        📖 Documentation
      </a>
    </div>
  </div>
</section>

<!-- Problem / Solution Window -->
<section style="padding: 0 20px 60px; max-width: 800px; margin: 0 auto;">
  <div class="window-panel" data-reveal="up">
    <div class="window-titlebar">
      <span class="window-titlebar-icon">⌨️</span>
      <span>The Problem</span>
      <div class="window-buttons">
        <span class="window-btn window-btn-minimize">_</span>
        <span class="window-btn window-btn-maximize">□</span>
        <span class="window-btn window-btn-close">×</span>
      </div>
    </div>
    <div class="window-content">
      <p><strong>Windows assigns one keyboard layout globally.</strong></p>
      <p>If you have two physical keyboards — a US layout desktop keyboard and a notebook with a PT-BR layout — Windows does not know which one you are typing on.</p>
      <p>Switching layouts manually every time you change keyboards is tedious and error-prone.</p>
      <pre style="background: #F5F2E7; padding: 12px; border-radius: 4px; font-size: 0.9rem;">
Keyboard 1 (USB, US)  ──┐
                         └─→ Windows: ???
Notebook Keyboard (PT-BR) ─┘

Type on Keyboard 1 → Layout is PT-BR (wrong!)
Type on Notebook   → Layout is US (wrong!)</pre>
    </div>
  </div>

  <div class="window-panel" data-reveal="up">
    <div class="window-titlebar">
      <span class="window-titlebar-icon">✨</span>
      <span>The Solution</span>
      <div class="window-buttons">
        <span class="window-btn window-btn-minimize">_</span>
        <span class="window-btn window-btn-maximize">□</span>
        <span class="window-btn window-btn-close">×</span>
      </div>
    </div>
    <div class="window-content">
      <p><strong>SwitchcraftKeys uses the Windows Raw Input API</strong> to detect which physical keyboard generated each keystroke.</p>
      <p>When you switch keyboards, it automatically activates the layout you previously assigned to that device.</p>
      <pre style="background: #F5F2E7; padding: 12px; border-radius: 4px; font-size: 0.9rem;">
Keyboard 1 (USB, US)  ──┐
                         ├─→ Raw Input detects hDevice
Notebook Keyboard (PT-BR)─┤    ↓
                         ├─→ Map device → layout
                         ├─→ Activate layout
                         └─→ No manual switching!</pre>
    </div>
  </div>
</section>

<!-- Features Section -->
<section class="features-section">
  <h2 data-reveal="up">Features</h2>
  <div class="features-grid" data-stagger>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">🔍</div>
      <h3>Auto-Detection</h3>
      <p>Detects USB keyboards (VID:PID) and built-in keyboards automatically via Raw Input API.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">⌨️</div>
      <h3>Per-Device Mappings</h3>
      <p>Assign different layouts to each keyboard. Settings persist across reboots.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">⚡</div>
      <h3>Zero Delay</h3>
      <p>Keystroke-driven activation. No polling, minimal latency (<1ms hook latency).</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">📦</div>
      <h3>Portable</h3>
      <p>Single .exe, no installer required. Runs anywhere, no registry pollution.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">🔔</div>
      <h3>Tray Integration</h3>
      <p>Minimizes to system tray. Stays out of your way until you need it.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">🎨</div>
      <h3>Luna Theme</h3>
      <p>Clean Windows XP-inspired interface with soft blues and familiar styling.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">💾</div>
      <h3>Config Backup</h3>
      <p>3-version automatic backup with corruption recovery. Never lose your mappings.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <div class="feature-icon">🛠️</div>
      <h3>CLI Support</h3>
      <p>Full command-line interface for automation, health checks, and data reset.</p>
    </div>
  </div>
</section>

<!-- Screenshots Section -->
<section class="screenshots-section">
  <h2 data-reveal="up">Screenshots</h2>
  <div class="screenshots-gallery" data-stagger>
    <div class="screenshot-item" data-reveal="up">
      <img src="{{ '/screenshots/dashboard.png' | relative_url }}" alt="Dashboard" data-caption="Main dashboard showing detected keyboards and layout assignments">
      <div class="screenshot-caption">Dashboard</div>
    </div>
    <div class="screenshot-item" data-reveal="up">
      <img src="{{ '/screenshots/tray-menu.png' | relative_url }}" alt="System Tray" data-caption="System tray integration with quick access menu">
      <div class="screenshot-caption">System Tray</div>
    </div>
    <div class="screenshot-item" data-reveal="up">
      <img src="{{ '/screenshots/debug-overlay.png' | relative_url }}" alt="Debug Overlay" data-caption="Real-time debug overlay for troubleshooting">
      <div class="screenshot-caption">Debug Overlay</div>
    </div>
  </div>
</section>

<!-- Download Section -->
<section class="download-section">
  <h2 data-reveal="up">Download</h2>
  <p data-reveal="up">Get the latest release for Windows 10/11 (x64 or ARM64).</p>
  <div class="download-options" data-stagger>
    <div class="download-card" data-reveal="up">
      <h3>📦 Portable</h3>
      <p class="size">~15 MB ZIP</p>
      <a href="https://github.com/marcelofrau/switchcraft-keys/releases/latest" class="btn btn-primary">Download ZIP</a>
    </div>
    <div class="download-card" data-reveal="up">
      <h3>🖥️ Installer</h3>
      <p class="size">~15 MB EXE</p>
      <a href="https://github.com/marcelofrau/switchcraft-keys/releases/latest" class="btn btn-secondary">Download Setup</a>
    </div>
  </div>
</section>
