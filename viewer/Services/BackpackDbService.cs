using System.IO;
using System.Text.Json;
using Backpack.Viewer.Models;
using Microsoft.Data.Sqlite;

namespace Backpack.Viewer.Services;

public sealed class BackpackDbService : IDisposable
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
        Exec("""
            CREATE TABLE IF NOT EXISTS weapons (
                guid        TEXT    PRIMARY KEY,
                id          INTEGER NOT NULL,
                name        TEXT    NOT NULL,
                type        TEXT    NOT NULL,
                rank        INTEGER NOT NULL,
                special_prop TEXT   NOT NULL,
                level       INTEGER NOT NULL,
                promote     INTEGER NOT NULL,
                refine      INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS artifacts (
                guid             TEXT    PRIMARY KEY,
                id               INTEGER NOT NULL,
                set_name         TEXT    NOT NULL,
                name             TEXT    NOT NULL,
                slot             TEXT    NOT NULL,
                locked           INTEGER NOT NULL,
                level            INTEGER NOT NULL,
                rank             INTEGER NOT NULL,
                main_stat_type   TEXT    NOT NULL,
                main_stat_raw    TEXT    NOT NULL,
                sub_stats        TEXT    NOT NULL
            );
            CREATE TABLE IF NOT EXISTS materials (
                id    INTEGER PRIMARY KEY,
                count INTEGER NOT NULL
            );
            """);
        // 旧版 equipped 列迁移
        try { Exec("ALTER TABLE artifacts RENAME COLUMN equipped TO locked"); } catch { }
    }

    public IReadOnlyList<WeaponEntry> LoadWeapons()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText =
            "SELECT guid,id,name,type,rank,special_prop,level,promote,refine FROM weapons ORDER BY rank DESC,id";
        using var r = cmd.ExecuteReader();
        var list = new List<WeaponEntry>();
        while (r.Read())
            list.Add(new WeaponEntry(
                (uint)r.GetInt64(1),
                r.GetString(0),
                r.GetString(2),
                r.GetString(3),
                r.GetInt32(4),
                r.GetString(5),
                r.GetInt32(6),
                r.GetInt32(7),
                r.GetInt32(8)));
        return list;
    }

    public IReadOnlyList<ArtifactEntry> LoadArtifacts()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText =
            "SELECT guid,id,set_name,name,slot,locked,level,rank,main_stat_type,main_stat_raw,sub_stats FROM artifacts ORDER BY locked DESC,rank DESC,level DESC";
        using var r = cmd.ExecuteReader();
        var list = new List<ArtifactEntry>();
        while (r.Read())
        {
            var subStats = JsonSerializer.Deserialize<ArtifactSubStat[]>(r.GetString(10)) ?? [];
            list.Add(new ArtifactEntry(
                (uint)r.GetInt64(1),
                r.GetString(0),
                r.GetString(2),
                r.GetString(3),
                r.GetString(4),
                r.GetInt32(5) != 0,
                r.GetInt32(6),
                r.GetInt32(7),
                new ArtifactMainStat(r.GetString(8), r.GetString(9)),
                subStats));
        }
        return list;
    }

    public Dictionary<uint, ulong> LoadMaterialCounts()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "SELECT id, count FROM materials";
        using var r = cmd.ExecuteReader();
        var dict = new Dictionary<uint, ulong>();
        while (r.Read())
            dict[(uint)r.GetInt64(0)] = (ulong)r.GetInt64(1);
        return dict;
    }

    public void SaveWeapons(IEnumerable<WeaponEntry> weapons)
    {
        using var tx  = _db.BeginTransaction();
        using var del = _db.CreateCommand();
        del.CommandText = "DELETE FROM weapons";
        del.ExecuteNonQuery();

        using var ins = _db.CreateCommand();
        ins.CommandText =
            "INSERT INTO weapons (guid,id,name,type,rank,special_prop,level,promote,refine) " +
            "VALUES ($g,$i,$n,$t,$r,$sp,$l,$p,$rf)";
        var pg  = ins.Parameters.Add("$g",  SqliteType.Text);
        var pi  = ins.Parameters.Add("$i",  SqliteType.Integer);
        var pn  = ins.Parameters.Add("$n",  SqliteType.Text);
        var pt  = ins.Parameters.Add("$t",  SqliteType.Text);
        var pr  = ins.Parameters.Add("$r",  SqliteType.Integer);
        var psp = ins.Parameters.Add("$sp", SqliteType.Text);
        var pl  = ins.Parameters.Add("$l",  SqliteType.Integer);
        var pp  = ins.Parameters.Add("$p",  SqliteType.Integer);
        var prf = ins.Parameters.Add("$rf", SqliteType.Integer);

        foreach (var w in weapons)
        {
            pg.Value  = w.Guid;
            pi.Value  = (long)w.Id;
            pn.Value  = w.Name;
            pt.Value  = w.Type;
            pr.Value  = w.Rank;
            psp.Value = w.SpecialProp;
            pl.Value  = w.Level;
            pp.Value  = w.Promote;
            prf.Value = w.Refine;
            ins.ExecuteNonQuery();
        }
        tx.Commit();
    }

    public void SaveArtifacts(IEnumerable<ArtifactEntry> artifacts)
    {
        using var tx  = _db.BeginTransaction();
        using var del = _db.CreateCommand();
        del.CommandText = "DELETE FROM artifacts";
        del.ExecuteNonQuery();

        using var ins = _db.CreateCommand();
        ins.CommandText =
            "INSERT INTO artifacts " +
            "(guid,id,set_name,name,slot,locked,level,rank,main_stat_type,main_stat_raw,sub_stats) " +
            "VALUES ($g,$i,$sn,$n,$sl,$lk,$l,$r,$mt,$mr,$ss)";
        var pg  = ins.Parameters.Add("$g",  SqliteType.Text);
        var pi  = ins.Parameters.Add("$i",  SqliteType.Integer);
        var psn = ins.Parameters.Add("$sn", SqliteType.Text);
        var pn  = ins.Parameters.Add("$n",  SqliteType.Text);
        var psl = ins.Parameters.Add("$sl", SqliteType.Text);
        var plk = ins.Parameters.Add("$lk", SqliteType.Integer);
        var pl  = ins.Parameters.Add("$l",  SqliteType.Integer);
        var pr  = ins.Parameters.Add("$r",  SqliteType.Integer);
        var pmt = ins.Parameters.Add("$mt", SqliteType.Text);
        var pmr = ins.Parameters.Add("$mr", SqliteType.Text);
        var pss = ins.Parameters.Add("$ss", SqliteType.Text);

        foreach (var a in artifacts)
        {
            pg.Value  = a.Guid;
            pi.Value  = (long)a.Id;
            psn.Value = a.SetName;
            pn.Value  = a.Name;
            psl.Value = a.Slot;
            plk.Value = a.Locked ? 1 : 0;
            pl.Value  = a.Level;
            pr.Value  = a.Rank;
            pmt.Value = a.MainStat.Type;
            pmr.Value = a.MainStat.TypeRaw;
            pss.Value = JsonSerializer.Serialize(a.SubStats);
            ins.ExecuteNonQuery();
        }
        tx.Commit();
    }

    public void SaveMaterials(Dictionary<uint, ulong> counts)
    {
        using var tx  = _db.BeginTransaction();
        using var cmd = _db.CreateCommand();
        cmd.CommandText = "INSERT OR REPLACE INTO materials (id, count) VALUES ($id, $c)";
        var pid = cmd.Parameters.Add("$id", SqliteType.Integer);
        var pc  = cmd.Parameters.Add("$c",  SqliteType.Integer);

        foreach (var (id, count) in counts)
        {
            pid.Value = (long)id;
            pc.Value  = (long)count;
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    private void Exec(string sql)
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    public void Dispose() => _db.Dispose();
}
