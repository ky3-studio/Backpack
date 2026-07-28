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
            "SELECT id,guid,level,promote,fetter,talents,skills,extras,equips FROM avatars ORDER BY level DESC,id";
        using var r    = cmd.ExecuteReader();
        var list = new List<AvatarEntry>();
        while (r.Read())
            list.Add(new AvatarEntry(
                (uint)r.GetInt64(0),
                r.GetString(1),
                r.GetInt32(2),
                r.GetInt32(3),
                r.GetInt32(4),
                JsonSerializer.Deserialize<uint[]>  (r.GetString(5)) ?? [],
                JsonSerializer.Deserialize<int[][]> (r.GetString(6)) ?? [],
                JsonSerializer.Deserialize<int[][]> (r.GetString(7)) ?? [],
                JsonSerializer.Deserialize<string[]>(r.GetString(8)) ?? []
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
            "INSERT OR REPLACE INTO avatars (id,guid,level,promote,fetter,talents,skills,extras,equips) " +
            "VALUES ($id,$g,$l,$p,$f,$t,$s,$e,$eq)";
        var pid = ins.Parameters.Add("$id", SqliteType.Integer);
        var pg  = ins.Parameters.Add("$g",  SqliteType.Text);
        var pl  = ins.Parameters.Add("$l",  SqliteType.Integer);
        var pp  = ins.Parameters.Add("$p",  SqliteType.Integer);
        var pf  = ins.Parameters.Add("$f",  SqliteType.Integer);
        var pt  = ins.Parameters.Add("$t",  SqliteType.Text);
        var ps  = ins.Parameters.Add("$s",  SqliteType.Text);
        var pe  = ins.Parameters.Add("$e",  SqliteType.Text);
        var peq = ins.Parameters.Add("$eq", SqliteType.Text);

        foreach (var a in avatars)
        {
            pid.Value = (long)a.Id;
            pg.Value  = a.Guid;
            pl.Value  = a.Level;
            pp.Value  = a.Promote;
            pf.Value  = a.Fetter;
            pt.Value  = JsonSerializer.Serialize(a.Talents);
            ps.Value  = JsonSerializer.Serialize(a.Skills);
            pe.Value  = JsonSerializer.Serialize(a.Extras);
            peq.Value = JsonSerializer.Serialize(a.Equips);
            ins.ExecuteNonQuery();
        }
        tx.Commit();
    }
}
