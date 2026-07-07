# Build Scripts

Scripts PowerShell para compilar, testar, publicar e versionar o SwitchcraftKeys.

Todos os scripts devem ser executados a partir da **raiz do repositório** (`D:\workspace\_non_work_\SwitchcraftKeys\`).

## Scripts disponíveis

### `build.ps1` — Compilar
Compila a solução inteira.

```powershell
# Debug (padrão)
.\build\build.ps1

# Release
.\build\build.ps1 -Config Release
```

Parâmetros:
- `-Config Debug|Release` — configuração de build (padrão: `Debug`)

---

### `clean.ps1` — Limpar artefatos
Remove todos os artefatos de build locais.

```powershell
.\build\clean.ps1
```

Remove:
- `src/**/bin/`
- `src/**/obj/`
- `dist/`
- `TestResults/`

---

### `publish.ps1` — Gerar executável
Gera o `.exe` single-file em `dist/`.

```powershell
# Lê versão do .csproj automaticamente
.\build\publish.ps1

# Versão explícita
.\build\publish.ps1 -Version 0.2.0
```

Output: `dist/switchcraft-keys-v{version}-win-x64.exe`

Parâmetros:
- `-Version` — sobrescreve a versão lida do `.csproj`

---

### `test.ps1` — Rodar testes
Executa os testes unitários.

```powershell
# Só testes
.\build\test.ps1

# Com relatório de coverage HTML
.\build\test.ps1 -Coverage
```

Parâmetros:
- `-Coverage` — gera relatório HTML em `TestResults/coverage/`

---

### `version.ps1` — Gerenciar versão
Atualiza a versão no `.csproj` e no `CHANGELOG.md`.

```powershell
# Bump automático (patch: 0.1.0 → 0.1.1)
.\build\version.ps1 -Bump patch

# Bump minor (0.1.0 → 0.2.0)
.\build\version.ps1 -Bump minor

# Bump major (0.1.0 → 1.0.0)
.\build\version.ps1 -Bump major

# Versão explícita
.\build\version.ps1 -Set 1.0.0
```

Parâmetros:
- `-Bump patch|minor|major` — incrementa a versão
- `-Set x.y.z` — define versão explícita
- `-Tag` — cria git tag local `v{version}` (opcional, padrão: não cria)

---

## Fluxo de release

```powershell
# 1. Bump versão
.\build\version.ps1 -Bump patch

# 2. Rodar testes
.\build\test.ps1

# 3. Gerar executável
.\build\publish.ps1

# 4. Verificar dist/
Get-ChildItem dist/
```
