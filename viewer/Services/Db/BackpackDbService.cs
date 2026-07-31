using System.IO;
using Microsoft.Data.Sqlite;

namespace Backpack.Viewer.Services;

public sealed partial class BackpackDbService : IDisposable
{
    private readonly SqliteConnection _db;

    public BackpackDbService()
    {
        var dir  = Path.Combine(AppContext.BaseDirectory, "data");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "backpack.db");
        _db = new SqliteConnection($"Data Source={path}");
        _db.Open();
        InitSchema();
    }

    private void InitSchema()
    {
        using var vCmd = _db.CreateCommand();
        vCmd.CommandText = "PRAGMA user_version";
        var version = (long)(vCmd.ExecuteScalar() ?? 0L);
        if (version < 3)
        {
            Exec("DROP TABLE IF EXISTS weapons");
            Exec("DROP TABLE IF EXISTS artifacts");
            Exec("DROP TABLE IF EXISTS avatars");
            Exec("DROP TABLE IF EXISTS materials");
            Exec("DROP TABLE IF EXISTS props");
        }

        Exec("""
            CREATE TABLE IF NOT EXISTS weapons (
                guid       TEXT    PRIMARY KEY,
                id         INTEGER NOT NULL,
                name       TEXT    NOT NULL,
                type       TEXT    NOT NULL,
                rank       INTEGER NOT NULL,
                main_stat  TEXT    NOT NULL,
                level      INTEGER NOT NULL,
                ascension  INTEGER NOT NULL,
                refine     INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS artifacts (
                guid           TEXT    PRIMARY KEY,
                id             INTEGER NOT NULL,
                [set]          TEXT    NOT NULL,
                name           TEXT    NOT NULL,
                slot           TEXT    NOT NULL,
                locked         INTEGER NOT NULL,
                level          INTEGER NOT NULL,
                rank           INTEGER NOT NULL,
                init_sub_stats INTEGER NOT NULL DEFAULT 0,
                main_stat      TEXT    NOT NULL,
                sub_stats      TEXT    NOT NULL
            );
            CREATE TABLE IF NOT EXISTS materials (
                id    INTEGER PRIMARY KEY,
                count INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS props (
                id    INTEGER PRIMARY KEY,
                value INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS avatars (
                id            INTEGER PRIMARY KEY,
                level         INTEGER NOT NULL,
                ascension     INTEGER NOT NULL,
                friendship    INTEGER NOT NULL,
                constellation INTEGER NOT NULL,
                skills        TEXT    NOT NULL,
                passives      TEXT    NOT NULL,
                equips        TEXT    NOT NULL,
                fight_props   TEXT    NOT NULL DEFAULT '{}'
            );
            """);

        if (version < 3)
            Exec("PRAGMA user_version = 3");
    }

    private void Exec(string sql)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public void Dispose() => _db.Dispose();
}
