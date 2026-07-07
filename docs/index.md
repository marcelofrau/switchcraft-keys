---
layout: home
title: Home
description: Device-aware keyboard layout manager for Windows. Automatically switches layouts when you switch keyboards.
---

<!-- Hero Section -->
<section class="hero">
  <div class="hero-content" data-reveal="up">
    <img class="hero-logo" src="{{ '/images/social-preview.png' | relative_url }}" alt="SwitchcraftKeys">
    <p class="tagline">Device-aware keyboard layout manager for Windows.<br>Switch keyboards, layouts follow automatically.</p>
    <div class="hero-buttons">
      <a href="https://github.com/marcelofrau/switchcraft-keys/releases/latest" class="btn btn-primary">
        <img src="{{ '/assets/images/site-icons/feature-package-100.png' | relative_url }}" alt="" class="btn-icon"> Download Latest
      </a>
      <a href="{{ '/docs' | relative_url }}" class="btn btn-secondary">
        <img src="{{ '/assets/images/site-icons/feature-cli-100.png' | relative_url }}" alt="" class="btn-icon"> Documentation
      </a>
    </div>
  </div>
</section>

<!-- Problem / Solution Window -->
<section style="padding: 0 20px 60px; max-width: 800px; margin: 0 auto;">
  <div class="window-panel" data-reveal="up">
    <div class="window-titlebar">
      <img class="window-titlebar-icon" src="{{ '/assets/images/site-icons/feature-keyboard-100.png' | relative_url }}" alt="">
      <span>The Problem</span>
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
      <img class="window-titlebar-icon" src="{{ '/assets/images/site-icons/feature-lightning-100.png' | relative_url }}" alt="">
      <span>The Solution</span>
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
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-search-100.png' | relative_url }}" alt="">
      <h3>Auto-Detection</h3>
      <p>Detects USB keyboards (VID:PID) and built-in keyboards automatically via Raw Input API.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-keyboard-100.png' | relative_url }}" alt="">
      <h3>Per-Device Mappings</h3>
      <p>Assign different layouts to each keyboard. Settings persist across reboots.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-lightning-100.png' | relative_url }}" alt="">
      <h3>Zero Delay</h3>
      <p>Keystroke-driven activation. No polling, minimal latency (<1ms hook latency).</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-package-100.png' | relative_url }}" alt="">
      <h3>Portable</h3>
      <p>Single .exe, no installer required. Runs anywhere, no registry pollution.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-tray-100.png' | relative_url }}" alt="">
      <h3>Tray Integration</h3>
      <p>Minimizes to system tray. Stays out of your way until you need it.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-theme-100.png' | relative_url }}" alt="">
      <h3>Luna Theme</h3>
      <p>Clean Windows XP-inspired interface with soft blues and familiar styling.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-backup-100.png' | relative_url }}" alt="">
      <h3>Config Backup</h3>
      <p>3-version automatic backup with corruption recovery. Never lose your mappings.</p>
    </div>
    <div class="feature-card" data-reveal="up">
      <img class="feature-icon" src="{{ '/assets/images/site-icons/feature-cli-100.png' | relative_url }}" alt="">
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
      <img src="{{ '/screenshots/SwitchcraftKeys_main.png' | relative_url }}" alt="Dashboard" data-caption="Main dashboard showing detected keyboards and active layout state">
      <div class="screenshot-caption">Dashboard</div>
    </div>
    <div class="screenshot-item" data-reveal="up">
      <img src="{{ '/screenshots/SwitchcraftKeys_main2.png' | relative_url }}" alt="Keyboard Details" data-caption="Device details with alias, language, and layout selection">
      <div class="screenshot-caption">Keyboard Details</div>
    </div>
    <div class="screenshot-item" data-reveal="up">
      <img src="{{ '/screenshots/SwitchcraftKeys_settings.png' | relative_url }}" alt="Settings" data-caption="Runtime actions, Windows input method scope, cache reset, and restart controls">
      <div class="screenshot-caption">Settings</div>
    </div>
    <div class="screenshot-item" data-reveal="up">
      <img src="{{ '/screenshots/SwitchcraftKeys_logs.png' | relative_url }}" alt="Logs" data-caption="In-app log stream with structured events and copy/clear tools">
      <div class="screenshot-caption">Logs</div>
    </div>
    <div class="screenshot-item" data-reveal="up">
      <img src="{{ '/screenshots/SwitchcraftKeys_about.png' | relative_url }}" alt="About" data-caption="Version, author, mode, and credits">
      <div class="screenshot-caption">About</div>
    </div>
  </div>
</section>

<!-- Download Section -->
<section class="download-section">
  <h2 data-reveal="up">Download</h2>
  <p data-reveal="up">Get the latest release for Windows 10/11 (x64 or ARM64).</p>
  <div class="download-options" data-stagger>
    <div class="download-card" data-reveal="up">
      <img class="download-icon" src="{{ '/assets/images/site-icons/feature-package-100.png' | relative_url }}" alt="">
      <h3>Portable</h3>
      <p class="size">~15 MB ZIP</p>
      <a href="https://github.com/marcelofrau/switchcraft-keys/releases/latest" class="btn btn-primary">Download ZIP</a>
    </div>
    <div class="download-card" data-reveal="up">
      <img class="download-icon" src="{{ '/assets/images/site-icons/feature-keyboard-100.png' | relative_url }}" alt="">
      <h3>Installer</h3>
      <p class="size">~15 MB EXE</p>
      <a href="https://github.com/marcelofrau/switchcraft-keys/releases/latest" class="btn btn-secondary">Download Setup</a>
    </div>
  </div>
</section>
