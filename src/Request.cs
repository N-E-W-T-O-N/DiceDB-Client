namespace DiceDB
{
    public record Response
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ulong Fingerprint64 { get; set; }
    }

    public record ResponseGet : Response
    {
        public string Value { get; set; }
    }

    public record ResponseSet : Response
    {
    }

    public record CommandList 
    {
        /// <summary>
        /// 
        /// </summary>
        public const string SET="SET";
        /// <summary>
        /// 
        /// </summary>
        public const string GET= "GET"; 
        /// <summary>
        /// 
        /// </summary>
        //SET,
        //DEL,
        //EXISTS,
        //INCR,
        //DECR


    }


}
