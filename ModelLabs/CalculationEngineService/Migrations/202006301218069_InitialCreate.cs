namespace CalculationEngineService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alarms",
                c => new
                    {
                        Gid = c.Int(nullable: false, identity: true),
                        AlarmValue = c.Single(nullable: false),
                        MinValue = c.Single(nullable: false),
                        MaxValue = c.Single(nullable: false),
                        AckState = c.Int(nullable: false),
                        AlarmType = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Gid);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Alarms");
        }
    }
}
