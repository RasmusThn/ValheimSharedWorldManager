VALHEIM SHARED WORLD MANAGER
============================

A Windows app for sharing a Valheim world through OneDrive and safely taking turns hosting it.

NO POWERSHELL REQUIRED
----------------------
The published app is a normal Windows .exe.
PowerShell execution policies do not matter for the finished app.

MAIN WORKFLOW
-------------
1. Both players have access to the same shared OneDrive folder.
2. Open Valheim Shared World Manager.
3. Choose the world you want to use.
4. If nobody is hosting, click HOST WORLD.
5. The app creates a host lock, optionally creates a backup, copies the latest shared world to the local Valheim folder, and starts Valheim automatically if enabled.
6. In Valheim, select the same world, enable Start Server, and enable Crossplay if you want a Join Code.
7. While hosting, enter the current Join Code and server password in the manager and click Save server info.
8. Other players can now see the active host and use JOIN WORLD.
9. When the host closes Valheim completely, the app creates an after-hosting backup and publishes the updated local world back to OneDrive.
10. Wait until OneDrive says Up to date.
11. The HOSTING... button changes to RELEASE HOST LOCK.
12. Click RELEASE HOST LOCK.
13. The next player can now host.

HOST / JOIN BEHAVIOUR
---------------------
FREE
- HOST WORLD is available.

YOU ARE HOSTING
- HOST WORLD changes to HOSTING...
- Only the app instance that owns the host-lock token may update Join Code and Password.
- After Valheim closes and the world has been published, the button changes to RELEASE HOST LOCK.

SOMEONE ELSE IS HOSTING
- HOST WORLD is replaced by JOIN WORLD.
- The current host machine/user is shown.
- Saved Join Code and Password are shown read-only.
- Clicking JOIN WORLD starts Valheim.
- The Join Code can also be copied to the clipboard.

Important:
The app checks the host lock again when HOST WORLD is clicked.
A stale UI therefore does not bypass the host-lock check.

FIRST SETUP
-----------
The first time the app starts it opens a setup window.

Before using the manager, you need an existing Valheim world.

If you do not have a world yet:

1. Start Valheim normally.
2. Create a new world.
3. Make sure the world is saved locally, not only in Steam Cloud.
4. Close Valheim.
5. Start Valheim Shared World Manager.
6. Select the local world and use Upload local world to create the initial shared copy.

The manager does not create a new Valheim world itself. It manages, backs up and synchronizes an existing local world.

If your existing world is currently stored only in Steam Cloud, move/save it as a local world in Valheim first so it is available in the local worlds folder.

LOCAL VALHEIM FOLDER
Usually:

C:\Users\YOURNAME\AppData\LocalLow\IronGate\Valheim\worlds_local

SHARED FOLDER
Example:

C:\Users\YOURNAME\OneDrive\Valheim2.0\worlds_local

Use Browse if either folder is stored somewhere else.

Share the parent OneDrive folder with the other player using edit permission.
On both computers it is recommended to choose:

Always keep on this device

FOLDER STRUCTURE
----------------
Recommended structure:

Valheim2.0\
|
+-- worlds_local\
|   +-- MySharedWorld\
|
+-- Backups\
|   +-- MySharedWorld\
|
+-- Sessions\
    +-- MySharedWorld.json

The app no longer uses .valheim-sync for backup history or session information.

WORLDS
------
The app scans both the local and shared folders and lists the Valheim worlds it finds.

You can also type a world name manually.

If a world only exists locally, use:

Upload local world

to create the initial shared copy.

BACKUPS
-------
Backups are enabled by default for new settings.

Backups are stored beside worlds_local:

SHARED_ROOT\Backups\WORLDNAME\

Example:

C:\Users\YOURNAME\OneDrive\ValheimShared\Backups\MySharedWorld\

Typical backups:

2026-09-12_18-15-00_before_hosting
2026-09-12_18-19-00_after_hosting
2026-09-12_19-30-00_before_restore

New backups also contain:

backup.json

The metadata records:
- world
- creation time
- backup type
- host machine
- host user

Older backups without backup.json are shown as Legacy backup.

RECENT BACKUPS / RESTORE
------------------------
Click the LAST PUBLISH status card to open Recent backups.

