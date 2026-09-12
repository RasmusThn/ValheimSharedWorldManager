VALHEIM SHARED WORLD MANAGER - CHANGELOG
========================================

1.0.0
----------
- Made the main window scrollable when expanded panels require more space.
- Improved the first-run setup layout and folder status presentation.
- Moved "Start Valheim automatically" next to HOST WORLD.
- Backups are enabled by default for new settings.
- Moved folder-open actions to small folder icons next to configured paths.
- Added clickable LAST PUBLISH card.
- Added Recent backups panel.
- Added restore support for local worlds.
- Restore creates a safety backup before replacing the local world.
- Backup history now reads from the shared Backups\WORLDNAME folder.
- Added backup.json metadata for new backups.
- Backup metadata records world, time, backup type, machine and user.
- Added support for older backups as "Legacy backup".
- LAST PUBLISH now derives its information from the latest after_hosting backup.
- Removed the need for SyncState / SyncStateService for publish history.
- Added HostSessionInfo and shared Sessions\WORLDNAME.json server information.
- Added Join Code and server password sharing.
- Only the app instance that owns the current host-lock token may update server information.
- Added distinction between Free, You are hosting, and Someone else is hosting.
- HOST WORLD is replaced by JOIN WORLD when another player is hosting.
- JOIN WORLD starts Valheim and can use/copy the saved Join Code.
- HOST WORLD changes to HOSTING... while the local player owns the active session.
- After Valheim closes and the world is published, HOSTING... changes to RELEASE HOST LOCK.
- Normal lock release is now part of the main hosting flow instead of being hidden in Advanced.
- Host lock is checked again when HOST WORLD is clicked, even if the UI is stale.
- Kept second lock/token verification after lock creation to reduce simultaneous-host race conditions.
- Updated recommended shared folder structure to worlds_local\, Backups\, and Sessions\.
- Removed .valheim-sync from the new backup/session design.
- Continued support for shared-world fingerprint conflict protection.
- Continued support for Force unlock as an emergency-only action.

0.5.0
-----
- Rebuilt the main UI as a cleaner dark dashboard.
- Added first-run setup wizard with Browse buttons.
- Added local/shared/lock/last-publish status cards.
- Added world auto-discovery from both configured folders.
- Added large HOST WORLD action for normal use.
- Moved technical actions to an Advanced section.
- Added option to disable automatic Valheim launch.
- Added application icon and version metadata.
- Added global UI exception handling.
- Added readable last-publish metadata.
- Kept host lock, backup, fingerprint and conflict-protection features.

0.0.0
-----
- Initial WinForms prototype.
- Shared folder browsing.
- Host lock.
- Backups.
- World mirroring.
- Valheim launching and wait-for-exit flow.
