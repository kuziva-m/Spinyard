; Inno Setup script for Spinyard Inventory Management System

[Setup]
AppName=Inventory Management System
AppVersion=1.0
DefaultDirName={autopf}\SpinyardInventory
DefaultGroupName=Spinyard Inventory
UninstallDisplayIcon={app}\Inventory.Presentation.Wpf.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
OutputBaseFilename=SpinyardSetup
OutputDir=.\Installer
AppMutex=SpinyardInventoryMutex

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "Inventory.Presentation.Wpf\build\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; This line tells Inno Setup to grab all the files from the build output folder

[Icons]
Name: "{group}\Inventory Management System"; Filename: "{app}\Inventory.Presentation.Wpf.exe"
Name: "{autodesktop}\Inventory Management System"; Filename: "{app}\Inventory.Presentation.Wpf.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\Inventory.Presentation.Wpf.exe"; Description: "{cm:LaunchProgram,Inventory Management System}"; Flags: nowait postinstall skipifsilent

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";