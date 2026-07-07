---
layout: default
title: Config & Backup
description: JSON persistence and backup recovery
---

# Config & Backup

Config is local, per-user, and automatically saved.

## Location

```text
%APPDATA%\SwitchcraftKeys\config.json
```

## Schema

```json
{
  "version": 1,
  "devices": {
    "VID_046D&PID_B380": {
      "alias": "LOGI MX2",
      "layoutKlid": "040A0C0A"
    }
  },
  "logging": {
    "minimumLevel": "Trace"
  },
  "ui": {
    "startMinimized": true
  }
}
```

## Save Flow

```mermaid
flowchart TD
    Change[Alias or layout changed] --> Save[ConfigService.Save]
    Save --> Rotate[Rotate backups]
    Rotate --> Tmp[Write config.json.tmp]
    Tmp --> Replace[Replace config.json]
```

## Backup Rotation

```mermaid
flowchart LR
    Active[config.json] --> Bak1[config.json.bak1]
    Bak1 --> Bak2[config.json.bak2]
    Bak2 --> Bak3[config.json.bak3]
    Bak3 --> Delete[delete old]
```

## Recovery

```mermaid
flowchart TD
    Load[Load config.json] --> Valid{Valid JSON?}
    Valid -->|Yes| Use[Use primary]
    Valid -->|No| B1[Try bak1]
    B1 --> B2[Try bak2]
    B2 --> B3[Try bak3]
    B3 --> Fresh[Start with defaults]
```
