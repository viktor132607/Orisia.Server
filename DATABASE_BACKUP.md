# Database backup and backend verification

Administrators use `/admin/database/` in Orisia.Client.

- `GET /api/database-backup/export`: streams a full PostgreSQL custom-format `.dump`; includes every database schema/table, user record, soft-deleted row, sequence and EF migration history. Nothing is filtered by application models.
- `POST /api/database-backup/restore`: multipart `archive` and exact `confirmation=RESTORE ORISIA`. Maximum upload is 512 MiB. Requires the current database user's Admin role, not just an old JWT role claim.
- Archives contain personal data and password hashes. Keep them private. Never upload an archive from an untrusted party: PostgreSQL restores can execute SQL from the archive.
- Restore uses `--clean --if-exists --no-owner --no-privileges --single-transaction --exit-on-error`. Failures roll back the transaction. Objects introduced after the archive and absent from it are not automatically dropped. Restore a compatible Orisia schema and pause writes first.
- Export the current database before restoring. The operation lock serializes backup/restore in one API process; it is not a multi-instance maintenance lock and does not suspend normal application writes.
- Uploaded photo bytes live outside PostgreSQL and are not part of a database dump. Back up the configured media directory separately.
- Temporary archives are deleted after download, failure or restore. Export is a download, not a recurring off-site retention system.
- PostgreSQL command-line utilities must be installed in the runtime. Their major version must be at least the database server's version; restore must support the archive's version.

## Startup and verification

Migrations run before seeding. Demo accounts and CMS content are only seeded in Development or when `Seed__DemoData=true` is explicitly configured. Do not enable demo accounts in production.

`GET /health/live` checks the process; `/health/ready` checks database connectivity. CI applies the complete migration history in a uniquely named disposable PostgreSQL database, checks model/snapshot consistency, seeds demo data twice, exports and mutates data, restores, and compares sorted contents of **every public table**, including migration history. Production data is never touched by this test.

Run `dotnet test Orisia.Server/Orisia.Server.sln`. To include the real PostgreSQL integration test, set `ORISIA_TEST_POSTGRES` to a localhost test-server connection with permission to create a temporary database; install `pg_dump` and `pg_restore` first.

## Production configuration

- For a new empty database set `BootstrapAdmin__Email` and `BootstrapAdmin__Password` (at least 16 characters). This creates an admin only if no active admin exists; it does not promote existing users or change existing passwords. Remove these values after initialization.
- For real email delivery set `Email__DeliveryMode=Resend`, `Email__ResendApiKey`, and `Email__SenderEmail` to a verified sender. Optionally set `Email__InquiryRecipient` for incoming-inquiry notifications. Keys remain in runtime environment variables, never Docker build arguments.
- Production Console mode is disabled: password reset links are not returned to anonymous callers or written to logs. Without a configured provider, email-dependent operations report unavailability rather than pretend to send.
- Set `MediaStorage__RootPath` to a persistent media directory. The configured directory is explicitly served at `MediaStorage__PublicBasePath`; do not place private backups in it. The default container filesystem is not durable across redeploys.
- Runtime images include PostgreSQL 14–18 utilities and automatically select the exact database server major for both export and restore. This avoids newer-client SQL settings such as `transaction_timeout` breaking restore on an older server. Backups from a newer server major must not be restored into an older server.
