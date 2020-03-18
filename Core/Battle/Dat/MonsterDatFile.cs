namespace OpenVIII.Battle.Dat
{
    public sealed class MonsterDatFile : DatFile
    {
        public static MonsterDatFile CreateInstance(int fileId, Sections flags = Sections.All)
        {
            return new MonsterDatFile(fileId, flags: flags);
        }

        private MonsterDatFile(int fileId, int additionalFileId = -1, DatFile skeletonReference = null,
            Sections flags = Sections.All) : base(fileId, EntityType.Monster, additionalFileId, skeletonReference,
            flags)
        {
        }
    }
}