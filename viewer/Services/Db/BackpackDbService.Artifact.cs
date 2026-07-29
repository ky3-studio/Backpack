using System.Text.Json;
using Backpack.Viewer.Models;
using Microsoft.Data.Sqlite;
using static Backpack.Viewer.Models.BagJsonContext;

namespace Backpack.Viewer.Services;

public sealed partial class BackpackDbService
{
    public IReadOnlyList<ArtifactEntry> LoadArtifacts()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText =
            "SELECT guid,id,[set],name,slot,locked,level,rank,init_sub_stats,main_stat,sub_stats FROM artifacts ORDER BY locked DESC,rank DESC,level DESC";
        using var r = cmd.ExecuteReader();
        var list = new List<ArtifactEntry>();
        while (r.Read())
        {
            var subStats = JsonSerializer.Deserialize(r.GetString(10), Default.ArtifactSubStatArray) ?? [];
            list.Add(new ArtifactEntry(
                (uint)r.GetInt64(1),
                r.GetString(0),
                r.GetString(2),
                r.GetString(3),
                r.GetString(4),
                r.GetInt32(5) != 0,
                r.GetInt32(6),
                r.GetInt32(7),
                r.GetInt32(8),
                r.GetString(9),
                subStats));
        }
        return list;
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
            "(guid,id,[set],name,slot,locked,level,rank,init_sub_stats,main_stat,sub_stats) " +
            "VALUES ($g,$i,$s,$n,$sl,$lk,$l,$r,$is,$ms,$ss)";
        var pg  = ins.Parameters.Add("$g",  SqliteType.Text);
        var pi  = ins.Parameters.Add("$i",  SqliteType.Integer);
        var ps  = ins.Parameters.Add("$s",  SqliteType.Text);
        var pn  = ins.Parameters.Add("$n",  SqliteType.Text);
        var psl = ins.Parameters.Add("$sl", SqliteType.Text);
        var plk = ins.Parameters.Add("$lk", SqliteType.Integer);
        var pl  = ins.Parameters.Add("$l",  SqliteType.Integer);
        var pr  = ins.Parameters.Add("$r",  SqliteType.Integer);
        var pis = ins.Parameters.Add("$is", SqliteType.Integer);
        var pms = ins.Parameters.Add("$ms", SqliteType.Text);
        var pss = ins.Parameters.Add("$ss", SqliteType.Text);

        foreach (var a in artifacts)
        {
            pg.Value  = a.Guid;
            pi.Value  = (long)a.Id;
            ps.Value  = a.Set;
            pn.Value  = a.Name;
            psl.Value = a.Slot;
            plk.Value = a.Locked ? 1 : 0;
            pl.Value  = a.Level;
            pr.Value  = a.Rank;
            pis.Value = a.InitSubStats;
            pms.Value = a.MainStat;
            pss.Value = JsonSerializer.Serialize(a.SubStats, Default.ArtifactSubStatArray);
            ins.ExecuteNonQuery();
        }
        tx.Commit();
    }
}
