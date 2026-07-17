namespace D2P.Core.Repository {
    public sealed class RepositoryOptions {
        public bool DeleteExisting { get; set; } = true;
        public bool OnlyDirty { get; set; } = true;
        public bool Validate { get; set; }
    }
}
