using System;

namespace MobileSync.Server
{
    public abstract class TokenValidator
    {
        public abstract bool IsValid(string tokenId);
    }

    public class AllTokensAreOk : TokenValidator
    {
        public override bool IsValid(string tokenId)
        {
            return true;
        }
    }
}
