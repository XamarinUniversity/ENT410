namespace CustomerSync.Models
{
    [System.AttributeUsage(System.AttributeTargets.All)]
	public sealed class PreserveAttribute : System.Attribute
	{
        public bool AllMembers;
        public bool Conditional;
    }

}
