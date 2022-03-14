namespace CartaCore.Persistence
{
    /// <summary>
    /// Struct used to store user secrets under a key.
    /// </summary>
    public struct UserSecretKeyValuePair
    {
        /// <summary>
        /// Key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Value.
        /// </summary>
        public string Value { get; set; }
    }
}