The manager shows the newest backups and includes:
- date/time
- backup type
- machine/user when metadata is available
- Restore button

Restore replaces only the LOCAL Valheim world.

The shared OneDrive world is NOT automatically replaced by Restore.

Before restoring, the manager creates a safety backup of the current local world when possible.

LAST PUBLISH
------------
LAST PUBLISH is based on the latest after_hosting backup.

For new backups it shows:
- publish date/time
- machine
- user

Older backups are supported as Legacy backup.

SERVER SESSION INFORMATION
--------------------------
Server information is stored in:

SHARED_ROOT\Sessions\WORLDNAME.json

Example:

C:\Users\YOURNAME\OneDrive\ValheimShared\Sessions\MySharedWorld.json

It can contain:
- World
- Join Code
- Password
- Updated time
- Host machine
- Host user

The file may remain after the server stops.

However, a Crossplay Join Code may change when a new server session starts.
The app should therefore treat an older Join Code as the last saved code, not as guaranteed to still be valid.

Only the app instance that currently owns the host-lock token is allowed to save/update the session information.

SAFETY FEATURES
---------------
- Shared host lock.
- One host at a time.
- Fresh lock check when HOST WORLD is clicked.
- Token verification after creating a host lock.
- Token verification before publishing the world.
- Backups before and after hosting.
- Backup before restore.
- Detects if the shared world changed while hosting and refuses to overwrite it.
- Keeps the host lock if a hosting error occurs.
- Force unlock requires confirmation.
- Shows whether the lock is free, yours, or owned by another player.
- Shared Join Code / Password information can only be updated by the active host.
- LAST PUBLISH and Recent backups are derived from real backup history.

ADVANCED
--------
Advanced contains less commonly used actions such as:
- Upload local world
- Download shared world
- Force unlock
- Open configured folders
- Create backups option

The normal hosting workflow should not require Force unlock.

IMPORTANT RULES
---------------
- Only ONE player may host the shared world at a time.
- Never Force unlock unless you are certain nobody is hosting.
- Always wait until OneDrive is fully synced before releasing the host lock.
- Keep the manager open while hosting.
- Do not manually overwrite shared files if the app reports a publishing conflict.
- The player who only joins does not need to copy the shared world locally before joining.

DOWNLOAD / INSTALL
------------------
The easiest way to use the app is to download the latest ready-to-use build from the GitHub Releases page.

1. Open the repository on GitHub.
2. Click Releases.
3. Open the latest release.
4. Download the attached ValheimSharedWorldManager.zip.
5. Extract the ZIP.
6. Run ValheimSharedWorldManager.exe.

The published app is self-contained, so players using the finished EXE do not need to install .NET separately.


BUILD IT YOURSELF
-----------------
If you prefer to build the app yourself:

1. Click Code -> Download ZIP on GitHub.
2. Extract the repository.
3. Make sure the .NET 8 SDK is installed.
4. Run:

BUILD_RELEASE.cmd

The finished standalone application is created in:

publish\ValheimSharedWorldManager.exe

If .NET 8 SDK is not installed, download it from Microsoft's official .NET 8 download page:

https://dotnet.microsoft.com/download/dotnet/8.0

The project targets:

net8.0-windows


GITHUB RELEASES
---------------
Ready-to-use builds should be uploaded to GitHub Releases.

A release can contain:

ValheimSharedWorldManager.zip

The ZIP should contain the published application, for example:

ValheimSharedWorldManager.exe

Users who only want to run the app should download the latest release instead of downloading the source code.


CURRENT UI HIGHLIGHTS
---------------------
- Dark dashboard UI.
- Scrollable main window when content expands.
- First-run folder setup wizard.
- Browse buttons for custom paths.
- Automatic world discovery.
- HOST WORLD / HOSTING... / RELEASE HOST LOCK workflow.
- JOIN WORLD when another player is hosting.
- Current server panel with Join Code and Password.
- Status cards for Local World, Shared Copy, Host Lock and Last Publish.
- Clickable Last Publish card with Recent backups.
- Restore from backup.
- Advanced panel for technical actions.
- Automatic Valheim launch can be enabled/disabled.
- App icon and global error handling.
