using SQLite;
namespace PlayOnCloud.Droid
{
    [Table("CloudItemProperties")]
    public class CloudItemProperties
    {
        [PrimaryKey, AutoIncrement]
        public int Key { get; set; }

        public string Id { get; set; }

        public string Email { get; set; }
    }
}