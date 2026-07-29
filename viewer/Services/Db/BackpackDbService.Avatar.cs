using System.Text.Json;
using Backpack.Viewer.Models;
using Microsoft.Data.Sqlite;

namespace Backpack.Viewer.Services;

public sealed partial class BackpackDbService
{
    public IReadOnlyList<AvatarEntry> LoadAvatars()
    {
        using var cmd = _db.CreateCommand();
        cmd.CommandText =
            "SELECT id,level,ascension,friendship,constellation,skills,passives,equips FROM avatars ORDER BY level DESC,id";
        using var r    = cmd.ExecuteReader();
        var list = new List<AvatarEntry>();
        while (r.Read())
            list.Add(new AvatarEntry(
                (uint)r.GetInt64(0),
                null, null, 0,
                r.GetInt32(1),
                r.GetInt32(2),
                r.GetInt32(3),
                r.GetInt32(4),
                JsonSerializer.Deserialize<SkillEntry[]>  (r.GetString(5)) ?? [],
                JsonSerializer.Deserialize<PassiveEntry[]>(r.GetString(6)) ?? [],
                JsonSerializer.Deserialize<string[]>      (r.GetString(7)) ?? []
            ));
        return list;
    }

    public void SaveAvatars(IEnumerable<AvatarEntry> avatars)
    {
        using var tx  = _db.BeginTransaction();
        using var del = _db.CreateCommand();
        del.CommandText = "DELETE FROM avatars";
        del.ExecuteNonQuery();

        using var ins = _db.CreateCommand();
        ins.CommandText =
            "INSERT OR REPLACE INTO avatars (id,level,ascension,friendship,constellation,skills,passives,equips) " +
            "VALUES ($id,$l,$a,$f,$c,$s,$p,$eq)";
        var pid = ins.Parameters.Add("$id", SqliteType.Integer);
        var pl  = ins.Parameters.Add("$l",  SqliteType.Integer);
        var pa  = ins.Parameters.Add("$a",  SqliteType.Integer);
        var pf  = ins.Parameters.Add("$f",  SqliteType.Integer);
        var pc  = ins.Parameters.Add("$c",  SqliteType.Integer);
        var ps  = ins.Parameters.Add("$s",  SqliteType.Text);
        var pp  = ins.Parameters.Add("$p",  SqliteType.Text);
        var peq = ins.Parameters.Add("$eq", SqliteType.Text);

        foreach (var a in avatars)
        {
            pid.Value = (long)a.Id;
            pl.Value  = a.Level;
            pa.Value  = a.Ascension;
            pf.Value  = a.Friendship;
            pc.Value  = a.Constellation;
            ps.Value  = JsonSerializer.Serialize(a.Skills);
            pp.Value  = JsonSerializer.Serialize(a.Passives);
            peq.Value = JsonSerializer.Serialize(a.Equips);
            ins.ExecuteNonQuery();
        }
        tx.Commit();
    }
}
