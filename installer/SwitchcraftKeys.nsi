; SwitchcraftKeys — NSIS Installer
; Usage: makensis /DVERSION=0.1.0 SwitchcraftKeys.nsi

!ifndef VERSION
  !define VERSION "0.1.0"
!endif

!define APPNAME "SwitchcraftKeys"
!define COMPANYNAME "Marcelo Frau"
!define DESCRIPTION "Device-aware keyboard layout manager for Windows"
!define VERSIONBUILD 0
!define HELPURL "https://github.com/marcelofrau/switchcraft-keys"
!define UPDATEURL "https://github.com/marcelofrau/switchcraft-keys/releases"
!define ABOUTURL "https://github.com/marcelofrau/switchcraft-keys"
!define INSTALLSIZE 100000

RequestExecutionLevel admin

InstallDir "$PROGRAMFILES64\${APPNAME}"

Name "${APPNAME} v${VERSION}"
OutFile "..\dist\${APPNAME}-v${VERSION}-win-x64-setup.exe"

!include "MUI2.nsh"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "PortugueseBR"

!define MUI_FINISHPAGE_RUN "$INSTDIR\${APPNAME}.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Launch ${APPNAME}"

Section "Install"
  SetOutPath "$INSTDIR"
  
  File /r "..\dist\publish\*.*"
  
  CreateDirectory "$SMPROGRAMS\${APPNAME}"
  CreateShortCut "$SMPROGRAMS\${APPNAME}\${APPNAME}.lnk" "$INSTDIR\${APPNAME}.exe" "" "$INSTDIR\${APPNAME}.exe" 0
  CreateShortCut "$SMPROGRAMS\${APPNAME}\Uninstall.lnk" "$INSTDIR\Uninstall.exe" "" "$INSTDIR\Uninstall.exe" 0
  
  CreateShortCut "$DESKTOP\${APPNAME}.lnk" "$INSTDIR\${APPNAME}.exe" "" "$INSTDIR\${APPNAME}.exe" 0
  
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayName" "${APPNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "DisplayIcon" "$INSTDIR\${APPNAME}.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "Publisher" "${COMPANYNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLInfoAbout" "${ABOUTURL}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "URLUpdateInfo" "${UPDATEURL}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "UninstallString" "$INSTDIR\Uninstall.exe"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" "EstimatedSize" ${INSTALLSIZE}
SectionEnd

Section "Uninstall"
  RMDir /r "$SMPROGRAMS\${APPNAME}"
  RMDir /r "$INSTDIR"
  Delete "$DESKTOP\${APPNAME}.lnk"
  
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"
SectionEnd
