namespace Nhaama.FFXIV.Model.Actor
{
    public struct ActorEntry
    {
        public long Offset { get; set; }
        public string Name { get; set; }
        public string CompanyTag { get; set; }
        public uint ActorID { get; set; }
        public uint OwnerID { get; set; }
        public short ModelChara { get; set; }
        public uint BnpcBase { get; set; }
        public byte Job { get; set; }
        public byte Level { get; set; }
        public byte World { get; set; }
        public ObjectKind ObjectKind { get; set; }
        public ActorAppearance Appearance { get; set; }
    }
}