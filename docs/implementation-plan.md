---
layout: default
title: Implementation Plan
description: Phase checklist and implementation status
---

# Implementation Plan

## Phase Status

```mermaid
gantt
    title SwitchcraftKeys Implementation Phases
    dateFormat  YYYY-MM-DD
    axisFormat  %b %d
    section Complete
    Scaffolding              :done, p0, 2026-07-01, 1d
    Logging + config         :done, p05, after p0, 1d
    Interop + core logic     :done, p1, after p05, 2d
    Services + persistence   :done, p2, after p1, 2d
    section Current
    Dashboard + Luna theme   :active, p3, after p2, 3d
    section Next
    Release polish           :p4, after p3, 2d
```

## Checklist

| Phase | Status | Scope |
|-------|--------|-------|
| 0 | Complete | Solution, projects, scripts |
| 0.5 | Complete | Logging, config, preflight |
| 1 | Complete | Raw Input and layout interop |
| 2 | Complete | Services, config, single instance |
| 3 | In progress | Dashboard, tray, settings, toasts |
| 4 | Planned | Docs, screenshots, release polish |

## Implementation Rule

Keep each phase runnable. Avoid large refactors while porting core behavior.
