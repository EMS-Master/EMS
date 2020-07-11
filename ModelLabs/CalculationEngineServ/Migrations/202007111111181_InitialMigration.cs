namespace CalculationEngineServ.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Alarms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Gid = c.Long(nullable: false),
                        AlarmValue = c.Single(nullable: false),
                        MinValue = c.Single(nullable: false),
                        MaxValue = c.Single(nullable: false),
                        AlarmTimeStamp = c.DateTime(nullable: false),
                        AckState = c.Int(nullable: false),
                        AlarmType = c.Int(nullable: false),
                        AlarmMessage = c.String(),
                        Severity = c.Int(nullable: false),
                        CurrentState = c.String(),
                        PubStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DiscreteCounters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Gid = c.Long(nullable: false),
                        CurrentValue = c.Boolean(nullable: false),
                        Counter = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DiscreteCounters");
            DropTable("dbo.Alarms");
        }
    }
}
