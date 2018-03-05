namespace MovieMatch.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ZipCode1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ZipCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ZipCode");
        }
    }
}
