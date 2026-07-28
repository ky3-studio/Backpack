namespace Backpack.Viewer.Models;

/// <summary>
/// C++ parsers 输出的属性短名（与 artifact JSON mainStat / subStat type 字段一一对应）。
/// 固定值：Hp、Attack、Defense、ElementMastery
/// 百分比：其余全部
/// </summary>
internal static class PropShortNames
{
    //固定值 
    public const string Hp             = "生命值";
    public const string Attack         = "攻击力";
    public const string Defense        = "防御力";
    public const string ElementMastery = "元素精通";

    //百分比
    public const string HpPercent        = "生命值%";
    public const string AttackPercent    = "攻击力%";
    public const string DefensePercent   = "防御力%";
    public const string ChargeEfficiency = "充能效率";
    public const string CritRate         = "暴击率";
    public const string CritDmg          = "暴击伤害";
    public const string HealBonus        = "治疗加成";

    // ── 元素 / 物理伤害加成（均为百分比）
    public const string FireDmg     = "火伤加成";
    public const string ElecDmg     = "雷伤加成";
    public const string IceDmg      = "冰伤加成";
    public const string WaterDmg    = "水伤加成";
    public const string WindDmg     = "风伤加成";
    public const string RockDmg     = "岩伤加成";
    public const string GrassDmg    = "草伤加成";
    public const string PhysicalDmg = "物伤加成";
}
